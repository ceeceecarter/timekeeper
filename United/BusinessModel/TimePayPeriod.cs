using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NM.Web.WebApplication.Timesheets.United.BusinessModel
{
    public class TimePayPeriod
    {
        public int PayPeriodID { get; set; }
        public DateTime dtmPeriodStart { get; set; }
        public DateTime dtmPeriodEnd { get; set; }
        public DateTime dtmPeriodDue { get; set; }
        public DateTime dtmPeriodPayDay { get; set; }
        public DateTime? dtmProcessed { get; set; }
        public string txtLastPeriodOfYear { get; set; }
        public string txtStatus { get; set; }
        public bool IsBiWeeklyPayroll { get; set; }
        public string TimePeriod { get; set; }
    }
}

