using NM.Web.WebApplication.Timesheets.Infrastructure.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class HoursTypeSummaryViewModel
    {
        public int WeekNumber { get; set; }
        public int DisplayOrder { get; set; }
        public int HoursTypeID { get; set; }
        public string HoursType { get; set; }
        public decimal TotalWeek1 { get; set; } 
        public decimal TotalWeek2 { get; set; }
        public decimal Total { get; set; }

        public double? TotalWeek1OnCall { get; set; }
        public double? TotalWeek2OnCall { get; set; }
        public double? TotalOnCall { get; set; }

        public float? TotalWeek1Milage { get; set; }
        public float? TotalWeek2Milage { get; set; }
        public float? TotalMilage { get; set; }

    }
}