using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NM.Lib.DataLibrary.United.Interface;
using NM.Lib.DataLibrary.DataAccess.United;
using NM.Lib.DataLibrary.United.Domain;
using System.Configuration;
using NM.Web.WebApplication.Timesheets.Infrastructure.Enums;
using NM.Web.WebApplication.Timesheets.Models.ViewModel;
using NM.Web.WebApplication.Timesheets.Filters;

namespace NM.Web.WebApplication.Timesheets.Controllers
{
    public class EmployeeAdministrationController : BaseController
    {
        public EmployeeAdministrationController(ITimesheet repo)
        {
            repository = repo;
        }
        // GET: EmployeeAdministration
        public ActionResult Index()
        {
            var loggedInEmployeeIndividual = GetEmployeeIndividual();

            if (!loggedInEmployeeIndividual.IsUserTSHRAdmin)
            {
                return RedirectToAction("Index", "Timesheets");
            }
            var currentValidUser = MasterUser;
            if (currentValidUser == null) throw new HttpException("Unable to retrieve valid MasterUserId for the current login user.");

            Models.ViewModel.EmployeeAdministrationViewModel model = new Models.ViewModel.EmployeeAdministrationViewModel();

            //Default new employee to true
            model.Employee.IsUserTSUser = true;

            //get all employee
            var employees = AutoMapper.Mapper.Map(repository.GetAllEmployeeIndividual(), new List<Models.ViewModel.EmployeeIndividualViewModel>());
            model.Employees = employees;
            model.Employee.CostCenterList = GetCostCenters().OrderBy(i => i.Text).ToList();
            model.Employee.CompanyCodeList = GetCompanyCodes().OrderBy(i => i.Text).ToList();
            model.Employee.JobTitleList = GetJobTitles().OrderBy(i => i.Text).ToList();
            model.Employee.EmploymentTypeList = GetEmploymentTypes().OrderBy(i => i.Text).ToList();
            model.Employee.WorkLocationList = GetWorkLocations().OrderBy(i => i.Text).ToList();
            model.Employee.EmployeeList = GetListOfEmployees();
            model.Employee.EmployeeInformation = new Models.ViewModel.EmployeeInformationViewModel();
            model.Employee.Individual = new Models.ViewModel.IndividualViewModel();
            model.Employee.FLSAStatusList = GetFLSAStatus();
            model.Employee.EmployeeStatusList = GetEmployeeStatusList();
            model.Employee.DelegateViewModel.SelectedIndividualID = 0;
            model.Employee.OfficerTitleList = GetOfficerTitleList().OrderBy(i => i.Text).ToList(); 

            return View("EmployeeAdmin", model);
        }

        [HttpPost]
        public ActionResult SearchForEmployees(string fName, string lName, string compCode, string empTypeCode, string empStatusCode)
        {
            Models.ViewModel.EmployeeAdministrationViewModel model = new Models.ViewModel.EmployeeAdministrationViewModel();

            //Default new employee to true
            model.Employee.IsUserTSUser = true;

            //get all employees by criteria
            var employees = AutoMapper.Mapper.Map(repository.GetEmployeeIndividualsByCriteria(fName, lName, compCode, empTypeCode, empStatusCode), new List<Models.ViewModel.EmployeeIndividualViewModel>());
            model.Employees = employees;
            model.Employee.CostCenterList = GetCostCenters().OrderBy(i => i.Text).ToList();
            model.Employee.CompanyCodeList = GetCompanyCodes().OrderBy(i => i.Text).ToList();
            model.Employee.JobTitleList = GetJobTitles().OrderBy(i => i.Text).ToList();
            model.Employee.EmploymentTypeList = GetEmploymentTypes().OrderBy(i => i.Text).ToList();
            model.Employee.WorkLocationList = GetWorkLocations().OrderBy(i => i.Text).ToList();
            model.Employee.EmployeeList = GetListOfEmployees();
            model.Employee.EmployeeInformation = new Models.ViewModel.EmployeeInformationViewModel();
            model.Employee.Individual = new Models.ViewModel.IndividualViewModel();
            model.Employee.FLSAStatusList = GetFLSAStatus();
            model.Employee.EmployeeStatusList = GetEmployeeStatusList();
            model.Employee.DelegateViewModel.SelectedIndividualID = 0;

            return PartialView("_TSEmployeeList", model);
        }

