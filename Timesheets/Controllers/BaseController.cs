using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Reflection;
using System.Runtime.Caching;
using NM.Web.WebApplication.Timesheets.Models.ViewModel;
using NM.Web.WebApplication.Timesheets.Filters;
using NM.Shared.Lib.DataLibrary.Domain;
using NM.Lib.DataLibrary.United.Handler;
using NM.Lib.DataLibrary.United.Interface;
using NM.Lib.CommonLibrary;
using NM.Lib.CentralModel.Domain;
using NM.Lib.CentralApiLibrary.Interface;
using NM.Shared.Lib.CentralApiLibrary;

namespace NM.Web.WebApplication.Timesheets.Controllers
{
    [CentralApiAuthorization]
    public class BaseController : Controller
    {
        private const string AUTHENTICATED_USER = "AUTHENTICATED_USER";
        //public string applicationName = CommonSettings.ApplicationName();
        public int timesheetAdmin = int.Parse(ConfigurationManager.AppSettings["TimesheetAdmin"]);
        public int timesheetUser = int.Parse(ConfigurationManager.AppSettings["TimesheetUser"]);
        public int timesheetManager = int.Parse(ConfigurationManager.AppSettings["TimesheetManager"]);
        public int submissionToManager_Exempt = int.Parse(ConfigurationManager.AppSettings["TimesheetSubmissionToManager_Exempt"]);
        public int submissionToManager_NonExempt = int.Parse(ConfigurationManager.AppSettings["TimesheetSubmissionToManager_NonExempt"]);
        public string defaultHREmail = ConfigurationManager.AppSettings["DefaultHREmail"];
        public string testUserName = ConfigurationManager.AppSettings["TestUserName"];
        public int defaultOfficeLocationID = int.Parse(ConfigurationManager.AppSettings["DefaultOfficeLocationID"]);
        public string defaultDomainName = ConfigurationManager.AppSettings["DefaultDomainName"];
        public ITimesheet repository;

        public string CurrentLoginUserName;
        public Models.ViewModel.MasterUserViewModel _masterUserViewModel;

        public IndividualViewModel AuthenticateUser
        {
            get
            {
                var item = MemoryCache.Default.Get(AUTHENTICATED_USER);

                if (Properties.Settings.Default.DisableCaching == true || item == null)
                {
                    NM.Lib.DataLibrary.United.Handler.BaseHandler baseHandler = new Lib.DataLibrary.United.Handler.BaseHandler();
                    var masterUserIdCookie = GetMasterUserIdCookie();
                    var currentValidUser = masterUserIdCookie.Values != null && !string.IsNullOrEmpty(masterUserIdCookie.Value) ? int.Parse(masterUserIdCookie.Value) : MasterUser.MasterUserID;
                    if (currentValidUser == 0) throw new HttpException("Unable to retrieve valid MasterUserId for the current login user.");

                    IndividualViewModel ivm = new IndividualViewModel();
                    AutoMapper.Mapper.Map(baseHandler.GetIndividual(currentValidUser), ivm);

                    MemoryCache.Default.Set(AUTHENTICATED_USER, ivm, DateTime.Now.AddMinutes(5));

                    return ivm;
                }
                else
                {
                    return (IndividualViewModel)item;
                }
            }
        }

        public Models.ViewModel.MasterUserViewModel MasterUser
        {
            get
            {
                var masterUserIdCookie = GetMasterUserIdCookie();
                var currentMasterUserId = masterUserIdCookie.Values != null && !string.IsNullOrEmpty(masterUserIdCookie.Value) ? int.Parse(masterUserIdCookie.Value) : GetCurrentUserAuthToken().MasterUserId;
                var _masterUser = new Models.ViewModel.MasterUserViewModel();
                var model = repository.GetMasterUserById(currentMasterUserId);
                if (model != null)
                {
                    _masterUser = AutoMapper.Mapper.Map(model, new Models.ViewModel.MasterUserViewModel());
                    return _masterUser;
                }
                return null;
            }
        }

        public HttpCookie GetMasterUserIdCookie()
        {
            if (this.ControllerContext != null)
            {
                return this.ControllerContext.HttpContext.Response.Cookies["MasterUserId"];
            }
            return null;
        }


