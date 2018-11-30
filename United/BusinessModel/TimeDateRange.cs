using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NM.Web.WebApplication.Timesheets.United.BusinessModel
{
    public class TimeDateRange
    {
        public DateTime dtmDate { get; set; }
        public int intDate { get; set; }
        public int intDay { get; set; }
        public int intMonth { get; set; }
        public int intYear { get; set; }
        public int intDayOfWeek { get; set; }
        public string strMonth { get; set; }
        public string strDayOfWeek { get; set; }

    }
}
