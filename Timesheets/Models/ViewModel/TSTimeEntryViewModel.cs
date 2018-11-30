using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class TSTimeEntryViewModel : IMapFrom<NM.Lib.DataLibrary.DataAccess.United.fnTSDateRange_Result>
    {
        public DateTime? dtmDate { get; set; }
        public int? intDate { get; set; }
        public int? intDay { get; set; }
        public int? intMonth { get; set; }
        public int? intYear { get; set; }
        public int? intDayOfWeek { get; set; }
        public string strMonth { get; set; }
        public string strDayOfWeek { get; set; }

    }
}