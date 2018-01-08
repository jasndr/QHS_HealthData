using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using HealthData2.Models;
using System.Text;
using System.Reflection;
using System.IO;
using System.Collections.Specialized;
using System.Configuration;
using System.Xml.XPath;
using HealthData2.Service;
//using System.IO;

namespace HealthData2
{
    public partial class NHANES : System.Web.UI.Page
    {
        public SasServer activeSession = null;

        //first dictionary for group, second for year, tree might be a better structure
        Dictionary<string, List<string>> _tables;
        Dictionary<string, Dictionary<string, List<string>>> _yeartables = new Dictionary<string, Dictionary<string, List<string>>>();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                BindGrid();
            }
        }   

        private void BindGrid()
        {
            string filePath = ConfigurationManager.AppSettings["NHANES"];
            string path = Server.MapPath(filePath);
            DataTable dt = new DataTable("Study");

            try
            {
                //Add Columns in datatable - Column names must match XML File nodes 
                dt.Columns.Add("Id", typeof(System.String));
                dt.Columns.Add("GroupName", typeof(System.String));
                dt.Columns.Add("GroupShortName", typeof(System.String));
                dt.Columns.Add("Name", typeof(System.String));
                dt.Columns.Add("FileExists", typeof(System.String));

                // Reading the XML file and display data in the gridview         
                dt.ReadXml(path);
            }
            catch (Exception e)
            {
                throw e;
            }

            GridViewStudy.DataSource = dt;
            GridViewStudy.DataBind();  

        }

        protected void GridViewStudy_PreRender(object sender, EventArgs e)
        {
            MergeGridviewRows(GridViewStudy);

            // disable check box if file doesn't exist
            DisableCheckBox(GridViewStudy);
        }       
        
        private void DisableCheckBox(GridView GridViewStudy)
        {
            XPathDocument docNav = new XPathDocument(@"C:\VS2013\HealthData2\App_Data\NHANES_column.xml");
            String strExpression;

            foreach (GridViewRow row in GridViewStudy.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    Label lblFileExists = row.FindControl("lblFileExists") as Label;
                    char[] arr = lblFileExists.Text.ToCharArray();
                    Array.Reverse(arr);
                    string reverse = new string(arr);
                    int bitsum = Convert.ToInt32(reverse, 2);

                    int yearFrom = 1999;
                    int value = 2;

                    for (int i = 0; i < 7; i++)
                    {
                        string chkBoxId = "chkRow" + yearFrom.ToString();
                        CheckBox chkBox = row.FindControl(chkBoxId) as CheckBox;

                        if (chkBox != null)
                        {
                            int j = (int)Math.Pow(value, i);
                            int bs = bitsum & j;
                            chkBox.Enabled = bs == j ? true: false;

                            if (chkBox.Enabled)
                            {
                                string folderName;
                                Label lblName = row.FindControl("lblName") as Label;
                                Label lblGroupName = row.FindControl("lblGroupName") as Label;

                                XPathNodeIterator NodeIter;
                                XPathItem nodeItem;
                                XPathNavigator nav;

                                if (lblName != null && lblGroupName != null)
                                {
                                    nav = docNav.CreateNavigator();
                                    strExpression = String.Format("/NHANES_Column/File[YearFrom='{0}' and GroupName='{1}' and FolderName='{2}']/ColumnName", yearFrom, lblGroupName.Text, lblName.Text);

                                    nodeItem = nav.SelectSingleNode(strExpression);

                                    if (nodeItem != null)
                                    {
                                        chkBox.Attributes.Add("ColumnName", nodeItem.Value);
                                    }                                                                   
                                }

                                //string columnName = GetColumnName(yearFrom, row.Cells[0].Text);
                                //chkBox.Attributes.Add("ColumnName", columnName);
                            }
                        }

                        yearFrom += 2;
                    }
                }
            }
        }

        private string GetColumnName(int yearFrom, string folderName)
        {
            //find column name from xml file
            StringBuilder sb = new StringBuilder();

            return sb.ToString();
        }

        private void MergeGridviewRows(GridView gridView)
        {
            for (int rowIndex = gridView.Rows.Count - 2; rowIndex >= 0; rowIndex--)
            {
                GridViewRow row = gridView.Rows[rowIndex];
                GridViewRow previousRow = gridView.Rows[rowIndex + 1];

                string s1 = ((Label)row.Cells[1].FindControl("lblGroupName")).Text;
                string s2 = ((Label)previousRow.Cells[1].FindControl("lblGroupName")).Text;
                if (s1 == s2)
                {
                    row.Cells[1].RowSpan = previousRow.Cells[1].RowSpan < 2 ? 2 : previousRow.Cells[1].RowSpan + 1;

                    previousRow.Cells[1].Visible = false;
                }
            } 
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string sessionId = this.Session.SessionID;

            string folder = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMdd"));    //Guid.NewGuid().ToString());
            if (!Directory.Exists(folder) && !File.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }            

            string studyYear = "";
            List<string>[] studyArrayList = new List<string>[7];
            for (int i = 0; i < 7; i++)
            {
                studyArrayList[i] = new List<string>();
            }

            CheckBox[] checkBoxArray = new CheckBox[7];
            foreach (GridViewRow row in GridViewStudy.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {  
                    int yearFrom = 1999;
                    for (int i = 0; i < 7; i++)
                    {
                        string chkBoxId = "chkRow" + yearFrom.ToString();
                        checkBoxArray[i] = row.FindControl(chkBoxId) as CheckBox;
                        yearFrom += 2;
                    }

                    for (int i = 0; i < 7; i++)
                    {
                        if (checkBoxArray[i] != null && checkBoxArray[i].Checked)
                        {
                            Label lblName = row.FindControl("lblName") as Label;
                            Label lblGroupName = row.FindControl("lblGroupName") as Label;

                            if (lblName != null && lblGroupName != null)
                            {
                                if (lblGroupName.Text.Equals("Demographic"))
                                {
                                    studyArrayList[i].Add(lblName.Text);
                                }
                                else
                                {
                                    studyArrayList[i].Add(lblGroupName.Text + " data\\" + lblName.Text);
                                }
                            }

                        }
                    }

                }
            }

            int yearHeader = 4;
            for (int i = 0; i < 7; i++)
            {
                studyYear = GridViewStudy.HeaderRow.Cells[yearHeader].Text;
                //studyYear = studyYear.Replace('-', '_');

                if (studyArrayList[i].Count > 0)
                {
                    //_tables.Add(studyYear, studyArrayList[i]);
                                        
                    //find the same group in arraylist
                    _tables = new Dictionary<string, List<string>>();
                    List<string> listGroup = new List<string>();
                    foreach(string fileName in studyArrayList[i])
                    {
                        if (fileName.Equals("Demographic"))
                        {
                            listGroup.Add(fileName);
                        }
                        else
                        {
                            string[] names = fileName.Split('\\'); 
                            if (!listGroup.Contains(names[0]))
                            {
                                listGroup.Add(names[0]);
                            }
                        }
                    }

                    foreach(string groupName in listGroup)
                    {
                        List<string> sameGroupList = new List<string>();
                        foreach(string fileName in studyArrayList[i])
                        {
                            string[] names = fileName.Split('\\'); 
                            if (groupName.Equals(names[0]))
                            {
                                sameGroupList.Add(fileName);
                            }
                        }

                        _tables.Add(groupName, sameGroupList);
                    }

                    _yeartables.Add(studyYear, _tables);
                }

                yearHeader += 1;
            }
           
            ////call SAS
            //string SASCode = SASBuilder.BuildNHANESCode(_yeartables, folder);

            //SASBuilder.RunSAS(SASCode);

            ////download SAS code
            //string codeFileName = @"sascode.txt";
            //string codeFilePath = string.Format("{0}\\{1}", folder, codeFileName); 
            //using (StreamWriter sw = File.CreateText(codeFilePath))
            //{
            //    sw.Write(SASCode);
            //}

            ////open file dialog
            //String FileName = @"merged.sas7bdat";
            //String FilePath = string.Format("{0}\\{1}", folder, FileName);  //@"D:\NHANES_EXTRA\1999-2000\lab\Biochemistry Profile and Hormones\lab18.sas7bdat"; //Replace this

            //if (DownloadableProduct_Tracking(FilePath, FileName))
            //{
            //    //Request.Headers.Add(Request.Headers);
            //    //Response.Redirect(Request.RawUrl);

            //    //Response.AppendHeader("Refresh", "0;URL=/NHANES.aspx");
            //}
            //else
            //{
            //    Response.Write("<script>alert('failed');</script>");
            //};
        }

        private void RunSAS(string sasCode)
        {
            string connectionStatus = ConnectToSAS();

            string log = RunSASProgram(sasCode);

        }

        private string RunSASProgram(string sasCode)
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

        private string ConnectToSAS()
        {
            string statusMsg="";
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

        private bool DownloadableProduct_Tracking(string _filePath, string _fileName)
        {
            //File Path and File Name
            string filePath = _filePath;                                         //Server.MapPath("~/ApplicationData/DownloadableProducts");
            string _DownloadableProductFileName = _fileName;          //"DownloadableProduct_FileName.pdf";

            System.IO.FileInfo FileName = new System.IO.FileInfo(filePath);
            FileStream myFile = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);

            //Reads file as binary values
            BinaryReader _BinaryReader = new BinaryReader(myFile);

            //Ckeck whether user is eligible to download the file
            if (true)
            {
                //Check whether file exists in specified location
                if (FileName.Exists)
                {
                    System.Web.UI.ScriptManager.RegisterStartupScript(this, GetType(),
                    "FileFoundWarning", "alert('File is available now!')", true);

                    try
                    {
                        long startBytes = 0;
                        string lastUpdateTiemStamp = File.GetLastWriteTimeUtc(filePath).ToString("r");
                        string _EncodedData = HttpUtility.UrlEncode(_DownloadableProductFileName, Encoding.UTF8) + lastUpdateTiemStamp;

                        Response.Clear();
                        Response.AppendCookie(new HttpCookie("fileDownloadToken", download_token_value_id.Value)); //downloadTokenValue will have been provided in the form submit via the hidden input field
                        Response.Buffer = false;
                        Response.AddHeader("Accept-Ranges", "bytes");
                        Response.AppendHeader("ETag", "\"" + _EncodedData + "\"");
                        Response.AppendHeader("Last-Modified", lastUpdateTiemStamp);
                        Response.ContentType = "application/octet-stream";
                        Response.AddHeader("Content-Disposition", "attachment;filename=" + FileName.Name);
                        Response.AddHeader("Content-Length", (FileName.Length - startBytes).ToString());
                        Response.AddHeader("Connection", "Keep-Alive");
                        Response.ContentEncoding = Encoding.UTF8;

                        //Send data
                        _BinaryReader.BaseStream.Seek(startBytes, SeekOrigin.Begin);

                        //Dividing the data in 1024 bytes package
                        int maxCount = (int)Math.Ceiling((FileName.Length - startBytes + 0.0) / 1024);

                        Response.Write("<script type='text/javascript'>");
                        Response.Write("window.location = '" + Request.RawUrl + "'</script>");

                        //Download in block of 1024 bytes
                        int i;
                        for (i = 0; i < maxCount && Response.IsClientConnected; i++)
                        {
                            Response.BinaryWrite(_BinaryReader.ReadBytes(1024));
                            Response.Flush();
                        }
                        //if blocks transfered not equals total number of blocks
                        if (i < maxCount)
                            return false;
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                    finally
                    {                        
                        //Response.End();
                        _BinaryReader.Close();
                        myFile.Close();

                        if (Response.IsClientConnected)
                        {
                            Response.Write("<script>alert('test');</script>");
                        }
                    }
                }
                else System.Web.UI.ScriptManager.RegisterStartupScript(this, GetType(),
                    "FileNotFoundWarning", "alert('File is not available now!')", true);
            }
            else
            {
                System.Web.UI.ScriptManager.RegisterStartupScript(this, GetType(),
                    "NotEligibleWarning", "alert('Sorry! File is not available for you')", true);
            }
            return false;
        }

    }

    public class NHANESTable
    {
        public NHANESTable(int studyId, string studyGroupShortName)
        {
            StudyId = studyId;
            StudyGroupShortName = studyGroupShortName;
        }

        public int StudyId { get; set; }
        public string StudyYear { get; set; }
        public string StudyGroupShortName { get; set; }
        public string StudyName { get; set; }
    }
}