using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
using System.Net.Http;
using NM.Web.WebApplication.Timesheets.Infrastructure.Enums;
using NM.Web.WebApplication.Timesheets.Models.ViewModel;
using NM.Lib.DataLibrary.United.Interface;


namespace NM.Web.WebApplication.Timesheets.Controllers
{
    public class ManagementController : BaseController
    {
        public ManagementController(ITimesheet repo)
        {
            repository = repo;
        }

        #region Actions
        // GET: Manager Timesheet Info
        public ActionResult Index()
        {
            ManagerViewModel mgrVM = new ManagerViewModel();
            mgrVM.Manager = GetManager();


            if (mgrVM.Manager == null)
            {
                return RedirectToAction("Index", "Timesheets");
            }

            return View("ManagementView", mgrVM);
        }

        public ActionResult EmployeeTimesheets()
        {
            int id = GetEmployeeIndividual().IndividualId;
            ManagerViewModel mgrVM = new ManagerViewModel();
            mgrVM.EmployeeList = GetManagerEmployees(id);

            var managersEmployees = repository.GetManagerIndividualListById(id);


            //mgrVM.DelegateEmployeeList = GetManagerDelegateEmployees(indId);
            //mgrVM.DelegatedToEmployeeList = GetDelegatedToEmployees(indId);
            mgrVM.ManagerIndividualID = id;

            return PartialView("EmployListPartialView", mgrVM);
        }

