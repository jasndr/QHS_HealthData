using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using HealthData2.Models;
using System.Xml;
using System.Xml.Linq;

namespace HealthData2.Controller
{
    public class NHISController : ApiController
    {
        string _filePath2 = ConfigurationManager.AppSettings["NHISFileJson"];
        private XDocument _xDoc;

        public NHISController()
        {
            _xDoc = XDocument.Load(System.Web.Hosting.HostingEnvironment.MapPath(_filePath2));
        }

        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<controller>/5
        public IHttpActionResult Get(int id)
        {
            //XmlElement elem = _xmlDoc.GetElementById(id.ToString());
            var nhisFile = _xDoc.Elements("NHISFile").Descendants("Study")
                .Select(x => new NHISFile
                {
                    Id = x.Element("Id").Value,
                    YearOfStudy = x.Element("YearOfStudy").Value,
                    GroupName = x.Element("GroupName").Value,
                    FolderName = x.Element("FolderName").Value,
                    FileName = x.Element("FileName").Value,
                    ColumnName = x.Element("ColumnName").Value.Split(',')
                })
                .Where( x => x.Id == id.ToString());
            
            if (nhisFile == null)
                return NotFound();

            return Ok(nhisFile);
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}