using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class OverViewTimeOffSummaryViewModel
    {
        public int EmployeeInfoID { get; set; }
        public int FileNumberId { get; set; }
        public string StatusCode { get; set; }
        public decimal TimeOffTotal { get; set; }

    }
}