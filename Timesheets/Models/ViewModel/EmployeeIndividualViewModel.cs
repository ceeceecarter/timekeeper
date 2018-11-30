using NM.Web.WebApplication.Timesheets.Infrastructure.Enums;
using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class EmployeeIndividualViewModel : IMapFrom<NM.Lib.DataLibrary.United.Domain.EmployeeIndividualDTO>
    {
        public int EmployeeInfoId { get; set; }
        public int MasterUserId { get; set; }
        public int IndividualId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool Inactive { get; set; }
        public string LogicalDepartment { get; set; }
        public string OfficeLocationName { get; set; }
        public int ManagerEmployeeInfoId { get; set; }
        public bool IsUserTSHRAdmin { get; set; }
        public bool IsUserTSUser { get; set; }
        public bool IsUserTSManager { get; set; }
        public bool IsUserDelegate { get; set; }
        public string FileNumber { get; set; } 
        public IndividualViewModel Individual { get; set; }
        public IndividualViewModel IndividualsManager { get; set; }
        public JobTitleViewModel JobTitle { get; set; }
        public EmployeeInformationViewModel EmployeeInformation { get; set; }
        public List<MasterUserRoleViewModel> MasterUserRoles { get; set; }
        public List<Models.ViewModel.TimesheetHoursViewModel> EmployeeTimesheetEntries { get; set; }
        public Models.ViewModel.TimesheetViewModel EmployeeTimesheet { get; set; }
        public List<EmployeeIndividualViewModel> ManagerDelegateEmployeeList { get; set; }
        public List<EmployeeIndividualViewModel> ManagerDelegatedToEmployeeList { get; set; }
        public List<EmployeeIndividualViewModel> ManagerEmployeeList { get; set; }
        public List<SelectListItem> CostCenterList { get; set; }
        public List<SelectListItem> CompanyCodeList { get; set; }
        public List<SelectListItem> JobTitleList { get; set; }
        public List<SelectListItem> EmploymentTypeList { get; set; }
        public List<SelectListItem> WorkLocationList { get; set; } 
        public List<SelectListItem> EmployeeList { get; set; }
        public List<SelectListItem> FLSAStatusList { get; set; }
        public List<SelectListItem> EmployeeStatusList { get; set; } 
        public List<SelectListItem> OfficerTitleList { get; set; }
        public bool? IsEmployee { get; set; }
        public string CompanyCode { get; set; }
        public string EmploymentTypeCode { get; set; }
        public int? FLSAStatus { get; set; }
        public int? EmployeeStatus { get; set; }
        public string EmployeeStatusText { get; set; }
        public string OfficerTitleName { get; set; } 
        public int? OfficerTitleID { get; set; }
        public string TimesheetStatus { get; set; }
        public decimal PTOBalance { get; set; } 

        public List<EmployeeInformationViewModel> ManagerEmployeeInfoList { get; set; }
        public DelegateViewModel DelegateViewModel { get; set; }

        public EmployeeIndividualViewModel()
        {
            DelegateViewModel = new DelegateViewModel();
        }

        public EmployeeIndividualViewModel(NM.Lib.DataLibrary.United.Domain.EmployeeIndividualDTO m)
        {
            EmployeeInformation = AutoMapper.Mapper.Map(m.EmployeeInformationDto, new EmployeeInformationViewModel());
            MasterUserRoles = new List<MasterUserRoleViewModel>();
            Individual = AutoMapper.Mapper.Map(m.Individual, new IndividualViewModel());
            JobTitle = AutoMapper.Mapper.Map(m.JobTitle, new JobTitleViewModel());
            IsEmployee = m.IsEmployee;
            //ManagerEmployeeInfoId = m.EmployeeInfo.ManagerEmployeeInfoId;
            IsUserTSHRAdmin = m.IsUserTSHRAdmin;
            IsUserTSManager = m.IsUserTSManager;
            IsUserTSUser = m.IsUserTSUser;
            CompanyCode = m.CompanyCode;
            EmploymentTypeCode = m.EmploymentTypeCode;
            FLSAStatus = m.FLSAStatus;
            EmployeeStatus = m.EmployeeStatus;
            OfficerTitleName = m.OfficerTitleName;
            OfficerTitleID = m.OfficerTitleID;
            foreach(Enumeration.EmployeeStatus item in Enum.GetValues(typeof(Enumeration.EmployeeStatus)))
            {
                if(m.EmployeeStatus == (int)item)
                {
                    EmployeeStatusText = item.ToString().Replace("_", " ");
                    break;
                }
            }
            EmployeeInformation.IsNonExempt = EmployeeInformation.IsNonExempt ?? true;
            DelegateViewModel = new DelegateViewModel();
            CostCenterList = new List<SelectListItem>();
            JobTitleList = new List<SelectListItem>();
            EmploymentTypeList = new List<SelectListItem>();
            WorkLocationList = new List<SelectListItem>();
            EmployeeList = new List<SelectListItem>();
            DelegateViewModel = new DelegateViewModel();
            FLSAStatusList = new List<SelectListItem>();
            EmployeeStatusList = new List<SelectListItem>();
            IndividualsManager = new IndividualViewModel();
            ManagerEmployeeList = new List<EmployeeIndividualViewModel>();
            ManagerDelegateEmployeeList = new List<EmployeeIndividualViewModel>();
            ManagerDelegatedToEmployeeList = new List<EmployeeIndividualViewModel>();
            OfficerTitleList = new List<SelectListItem>();           
        }
    }
}