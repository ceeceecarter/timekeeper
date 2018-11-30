using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class EmployeeAdministrationViewModel
    {
        public List<EmployeeIndividualViewModel> Employees { get; set; }
        public EmployeeIndividualViewModel Employee { get; set; }
        public LoggedInUserViewModel LoggedInUser { get; set; }

        public EmployeeAdministrationViewModel()
        {
            Employees = new List<EmployeeIndividualViewModel>();
            Employee = new EmployeeIndividualViewModel();
        }
    }
}