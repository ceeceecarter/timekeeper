using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class SelectedTimeEntryViewModel
    {
        public string EmployeeInfoId { get; set; } 
        public string Name { get; set; }
        public List<string> Values { get; set; } 
        public List<int> SelectedDelegateIds { get; set; }
        public string Comments { get; set; } 
        public string SelectedPayPeriodId { get; set; }
        public SelectedTimeEntryViewModel()
        {
            SelectedDelegateIds = new List<int>();
            Values = new List<string>();
        }
    }
}