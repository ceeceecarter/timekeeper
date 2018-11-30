using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class DelegateViewModel
    {
        public List<IndividualDelegateViewModel> ApproverDelegates { get; set; }
        public List<IndividualViewModel> SearchedForEmployees { get; set; }
        public int ManagerID { get; set; }
        public int SelectedIndividualID { get; set; } 

    }
}
