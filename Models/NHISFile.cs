using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HealthData2.Models
{
    public class NHISFile
    {
        public string Id { get; set; }
        public string YearOfStudy { get; set; }
        public string GroupName { get; set; }
        public string FolderName { get; set; }
        public string FileName { get; set; }
        public string[] ColumnName { get; set; }
    }
}