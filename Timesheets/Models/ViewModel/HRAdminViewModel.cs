using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class HRAdminViewModel : BaseViewModel
    {
        public YearAndPayPeriodViewModel YearAndPayPeriod { get; set; }
        public string PayPeriodDisplayDates { get; set; }
        public List<PayPeriodViewModel> PayPeriods { get; set; }
        public List<SelectListItem> PayrollYears { get; set; }
        public int WorkingWithYear { get; set; }
        public LoggedInUserViewModel LoggedInUser { get; set; }
        public HRAdminViewModel()
        {
            LoggedInUser = new LoggedInUserViewModel();
        }
    }
}