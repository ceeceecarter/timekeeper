using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class TimesheetViewModel : BaseViewModel
    {
        public LoggedInUserViewModel LoggedInUser { get; set; }
        public int EmployeeInfoId { get; set; }
        public int MasterUserId { get; set; }
        public bool IsUserNonExempt { get; set; }
        public bool IsUserHRAdmin { get; set; }
        public bool IsPayPeriodActionOccurring { get; set; }
        public string PayPeriodActionOccurring { get; set; }
        public int SelectedHoursType { get; set; }
        public bool cbSelectAll { get; set; } 
        public bool cbMonday { get; set; }
        public bool cbTuesday { get; set; }
        public bool cbWednesday { get; set; }
        public bool cbThursday { get; set; }
        public bool cbFriday { get; set; }
        public bool cbSaturday { get; set; }
        public bool cbSunday { get; set; } 
        public bool cbDateRange { get; set; } 
        public DateTime tbDateRangeStart { get; set; }
        public DateTime tbDateRangeEnd { get; set; } 
        public DateTime tbDateSingleEntry { get; set; }
        public DateTime MileageDate { get; set; }
        public string MileageFrom { get; set; }
        public string MileageTo { get; set; }
        public string MileageDescription { get; set; }
        public float MileageMiles { get; set; }

        public int SelectedAdditionalTimeStart { get; set; }

        public int SelectedAdditionalTimeEnd { get; set;}

        public int SelectedAdditionalTimeMinutesStart { get; set; }

        public int SelectedAdditionalTimeMinutesEnd { get; set; }

        public int SelectedAMTimeStart { get; set; }
        public int SelectedAMTimeEnd { get; set; }
        public int SelectedAMMinutesStart { get; set; }
        public int SelectedAMMinutesEnd { get; set; }
        public int SelectedPMTimeStart { get; set; }
        public int SelectedPMTimeEnd { get; set; }
        public int SelectedPMMinutesStart { get; set; }
        public int SelectedPMMinutesEnd { get; set; } 
        public int SelectedPayPeriodID { get; set; }
        public decimal SelectedHourMinuteInDecimal { get; set; }
        public PayPeriodViewModel SelectedPayPeriod { get; set; } 
        public EmployeeIndividualViewModel EmployeeIndividual { get; set; } 
        public List<TimesheetHoursViewModel> TimesheetHours { get; set; }
        public YearAndPayPeriodViewModel YearAndPayPeriod { get; set; }
        public List<SelectListItem> HoursType { get; set; }

        public List<SelectListItem> HoursTypeOnCall { get; set; }
        public List<SelectListItem> PickerHours { get; set; }
        public List<SelectListItem> PickerMinutes { get; set; }
        public List<TSMileageViewModel> MileageEntries { get; set; }
        
        public List<TSOnCallViewModel> OnCallEntries { get; set; }
        public List<SelectListItem> PickerHourMinuteInDecimal { get; set; }
        public List<TimeOffSummaryViewModel> TimeOffSummaryYTD { get; set; }
        public List<HoursTypeSummaryViewModel> TimeOffSummaryNonExempt { get; set; }
        public List<HoursTypeSummaryViewModel> OnCallSummaryNonExempt { get; set; }
        public List<HoursTypeSummaryViewModel> MileageSummary { get; set; }
        //public List<TSHoursTypeSummaryViewModel> HoursTypeSummary { get; set; }
        public List<TimeOffSummaryViewModel> TimeOffSummaryExempt { get; set; } 
        public string NonExemptTimesheetStatus { get; set; }
        public List<HoursTypeSummaryViewModel> MilageSummary { get; set; }
        public List<TSHoursTypeSummaryViewModel> HoursTypeSummary { get; set; }
        public PTOBalanceViewModel PTOBalances { get; set; }
        public List<IndividualDelegateViewModel> IndividualDelegates { get; set; } 

        public int EditSelectedMinuteStart { get; set; }
        public int EditSelectedMinuteEnd { get; set; }
        public int EditSelectedHourStart { get; set; }
        public int EditSelectedHourEnd { get; set; }
        public decimal EditSelectedHourMinuteInDecimal { get; set; }
        public int EditTimesheetHourId { get; set; }
        public DateTime EditSelectedDateEntry { get; set; }
        [DataType(DataType.MultilineText)]
        public string Comments { get; set; }
        public IndividualViewModel ManagerOfIndividual { get; set; }
        public List<EmployeeIndividualViewModel> DelegatedEmployees { get; set; }
        public List<EmployeeIndividualViewModel> DirectEmployees { get; set; }
        public List<SelectListItem> SelectedYearPayPeriods { get; set; }
        public EmployeeIndividualViewModel SelectedEmployeeIndividual { get; set; }
        public List<TimesheetHoursViewModel> WeekOneTimeEntries { get; set; }
        public List<TimesheetHoursViewModel> WeekTwoTimeEntries { get; set; }
        public List<TimesheetHoursViewModel> TimeEntriesYTD { get; set; }
        public List<SelectListItem> PayPeriodYearList { get; set; } 
        public TimeEntryWeeklyDateRangeViewModel WeeklyDateRange { get; set; }
        public int SelectedPayPeriodYear { get; set; } 

        public TimesheetViewModel()
        {
            DelegatedEmployees = new List<EmployeeIndividualViewModel>();
            DirectEmployees = new List<EmployeeIndividualViewModel>();
            IndividualDelegates = new List<IndividualDelegateViewModel>();
            WeeklyDateRange = new TimeEntryWeeklyDateRangeViewModel();
            PayPeriodYearList = new List<SelectListItem>();
            LoggedInUser = new LoggedInUserViewModel();
            IsPayPeriodActionOccurring = false;
        }
    }
}