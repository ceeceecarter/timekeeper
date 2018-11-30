using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class PTOBalanceColumns : IMapFrom<NM.Lib.DataLibrary.DataAccess.United.tblTSPTOBalanceColumn>
    {
        public string PTOBalanceColumnName { get; set; }
        public string PTOBalanceColumnDisplayName { get; set; }
        public int? ColumnPositionNumber { get; set; }

    }
}
