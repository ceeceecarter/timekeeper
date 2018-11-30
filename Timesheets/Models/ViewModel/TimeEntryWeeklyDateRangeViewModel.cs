using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class TimeEntryWeeklyDateRangeViewModel
    {
        public PayPeriodViewModel PayPeriodViewModel { get; set; }
        public DateTime WeekOneStartDate { get; set; }
        public DateTime WeekOneEndDate { get; set; }
        public DateTime WeekTwoStartDate { get; set; }
        public DateTime WeekTwoEndDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; } 
    }
}
