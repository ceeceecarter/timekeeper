using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class YearAndPayPeriodViewModel
    {
        public List<SelectListItem> TimeYears { get; set; }
        public List<PayPeriodViewModel> PayPeriods { get; set; }
        public int CurrentYear { get; set; }
        public PayPeriodViewModel CurrentPayPeriod { get; set; }
        public PayPeriodViewModel PreviousPayPeriod { get; set;}
        public PayPeriodViewModel NextPayPeriod { get; set; }
        public List<SelectListItem> TimePeriods { get; set; }
        public bool IsNonExempt { get; set; } 

        public YearAndPayPeriodViewModel()
        {
            PayPeriods = new List<PayPeriodViewModel>();
            TimeYears = new List<SelectListItem>();
        }
    }
} 