using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class PTOBalanceViewModel : IMapFrom<NM.Lib.DataLibrary.United.Domain.PTOBalancesDTO>
    {
        public int PtoBalanceID { get; set; } 
        public string FileNumber { get; set; }
        public DateTime LastUpdated { get; set; }
        public decimal? Accrual { get; set; }
        public decimal? Allowed { get; set; }
        public decimal? Taken { get; set; }
        public decimal? Balance { get; set; }
        public string CompanyCode { get; set; }
  
    }
}
