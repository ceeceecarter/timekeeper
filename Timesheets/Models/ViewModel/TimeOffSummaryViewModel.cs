using NM.Web.WebApplication.Timesheets.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class TimeOffSummaryViewModel
    {
        public Enumeration.StatusType StatusType { get; set; }
        public Enumeration.HoursType HoursType { get; set; }
        public int HoursTypeID { get; set; } 
        public string HoursTypeName { get; set; }
        public decimal NonSubmitted { get; set; }
        public decimal Submitted { get; set; }
        public decimal Processed { get; set; }
        public decimal Approved { get; set; }
        public decimal Total { get; set; } 
    }
}