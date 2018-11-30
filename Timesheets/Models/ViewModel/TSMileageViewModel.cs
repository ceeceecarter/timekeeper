using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class TSMileageViewModel : IMapFrom<NM.Lib.DataLibrary.DataAccess.United.tblTSTimesheetHour>
    {
        public int TimesheetHoursID { get; set; }

        public int EmployeeInfoID { get; set; }
        public int? StatusTypeID { get; set; }
        public int SelectedPayPeriodID { get; set; }
        //public string DayOfTheWeek { get; set; }
        public DateTime Date { get; set; }
        public string MileageFrom { get; set; }
        public string MileageTo { get; set; }

        public int SelectedHoursType { get; set; }
        public string MileageDescription { get; set; }
        public float MileageMiles { get; set; }
        public int? PayPeriodID { get; set; }
        public DateTime EntryDate { get; set; }
        public int EntryUser { get; set; }
        public bool LockOutEmployee { get; set; }
        public bool LockOutManager { get; set; }
        public bool LockOutAll { get; set; }
        public decimal Hours { get; set; }
        public int HoursTypeID { get; set; }
        public virtual TSHoursTypeViewModel tblTSHoursType { get; set; }
        public virtual TSStatusTypeViewModel tblTSStatusType { get; set; }

        //unmapped property
        public float MileageRate { get; set; }
        public DateTime MileageEffectiveDate { get; set; } 

    }
}