        public AuthToken GetCurrentUserAuthToken()
        {
            AuthToken token = new AuthToken();
            var currentUser = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split(new string[] { "\\" }, StringSplitOptions.None);
            token.ApplicationName = CommonSettings.ApplicationName();
            token.DomainName = currentUser[0];
            token.UserName = testUserName == string.Empty ? currentUser[1] : testUserName;

            AuthToken result = CentralUserAuthenticationToken(token);

            return result;
        }

        public Models.ViewModel.EmployeeIndividualViewModel GetEmployeeIndividual()
        {
            return GetEmployeeIndividual(0);
        }
        public Models.ViewModel.EmployeeIndividualViewModel GetEmployeeIndividual(int MasterUserID)
        {
            //Gets Employee Individual's View Model
            var masterUserIdCookie = GetMasterUserIdCookie();
            TimesheetHandler handler = new TimesheetHandler();
            int currentValidUserMasterUserID = 0;
            EmployeeIndividualViewModel result;
            if (MasterUserID == 0)
            {
                currentValidUserMasterUserID = masterUserIdCookie.Values != null && !string.IsNullOrEmpty(masterUserIdCookie.Value) ? int.Parse(masterUserIdCookie.Value) : MasterUser.MasterUserID;
            }
            else
            {
                currentValidUserMasterUserID = MasterUserID;
            }


            if (currentValidUserMasterUserID == 0) throw new HttpException("Unable to retrieve valid MasterUserId for the current login user.");
            var individual = handler.GetIndividualByMasterUserId(currentValidUserMasterUserID);
            var employeeIndividual = handler.GetEmployeeIndividual(individual);
            result = AutoMapper.Mapper.Map(employeeIndividual, new Models.ViewModel.EmployeeIndividualViewModel(employeeIndividual));

            //Sets Roles
            result.MasterUserRoles = AutoMapper.Mapper.Map(handler.GetRoleByMasterUserId(currentValidUserMasterUserID), new List<MasterUserRoleViewModel>());

            //Gets users manager
            var managerDTO = handler.GetIndividualByIndividualId(result.Individual.ManagerIndividualID);
            if (managerDTO != null)
            {
                var managerEmpIndDTO = handler.GetEmployeeIndividual(managerDTO);
                result.IndividualsManager = AutoMapper.Mapper.Map(managerEmpIndDTO.Individual, new IndividualViewModel());
            }

            //Determines if the user is a manager
            result.IsUserTSManager = handler.GetManagerEmployeeCount(result.IndividualId) > 0 ? true : false;

            //Determine if the user is a delegatee
            result.IsUserDelegate = handler.GetIsDelegate(result.IndividualId);

            foreach (var role in result.MasterUserRoles)
            {
                if (role.RoleID == timesheetAdmin) result.IsUserTSHRAdmin = true;
                if (role.RoleID == timesheetUser) result.IsUserTSUser = true;
            }
            return result;
        }

        public DelegateViewModel GetApprovalDelegateWithSearchListByName(string fName, string lName)
        {
            var individuals = repository.GetIndividualsByName(fName, lName);
            List<IndividualViewModel> searchList = new List<IndividualViewModel>();
            foreach (var individual in individuals)
            {
                searchList.Add(AutoMapper.Mapper.Map(individual, new IndividualViewModel()));
            }
            return GetApprovalDelegate(searchList);
        }

        public List<IndividualViewModel> GetIndividaulSearchResultsByName(string fName, string lName)
        {
            var individuals = repository.GetIndividualsByName(fName, lName);
            List<IndividualViewModel> searchList = new List<IndividualViewModel>();
            foreach (var individual in individuals)
            {
                searchList.Add(AutoMapper.Mapper.Map(individual, new IndividualViewModel()));
            }
            return searchList;
        }

        public DelegateViewModel GetApprovalDelegate(List<IndividualViewModel> searchList)
        {
            var delegateVM = new DelegateViewModel();

            if (searchList == null)
            {
                searchList = new List<IndividualViewModel>();
            }
            var id = GetEmployeeIndividual().IndividualId;
            List<IndividualDelegateViewModel> delegateApprovers = GetManagersApprovalDelegates(id);
            delegateVM.ApproverDelegates = delegateApprovers;
            delegateVM.SearchedForEmployees = searchList;
            return delegateVM;
        }

