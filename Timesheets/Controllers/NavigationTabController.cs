using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NM.Lib.DataLibrary.United.Interface;
using NM.Web.WebApplication.Timesheets.Infrastructure.Enums;
using System.Configuration;

namespace NM.Web.WebApplication.Timesheets.Controllers
{
    public class NavigationTabController : BaseController
    {
        public NavigationTabController(ITimesheet repo)
        {
            repository = repo;
        }
        
        #region Actions
        // GET: NavigationTab
        public ActionResult Nav()
        {           
            var employeeIndividual = GetEmployeeIndividual();
            var model = GetNavigationTabs(employeeIndividual);
            model.EmployeeIndividual = employeeIndividual;
            return PartialView("Nav", model);
        }

        public ActionResult TimesheetHeader()
        {
            var timeYears = new List<SelectListItem>();
            var masterUserIdCookie = GetMasterUserIdCookie();
            var currentMasterUserId = !string.IsNullOrEmpty(masterUserIdCookie.Value) ? int.Parse(masterUserIdCookie.Value) : MasterUser.MasterUserID;
            var _employee = repository.GetIndividualByMasterUserId(currentMasterUserId);
            var isNonExempt = (bool)repository.GetEmployeeIndividual(_employee).EmployeeInformationDto.IsNonExempt;

            //Get list of years based on employee hire/start date; if none use current year as the start
            timeYears = GetListOfYearsByDate(_employee.StartDate);

            //Get current, next and previous pay periods
            var payPeriods = AutoMapper.Mapper.Map(repository.GetCurrentNextPreviousPayPeriod(), new List<Models.ViewModel.PayPeriodViewModel>());

            //Get TimePeriod Type
            List<SelectListItem> slTimePeriods = new List<SelectListItem>();
            slTimePeriods = GetTimePeriodTypes();

            //Populate YearAndPayPeriodViewModel
            Models.ViewModel.YearAndPayPeriodViewModel model = new Models.ViewModel.YearAndPayPeriodViewModel()
            {
                TimeYears = timeYears,
                PayPeriods = payPeriods,
                CurrentYear = DateTime.Now.Year,
                CurrentPayPeriod = payPeriods.FirstOrDefault(i => i.TimePeriod == Enumeration.TimePayPeriod.Current.ToString()),
                PreviousPayPeriod = payPeriods.FirstOrDefault(i => i.TimePeriod == Enumeration.TimePayPeriod.Previous.ToString()),
                NextPayPeriod = payPeriods.FirstOrDefault(i => i.TimePeriod == Enumeration.TimePayPeriod.Next.ToString()),
                TimePeriods = slTimePeriods,
                //Identify FLSA status of current login user
                IsNonExempt = isNonExempt
            };

            return PartialView("Header", model);
        }


        public ActionResult ManagementHeader()
        {
            //Get the list of Years based on current Year
            int defaultYearsToGenerateBasedOnCurrentYear = int.Parse(ConfigurationManager.AppSettings["NumberOfYearsToGenerateBasedOnCurrentYear"]);
            var listOfYears = repository.GenerateYearsBasedOnCurrentYear(defaultYearsToGenerateBasedOnCurrentYear).OrderBy(i => i);
            var timeYears = new List<SelectListItem>();
            foreach (var lstYear in listOfYears)
            {
                timeYears.Add(new SelectListItem { Text = lstYear, Value = lstYear });
            }
            var currentYear = DateTime.Now.Year;

            //Get current, next and previous pay periods
            var payPeriods = AutoMapper.Mapper.Map(repository.GetCurrentNextPreviousPayPeriod(), new List<Models.ViewModel.PayPeriodViewModel>());

            //Get TimePeriod Type
            List<SelectListItem> slTimePeriods = new List<SelectListItem>();
            foreach (var payperiod in payPeriods)
            {
                var strPayPeriod = payperiod.dtmPeriodStart + "-" + payperiod.dtmPeriodEnd;
                slTimePeriods.Add(new SelectListItem { Text = payperiod.TimePeriod, Value = strPayPeriod.ToString() });
            }
            if (slTimePeriods.Count() == payPeriods.Count())
            {
                slTimePeriods.Add(new SelectListItem { Text = "Date Range", Value = "-1" });
            }

            //Populate YearAndPayPeriodViewModel
            Models.ViewModel.YearAndPayPeriodViewModel model = new Models.ViewModel.YearAndPayPeriodViewModel()
            {
                TimeYears = timeYears,
                PayPeriods = payPeriods,
                CurrentYear = currentYear,
                CurrentPayPeriod = payPeriods.FirstOrDefault(i => i.TimePeriod == Enumeration.TimePayPeriod.Current.ToString()),
                PreviousPayPeriod = payPeriods.FirstOrDefault(i => i.TimePeriod == Enumeration.TimePayPeriod.Previous.ToString()),
                NextPayPeriod = payPeriods.FirstOrDefault(i => i.TimePeriod == Enumeration.TimePayPeriod.Next.ToString()),
                TimePeriods = slTimePeriods
            };

            return PartialView("Header", model);
        }

        //public ActionResult HRAdminHeader() {
        //    List<string> listOfYears = new List<string>();
        //    listOfYears = repository.GenerateYearsBasedOnCurrentYear(numberOfYearsToGenerateBasedOnCurrentYear).OrderBy(i => i).ToList();
        //    var timeYears = new List<SelectListItem>();
        //    foreach(var lstYear in listOfYears) { timeYears.Add(new SelectListItem { Text = lstYear, Value=lstYear }); }

        //    return null;
        //}

        #endregion Actions

        #region private methods

        private Models.ViewModel.TSNavigationItemCollectionViewModel GetNavigationTabs(Models.ViewModel.EmployeeIndividualViewModel model)
        {
            if (model != null)
            {
                var result = new Models.ViewModel.TSNavigationItemCollectionViewModel();
                var navtabs = AutoMapper.Mapper.Map(repository.GetNavigationData(), new List<Models.ViewModel.TSNavigationItemViewModel>());
                //if (model.IsUserTSUser)
                //{
                //    foreach(var item in navtabs)
                //    {
                //        var isExists = result.NavigationItems.Exists(i => i.TabId == item.TabId);
                //        if(item.IsTSUser &!isExists) { result.NavigationItems.Add(item); }
                //    }
                //}
                //if (model.IsUserTSHRAdmin)
                //{
                //    foreach (var item in navtabs)
                //    {
                //        var isExists = result.NavigationItems.Exists(i => i.TabId == item.TabId);
                //        if (item.IsTSHRAdmin & !isExists) { result.NavigationItems.Add(item); }
                //    }
                //}
                foreach (var item in navtabs)
                {
                    var isExists = result.NavigationItems.Exists(i => i.TabId == item.TabId);
                    if (model.IsUserTSUser & item.IsTSUser & !isExists) { result.NavigationItems.Add(item); isExists = true; }
                    if (model.IsUserTSManager & item.IsTSManager & !isExists) { result.NavigationItems.Add(item); isExists = true; }
                    if (model.IsUserTSHRAdmin & item.IsTSHRAdmin & !isExists) { result.NavigationItems.Add(item); isExists = true; }
                }
                return result;
            }
            return null;
        }
        #endregion private methods
    }
} 