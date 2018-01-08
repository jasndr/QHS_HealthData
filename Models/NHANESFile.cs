using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HealthData2.Models
{
    public class NHANESFile
    {
        public string YearFrom { get; set; }
        public string GroupName { get; set; }
        public string FolderName { get; set; }
        public string CodeBook { get; set; }
        public string ColumnName { get; set; }
    }
}