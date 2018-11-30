using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class PTOBalancesDataViewModel
    {
        public List<PTOBalanceColumns> ColumnsToImport { get; set; }
        public DateTime LastUpdateOfPTOBalances { get; set; }
        public bool ImportAccrualRate { get; set; }

        public System.Data.DataTable DataTable { get; set; } 

    }
}

