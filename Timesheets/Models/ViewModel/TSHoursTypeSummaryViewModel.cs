using NM.Web.WebApplication.Timesheets.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class TSHoursTypeSummaryViewModel
    {
        public Enumeration.HoursType HoursType { get; set; }
        public int HoursTypeId { get; set; }
        public string HoursTypeName { get; set; } 
        public decimal NonSubmitted { get; set; }
        public decimal Submitted { get; set; }
        public decimal Processed { get; set; }
        public decimal Approved { get; set; } 
        public decimal Total { get; set; } 
    }
}