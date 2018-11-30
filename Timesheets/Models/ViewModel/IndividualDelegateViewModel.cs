using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class IndividualDelegateViewModel : IMapFrom<NM.Lib.DataLibrary.United.Domain.IndividualDelegateDTO>
    {
        public int DelegateID { get; set; }
        public int IndividualID { get; set; }
        public int DelegateToIndividualID { get; set; }
        public bool IsPrimaryDelegate { get; set; } 
        public EmployeeIndividualViewModel DelegateToIndividual { get; set; }
    }
}
