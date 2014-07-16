using System;
using CmsWeb.Code;

namespace CmsWeb.Areas.Search.Models
{
    [Serializable]
    public class RegistrationSearchInfo
    {
        public string Registrant { get; set; }
        public string User { get; set; }
        public string Organization { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public CodeInfo Status { get; set; }
        public string count { get; set; }

        public RegistrationSearchInfo()
        {
            Status = new CodeInfo("RegistrationStatus");
        }
    }
}