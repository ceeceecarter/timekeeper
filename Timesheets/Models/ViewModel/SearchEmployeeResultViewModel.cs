using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class SearchEmployeeResultViewModel : IMapFrom<NM.Lib.DataLibrary.United.Domain.SearchEmployeeResultDTO>
    {
        public int EmployeeInfoId { get; set; }
        public int MasterUserId { get; set; }
        public int IndividualId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FileNumber { get; set; }
        public decimal Balance { get; set; }
        public int CompanyCodeId { get; set; }
        public string CompanyCode { get; set; }
        public string FlsaStatus { get; set; }
        public string EmploymentStatus { get; set; }

        //unmapped properties

    }
}
