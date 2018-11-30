using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class EmployeeInfoHeaderViewModel
    {
        public string FullName { get; set; }
        public string displayYear { get; set; }
        public List<SelectListItem> PayPeriod { get; set; }
        public List<SelectListItem> TimeYears { get; set; }
    }
}