        [HttpPost]
        public ActionResult Edit(int id)
        {
            if (id <= 0) return null;
            var individual = repository.GetEmployeeIndividualById(id);
            var model = AutoMapper.Mapper.Map(individual, new Models.ViewModel.EmployeeIndividualViewModel(individual));
            model.CostCenterList = GetCostCenters().OrderBy(i => i.Text).ToList();
            model.CompanyCodeList = GetCompanyCodes().OrderBy(i => i.Text).ToList();
            model.JobTitleList = GetJobTitles().OrderBy(i => i.Text).ToList();
            model.EmploymentTypeList = GetEmploymentTypes();
            model.WorkLocationList = GetWorkLocations().OrderBy(i => i.Text).ToList();
            model.EmployeeList = GetListOfEmployees().OrderBy(i => i.Text).ToList();
            model.FLSAStatusList = GetFLSAStatus();
            model.OfficerTitleList = GetOfficerTitleList().OrderBy(i => i.Text).ToList();
            model.DelegateViewModel.SelectedIndividualID = id;
            model.DelegateViewModel.ApproverDelegates = GetManagersApprovalDelegates(individual.IndividualId);

            return PartialView("_TSEmployeeInformation", model);
        }

        [HttpPost]
        [ValidateModelState]
        public ActionResult EditSave(Models.ViewModel.EmployeeIndividualViewModel model)
        {
            if (ModelState.IsValid)
            {
                var loggedInUser = AuthenticateUser;
                var domainName = ConfigurationManager.AppSettings["DefaultDomainName"];

                //Save changes to Individual data
                if (model.Individual != null)
                {
                    var individual = AutoMapper.Mapper.Map(model.Individual, new IndividualDTO());
                    individual.OfficeLocationsID = individual.OfficeLocationsID == 0 ? defaultOfficeLocationID : individual.OfficeLocationsID;
                    if (!model.Individual.MasterUserID.HasValue)
                    {
                        var empMasterUserId = repository.InsertMasterUserByName(model.Individual.FirstName, model.Individual.LastName, domainName, (int)loggedInUser.MasterUserID);
                        model.Individual.MasterUserID = empMasterUserId;
                        individual.MasterUserID = empMasterUserId;
                    }
                    individual.Email = !string.IsNullOrEmpty(individual.Email) ? individual.Email.Trim().Replace(" ", "") : string.Empty;

                    repository.UpsertIndividual(AutoMapper.Mapper.Map(individual, new tblIndividual()));

                }
                //Save changes to EmployeeInformation data
                if (model.EmployeeInformation != null)
                {
                    var employeeInfo = AutoMapper.Mapper.Map(model.EmployeeInformation, new EmployeeInformationDTO());

                    //manager employee info 
                    var managerEmployeeInfo = repository.GetEmployeeInformationByIndividualId(model.Individual.ManagerIndividualID);
                    employeeInfo.ManagerEmployeeInfoId = managerEmployeeInfo != null ? managerEmployeeInfo.EmployeeInfoId : 0;

                    employeeInfo.MasterUserID = employeeInfo.MasterUserID > 0 ? employeeInfo.MasterUserID : model.Individual.MasterUserID.HasValue ? (int)model.Individual.MasterUserID : repository.InsertMasterUserByName(model.Individual.FirstName, model.Individual.LastName, domainName, (int)loggedInUser.MasterUserID);

                    //Recalculate FTE
                    var fteCurrentValue = employeeInfo.realFTE;
                    var fteNewValue = CalculateFTE((double)employeeInfo.realHoursPerWeek);
                    if (fteCurrentValue != fteNewValue) employeeInfo.realFTE = fteNewValue;

                    //FLSAStatus: NonExempt, Exempt and NonApplicable
                    if (model.FLSAStatus == 1) employeeInfo.IsNonExempt = true; //non exempt
                    if (model.FLSAStatus == 0) employeeInfo.IsNonExempt = false; //exempt
                    if (model.FLSAStatus == -1) employeeInfo.IsNonExempt = null; // non applicable

                    repository.UpsertEmployeeInfo(AutoMapper.Mapper.Map(employeeInfo, new tblTSEmployeeInfo()));


                    InsertDeleteEmployeeAccess(model);
                }


                //Return updated employee list
                Models.ViewModel.EmployeeAdministrationViewModel employeeAdmin = new Models.ViewModel.EmployeeAdministrationViewModel();
                //get all employee
                var employees = AutoMapper.Mapper.Map(repository.GetAllEmployeeIndividual(), new List<Models.ViewModel.EmployeeIndividualViewModel>());

                employeeAdmin.Employees = employees;
                employeeAdmin.Employee.EmployeeStatusList = GetEmployeeStatusList();
                employeeAdmin.Employee.FLSAStatusList = GetFLSAStatus();
                return PartialView("_TSEmployeeList", employeeAdmin);
            }
            return RedirectToAction("Index");
        }



