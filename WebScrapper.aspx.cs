using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using HtmlAgilityPack;
using HealthData2;
using HealthData2.Service;

namespace HealthData
{
    public partial class WebScrapper : System.Web.UI.Page
    {
        const int HowDeepToScan = 10; 
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                TextBox1.Text = string.Empty;
            }
        }

        public void ProcessDir(string sourceDir, int recursionLvl)
        {
            if (recursionLvl <= HowDeepToScan)
            {
                // Process the list of files found in the directory. 
                string[] fileEntries = Directory.GetFiles(sourceDir);
                foreach (string fileName in fileEntries)
                {
                    // do something with fileName
                    //Console.WriteLine(fileName);
                    //
                    //string sasFileName = fileName.Replace(".XPT", ".sas7bdat");
                    var regex = new Regex(".XPT", RegexOptions.IgnoreCase);
                    if (regex.IsMatch(fileName))
                    {
                        var sasFileName = regex.Replace(fileName, ".sas7bdat");
                        if (!File.Exists(sasFileName))
                        {
                            ConvertXPT(sourceDir, fileName);

                            TextBox1.Text += fileName + "\n";
                        }
                    }
                }


                // Recurse into subdirectories of this directory.
                string[] subdirEntries = Directory.GetDirectories(sourceDir);
                foreach (string subdir in subdirEntries)
                    // Do not iterate through reparse points
                    if ((File.GetAttributes(subdir) &
                         FileAttributes.ReparsePoint) !=
                             FileAttributes.ReparsePoint)

                        ProcessDir(subdir, recursionLvl + 1);
            }
        }

        protected void btnScrap_Click(object sender, EventArgs e)
        {
            //https://wwwn.cdc.gov/Nchs/Nhanes/Search/Nhanes13_14.aspx
            string url = txtUrl.Text;
            string rootPath = txtFolder.Text;
            //string rootPath = @"D:\NHANES\WebScrapper\2003_2004\Demographics\";

            //loop through download folder
            ProcessDir(rootPath, 5);

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);

            List<string> hrefTags = new List<string>();
           
            //string rootPath = Server.MapPath("~/FileDownload/");

            //get list of hrefs contain Demographics, Dietary, Examination, Laboratory, Questionnaire
            List<string> downloadTargetList = new List<string> { "Demographics", "Dietary", "Examination", "Laboratory", "Questionnaire" };
            
            Dictionary<string, string> linkDictionary = new Dictionary<string, string>();

            foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//a[@href]"))
            {
                if (downloadTargetList.Contains(link.InnerText))
                {
                    HtmlAttribute att = link.Attributes["href"];
                    hrefTags.Add(att.Value);

                    //linkDictionary.Add(link.InnerText, att.Value);
                    linkDictionary.Add(link.InnerText, HttpUtility.HtmlDecode(att.Value));
                }
            }

            var subUrl = "";
            foreach (KeyValuePair<string, string> linkPair in linkDictionary)
            {
                var urlArray = url.Split('/');
                //subUrl = url.Replace(urlArray[urlArray.Length - 4], linkPair.Value);
                subUrl = url.Remove(20) + linkPair.Value;

                doc = web.Load(subUrl);

                if (doc.DocumentNode.SelectNodes("//table/tbody/tr") != null)
                {
                    foreach (HtmlNode link in doc.DocumentNode.SelectNodes("//table/tbody/tr"))
                    {
                        //for each tr, get th, td a
                        string localPath = rootPath;
                        //foreach (var child in link.ChildNodes)
                        for (int i = 1; i < 4; i++ )
                        {
                            var child = link.ChildNodes[i];
                            //if (child.Name == "th")
                            //{
                            //    string header = "\\" + linkPair.Key + "\\";
                            //    header += child.InnerText.Replace('/', '%');
                            //    header = header.Replace(":", "");
                            //    hrefTags.Add(header);

                            //    localPath = HttpUtility.HtmlDecode(rootPath + header);
                            //    DirectoryInfo dirInfo = Directory.CreateDirectory(localPath);
                            //}

                            if (child.Name == "td")
                            {
                                //if (child.ChildNodes[1].Attributes["href"] != null)
                                //{
                                //    hrefTags.Add(child.ChildNodes[1].Attributes["href"].Value);
                                //}
                                if (!child.InnerHtml.Contains("a href"))
                                {
                                    string header = "\\" + linkPair.Key + "\\" + Regex.Replace(HttpUtility.HtmlDecode(child.InnerText), @"[^\w\s\-\&\(\)\+\,]", ""); 
                                    //header = header.Replace('/', '%');
                                    //header = header.Replace(":", "");
                                    //header = header.Replace(",", "");
                                    //header = header.Replace("'", "");    
                                    //hrefTags.Add(header.Trim());

                                    localPath = HttpUtility.HtmlDecode(rootPath + header.Trim());
                                    DirectoryInfo dirInfo = Directory.CreateDirectory(localPath);
                                }

                                if (child.InnerHtml.Contains("a href"))
                                {
                                    hrefTags.Add(child.InnerHtml.ToString());

                                    MatchCollection mc1 = Regex.Matches(child.InnerHtml.ToString(), @"(<a.*?>.*?</a>)", RegexOptions.Singleline);

                                    foreach (Match m in mc1)
                                    {
                                        Match mc2 = Regex.Match(m.Groups[1].Value, @"href=\""(.*?)\""", RegexOptions.Singleline);

                                        if (mc2.Success)
                                        {
                                            //using (var client = new WebClient())
                                            //{
                                            //    client.DownloadFile(new Uri(mc2.Groups[1].Value), rootPath);
                                            //}

                                            string newURL = "http://wwwn.cdc.gov" + mc2.Groups[1].Value;
                                            var temp = newURL.Split('/');
                                            var fileName = localPath + '\\' + temp[temp.Length - 1];
                                            if (!File.Exists(fileName))
                                            {
                                                //var thread = new Thread(() => DownloadFile(newURL, fileName));

                                                //// create a delegate to the TreadEnd function
                                                ////ThreadEndDelegate EndDelegate = ThreadEnd;

                                                //thread.Start();

                                                DownloadFile(newURL, fileName);
                                            }

                                            //convert transport file xpt to SAS dataset
                                            //string sasFileName = fileName.Replace(".XPT", ".sas7bdat");
                                            var regex = new Regex(".XPT", RegexOptions.IgnoreCase);
                                            var sasFileName = regex.Replace(fileName, ".sas7bdat");
                                            if (!File.Exists(sasFileName))
                                            {
                                                if (regex.IsMatch(fileName))
                                                {
                                                    ConvertXPT(localPath, fileName);
                                                }
                                            }
                                        }
                                    }


                                }
                            }

                        }

                    }
                }
            }

            List<string> s = hrefTags;
        }

   

        private void ConvertXPT(string localPath, string fileName)
        {
            //call SAS
            string SASCode = SASBuilder.ConvertXPT(localPath, fileName);

            SASBuilder.RunSAS(SASCode);
        }

        // ---- prototype of function to be passed to thread
        delegate void ThreadEndDelegate(string s);


        // ---- ThreadEnd ---------------------------------
        // this function gets called when the thread ends


        static protected void ThreadEnd(string s)
        {
            //TextBox1.Text += s + "\n";
        }


        public void DownloadFile(string newURL, string localPath)
        {
            var webClient = new WebClient();
            //webClient.DownloadFileAsync(new Uri(newURL), localPath);
            webClient.DownloadFile(new Uri(newURL), localPath);

            TextBox1.Text += localPath + "\n";

            //ThreadEndDelegate EndDelegate = Data as ThreadEndDelegate;
            //if (EndDelegate != null)
            //    EndDelegate(localPath);
            
        }

        //private async Task DownloadFileAsync(DocumentObject doc)
        //{
        //    try
        //    {
        //        using (WebClient webClient = new WebClient())
        //        {
        //            string downloadToDirectory = @Resources.defaultDirectory + value.docName;
        //            webClient.Credentials = System.Net.CredentialCache.DefaultNetworkCredentials;
        //            await webClient.DownloadFileTaskAsync(new Uri(value.docUrl), @downloadToDirectory);

        //            //Add them to the local
        //            Context.listOfLocalDirectories.Add(downloadToDirectory);
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        Errors.printError("Failed to download File: " + value.docName);
        //    }
        //}

        //private async Task DownloadMultipleFilesAsync(List<DocumentObject> doclist)
        //{
        //    await Task.WhenAll(doclist.Select(doc => DownloadFileAsync(doc)));
        //}
    }
}