        public List<Models.ViewModel.IndividualDelegateViewModel> GetManagersApprovalDelegates(int id)
        {
            var currentValidUser = MasterUser;
            if (currentValidUser == null) throw new HttpException("Unable to retrieve valid MasterUserId for the current login user.");

            var result = new List<Models.ViewModel.IndividualDelegateViewModel>();
            var manager = repository.GetIndividualByIndividualId(id);
            var managerIndividual = repository.GetEmployeeIndividualById(id);
            //var managerIndividual = repository.GetEmployeeIndividual(manager);
            var mappedManager = AutoMapper.Mapper.Map(managerIndividual, new Models.ViewModel.EmployeeIndividualViewModel(managerIndividual));

            var managersApprovalDelegates = repository.GetManagerApprovalDelegates(id);

            foreach (var approvalDelegate in managersApprovalDelegates)
            {
                var approvalDelegateIndividual = repository.GetIndividualByIndividualId(approvalDelegate.DelegateToIndividualID);
                var employeeIndividual = repository.GetEmployeeIndividual(approvalDelegateIndividual);
                var mappedEmployee = AutoMapper.Mapper.Map(employeeIndividual, new Models.ViewModel.EmployeeIndividualViewModel(employeeIndividual));
                result.Add(new IndividualDelegateViewModel() { DelegateID = approvalDelegate.DelegateID, DelegateToIndividual = mappedEmployee, DelegateToIndividualID = mappedEmployee.IndividualId, IndividualID = manager.IndividualID, IsPrimaryDelegate = approvalDelegate.IsPrimaryDelegate });
            }
            return result;
        }

        public List<EmployeeIndividualViewModel> GetManagerDelegateEmployees(int managerID)
        {
            List<EmployeeIndividualViewModel> delegateEmployeeList = new List<EmployeeIndividualViewModel>();
            var managerApprovalDelegates = repository.GetManagerDelegateEmployees(managerID);
            YearAndPayPeriodViewModel yearAndPayPeriod = GetYearAndPayPeriodViewModel();
            foreach (var item in managerApprovalDelegates)
            {
                //Get Individual
                var employee = repository.GetIndividualByIndividualId(item.IndividualID);
                var employeeIndividualDto = repository.GetEmployeeIndividual(employee);
                //Gets individual's manager
                var managerDTO = repository.GetIndividualByIndividualId(employeeIndividualDto.Individual.ManagerIndividualID);
                var managerEmpIndDTO = repository.GetEmployeeIndividual(managerDTO);
                delegateEmployeeList.Add(new EmployeeIndividualViewModel(employeeIndividualDto) { IndividualsManager = AutoMapper.Mapper.Map(managerEmpIndDTO.Individual, new IndividualViewModel()) });
            }
            foreach (var item in delegateEmployeeList)
            {
                item.EmployeeInfoId = item.EmployeeInformation.EmployeeInfoId;
            }

            return delegateEmployeeList;
        }


        public List<EmployeeIndividualViewModel> GetManagerDelegatedToEmployees(int delegatedToID)
        {
            var delegatorIDs = repository.GetDelegatorIndividualIds(delegatedToID);
            var delegateeEmployeeList = new List<EmployeeIndividualViewModel>();
            foreach (var item in delegatorIDs)
            {
                var delegatedEmps = GetManagersEmployeeIndividuals(item);
                delegateeEmployeeList.AddRange(delegatedEmps);
            }

            return delegateeEmployeeList;
        }


