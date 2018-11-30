using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class EmployeeInformationViewModel : IMapFrom<NM.Lib.DataLibrary.United.Domain.EmployeeInformationDTO>
    {
        public int EmployeeInfoId { get; set; }
        public int MasterUserID { get; set; }
        public int IndividualID { get; set; }


        [Display(Name = "Company")]
        public int CompanyCodeId { get; set; }

        public Nullable<bool> HRBP { get; set; }
        public Nullable<bool> IsNewPosition { get; set; }
        public Nullable<int> ReplacingMasterUserId { get; set; }
        public Nullable<System.DateTime> DateOfWelcomeEmail { get; set; }
        public Nullable<System.DateTime> DateOfNewHireFormSent { get; set; }
        public Nullable<System.DateTime> DateOfEnteredInPayForce { get; set; }
        public Nullable<System.DateTime> DateOfTimeSaverReminder { get; set; }
        public Nullable<System.DateTime> DateOfBackgroundCheck { get; set; }
        public Nullable<System.DateTime> DateOfEVerify { get; set; }
        public Nullable<System.DateTime> DatePopcornSent { get; set; }
        public Nullable<int> WorkStateID { get; set; }
        public string ReasonForTermination { get; set; }
        public string SpecialPayIssues { get; set; }
        public Nullable<bool> IsLetterReceived { get; set; }
        public Nullable<bool> IsNopaReceived { get; set; }
        public Nullable<System.DateTime> DateOfEmailSentToTechOps { get; set; }
        public Nullable<System.DateTime> DateOfExitInterview { get; set; }
        public Nullable<System.DateTime> DateOfInformationToPayroll { get; set; }
        public Nullable<System.DateTime> DateOfPTOPayout { get; set; }
        public Nullable<bool> IsNonDisputeUnemployment { get; set; }
        public Nullable<short> chkBenefitEligible { get; set; }
        public Nullable<short> chkPhotoShowExternal { get; set; }
        public Nullable<short> chkPhotoShowInternal { get; set; }
        public string intHRSupervisor { get; set; }
        public string memBiography { get; set; }
        public string memHRDelegate { get; set; }
        public Nullable<double> realFTE { get; set; }
        public Nullable<double> realMaxTimeOffPerWeek { get; set; }
        public string txtDomain { get; set; }
        public string txtNewOrRehire { get; set; }
        public string txtOfficer { get; set; }
        public string txtPhotoHighResolution { get; set; }
        public string txtPhotoWeb { get; set; }


        public Nullable<bool> IsNonExempt { get; set; }

        public string txtUserID { get; set; }
        public string txtUserIDFics { get; set; }
        public string txtAutograph { get; set; }
        public string FileNumber { get; set; }


        public Nullable<double> realHoursPerWeek { get; set; }

        public string memTemp { get; set; }
        public string memTemp2 { get; set; }
        public Nullable<short> chkProcessAsNewEmployee { get; set; }
        public Nullable<short> chkPositionChange { get; set; }
        public Nullable<short> chkTimesheetAdmin { get; set; }
        public Nullable<System.DateTime> dtmTempStartDate { get; set; }
        public string txtEmploymentType { get; set; }
        public string txtNewEmployeeIdentifier { get; set; }
        public string ManagerComments { get; set; }
        public string ITComments { get; set; }
        public string PhysicalLocation { get; set; }
        public Nullable<int> PreviousExperience { get; set; }
        public Nullable<System.Guid> TemplateId { get; set; }
        public Nullable<System.Guid> ComputerSetupId { get; set; }
        public System.DateTime AutoAudit_CreatedDate { get; set; }
        public string AutoAudit_CreatedBy { get; set; }
        public System.DateTime AutoAudit_ModifiedDate { get; set; }
        public string AutoAudit_ModifiedBy { get; set; }
        public Nullable<int> AutoAudit_RowVersion { get; set; }
        public Nullable<System.Guid> AccountSetupId { get; set; }
        public string PhonePassCode { get; set; }
        public Nullable<bool> BenefitEligible { get; set; }
        public Nullable<bool> TimesheetAdmin { get; set; }
        public Nullable<System.Guid> EmployeeTerminationid { get; set; }
        public Nullable<bool> UploadToExternalWebSite { get; set; }
        public Nullable<bool> Freddie_Mac_Signatory { get; set; }
        public Nullable<bool> PTOEligible { get; set; }
        public bool? OnCallEligible { get; set; }
        public string EmployeeID { get; set; }
        public int? ManagerEmployeeInfoId { get; set; }


        public int? JobTitleID { get; set; }
        public int? CostCenterID { get; set; }
        public int? OfficeID { get; set; }
        public int? EmploymentTypeID { get; set; }
        public int? WorkLocationID { get; set; }


        public bool? ExcludeFromReminderEmails { get; set; }
        public bool? TimekeeperAccess { get; set; }

        [Display(Name ="Officer Title ID")]
        public int? OfficerTitleID { get; set; }
        public bool Inactive { get; set; } 

    }
}