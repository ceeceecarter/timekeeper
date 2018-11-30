using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Net;
using NM.Web.WebApplication.Timesheets.Infrastructure.Enums;
using NM.Web.WebApplication.Timesheets.Models.ViewModel;
using NM.Web.WebApplication.Timesheets.Filters;
using NM.Lib.DataLibrary.United.Interface;
using NM.Lib.DataLibrary.DataAccess.United;
using System.Configuration;

namespace NM.Web.WebApplication.Timesheets.Controllers
{
    //[CentralApiAuthorization]
    public class TimesheetsController : BaseController
    {

        public TimesheetsController(ITimesheet repo)
        {
            repository = repo;
        }

        #region Actions
        // GET: Timesheet
        public ActionResult Index()
        {
            var employeeIndividual = GetEmployeeIndividual();
            TimesheetViewModel model = new TimesheetViewModel();

            if (employeeIndividual.FLSAStatus != -1)
            {

                model = GetEmployeeTimesheetInitialInfo(employeeIndividual); //TODO: need to refactor this method

                //Populate the logged in user information
                model.LoggedInUser.AuthenticatedUser = AuthenticateUser;
                model.LoggedInUser.EmployeeInfoID = employeeIndividual.EmployeeInfoId;
                model.LoggedInUser.JobTitle = employeeIndividual.JobTitle;
                model.LoggedInUser.IsUserDelegate = employeeIndividual.IsUserDelegate;
                model.LoggedInUser.IsUserTSManager = employeeIndividual.IsUserTSManager;
                model.LoggedInUser.IsUserTSHRAdmin = employeeIndividual.IsUserTSHRAdmin;
                model.LoggedInUser.IsUserTSUser = employeeIndividual.IsUserTSUser;
                model.LoggedInUser.IndividualID = employeeIndividual.IndividualId;

                //initial load use the login user as the selected employee-individual
                model.SelectedEmployeeIndividual = employeeIndividual;
                //initial load use current year
                var selectedYearPayPeriodList = GetPayPeriodsList(model.SelectedPayPeriodYear).OrderByDescending(i => i.dtmPeriodEnd).ToList();
                model.SelectedYearPayPeriods = GetPayPeriodsListItems(selectedYearPayPeriodList);

                var individualId = employeeIndividual.IndividualId;
                List<EmployeeIndividualViewModel> managerDelegateEmployeeList = new List<EmployeeIndividualViewModel>();
                var directEmployeesOfIndividual = GetDirectEmployees(individualId);
                var delegatedEmployeesOfIndividual = GetEmployeesDelegatedToIndividual(individualId);

                //get the current open or next pay period to be process
                var currentpayperiod = repository.GetCurrentNextPreviousPayPeriod().Where(i => i.TimePeriod == Enumeration.TimePayPeriod.Current.ToString()).FirstOrDefault();
                var payperiod = currentpayperiod != null ? AutoMapper.Mapper.Map(currentpayperiod, new PayPeriodViewModel()) : selectedYearPayPeriodList.FirstOrDefault();

                var ytdDateStart = new DateTime();
                var ytdDateEnd = new DateTime();
                var startDate = DateTime.Now;
                repository.DatesYTD(startDate, out ytdDateStart, out ytdDateEnd);
                var textNonSubmitted = repository.GetStatusType().FirstOrDefault(i => i.StatusTypeID == (int)Enumeration.StatusType.NonSubmitted).StatusType;

                //TODO: Refactor
                //Get Delegated employees if the login user is a delegate
                if (employeeIndividual.IsUserDelegate && delegatedEmployeesOfIndividual.Count() > 0)
                {
                    foreach (var item in delegatedEmployeesOfIndividual)
                    {
                        if (item.EmployeeStatus != (int)Enumeration.EmployeeStatus.Terminated)
                        {
                            var tsStatus = string.Empty;
                            if (item.EmployeeInformation.IsNonExempt ?? true)
                            {
                                var timeEntries = GetEmployeeTimeEntriesByDateRange(payperiod.dtmPeriodStart, payperiod.dtmPeriodEnd, item.EmployeeInfoId, item.EmployeeInformation.IsNonExempt ?? true).Where(i => i.StatusTypeID > (int)Enumeration.StatusType.Processed && i.PayPeriodID == currentpayperiod.PayPeriodID).Take(5);
                                if (!string.IsNullOrEmpty(item.CompanyCode))
                                {
                                    tsStatus = timeEntries != null && timeEntries.Count() > 0 ? timeEntries.OrderByDescending(i => i.StatusTypeID).FirstOrDefault().tblTSStatusType.StatusType : textNonSubmitted;
                                    item.TimesheetStatus = tsStatus;
                                    var result = item.FileNumber != null ? GetPTOBalances(item.FileNumber.Trim(), item.CompanyCode) : null;
                                    item.PTOBalance = result != null ? (decimal)result.Balance : 0.00M;
                                }
                            }
                            else//Exempt
                            {
                                var timeEntries = GetEmployeeTimeEntriesByDateRange(ytdDateStart, ytdDateEnd, item.EmployeeInfoId,
                                    item.EmployeeInformation.IsNonExempt ?? true).Where(i => i.StatusTypeID == (int)Enumeration.StatusType.Submitted);
                                var groupbyStatus = timeEntries.GroupBy(i => i.StatusTypeID).Select(i => new { statusType = i.Key, TotalHours = i.Sum(y => y.Hours) });
                                foreach (var entry in groupbyStatus)
                                {
                                    if (entry.statusType == (int)Enumeration.StatusType.Submitted)
                                    {
                                        tsStatus = string.Format("{0} {1}", entry.TotalHours, "Hours Pending Approval");
                                        break;
                                    }
                                }
                                tsStatus = groupbyStatus != null && groupbyStatus.Count() > 0 ? tsStatus : "No Pending Time Off";
                                item.TimesheetStatus = tsStatus;
                                var result = item.FileNumber != null ? GetPTOBalances(item.FileNumber.Trim(), item.CompanyCode) : null;
                                item.PTOBalance = result != null ? (decimal)result.Balance : 0.00M;
                            }
                        }
                        else
                        {
                            item.TimesheetStatus = "Terminated";
                        }
                    }
                    model.DelegatedEmployees = FilterTerminatedEmployeesOverThreeWeeks(delegatedEmployeesOfIndividual);
                }
                //Get Direct employees if the login user it has direct employees
                if (directEmployeesOfIndividual.Count() > 0)
                {
                    foreach (var item in directEmployeesOfIndividual)
                    {
                        if (item.EmployeeStatus != (int)Enumeration.EmployeeStatus.Terminated)
                        {
                            var tsStatus = string.Empty;
                            if (item.EmployeeInformation.IsNonExempt ?? true)
                            {
                                var timeEntries = GetEmployeeTimeEntriesByDateRange(ytdDateStart, ytdDateEnd, item.EmployeeInfoId, item.EmployeeInformation.IsNonExempt ?? true).Where(i => i.StatusTypeID > (int)Enumeration.StatusType.Processed && i.PayPeriodID == currentpayperiod.PayPeriodID).Take(5);
                                if (!string.IsNullOrEmpty(item.CompanyCode))
                                {
                                    tsStatus = timeEntries != null && timeEntries.Count() > 0 ? timeEntries.OrderByDescending(i => i.StatusTypeID).FirstOrDefault().tblTSStatusType.StatusType : textNonSubmitted;
                                    item.TimesheetStatus = tsStatus;
                                    var result = item.FileNumber != null ? GetPTOBalances(item.FileNumber.Trim(), item.CompanyCode) : null;
                                    item.PTOBalance = result != null ? (decimal)result.Balance : 0.00M;
                                }
                            }
                            else//Exempt
                            {
                                var timeEntries = GetEmployeeTimeEntriesByDateRange(ytdDateStart, ytdDateEnd, item.EmployeeInfoId, item.EmployeeInformation.IsNonExempt ?? true).Where(i => i.StatusTypeID == (int)Enumeration.StatusType.Submitted);
                                var groupbyStatus = timeEntries.GroupBy(i => i.StatusTypeID).Select(i => new { statusType = i.Key, TotalHours = i.Sum(y => y.Hours) });
                                foreach (var entry in groupbyStatus)
                                {
                                    if (entry.statusType == (int)Enumeration.StatusType.Submitted)
                                    {
                                        tsStatus = string.Format("{0} {1}", entry.TotalHours, "Hours Pending Approval");
                                        break;
                                    }
                                }
                                tsStatus = groupbyStatus != null && groupbyStatus.Count() > 0 ? tsStatus : "No Pending Time Off";
                                item.TimesheetStatus = tsStatus;
                                var result = item.FileNumber != null ? GetPTOBalances(item.FileNumber.Trim(), item.CompanyCode) : null;
                                item.PTOBalance = result != null ? (decimal)result.Balance : 0.00M;
                            }
                        }
                        else
                        {
                            item.TimesheetStatus = "Terminated";
                        }
                    }

                    model.DirectEmployees = FilterTerminatedEmployeesOverThreeWeeks(directEmployeesOfIndividual);
                }
            }
            else
            {
                //Login user is not timekeeper user, FLSAStatus is NonApplicable
                model.EmployeeIndividual = new EmployeeIndividualViewModel() { FLSAStatus = employeeIndividual.FLSAStatus };
            }


            return View("TimesheetView", model);
        }

