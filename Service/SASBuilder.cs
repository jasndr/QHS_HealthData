using HealthData2.Models;
using HealthData2.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace HealthData2.Service
{
    public static class SASBuilder
    {
        public static SasServer activeSession = null;
        internal static string BuildNHANESCode(Dictionary<string, Dictionary<string, List<NHANESFile>>> _tables, string libFolder, string macroSource, string fileSource)
        {            
            StringBuilder sb = new StringBuilder();
            string mergeClause = string.Empty;
            //demo only
            //sb.AppendFormat("libname SASTemp '{0}';", libFolder);
            //sb.AppendLine();
            ////sb.AppendLine(@"libname testSAS 'C:\Users\yrui\Desktop\SAS\TestSAS';");
            //sb.AppendLine(@"libname lab1 'K:\shared\Databases\NHANES\SAS data 1999-2000\Laboratory data\Biochemistry Profile and Hormones';");
            //sb.AppendLine(@"libname lab2 'K:\shared\Databases\NHANES\SAS data 1999-2000\Laboratory data\Complete Blood Count';");
            //sb.AppendLine("data SASTemp.merged; merge lab1.Lab18 lab2.Lab25; by SEQN; run;");

            //prepare SAS macro
            //sb.AppendLine(@"%macro combineall (year, n);");
            //sb.AppendLine(@"data &year;");
            //sb.AppendLine(@"merge");
            //sb.AppendLine(@"%do i=1 %to &n;");
            //sb.AppendLine(@"&year&i");
            //sb.AppendLine(@"%end;");
            //sb.AppendLine(@";");
            //sb.AppendLine(@"by SEQN;");
            //sb.AppendLine(@"%mend combineall;");

            //parse _tables
            //string fileSource = @"K:\shared\Databases\NHANES\WebScrapper\";                     
            
            sb.AppendFormat("%include '{0}';", macroSource);
            sb.AppendLine();

            string rootLibName = string.Empty;
            bool hasKey = true;     // has key column SEQN

            foreach (var pair in _tables)
            {
                string year = pair.Key;
                Dictionary<string, List<NHANESFile>> files = pair.Value;

                rootLibName = 'Y' + year.Split('-')[0];
                sb.AppendFormat("libname {0} '{1}';", rootLibName, libFolder);
                sb.AppendLine();                
                
                sb.Append(Environment.NewLine);

                //foreach (List<string> folderName in files)
                //{
                //    string fileFullPath = fileSource + year.Replace('_', '-') + "\\" + folderName;

                //    if (Directory.GetFiles(fileFullPath, "*.sas7bdat").Length > 0)
                //    {
                //        string fileName = Path.GetFileName(Directory.GetFiles(fileFullPath, "*.sas7bdat")[0]);                         
                //    }
                //}

                int index = 1;
                foreach (var group in files)
                {
                    string groupName = group.Key;
                    List<NHANESFile> groupFiles = group.Value;

                    foreach(NHANESFile file in groupFiles)
                    {
                        string fileFullPath = fileSource + year.Replace('-','_') + "\\" + file.GroupName + "\\" + file.FolderName;
                        if (Directory.GetFiles(fileFullPath, "*.sas7bdat").Length > 0)
                        {
                            string fileName = Path.GetFileName(Directory.GetFiles(fileFullPath, "*.sas7bdat")[0]);

                            string subLibName = rootLibName + index;
                            sb.AppendFormat("libname {0} '{1}';", subLibName, fileFullPath);
                            sb.AppendLine();

                            sb.AppendFormat("data {0}.{1}; set {2}.{3} (keep={4}); run;", rootLibName, subLibName, subLibName, fileName.Split('.')[0], file.ColumnName.Replace(","," "));
                            sb.AppendLine();

                            string[] columns = file.ColumnName.Split(',');
                            if (Array.FindAll(columns, s=>s.Equals("SEQN")).Length == 0)
                            {
                                hasKey = false;
                            }

                            index++;
                        }

                    }

                }

                sb.Append(Environment.NewLine);

                if (index > 1)
                {
                    if (hasKey)
                    {
                        sb.AppendFormat("%combineyear({0}.{1}, {2}, {3});", rootLibName, rootLibName, --index, "SEQN");
                    }
                    else
                        sb.AppendFormat("%combineyear({0}.{1}, {2}, {3});", rootLibName, rootLibName, --index, string.Empty);
                   
                    sb.AppendLine("run;");
                }
                

                mergeClause += string.Format("{0}.{0}merged ", rootLibName);
            }

            //final merge
            //data Y1999.merged; merge Y1999.Y1999merged Y2003.Y2003merged; by SEQN; run;
            sb.Append(Environment.NewLine);

            sb.AppendFormat("data {0}.merged; merge {1}; ", rootLibName, mergeClause);

            if (hasKey)
            {
                sb.AppendFormat("by {0}; ", "SEQN");
            }

            sb.Append("run;");

            sb.AppendLine();

            return sb.ToString();
        }

        internal static void RunSAS(string sasCode)
        {
            string connectionStatus = ConnectToSAS();

            string log = RunSASProgram(sasCode);
        }

        private static string RunSASProgram(string sasCode)
        {
            StringBuilder logBuilder = new StringBuilder();
            if (activeSession != null && activeSession.Workspace != null)
            {
                logBuilder.AppendLine("Running SAS ...");

                activeSession.Workspace.LanguageService.Submit(sasCode);

                bool hasErrors = false, hasWarnings = false;

                Array carriage, lineTypes, lines;
                do
                {
                    SAS.LanguageServiceCarriageControl CarriageControl = new SAS.LanguageServiceCarriageControl();
                    SAS.LanguageServiceLineType LineType = new SAS.LanguageServiceLineType();

                    activeSession.Workspace.LanguageService.FlushLogLines(1000,
                        out carriage,
                        out lineTypes,
                        out lines);

                    for (int i = 0; i < lines.GetLength(0); i++)
                    {
                        SAS.LanguageServiceLineType pre =
                            (SAS.LanguageServiceLineType)lineTypes.GetValue(i);
                        switch (pre)
                        {
                            case SAS.LanguageServiceLineType.LanguageServiceLineTypeError:
                                hasErrors = true;
                                break;
                            case SAS.LanguageServiceLineType.LanguageServiceLineTypeNote:
                                break;
                            case SAS.LanguageServiceLineType.LanguageServiceLineTypeWarning:
                                hasWarnings = true;
                                break;
                            case SAS.LanguageServiceLineType.LanguageServiceLineTypeTitle:
                            case SAS.LanguageServiceLineType.LanguageServiceLineTypeFootnote:
                                break;
                            default:
                                break;
                        }

                        logBuilder.AppendLine(string.Format("{0}{1}", lines.GetValue(i) as string, Environment.NewLine));
                    }
                }
                while (lines != null && lines.Length > 0);

                if (hasWarnings && hasErrors)
                    logBuilder.AppendLine("Program complete - has ERRORS and WARNINGS");
                else if (hasErrors)
                    logBuilder.AppendLine("Program complete - has ERRORS");
                else if (hasWarnings)
                    logBuilder.AppendLine("Program complete - has WARNINGS");
                else
                    logBuilder.AppendLine("Program complete - no warnings or errors!");
            }

            return logBuilder.ToString();
        }

        private static string ConnectToSAS()
        {
            string statusMsg = "";
            activeSession = new SasServer();

            try
            {
                activeSession.Connect();

                if (activeSession.UseLocal)
                {
                    statusMsg = "Connected to local SAS session";
                }
            }
            catch (Exception ex)
            {
                statusMsg = string.Format("Connection failure {0}", ex.Message);
            }

            return statusMsg;
        }

        internal static string ConvertXPT(string localPath, string fileFullName)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("libname sasfile '{0}';", localPath);
            sb.AppendFormat("libname xptfile xport '{0}' access=readonly;", fileFullName);
            sb.AppendLine("proc copy inlib=xptfile outlib=sasfile; run;");

            return sb.ToString();
        }
    }

    //enum NHANESYear
    //{
    //    [StringValue("1999-2000")]
    //    Year1 = 1,
    //    [StringValue("2001-2002")]
    //    Year2 = 2
    //}
}