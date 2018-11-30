//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NM.Web.WebApplication.Timesheets.United.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class tblTSPayPeriod
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public tblTSPayPeriod()
        {
            this.tblTSTimesheetHours = new HashSet<tblTSTimesheetHour>();
        }
    
        public int PayPeriodID { get; set; }
        public string clpCSV { get; set; }
        public Nullable<System.DateTime> dtmPeriodDue { get; set; }
        public Nullable<System.DateTime> dtmPeriodEnd { get; set; }
        public Nullable<System.DateTime> dtmPeriodPayDay { get; set; }
        public Nullable<System.DateTime> dtmPeriodStart { get; set; }
        public Nullable<System.DateTime> dtmProcessed { get; set; }
        public string txtLastPeriodOfYear { get; set; }
        public string txtStatus { get; set; }
        public Nullable<System.DateTime> dtmReminderAllEmployees { get; set; }
        public Nullable<System.DateTime> dtmReminderAllNonExempt { get; set; }
        public Nullable<System.DateTime> dtmReminderUnApproved { get; set; }
        public Nullable<System.DateTime> dtmReminderUnSubmittedNonExempt { get; set; }
        public string txtVerifyDelete { get; set; }
        public Nullable<short> chkDateConflict { get; set; }
        public string clpCSVNonExempt { get; set; }
        public Nullable<System.DateTime> dtm1stReminder { get; set; }
        public Nullable<System.DateTime> dtm2ndReminder { get; set; }
        public Nullable<System.DateTime> dtmApprovalDueDate { get; set; }
        public Nullable<System.DateTime> dtmCutOffDate { get; set; }
        public Nullable<bool> IsBiWeeklyPayroll { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<tblTSTimesheetHour> tblTSTimesheetHours { get; set; }
    }
}