        [HttpPost]
        [ValidateModelState]
        //[ValidateAntiForgeryToken]
        public ActionResult PostAddTime(TimesheetViewModel model)
        {
            if (model == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (ModelState.IsValid)
            {
                //Get all dates based on DateTimeStart and DateTimeEnd
                var timeDateRange = AutoMapper.Mapper.Map(repository.GetTimeDateRange(model.tbDateRangeStart, model.tbDateRangeEnd), new List<TSTimeEntryViewModel>());
                List<Models.ViewModel.TimesheetHoursViewModel> currentHours = new List<Models.ViewModel.TimesheetHoursViewModel>();
                List<Models.ViewModel.TimesheetHoursViewModel> tsHours = new List<Models.ViewModel.TimesheetHoursViewModel>();
                // currentHours = AutoMapper.Mapper.Map(repository.GetTimeEntriesByEmployeeInfoId(model.EmployeeInfoId), new List<TimesheetHoursViewModel>());


                if (model.cbDateRange) //DateRange
                {
                    //find all selected days when DateRange is checked
                    //then reassign values to timeDateRange variable to override values
                    var selectedDaysInDateRange = new List<TSTimeEntryViewModel>();
                    foreach (var item in timeDateRange)
                    {
                        bool flagItemToAdd = false;
                        if (model.cbSunday && item.strDayOfWeek == DayOfWeek.Sunday.ToString()) flagItemToAdd = true;
                        if (model.cbMonday && item.strDayOfWeek == DayOfWeek.Monday.ToString()) flagItemToAdd = true;
                        if (model.cbTuesday && item.strDayOfWeek == DayOfWeek.Tuesday.ToString()) flagItemToAdd = true;
                        if (model.cbWednesday && item.strDayOfWeek == DayOfWeek.Wednesday.ToString()) flagItemToAdd = true;
                        if (model.cbThursday && item.strDayOfWeek == DayOfWeek.Thursday.ToString()) flagItemToAdd = true;
                        if (model.cbFriday && item.strDayOfWeek == DayOfWeek.Friday.ToString()) flagItemToAdd = true;
                        if (model.cbSaturday && item.strDayOfWeek == DayOfWeek.Saturday.ToString()) flagItemToAdd = true;
                        if (flagItemToAdd) selectedDaysInDateRange.Add(item);
                    }
                    if (selectedDaysInDateRange.Count() > 0)
                    {
                        timeDateRange = selectedDaysInDateRange;
                    }
                }
                if (model.SelectedHoursType != (int)Enumeration.HoursType.Regular)
                {
                    //Non-Regular Hours i.e. PTO, Volunteer PTO, etc
                    tsHours = GetTimesheetForNonRegularHours(model, timeDateRange);
                }
                else
                {
                    //Regular Hours
                    tsHours = GetTimesheetForRegularHours(model, timeDateRange);
                }

                try
                {
                    foreach (var item in tsHours)
                    {
                        repository.UpsertSingleDateEntry(AutoMapper.Mapper.Map(item, new tblTSTimesheetHour()));
                    }
                    //repository.SaveChanges();

                    //string url = Url.Action("Index", "Timesheets", new { id = model.EmployeeInfoId });
                    //return Json(new { success = true, url = url });
                    var individualId = repository.GetEmployeeInformationById(model.EmployeeInfoId).IndividualID;
                    var payperiod = AutoMapper.Mapper.Map(repository.GetPayPeriodByPayPeriodId(model.SelectedPayPeriodID), new PayPeriodViewModel());
                    var payperiodYear = payperiod.dtmPeriodEnd.Year.ToString(); // model.IsUserNonExempt ? payperiod.dtmPeriodEnd.Year.ToString() : "";
                    var payPeriodId = payperiod.PayPeriodID; // model.IsUserNonExempt ? payperiod.PayPeriodID : 0;

                    return RedirectToAction("GetRefresh_TSWeeklyView", new
                    {
                        individualId = individualId,
                        selectedYear = payperiodYear,
                        selectedPayPeriodId = payPeriodId
                    });
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            //return View("_TSWeeklyView", model);
            return RedirectToAction("Index");
        }


        public ActionResult PostAddMileage(TSMileageViewModel mileageModel)
        {
            TimesheetViewModel tsModel = new TimesheetViewModel();
            if (mileageModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            if (ModelState.IsValid)
            {
                //Get all dates based on DateTimeStart and DateTimeEnd
                //var timeDateRange = AutoMapper.Mapper.Map(repository.GetTimeDateRange(model.tbDateRangeStart, model.tbDateRangeEnd), new List<TSMileageViewModel>());
                //timeDate = model.MileageDate;
                //List<Models.ViewModel.TimesheetHoursViewModel> tsMileage = new List<Models.ViewModel.TimesheetHoursViewModel>();
                // List<Models.ViewModel.TSMileageViewModel> tsMileage = new List<Models.ViewModel.TSMileageViewModel>();

                //tsMileage = GetTimesheetForMileage(model,);
                mileageModel.MileageMiles = (float)Math.Round(mileageModel.MileageMiles, 1, MidpointRounding.AwayFromZero);
                mileageModel.EntryDate = DateTime.Now.Date;
                mileageModel.PayPeriodID = mileageModel.SelectedPayPeriodID;
                mileageModel.Hours = 0;
                mileageModel.HoursTypeID = (int)Enumeration.HoursType.Mileage;
                mileageModel.LockOutAll = false;
                mileageModel.LockOutEmployee = false;
                mileageModel.LockOutManager = false;
                mileageModel.StatusTypeID = (int)Enumeration.StatusType.NonSubmitted;

                try
                {
                    //foreach (var item in tsMileage)
                    {
                        repository.UpsertSingleDateEntry(AutoMapper.Mapper.Map(mileageModel, new tblTSTimesheetHour()));
                    }
                    //var employeeIndividual = GetEmployeeIndividual();
                    //tsModel = GetEmployeeTimesheetInitialInfo(employeeIndividual);
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                //string url = Url.Action("Index", "Timesheets", new { id = tsModel.EmployeeInfoId });
                var individualId = repository.GetEmployeeInformationById(mileageModel.EmployeeInfoID).IndividualID;
                var payperiod = AutoMapper.Mapper.Map(repository.GetPayPeriodByPayPeriodId(mileageModel.SelectedPayPeriodID), new PayPeriodViewModel());

                return RedirectToAction("GetRefresh_TSWeeklyView", new
                {
                    selectedPayPeriodId = mileageModel.SelectedPayPeriodID,
                    employeeInfoId = mileageModel.EmployeeInfoID,
                    individualId = individualId
                });
            }
            //return View("_TSWeeklyView", tsModel);
            return RedirectToAction("Index");
        }

        public ActionResult PostAddOnCall(TSOnCallViewModel onCallModel)
        {
            TimesheetViewModel tsModel = new TimesheetViewModel();
            if (onCallModel == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            if (ModelState.IsValid)
            {
                onCallModel.EntryDate = DateTime.Now.Date;
                onCallModel.PayPeriodID = onCallModel.SelectedPayPeriodID;
                onCallModel.LockOutAll = false;
                onCallModel.LockOutEmployee = false;
                onCallModel.LockOutManager = false;
                onCallModel.StatusTypeID = (int)Enumeration.StatusType.NonSubmitted;
                onCallModel.HoursTypeID = onCallModel.SelectedHoursType;

                try
                {
                    repository.UpsertSingleDateEntry(AutoMapper.Mapper.Map(onCallModel, new tblTSTimesheetHour()));

                    //var employeeIndividual = GetEmployeeIndividual();
                    //tsModel = GetEmployeeTimesheetInitialInfo(employeeIndividual);
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                //string url = Url.Action("Index", "TimeSheets", new { id = tsModel.EmployeeInfoId });
                var individualId = repository.GetEmployeeInformationById(onCallModel.EmployeeInfoID).IndividualID;
                var payperiod = AutoMapper.Mapper.Map(repository.GetPayPeriodByPayPeriodId(onCallModel.SelectedPayPeriodID), new PayPeriodViewModel());
                return RedirectToAction("GetRefresh_TSWeeklyView", new
                {
                    individualId = individualId,
                    selectedYear = payperiod.dtmPeriodEnd.Year.ToString(),
                    selectedPayPeriodId = payperiod.PayPeriodID
                });

            }
            //return View("_TSWeeklyView", tsModel);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateModelState]
        public ActionResult EditSave(TimesheetViewModel model)
        {
            //Fields that can be updated for Non-exempt and Exempt
            //Hours Type, Date, Hours, TimeStart and TimeEnd

            if (ModelState.IsValid)
            {
                var timeEntry = repository.GetTimesheetHourById(model.EditTimesheetHourId);
                if (timeEntry != null)
                {
                    if (model.SelectedHoursType == (int)Enumeration.HoursType.Regular)
                    {
                        timeEntry.Hours = GetTotalNumberOfHours(model.EditSelectedHourStart,
                                                            model.EditSelectedHourEnd,
                                                            model.EditSelectedMinuteStart,
                                                            model.EditSelectedMinuteEnd);
                        timeEntry.TimeStart = CalculateTimeHourMinute(model.EditSelectedHourStart, model.EditSelectedMinuteStart);
                        timeEntry.TimeEnd = CalculateTimeHourMinute(model.EditSelectedHourEnd, model.EditSelectedMinuteEnd);
                    }
                    else if (model.SelectedHoursType == (int)Enumeration.HoursType.Mileage)
                    {
                        timeEntry.MileageMiles = (float)Math.Round(model.MileageMiles, 1, MidpointRounding.AwayFromZero);
                        timeEntry.Date = model.MileageDate;
                        timeEntry.MileageDescription = model.MileageDescription;
                    }
                    else if (model.SelectedHoursType == (int)Enumeration.HoursType.OnCallHoliday || model.SelectedHoursType == (int)Enumeration.HoursType.OnCallRegular)
                    {
                        timeEntry.Date = model.EditSelectedDateEntry;
                        timeEntry.HoursTypeID = model.SelectedHoursType;
                    }
                    else
                    {
                        timeEntry.Hours = model.EditSelectedHourMinuteInDecimal; //TODO: fix issue here: values from view to controller were incorrect
                        timeEntry.TimeStart = 0;
                        timeEntry.TimeEnd = 0;
                    }
                    timeEntry.Date = model.EditSelectedDateEntry;
                    timeEntry.StatusTypeId = (int)Enumeration.StatusType.NonSubmitted;
                    timeEntry.HoursTypeID = model.SelectedHoursType;
                    repository.UpsertSingleDateEntry(timeEntry);
                    var employeeInfo = repository.GetEmployeeInformationById(timeEntry.EmployeeInfoId);
                    if (employeeInfo.IsNonExempt ?? true)
                    {
                        repository.UpdateStatusForMultipleDateEntries(timeEntry.EmployeeInfoId, timeEntry.PayPeriodID, (int)Enumeration.StatusType.NonSubmitted);
                    }
                }
            }
            //var employeeIndividual = GetEmployeeIndividual(); //_employee; 
            //var tsmodel = GetEmployeeTimesheetInitialInfo(employeeIndividual);
            //return PartialView("_TSWeeklyView", tsmodel);
            var payperiodYear = model.SelectedPayPeriodYear.ToString(); // model.IsUserNonExempt ? model.SelectedPayPeriodYear.ToString() : "";
            var payperiodId = model.SelectedPayPeriodID; // model.IsUserNonExempt ? model.SelectedPayPeriodID : 0;
            return RedirectToAction("GetRefresh_TSWeeklyView", new
            {
                individualId = model.EmployeeIndividual.IndividualId,
                selectedYear = payperiodYear,
                selectedPayPeriodId = payperiodId
            });
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            string url = Url.Action("Index", "Timesheets", new { id = id });
            var timeEntry = repository.GetTimesheetHourById(id);
            int hoursType = timeEntry.HoursTypeID;
            List<int> timeEntryType = new List<int>(new int[] { 1, 2, 3, 4, 5, 6, 9, 10 });
            List<int> mileageEntryType = new List<int>(new int[] { 8 });
            List<int> onCallEntryType = new List<int>(new int[] { 7, 11, 12 });

            var payperiod = AutoMapper.Mapper.Map(repository.GetPayPeriodByPayPeriodId(timeEntry.PayPeriodID), new PayPeriodViewModel());
            var payperiodInCurrentYearListItems = GetPayPeriodsListItems(GetPayPeriodsList(payperiod.dtmPeriodEnd.Year));

            if (timeEntry != null && timeEntryType.Contains(hoursType))
            {
                var employeeInformationDto = repository.GetEmployeeInformationById(timeEntry.EmployeeInfoId);
                var timesheetHoursForDateEntered = new List<TimesheetHoursViewModel>();
                var tsEntry = AutoMapper.Mapper.Map(timeEntry, new TimesheetHoursViewModel());
                timesheetHoursForDateEntered.Add(tsEntry);
                var model = GetTimesheetDefaultValues((bool)employeeInformationDto.IsNonExempt);
                model.EmployeeIndividual = new EmployeeIndividualViewModel()
                {
                    EmployeeInformation = AutoMapper.Mapper.Map(employeeInformationDto, new EmployeeInformationViewModel()),
                    IndividualId = employeeInformationDto.IndividualID,
                    FLSAStatus = (bool)employeeInformationDto.IsNonExempt ? 1 : employeeInformationDto.IsNonExempt == false ? 0 : -1
                };
                model.TimesheetHours = timesheetHoursForDateEntered;
                model.EditSelectedHourStart = GetSelectedHoursOrMinutes(tsEntry.TimeStart, true);
                model.EditSelectedMinuteStart = GetSelectedHoursOrMinutes(tsEntry.TimeStart, false);
                model.EditSelectedHourEnd = GetSelectedHoursOrMinutes(tsEntry.TimeEnd, true);
                model.EditSelectedMinuteEnd = GetSelectedHoursOrMinutes(tsEntry.TimeEnd, false);
                model.EditSelectedHourMinuteInDecimal = tsEntry.Hours;
                model.EditTimesheetHourId = timesheetHoursForDateEntered.FirstOrDefault().TimesheetHoursID;
                model.EmployeeInfoId = employeeInformationDto.EmployeeInfoId;
                model.MasterUserId = employeeInformationDto.MasterUserID;
                model.IsUserNonExempt = employeeInformationDto.IsNonExempt ?? true;
                model.SelectedPayPeriod = payperiod;
                model.SelectedPayPeriodID = payperiod.PayPeriodID;
                model.SelectedPayPeriodYear = payperiod.dtmPeriodEnd.Year;
                model.SelectedYearPayPeriods = payperiodInCurrentYearListItems;
                return PartialView("_TSHourDataEdit", model);
            }

            if (timeEntry != null && mileageEntryType.Contains(hoursType))
            {
                var employeeInformationDto = repository.GetEmployeeInformationById(timeEntry.EmployeeInfoId);
                var timesheets = new List<TimesheetHoursViewModel>();
                var tsEntry = AutoMapper.Mapper.Map(timeEntry, new TimesheetHoursViewModel());
                timesheets.Add(tsEntry);
                var model = GetTimesheetDefaultValues((bool)employeeInformationDto.IsNonExempt);
                model.EmployeeIndividual = new EmployeeIndividualViewModel()
                {
                    EmployeeInformation = AutoMapper.Mapper.Map(employeeInformationDto, new EmployeeInformationViewModel()),
                    IndividualId = employeeInformationDto.IndividualID,
                    FLSAStatus = (bool)employeeInformationDto.IsNonExempt ? 1 : (bool)employeeInformationDto.IsNonExempt == false ? 0 : -1
                };
                model.TimesheetHours = timesheets;
                model.MileageDescription = tsEntry.MileageDescription;
                model.MileageDate = tsEntry.Date;
                model.MileageMiles = (float)tsEntry.MileageMiles;
                model.EditTimesheetHourId = timesheets.FirstOrDefault().TimesheetHoursID;
                model.EmployeeInfoId = employeeInformationDto.EmployeeInfoId;
                model.MasterUserId = employeeInformationDto.MasterUserID;
                model.IsUserNonExempt = employeeInformationDto.IsNonExempt ?? true;
                model.SelectedPayPeriod = payperiod;
                model.SelectedPayPeriodID = payperiod.PayPeriodID;
                model.SelectedPayPeriodYear = payperiod.dtmPeriodEnd.Year;
                model.SelectedYearPayPeriods = payperiodInCurrentYearListItems;
                return PartialView("_TSMileageDataEdit", model);
            }

            if (timeEntry != null && onCallEntryType.Contains(hoursType))
            {
                var employeeInformationDto = repository.GetEmployeeInformationById(timeEntry.EmployeeInfoId);
                var timesheets = new List<TimesheetHoursViewModel>();
                var tsEntry = AutoMapper.Mapper.Map(timeEntry, new TimesheetHoursViewModel());
                timesheets.Add(tsEntry);
                var model = GetTimesheetDefaultValues((bool)employeeInformationDto.IsNonExempt);
                model.EmployeeIndividual = new EmployeeIndividualViewModel()
                {
                    EmployeeInformation = AutoMapper.Mapper.Map(employeeInformationDto, new EmployeeInformationViewModel()),
                    IndividualId = employeeInformationDto.IndividualID,
                    FLSAStatus = (bool)employeeInformationDto.IsNonExempt ? 1 : (bool)employeeInformationDto.IsNonExempt == false ? 0 : -1
                };
                model.TimesheetHours = timesheets; //time entries for the same date                
                model.HoursTypeOnCall = GetHoursTypeForOnCall();
                model.EditTimesheetHourId = timesheets.FirstOrDefault().TimesheetHoursID;
                model.EmployeeInfoId = employeeInformationDto.EmployeeInfoId;
                model.MasterUserId = employeeInformationDto.MasterUserID;
                model.IsUserNonExempt = employeeInformationDto.IsNonExempt ?? true;
                model.SelectedPayPeriod = payperiod;
                model.SelectedPayPeriodID = payperiod.PayPeriodID;
                model.SelectedPayPeriodYear = payperiod.dtmPeriodEnd.Year;
                model.SelectedYearPayPeriods = payperiodInCurrentYearListItems;
                return PartialView("_TSOnCallDataEdit", model);
            }

            return Json(new { success = true, url = url });
        }

        [HttpGet]
        public ActionResult ConfirmDelete(int id)
        {
            var timeEntry = AutoMapper.Mapper.Map(repository.GetTimesheetHourById(id), new TimesheetHoursViewModel());
            SelectedTimeEntryViewModel selTimeEntry = new SelectedTimeEntryViewModel();
            selTimeEntry.EmployeeInfoId = timeEntry.EmployeeInfoID.ToString();
            selTimeEntry.SelectedPayPeriodId = timeEntry.PayPeriodID.ToString();
            selTimeEntry.Values.Add(timeEntry.TimesheetHoursID.ToString());

            var model = GetSelectedTimeHoursEntries(selTimeEntry);
            return PartialView("_TSHourConfirmDeletingTimeEntries", model);
        }

        //public ActionResult ConfirmMileageDelete(int id)
        //{
        //    var model = AutoMapper.Mapper.Map(repository.GetTimesheetHourById(id), new TSMileageViewModel());
        //    return PartialView("_TSHourConfirmDeletingMileageEntry", model);
        //}

        [HttpPost]
        public ActionResult DeleteTimeEntry(int id)
        {
            string url = Url.Action("Index", "Timesheets", new { id = id });
            var timeEntry = repository.GetTimesheetHourById(id);
            if (timeEntry != null)
            {
                repository.DeleteTimeEntry(timeEntry);


                //Refresh _TSWeeklyView partial view
                var individualId = repository.GetEmployeeInformationById(timeEntry.EmployeeInfoId).IndividualID;
                var payperiod = AutoMapper.Mapper.Map(repository.GetPayPeriodByPayPeriodId(timeEntry.PayPeriodID), new PayPeriodViewModel());

                return RedirectToAction("GetRefresh_TSWeeklyView", new { individualId = individualId, selectedYear = payperiod.dtmPeriodEnd.Year.ToString(), selectedPayPeriodId = payperiod.PayPeriodID });

                //var employeeIndividual = GetEmployeeIndividual(); //_employee;
                //var tsmodel = GetEmployeeTimesheetInitialInfo(employeeIndividual);

                //return PartialView("_TSWeeklyView", tsmodel);
            }
            return Json(new { success = true, url = url });
        }

        //public ActionResult DeleteMileageEntry(int id)
        //{
        //    string url = Url.Action("Index", "Timesheets", new { id = id });
        //    var mileageEntry = repository.GetTimesheetHourById(id);
        //    if (mileageEntry != null)
        //    {
        //        repository.DeleteTimeEntry(mileageEntry);

        //        var employeeIndividual = GetEmployeeIndividual();
        //        var tsModel = GetEmployeeTimesheetInitialInfo(employeeIndividual);

        //        return PartialView("_TSMileageView", tsModel);
        //    }
        //    return Json(new { success = true, url = url });
        //}


        public ActionResult ConfirmBeforeSubmittingApproval(SelectedTimeEntryViewModel SelectedTimeEntries)
        {
            var model = new TimesheetViewModel();

            if (SelectedTimeEntries != null)
            {
                var employeeInformation = repository.GetEmployeeInformationById(int.Parse(SelectedTimeEntries.EmployeeInfoId));
                model = GetTimesheetsForIndividual(employeeInformation.IndividualID, "", int.Parse(SelectedTimeEntries.SelectedPayPeriodId));
                model.ManagerOfIndividual = GetIndividual(employeeInformation.tblIndividual.ManagerIndividualID);
                var listOfDelegates = GetDelegatesToIndividual(employeeInformation.IndividualID);
                var selTimeEntries = GetSelectedTimeHoursEntries(SelectedTimeEntries);
                var employeeIndividual = GetEmployeeIndividual();
                model.MasterUserId = employeeIndividual.MasterUserId; //approver masteruserid
                model.LoggedInUser.IsUserTSHRAdmin = employeeIndividual.IsUserTSHRAdmin;
                //model.LoggedInUser.EmployeeInfoID = employeeIndividual.EmployeeInfoId;
                //model.LoggedInUser.IsUserDelegate = employeeIndividual.IsUserDelegate;
                //model.LoggedInUser.IsUserTSUser = employeeIndividual.IsUserTSUser;
                //model.LoggedInUser.IndividualID = employeeIndividual.IndividualId;

                if (!model.IsUserNonExempt) { model.TimeOffSummaryExempt = GetNonRegularHoursTypeSummary(selTimeEntries); }
                model.TimesheetHours = selTimeEntries;
                model.IndividualDelegates = listOfDelegates.ToList();
            }
            return PartialView("_TSHourSubmitApproval", model);
        }

        public ActionResult ConfirmBeforeApproving(SelectedTimeEntryViewModel SelectedTimeEntries, int ApproverEmployeeInfoID)
        {
            var employeeInformation = repository.GetEmployeeInformationById(int.Parse(SelectedTimeEntries.EmployeeInfoId));
            var model = GetTimesheetsForIndividual(employeeInformation.IndividualID, "", int.Parse(SelectedTimeEntries.SelectedPayPeriodId));
            var approverEmployeeInfo = repository.GetEmployeeInformationById(ApproverEmployeeInfoID);
            var approverEmployeeIndividual = GetEmployeeIndividual(approverEmployeeInfo.MasterUserID);
            model.MasterUserId = approverEmployeeInfo.MasterUserID;
            model.EmployeeIndividual.MasterUserId = employeeInformation.MasterUserID;
            model.LoggedInUser.EmployeeInfoID = approverEmployeeIndividual.EmployeeInfoId;
            model.LoggedInUser.IsUserDelegate = approverEmployeeIndividual.IsUserDelegate;
            model.LoggedInUser.IsUserTSHRAdmin = approverEmployeeIndividual.IsUserTSHRAdmin;
            model.LoggedInUser.IsUserTSUser = approverEmployeeIndividual.IsUserTSUser;
            model.LoggedInUser.IndividualID = approverEmployeeIndividual.IndividualId;
            var listOfDelegates = GetDelegatesToIndividual(employeeInformation.IndividualID);

            if (SelectedTimeEntries != null)
            {
                var selTimeEntries = GetSelectedTimeHoursEntries(SelectedTimeEntries);
                //model.HoursTypeSummary = GetHoursTypeSummary(selTimeEntries);
                if (!employeeInformation.IsNonExempt ?? true)
                {
                    model.TimeOffSummaryExempt = SelectedTimeEntriesSummaryExempt(model.TimeOffSummaryExempt, selTimeEntries);
                }
                model.TimesheetHours = selTimeEntries;
                model.IndividualDelegates = listOfDelegates.ToList();
            }
            return PartialView("_TSHourApprove", model);
        }

        public ActionResult ConfirmBeforeRejecting(SelectedTimeEntryViewModel SelectedTimeEntries, int ApproverEmployeeInfoID)
        {

            var employeeInformation = repository.GetEmployeeInformationById(int.Parse(SelectedTimeEntries.EmployeeInfoId));
            var model = GetTimesheetsForIndividual(employeeInformation.IndividualID, "", int.Parse(SelectedTimeEntries.SelectedPayPeriodId));
            var approverEmployeeInfo = repository.GetEmployeeInformationById(ApproverEmployeeInfoID);
            var approverEmployeeIndividual = GetEmployeeIndividual(approverEmployeeInfo.MasterUserID);

            model.MasterUserId = approverEmployeeInfo.MasterUserID;
            model.EmployeeIndividual.MasterUserId = employeeInformation.MasterUserID;
            model.LoggedInUser.EmployeeInfoID = approverEmployeeIndividual.EmployeeInfoId;
            model.LoggedInUser.IsUserDelegate = approverEmployeeIndividual.IsUserDelegate;
            model.LoggedInUser.IsUserTSHRAdmin = approverEmployeeIndividual.IsUserTSHRAdmin;
            model.LoggedInUser.IsUserTSUser = approverEmployeeIndividual.IsUserTSUser;
            model.LoggedInUser.IndividualID = approverEmployeeIndividual.IndividualId;
            var listOfDelegates = GetDelegatesToIndividual(employeeInformation.IndividualID);

            if (SelectedTimeEntries != null)
            {
                var selTimeEntries = GetSelectedTimeHoursEntries(SelectedTimeEntries);
                if (model.IsUserNonExempt && model.EmployeeIndividual.FLSAStatus == 1)
                {
                    model.TimesheetHours = selTimeEntries;
                }
                else
                {
                    var exemptTimeEntries = selTimeEntries.Where(i => i.HoursTypeID != (int)Enumeration.HoursType.Mileage
                                                    && i.HoursTypeID != (int)Enumeration.HoursType.OnCallHoliday
                                                    && i.HoursTypeID != (int)Enumeration.HoursType.Regular
                                                    && i.HoursTypeID != (int)Enumeration.HoursType.OnCallRegular).ToList();
                    model.TimesheetHours = exemptTimeEntries;
                }

                model.IndividualDelegates = listOfDelegates.ToList();
            }
            return PartialView("_TSHourReject", model);
        }


        public ActionResult ConfirmBeforeDeletingTimeEntries(SelectedTimeEntryViewModel SelectedTimeEntries)
        {
            var model = GetSelectedTimeHoursEntries(SelectedTimeEntries);
            return PartialView("_TSHourConfirmDeletingTimeEntries", model);
        }

        public ActionResult PostDelete(SelectedTimeEntryViewModel SelectedTimeEntries)
        {
            EmployeeIndividualViewModel employeeIndividual = new EmployeeIndividualViewModel();
            var selTimesheetHoursEntries = GetSelectedTimeHoursEntries(SelectedTimeEntries);
            int id = selTimesheetHoursEntries.First().EmployeeInfoID;
            if (id > 0)
            {
                id = repository.GetEmployeeInformationById(id).MasterUserID;
            }

            employeeIndividual = id > 0 ? GetEmployeeIndividual(id) : GetEmployeeIndividual(); //_employee;

            if (SelectedTimeEntries != null || SelectedTimeEntries.Values.Count > 0)
            {
                foreach (var item in SelectedTimeEntries.Values)
                {
                    repository.DeleteTimeEntry(repository.GetTimesheetHourById((Int32.Parse(item))));
                }
            }

            //var tsmodel = GetEmployeeTimesheetInitialInfo(employeeIndividual);
            //var model = GetTimesheetsForIndividual(employeeIndividual.IndividualId, "", int.Parse(SelectedTimeEntries.SelectedPayPeriodId));
            //return PartialView("_TSWeeklyView", model);
            var payperiod = AutoMapper.Mapper.Map(repository.GetPayPeriodByPayPeriodId(int.Parse(SelectedTimeEntries.SelectedPayPeriodId)), new PayPeriodViewModel());
            var payperiodYear = payperiod.dtmPeriodEnd.Year.ToString(); // employeeIndividual.FLSAStatus > 0 ? payperiod.dtmPeriodEnd.Year.ToString() : "";
            var payperiodId = payperiod.PayPeriodID; // employeeIndividual.FLSAStatus > 0 ? payperiod.PayPeriodID : 0;
            return RedirectToAction("GetRefresh_TSWeeklyView", new
            {
                individualId = employeeIndividual.IndividualId,
                selectedYear = payperiodYear,
                selectedPayPeriodId = payperiodId
            });

        }

        public ActionResult PostSubmitForApproval(SelectedTimeEntryViewModel SelectedTimeEntries, int MasterUserID, int ApproverMasterUserID)
        {
            if (ModelState.IsValid)
            {
                EmployeeIndividualViewModel employeeIndividual = new EmployeeIndividualViewModel();
                employeeIndividual = ProcessSelectedTimeEntries(SelectedTimeEntries, ApproverMasterUserID, employeeIndividual, Enumeration.StatusType.Submitted, true, (int)Enumeration.TimesheetAction.Submit);
                var model = GetTimesheetsForIndividual(employeeIndividual.IndividualId, "", int.Parse(SelectedTimeEntries.SelectedPayPeriodId));
                var approverEmployeeIndividual = GetEmployeeIndividual(ApproverMasterUserID);
                model.LoggedInUser.EmployeeInfoID = approverEmployeeIndividual.EmployeeInfoId;
                model.LoggedInUser.IsUserDelegate = approverEmployeeIndividual.IsUserDelegate;
                model.LoggedInUser.IsUserTSHRAdmin = approverEmployeeIndividual.IsUserTSHRAdmin;
                model.LoggedInUser.IsUserTSManager = approverEmployeeIndividual.IsUserTSManager;
                model.LoggedInUser.IsUserTSUser = approverEmployeeIndividual.IsUserTSUser;
                return PartialView("_TSWeeklyView", model);
            }

            //var employeeIndividual = GetEmployeeIndividual(); //_employee;
            //var tsmodel = GetEmployeeTimesheetInitialInfo(employeeIndividual);

            //return PartialView("_TSWeeklyView", tsmodel);

            return RedirectToAction("Index");
        }

        public ActionResult PostApprove(SelectedTimeEntryViewModel SelectedTimeEntries, int MasterUserID, int ApproverMasterUserID)
        {
            if (ModelState.IsValid)
            {
                EmployeeIndividualViewModel employeeIndividual = new EmployeeIndividualViewModel();
                employeeIndividual = ProcessSelectedTimeEntries(SelectedTimeEntries, ApproverMasterUserID, employeeIndividual, Enumeration.StatusType.Approved, true, (int)Enumeration.TimesheetAction.Approve);
                var tsmodel = GetTimesheetsForIndividual(employeeIndividual.IndividualId, "", int.Parse(SelectedTimeEntries.SelectedPayPeriodId));
                var approverEmployeeIndividual = GetEmployeeIndividual(ApproverMasterUserID);
                tsmodel.LoggedInUser.EmployeeInfoID = approverEmployeeIndividual.EmployeeInfoId;
                tsmodel.LoggedInUser.IsUserDelegate = approverEmployeeIndividual.IsUserDelegate;
                tsmodel.LoggedInUser.IsUserTSHRAdmin = approverEmployeeIndividual.IsUserTSHRAdmin;
                tsmodel.LoggedInUser.IsUserTSManager = approverEmployeeIndividual.IsUserTSManager;
                tsmodel.LoggedInUser.IsUserTSUser = approverEmployeeIndividual.IsUserTSUser;
                return PartialView("_TSWeeklyView", tsmodel);
            }
            return RedirectToAction("Index");
        }
        public ActionResult PostReject(SelectedTimeEntryViewModel SelectedTimeEntries, int MasterUserID, int ApproverMasterUserID)
        {
            if (ModelState.IsValid)
            {
                EmployeeIndividualViewModel employeeIndividual = new EmployeeIndividualViewModel();
                employeeIndividual = ProcessSelectedTimeEntries(SelectedTimeEntries, ApproverMasterUserID, employeeIndividual, Enumeration.StatusType.NonSubmitted, false, (int)Enumeration.TimesheetAction.Reject);
                var approverEmployeeIndividual = GetEmployeeIndividual(ApproverMasterUserID);
                var model = GetTimesheetsForIndividual(employeeIndividual.IndividualId, "", int.Parse(SelectedTimeEntries.SelectedPayPeriodId));
                model.LoggedInUser.EmployeeInfoID = approverEmployeeIndividual.EmployeeInfoId;
                model.LoggedInUser.IsUserDelegate = approverEmployeeIndividual.IsUserDelegate;
                model.LoggedInUser.IsUserTSHRAdmin = approverEmployeeIndividual.IsUserTSHRAdmin;
                model.LoggedInUser.IsUserTSManager = approverEmployeeIndividual.IsUserTSManager;
                model.LoggedInUser.IsUserTSUser = approverEmployeeIndividual.IsUserTSUser;
                return PartialView("_TSWeeklyView", model);
            }
            return RedirectToAction("Index");
        }

        private EmployeeIndividualViewModel ProcessSelectedTimeEntries(SelectedTimeEntryViewModel SelectedTimeEntries, int ApproverMasterUserID, EmployeeIndividualViewModel employeeIndividual, Enumeration.StatusType status, bool lockout, int timesheetAction = 0)
        {
            if (SelectedTimeEntries != null) // && SelectedTimeEntries.SelectedDelegateIds.Count > 0)
            {
                var selTimesheetHoursEntries = GetSelectedTimeHoursEntries(SelectedTimeEntries);
                int id = selTimesheetHoursEntries.First().EmployeeInfoID;
                if (id > 0)
                {
                    id = repository.GetEmployeeInformationById(id).MasterUserID;
                }

                employeeIndividual = id > 0 ? GetEmployeeIndividual(id) : GetEmployeeIndividual(); //_employee;
                                                                                                   //1.Update Time Entries status to Approved
                foreach (var item in selTimesheetHoursEntries)
                {
                    item.StatusTypeID = (int)status;
                    item.LockOutEmployee = lockout;
                    if (timesheetAction == (int)Enumeration.TimesheetAction.Submit)
                    {
                        item.Submitdate = DateTime.Now;
                        item.SubmitUser = ApproverMasterUserID == 0 ? employeeIndividual.MasterUserId : ApproverMasterUserID;
                    }
                    if (timesheetAction == (int)Enumeration.TimesheetAction.Approve)
                    {
                        item.ApproveDate = DateTime.Now;
                        item.ApproveUser = ApproverMasterUserID == 0 ? employeeIndividual.MasterUserId : ApproverMasterUserID;
                    }
                    if (timesheetAction == (int)Enumeration.TimesheetAction.Reject)
                    {
                        item.ApproveDate = null;
                        item.ApproveUser = null;
                    }
                    repository.UpsertSingleDateEntry(AutoMapper.Mapper.Map(item, new tblTSTimesheetHour()));
                }

                //2.send email to notify manager
                SendEmailProcess(SelectedTimeEntries, employeeIndividual, ApproverMasterUserID, selTimesheetHoursEntries, timesheetAction);
            }
            return employeeIndividual;
        }


        //public ActionResult GetTimeEntriesByYear(int year, string employeeInfoId)
        //{
        //    if (!string.IsNullOrEmpty(employeeInfoId))
        //    {
        //        var startDate = new DateTime(year, 1, 1);
        //        var endDate = new DateTime(year, 12, DateTime.DaysInMonth(year, 12));
        //        var timeEntries = GetEmployeeTimeEntriesByDateRange(startDate, endDate, int.Parse(employeeInfoId));
        //        var model = GetEmployeeTimesheetInitialInfo(GetEmployeeIndividual());
        //        model.TimesheetHours = timeEntries;
        //        return PartialView("_TSWeeklyView", model);
        //    }
        //    return RedirectToAction("Index");
        //}

        //public ActionResult GetTimeEntries(DateTime startDate, DateTime endDate, string employeeInfoId)
        //{
        //    if (!string.IsNullOrEmpty(employeeInfoId))
        //    {
        //        var timeEntries = GetEmployeeTimeEntriesByDateRange(startDate, endDate, int.Parse(employeeInfoId));
        //        var model = GetEmployeeTimesheetInitialInfo(GetEmployeeIndividual());
        //        model.TimesheetHours = timeEntries;
        //        return PartialView("_TSWeeklyView", model);
        //    }
        //    return RedirectToAction("Index");
        //}

        [HttpPost]
        public ActionResult PostSearchEmployees(string firstName, string lastName, string fileNumber)
        {
            var model = new List<SearchEmployeeResultViewModel>();
            if (!string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName) || !string.IsNullOrEmpty(fileNumber))
            {
                var result = repository.GetEmployeeIndividualByFirstLastNameFileNumber(firstName, lastName, fileNumber);
                model = AutoMapper.Mapper.Map(result, new List<SearchEmployeeResultViewModel>());
            }
            return PartialView("_TSSearchEmployeeResults", model);
        }

        /// <summary>
        /// This method is called in the jQuery from Timesheetview
        /// cascading picklist based on the selected year
        /// </summary>
        /// <param name="selectedYear"></param>
        /// <returns></returns>
        public JsonResult GetPayPeriodsBySelectedYear(string selectedYear)
        {
            List<SelectListItem> payperiods = new List<SelectListItem>();
            var payperiodsBySelectedYear = GetPayPeriodsList(string.IsNullOrEmpty(selectedYear) ? DateTime.Now.Year : int.Parse(selectedYear));

            if (!string.IsNullOrEmpty(selectedYear))
            {
                payperiods = GetPayPeriodsListItems(payperiodsBySelectedYear.OrderByDescending(i => i.dtmPeriodEnd).ToList());
            }
            //TODO: get the first open pay period, if none, then get the first processed pay period, and then set it as selected value
            if (payperiods == null || payperiods.Count() == 0) { payperiods.Add(new SelectListItem() { Text = "Select PayPeriod", Value = "0" }); }
            return Json(new SelectList(payperiods, "Value", "Text"));
        }


        /// <summary>
        /// This function validates nonregular time hour entries.  Time off in a day cannot exceed 12 hours.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public JsonResult GetCurrentHoursByDate(TimesheetViewModel model)
        {
            //Get all dates based on DateTimeStart and DateTimeEnd
            var timeDateRange = AutoMapper.Mapper.Map(repository.GetTimeDateRange(model.tbDateRangeStart, model.tbDateRangeEnd), new List<TSTimeEntryViewModel>());
            List<TimesheetHoursViewModel> currentEntries = GetEmployeeTimeEntriesByDateRange(model.tbDateRangeStart, model.tbDateRangeEnd, model.EmployeeInfoId).Where(i => i.tblTSHoursType.HoursTypeID != (int)Enumeration.HoursType.Mileage).ToList();

            if (model.cbDateRange) //DateRange
            {
                //find all selected days when DateRange is checked
                //then reassign values to timeDateRange variable to override values
                var selectedDaysInDateRange = new List<TSTimeEntryViewModel>();
                foreach (var item in timeDateRange)
                {
                    bool flagItemToAdd = false;
                    if (model.cbSunday && item.strDayOfWeek == DayOfWeek.Sunday.ToString()) flagItemToAdd = true;
                    if (model.cbMonday && item.strDayOfWeek == DayOfWeek.Monday.ToString()) flagItemToAdd = true;
                    if (model.cbTuesday && item.strDayOfWeek == DayOfWeek.Tuesday.ToString()) flagItemToAdd = true;
                    if (model.cbWednesday && item.strDayOfWeek == DayOfWeek.Wednesday.ToString()) flagItemToAdd = true;
                    if (model.cbThursday && item.strDayOfWeek == DayOfWeek.Thursday.ToString()) flagItemToAdd = true;
                    if (model.cbFriday && item.strDayOfWeek == DayOfWeek.Friday.ToString()) flagItemToAdd = true;
                    if (model.cbSaturday && item.strDayOfWeek == DayOfWeek.Saturday.ToString()) flagItemToAdd = true;
                    if (flagItemToAdd) selectedDaysInDateRange.Add(item);
                }
                if (selectedDaysInDateRange.Count() > 0)
                {
                    timeDateRange = selectedDaysInDateRange;
                }
            }

            bool timeEntryIsValid = true;
            var selectedAMStart = (model.SelectedAMTimeStart * 100) + model.SelectedAMMinutesStart;
            var selectedAMEnd = (model.SelectedAMTimeEnd * 100) + model.SelectedAMMinutesEnd;
            var selectedPMStart = (model.SelectedPMTimeStart * 100) + model.SelectedPMMinutesStart;
            var selectedPMEnd = (model.SelectedPMTimeEnd * 100) + model.SelectedPMMinutesEnd;
            var selectedAdditionalTimeStart = (model.SelectedAdditionalTimeStart * 100) + model.SelectedAdditionalTimeMinutesStart;
            var selectedAdditionalTimeEnd = (model.SelectedAdditionalTimeMinutesEnd * 100) + model.SelectedAdditionalTimeMinutesEnd;
            decimal counterPTO = 0;


            //update this value if an invalid entry is added, pass it back with Json result to throw in error message.
            List<string> invalidDateEntries = new List<string>();
            List<TSTimeEntryViewModel> validDateEntries = new List<TSTimeEntryViewModel>();
            var messageDateValidation = string.Empty;
            decimal newHoursToAdd = model.SelectedHourMinuteInDecimal;

            //Check for Max Time Off            
            var payperiod = repository.GetPayPeriodByPayPeriodId(model.SelectedPayPeriodID);

            if (model.SelectedHoursType == (int)Enumeration.HoursType.PTO)
            {
                var weekOneEndDate = payperiod.dtmPeriodStart.AddDays(6);
                var weekTwoStartDate = weekOneEndDate.AddDays(1);
                var weekOneTimeEntries = GetEmployeeTimeEntriesByDateRange(payperiod.dtmPeriodStart, weekOneEndDate, model.EmployeeInfoId).Where(i => i.tblTSHoursType.HoursTypeID == (int)Enumeration.HoursType.PTO).ToList();
                var weekTwoTimeEntries = GetEmployeeTimeEntriesByDateRange(weekTwoStartDate, payperiod.dtmPeriodEnd, model.EmployeeInfoId).Where(i => i.tblTSHoursType.HoursTypeID != (int)Enumeration.HoursType.PTO).ToList();
                decimal weekOnePTOTotalHours = weekOneTimeEntries.Where(i => i.HoursTypeID == (int)Enumeration.HoursType.PTO).Sum(i => i.Hours);
                decimal weekTwoPTOTotalHours = weekTwoTimeEntries.Where(i => i.HoursTypeID == (int)Enumeration.HoursType.PTO).Sum(i => i.Hours);
                var totalHoursEntered = timeDateRange.Count() * newHoursToAdd;

                foreach (var item in timeDateRange)
                {
                    if (item.dtmDate >= payperiod.dtmPeriodStart && item.dtmDate < weekOneEndDate)
                    {
                        counterPTO += newHoursToAdd;
                        if (counterPTO + weekOnePTOTotalHours > (decimal)model.EmployeeIndividual.EmployeeInformation.realMaxTimeOffPerWeek)
                        {
                            counterPTO = weekOnePTOTotalHours + totalHoursEntered;
                            messageDateValidation += payperiod.dtmPeriodStart.ToShortDateString() + "-" + weekOneEndDate.ToShortDateString() + "#OverMaxTimeOffWeekOne";
                            timeEntryIsValid = false;
                            invalidDateEntries.Add(messageDateValidation);
                            messageDateValidation = string.Empty;
                        }
                        else
                        {
                            validDateEntries.Add(item);
                        }
                    }
                    if (item.dtmDate >= weekTwoStartDate && item.dtmDate < payperiod.dtmPeriodEnd)
                    {
                        counterPTO += newHoursToAdd;
                        if (counterPTO + weekTwoPTOTotalHours > (decimal)model.EmployeeIndividual.EmployeeInformation.realMaxTimeOffPerWeek)
                        {
                            counterPTO = weekTwoPTOTotalHours + totalHoursEntered;
                            messageDateValidation += weekTwoStartDate.ToShortDateString() + "-" + payperiod.dtmPeriodEnd.ToShortDateString() + "#OverMaxTimeOffWeekTwo";
                            timeEntryIsValid = false;
                            invalidDateEntries.Add(messageDateValidation);
                            messageDateValidation = string.Empty;
                        }
                        else
                        {
                            validDateEntries.Add(item);
                        }
                    }

                }
            }
            //compare new entry hour total to current hour total for that date
            foreach (var newEntry in timeDateRange)
            {
                List<TimesheetHoursViewModel> tempCurrentEntries = new List<TimesheetHoursViewModel>();
                foreach (var currEntry in currentEntries)
                {
                    if (newEntry.dtmDate == currEntry.Date)
                        tempCurrentEntries.Add(currEntry);
                }

                if (model.SelectedHoursType == (int)Enumeration.HoursType.Regular)
                {
                    decimal hoursTotalAM = GetTotalNumberOfHours(model.SelectedAMTimeStart, model.SelectedAMTimeEnd, model.SelectedAMMinutesStart, model.SelectedAMMinutesEnd);
                    decimal hoursTotalPM = GetTotalNumberOfHours(model.SelectedPMTimeStart, model.SelectedPMTimeEnd, model.SelectedPMMinutesStart, model.SelectedPMMinutesEnd);
                    decimal hoursTotalAdditional = GetTotalNumberOfHours(model.SelectedAdditionalTimeStart, model.SelectedAdditionalTimeEnd, model.SelectedAdditionalTimeMinutesStart, model.SelectedAdditionalTimeMinutesEnd);
                    newHoursToAdd = hoursTotalAM + hoursTotalPM + hoursTotalAdditional;
                }
                decimal currentHours = 0;
                if (tempCurrentEntries.Count() == 0)
                {
                    //no existing time entries
                    if (model.SelectedHoursType == (int)Enumeration.HoursType.PTO)
                    {
                        //check if over 12 hours
                        if (newHoursToAdd > 12)
                        {
                            messageDateValidation += newEntry.dtmDate.ToString() + "#PTOExceeds12HourADay";
                            timeEntryIsValid = false;
                            invalidDateEntries.Add(messageDateValidation);
                            messageDateValidation = string.Empty;
                        }
                    }
                    else if (model.SelectedHoursType == (int)Enumeration.HoursType.Regular)
                    {
                        if (newHoursToAdd > 24)
                        {
                            messageDateValidation += newEntry.dtmDate.ToString() + "#Over24Hours";
                            timeEntryIsValid = false;
                            invalidDateEntries.Add(messageDateValidation);
                            messageDateValidation = string.Empty;
                        }
                    }
                    else if ((selectedAMStart >= 0 && selectedAMEnd <= 2400) ||
                           (selectedPMStart >= 0 && selectedPMEnd <= 2400) ||
                           (selectedAdditionalTimeStart >= 0 && selectedAdditionalTimeEnd <= 2400))
                    {
                        timeEntryIsValid = true;
                        validDateEntries.Add(newEntry);
                    }
                }
                else //with existing time entries
                {
                    var totalTimeOffHoursExist = tempCurrentEntries.Where(i => i.HoursTypeID == (int)Enumeration.HoursType.PTO && i.Date == newEntry.dtmDate).Sum(i => i.Hours);
                    var overallTotalHoursOfTheDay = tempCurrentEntries.Where(i => i.Date == newEntry.dtmDate).Sum(i => i.Hours);
                    //PTOExceeds 12 Hour-day
                    if ((totalTimeOffHoursExist + newHoursToAdd) > 12 && model.SelectedHoursType == (int)Enumeration.HoursType.PTO)
                    {
                        messageDateValidation += newEntry.dtmDate.ToString() + "#PTOExceeds12HourADay";
                        timeEntryIsValid = false;
                        invalidDateEntries.Add(messageDateValidation);
                        messageDateValidation = string.Empty;
                    }
                    //Check for Over 24 hours total time entered for a day
                    else if ((newHoursToAdd + overallTotalHoursOfTheDay) > 24)
                    {
                        messageDateValidation += newEntry.dtmDate.ToString() + "#Over24Hours";
                        timeEntryIsValid = false;
                        invalidDateEntries.Add(messageDateValidation);
                        messageDateValidation = string.Empty;
                    }

                    int amStartTime = CalculateTimeHourMinute(model.SelectedAMTimeStart, model.SelectedAMMinutesStart);
                    int amEndTime = CalculateTimeHourMinute(model.SelectedAMTimeEnd, model.SelectedAMMinutesEnd);
                    int pmStartTime = CalculateTimeHourMinute(model.SelectedPMTimeStart, model.SelectedPMMinutesStart);
                    int pmEndTime = CalculateTimeHourMinute(model.SelectedPMTimeEnd, model.SelectedPMMinutesEnd);
                    int addStartTime = CalculateTimeHourMinute(model.SelectedAdditionalTimeStart, model.SelectedAdditionalTimeMinutesStart);
                    int addStartEnd = CalculateTimeHourMinute(model.SelectedAdditionalTimeEnd, model.SelectedAdditionalTimeMinutesEnd);

                    foreach (var currentEntry in tempCurrentEntries)
                    {
                        //there may be more than one entry already for a date.  get sum of hours on that day
                        currentHours += currentEntry.Hours;

                        //if (model.SelectedHoursType != (int)Enumeration.HoursType.Regular)
                        //{
                        //    //PTOExceeds 12 Hour-day
                        //    if ((totalTimeOffHoursExist + newHoursToAdd) > 12 && model.SelectedHoursType == (int)Enumeration.HoursType.PTO)
                        //    {
                        //        messageDateValidation += newEntry.dtmDate.ToString() + "#PTOExceeds12HourADay";
                        //        timeEntryIsValid = false;
                        //        invalidDateEntries.Add(messageDateValidation);
                        //    }
                        //    //Check for Over 24 hours total time entered for a day
                        //    else if ((newHoursToAdd + overallTotalHoursOfTheDay) > 24)
                        //    {
                        //        messageDateValidation += newEntry.dtmDate.ToString() + "#Over24Hours";
                        //        timeEntryIsValid = false;
                        //        invalidDateEntries.Add(messageDateValidation);
                        //    }
                        //}
                        if (currentEntry.HoursTypeID == (int)Enumeration.HoursType.Regular && model.SelectedHoursType == (int)Enumeration.HoursType.Regular)
                        {

                            //check for overlapping previously entered regular time entries

                            if ((amStartTime >= currentEntry.TimeStart && amStartTime < currentEntry.TimeEnd) &&
                                (amEndTime >= currentEntry.TimeStart && amEndTime > currentEntry.TimeEnd))
                            {
                                messageDateValidation += newEntry.dtmDate.ToString() + "#OverlappingTimeEntry";
                                timeEntryIsValid = false;
                                invalidDateEntries.Add(messageDateValidation);
                                messageDateValidation = string.Empty;
                            }
                            else if ((amStartTime > currentEntry.TimeStart && amStartTime < currentEntry.TimeEnd) &&
                                    (amEndTime > currentEntry.TimeStart && amEndTime < currentEntry.TimeEnd))
                            {
                                messageDateValidation += newEntry.dtmDate.ToString() + "#OverlappingTimeEntry";
                                timeEntryIsValid = false;
                                invalidDateEntries.Add(messageDateValidation);
                                messageDateValidation = string.Empty;
                            }
                            else if (amStartTime < currentEntry.TimeEnd && amEndTime > currentEntry.TimeStart)
                            {
                                messageDateValidation += newEntry.dtmDate.ToString() + "#OverlappingTimeEntry";
                                timeEntryIsValid = false;
                                invalidDateEntries.Add(messageDateValidation);
                                messageDateValidation = string.Empty;
                            }

                            //second row
                            if ((pmStartTime >= currentEntry.TimeStart && pmStartTime < currentEntry.TimeEnd) &&
                                (pmEndTime >= currentEntry.TimeStart && pmEndTime > currentEntry.TimeEnd))
                            {
                                messageDateValidation += newEntry.dtmDate.ToString() + "#OverlappingTimeEntry";
                                timeEntryIsValid = false;
                                invalidDateEntries.Add(messageDateValidation);
                                messageDateValidation = string.Empty;
                            }
                            else if ((pmStartTime > currentEntry.TimeStart && pmStartTime < currentEntry.TimeEnd) &&
                                (pmEndTime > currentEntry.TimeStart && pmEndTime < currentEntry.TimeEnd))
                            {
                                messageDateValidation += newEntry.dtmDate.ToString() + "#OverlappingTimeEntry";
                                timeEntryIsValid = false;
                                invalidDateEntries.Add(messageDateValidation);
                                messageDateValidation = string.Empty;
                            }
                            else if (pmStartTime < currentEntry.TimeEnd && pmEndTime > currentEntry.TimeStart)
                            {
                                messageDateValidation += newEntry.dtmDate.ToString() + "#OverlappingTimeEntry";
                                timeEntryIsValid = false;
                                invalidDateEntries.Add(messageDateValidation);
                                messageDateValidation = string.Empty;
                            }

                            //third row
                            if ((addStartTime >= currentEntry.TimeStart && addStartTime < currentEntry.TimeEnd) &&
                                (addStartEnd >= currentEntry.TimeStart && addStartEnd > currentEntry.TimeEnd))
                            {
                                messageDateValidation += newEntry.dtmDate.ToString() + "#OverlappingTimeEntry";
                                timeEntryIsValid = false;
                                invalidDateEntries.Add(messageDateValidation);
                                messageDateValidation = string.Empty;
                            }
                            else if ((addStartTime > currentEntry.TimeStart && addStartTime < currentEntry.TimeEnd) &&
                                (addStartEnd > currentEntry.TimeStart && addStartEnd < currentEntry.TimeEnd))
                            {
                                messageDateValidation += newEntry.dtmDate.ToString() + "#OverlappingTimeEntry";
                                timeEntryIsValid = false;
                                invalidDateEntries.Add(messageDateValidation);
                                messageDateValidation = string.Empty;
                            }
                            else if (addStartTime < currentEntry.TimeEnd && addStartEnd > currentEntry.TimeStart)
                            {
                                messageDateValidation += newEntry.dtmDate.ToString() + "#OverlappingTimeEntry";
                                timeEntryIsValid = false;
                                invalidDateEntries.Add(messageDateValidation);
                                messageDateValidation = string.Empty;
                            }
                        }

                    }
                    currentHours = 0;
                    tempCurrentEntries.Clear();
                }
            }
            //TODO: Add time for valid date entries
            if (timeEntryIsValid)
            {
                return Json(new { valid = true });
            }
            else
                return Json(new
                {
                    valid = false,
                    invalidDates = invalidDateEntries.Distinct().ToList(),
                    maxTimeOff = model.EmployeeIndividual.EmployeeInformation.realMaxTimeOffPerWeek,
                    maxTimeOffToAdd = counterPTO
                });

        }

        public JsonResult GetDateEnteredTotalHours(TimesheetViewModel model)
        {
            string dateEntered = model.EditSelectedDateEntry.ToShortDateString();
            int payperiodId = model.SelectedPayPeriodID;
            int employeeInfoId = model.EmployeeInfoId;
            int editTimesheetHourId = model.EditTimesheetHourId;
            decimal hoursSelected = model.EditSelectedHourMinuteInDecimal;
            if (string.IsNullOrEmpty(dateEntered) || payperiodId != 0 || employeeInfoId > 0)
            {
                //Submitted and Non-Submitted
                var editTimesheet = repository.GetTimesheetHourById(editTimesheetHourId);
                var payperiod = repository.GetPayPeriodByPayPeriodId(payperiodId);
                var timeEntries = repository.GetTimesheetHoursForDateEntered(payperiodId, employeeInfoId, DateTime.Parse(dateEntered)).Where(i => i.tblTSStatusType.StatusTypeID > (int)Enumeration.StatusType.Approved);
                List<tblTSTimesheetHour> currentDateTimeEntries = repository.GetTimeEntriesOnSelectedDate(editTimesheet.Date, payperiodId, employeeInfoId);
                var employeeInfo = repository.GetEmployeeInformationById(employeeInfoId);
                var maxTimeOffPerWeek = repository.GetEmployeeInformationById(employeeInfoId).realMaxTimeOffPerWeek;
                bool timeEntryIsValid = true;
                string messageDateValidation = string.Empty;
                List<string> invalidTimeEntryReason = new List<string>();
                decimal weeklyPTOTotalHours = 0;

                if (editTimesheet.HoursTypeID == (int)Enumeration.HoursType.PTO)
                {
                    var weekOneEndDate = payperiod.dtmPeriodStart.AddDays(6);
                    var weekTwoStartDate = weekOneEndDate.AddDays(1);
                    var weeklyTimeEntries = new List<TimesheetHoursViewModel>();
                    string payperiodWeek = string.Empty;

                    var currentDateTotalHours = currentDateTimeEntries.Where(i => i.HoursTypeID == (int)Enumeration.HoursType.PTO).Sum(i => i.Hours) - editTimesheet.Hours;

                    if (editTimesheet.Date >= payperiod.dtmPeriodStart && editTimesheet.Date <= weekOneEndDate)
                    {
                        //week one
                        weeklyTimeEntries = GetEmployeeTimeEntriesByDateRange(payperiod.dtmPeriodStart, weekOneEndDate, employeeInfoId).Where(i => i.tblTSHoursType.HoursTypeID == (int)Enumeration.HoursType.PTO).ToList();
                        weeklyPTOTotalHours = weeklyTimeEntries.Sum(i => i.Hours) - editTimesheet.Hours;
                        payperiodWeek = payperiod.dtmPeriodStart.ToShortDateString() + "-" + weekOneEndDate.ToShortDateString();
                    }
                    else
                    {
                        //week two
                        weeklyTimeEntries = GetEmployeeTimeEntriesByDateRange(weekTwoStartDate, payperiod.dtmPeriodEnd, employeeInfoId).Where(i => i.tblTSHoursType.HoursTypeID != (int)Enumeration.HoursType.PTO).ToList();
                        weeklyPTOTotalHours = weeklyTimeEntries.Sum(i => i.Hours) - editTimesheet.Hours;
                        payperiodWeek = weekTwoStartDate.ToShortDateString() + "-" + payperiod.dtmPeriodEnd.ToShortDateString();
                    }

                    if ((weeklyPTOTotalHours + hoursSelected) > (decimal)(maxTimeOffPerWeek ?? 0))
                    {
                        messageDateValidation += payperiodWeek + "#OverMaxTimeOffPerWeek";
                        timeEntryIsValid = false;
                        invalidTimeEntryReason.Add(messageDateValidation);
                        messageDateValidation = string.Empty;
                    }
                    if ((currentDateTotalHours + hoursSelected) > 12)
                    {
                        messageDateValidation += editTimesheet.Date.ToShortDateString() + "#PTOExceeds12HourADay";
                        timeEntryIsValid = false;
                        invalidTimeEntryReason.Add(messageDateValidation);
                        messageDateValidation = string.Empty;
                    }
                }
                else if (editTimesheet.HoursTypeID == (int)Enumeration.HoursType.Regular)
                {
                    int amStartTime = CalculateTimeHourMinute(model.EditSelectedHourStart, model.EditSelectedMinuteStart);
                    int amEndTime = CalculateTimeHourMinute(model.EditSelectedHourEnd, model.EditSelectedMinuteEnd);
                    decimal totalHoursEntered = GetTotalNumberOfHours(model.EditSelectedHourStart, model.EditSelectedHourEnd, model.EditSelectedMinuteStart, model.EditSelectedMinuteEnd);
                    var existingHours = currentDateTimeEntries.Where(i => i.HoursTypeID == (int)Enumeration.HoursType.Regular).Sum(i => i.Hours);
                    foreach (var currentEntry in currentDateTimeEntries)
                    {
                        if (currentEntry.HoursTypeID == (int)Enumeration.HoursType.Regular && editTimesheet.TimesheetHoursID != currentEntry.TimesheetHoursID)
                        {
                            if ((amStartTime >= currentEntry.TimeStart && amStartTime < currentEntry.TimeEnd) &&
                                (amEndTime >= currentEntry.TimeStart && amEndTime > currentEntry.TimeEnd))
                            {
                                messageDateValidation += editTimesheet.Date.ToShortDateString() + "#OverlappingTimeEntry";
                                timeEntryIsValid = false;
                                invalidTimeEntryReason.Add(messageDateValidation);
                                messageDateValidation = string.Empty;
                            }
                        }
                    }

                }

                decimal totalHours = 0;
                if (timeEntryIsValid)
                {
                    return Json(new { valid = true });
                }
                else
                {
                    foreach (var item in timeEntries)
                    {
                        totalHours += item.Hours;
                    }
                    return Json(new
                    {
                        valid = false,
                        dateEnteredTotalHours = totalHours,
                        originalHours = editTimesheet.Hours,
                        invalidDateReason = invalidTimeEntryReason.Distinct().ToList(),
                        maxTimeOff = maxTimeOffPerWeek,
                        hoursToAdd = weeklyPTOTotalHours + hoursSelected
                    });
                }
                //if (timeEntries != null && timeEntries.Count() > 0)
                //{
                //    foreach (var item in timeEntries)
                //    {
                //        totalHours += item.Hours;
                //    }
                //    return Json(new { valid = timeEntryIsValid, dateEnteredTotalHours = totalHours, originalHours = editTimesheet.Hours });
                //}
                //else
                //{
                //    return Json(new { valid = timeEntryIsValid, dateEnteredTotalHours = 0, originalHours = 0 });
                //}
            }
            return Json(new { valid = true, dateEnteredTotalHours = 0, originalHours = 0 });
        }

        public ActionResult GetTimeEntryForIndividual(int id, string selectedYear = "", int selectedPayPeriodId = 0)
        {
            TimesheetViewModel model = new TimesheetViewModel();
            if (id > 0)
            {
                model = GetTimesheetsForIndividual(id, selectedYear, selectedPayPeriodId);
                var authenticatedUser = AuthenticateUser;
                var loggedInUser = GetEmployeeIndividual((int)authenticatedUser.MasterUserID);
                model.AuthenticatedUser = authenticatedUser;
                model.LoggedInUser.EmployeeInfoID = loggedInUser.EmployeeInfoId;
                model.LoggedInUser.IsUserTSHRAdmin = loggedInUser.IsUserTSHRAdmin;
                model.LoggedInUser.IsUserTSManager = loggedInUser.IsUserTSManager;
                model.LoggedInUser.IsUserDelegate = loggedInUser.IsUserDelegate;
                model.LoggedInUser.IsUserTSUser = loggedInUser.IsUserTSUser;
                return PartialView("_TSTimeEntryManageView", model);
            }
            return RedirectToAction("Index");
        }

        public ActionResult TimesheetOfEmployee(int individualId, string selectedYear = "", int selectedPayPeriodId = 0)
        {
            return View("TimesheetOfEmployee");
        }


        public ActionResult GetTimesheetOfEmployee(int individualId, string selectedYear = "", int selectedPayPeriodId = 0)
        {
            TimesheetViewModel model = new TimesheetViewModel();
            if (individualId > 0)
            {
                return RedirectToAction("GetTimeEntryForIndividual", new { id = individualId, selectedYear = selectedYear, selectedPayPeriodId = selectedPayPeriodId });
                //model = GetTimesheetsForIndividual(individualId, selectedYear, selectedPayPeriodId);
                //var authenticatedUser = AuthenticateUser;
                //var loggedInUser = GetEmployeeIndividual((int)authenticatedUser.MasterUserID);
                //model.AuthenticatedUser = authenticatedUser;
                //model.LoggedInUser.EmployeeInfoID = loggedInUser.EmployeeInfoId;
                //model.LoggedInUser.IsUserTSHRAdmin = loggedInUser.IsUserTSHRAdmin;
                //model.LoggedInUser.IsUserTSManager = loggedInUser.IsUserTSManager;
                //model.LoggedInUser.IsUserDelegate = loggedInUser.IsUserDelegate;
                //model.LoggedInUser.IsUserTSUser = loggedInUser.IsUserTSUser;
                //return PartialView("_TSTimeEntryManageView", model);
            }
            return RedirectToAction("Index");
        }

        public ActionResult GetRefresh_TSWeeklyView(int individualId, string selectedYear = "", int selectedPayPeriodId = 0)
        {
            TimesheetViewModel model = new TimesheetViewModel();
            if (individualId > 0)
            {
                model = GetTimesheetsForIndividual(individualId, selectedYear, selectedPayPeriodId);
                var authenticatedUser = AuthenticateUser;
                var loggedInUser = GetEmployeeIndividual((int)authenticatedUser.MasterUserID);
                model.AuthenticatedUser = authenticatedUser;
                model.LoggedInUser.EmployeeInfoID = loggedInUser.EmployeeInfoId;
                model.LoggedInUser.IsUserTSHRAdmin = loggedInUser.IsUserTSHRAdmin;
                model.LoggedInUser.IsUserTSManager = loggedInUser.IsUserTSManager;
                model.LoggedInUser.IsUserDelegate = loggedInUser.IsUserDelegate;
                model.LoggedInUser.IsUserTSUser = loggedInUser.IsUserTSUser;
                return PartialView("_TSWeeklyView", model);
            }
            return null;
        }

        public ActionResult GetRefresh_ManageView(int pIndividualId, string pSelectedYear = "", int pSelectedPayPeriodId = 0)
        {
            TimesheetViewModel model = new TimesheetViewModel();
            if (pIndividualId > 0)
            {
                model = GetTimesheetsForIndividual(pIndividualId, pSelectedYear, pSelectedPayPeriodId);
                var authenticatedUser = AuthenticateUser;
                var loggedInUser = GetEmployeeIndividual((int)authenticatedUser.MasterUserID);
                model.AuthenticatedUser = authenticatedUser;
                model.LoggedInUser.EmployeeInfoID = loggedInUser.EmployeeInfoId;
                model.LoggedInUser.IsUserTSHRAdmin = loggedInUser.IsUserTSHRAdmin;
                model.LoggedInUser.IsUserTSManager = loggedInUser.IsUserTSManager;
                model.LoggedInUser.IsUserDelegate = loggedInUser.IsUserDelegate;
                model.LoggedInUser.IsUserTSUser = loggedInUser.IsUserTSUser;
                return PartialView("_TSTimeEntryManageView", model);
            }
            return null;
        }

        public ActionResult GetRefresh_PayPeriodHoursTypeSummary(int selectedPayPeriodId, int employeeInfoId, int individualId)
        {
            TimesheetViewModel model = new TimesheetViewModel();
            if (employeeInfoId > 0 && selectedPayPeriodId > 0)
            {
                var selectedPayPeriod = repository.GetPayPeriodByPayPeriodId(selectedPayPeriodId);
                model = GetTimesheetsForIndividual(individualId, "", selectedPayPeriodId);
                var timeEntries = GetEmployeeTimeEntriesByDateRange(selectedPayPeriod.dtmPeriodStart, selectedPayPeriod.dtmPeriodEnd, employeeInfoId).Where(i => i.HoursTypeID != (int)Enumeration.HoursType.Mileage && i.PayPeriodID == selectedPayPeriod.PayPeriodID).ToList();
                var onCallEntries = GetEmployeeOnCallEntriesByDateRange(selectedPayPeriod.dtmPeriodStart, selectedPayPeriod.dtmPeriodEnd, employeeInfoId);
                var mileageEntries = GetMileageEntriesByPayPeriod(selectedPayPeriod.PayPeriodID, employeeInfoId);//GetMileageEntriesByDate(selectedPayPeriod.dtmPeriodStart, selectedPayPeriod.dtmPeriodEnd, employeeInfoId);
                var employeeInfo = repository.GetEmployeeInformationById(employeeInfoId);
                model.IsUserNonExempt = employeeInfo.IsNonExempt ?? true;
                model.OnCallSummaryNonExempt = onCallEntries != null && onCallEntries.Count() > 0 ? GetEmployeeOnCallSummary(selectedPayPeriod.dtmPeriodStart, selectedPayPeriod.dtmPeriodEnd, onCallEntries) : null;
                model.TimeOffSummaryNonExempt = timeEntries != null && timeEntries.Count() > 0 ? GetTimeOffSummaryNonExempt(selectedPayPeriod.dtmPeriodStart, selectedPayPeriod.dtmPeriodEnd, timeEntries) : null;
                model.MileageSummary = mileageEntries != null & mileageEntries.Count() > 0 ? GetMileageSummaryBasedOnSelectedPayPeriod(selectedPayPeriod.PayPeriodID, mileageEntries) : null;
                model.SelectedPayPeriod = AutoMapper.Mapper.Map(selectedPayPeriod, new PayPeriodViewModel());
                model.SelectedPayPeriodID = selectedPayPeriodId;
                model.EmployeeInfoId = employeeInfoId;

                return PartialView("_PayPeriodHoursTypeSummary", model);
            }
            return null;
        }

        public ActionResult GetRefreshAfterDelete_PayPeriodSummary(int selectedPayPeriodId, int employeeInfoId)
        {
            TimesheetViewModel model = new TimesheetViewModel();
            if (employeeInfoId > 0)
            {
                var selectedPayPeriod = repository.GetPayPeriodByPayPeriodId(selectedPayPeriodId);
                var employeeInformation = repository.GetEmployeeInformationById(employeeInfoId);
                model = GetTimesheetsForIndividual(employeeInformation.IndividualID, "", selectedPayPeriodId);
                var timeEntries = GetEmployeeTimeEntriesByDateRange(selectedPayPeriod.dtmPeriodStart, selectedPayPeriod.dtmPeriodEnd, employeeInfoId).Where(i => i.HoursTypeID != (int)Enumeration.HoursType.Mileage && i.PayPeriodID == selectedPayPeriod.PayPeriodID).ToList();
                var onCallEntries = GetEmployeeOnCallEntriesByDateRange(selectedPayPeriod.dtmPeriodStart, selectedPayPeriod.dtmPeriodEnd, employeeInfoId);
                var mileageEntries = GetMileageEntriesByPayPeriod(selectedPayPeriod.PayPeriodID, employeeInfoId);
                var employeeInfo = repository.GetEmployeeInformationById(employeeInfoId);
                model.IsUserNonExempt = employeeInfo.IsNonExempt ?? true;
                model.OnCallSummaryNonExempt = onCallEntries != null && onCallEntries.Count() > 0 ? GetEmployeeOnCallSummary(selectedPayPeriod.dtmPeriodStart, selectedPayPeriod.dtmPeriodEnd, onCallEntries) : null;
                model.TimeOffSummaryNonExempt = GetTimeOffSummaryNonExempt(selectedPayPeriod.dtmPeriodStart, selectedPayPeriod.dtmPeriodEnd, timeEntries);
                model.MileageSummary = mileageEntries != null & mileageEntries.Count() > 0 ? GetMileageSummaryBasedOnSelectedPayPeriod(selectedPayPeriod.PayPeriodID, mileageEntries) : null;
                model.SelectedPayPeriod = AutoMapper.Mapper.Map(selectedPayPeriod, new PayPeriodViewModel());
                model.SelectedPayPeriodID = selectedPayPeriodId;
                model.EmployeeInfoId = employeeInfoId;
                return PartialView("_PayPeriodHoursTypeSummary", model);
            }
            return null;
        }

        //public ActionResult GetRefreshAfterDelete_TimeOffSummaryYTD(DateTime startDate, int employeeInfoId)
        //{
        //    TimesheetViewModel model = new TimesheetViewModel();
        //    if (employeeInfoId > 0)
        //    {
        //        GetRefresh_TimeOffSummaryYTD(startDate, employeeInfoId, 0);
        //        //startDate = startDate != null ? startDate : DateTime.Now;
        //        //DateTime ytdStartDate = new DateTime();
        //        //DateTime ytdEndDate = new DateTime();
        //        //repository.DatesYTD(startDate, out ytdStartDate, out ytdEndDate);
        //        //var timeEntriesYTD = GetEmployeeTimeEntriesByDateRange(ytdStartDate, ytdEndDate, employeeInfoId);
        //        //var employeeInformation = repository.GetEmployeeInformationById(employeeInfoId);
        //        //model.TimeOffSummaryYTD = GetNonRegularHoursTypeSummary(timeEntriesYTD).OrderByDescending(i => (int)i.StatusType).ToList();
        //        //model.EmployeeInfoId = employeeInfoId;
        //        //return PartialView("_TimeOffSummaryYTD", model);

        //    }
        //    return null;
        //}

        public ActionResult GetRefresh_TimeOffSummaryYTD(DateTime startDate, int employeeInfoId, int individualId)
        {
            TimesheetViewModel model = new TimesheetViewModel();
            if (employeeInfoId > 0)
            {
                startDate = startDate != null ? startDate : DateTime.Now;
                DateTime ytdStartDate = new DateTime();
                DateTime ytdEndDate = new DateTime();
                repository.DatesYTD(startDate, out ytdStartDate, out ytdEndDate);
                var timeEntriesYTD = GetEmployeeTimeEntriesByDateRange(ytdStartDate, ytdEndDate, employeeInfoId);
                var employeeInformation = repository.GetEmployeeInformationById(employeeInfoId);
                if (individualId == 0) { individualId = employeeInformation.IndividualID; }
                model.TimeOffSummaryYTD = GetNonRegularHoursTypeSummary(timeEntriesYTD).OrderByDescending(i => (int)i.StatusType).ToList();
                model.EmployeeInfoId = employeeInfoId;
                model.EmployeeIndividual = new EmployeeIndividualViewModel() { IndividualId = individualId, EmployeeInfoId = employeeInfoId };
                return PartialView("_TimeOffSummaryYTD", model);
            }
            return null;
        }

        public ActionResult GetRefresh_TSMileageView(int selectedPayPeriodId, int employeeInfoId)
        {
            TimesheetViewModel model = new TimesheetViewModel();
            List<TSMileageViewModel> entries = new List<TSMileageViewModel>();
            if (employeeInfoId > 0 && selectedPayPeriodId > 0)
            {
                //var selectedPayPeriod = repository.GetPayPeriodByPayPeriodId(selectedPayPeriodId);
                entries = GetMileageEntriesByPayPeriod(selectedPayPeriodId, employeeInfoId);
                model.MileageEntries = entries;
                return PartialView("_TSMileageView", model);
            }
            return null;
        }

        public ActionResult GetRefresh_TSEmployeeAccessList(int approverMasterUserId)
        {
            TimesheetViewModel model = new TimesheetViewModel();
            if (approverMasterUserId > 0)
            {
                var employeeIndividual = GetEmployeeIndividual(approverMasterUserId);
                var directEmployeesOfApprover = GetDirectEmployees(employeeIndividual.IndividualId);
                var delegatedEmployeesOfApprover = GetEmployeesDelegatedToIndividual(employeeIndividual.IndividualId);
                var selectedYearPayPeriodList = GetPayPeriodsList(DateTime.Now.Year).OrderByDescending(i => i.dtmPeriodEnd).ToList();

                //get the current open or next pay period to be process
                var currentpayperiod = repository.GetCurrentNextPreviousPayPeriod().Where(i => i.TimePeriod == Enumeration.TimePayPeriod.Current.ToString()).FirstOrDefault();
                var payperiod = currentpayperiod != null ? AutoMapper.Mapper.Map(currentpayperiod, new PayPeriodViewModel()) : selectedYearPayPeriodList.FirstOrDefault();

                var ytdDateStart = new DateTime();
                var ytdDateEnd = new DateTime();
                var startDate = DateTime.Now;
                repository.DatesYTD(startDate, out ytdDateStart, out ytdDateEnd);

                //Get Delegated employees if the login user is a delegate
                if (employeeIndividual.IsUserDelegate && delegatedEmployeesOfApprover.Count() > 0)
                {
                    var delegatedemployees = GetEmployeeIndividualTimesheetStatus(delegatedEmployeesOfApprover, ytdDateStart, ytdDateEnd, payperiod);
                    model.DelegatedEmployees = delegatedemployees;
                }
                //Get Direct employees if the login user it has direct employees
                if (directEmployeesOfApprover.Count() > 0)
                {
                    var directEmployees = GetEmployeeIndividualTimesheetStatus(directEmployeesOfApprover, ytdDateStart, ytdDateEnd, payperiod);
                    model.DirectEmployees = directEmployees;
                }
                return PartialView("_TSEmployeeAccessList", model);
            }
            return null;
        }

        #endregion End of Action Methods

        #region private methods

        private List<EmployeeIndividualViewModel> GetEmployeeIndividualTimesheetStatus(List<EmployeeIndividualViewModel> employeeList, DateTime ytdDateStart, DateTime ytdDateEnd, PayPeriodViewModel payperiod)
        {
            if (employeeList != null)
            {
                var textNonSubmitted = repository.GetStatusType().FirstOrDefault(i => i.StatusTypeID == (int)Enumeration.StatusType.NonSubmitted).StatusType;
                foreach (var item in employeeList)
                {
                    if (item.EmployeeStatus != (int)Enumeration.EmployeeStatus.Terminated)
                    {
                        var tsStatus = string.Empty;
                        if (item.EmployeeInformation.IsNonExempt ?? true)
                        {
                            var timeEntries = GetEmployeeTimeEntriesByDateRange(payperiod.dtmPeriodStart, payperiod.dtmPeriodEnd, item.EmployeeInfoId, item.EmployeeInformation.IsNonExempt ?? true).Where(i => i.StatusTypeID > (int)Enumeration.StatusType.Processed).Take(5);
                            if (!string.IsNullOrEmpty(item.CompanyCode))
                            {
                                tsStatus = timeEntries != null && timeEntries.Count() > 0 ? timeEntries.OrderByDescending(i => i.StatusTypeID).FirstOrDefault().tblTSStatusType.StatusType : textNonSubmitted;
                                item.TimesheetStatus = tsStatus;
                                var result = item.FileNumber != null ? GetPTOBalances(item.FileNumber.Trim(), item.CompanyCode) : null;
                                item.PTOBalance = result != null ? (decimal)result.Balance : 0.00M;
                            }
                        }
                        else//Exempt
                        {
                            var timeEntries = GetEmployeeTimeEntriesByDateRange(ytdDateStart, ytdDateEnd, item.EmployeeInfoId, item.EmployeeInformation.IsNonExempt ?? true).Where(i => i.StatusTypeID == (int)Enumeration.StatusType.Submitted);
                            var groupbyStatus = timeEntries.GroupBy(i => i.StatusTypeID).Select(i => new { statusType = i.Key, TotalHours = i.Sum(y => y.Hours) });
                            foreach (var entry in groupbyStatus)
                            {
                                if (entry.statusType == (int)Enumeration.StatusType.Submitted)
                                {
                                    tsStatus = string.Format("{0} {1}", entry.TotalHours, "Hours Pending Approval");
                                    break;
                                }
                            }
                            tsStatus = groupbyStatus != null && groupbyStatus.Count() > 0 ? tsStatus : "No Pending Time Off";
                            item.TimesheetStatus = tsStatus;
                            var result = item.FileNumber != null ? GetPTOBalances(item.FileNumber.Trim(), item.CompanyCode) : null;
                            item.PTOBalance = result != null ? (decimal)result.Balance : 0.00M;
                        }
                    }
                    else
                    {
                        item.TimesheetStatus = Enumeration.EmployeeStatus.Terminated.ToString();
                    }
                }
                return employeeList;
            }
            return null;
        }

        private List<HoursTypeSummaryViewModel> SelectedTimeEntriesSummaryNonExempt(List<HoursTypeSummaryViewModel> timeEntries, List<TimesheetHoursViewModel> selectedTimeEntries)
        {
            if (timeEntries != null && selectedTimeEntries != null)
            {
                var oldValue = timeEntries;
                var temp = new List<HoursTypeSummaryViewModel>();
                foreach (var item in timeEntries)
                {
                    foreach (var entry in selectedTimeEntries)
                    {
                        if (entry.HoursTypeID == item.HoursTypeID)
                        {
                            temp.Add(item);
                            break;
                        }
                    }
                }
                return temp;
            }
            return null;
        }

        private List<TimeOffSummaryViewModel> SelectedTimeEntriesSummaryExempt(List<TimeOffSummaryViewModel> timeEntries, List<TimesheetHoursViewModel> selectedTimeEntries)
        {
            if (timeEntries != null && selectedTimeEntries != null)
            {
                var oldValue = timeEntries;
                var temp = new List<TimeOffSummaryViewModel>();
                foreach (var item in timeEntries)
                {
                    foreach (var entry in selectedTimeEntries)
                    {
                        if (entry.HoursTypeID == item.HoursTypeID)
                        {
                            temp.Add(item);
                            break;
                        }
                    }
                }
                temp.Add(timeEntries.FirstOrDefault(i => i.HoursTypeID == 0));
                return temp;
            }
            return null;
        }


        private string EmployeeTimesheetStatus(int employeeInfoId)
        {
            return string.Empty;
        }

        /// <summary>
        /// Return list of employees that were delegated to Individual
        /// </summary>
        /// <param name="delegatedToIndividualId"></param>
        /// <returns></returns>
        public List<EmployeeIndividualViewModel> GetEmployeesDelegatedToIndividual(int delegatedToIndividualId)
        {
            var individualIDs = repository.GetDelegatorIndividualIds(delegatedToIndividualId);
            var result = new List<EmployeeIndividualViewModel>();
            foreach (var item in individualIDs)
            {
                var individual = repository.GetIndividualByIndividualId(item);
                var employeeIndividual = repository.GetEmployeeIndividual(individual);
                result.Add(AutoMapper.Mapper.Map(employeeIndividual, new EmployeeIndividualViewModel(employeeIndividual)));
            }
            return result;
        }


        private TimesheetViewModel GetTimesheetsForIndividual(int id, string selectedYear = "", int selectedPayPeriodId = 0)
        {
            TimesheetViewModel model = new TimesheetViewModel();
            if (id > 0)
            {
                //Get employee individual based on the param id
                var individual = repository.GetIndividualByIndividualId(id);
                var employeeIndividual = repository.GetEmployeeIndividual(individual);
                var employeeCompanyCode = repository.GetCompanyCodes().FirstOrDefault(i => i.CompanyCodeID == employeeIndividual.EmployeeInformationDto.CompanyCodeId);

                model.EmployeeIndividual = AutoMapper.Mapper.Map(employeeIndividual, new EmployeeIndividualViewModel(employeeIndividual));
                model.IsUserNonExempt = employeeIndividual.EmployeeInformationDto.IsNonExempt ?? true;
                //----------------------------------------------//
                // Time Entry Summary Panel
                //-----------------------------------------------// 
                if (string.IsNullOrEmpty(selectedYear) && selectedPayPeriodId > 0)
                {
                    var selectedPayPeriod = repository.GetPayPeriodByPayPeriodId(selectedPayPeriodId);
                    selectedYear = selectedPayPeriod.dtmPeriodEnd.Year.ToString();
                }
                //Get YTD start and end date
                DateTime ytdStartDate = new DateTime();
                DateTime ytdEndDate = new DateTime();
                var defaultStartDate = "1/1/" + (string.IsNullOrEmpty(selectedYear) ? DateTime.Now.Year.ToString() : selectedYear);

                //for exempt - retrieve all time entries by default
                if (!model.IsUserNonExempt)
                {
                    ytdStartDate = DateTime.Parse("1/1/1900");
                    ytdEndDate = DateTime.Parse("12/31/5000");
                }
                else
                {
                    var selectedStartDate = DateTime.Parse(defaultStartDate);
                    repository.DatesYTD(selectedStartDate, out ytdStartDate, out ytdEndDate);
                }

                //For NonExempt
                //  - get the first open pay period based on ascending order of the selected year; if none exist then display the last processed pay period as the selected pay period
                //  - get the list of pay periods of the selected year
                //  - list of time entries of the selected pay period
                PayPeriodViewModel payperiod = new PayPeriodViewModel();
                List<TimesheetHoursViewModel> selectedPayPeriodTimeEntries = new List<TimesheetHoursViewModel>();
                ////Get the Current Pay Period
                model.YearAndPayPeriod = GetYearAndPayPeriodViewModel(); //TODO: what if null is return? (this is use to get the current pay period, if it exists)
                
                var payperiodsBySelectedYear = GetPayPeriodsList(string.IsNullOrEmpty(selectedYear) ?
                                               DateTime.Now.Year : int.Parse(selectedYear)).OrderByDescending(i => i.dtmPeriodEnd).ToList();
                if (payperiodsBySelectedYear.Count() > 0)
                {
                    var openPayPeriod = payperiodsBySelectedYear.Where(i => i.txtStatus == "Open")
                                            .OrderBy(i => i.dtmPeriodEnd).FirstOrDefault();
                    var processedPayPeriod = payperiodsBySelectedYear.Where(i => i.txtStatus != "Open")
                                            .OrderBy(i => i.dtmPeriodEnd).FirstOrDefault();

                    payperiod = openPayPeriod != null ? openPayPeriod : processedPayPeriod;
                    if (selectedPayPeriodId != 0)
                    {
                        payperiod = payperiodsBySelectedYear.FirstOrDefault(i => i.PayPeriodID == selectedPayPeriodId);
                    }
                    model.SelectedYearPayPeriods = GetPayPeriodsListItems(payperiodsBySelectedYear);
                    model.SelectedPayPeriod = payperiod;
                    model.SelectedPayPeriodID = payperiod.PayPeriodID;
                }
                else
                {
                    model.SelectedYearPayPeriods = null;
                    model.SelectedPayPeriod = null;
                    model.SelectedPayPeriodID = -1;
                    payperiod.dtmPeriodStart = DateTime.Parse("1/1/" + selectedYear);
                    payperiod.dtmPeriodEnd = payperiod.dtmPeriodStart.AddDays(13); //two weeks; days starts at zero
                }
                model.SelectedPayPeriodYear = payperiodsBySelectedYear.Count() > 0 ? payperiod.dtmPeriodEnd.Year : int.Parse(selectedYear); //TODO: handle when no payperiod exist
                model.EmployeeInfoId = employeeIndividual.EmployeeInfoId;

                selectedPayPeriodTimeEntries = GetEmployeeTimeEntriesByDateRange(payperiod.dtmPeriodStart, payperiod.dtmPeriodEnd,
                                               employeeIndividual.EmployeeInfoId, model.IsUserNonExempt)
                                                .Where(i => i.HoursTypeID != (int)Enumeration.HoursType.Mileage).ToList();

                //OnCall
                var onCallEntries = GetEmployeeOnCallEntriesByDateRange(payperiod.dtmPeriodStart, payperiod.dtmPeriodEnd, employeeIndividual.EmployeeInfoId);
                model.OnCallEntries = onCallEntries;
                model.OnCallSummaryNonExempt = onCallEntries != null && onCallEntries.Count() > 0 ?
                                               GetEmployeeOnCallSummary(payperiod.dtmPeriodStart, payperiod.dtmPeriodEnd, onCallEntries) : null;

                //Mileage
                var mileageEntries = GetMileageEntriesByPayPeriod(model.SelectedPayPeriodID, model.EmployeeInfoId);
                model.MileageEntries = mileageEntries != null && mileageEntries.Count() > 0 ? mileageEntries : null;
                model.MileageSummary = mileageEntries != null && mileageEntries.Count() > 0 ?
                                       GetMileageSummaryBasedOnSelectedPayPeriod(model.SelectedPayPeriodID, mileageEntries) : null;

                //Get PTO Balances
                model.PTOBalances = GetPTOBalances(employeeIndividual.EmployeeInformationDto.FileNumber,
                                                   employeeCompanyCode.txtCompanyCode);

                if (model.EmployeeIndividual.FLSAStatus != -1)
                {
                    if (model.IsUserNonExempt && model.EmployeeIndividual.FLSAStatus == 1)
                    {
                        //NonExempt

                        //Pay Period Year list
                        model.PayPeriodYearList = PayPeriodYearListItems(model.IsUserNonExempt, individual.IndividualID);

                        var weekOneEndDate = payperiod.dtmPeriodStart.AddDays(6); //Ends on Saturday
                        var weekTwoStartDate = weekOneEndDate.AddDays(1); //Starts on Sunday
                        var week1 = selectedPayPeriodTimeEntries.Where(m => m.Date >= payperiod.dtmPeriodStart &&
                                                                            m.Date <= weekOneEndDate).OrderBy(i => i.Date).ToList();
                        var week2 = selectedPayPeriodTimeEntries.Where(m => m.Date >= weekTwoStartDate &&
                                                                            m.Date <= payperiod.dtmPeriodEnd).OrderBy(i => i.Date).ToList();
                        if (model.EmployeeIndividual.FLSAStatus == 1)
                        {
                            if (selectedPayPeriodTimeEntries != null)
                            {
                                model.NonExemptTimesheetStatus = selectedPayPeriodTimeEntries.Count > 0 ?
                                                                 selectedPayPeriodTimeEntries.First().tblTSStatusType.StatusType : "Non-Submitted";
                            }
                        }
                        model.WeekOneTimeEntries = employeeIndividual.FLSAStatus == 1 ? week1 : null;
                        model.WeekTwoTimeEntries = employeeIndividual.FLSAStatus == 1 ? week2 : null;
                        model.WeeklyDateRange.WeekOneEndDate = weekOneEndDate;
                        model.WeeklyDateRange.WeekOneStartDate = payperiod.dtmPeriodStart;
                        model.WeeklyDateRange.WeekTwoEndDate = payperiod.dtmPeriodEnd;
                        model.WeeklyDateRange.WeekTwoStartDate = weekTwoStartDate;
                        model.TimesheetHours = GetEmployeeTimeEntriesByDateRange(payperiod.dtmPeriodStart, payperiod.dtmPeriodEnd,
                                               employeeIndividual.EmployeeInfoId, model.IsUserNonExempt); //non-exempt two-week time entries
                        model.WeekOneTimeEntries = UpdateOnCallTimeEntries(true, model.WeeklyDateRange.WeekOneStartDate,
                                                   model.WeeklyDateRange.WeekTwoEndDate, model.WeekOneTimeEntries, onCallEntries);
                        model.WeekTwoTimeEntries = UpdateOnCallTimeEntries(true, model.WeeklyDateRange.WeekOneStartDate,
                                                   model.WeeklyDateRange.WeekTwoEndDate, model.WeekTwoTimeEntries, onCallEntries);
                        //Get Time Off Hours Type Summary for YTD Time Entries
                        var ytdTimeEntries = GetEmployeeTimeEntriesByDateRange(ytdStartDate, ytdEndDate,
                                             employeeIndividual.EmployeeInfoId, model.IsUserNonExempt);
                        model.TimeOffSummaryYTD = GetNonRegularHoursTypeSummary(ytdTimeEntries)
                                                  .OrderByDescending(i => i.HoursTypeName).ToList();

                        var timeOffSummaryNonExemptTimeEntries = selectedPayPeriodTimeEntries != null &&
                                                                selectedPayPeriodTimeEntries.Count() > 0 ?
                                                                selectedPayPeriodTimeEntries.Where(i => i.HoursTypeID != (int)Enumeration.HoursType.OnCallHoliday && i.HoursTypeID != (int)Enumeration.HoursType.OnCallRegular).ToList() : null;
                        var timeOffSummaryNonExempt = GetTimeOffSummaryNonExempt(payperiod.dtmPeriodStart, payperiod.dtmPeriodEnd, timeOffSummaryNonExemptTimeEntries);
                        model.TimeOffSummaryNonExempt = timeOffSummaryNonExemptTimeEntries != null &&
                            timeOffSummaryNonExemptTimeEntries.Count() > 0 &&
                            timeOffSummaryNonExempt != null ? timeOffSummaryNonExempt.OrderByDescending(i => i.HoursType).ToList() : null;
                    }
                    if (!model.IsUserNonExempt && model.EmployeeIndividual.FLSAStatus == 0)
                    {
                        //Exempt
                        var exemptTimeEntries = GetEmployeeTimeEntriesByDateRange(ytdStartDate, ytdEndDate,
                                                model.EmployeeInfoId, model.IsUserNonExempt);
                        var exemptYtdEntries = exemptTimeEntries != null ? exemptTimeEntries.Where(i => i.Date.Year == DateTime.Now.Year).ToList() : null;
                        model.TimesheetHours = !string.IsNullOrEmpty(selectedYear) && exemptTimeEntries.Count() > 0 ? exemptTimeEntries.Where(i => i.Date.Year == int.Parse(selectedYear)).ToList() :
                                                exemptTimeEntries != null && exemptTimeEntries.Count() > 0 ? exemptTimeEntries : null; //exempt ytd time entries
                        model.TimeEntriesYTD = !string.IsNullOrEmpty(selectedYear) ? exemptTimeEntries.Where(i => i.Date.Year == int.Parse(selectedYear)).ToList() :
                                                exemptTimeEntries != null && exemptTimeEntries.Count() > 0 ? exemptYtdEntries : null;
                        model.TimeOffSummaryYTD = exemptTimeEntries != null ? GetNonRegularHoursTypeSummary(exemptYtdEntries).OrderByDescending(i => i.HoursTypeName).ToList() : null;
                        model.PayPeriodYearList = PayPeriodYearListItems(model.IsUserNonExempt, individual.IndividualID);
                    }

                }

                var defaultValues = GetTimesheetDefaultValues(employeeIndividual.EmployeeInformationDto.IsNonExempt ?? true);
                model.HoursType = defaultValues.HoursType;
                model.PickerHourMinuteInDecimal = defaultValues.PickerHourMinuteInDecimal;
                model.PickerHours = defaultValues.PickerHours;
                model.PickerMinutes = defaultValues.PickerMinutes;
                model.HoursTypeOnCall = defaultValues.HoursTypeOnCall;

                return model;
            }
            return null;
        }

        private List<EmployeeIndividualViewModel> GetDirectEmployees(int individualId)
        {
            var directEmployees = GetManagersEmployeeIndividuals(individualId);
            return directEmployees;
        }

        private void SendEmailProcess(SelectedTimeEntryViewModel SelectedTimeEntries, EmployeeIndividualViewModel employeeIndividual, int approverMasterUserId, List<TimesheetHoursViewModel> selTimesheetHoursEntries, int timesheetAction = 0)
        {
            if (SelectedTimeEntries != null)
            {
                if (timesheetAction > 0)
                {
                    int EmailContentId = -1;
                    if (employeeIndividual.EmployeeInformation.IsNonExempt ?? true || employeeIndividual.FLSAStatus == 1)
                    {
                        EmailContentId = timesheetAction == (int)Enumeration.TimesheetAction.Submit ? int.Parse(ConfigurationManager.AppSettings["TimesheetSubmissionToManager_NonExempt"]) :
                            timesheetAction == (int)Enumeration.TimesheetAction.Reject ? int.Parse(ConfigurationManager.AppSettings["TimesheetRejectTimeEmail_NonExempt"]) :
                            timesheetAction == (int)Enumeration.TimesheetAction.Approve ? int.Parse(ConfigurationManager.AppSettings["TimesheetApprovedToEmployee_NonExempt"]) : -1;
                    }
                    else if (!employeeIndividual.EmployeeInformation.IsNonExempt ?? true || employeeIndividual.FLSAStatus == 0)
                    {
                        EmailContentId = timesheetAction == (int)Enumeration.TimesheetAction.Submit ? int.Parse(ConfigurationManager.AppSettings["TimesheetSubmissionToManager_Exempt"]) :
                            timesheetAction == (int)Enumeration.TimesheetAction.Reject ? int.Parse(ConfigurationManager.AppSettings["TimesheetRejectTimeEmail_Exempt"]) :
                            timesheetAction == (int)Enumeration.TimesheetAction.Approve ? int.Parse(ConfigurationManager.AppSettings["TimesheetApprovedToEmployee_Exempt"]) : -1;
                    }
                    if (EmailContentId > 0)
                    {
                        //SelectedDelegateIds includes manager and/or designated delegates 
                        var payperiodid = selTimesheetHoursEntries.Select(i => i.PayPeriodID).Distinct().FirstOrDefault();
                        var selectedPayPeriod = repository.GetPayPeriodByPayPeriodId(int.Parse(SelectedTimeEntries.SelectedPayPeriodId));
                        List<IndividualViewModel> delegates = new List<IndividualViewModel>();
                        if (SelectedTimeEntries.SelectedDelegateIds != null)
                        {
                            foreach (var item in SelectedTimeEntries.SelectedDelegateIds)
                            {
                                delegates.Add(AutoMapper.Mapper.Map(repository.GetIndividualByIndividualId(item), new IndividualViewModel()));
                            }
                        }
                        //EmailContentId = (bool)employeeIndividual.EmployeeInformation.IsNonExempt ? submissionToManager_NonExempt : submissionToManager_Exempt;
                        var FullName = String.Concat(employeeIndividual.FirstName, " ", employeeIndividual.LastName);
                        var emailContent = repository.GetEmailContentById(EmailContentId);
                        var totalHours = selTimesheetHoursEntries.Sum(i => i.Hours).ToString();
                        var buildHtmlString = string.Empty;

                        if (timesheetAction == (int)Enumeration.TimesheetAction.Submit || timesheetAction == (int)Enumeration.TimesheetAction.Approve)
                        {
                            if (employeeIndividual.FLSAStatus == 1) //NonExempt Summary
                            {
                                var onCallEntries = GetEmployeeOnCallEntriesByDateRange(selectedPayPeriod.dtmPeriodStart, selectedPayPeriod.dtmPeriodEnd, employeeIndividual.EmployeeInfoId);
                                var mileageEntries = GetMileageEntriesByPayPeriod(selectedPayPeriod.PayPeriodID, employeeIndividual.EmployeeInfoId);
                                var nonExemptEntries = GetTimeOffSummaryNonExempt(selectedPayPeriod.dtmPeriodStart, selectedPayPeriod.dtmPeriodEnd, selTimesheetHoursEntries);
                                var oncallSummary = GetEmployeeOnCallSummary(selectedPayPeriod.dtmPeriodStart, selectedPayPeriod.dtmPeriodEnd, onCallEntries);
                                var mileageSummary = GetMileageSummaryBasedOnSelectedPayPeriod(selectedPayPeriod.PayPeriodID, mileageEntries);
                                List<WeeklyHoursTypeViewModel> displayEntries = new List<WeeklyHoursTypeViewModel>();
                                List<WeeklyHoursTypeViewModel> oncallDisplayEntries = new List<WeeklyHoursTypeViewModel>();
                                List<WeeklyHoursTypeViewModel> mileageDisplayEntries = new List<WeeklyHoursTypeViewModel>();
                                foreach (var item in nonExemptEntries)
                                {
                                    displayEntries.Add(new WeeklyHoursTypeViewModel()
                                    {
                                        HoursType = item.HoursType,
                                        WeekOneTotal = item.TotalWeek1.ToString(),
                                        WeekTwoTotal = item.TotalWeek2.ToString(),
                                        Total = item.Total.ToString()
                                    });
                                }
                                if (oncallSummary != null)
                                {
                                    foreach (var item in oncallSummary)
                                    {
                                        oncallDisplayEntries.Add(new WeeklyHoursTypeViewModel()
                                        {
                                            HoursType = item.HoursType,
                                            WeekOneTotal = string.Format("{0:C}", item.TotalWeek1OnCall),
                                            WeekTwoTotal = string.Format("{0:C}", item.TotalWeek2OnCall),
                                            Total = string.Format("{0:C}", item.TotalOnCall)
                                        });
                                    }
                                }
                                if (mileageSummary != null)
                                {
                                    foreach (var item in mileageSummary)
                                    {
                                        mileageDisplayEntries.Add(new WeeklyHoursTypeViewModel()
                                        {
                                            HoursType = item.HoursType,
                                            WeekOneTotal = item.TotalWeek1Milage != null && item.TotalWeek1Milage > 0 ? item.TotalWeek1Milage.ToString() : "-",
                                            WeekTwoTotal = item.TotalWeek2Milage != null && item.TotalWeek2Milage > 0 ? item.TotalWeek2Milage.ToString() : "-",
                                            Total = item.TotalMilage.ToString()
                                        });
                                    }
                                }


                                var timeBuildHtmlString = displayEntries.Count() > 0 && displayEntries != null ? BuildHtmlString<WeeklyHoursTypeViewModel>(displayEntries.ToList(), new List<string>() { { "Type" }, { "Week One" }, { "Week Two" }, { "Total" } }).ToHtmlString() : string.Empty;
                                var oncallBuildHtmlString = oncallDisplayEntries.Count() > 0 && oncallDisplayEntries != null ? BuildHtmlString<WeeklyHoursTypeViewModel>(oncallDisplayEntries.ToList(), new List<string>() { { "Type" }, { "Week One" }, { "Week Two" }, { "Total" } }).ToHtmlString() : string.Empty;
                                var mileageBuildHtmlString = mileageDisplayEntries.Count() > 0 && mileageDisplayEntries != null ? BuildHtmlString<WeeklyHoursTypeViewModel>(mileageDisplayEntries.ToList(), new List<string>() { { "Type" }, { "Week One" }, { "Week Two" }, { "Total" } }).ToHtmlString() : string.Empty;
                                buildHtmlString = timeBuildHtmlString + "<br />" + oncallBuildHtmlString + "<br />" + mileageBuildHtmlString;
                            }
                            else
                            {
                                buildHtmlString = BuildHtmlString<DateTypeHoursItemViewModel>(DisplayDateTypeHoursBasedOnTimeEntries(selTimesheetHoursEntries).ToList()
                                                        , new List<string>() { { "Date" }, { "Type" }, { "Hours" } }).ToHtmlString();
                            }
                        }
                        else
                        {
                            buildHtmlString = BuildHtmlString<DateTypeHoursItemViewModel>(DisplayDateTypeHoursBasedOnTimeEntries(selTimesheetHoursEntries).ToList()
                                                    , new List<string>() { { "Date" }, { "Type" }, { "Hours" } }).ToHtmlString();
                        }

                        //View Timesheet Link

                        var viewTimesheetLink = repository.GetEmailContentById(int.Parse(ConfigurationManager.AppSettings["TimesheetLink_ViewTimesheet"]));
                        var approvalContent = repository.GetEmailContentById(int.Parse(ConfigurationManager.AppSettings["TimesheetLink_ApprovalLink"]));
                        var rejectContent = repository.GetEmailContentById(int.Parse(ConfigurationManager.AppSettings["TimesheetLink_RejectLink"]));
                        var timesheetHomeLink = repository.GetEmailContentById(int.Parse(ConfigurationManager.AppSettings["TimesheetLink_HomePage"]));

                        var buildQueryString = ConfigurationManager.AppSettings["BuildQueryStringLink"];
                        //Approval and Reject Links: sample of querystring -->  ?app=timesheets&cmd=Approve&EmpInfoId=1&ppId=13&MgrId=5924&Ids=13251
                        string approvalLink = approvalContent.Subject + buildQueryString + "Approve";
                        string rejectLink = rejectContent.Subject + buildQueryString + "Reject";
                        string timesheetIds = "&Ids=";
                        if (!employeeIndividual.EmployeeInformation.IsNonExempt ?? true)//only for exempt employees: add timesheetHoursId to param
                        {
                            var tsIds = string.Empty;
                            foreach (var item in selTimesheetHoursEntries)
                            {
                                tsIds += item.TimesheetHoursID.ToString() + ",";
                            }
                            timesheetIds += tsIds.EndsWith(",") ? tsIds.Remove(tsIds.LastIndexOf(',')) : tsIds;
                        }
                        var param = "&EmpInfoId=" + employeeIndividual.EmployeeInfoId + "&ppId=" + payperiodid;
                        var viewTimesheetParams = "?individualId=" + employeeIndividual.IndividualId + "&selectedYear=" + selectedPayPeriod.dtmPeriodStart.Year + "&selectedPayPerioId=" + selectedPayPeriod.PayPeriodID;
                        approvalLink += param;
                        rejectLink += param;

                        var emailFrom = employeeIndividual.Individual.Email;
                        var approvedOrRejectedBy = string.Empty;
                        if (timesheetAction == (int)Enumeration.TimesheetAction.Approve || timesheetAction == (int)Enumeration.TimesheetAction.Reject)
                        {
                            //send email to employee from manager
                            var individualManager = repository.GetIndividualByMasterUserId(approverMasterUserId);

                            delegates.Clear();
                            delegates.Add(employeeIndividual.Individual);//send back to employee
                            emailFrom = individualManager.Email; //reject or approve by manager
                            approvedOrRejectedBy = string.Format("{0} {1}", individualManager.FirstName, individualManager.LastName);
                            emailContent.Body = emailContent.Body.Replace("##ApprovedBy##", approvedOrRejectedBy);
                        }

                        //Sends submit email for approval to each selected delegates and the first one to approve or reject wins     
                        var payPeriodDates = string.Format("{0} - {1}", selectedPayPeriod.dtmPeriodStart.ToShortDateString(), selectedPayPeriod.dtmPeriodEnd.ToShortDateString());
                        foreach (var item in delegates)
                        {
                            var emailTo = item.Email;
                            //TODO: Verify this function from HR
                            //if delegate doesn't have email then use default HR Email (stored in web.config)
                            if (String.IsNullOrEmpty(emailTo)) { emailTo = defaultHREmail; }
                            if (String.IsNullOrEmpty(emailFrom)) { emailFrom = defaultHREmail; }
                            approvalLink += "&MgrId=" + item.IndividualID + timesheetIds;
                            rejectLink += "&MgrId=" + item.IndividualID + timesheetIds;
                            emailContent.Subject = emailContent.Subject.Replace("##EmployeeName##", FullName);
                            emailContent.Body = emailContent.Body.Replace("##PeriodDueDate##", selectedPayPeriod.dtmPeriodDue.ToShortDateString())
                                    .Replace("##PeriodPayDate##", selectedPayPeriod.dtmPeriodPayDay.ToShortDateString())
                                    .Replace("##PayPeriodDates##", payPeriodDates)
                                    .Replace("##ViewTimesheetLink##", viewTimesheetLink.Subject + viewTimesheetParams)
                                    .Replace("##EmployeeName##", FullName)
                                    .Replace("##TimeSummary##", buildHtmlString)
                                    .Replace("##TotalHours##", totalHours)
                                    .Replace("##ApprovalLink##", approvalLink)
                                    .Replace("##RejectLink##", rejectLink)
                                    .Replace("##TimesheetHomeLink##", timesheetHomeLink.Subject)
                                    .Replace("##EmployeeComments##", !String.IsNullOrEmpty(SelectedTimeEntries.Comments) ? SelectedTimeEntries.Comments.ToString() : string.Empty);
                            //Send Email
                            SendEmail(emailTo, emailFrom, emailContent.Subject, emailContent.Body);
                        }
                    }
                }
            }
        }

        private List<IndividualDelegateViewModel> GetDelegatesToIndividual(int individualId)
        {
            if (individualId > 0)
            {
                var result = AutoMapper.Mapper.Map(repository.GetDelegatesByIndividualId(individualId), new List<IndividualDelegateViewModel>());
                foreach (var item in result)
                {
                    item.DelegateToIndividual = AutoMapper.Mapper.Map(repository.GetEmployeeIndividualById(item.DelegateToIndividualID), new EmployeeIndividualViewModel());
                }
                return result;
            }
            return null;
        }

        /// <summary>
        /// Time Off Summary
        /// </summary>
        /// <returns></returns>
        private static List<TimeOffSummaryViewModel> GetTimeOffSummary(List<TimesheetHoursViewModel> timesheetHoursViewModel = null)
        {
            List<TimeOffSummaryViewModel> result = new List<TimeOffSummaryViewModel>();
            //Non-Regular hours
            if (timesheetHoursViewModel != null)
            {
                foreach (Enumeration.StatusType stat in Enum.GetValues(typeof(Enumeration.StatusType)))
                {
                    var totalByStatusType = timesheetHoursViewModel.Where(i => i.StatusTypeID == (int)stat & i.EntryDate.Year == DateTime.Now.Year & i.HoursTypeID != (int)Enumeration.HoursType.Regular).Sum(i => i.Hours);
                    result.Add(new TimeOffSummaryViewModel() { StatusType = stat, Total = totalByStatusType });
                }
                return result;
            }
            return null;
        }


        private PTOBalanceViewModel GetPTOBalances(string FileNumber, string txtCompanyCode)
        {
            var result = AutoMapper.Mapper.Map(repository.GetPTOBalanceByEmployeeFileNumber(FileNumber, txtCompanyCode), new PTOBalanceViewModel());
            return result;
        }

        private List<TimeOffSummaryViewModel> GetNonRegularHoursTypeSummary(List<TimesheetHoursViewModel> timesheetEntries)
        {
            //Non-regulars hours only i.e no regular or overtime enum hour type
            List<TimeOffSummaryViewModel> result = new List<TimeOffSummaryViewModel>();
            foreach (Enumeration.HoursType item in Enum.GetValues(typeof(Enumeration.HoursType)))
            {
                var summary = new TimeOffSummaryViewModel();
                if (item != Enumeration.HoursType.Regular && item != Enumeration.HoursType.Overtime
                    && item != Enumeration.HoursType.Mileage
                    && item != Enumeration.HoursType.OnCallRegular
                    && item != Enumeration.HoursType.OnCallHoliday)
                {
                    var byHourType = timesheetEntries.Where(i => i.HoursTypeID == (int)item);
                    var total = byHourType.Sum(u => u.Hours);
                    if (total > 0)
                    {
                        summary.HoursType = item;
                        summary.HoursTypeID = (int)item;
                        summary.Total = total;
                        summary.HoursTypeName = byHourType.FirstOrDefault().tblTSHoursType.HoursType;
                        foreach (Enumeration.StatusType stat in Enum.GetValues(typeof(Enumeration.StatusType)))
                        {
                            var totalByStatusType = byHourType.Where(i => i.StatusTypeID == (int)stat).Sum(i => i.Hours);
                            summary.Submitted = stat == Enumeration.StatusType.Submitted ? totalByStatusType : summary.Submitted != 0 ? summary.Submitted : 0;
                            summary.NonSubmitted = stat == Enumeration.StatusType.NonSubmitted ? totalByStatusType : summary.NonSubmitted != 0 ? summary.NonSubmitted : 0;
                            summary.Processed = stat == Enumeration.StatusType.Processed ? totalByStatusType : summary.Processed != 0 ? summary.Processed : 0;
                            summary.Approved = stat == Enumeration.StatusType.Approved ? totalByStatusType : summary.Approved != 0 ? summary.Approved : 0;
                        }
                        result.Add(summary);
                    }
                }
            }

            //Add total summary at the bottom of the grid
            var sumNonSubmitted = result.Sum(i => i.NonSubmitted);
            var sumSubmitted = result.Sum(i => i.Submitted);
            var sumApproved = result.Sum(i => i.Approved);
            var sumProcessed = result.Sum(i => i.Processed);
            var sumTotal = result.Sum(i => i.Total);

            result.Add(new TimeOffSummaryViewModel()
            {
                HoursTypeName = "Total",
                NonSubmitted = sumNonSubmitted,
                Submitted = sumSubmitted,
                Approved = sumApproved,
                Processed = sumProcessed,
                Total = sumTotal
            });

            return result;
        }


        private List<TSHoursTypeSummaryViewModel> GetHoursTypeSummary(List<TimesheetHoursViewModel> timesheetHoursViewModel)
        {

            List<TSHoursTypeSummaryViewModel> result = new List<TSHoursTypeSummaryViewModel>();
            foreach (Enumeration.HoursType item in Enum.GetValues(typeof(Enumeration.HoursType)))
            {
                var summary = new TSHoursTypeSummaryViewModel();
                if (item != Enumeration.HoursType.Regular && item != Enumeration.HoursType.Overtime)
                {
                    var byHourType = timesheetHoursViewModel.Where(i => i.HoursTypeID == (int)item);
                    var total = byHourType.Sum(u => u.Hours);
                    var milesTotal = byHourType.Sum(u => u.MileageMiles);

                    //summary.HoursType = item;
                    //summary.Total = totalhours.Sum(i => i.total);
                    //summary.HoursTypeName = byHourType.FirstOrDefault().tblTSHoursType.HoursType;

                    if (total > 0)
                    {
                        summary.HoursType = item;
                        summary.Total = total;
                        summary.HoursTypeName = byHourType.FirstOrDefault().tblTSHoursType.HoursType;
                        foreach (Enumeration.StatusType stat in Enum.GetValues(typeof(Enumeration.StatusType)))
                        {
                            var totalByStatusType = byHourType.Where(i => i.StatusTypeID == (int)stat).Sum(i => i.Hours);
                            summary.Submitted = stat == Enumeration.StatusType.Submitted ? totalByStatusType : summary.Submitted != 0 ? summary.Submitted : 0;
                            summary.NonSubmitted = stat == Enumeration.StatusType.NonSubmitted ? totalByStatusType : summary.NonSubmitted != 0 ? summary.NonSubmitted : 0;
                            summary.Processed = stat == Enumeration.StatusType.Processed ? totalByStatusType : summary.Processed != 0 ? summary.Processed : 0;
                            summary.Approved = stat == Enumeration.StatusType.Approved ? totalByStatusType : summary.Approved != 0 ? summary.Approved : 0;
                        }
                        result.Add(summary);
                    }
                    else if (milesTotal > 0)
                    {
                        summary.HoursType = item;
                        summary.Total = (decimal)milesTotal;
                        summary.HoursTypeName = byHourType.FirstOrDefault().tblTSHoursType.HoursType;
                        foreach (Enumeration.StatusType stat in Enum.GetValues(typeof(Enumeration.StatusType)))
                        {
                            var totalByStatusType = byHourType.Where(i => i.StatusTypeID == (int)stat).Sum(i => i.Hours);
                            summary.Submitted = stat == Enumeration.StatusType.Submitted ? totalByStatusType : summary.Submitted != 0 ? summary.Submitted : 0;
                            summary.NonSubmitted = stat == Enumeration.StatusType.NonSubmitted ? totalByStatusType : summary.NonSubmitted != 0 ? summary.NonSubmitted : 0;
                            summary.Processed = stat == Enumeration.StatusType.Processed ? totalByStatusType : summary.Processed != 0 ? summary.Processed : 0;
                            summary.Approved = stat == Enumeration.StatusType.Approved ? totalByStatusType : summary.Approved != 0 ? summary.Approved : 0;
                        }
                        result.Add(summary);
                    }
                }
                else
                {
                    var byHourType = timesheetHoursViewModel.Where(i => i.HoursTypeID == (int)item);
                    var total = byHourType.Sum(u => u.Hours);
                    if (total > 40)
                    {
                        //overtime
                    }
                    else
                    {
                        summary.HoursType = item;
                        summary.Total = total;
                        summary.HoursTypeName = byHourType.FirstOrDefault().tblTSHoursType.HoursType;
                        foreach (Enumeration.StatusType stat in Enum.GetValues(typeof(Enumeration.StatusType)))
                        {
                            var totalByStatusType = byHourType.Where(i => i.StatusTypeID == (int)stat).Sum(i => i.Hours);
                            summary.Submitted = stat == Enumeration.StatusType.Submitted ? totalByStatusType : summary.Submitted != 0 ? summary.Submitted : 0;
                            summary.NonSubmitted = stat == Enumeration.StatusType.NonSubmitted ? totalByStatusType : summary.NonSubmitted != 0 ? summary.NonSubmitted : 0;
                            summary.Processed = stat == Enumeration.StatusType.Processed ? totalByStatusType : summary.Processed != 0 ? summary.Processed : 0;
                            summary.Approved = stat == Enumeration.StatusType.Approved ? totalByStatusType : summary.Approved != 0 ? summary.Approved : 0;
                        }
                        result.Add(summary);
                    }
                }
            }
            return result;
        }

        private List<TSMileageViewModel> GetMileageEntriesByPayPeriod(int selectedPayPeriodId, int employeeInfoId)
        {
            List<TSMileageViewModel> mileageEntries = new List<TSMileageViewModel>();
            var tsMileageEntries = repository.GetTimeEntriesBySelectedPayPeriodId(selectedPayPeriodId, employeeInfoId, (int)Enumeration.HoursType.Mileage);
            var groupDateEntries = tsMileageEntries.Distinct().Select(i => i.Date);
            List<SelectListItem> mileageRates = new List<SelectListItem>();
            foreach (var item in groupDateEntries)
            {
                var effectiveRate = repository.GetHoursTypeEffectiveRateBasedOnDate(item, (int)Enumeration.HoursType.Mileage);
                mileageRates.Add(new SelectListItem() { Text = item.ToShortDateString(), Value = effectiveRate.Rate.ToString() });
            }
            foreach (var item in tsMileageEntries)
            {
                mileageEntries.Add(new TSMileageViewModel()
                {
                    TimesheetHoursID = item.TimesheetHoursID,
                    Date = item.Date,
                    MileageMiles = (float)item.MileageMiles,
                    MileageFrom = item.MileageFrom,
                    MileageTo = item.MileageTo,
                    MileageDescription = item.MileageDescription,
                    HoursTypeID = item.HoursTypeID,
                    StatusTypeID = item.StatusTypeId,
                    EmployeeInfoID = item.EmployeeInfoId,
                    PayPeriodID = item.PayPeriodID,
                    tblTSHoursType = AutoMapper.Mapper.Map(item.tblTSHoursType, new TSHoursTypeViewModel()),
                    tblTSStatusType = AutoMapper.Mapper.Map(item.tblTSStatusType, new TSStatusTypeViewModel())
                });
            }
            foreach (var item in mileageEntries)
            {
                foreach (var rate in mileageRates)
                {
                    if (item.Date.ToShortDateString() == rate.Text)
                    {
                        item.MileageEffectiveDate = DateTime.Parse(rate.Text);
                        item.MileageRate = float.Parse(rate.Value);
                        break;
                    }
                }
            }
            return mileageEntries.OrderBy(m => m.Date).ToList();
        }

        private List<TSMileageViewModel> GetMileageEntriesByDate(DateTime startDate, DateTime endDate, int employeeIndividualInfoID)
        {
            List<TSMileageViewModel> mileageEntries = new List<TSMileageViewModel>();
            var tsMileageEntries = GetEmployeeMileageEntriesByDateRange(startDate, endDate, employeeIndividualInfoID);
            var groupDateEntries = tsMileageEntries.Distinct().Select(i => i.Date);
            List<SelectListItem> mileageRates = new List<SelectListItem>();
            foreach (var item in groupDateEntries)
            {
                var effectiveRate = repository.GetHoursTypeEffectiveRateBasedOnDate(item, (int)Enumeration.HoursType.Mileage);
                mileageRates.Add(new SelectListItem() { Text = item.ToShortDateString(), Value = effectiveRate.Rate.ToString() });
            }
            foreach (var item in tsMileageEntries)
            {
                mileageEntries.Add(new TSMileageViewModel()
                {
                    TimesheetHoursID = item.TimesheetHoursID,
                    Date = item.Date,
                    MileageMiles = (float)item.MileageMiles,
                    MileageFrom = item.MileageFrom,
                    MileageTo = item.MileageTo,
                    MileageDescription = item.MileageDescription,
                    HoursTypeID = item.HoursTypeID,
                    StatusTypeID = item.StatusTypeID,
                    EmployeeInfoID = item.EmployeeInfoID,
                    PayPeriodID = item.PayPeriodID,
                    tblTSHoursType = item.tblTSHoursType,
                    tblTSStatusType = item.tblTSStatusType,

                });
            }
            foreach (var item in mileageEntries)
            {
                foreach (var rate in mileageRates)
                {
                    if (item.Date.ToShortDateString() == rate.Text)
                    {
                        item.MileageEffectiveDate = DateTime.Parse(rate.Text);
                        item.MileageRate = float.Parse(rate.Value);
                        break;
                    }
                }
            }
            return mileageEntries.OrderBy(m => m.Date).ToList();
        }


        private TimesheetViewModel GetTimesheetDefaultValues(bool? IsNonExempt)
        {
            var payperiods = GetYearAndPayPeriodViewModel();
            //if employee is non-applicable the value is null
            var isUserNonExempt = IsNonExempt ?? true;
            var timesheetViewModel = new TimesheetViewModel()
            {
                HoursType = isUserNonExempt ? GetHoursTypeNonExemptList() : GetHoursTypeForExempt(),
                PickerHours = GetPickerHours(),
                PickerMinutes = GetPickerMinutes(),
                PickerHourMinuteInDecimal = GetPickerTimeHourMinuteInDecimal(),
                YearAndPayPeriod = payperiods,
                SelectedPayPeriodID = payperiods.CurrentPayPeriod != null ? payperiods.CurrentPayPeriod.PayPeriodID : 0,
                HoursTypeOnCall = GetHoursTypeForOnCall(),
                IsUserNonExempt = isUserNonExempt
            };
            return timesheetViewModel;
        }

        private TimesheetViewModel GetEmployeeTimesheetInitialInfo(EmployeeIndividualViewModel employee)
        {
            //1. Get default values for picker controls
            //2. Current pay periods time entries
            //3. TimeOff Summary
            //4. Mileage Entries
            //5. PTO Balances

            var model = GetTimesheetDefaultValues(employee.EmployeeInformation.IsNonExempt);
            var startDate = model.YearAndPayPeriod.CurrentPayPeriod.dtmPeriodStart;
            var endDate = model.YearAndPayPeriod.CurrentPayPeriod.dtmPeriodEnd;
            DateTime ytdStartDate = new DateTime();
            DateTime ytdEndDate = new DateTime();
            if (!model.IsUserNonExempt) //Exempt
            {
                ytdStartDate = DateTime.Parse(ConfigurationManager.AppSettings["YTDStartDate"].ToString());
                ytdEndDate = DateTime.Parse(ConfigurationManager.AppSettings["YTDEndDate"].ToString());
            }
            else //NonExempt
            {
                repository.DatesYTD(startDate, out ytdStartDate, out ytdEndDate);
            }
            var weekOneEndDate = startDate.AddDays(6); //Ends on Saturday
            var weekTwoStartDate = weekOneEndDate.AddDays(1); //Starts on Sunday
            model.WeeklyDateRange.WeekOneStartDate = startDate;
            model.WeeklyDateRange.WeekOneEndDate = weekOneEndDate;
            model.WeeklyDateRange.WeekTwoStartDate = weekTwoStartDate;
            model.WeeklyDateRange.WeekTwoEndDate = endDate;
            model.EmployeeIndividual = employee;
            model.EmployeeInfoId = employee.EmployeeInfoId;
            model.MasterUserId = employee.MasterUserId;
            model.IsUserNonExempt = employee.EmployeeInformation.IsNonExempt ?? true;
            model.IsUserHRAdmin = employee.IsUserTSHRAdmin;

            var currentTimeEntries = GetEmployeeTimeEntriesByDateRange(startDate, endDate, employee.EmployeeInfoId, model.IsUserNonExempt).Where(i => i.HoursTypeID != (int)Enumeration.HoursType.Mileage).ToList();
            var onCallEntries = GetEmployeeOnCallEntriesByDateRange(startDate, endDate, employee.EmployeeInfoId);
            var ytdTimeEntries = GetEmployeeTimeEntriesByDateRange(ytdStartDate, ytdEndDate, employee.EmployeeInfoId, model.IsUserNonExempt);

            var yearAndPayPeriodViewModel = GetYearAndPayPeriodViewModel();

            var currentPayPeriod = yearAndPayPeriodViewModel.CurrentPayPeriod;
            var mileageEntries = GetMileageEntriesByPayPeriod(currentPayPeriod.PayPeriodID, employee.EmployeeInfoId); //GetMileageEntriesByDate(startDate, endDate, employee.EmployeeInfoId);
            var employeeCompanyCode = repository.GetCompanyCodes().FirstOrDefault(i => i.CompanyCodeID == employee.EmployeeInformation.CompanyCodeId);
            var timeOffSummaryNonExempt = GetTimeOffSummaryNonExempt(startDate, endDate, currentTimeEntries);
            var onCallSummaryNonExempt = GetEmployeeOnCallSummary(startDate, endDate, onCallEntries);


            model.MileageEntries = mileageEntries;
            model.OnCallEntries = onCallEntries;

            model.MileageSummary = GetMileageSummaryBasedOnSelectedPayPeriod(currentPayPeriod.PayPeriodID, mileageEntries);
            /* Notes for Timesheet entries:*/

            if (model.IsUserNonExempt)
            {
                //Non-Exempt: display current pay period time entries
                model.TimeOffSummaryNonExempt = timeOffSummaryNonExempt != null ? timeOffSummaryNonExempt.OrderByDescending(i => i.HoursType).ToList() : null;
                model.OnCallSummaryNonExempt = onCallSummaryNonExempt != null ? onCallSummaryNonExempt : null;
                model.WeekOneTimeEntries = UpdateOnCallTimeEntries(true, startDate, endDate,
                                            GetEmployeeTimeEntriesByDateRange(startDate, weekOneEndDate, employee.EmployeeInfoId, model.IsUserNonExempt)
                                            .Where(i => i.HoursTypeID != (int)Enumeration.HoursType.Mileage).ToList(), onCallEntries);
                model.WeekTwoTimeEntries = UpdateOnCallTimeEntries(true, startDate, endDate,
                                            GetEmployeeTimeEntriesByDateRange(weekTwoStartDate, endDate, employee.EmployeeInfoId, model.IsUserNonExempt)
                                            .Where(i => i.HoursTypeID != (int)Enumeration.HoursType.Mileage).ToList(), onCallEntries);
                model.TimesheetHours = currentTimeEntries;
                model.NonExemptTimesheetStatus = model.WeekOneTimeEntries.Count > 0 ? model.WeekOneTimeEntries.First().tblTSStatusType.StatusType :
                                 model.WeekTwoTimeEntries.Count > 0 ? model.WeekTwoTimeEntries.First().tblTSStatusType.StatusType : "Non-Submitted";


                model.PayPeriodYearList = PayPeriodYearListItems(model.IsUserNonExempt, model.EmployeeIndividual.IndividualId);

                var timeOffSummaryYTD = GetNonRegularHoursTypeSummary(ytdTimeEntries);
                model.TimeOffSummaryYTD = timeOffSummaryYTD != null ? timeOffSummaryYTD.OrderByDescending(i => (int)i.StatusType).ToList() : null;

            }
            else
            {
                //Exempt: display current YTD time entries 
                var timeOffSummaryExempt = GetNonRegularHoursTypeSummary(currentTimeEntries);
                //model.TimeEntriesYTD = ytdTimeEntries.Where(i => i.Date.Year == DateTime.Now.Year).ToList();
                model.TimesheetHours = ytdTimeEntries.Where(i => i.Date.Year == DateTime.Now.Year).ToList();
                model.TimeOffSummaryExempt = timeOffSummaryExempt != null ?
                    timeOffSummaryExempt.OrderByDescending(i => (int)i.HoursType).ToList() : null;


                model.PayPeriodYearList = PayPeriodYearListItems(model.IsUserNonExempt, model.EmployeeIndividual.IndividualId);
                var timeOffSummaryYTD = GetNonRegularHoursTypeSummary(ytdTimeEntries.Where(i => i.Date.Year == DateTime.Now.Year).ToList()); //TimeOffSummary Current Year
                model.TimeOffSummaryYTD = timeOffSummaryYTD != null ? timeOffSummaryYTD.OrderByDescending(i => (int)i.StatusType).ToList() : null;

            }

            //model.HoursTypeSummary = GetHoursTypeSummary(ytdTimeEntries);
            model.PTOBalances = GetPTOBalances(employee.EmployeeInformation.FileNumber, employeeCompanyCode.txtCompanyCode);
            model.YearAndPayPeriod = yearAndPayPeriodViewModel;
            model.YearAndPayPeriod.TimeYears = PayPeriodYearListItems(model.IsUserNonExempt, model.EmployeeIndividual.IndividualId);//GetListOfYearsByDate(employee.Individual.StartDate); //TODO: change the value here
            model.YearAndPayPeriod.IsNonExempt = employee.EmployeeInformation.IsNonExempt ?? true;
            model.SelectedPayPeriodID = currentPayPeriod.PayPeriodID;
            model.SelectedPayPeriod = currentPayPeriod;
            model.SelectedPayPeriodYear = currentPayPeriod.dtmPeriodEnd.Year;
            model.ManagerOfIndividual = GetIndividual(employee.Individual.ManagerIndividualID);
            model.HoursType = FilterHoursTypeByPTOEligibility(model.EmployeeIndividual.EmployeeInformation, model.HoursType);
            model.HoursTypeOnCall = GetHoursTypeForOnCall();
            return model;
        }

        private List<TimesheetHoursViewModel> UpdateOnCallTimeEntries(bool isNonExempt, DateTime startDate, DateTime endDate, List<TimesheetHoursViewModel> timeEntries, List<TSOnCallViewModel> oncallEntries)
        {
            if (isNonExempt)
            {
                var rates = repository.GetOnCallRateByDateRange(startDate, endDate);
                foreach (var entry in timeEntries)
                {
                    var oncallTimeEntry = oncallEntries.FirstOrDefault(m => m.Date == entry.Date && m.tblTSHoursType.HoursTypeID == entry.HoursTypeID);
                    if (oncallTimeEntry != null)
                    {
                        entry.OnCallDate = oncallTimeEntry.Date;

                        var rate = rates.Where(m => m.tblTSHourTypeRate.HourTypeId == oncallTimeEntry.HoursTypeID).OrderByDescending(i => i.EffectiveDate).ToList();
                        if (rate.Count > 0)
                            entry.OnCallDayRate = rate.FirstOrDefault(m => m.EffectiveDate <= oncallTimeEntry.Date).Rate;
                    }

                }

            }

            return timeEntries.OrderBy(m => m.Date).ToList();
        }

        private List<SelectListItem> FilterHoursTypeByPTOEligibility(EmployeeInformationViewModel model, List<SelectListItem> hoursTypeList)
        {
            //Filter hours type based on the Employment type and PTO eligiblity
            //  Intern and Temporary are non-exempt employees - if PTOEligible is false, regular hour type is the only option
            //  PartTime and FTE employees not PTOEligible will only get regular hour type option

            var result = new List<SelectListItem>();
            var internEmployment = int.Parse(ConfigurationManager.AppSettings["InternEmploymentType"]);
            var temporaryEmployment = int.Parse(ConfigurationManager.AppSettings["TemporaryEmploymentType"]);
            if ((!(bool)model.PTOEligible
                && (model.EmploymentTypeID == internEmployment ||
                        model.EmploymentTypeID == temporaryEmployment)
                && (bool)model.IsNonExempt) || (!(bool)model.PTOEligible))
            {
                foreach (var item in hoursTypeList)
                {
                    if (int.Parse(item.Value) == (int)Enumeration.HoursType.Regular)
                    {
                        result.Add(item);
                        break;
                    }
                }
            }
            else
            {
                result = hoursTypeList;
            }
            return result;
        }

        /// <summary>
        /// Return all employee time entries based on date range
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="employeeInfoId"></param>
        /// <returns></returns>
        private List<TimesheetHoursViewModel> GetEmployeeTimeEntriesByDateRange(DateTime startDate, DateTime endDate,
                                                                                int employeeInfoId, bool IsNonExempt = true)
        {
            List<Models.ViewModel.TimesheetHoursViewModel> result = new List<Models.ViewModel.TimesheetHoursViewModel>();

            var entries = AutoMapper.Mapper.Map(repository.GetTimeEntriesByDateRangeByEmployeeInfoId(startDate, endDate, employeeInfoId), new List<TimesheetHoursViewModel>());
            if (!IsNonExempt) //Exempt
            {
                foreach (var item in entries)
                {
                    if (item.HoursTypeID != (int)Enumeration.HoursType.Regular
                                 && item.HoursTypeID != (int)Enumeration.HoursType.Mileage
                                 && item.HoursTypeID != (int)Enumeration.HoursType.OnCallHoliday
                                 && item.HoursTypeID != (int)Enumeration.HoursType.OnCallRegular
                                 && item.HoursTypeID != (int)Enumeration.HoursType.Holiday)
                        result.Add(item);
                }
            }
            else //NonExempt
            {
                result = entries;
            }
            return result.OrderByDescending(i => i.Date).ToList();
        }

        /// <summary>
        /// Return all employee mileage entries based on date range
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="employeeInfoId"></param>
        /// <returns></returns>
        private List<TimesheetHoursViewModel> GetEmployeeMileageEntriesByDateRange(DateTime startDate, DateTime endDate, int employeeInfoId)
        {
            List<Models.ViewModel.TimesheetHoursViewModel> entries = new List<Models.ViewModel.TimesheetHoursViewModel>();
            entries = AutoMapper.Mapper.Map(repository.GetTimeEntriesByDateRangeByEmployeeInfoId(startDate, endDate, employeeInfoId), new List<TimesheetHoursViewModel>());
            return entries.OrderByDescending(i => i.StatusTypeID).ThenByDescending(i => i.Date).Where(i => i.HoursTypeID == 8).ToList();
        }


        /// <summary>
        /// Return all employee on Call entries based on date range
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="employeeInfoId"></param>
        /// <returns></returns>
        private List<TSOnCallViewModel> GetEmployeeOnCallEntriesByDateRange(DateTime startDate, DateTime endDate, int employeeInfoId)
        {
            List<Models.ViewModel.TSOnCallViewModel> entries = new List<Models.ViewModel.TSOnCallViewModel>();
            entries = AutoMapper.Mapper.Map(repository.GetTimeEntriesByDateRangeByEmployeeInfoId(startDate, endDate, employeeInfoId), new List<TSOnCallViewModel>());
            // return entries;
            //List<Models.ViewModel.TimesheetHoursViewModel> entries = new List<Models.ViewModel.TimesheetHoursViewModel>();
            //entries = AutoMapper.Mapper.Map(repository.GetTimeEntriesByDateRangeByEmployeeInfoId(startDate, endDate, employeeInfoId), new List<TimesheetHoursViewModel>());
            return entries.OrderByDescending(i => i.StatusTypeID).ThenByDescending(i => i.Date).Where(i => i.HoursTypeID == 7 || i.HoursTypeID == 11 || i.HoursTypeID == 12).ToList();
        }

        private List<TimeOffSummaryViewModel> GetTimeOffSummaryYTD(List<TimesheetHoursViewModel> entries)
        {
            List<TimeOffSummaryViewModel> ytdTimeOffSummary = new List<TimeOffSummaryViewModel>();
            var groupEntries = entries.GroupBy(i => i.StatusTypeID).Select(o => new { statusType = o.Key, Total = o.Sum(y => y.Hours) });

            foreach (var item in groupEntries)
            {
                ytdTimeOffSummary.Add(new TimeOffSummaryViewModel() { StatusType = (Enumeration.StatusType)item.statusType, Total = item.Total });
            }


            foreach (Enumeration.StatusType item in Enum.GetValues(typeof(Enumeration.StatusType)))
            {
                var recordexist = ytdTimeOffSummary.Where(i => i.StatusType.ToString() == item.ToString());
                if (recordexist.Count() == 0)
                {
                    ytdTimeOffSummary.Add(new TimeOffSummaryViewModel() { StatusType = item, Total = 0 });
                }
            }
            return ytdTimeOffSummary;
        }


        private List<HoursTypeSummaryViewModel> GetTimeOffSummaryNonExempt(DateTime startDate, DateTime endDate, List<TimesheetHoursViewModel> entries)
        {
            if (entries != null)
            {
                var week1StartDate = startDate;
                var week2StartDate = startDate.AddDays(7);
                var hrsTypes = repository.GetHoursTypeNonExempt();

                List<HoursTypeSummaryViewModel> timeOffSummaryTemp = new List<HoursTypeSummaryViewModel>();
                List<HoursTypeSummaryViewModel> timeOffSummary = new List<HoursTypeSummaryViewModel>();

                var entriesWeek1 = entries.Where(m => m.Date >= week1StartDate && m.Date < week2StartDate).ToList();
                var entriesWeek2 = entries.Where(m => m.Date >= week2StartDate && m.Date <= endDate).ToList();

                foreach (var item in hrsTypes)
                {
                    timeOffSummaryTemp.Add(new HoursTypeSummaryViewModel()
                    {
                        HoursType = item.HoursType,
                        HoursTypeID = item.HoursTypeID
                    });
                }

                foreach (var item in timeOffSummaryTemp)
                {
                    var groupEntriesWeek1 = entriesWeek1.GroupBy(i => i.tblTSHoursType.HoursTypeID).Select(o => new { hoursTypeID = o.Key, Total = o.Sum(y => y.Hours) }).FirstOrDefault(m => m.hoursTypeID == item.HoursTypeID);
                    var groupEntriesWeek2 = entriesWeek2.GroupBy(i => i.tblTSHoursType.HoursTypeID).Select(o => new { hoursTypeID = o.Key, Total = o.Sum(y => y.Hours) }).FirstOrDefault(m => m.hoursTypeID == item.HoursTypeID);

                    item.TotalWeek1 = groupEntriesWeek1 != null ? groupEntriesWeek1.Total : 0;
                    item.TotalWeek2 = groupEntriesWeek2 != null ? groupEntriesWeek2.Total : 0;
                    item.Total = item.TotalWeek1 + item.TotalWeek2;
                }

                var count = 10;
                foreach (var item in timeOffSummaryTemp)
                {
                    if (item.Total > 0)
                    {
                        item.DisplayOrder = count;
                        count = count + 10;
                        timeOffSummary.Add(item);
                    }
                }

                if (timeOffSummary.Count > 0)
                {
                    decimal overTimeRate = 40;
                    var workEntries = timeOffSummary.Where(m => m.HoursType == "Regular" || m.HoursType == "Holiday").ToList();
                    var otTotal = timeOffSummary.Where(m => m.HoursType == "Regular" || m.HoursType == "Holiday").Sum(o => o.Total);
                    var sumWeek1 = timeOffSummary.Where(m => m.HoursType == "Regular" || m.HoursType == "Holiday").Sum(o => o.TotalWeek1);
                    var sumWeek2 = timeOffSummary.Where(m => m.HoursType == "Regular" || m.HoursType == "Holiday").Sum(o => o.TotalWeek2);

                    if (sumWeek1 > overTimeRate || sumWeek2 > overTimeRate)
                    {
                        decimal week1Overtime = sumWeek1 > overTimeRate ? sumWeek1 - overTimeRate : 0;
                        decimal week2Overtime = sumWeek2 > overTimeRate ? sumWeek2 - overTimeRate : 0;
                        decimal totalOvertime = week1Overtime + week2Overtime;

                        timeOffSummary.Where(m => m.HoursType == "Regular").FirstOrDefault().TotalWeek1 = week1Overtime > timeOffSummary.Where(m => m.HoursType == "Regular").FirstOrDefault().TotalWeek1 ?
                            0 : timeOffSummary.Where(m => m.HoursType == "Regular").FirstOrDefault().TotalWeek1 - week1Overtime;

                        timeOffSummary.Where(m => m.HoursType == "Regular").FirstOrDefault().TotalWeek2 = week2Overtime > timeOffSummary.Where(m => m.HoursType == "Regular").FirstOrDefault().TotalWeek2 ?
                            0 : timeOffSummary.Where(m => m.HoursType == "Regular").FirstOrDefault().TotalWeek2 - week2Overtime;

                        timeOffSummary.Where(m => m.HoursType == "Regular").FirstOrDefault().Total = timeOffSummary.Where(m => m.HoursType == "Regular").FirstOrDefault().TotalWeek1 + timeOffSummary.Where(m => m.HoursType == "Regular").FirstOrDefault().TotalWeek2;

                        timeOffSummary.Add(new HoursTypeSummaryViewModel()
                        {
                            HoursTypeID = timeOffSummary.Max(m => m.HoursTypeID) + 1,
                            DisplayOrder = 0,
                            HoursType = "Overtime",
                            Total = totalOvertime,
                            TotalWeek1 = week1Overtime,
                            TotalWeek2 = week2Overtime
                        });
                    }


                    timeOffSummary.Add(new HoursTypeSummaryViewModel()
                    {
                        HoursTypeID = timeOffSummary.Max(m => m.HoursTypeID) + 1,
                        HoursType = "Total",
                        Total = timeOffSummary.Sum(m => m.Total),
                        TotalWeek1 = timeOffSummary.Sum(m => m.TotalWeek1),
                        TotalWeek2 = timeOffSummary.Sum(m => m.TotalWeek2)
                    });

                    timeOffSummary.OrderBy(m => m.DisplayOrder);

                    return timeOffSummary;
                }
            }
            return null;
        }

        private List<HoursTypeSummaryViewModel> GetMileageSummaryBasedOnSelectedPayPeriod(int selectedPayPeriodId, List<TSMileageViewModel> entries)
        {
            if (entries.Count() > 0)
            {
                List<HoursTypeSummaryViewModel> mileageSummary = new List<HoursTypeSummaryViewModel>();
                var totalMileage = entries.Sum(i => i.MileageMiles);
                mileageSummary.Add(new HoursTypeSummaryViewModel()
                {
                    HoursType = Enumeration.HoursType.Mileage.ToString(),
                    HoursTypeID = (int)Enumeration.HoursType.Mileage,
                    TotalMilage = totalMileage,
                    TotalWeek1Milage = 0.0f,
                    TotalWeek2Milage = 0.0f
                });
                return mileageSummary;
            }
            return null;
        }

        //private List<HoursTypeSummaryViewModel> GetEmployeeMileageSummary(DateTime startDate, DateTime endDate, List<TSMileageViewModel> entries)
        //{
        //    if (entries.Count > 0)
        //    {
        //        var week1StartDate = startDate;
        //        var week2StartDate = startDate.AddDays(7);
        //        var milageType = repository.GetHoursTypeMilage();
        //        List<HoursTypeSummaryViewModel> milageSummary = new List<HoursTypeSummaryViewModel>();

        //        var entriesWeek1 = entries.Where(m => m.Date >= week1StartDate && m.Date < week2StartDate);
        //        var entriesWeek2 = entries.Where(m => m.Date >= week2StartDate && m.Date <= endDate);

        //        milageSummary.Add(new HoursTypeSummaryViewModel()
        //        {
        //            HoursType = milageType.HoursType,
        //            HoursTypeID = milageType.HoursTypeID
        //        });

        //        foreach (var item in milageSummary)
        //        {
        //            var groupEntriesWeek1 = entriesWeek1.GroupBy(i => i.HoursTypeID).Select(o => new { hoursTypeID = o.Key, Total = o.Sum(y => y.MileageMiles) }).FirstOrDefault(m => m.hoursTypeID == item.HoursTypeID);
        //            var groupEntriesWeek2 = entriesWeek2.GroupBy(i => i.HoursTypeID).Select(o => new { hoursTypeID = o.Key, Total = o.Sum(y => y.MileageMiles) }).FirstOrDefault(m => m.hoursTypeID == item.HoursTypeID);

        //            item.TotalWeek1Milage = groupEntriesWeek1 != null ? groupEntriesWeek1.Total : 0;
        //            item.TotalWeek2Milage = groupEntriesWeek2 != null ? groupEntriesWeek2.Total : 0;
        //            item.TotalMilage = item.TotalWeek1Milage + item.TotalWeek2Milage;
        //        }

        //        if (milageSummary.Count > 0)
        //        {
        //            return milageSummary;
        //        }
        //    }
        //    return null;
        //}

        private List<HoursTypeSummaryViewModel> GetEmployeeOnCallSummary(DateTime startDate, DateTime endDate, List<TSOnCallViewModel> entries)
        {
            if (entries.Count > 0)
            {
                var week1StartDate = startDate;
                var week2StartDate = startDate.AddDays(7);
                var onCallType = repository.GetHoursTypeOnCall();
                List<HoursTypeSummaryViewModel> onCallSummaryTemp = new List<HoursTypeSummaryViewModel>();
                List<HoursTypeSummaryViewModel> onCallSummary = new List<HoursTypeSummaryViewModel>();

                var entriesWeek1 = entries.Where(m => m.Date >= week1StartDate && m.Date < week2StartDate);
                var entriesWeek2 = entries.Where(m => m.Date >= week2StartDate && m.Date <= endDate);
                var rates = repository.GetOnCallRateByDateRange(startDate, endDate);

                foreach (var item in entriesWeek1)
                {
                    var rate = rates.Where(m => m.tblTSHourTypeRate.HourTypeId == item.HoursTypeID).OrderByDescending(i => i.EffectiveDate).ToList();
                    if (rate.Count > 0)
                        item.OnCallRateForDay = rate.FirstOrDefault(m => m.EffectiveDate <= item.Date).Rate;
                }

                foreach (var item in entriesWeek2)
                {
                    var rate = rates.Where(m => m.tblTSHourTypeRate.HourTypeId == item.HoursTypeID).OrderByDescending(i => i.EffectiveDate).ToList();
                    if (rate.Count > 0)
                        item.OnCallRateForDay = rate.FirstOrDefault(m => m.EffectiveDate <= item.Date).Rate;
                }

                foreach (var item in onCallType)
                {
                    onCallSummaryTemp.Add(new HoursTypeSummaryViewModel()
                    {
                        HoursType = item.HoursType,
                        HoursTypeID = item.HoursTypeID
                    });
                }

                foreach (var item in onCallSummaryTemp)
                {
                    var groupEntriesWeek1 = entriesWeek1.GroupBy(i => i.HoursTypeID).Select(o => new { hoursTypeID = o.Key, Total = o.Sum(y => y.OnCallRateForDay) }).FirstOrDefault(m => m.hoursTypeID == item.HoursTypeID);
                    var groupEntriesWeek2 = entriesWeek2.GroupBy(i => i.HoursTypeID).Select(o => new { hoursTypeID = o.Key, Total = o.Sum(y => y.OnCallRateForDay) }).FirstOrDefault(m => m.hoursTypeID == item.HoursTypeID);

                    item.TotalWeek1OnCall = groupEntriesWeek1 != null ? groupEntriesWeek1.Total : 0;
                    item.TotalWeek2OnCall = groupEntriesWeek2 != null ? groupEntriesWeek2.Total : 0;
                    item.TotalOnCall = item.TotalWeek1OnCall + item.TotalWeek2OnCall;
                }

                foreach (var item in onCallSummaryTemp)
                {
                    if (item.TotalOnCall > 0)
                    {
                        onCallSummary.Add(item);
                    }
                }

                if (onCallSummary.Count > 0)
                {
                    return onCallSummary;
                }
            }
            return null;
        }


        /// <summary>
        /// Regular hours - this apply to Non-Exempt (FLSA) employee
        /// Each day has two entry record for AM and PM hours
        /// </summary>
        /// <param name="model"></param>
        /// <param name="DateEntryStart"></param>
        /// <param name="DateEntryEnd"></param>
        /// <returns></returns>
        private List<TimesheetHoursViewModel> GetTimesheetForRegularHours(TimesheetViewModel model, List<TSTimeEntryViewModel> timeEntries)
        {
            if (timeEntries != null)
            {
                List<Models.ViewModel.TimesheetHoursViewModel> tsHours = new List<Models.ViewModel.TimesheetHoursViewModel>();
                List<TimesheetHoursViewModel> currentEntries = GetEmployeeTimeEntriesByDateRange((DateTime)timeEntries.First().dtmDate, (DateTime)timeEntries.First().dtmDate, model.EmployeeInfoId);

                //Regular Hours
                decimal hoursTotalAM = GetTotalNumberOfHours(model.SelectedAMTimeStart, model.SelectedAMTimeEnd, model.SelectedAMMinutesStart, model.SelectedAMMinutesEnd);
                decimal hoursTotalPM = GetTotalNumberOfHours(model.SelectedPMTimeStart, model.SelectedPMTimeEnd, model.SelectedPMMinutesStart, model.SelectedPMMinutesEnd);
                decimal hoursTotalAdditional = GetTotalNumberOfHours(model.SelectedAdditionalTimeStart, model.SelectedAdditionalTimeEnd, model.SelectedAdditionalTimeMinutesStart, model.SelectedAdditionalTimeMinutesEnd);
                var currentPayPeriod = repository.GetPayPeriodByPayPeriodId(model.SelectedPayPeriodID);  //GetYearAndPayPeriodViewModel().CurrentPayPeriod;
                DateTime currentStartDate = currentPayPeriod.dtmPeriodStart;
                DateTime currentEndDate = currentPayPeriod.dtmPeriodEnd;

                foreach (var item in timeEntries)
                {
                    var entries = new List<Models.ViewModel.TimesheetHoursViewModel>();
                    bool IsOverlapping = false;
                    //first row time entry
                    if (hoursTotalAM > 0)
                    {
                        int amStartTime = CalculateTimeHourMinute(model.SelectedAMTimeStart, model.SelectedAMMinutesStart);
                        int amEndTime = CalculateTimeHourMinute(model.SelectedAMTimeEnd, model.SelectedAMMinutesEnd);

                        //check current time entries in DB for overlap
                        foreach (var entry in currentEntries)
                        {
                            int currEntryStart = entry.TimeStart;
                            int currEntryEnd = entry.TimeEnd;

                            //time entry is before current startTime
                            if (amStartTime < currEntryStart && amEndTime <= currEntryStart)
                                continue;

                            //time entry is after current EndTime
                            else if (amStartTime >= currEntryEnd)
                                continue;

                            else
                            {
                                IsOverlapping = true;
                                //throw new Exception("Conflicting time entry: new time entry in first row overlaps previously entered time for this date.");
                            }

                        }
                        if (!IsOverlapping)
                        {
                            entries.Add(
                            new Models.ViewModel.TimesheetHoursViewModel()
                            {
                                EmployeeInfoID = model.EmployeeInfoId,
                                PayPeriodID = model.SelectedPayPeriodID,
                                Date = (DateTime)item.dtmDate,
                                HoursTypeID = model.SelectedHoursType,
                                Hours = hoursTotalAM,
                                TimeStart = amStartTime,
                                TimeEnd = amEndTime,
                                StatusTypeID = (int)Enumeration.StatusType.NonSubmitted,
                                EntryDate = DateTime.Now.Date,
                                EntryUser = model.MasterUserId,
                                LockOutAll = false,
                                LockOutEmployee = false,
                                LockOutManager = false
                            });
                        }

                    }

                    //second row time entry
                    if (hoursTotalPM > 0)
                    {

                        int pmStartTime = CalculateTimeHourMinute(model.SelectedPMTimeStart, model.SelectedPMMinutesStart);
                        int pmEndTime = CalculateTimeHourMinute(model.SelectedPMTimeEnd, model.SelectedPMMinutesEnd);

                        //check current time entries in DB for overlap
                        foreach (var entry in currentEntries)
                        {
                            int currEntryStart = entry.TimeStart;
                            int currEntryEnd = entry.TimeEnd;

                            //time entry is before current startTime
                            if (pmStartTime < currEntryStart && pmEndTime <= currEntryStart)
                                continue;

                            //time entry is after current EndTime
                            else if (pmStartTime >= currEntryEnd)
                                continue;

                            else
                            {
                                IsOverlapping = true;
                                //throw new Exception("Conflicting time entry: new time entry in 2nd row overlaps previously entered time for this date.");
                            }

                        }
                        if (!IsOverlapping)
                        {
                            entries.Add(
                             new Models.ViewModel.TimesheetHoursViewModel()
                             {
                                 EmployeeInfoID = model.EmployeeInfoId,
                                 PayPeriodID = model.SelectedPayPeriodID,
                                 Date = (DateTime)item.dtmDate,
                                 HoursTypeID = model.SelectedHoursType,
                                 Hours = hoursTotalPM,
                                 TimeStart = pmStartTime,
                                 TimeEnd = pmEndTime,
                                 StatusTypeID = (int)Enumeration.StatusType.NonSubmitted,
                                 EntryDate = DateTime.Now.Date,
                                 EntryUser = model.MasterUserId,
                                 LockOutAll = false,
                                 LockOutEmployee = false,
                                 LockOutManager = false
                             });
                        }
                    }

                    //third row time entry
                    if (hoursTotalAdditional > 0)
                    {

                        int additionalStartTime = CalculateTimeHourMinute(model.SelectedAdditionalTimeStart, model.SelectedAdditionalTimeMinutesStart);
                        int additionalEndTime = CalculateTimeHourMinute(model.SelectedAdditionalTimeEnd, model.SelectedAdditionalTimeMinutesEnd);

                        //check current time entries in DB for overlap
                        foreach (var entry in currentEntries)
                        {
                            int currEntryStart = entry.TimeStart;
                            int currEntryEnd = entry.TimeEnd;

                            //time entry is before current startTime
                            if (additionalStartTime < currEntryStart && additionalEndTime <= currEntryStart)
                                continue;

                            //time entry is after current EndTime
                            else if (additionalStartTime >= currEntryEnd)
                                continue;

                            else
                            {
                                IsOverlapping = true;
                                //throw new Exception("Conflicting time entry: time entry in 3rd row overlaps previously entered time for this date.");
                            }

                        }

                        if (!IsOverlapping)
                        {
                            entries.Add(
                             new Models.ViewModel.TimesheetHoursViewModel()
                             {
                                 EmployeeInfoID = model.EmployeeInfoId,
                                 PayPeriodID = model.SelectedPayPeriodID,
                                 Date = (DateTime)item.dtmDate,
                                 HoursTypeID = model.SelectedHoursType,
                                 Hours = hoursTotalAdditional,
                                 TimeStart = additionalStartTime,
                                 TimeEnd = additionalEndTime,
                                 StatusTypeID = (int)Enumeration.StatusType.NonSubmitted,
                                 EntryDate = DateTime.Now.Date,
                                 EntryUser = model.MasterUserId,
                                 LockOutAll = false,
                                 LockOutEmployee = false,
                                 LockOutManager = false
                             });
                        }
                    }
                    foreach (var entry in entries)
                    {
                        //Add time entries to current pay period only; remove if date is not within the start/end payperiod
                        var timeEntry = AutoMapper.Mapper.Map(entry, new tblTSTimesheetHour());
                        if (entry.Date >= currentStartDate && entry.Date <= currentEndDate && !repository.IsTimeEntryExist(timeEntry))
                        {
                            tsHours.Add(entry);
                        }
                    }
                }
                return tsHours;
            }
            return null;
        }

        private List<DateTypeHoursItemViewModel> DisplayDateTypeHoursBasedOnTimeEntries(List<TimesheetHoursViewModel> viewModel)
        {
            var result = new List<DateTypeHoursItemViewModel>();
            var model = viewModel.GroupBy(i => new { i.Date, i.HoursTypeID, i.tblTSHoursType.HoursType })
                                 .Select(m => new TimesheetHoursViewModel()
                                 {
                                     Date = m.First().Date,
                                     Hours = m.Sum(i => i.Hours),
                                     HoursTypeID = m.First().HoursTypeID
                                 }).ToList();

            foreach (var item in model)
            {
                result.Add(new DateTypeHoursItemViewModel()
                {
                    DisplayDate = item.Date.ToShortDateString(),
                    HoursType = viewModel.First(i => i.tblTSHoursType.HoursTypeID == item.HoursTypeID).tblTSHoursType.HoursType,
                    DisplayHours = item.Hours.ToString()
                });
            }

            return result;
        }



        /// <summary>
        /// Non-Regular hours types are PTO, Voluntary PTO, etc. - this apply to both FLSA (non-exempt and exempt)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="DateEntryStart"></param>
        /// <param name="DateEntryEnd"></param>
        /// <returns></returns>
        private List<TimesheetHoursViewModel> GetTimesheetForNonRegularHours(TimesheetViewModel model, List<TSTimeEntryViewModel> timeEntries)
        {
            if (timeEntries != null)
            {

                List<TimesheetHoursViewModel> result = new List<TimesheetHoursViewModel>();
                foreach (var item in timeEntries)
                {
                    result.Add(new TimesheetHoursViewModel()
                    {
                        EmployeeInfoID = model.EmployeeInfoId,
                        PayPeriodID = model.SelectedPayPeriodID,
                        Date = (DateTime)item.dtmDate,
                        HoursTypeID = model.SelectedHoursType,
                        Hours = model.SelectedHourMinuteInDecimal,
                        TimeStart = 0,
                        TimeEnd = 0,
                        StatusTypeID = (int)Enumeration.StatusType.NonSubmitted,
                        EntryDate = DateTime.Now.Date,
                        EntryUser = model.MasterUserId,
                        LockOutAll = false,
                        LockOutEmployee = false,
                        LockOutManager = false,
                    });
                }
                //}
                //else
                //{
                //    throw new Exception("Conflicting time entry: time entry cannot exceed 12 hours for nonregular hour types in a day.");
                //}

                return result;
            }
            return null;
        }

        /// <summary>
        /// Get Timesheet for mileage entries
        /// </summary>
        /// <param name="individualId"></param>
        /// <returns></returns>
        //private List<TSMileageViewModel> GetTimesheetForMileage(TimesheetViewModel model, List<TSMileageViewModel> mileageEntries)
        //{

        //    if (model != null)
        //    {

        //            List<TSMileageViewModel> result = new List<TSMileageViewModel>();


        //            result.Add(new TSMileageViewModel()
        //            {
        //                //Date = DateTime.Now.Date,
        //                EmployeeInfoID = model.EmployeeInfoID,
        //                PayPeriodID = model.SelectedPayPeriodID,
        //                MileageDate = (DateTime)model.MileageDate,
        //                MileageMiles = model.MileageMiles,
        //                MileageTo = model.MileageTo,
        //                MileageFrom = model.MileageFrom,
        //                MileageDescription = model.MileageDescription,
        //                StatusTypeID = (int)Enumeration.StatusType.NonSubmitted,
        //                EntryDate = DateTime.Now.Date,
        //                EntryUser = model.EntryUser,
        //                LockOutAll = false,
        //                LockOutEmployee = false,
        //                LockOutManager = false,
        //            });

        //        return result;
        //    }
        //    return null;

        //}

        private IndividualViewModel GetIndividual(int individualId)
        {
            if (individualId > 0)
            {
                return AutoMapper.Mapper.Map(repository.GetIndividualByIndividualId(individualId), new IndividualViewModel());
            }
            return null;
        }
        private List<TimesheetHoursViewModel> GetSelectedTimeHoursEntries(SelectedTimeEntryViewModel model)
        {
            var result = new List<TimesheetHoursViewModel>();
            foreach (var item in model.Values)
            {
                var selectedTimeEntry = AutoMapper.Mapper.Map(repository.GetTimesheetHourById(Convert.ToInt32(item)), new TimesheetHoursViewModel());
                result.Add(selectedTimeEntry);
            }
            return result;

        }

        private List<PayPeriodViewModel> GetPayPeriodsList(int selectedYear)
        {
            List<PayPeriodViewModel> result = new List<PayPeriodViewModel>();
            var payperiods = repository.GetPayPeriodsBasedOnYear(selectedYear != 0 ? selectedYear : DateTime.Now.Year);
            result = AutoMapper.Mapper.Map(payperiods, new List<PayPeriodViewModel>());
            return result;
        }

        private List<SelectListItem> GetPayPeriodsListItems(List<PayPeriodViewModel> payperiods)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            foreach (var item in payperiods)
            {
                var displayText = string.Format("{0} - {1} ({2})", item.dtmPeriodStart.ToShortDateString(), item.dtmPeriodEnd.ToShortDateString(), item.txtStatus);
                result.Add(new SelectListItem() { Text = displayText, Value = item.PayPeriodID.ToString() });
            }
            return result; //.OrderByDescending(i => i.Value).ToList();
        }

        private int CalculateTimeHourMinute(int hour, int minutes)
        {
            return ((hour * 100) + minutes);
        }

        private int GetSelectedHoursOrMinutes(int time, bool isHour)
        {
            if (time > 0)
            {
                int hours = (time / 100) * 100;
                int minutes = (time - hours);
                DateTime result = DateTime.MinValue;
                if (isHour)
                {
                    return hours;
                }
                else
                {
                    return minutes;
                }
            }
            return 0;
        }

        private List<SelectListItem> PayPeriodYearListItems(bool pIsUserNonExempt, int pIndividualId)
        {
            List<SelectListItem> result = new List<SelectListItem>();
            
            // Returns the start and end date to determine the pay period listing years
            var payperiodDate = repository.GetEmployeePayPeriodDate(pIndividualId, pIsUserNonExempt);
            var startDate = payperiodDate != null ? payperiodDate.StartDate : DateTime.Now;
            var endDate = payperiodDate != null ? payperiodDate.EndDate : DateTime.Now;

            if (pIsUserNonExempt)
            {
                //NonExempt: Return Pay Period Years starting from the pay period year employee starts entering time up to the current year
                result = GetListOfYearsByDate(startDate);
            }
            else
            {
                //Exempt: Return all Years based on employees time entries
                result = GetExemptListOfYears(startDate, endDate);
            }

            return result;
        }

        private List<EmployeeIndividualViewModel> FilterTerminatedEmployeesOverThreeWeeks(List<EmployeeIndividualViewModel> pModel)
        {
            if (pModel != null)
            {
                var activeEmployees = pModel.Where(i => i.TimesheetStatus != "Terminated").ToList();
                var terminatedEmployees = pModel.Where(i => i.TimesheetStatus == "Terminated").ToList();
                pModel.Clear();
                pModel = activeEmployees;
                foreach (var item in terminatedEmployees)
                {                    
                    // Ticket 53484: Request from HR to hide or exclude terminated EE three weeks after the termination date 
                    if ((DateTime.Now - (DateTime)item.Individual.TermDate).TotalDays < 21)
                    {
                        pModel.Add(item);
                    }
                }
                return pModel;
            }
            return null;
        }


        #endregion
    }
}