        [HttpGet]
        public ActionResult ApproveAll(int managerID)
        {
            string url = Url.Action("Index", "ManagementView");

            EmployeeIndividualViewModel managerViewModel = GetManager();
            if (managerViewModel == null)
            {
                return RedirectToAction("Index", "Timesheets");
            }

            ApproveTimesheets(managerViewModel, managerID, 0, 0, 0);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult ApproveAllForEmployee(int empInfoID, int managerID, int delegateID)
        {
            string url = Url.Action("Index", "ManagementView");

            EmployeeIndividualViewModel managerViewModel = GetManager();
            if (managerViewModel == null)
            {
                return RedirectToAction("Index", "Timesheets");
            }

            ApproveTimesheets(managerViewModel, managerID, empInfoID, 0, delegateID);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Approve(int timeSheetHoursID, int empInfoID, int managerID, int delegateID)
        {
            string url = Url.Action("Index", "ManagementView");

            EmployeeIndividualViewModel managerViewModel = GetManager();
            ApproveTimesheets(managerViewModel, managerID, empInfoID, timeSheetHoursID, delegateID);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Submit(int id, int managerID)
        {
            string url = Url.Action("Index", "ManagementView");

            var timeEntry = repository.GetTimesheetHourById(id);
            if (timeEntry != null)
            {
                PersistTimesheetUpdate(timeEntry, managerID, 3);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Reject(int id, int managerID)
        {
            var timeEntry = repository.GetTimesheetHourById(id);
            if (timeEntry != null)
            {
                PersistTimesheetUpdate(timeEntry, managerID, 4);
            }


            List<EmployeeIndividualViewModel> employeeList = new List<EmployeeIndividualViewModel>();
            employeeList = GetManagerEmployees(GetEmployeeIndividual().IndividualId);

            return RedirectToAction("Index");
        }


        #region Delegate Actions
        //public ActionResult ManagerDelegates()
        //{
        //    var delegateVM = new DelegateViewModel();
        //    delegateVM = GetApprovalDelegate(null);

        //    return PartialView("_DelegateTSApprovalAuthorityPartialView", delegateVM);
        //}

        //[HttpPost]
        //public ActionResult SearchApproverDelegates(string fName, string lName)
        //{
        //    var searchResults = GetIndividaulSearchResultsByName(fName, lName);
        //    return PartialView("_DelegateTSSearchResultsPartialView", searchResults);
        //}

        //[HttpGet]
        //public ActionResult DeleteDelegate(int id, int selectedIndividualID)
        //{
        //    var deleteDelegateList = repository.GetDelegateByDelegateId(id);

        //    if (deleteDelegateList != null)
        //    {
        //        repository.DeleteManagerApprovalDelegate(deleteDelegateList.DelegateToIndividualID, deleteDelegateList.IndividualID);
        //    }
        //    var model = new Models.ViewModel.DelegateViewModel();
        //    model.ApproverDelegates = GetManagersApprovalDelegates(selectedIndividualID);
        //    model.SelectedIndividualID = selectedIndividualID;

        //    return PartialView("_DelegateTSApprovalAuthorityPartialView", model);
        //}

        //[HttpGet]
        //public ActionResult UpsertDelegate(int delegateToId, int managerId, bool isPrimaryDelegate)
        //{
        //    managerId = managerId == 0 ? GetEmployeeIndividual().IndividualId : managerId;

        //    repository.UpsertManagerApprovalDelegate(delegateToId, managerId, isPrimaryDelegate);
        //    return RedirectToAction("Index");
        //}
        #endregion Delegate Actions - END

        #endregion

        #region Helper Methods
        private void SetEmployeeTimesheetProperties(YearAndPayPeriodViewModel yearAndPayPeriod, EmployeeIndividualViewModel employee)
        {
            employee.EmployeeTimesheetEntries = new List<Models.ViewModel.TimesheetHoursViewModel>();
            //List<tblTSTimesheetHour> 
            var startDate = yearAndPayPeriod.CurrentPayPeriod.dtmPeriodStart;
            var endDate = yearAndPayPeriod.CurrentPayPeriod.dtmPeriodEnd;
            employee.EmployeeTimesheetEntries = AutoMapper.Mapper.Map(repository.GetTimeEntriesByDateRangeByEmployeeInfoId(startDate, endDate, employee.EmployeeInfoId), new List<TimesheetHoursViewModel>());
            employee.EmployeeTimesheet = new TimesheetViewModel();
            employee.EmployeeTimesheet = GetTimesheetDefaultValues(employee.EmployeeInformation.IsNonExempt);
            employee.EmployeeTimesheet.EmployeeIndividual = employee;
            employee.EmployeeTimesheet.EmployeeInfoId = employee.EmployeeInfoId;
            employee.EmployeeTimesheet.MasterUserId = employee.MasterUserId;
            employee.EmployeeTimesheet.IsUserNonExempt = employee.EmployeeInformation.IsNonExempt ?? true;
            employee.EmployeeTimesheet.IsUserHRAdmin = employee.IsUserTSHRAdmin;
            employee.EmployeeTimesheet.TimesheetHours = employee.EmployeeTimesheetEntries.OrderByDescending(i => i.StatusTypeID).ThenByDescending(i => i.Date).ToList();
        }

        private TimesheetViewModel GetTimesheetDefaultValues(bool? IsNonExempt)
        {
            var isUserNonExempt = IsNonExempt ?? true;
            var timesheetViewModel = new TimesheetViewModel()
            {
                //If IsNonExempt is null - then apply Non-exempt hour type; although this users are not timesheet users
                
                HoursType = isUserNonExempt ? GetHoursTypeNonExemptList() : GetHoursTypeForExempt(),
                PickerHours = GetPickerHours(),
                PickerMinutes = GetPickerMinutes(),
                PickerHourMinuteInDecimal = GetPickerTimeHourMinuteInDecimal(),
                YearAndPayPeriod = GetYearAndPayPeriodViewModel()
            };
            return timesheetViewModel;
        }

        private EmployeeIndividualViewModel GetManager()
        {
            EmployeeIndividualViewModel managerViewModel = new EmployeeIndividualViewModel();
            managerViewModel = GetEmployeeIndividual();

            if (!managerViewModel.IsUserTSManager)
            {
                if(!managerViewModel.IsUserTSHRAdmin) return null;
            }
            //managerViewModel.ManagerEmployeeList = GetManagerEmployees(managerViewModel.IndividualId);
            //managerViewModel.ManagerDelegateEmployeeList = GetManagerDelegateEmployees(managerViewModel.IndividualId);
            //managerViewModel.ManagerDelegatedToEmployeeList = GetManagerDelegatedToEmployees(managerViewModel.IndividualId);

            YearAndPayPeriodViewModel yearAndPayPeriod = GetYearAndPayPeriodViewModel();

            //Set Managers Timesheet Properties
            SetEmployeeTimesheetProperties(yearAndPayPeriod, managerViewModel);

            //SetTimesheetPropsForEmpList(managerViewModel.ManagerEmployeeList);

            return managerViewModel;
        }

        private List<IndividualDelegateViewModel> GetApproverDelegates(int managerID)
        {
            List<IndividualDelegateViewModel> delegateEmployeeList = new List<IndividualDelegateViewModel>();
            delegateEmployeeList = GetManagersApprovalDelegates(managerID);

            YearAndPayPeriodViewModel yearAndPayPeriod = GetYearAndPayPeriodViewModel();

            //Set Manager's Employees Timesheet Properties
            foreach (var employee in delegateEmployeeList)
            {
                SetEmployeeTimesheetProperties(yearAndPayPeriod, employee.DelegateToIndividual);
            }
            return delegateEmployeeList;
        }

        private void ApproveTimesheets(EmployeeIndividualViewModel managerViewModel, int managerID, int empInfoID, int timeSheetHoursID, int delegateID)
        {
            int currentEmpInfoID;
            var empList = new List<EmployeeIndividualViewModel>();
            if (delegateID > 0)
            {
                empList = GetManagerDelegatedToEmployees(delegateID);
            }
            else
            {
                if (managerViewModel.ManagerEmployeeList == null || managerViewModel.ManagerEmployeeList.Count == 0)
                {
                    empList = managerViewModel.ManagerEmployeeList = GetManagersEmployeeIndividuals(managerID);
                }
            }

            SetTimesheetPropsForEmpList(empList);

            foreach (var item in empList)
            {
                currentEmpInfoID = empInfoID == 0 ? item.EmployeeInfoId : empInfoID;
                if (item.EmployeeInfoId == currentEmpInfoID)
                {
                    if(timeSheetHoursID == 0)
                    {
                        foreach (var timeSheetItem in item.EmployeeTimesheet.TimesheetHours)
                        {
                            ApproveSingleEmployeeTimesheets(timeSheetItem.TimesheetHoursID, managerID, currentEmpInfoID);
                        }
                    }
                    else
                    {
                        ApproveSingleEmployeeTimesheets(timeSheetHoursID, managerID, currentEmpInfoID);
                    }
                }
            }
        }

        private void ApproveSingleEmployeeTimesheets(int timesheetID, int managerID, int currentEmpInfoID)
        {
            var timeEntry = repository.GetTimesheetHourById(timesheetID);
            if (timeEntry != null && timeEntry.StatusTypeId == 3 && timeEntry.EmployeeInfoId == currentEmpInfoID)
            {
                PersistTimesheetUpdate(timeEntry, managerID, 2);
            }
        }

        private List<EmployeeIndividualViewModel> GetManagerEmployees(int managerID)
        {
            var employeeList = new List<EmployeeIndividualViewModel>();
            employeeList = GetManagersEmployeeIndividuals(managerID);

            SetTimesheetPropsForEmpList(employeeList);

            return employeeList;
        }

        private List<EmployeeIndividualViewModel> GetDelegatedToEmployees(int managerID)
        {
            var employeeList = new List<EmployeeIndividualViewModel>();
            employeeList = GetManagerDelegatedToEmployees(managerID); ;

            SetTimesheetPropsForEmpList(employeeList);

            return employeeList;
        }

        private void SetTimesheetPropsForEmpList(List<EmployeeIndividualViewModel> employeeList)
        {
            YearAndPayPeriodViewModel yearAndPayPeriod = GetYearAndPayPeriodViewModel();

            //Set Manager's Employees Timesheet Properties
            foreach (var employee in employeeList)
            {
                SetEmployeeTimesheetProperties(yearAndPayPeriod, employee);
            }
        }


        private void PersistTimesheetUpdate(Lib.DataLibrary.DataAccess.United.tblTSTimesheetHour timeEntry,  int managerID, int statusTypeID)
        {
            timeEntry.StatusTypeId = statusTypeID;
            timeEntry.SubmitUser = managerID;
            timeEntry.SubmitDate = DateTime.Now;
            repository.UpsertSingleDateEntry(timeEntry);
        }

        #endregion

    }

} 