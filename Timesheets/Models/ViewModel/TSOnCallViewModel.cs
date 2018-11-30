using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class TSOnCallViewModel : IMapFrom<NM.Lib.DataLibrary.DataAccess.United.tblTSTimesheetHour>
    {
        public int TimesheetHoursID { get; set; }
        public DateTime Date { get; set; }
        public double? OnCallRateForDay { get; set; }
        public int EmployeeInfoID { get; set; }
        public int? StatusTypeID { get; set; }
        public int SelectedPayPeriodID { get; set; }
        public int Hours { get; set; }
        public int HoursTypeID { get; set; }
        public int? PayPeriodID { get; set; }
        public DateTime EntryDate { get; set; }
        public int EntryUser { get; set; }
        public bool LockOutEmployee { get; set; }
        public bool LockOutManager { get; set; }
        public bool LockOutAll { get; set; }
        public List<SelectListItem> HoursType { get; set; }

        public int SelectedHoursType { get; set; }

        public virtual TSHoursTypeViewModel tblTSHoursType { get; set; }
    }
}

