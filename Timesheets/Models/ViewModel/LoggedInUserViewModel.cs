using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class LoggedInUserViewModel : BaseViewModel
    {
        public int EmployeeInfoID { get; set; }
        public string JobTitleName { get; set; }
        public bool IsUserTSUser { get; set; }
        public bool IsUserTSManager { get; set; }
        public bool IsUserDelegate { get; set; }
        public bool IsUserTSHRAdmin { get; set; }
        public JobTitleViewModel JobTitle { get; set; }
        public int IndividualID { get; set; } 
    }
}
