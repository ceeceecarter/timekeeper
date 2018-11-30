using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class TimesheetHoursViewModel : IMapFrom<NM.Lib.DataLibrary.DataAccess.United.tblTSTimesheetHour>
    {
        public int TimesheetHoursID { get; set; }
        public int EmployeeInfoID { get; set; }
        public int? PayPeriodID { get; set; }
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime Date { get; set; }
        public int TimeStart { get; set; }
        public int TimeEnd { get; set; }
        public decimal Hours { get; set; }
        public int HoursTypeID { get; set; }
        public float? MileageMiles { get; set; }

        public Nullable<System.DateTime> MileageDate { get; set; }

        public string MileageTo {get; set;}

        public string MileageFrom { get; set; }

        public Nullable<System.DateTime> OnCallDate { get; set; }
        public double? OnCallDayRate { get; set; }

        public int OnCallHours { get; set; }

        public string MileageDescription { get; set; }
        public decimal? Parking { get; set; }
        public int? StatusTypeID { get; set; }
        public DateTime EntryDate { get; set; }
        public int EntryUser { get; set; }
        public DateTime? Submitdate { get; set; }
        public int? SubmitUser { get; set; }
        public DateTime? ApproveDate { get; set; }
        public int? ApproveUser { get; set; }
        public Nullable<System.DateTime> ProcessDate { get; set; }
        public int? ProcessPayPeriod { get; set; }
        public bool LockOutEmployee { get; set; }
        public bool LockOutManager { get; set; }
        public bool LockOutAll { get; set; }
        public string LocationFrom { get; set; }
        public string LocationTo { get; set; }
        public string Reason { get; set; }
        //public Nullable<int> ProcessedPayPeriod { get; set; }
        public virtual TSHoursTypeViewModel tblTSHoursType { get; set; }
        public virtual TSStatusTypeViewModel tblTSStatusType { get; set; }

    }
}