        public List<EmployeeIndividualViewModel> GetManagersEmployeeIndividuals(int id)
        {
            var currentValidUser = MasterUser;
            if (currentValidUser == null) throw new HttpException("Unable to retrieve valid MasterUserId for the current login user.");

            var result = new List<Models.ViewModel.EmployeeIndividualViewModel>();
            var managersEmployees = repository.GetManagerIndividualListById(id);
            foreach (var employee in managersEmployees)
            {
                var employeeIndividual = repository.GetEmployeeIndividual(employee);
                var mappedEmployee = AutoMapper.Mapper.Map(employeeIndividual, new Models.ViewModel.EmployeeIndividualViewModel(employeeIndividual));

                var manager = repository.GetIndividualByIndividualId(mappedEmployee.Individual.ManagerIndividualID);
                if (manager != null)
                {
                    var mappedManager = AutoMapper.Mapper.Map(manager, new Models.ViewModel.IndividualViewModel());
                    mappedEmployee.IndividualsManager = mappedManager;
                }

                mappedEmployee.IsUserTSManager = repository.GetManagerEmployeeCount(mappedEmployee.IndividualId) > 0 ? true : false;

                if (mappedEmployee.MasterUserRoles != null)
                {
                    foreach (var role in mappedEmployee.MasterUserRoles)
                    {
                        if (role.RoleID == timesheetAdmin) mappedEmployee.IsUserTSHRAdmin = true;
                        //if (role.RoleID == timesheetManager) mappedEmployee.IsUserTSManager = true;
                        if (role.RoleID == timesheetUser) mappedEmployee.IsUserTSUser = true;
                    }
                }
                result.Add(mappedEmployee);
            }
            return result;
        }

        private AuthToken CentralUserAuthenticationToken(AuthToken token)
        {
            Lib.CentralApiLibrary.CentralFactory cf = new Lib.CentralApiLibrary.CentralFactory(token, Lib.CentralApiLibrary.CentralFactory.ServiceHandler.Auth);
            IAuthServiceHandler authServiceHandler = (IAuthServiceHandler)cf.GetServiceHandler();
            AuthToken result = (AuthToken)authServiceHandler.ValidateUser();
            return result;
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            Exception exception = filterContext.Exception;
            //Logging the Exception
            filterContext.ExceptionHandled = true;

            NMException exc = new NMException(CommonSettings.ApplicationName(), CommonSettings.ApplicationVersion(), filterContext.Exception);
            LogService.LogExceptionAsync(exc);

            var Result = this.View("Error", new HandleErrorInfo(exception,
                filterContext.RouteData.Values["controller"].ToString(),
                filterContext.RouteData.Values["action"].ToString()));

            ViewBag.CurrentEnvironment = CommonSettings.ApplicationEnvironment();

            filterContext.Result = Result;


            //base.OnException(filterContext);
        }

        protected List<SelectListItem> GetHoursTypeNonExemptList()
        {
            var hoursType = new List<SelectListItem>();
            var tsHoursTypes = AutoMapper.Mapper.Map<List<Models.ViewModel.TSHoursTypeViewModel>>(repository.GetHoursTypeNonExempt().Where(i => i.SubTypeID != int.Parse(ConfigurationManager.AppSettings["OnCallHoursType"])));
            foreach (var hourtype in tsHoursTypes)
            {
                hoursType.Add(new SelectListItem { Text = hourtype.HoursType, Value = hourtype.HoursTypeID.ToString() });
            };
            return hoursType;
        }


        protected List<SelectListItem> GetHoursTypeForExempt()
        {
            var hoursType = new List<SelectListItem>();
            var tsHoursTypes = AutoMapper.Mapper.Map<List<Models.ViewModel.TSHoursTypeViewModel>>(repository.GetHoursTypeExempt());
            foreach (var ht in tsHoursTypes)
            {
                hoursType.Add(new SelectListItem { Text = ht.HoursType, Value = ht.HoursTypeID.ToString() });
            }
            return hoursType;
        }

        protected List<SelectListItem> GetHoursTypeForOnCall()
        {
            var hoursType = new List<SelectListItem>();
            var hoursData = repository.GetHoursTypeOnCall().OrderByDescending(m => m.HoursTypeID).ToList();
            var tsHoursTypes = AutoMapper.Mapper.Map<List<Models.ViewModel.TSHoursTypeViewModel>>(hoursData);
            foreach (var ht in tsHoursTypes)
            {
                hoursType.Add(new SelectListItem { Text = ht.HoursType, Value = ht.HoursTypeID.ToString() });
            }

            return hoursType;
        }

