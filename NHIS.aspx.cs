using HealthData2.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using HealthData2.Service;
using System.IO;

namespace HealthData2
{
    public partial class NHIS : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                BindGrid();
            }
        }

        private void BindGrid()
        {
            string filePath = ConfigurationManager.AppSettings["NHIS"];
            string path = Server.MapPath(filePath);
            DataTable dt = new DataTable("Study");

            if (path != null)
            {
                try
                {
                    //Add Columns in datatable - Column names must match XML File nodes 
                    dt.Columns.Add("Id", typeof(System.String));
                    dt.Columns.Add("Name", typeof(System.String));
                    dt.Columns.Add("FileExists", typeof(System.String));

                    // Reading the XML file and display data in the gridview         
                    dt.ReadXml(path);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            //GridViewStudy.DataSource = dt;
            GridViewStudy.DataSource = dt;
            GridViewStudy.DataBind();  

            GridViewStudy.HeaderRow.TableSection = TableRowSection.TableHeader;
        }

        protected void GridViewStudy_PreRender(object sender, EventArgs e)
        {
            // disable check box if file doesn't exist
            DisableCheckBox(GridViewStudy);
        }

        private void DisableCheckBox(GridView gridView)
        {
            string filePath = ConfigurationManager.AppSettings["NHISFile"];
            string filePath2 = ConfigurationManager.AppSettings["NHISFileJson"];
            string path = Server.MapPath(filePath);

            //convert xml to json
            XmlDocument doc = new XmlDocument();
            doc.Load(HttpContext.Current.Server.MapPath(filePath2));
            //doc.RemoveChild(doc.FirstChild);
            JsonConvert.SerializeXmlNode(doc);

            IEnumerable<NHISFile> nhisFiles = XDocument.Load(path).Elements("NHISFile").Descendants("Study")
                .Select(x => new NHISFile
                {
                    Id = x.Element("Id").Value,
                    YearOfStudy = x.Element("YearOfStudy").Value,
                    GroupName = x.Element("GroupName").Value,
                    FolderName = x.Element("FolderName").Value,
                    FileName = x.Element("FileName").Value,
                    ColumnName = x.Element("ColumnName").Value.Split(',')
                });

            foreach (GridViewRow row in gridView.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    for (int i = 2004; i < 2016; i++)
                    {
                        string chkBoxId = "chkRow" + i.ToString();
                        var controls = row.Controls;

                        CheckBox chkBox = row.FindControl(chkBoxId) as CheckBox;

                        if (chkBox != null)
                        {
                            string folderName;
                            Label lblGroupName = row.FindControl("lblGroupName") as Label;

                            if (lblGroupName != null)
                            {
                                var file = nhisFiles.SingleOrDefault(
                                        f => f.YearOfStudy == i.ToString() && f.GroupName == lblGroupName.Text);

                                if (file != null)
                                {
                                    chkBox.Enabled = Int32.Parse(file.Id) > 0 ? true : false;
                                    chkBox.Attributes.Add("Id", file.Id);
                                    chkBox.Attributes.Add("YearFrom", file.YearOfStudy);
                                    chkBox.Attributes.Add("GroupName", file.GroupName);
                                    chkBox.Attributes.Add("FolderName", file.FolderName);
                                    chkBox.Attributes.Add("FileName", file.FileName);
                                    chkBox.Attributes.Add("ColumnName", "");

                                    //foreach (var ctrl in controls)
                                    //{
                                    //    (WebControl)ctrl).
                                    //    //if((WebControl)ctrl)..CssClass == "instructions" )
                                    //}
                                }
                               
                            }
                        }
                    }
                }
            }
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string[] selected = selectedFiles.Value.Split(';');

            var jsonStr = JsonConvert.SerializeObject(selected);

            if (selected.Length > 0)
            {
                //string folder = Path.Combine(Path.GetTempPath(), "SAS Temporary Files\\"+ DateTime.Now.ToString("yyyyMMddHHmmss"));
                string folder = Path.Combine("C:\\SAS\\", DateTime.Now.ToString("yyyyMMddHHmmss"));
                if (!Directory.Exists(folder) && !File.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }    
                
                // create SAS code
                string sasCode = GetSASCode(jsonStr, folder);

                //call SAS service
                SASService sas = new SASService();
                sas.SaveFile("sascode.txt", sasCode, folder);

                var connectionError = sas.ConnectToSAS();
                sas.SaveFile("connection.txt", connectionError, folder);
                if (connectionError != string.Empty)
                {
                    var errorMsg = string.Format("Can not connect to SAS, error {0}.", connectionError); 
                    Response.Write("<script>alert(errorMsg)</script>");
                }
                else
                {
                    string log = sas.RunSASProgram(sasCode);
                    sas.SaveFile("log.txt", log, folder);

                    String fileName = @"nhis.sas7bdat";
                    String filePath = string.Format("{0}\\{1}", folder, fileName);

                    if (!DownloadDataSet(filePath, fileName))
                    {
                        Response.Write("<script>alert('Download failed.');</script>");
                    };
                }

            }

          
            //todo uncheck file table
            //ResetGridView();

            return;
        }
      

        private string GetSASCode(string jsonStr, string folder)
        {
            string macorPath = ConfigurationManager.AppSettings["NHANESMacro"];
            string macroSource = Server.MapPath(macorPath);

            string fileSource = ConfigurationManager.AppSettings["NHISSource"];

            StringBuilder sb = new StringBuilder();
            var rootLibName = "nhis";
            //sb.AppendFormat("%include '{0}';", macroSource);
            sb.AppendFormat("libname {0} '{1}';", rootLibName, folder);
            sb.AppendLine();

            var jsonObj = JsonConvert.DeserializeObject<List<dynamic>>(jsonStr);

            foreach (var item in jsonObj)
            {
                if (item.ToString() != string.Empty)
                {
                    var iParser = JObject.Parse(item);
                    var id = iParser["Id"];
                    var groupName = iParser["GroupName"];
                    var filePath = iParser["FolderName"];
                    var fileName = iParser["FileName"];
                    //fileName = fileName.Value.Split('.')[0];
                    var columns = iParser["Columns"];
                    //columns = columns.Value.Replace(",", " ");
                    var libName = "N" + id;

                    if (Directory.GetFiles(filePath.Value, fileName.Value).Length > 0)
                    {
                        sb.AppendFormat("libname {0} '{1}';", libName, filePath);
                        sb.AppendLine();
                        sb.AppendFormat("libname library '{0}';", filePath);
                        sb.AppendLine();
                        sb.AppendFormat("data {0}.{1}; set {2}.{3} (keep={4}); run;", rootLibName, libName, libName, fileName.Value.Split('.')[0], columns.Value.Replace(",", " "));
                        sb.AppendLine();

                    }
                    
                }

            }

            sb.AppendFormat("data {0}.{1}; set ", rootLibName, rootLibName);
            foreach (var item in jsonObj)
            {
                if (item.ToString() != string.Empty)
                {
                    var iParser = JObject.Parse(item);
                    var id = iParser["Id"];

                    //data nhis.all; set nhis.N68 nhis.N75; run;

                    sb.AppendFormat("{0}.{1} ", rootLibName, "N" + id);

                }
            }

            sb.Append("; run;");

            return sb.ToString();
        }

        private bool DownloadDataSet(string filePath, string fileName)
        {                                 
            string _DownloadableProductFileName = fileName;       

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

        //private void ResetGridView()
        //{
        //    foreach (GridViewRow row in GridViewStudy.Rows)
        //    {
        //        if (row.RowType == DataControlRowType.DataRow)
        //        {
        //            for (int i = 2004; i < 2016; i++)
        //            {
        //                string chkBoxId = "chkRow" + i.ToString();
        //                var controls = row.Controls;
        //                CheckBox chkBox = row.FindControl(chkBoxId) as CheckBox;

        //                if (chkBox != null && chkBox.Checked)
        //                    chkBox.Checked = false;
        //            }

        //        }
        //    }
        //}

    }
}