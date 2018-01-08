using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace HealthData2.Service
{
    public class SASService
    {
        private SasServer _sasServer;
        public SASService()
        {
            _sasServer = new SasServer();
        }
        internal string ConnectToSAS()
        {
            string statusMsg = "";

            try
            {
                _sasServer.Connect();
            }
            catch (Exception ex)
            {
                statusMsg = string.Format("Connection failure {0}", ex.Message);
            }

            return statusMsg;
        }

        internal string RunSASProgram(string sasCode)
        {
            StringBuilder logBuilder = new StringBuilder();
            if (_sasServer != null && _sasServer.Workspace != null)
            {
                logBuilder.AppendLine("Running SAS ...");

                _sasServer.Workspace.LanguageService.Submit(sasCode);

                bool hasErrors = false, hasWarnings = false;

                Array carriage, lineTypes, lines;
                do
                {
                    SAS.LanguageServiceCarriageControl CarriageControl = new SAS.LanguageServiceCarriageControl();
                    SAS.LanguageServiceLineType LineType = new SAS.LanguageServiceLineType();

                    _sasServer.Workspace.LanguageService.FlushLogLines(1000,
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

        internal void SaveFile(string fileName, string sasCode, string folder)
        {
            //string codeFileName = @"sascode.txt";
            string codeFilePath = string.Format("{0}\\{1}", folder, fileName);
            using (StreamWriter sw = File.CreateText(codeFilePath))
            {
                sw.Write(sasCode);
            }
        }

        internal string ConvertXPT(string localPath, string fileFullName)
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("libname sasfile '{0}';", localPath);
            sb.AppendFormat("libname xptfile xport '{0}' access=readonly;", fileFullName);
            sb.AppendLine("proc copy inlib=xptfile outlib=sasfile; run;");

            return sb.ToString();
        }
        
    }
}