        /// <summary>
        /// Minute values for Regular Hours pick list
        /// </summary>
        /// <returns></returns>
        protected List<SelectListItem> GetPickerMinutes()
        {
            var lstMinutes = new List<SelectListItem>();
            var m = DateTime.MinValue.AddMinutes(60);
            foreach (int mn in Enumerable.Range(00, 4))
            {
                //must be set consecutively 
                lstMinutes.Add(new SelectListItem() { Text = m.ToString("mm"), Value = m.ToString("mm") });
                m = m.AddMinutes(15);
            }
            return lstMinutes;
        }

        /// <summary>
        /// Hour values for Regular Hours pick list
        /// </summary>
        /// <returns></returns>
        protected List<SelectListItem> GetPickerHours()
        {
            var lstHours = new List<SelectListItem>();
            var hours = Enumerable.Range(00, 24).Select(i => (DateTime.MinValue.AddHours(i)));
            foreach (var hh in hours)
            {
                lstHours.Add(new SelectListItem() { Text = hh.ToString("hh.mm tt"), Value = hh.ToString("HH") });
            }
            lstHours.Add(new SelectListItem() { Text = "", Value = "24" });
            return lstHours;
        }

        /// <summary>
        /// Values for Non-Regular hours pick list i.e. PTO, Jury Duty, etc.
        /// </summary>
        /// <returns></returns>
        protected List<SelectListItem> GetPickerTimeHourMinuteInDecimal()
        {
            var lstHours = new List<SelectListItem>();
            double valueToIncrement = 0;
            double endval = 48; //TODO: make this dynamic next time
            for (int i = 0; i < endval; i++)
            {
                valueToIncrement = valueToIncrement + .25;
                lstHours.Add(new SelectListItem() { Text = valueToIncrement.ToString(), Value = valueToIncrement.ToString() });
            }
            return lstHours;
        }

        protected decimal GetTotalNumberOfHours(int timeStart, int timeEnd, int minuteStart, int minuteEnd)
        {
            //Get total number of hours
            var _timeEnd = timeEnd + ((decimal)minuteEnd / 60);
            var _timeStart = timeStart + ((decimal)minuteStart / 60);

            decimal result = (_timeEnd - _timeStart);

            return result;
        }

        protected YearAndPayPeriodViewModel GetYearAndPayPeriodViewModel()
        {
            var payperiods = AutoMapper.Mapper.Map(repository.GetCurrentNextPreviousPayPeriod(), new List<Models.ViewModel.PayPeriodViewModel>());
            var yearPayPeriod = new YearAndPayPeriodViewModel();
            foreach (var item in payperiods)
            {
                item.dtm1stReminder = item.dtm1stReminder ?? DateTime.Now.AddDays(-30);
                item.dtm2ndReminder = item.dtm2ndReminder ?? DateTime.Now.AddDays(-30);
                item.txtLastPeriodOfYear = string.IsNullOrEmpty(item.txtLastPeriodOfYear) ? "-" : item.txtLastPeriodOfYear;
                switch (item.TimePeriod)
                {
                    case "Current":
                        yearPayPeriod.CurrentPayPeriod = item;
                        break;
                    case "Next":
                        yearPayPeriod.NextPayPeriod = item;
                        break;
                    case "Previous":
                        yearPayPeriod.PreviousPayPeriod = item;
                        break;
                    default:
                        break;
                }
            }
            yearPayPeriod.CurrentYear = DateTime.Now.Year;
            yearPayPeriod.PayPeriods = payperiods;
            yearPayPeriod.TimePeriods = GetTimePeriodTypes();
            return yearPayPeriod;
        }

        /// <summary>
        /// Get list of years to display based on employee hire/start date; if none specified use current year.
        /// </summary>
        /// <param name="startDate"></param>
        /// <returns></returns>
        protected List<SelectListItem> GetListOfYearsByDate(DateTime? startDate)
        {
            List<string> listOfYears = new List<string>();
            int defaultNumberOfYearsToGenerateBasedOnCurrentYear = int.Parse(ConfigurationManager.AppSettings["NumberOfYearsToGenerateBasedOnCurrentYear"]);
            var result = new List<SelectListItem>();
            if (startDate.HasValue)
            {
                //Get list of years based on employee hire date
                listOfYears = repository.GenerateListOfYearsBasedOnDate((DateTime)startDate);
            }
            else
            {
                //Get the list of Years based on current Year
                listOfYears = repository.GenerateYearsBasedOnCurrentYear(defaultNumberOfYearsToGenerateBasedOnCurrentYear).OrderBy(i => i).ToList();
            }
            foreach (var lstYear in listOfYears)
            {
                result.Add(new SelectListItem { Text = lstYear, Value = lstYear });
            }
            return result;
        }

