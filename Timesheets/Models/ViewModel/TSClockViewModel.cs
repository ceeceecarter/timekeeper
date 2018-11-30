using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    #region Classes

    public class TSClockViewModel
    {
        public List<Hour> Hours { get; set; }
        public List<Minute> Minutes { get; set; }

        public TSClockViewModel()
        {
            Hours = new List<Hour>();
            Minutes = new List<Minute>();
        }
    }

    public class Hour
    {
        public int HourValue { get; set; }
        public string HourText { get; set; }
    }

    public class Minute
    {
        public int MinuteValue { get; set; }
        public string MinuteText { get; set; }
    }

    #endregion
}