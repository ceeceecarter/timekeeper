using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class PayPeriodViewModel : BaseViewModel, IMapFrom<NM.Lib.DataLibrary.United.Domain.TimePayPeriodDTO>
    {
        public int PayPeriodID { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Period Due")]
        [DataType(DataType.DateTime)]
        public DateTime dtmPeriodDue { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Period End Date")]
        [DataType(DataType.DateTime)]
        public DateTime dtmPeriodEnd { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Period Start Date")]
        [DataType(DataType.DateTime)]
        public DateTime dtmPeriodStart { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Period Pay Day")]
        [DataType(DataType.DateTime)]
        public DateTime dtmPeriodPayDay { get; set; }

        //[Required(ErrorMessage = "Status is required")]
        [Display(Name = "Status")]
        [DataType(DataType.Text)]
        public string txtStatus { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Date Processed")]
        [DataType(DataType.DateTime)]
        public DateTime? dtmProcessed { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "First Reminder")]
        [DataType(DataType.DateTime)]
        public DateTime? dtm1stReminder { get; set; }

        [Required]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Second Reminder")]
        [DataType(DataType.DateTime)]
        public DateTime? dtm2ndReminder { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Reminder All Employees")]
        [DataType(DataType.DateTime)]
        public DateTime? dtmReminderAllEmployees { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Reminder All Non-Exempt")]
        [DataType(DataType.DateTime)]
        public DateTime? dtmReminderAllNonExempt { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Reminder Unapproved")]
        [DataType(DataType.DateTime)]
        public DateTime? dtmReminderUnApproved { get; set; }


        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Reminder Unapproved Exempt")]
        [DataType(DataType.DateTime)]
        public DateTime? dtmReminderUnApprovedExempt { get; set; }


        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Reminder Unsubmitted Non-Exempt")]
        [DataType(DataType.DateTime)]
        public DateTime? dtmReminderUnSubmittedNonExempt { get; set; }

        [Required]
        [Display(Name = "Last Period Of Year")]
        public string txtLastPeriodOfYear { get; set; }

        [Display(Name = "BiWeekly Payroll")]
        public bool IsBiWeeklyPayroll { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Approval Due Date")]
        [DataType(DataType.DateTime)]
        public DateTime? dtmApprovalDueDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        [Display(Name = "Cut off Date")]
        [DataType(DataType.DateTime)]
        public DateTime? dtmCutOffDate { get; set; }

        /// <summary>
        /// TimePeriod values are Current, Previous, Next
        /// </summary>
        public string TimePeriod { get; set; }

        [Display(Name = "E CSV")]
        public string clpCSV { get; set; }

        [Display(Name = "NE CSV")]
        public string clpCSVNonExempt { get; set; }

        public bool DisplayCsvPreview { get; set; }
    }
}