        [HttpPost]
        public ActionResult AddEmployee(Models.ViewModel.EmployeeAdministrationViewModel model)
        {
            if (ModelState.IsValid)
            {
                var individual = AutoMapper.Mapper.Map(model.Employee.Individual, new IndividualDTO());

                //1. Create MasterUser record
                var masterUser = new MasterUserViewModel();
                masterUser.FirstName = individual.FirstName;
                masterUser.LastName = individual.LastName;
                masterUser.CreatedByMasterUserID = -1;
                masterUser.CreatedDatetime = DateTime.Now;
                masterUser.ModifiedByMasterUserID = -1;
                masterUser.ModifiedDatetime = DateTime.Now;
                masterUser.Version = 1;
                masterUser.DomainName = defaultDomainName;
                masterUser.UserName = string.Format("{0}.{1}", individual.FirstName.Replace(" ", ""), individual.LastName.Replace(" ", ""));
                var masterUserID = repository.InsertMasterUser(AutoMapper.Mapper.Map(masterUser, new tblMasterUser()));

                //2. Create Individual record
                individual.MasterUserID = masterUserID;
                individual.OfficeLocationsID = defaultOfficeLocationID;
                individual.CreatedByMasterUserID = -1;
                individual.CreatedDatetime = DateTime.Now;
                individual.ModifiedByMasterUserID = -1;
                individual.ModifiedDatetime = DateTime.Now;
                individual.Inactive = false;
                individual.IsEmployee = true;
                individual.Email = !string.IsNullOrEmpty(individual.Email) ? individual.Email.Trim().Replace(" ", "") : string.Empty;

                var individualID = repository.InsertIndividual(AutoMapper.Mapper.Map(individual, new tblIndividual()));

                //3. Create EmployeeInfo record 
                // Get Manager Individual Id for the employee
                var employeeManager = repository.GetEmployeeInformationByIndividualId(individual.ManagerIndividualID);               
                var employeeInformation = AutoMapper.Mapper.Map(model.Employee.EmployeeInformation, new EmployeeInformationDTO());
                if (model.Employee.FLSAStatus == 1) employeeInformation.IsNonExempt = true;
                if (model.Employee.FLSAStatus == 0) employeeInformation.IsNonExempt = false;
                if (model.Employee.FLSAStatus == -1) employeeInformation.IsNonExempt = null;
                employeeInformation.ManagerEmployeeInfoId = employeeManager.EmployeeInfoId;
                employeeInformation.IndividualID = individualID;
                employeeInformation.MasterUserID = masterUserID;
                employeeInformation.realFTE = CalculateFTE((double)employeeInformation.realHoursPerWeek);
                employeeInformation.AutoAudit_CreatedBy = "-1";
                employeeInformation.AutoAudit_CreatedDate = DateTime.Now;
                employeeInformation.AutoAudit_ModifiedBy = "-1";
                employeeInformation.AutoAudit_ModifiedDate = DateTime.Now;
                repository.UpsertEmployeeInfo(AutoMapper.Mapper.Map(employeeInformation, new tblTSEmployeeInfo()));


                //4. Create Access
                var empIndividual = repository.GetEmployeeIndividualById(individualID);
                empIndividual.IsUserTSHRAdmin = model.Employee.IsUserTSHRAdmin;
                empIndividual.IsUserTSManager = model.Employee.IsUserTSManager;
                empIndividual.IsUserTSUser = model.Employee.IsUserTSUser;
                var employeeInfoVM = AutoMapper.Mapper.Map(empIndividual, new EmployeeIndividualViewModel());
                InsertDeleteEmployeeAccess(employeeInfoVM);


                //Return updated employee list
                //get all employee
                var employees = AutoMapper.Mapper.Map(repository.GetAllEmployeeIndividual(), new List<Models.ViewModel.EmployeeIndividualViewModel>());
                model.Employees = employees;
                model.Employee.EmployeeStatusList = GetEmployeeStatusList();
                model.Employee.FLSAStatusList = GetFLSAStatus();

                return PartialView("_TSEmployeeList", model);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public ActionResult SearchApproverDelegates(string fName, string lName, int individualId)
        {
            var model = new Models.ViewModel.DelegateViewModel();
            var searchResults = GetIndividaulSearchResultsByName(fName, lName);
            model.SearchedForEmployees = searchResults;
            model.SelectedIndividualID = individualId;
            return PartialView("_DelegateTSSearchResults", model);
        }

        [HttpGet]
        public ActionResult UpsertDelegate(int delegateToId, int selectedIndividualId, bool isPrimaryDelegate)
        {
            selectedIndividualId = selectedIndividualId == 0 ? GetEmployeeIndividual().IndividualId : selectedIndividualId;

            repository.UpsertManagerApprovalDelegate(delegateToId, selectedIndividualId, isPrimaryDelegate);
            var model = new Models.ViewModel.DelegateViewModel();
            model.ApproverDelegates = GetManagersApprovalDelegates(selectedIndividualId);
            model.SelectedIndividualID = selectedIndividualId;
            return PartialView("_DelegateTSApprovalAuthorityPartialView", model);
        }

        [HttpGet]
        public ActionResult DeleteDelegate(int id, int selectedIndividualID)
        {
            var deleteDelegateList = repository.GetDelegateByDelegateId(id);

            if (deleteDelegateList != null)
            {
                repository.DeleteManagerApprovalDelegate(deleteDelegateList.DelegateToIndividualID, deleteDelegateList.IndividualID);
            }
            var model = new Models.ViewModel.DelegateViewModel();
            model.ApproverDelegates = GetManagersApprovalDelegates(selectedIndividualID);
            model.SelectedIndividualID = selectedIndividualID;

            return PartialView("_DelegateTSApprovalAuthorityPartialView", model);
        }

        #region private methods

        private void InsertDeleteEmployeeAccess(Models.ViewModel.EmployeeIndividualViewModel model)
        {
            /* Save changes to Time Access: 
             *  access will be deleted or added from tblMasterUserRoles; 
             *  no logical delete                  
             */
            var tsAdmin = repository.GetMasterUserRoleByMasterUserIDAndRoleID(model.MasterUserId, timesheetAdmin);
            var tsUser = repository.GetMasterUserRoleByMasterUserIDAndRoleID(model.MasterUserId, timesheetUser);
            var tsManager = repository.GetMasterUserRoleByMasterUserIDAndRoleID(model.MasterUserId, timesheetManager);
            var masterUserRole = new tblMasterUserRole() { MasterUserID = model.MasterUserId };
            if (model.IsUserTSHRAdmin)
            {
                //employee has admin access; if non-exist add admin access
                masterUserRole.RoleID = timesheetAdmin;
                if (tsAdmin == null) { repository.AddMasterUserRole(masterUserRole); }
            }
            else
            {
                //employee do not have admin; if exist remove admin access
                if (tsAdmin != null) { repository.DeleteSingleMasterUserRole(tsAdmin); }
            }
            if (model.IsUserTSManager)
            {
                //employee has manager access; if non exist add manager access
                masterUserRole.RoleID = timesheetManager;
                if (tsManager == null) { repository.AddMasterUserRole(masterUserRole); };
            }
            else
            {
                //employee do not have manager access; if exist remove manager access
                if (tsManager != null) { repository.DeleteSingleMasterUserRole(tsManager); }
            }
            if (model.IsUserTSUser)
            {
                //employee has user access; if non-exist add user access
                masterUserRole.RoleID = timesheetUser;
                if (tsUser == null) { repository.AddMasterUserRole(masterUserRole); }
            }
            else
            {
                //employee do not have user access; if exist remove user access
                if (tsUser != null) { repository.DeleteSingleMasterUserRole(tsUser); }
            }
        }

        /// <summary>
        /// Cost Center list
        /// </summary>
        /// <returns></returns>
        private List<SelectListItem> GetCostCenters()
        {
            var listCostCenters = repository.GetCostCenters();
            List<SelectListItem> result = new List<SelectListItem>();
            foreach (var item in listCostCenters)
            {
                var displayText = string.Format("{0} - {1}", item.CostNumber, item.Description);
                result.Add(new SelectListItem { Text = displayText, Value = item.CostCenterID.ToString() });
            }
            return result;
        }

        private List<SelectListItem> GetCompanyCodes()
        {
            var listOfCompanyCodes = repository.GetCompanyCodes().OrderBy(i => i.txtCompanyName);
            List<SelectListItem> result = new List<SelectListItem>();
            foreach (var item in listOfCompanyCodes)
            {
                string displayText = string.Format("{0} - {1}", item.txtCompanyCode, item.txtCompanyName);
                result.Add(new SelectListItem { Text = displayText, Value = item.CompanyCodeID.ToString() });
            }
            return result;
        }


        private List<SelectListItem> GetJobTitles()
        {
            var listOfJobTitle = repository.GetJobTitles();
            List<SelectListItem> result = new List<SelectListItem>();
            foreach (var item in listOfJobTitle)
            {
                result.Add(new SelectListItem { Text = item.JobTitleName, Value = item.JobTitleID.ToString() });
            }
            return result;
        }

        private List<SelectListItem> GetEmploymentTypes()
        {
            var listOfEmploymentType = repository.GetEmploymentTypes().OrderBy(i => i.EmploymentType);
            List<SelectListItem> result = new List<SelectListItem>();
            foreach (var item in listOfEmploymentType)
            {
                result.Add(new SelectListItem { Text = item.EmploymentType, Value = item.EmploymentTypeID.ToString() });
            }
            return result;
        }

        private List<SelectListItem> GetListOfEmployees()
        {
            var listOfEmployees = repository.GetAllEmployeeIndividual().OrderBy(i => i.LastName);
            List<SelectListItem> result = new List<SelectListItem>();
            foreach (var item in listOfEmployees)
            {
                string displayText = string.Format("{0}, {1}", item.LastName, item.FirstName);
                result.Add(new SelectListItem { Text = displayText, Value = item.IndividualId.ToString() });
            }
            return result;
        }

        private List<SelectListItem> GetWorkLocations()
        {
            var locations = repository.GetWorkLocations();
            List<SelectListItem> result = new List<SelectListItem>();
            foreach (var item in locations)
            {
                var displayText = string.Format("{0} - {1}", item.WorkLocationCode, item.WorkLocationName);
                result.Add(new SelectListItem { Text = displayText, Value = item.WorkLocationId.ToString() });
            }
            return result;
        }

        private List<SelectListItem> GetFLSAStatus()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            foreach (Enumeration.FLSAStatusType item in Enum.GetValues(typeof(Enumeration.FLSAStatusType)))
            {
                var itemValue = (int)item;
                result.Add(new SelectListItem() { Text = item.ToString(), Value = itemValue.ToString() });
            }

            return result;
        }

        private List<SelectListItem> GetEmployeeStatusList()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            foreach (Enumeration.EmployeeStatus item in Enum.GetValues(typeof(Enumeration.EmployeeStatus)))
            {
                var itemValue = (int)item;
                result.Add(new SelectListItem() { Text = item.ToString(), Value = itemValue.ToString() });
            }
            return result;
        }

        private List<SelectListItem> GetOfficerTitleList()
        {
            List<SelectListItem> result = new List<SelectListItem>();
            var officerTitles = repository.GetOfficerTitles();
            foreach (var item in officerTitles)
            {
                result.Add(new SelectListItem() { Text = item.OfficerTitleName, Value = item.OfficerTitleID.ToString() });
            }
            return result;
        }

        private double CalculateFTE(double hoursPerWeek)
        {
            return hoursPerWeek / 40;
        }

        #endregion private methods

    }
}