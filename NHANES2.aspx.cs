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
    public partial class NHANES2 : System.Web.UI.Page
    {
        public SasServer activeSession = null;

        //first dictionary for group, second for year, tree might be a better structure
        Dictionary<string, List<NHANESFile>> _tables;
        Dictionary<string, Dictionary<string, List<NHANESFile>>> _yeartables = new Dictionary<string, Dictionary<string, List<NHANESFile>>>();
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

            //GridViewStudy.DataSource = dt;
            GridViewStudy.DataSource = InsertGroupHeaderRow(dt);
            GridViewStudy.DataBind();  

        }

        private DataTable InsertGroupHeaderRow(DataTable dt)
        {
            //DataTable dt = dsProducts.Tables[0];
            //clone the dataschema into a new table
            //this is our final sorted list
            DataTable dtNew = dt.Clone();

            //group rows by productsubcategory
            var results = from myRow in dt.AsEnumerable()
                          group myRow by myRow["GroupName"]
                              into grp
                              select new
                              {
                                  Id = grp.Key,
                                  Rows = grp.Select(x => x)
                              };
            DataRow newRow;
            //iterate through resultset and insert the group header row
            //at the start of each subcategory
            foreach (var row in results)
            {
                //create a new row with only subcategoryid and name populated
                //do not populate any other attributes as that would be our
                //criteria to figure out the group start
                newRow = dtNew.NewRow();
                //newRow["ProductSubCategoryID"] = row.ProductSubCategoryID;
                newRow["GroupName"] = row.Rows.FirstOrDefault()["GroupName"];
                List<DataRow> dataRows = row.Rows.ToList();
                dataRows.Insert(0, newRow);
                //copy results to a new resultset
                foreach (DataRow dr in dataRows)
                {
                    dtNew.Rows.Add(dr.ItemArray);
                }
            };
            return dtNew;
        }

        protected void GridViewStudy_DataBound(object sender, GridViewRowEventArgs e)
        {
            //check if it is a datarow or header row
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //check if the rows has product id populated
                //string productID = e.Row.Cells[1].Text.Replace("&nbsp;", "");
                Label lblName = e.Row.FindControl("lblName") as Label;
                //if (productID.Trim().Length > 0)
                if (lblName != null && lblName.Text.Trim().Length > 0)
                {
                    //if it is not a header row, hide collapse & expand images along
                    //with subcategory text
                    HideUnHideToggleButtons(e.Row.Cells[0], true, true);
                    Label lblSubcategory =
                             (Label)e.Row.Cells[0].FindControl("lblGroupName");
                    lblSubcategory.Visible = false;
                    //set the row style to content row style
                    e.Row.CssClass = "RowStyle";
                }
                else
                {
                    //if it is a header row, set the colspan to the number of columns
                    e.Row.Cells[0].ColumnSpan = GridViewStudy.Columns.Count - 1;
                    //set the row style to group header style
                    e.Row.CssClass = "GroupHeaderRowStyle";
                    //hide extra table cells
                    //e.Row.Cells[1].Visible = false;
                    //e.Row.Cells[2].Visible = false;
                    for (int i = 1; i < 13; i++)
                    {
                        e.Row.Cells[i].Visible = false;
                    }
                }
            }
        }

        protected void GridViewStudy_PreRender(object sender, EventArgs e)
        {
            //MergeGridviewRows(GridViewStudy);

            //// disable check box if file doesn't exist
            DisableCheckBox(GridViewStudy);
        }       
        
        private void DisableCheckBox(GridView GridViewStudy)
        {
            string filePath = ConfigurationManager.AppSettings["NHANESFile"];
            string path = Server.MapPath(filePath);
            XPathDocument docNav = new XPathDocument(path); //(@"C:\VS2013\HealthData2\App_Data\NHANES_column.xml");
            String strExpression;

            foreach (GridViewRow row in GridViewStudy.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    Label lblFileExists = row.FindControl("lblFileExists") as Label;
                    if (lblFileExists.Text.Trim().Length > 0)
                    {
                        char[] arr = lblFileExists.Text.ToCharArray();
                        Array.Reverse(arr);
                        string reverse = new string(arr);
                        int bitsum = Convert.ToInt32(reverse, 2);

                        int yearFrom = 1999;
                        int value = 2;

                        for (int i = 0; i < 9; i++)
                        {
                            string chkBoxId = "chkRow" + yearFrom.ToString();
                            CheckBox chkBox = row.FindControl(chkBoxId) as CheckBox;

                            if (chkBox != null)
                            {
                                int j = (int)Math.Pow(value, i);
                                int bs = bitsum & j;
                                chkBox.Enabled = bs == j ? true : false;

                                if (chkBox.Enabled)
                                {
                                    string folderName;
                                    Label lblName = row.FindControl("lblName") as Label;
                                    Label lblGroupName = row.FindControl("lblGroupName") as Label;
                                    Label lblId = row.FindControl("lblId") as Label;

                                    XPathNodeIterator NodeIter;
                                    XPathItem nodeItem;
                                    XPathNavigator nav;

                                    if (lblName != null && lblGroupName != null && lblId != null)
                                    {
                                        nav = docNav.CreateNavigator();
                                        strExpression = String.Format("/NHANES_Column/File[YearFrom='{0}' and GroupName='{1}' and FolderName='{2}']/ColumnName", yearFrom, lblGroupName.Text, lblName.Text);

                                        nodeItem = nav.SelectSingleNode(strExpression);

                                        if (nodeItem != null)
                                        {
                                            chkBox.Attributes.Add("Id", lblId.Text);
                                            chkBox.Attributes.Add("YearFrom", yearFrom.ToString());
                                            chkBox.Attributes.Add("GroupName", lblGroupName.Text);
                                            chkBox.Attributes.Add("FolderName", lblName.Text);                                            
                                            chkBox.Attributes.Add("ColumnName", nodeItem.Value);
                                        }

                                        strExpression = String.Format("/NHANES_Column/File[YearFrom='{0}' and GroupName='{1}' and FolderName='{2}']/CodeBook", yearFrom, lblGroupName.Text, lblName.Text);
                                        nodeItem = nav.SelectSingleNode(strExpression);

                                        if (nodeItem != null)
                                        {
                                            chkBox.Attributes.Add("CodeBook", nodeItem.Value);
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
            //get cookies
            //string s = Request.Cookies["1999/Dietary/Dietary Interview - Individual Foods"].Value;

            //return;

            string sessionId = this.Session.SessionID;

            string folder = Path.Combine(Path.GetTempPath(), DateTime.Now.ToString("yyyyMMdd"));    //Guid.NewGuid().ToString());
            if (!Directory.Exists(folder) && !File.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }            

            string studyYear = "";
            List<NHANESFile>[] studyArrayList = new List<NHANESFile>[8];
            for (int i = 0; i < 8; i++)
            {
                studyArrayList[i] = new List<NHANESFile>();
            }

            CheckBox[] checkBoxArray = new CheckBox[8];
            foreach (GridViewRow row in GridViewStudy.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    int yearFrom = 1999;
                    for (int i = 0; i < 8; i++)
                    {
                        string chkBoxId = "chkRow" + yearFrom.ToString();
                        checkBoxArray[i] = row.FindControl(chkBoxId) as CheckBox;

                        if (checkBoxArray[i] != null && checkBoxArray[i].Checked)
                        {
                            Label lblId = row.FindControl("lblId") as Label;
                            Label lblName = row.FindControl("lblName") as Label;
                            Label lblGroupName = row.FindControl("lblGroupName") as Label;

                            if (lblId != null && lblName != null && lblGroupName != null)
                            {
                                string cookieName = yearFrom.ToString() + lblId.Text;
                                if (Request.Cookies[cookieName] != null)
                                {
                                    NHANESFile file = new NHANESFile()
                                    {
                                        YearFrom = yearFrom.ToString(),
                                        GroupName = lblGroupName.Text,
                                        FolderName = lblName.Text,
                                        ColumnName = HttpUtility.UrlDecode(Request.Cookies[cookieName].Value)
                                    };

                                    studyArrayList[i].Add(file);
                                    //studyArrayList[i].Add(lblGroupName.Text + "\\" + lblName.Text + "\\" + HttpUtility.UrlDecode(Request.Cookies[cookieName].Value));
                                }
                            }
                        }

                        yearFrom += 2;
                    }

                    //for (int i = 0; i < 7; i++)
                    //{
                    //    if (checkBoxArray[i] != null && checkBoxArray[i].Checked)
                    //    {
                    //        Label lblName = row.FindControl("lblName") as Label;
                    //        Label lblGroupName = row.FindControl("lblGroupName") as Label;

                    //        if (lblName != null && lblGroupName != null)
                    //        {
                    //            if (lblGroupName.Text.Equals("Demographic"))
                    //            {
                    //                studyArrayList[i].Add(lblName.Text);
                    //            }
                    //            else
                    //            {
                    //                studyArrayList[i].Add(lblGroupName.Text + " data\\" + lblName.Text);
                    //            }
                    //        }

                    //    }
                    //}

                }
            }

            int yearHeader = 4;
            for (int i = 0; i < 8; i++)
            {
                studyYear = GridViewStudy.HeaderRow.Cells[yearHeader].Text;
                //studyYear = studyYear.Replace('-', '_');

                if (studyArrayList[i].Count > 0)
                {
                    //_tables.Add(studyYear, studyArrayList[i]);

                    //find the same group in arraylist
                    _tables = new Dictionary<string, List<NHANESFile>>();
                    List<string> listGroup = new List<string>();

                    foreach (NHANESFile file in studyArrayList[i])
                    {
                        if (file.GroupName.Equals("Demographics"))
                        {
                            listGroup.Add(file.GroupName);
                        }
                        else
                        {
                            //string[] names = fileName.Split('\\');
                            //if (!listGroup.Contains(names[0]))
                            //{
                            //    listGroup.Add(names[0]);
                            //}
                            if (!listGroup.Contains(file.GroupName))
                            {
                                listGroup.Add(file.GroupName);
                            }
                        }
                    }

                    foreach (string groupName in listGroup)
                    {
                        List<NHANESFile> sameGroupList = new List<NHANESFile>();
                        foreach (NHANESFile file in studyArrayList[i])
                        {
                            //string[] names = fileName.Split('\\'); 
                            //if (groupName.Equals(names[0]))
                            //{
                            //    sameGroupList.Add(fileName);
                            //}

                            if (file.GroupName.Equals(groupName))
                            {
                                sameGroupList.Add(file);
                            }
                        }

                        _tables.Add(groupName, sameGroupList);
                    }

                    _yeartables.Add(studyYear, _tables);
                }

                yearHeader += 1;
            }
           
            ////call SAS
            string macorPath = ConfigurationManager.AppSettings["NHANESMacro"];
            string macroSource = Server.MapPath(macorPath); //@"C:\VS2013\HealthData2\SASMacro\combineall.txt";

            string fileSource = ConfigurationManager.AppSettings["NHANESSource"];

            string SASCode = SASBuilder.BuildNHANESCode(_yeartables, folder, macroSource, fileSource);

            SASBuilder.RunSAS(SASCode);

            //download SAS code
            string codeFileName = @"sascode.txt";
            string codeFilePath = string.Format("{0}\\{1}", folder, codeFileName);
            using (StreamWriter sw = File.CreateText(codeFilePath))
            {
                sw.Write(SASCode);
            }

            //open file dialog
            String FileName = @"merged.sas7bdat";
            String FilePath = string.Format("{0}\\{1}", folder, FileName);  //@"D:\NHANES_EXTRA\1999-2000\lab\Biochemistry Profile and Hormones\lab18.sas7bdat"; //Replace this

            if (DownloadableProduct_Tracking(FilePath, FileName))
            {
                //Request.Headers.Add(Request.Headers);
                //Response.Redirect(Request.RawUrl);

                //Response.AppendHeader("Refresh", "0;URL=/NHANES.aspx");
            }
            else
            {
                Response.Write("<script>alert('failed');</script>");
            };
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

                        //Response.Write("<script type='text/javascript'>");
                        //Response.Write("window.location = '" + Request.RawUrl + "'</script>");

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

        
        private void HideUnHideToggleButtons(TableCell cell, bool hideCollapseButton, bool hideExpandButton)
        {
            //ImageButton imgExpand = (ImageButton)cell.FindControl("imgExpand");
            //imgExpand.Visible = !hideExpandButton;
            Image imgCollapse = (Image)cell.FindControl("imgCollapse");
            imgCollapse.Visible = !hideCollapseButton;
        }

        protected void GridViewStudy_RowCreated(object sender, GridViewRowEventArgs e)
        {
            // The GridViewCommandEventArgs class does not contain a
            // property that indicates which row's command button was
            // clicked. To identify which row's button was clicked, use
            // the button's CommandArgument property by setting it to the
            // row's index.
            //if (e.Row.RowType == DataControlRowType.DataRow)
            //{
            //    ImageButton imgExpand = (ImageButton)e.Row.Cells[0].FindControl("imgExpand");
            //    imgExpand.CommandArgument = e.Row.RowIndex.ToString();
            //    ImageButton imgCollapse = (ImageButton)e.Row.Cells[0].FindControl("imgCollapse");
            //    imgCollapse.CommandArgument = e.Row.RowIndex.ToString();
            //}
        }

        protected void GridViewStudy_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // If multiple buttons are used in a GridView control, use the
            // CommandName property to determine which button was clicked.
            if (e.CommandName == "Expand")
            {
                // Convert the row index stored in the CommandArgument
                // property to an Integer.
                int currRowIndex = Convert.ToInt32(e.CommandArgument);

                // Retrieve the row that contains the button clicked
                // by the user from the Rows collection.
                GridViewRow row = GridViewStudy.Rows[currRowIndex];

                int productCount = GridViewStudy.Rows.Count;
                //starting from the row on which user clicked the "expand" button
                //make all rows visible until you find the next group header row
                for (int i = currRowIndex + 1; i < productCount; i++)
                {
                    if (GridViewStudy.Rows[i].Cells[1].Text.Replace("&nbsp;", "") != "")
                    {
                        GridViewStudy.Rows[i].Visible = true;
                    }
                    else
                    {
                        //we have reached the end of the current group
                        //make expand image invisible and collapse image visible
                        //HideUnHideToggleButtons(row.Cells[0], false, true);
                        break;
                    }
                    //if we are dealing with the last row,
                    //hide/unhide collapse/expand logic needs to be
                    //handled here
                    //if (i + 1 == GridViewStudy.Rows.Count)
                    //    HideUnHideToggleButtons(row.Cells[0], false, true);
                }
            }

            if (e.CommandName == "Collapse")
            {
                // Convert the row index stored in the CommandArgument
                // property to an Integer.
                int index = Convert.ToInt32(e.CommandArgument);
                GridViewRow row = GridViewStudy.Rows[index];
                for (int i = index + 1; i < GridViewStudy.Rows.Count; i++)
                {
                    if (GridViewStudy.Rows[i].Cells[1].Text.Replace("&nbsp;", "") != "")
                    {
                        GridViewStudy.Rows[i].Visible = false;
                    }
                    else
                    {
                        //we have reached the end of the current group
                        //make expand image visible and collapse image invisible
                        //HideUnHideToggleButtons(row.Cells[0], true, false);
                        break;
                    }
                    //if we are dealing with the last row,
                    //hide/unhide collapse/expand logic needs to be
                    //handled here
                    //if (i + 1 == GridViewStudy.Rows.Count)
                    //    HideUnHideToggleButtons(row.Cells[0], true, false);
                }
            }
        }

        

    }

    //public class NHANESTable
    //{
    //    public NHANESTable(int studyId, string studyGroupShortName)
    //    {
    //        StudyId = studyId;
    //        StudyGroupShortName = studyGroupShortName;
    //    }

    //    public int StudyId { get; set; }
    //    public string StudyYear { get; set; }
    //    public string StudyGroupShortName { get; set; }
    //    public string StudyName { get; set; }
    //}
    
}