        protected List<SelectListItem> GetExemptListOfYears(DateTime pMinDate, DateTime pMaxDate)
        {
            List<string> sYears = new List<string>();
            List<SelectListItem> result = new List<SelectListItem>();
            var minYear = pMinDate.Year;
            var maxYear = pMaxDate.Year;

            int numberOfYears = (maxYear - minYear) + 1;
            for(int i = 0; i < numberOfYears;)
            {
                sYears.Add((maxYear - i).ToString());
                i++;
            }
            foreach(var item in sYears)
            {
                result.Add(new SelectListItem { Text = item, Value = item });
            }

            return result;
        }

        protected List<SelectListItem> GetTimePeriodTypes()
        {
            var payperiods = AutoMapper.Mapper.Map(repository.GetCurrentNextPreviousPayPeriod(), new List<Models.ViewModel.PayPeriodViewModel>());
            List<SelectListItem> result = new List<SelectListItem>();
            foreach (var item in payperiods)
            {
                var strPayPeriod = item.dtmPeriodStart + " - " + item.dtmPeriodEnd;
                result.Add(new SelectListItem { Text = item.TimePeriod, Value = strPayPeriod.ToString() });
            }
            if (result.Count() == payperiods.Count())
            {
                result.Add(new SelectListItem { Text = "Date Range", Value = "-1" });
            }
            return result;
        }

        public void SendEmail(string emailTo, string emailFrom, string emailSubject, string emailBody, string applicationName = "")
        {
            if (string.IsNullOrEmpty(applicationName)) applicationName = CommonSettings.ApplicationName();
            repository.InsertEmail(new Lib.DataLibrary.DataAccess.United.EmailLog()
            {
                EmailTo = emailTo,
                EmailFrom = emailFrom,
                EmailSubject = emailSubject,
                EmailBody = emailBody,
                Application = applicationName
                //ProcessExternal = false //TODO: get latest and refresh edmx
            });
        }

        /// <summary>
        /// Build HtmlString table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public static HtmlString BuildHtmlString<T>(List<T> data, List<string> headers)
        {
            //Tags
            TagBuilder table = new TagBuilder("table");
            table.Attributes.Add("style", "width:500px;font-family: verdana;");
            TagBuilder tr = new TagBuilder("tr");
            TagBuilder td = new TagBuilder("td");
            td.Attributes.Add("style", "width:25%");
            
            TagBuilder th = new TagBuilder("th");
            th.Attributes.Add("bgcolor", "#003865");
            th.Attributes.Add("style", "font-weight: bold; text-align: left; color: #FFFFFF");

            //Inner html of table
            StringBuilder sb = new StringBuilder();

            //Add headers
            foreach (var s in headers)
            {
                th.InnerHtml = s;
                tr.InnerHtml += th.ToString();
            }
            sb.Append(tr.ToString());

            //Add data
            foreach (var d in data)
            {
                tr.InnerHtml = "";

                d.GetType().GetProperties().ToList().ForEach(delegate (PropertyInfo prop)
                {
                    if (prop.GetValue(d, null) != null)
                    {
                        td.InnerHtml = prop.GetValue(d, null).ToString();
                        tr.InnerHtml += td.ToString();
                    }
                });
                sb.Append(tr.ToString());
            }

            table.InnerHtml = sb.ToString();
            return new HtmlString(table.ToString());
        }

        public static string GetMyTable<T>(IEnumerable<T> list, params string[] columns)
        {
            var sb = new StringBuilder();
            foreach (var item in list)
            {
                foreach (var column in columns)
                    sb.Append(item.GetType().GetProperty(column).GetValue(item, null));
            }
            return sb.ToString();
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            base.OnActionExecuted(filterContext);

            var model = filterContext.Controller.ViewData.Model as BaseViewModel;

            if (model != null)
            {
                model.AuthenticatedUser = this.AuthenticateUser;
            }

        }
    }
}