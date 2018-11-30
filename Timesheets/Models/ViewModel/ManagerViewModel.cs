using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class ManagerViewModel  //, IMapFrom<United.BusinessModel.EmployeeIndividual>
    {

        #region Properties

        public EmployeeIndividualViewModel Manager { get; set; }

        public List<IndividualDelegateViewModel> ApproverDelegates = new List<IndividualDelegateViewModel>();
        public List<EmployeeIndividualViewModel> EmployeeList { get; set; }
        public List<EmployeeIndividualViewModel> DelegateEmployeeList { get; set; }
        public List<EmployeeIndividualViewModel> DelegatedToEmployeeList { get; set; }
        public List<IndividualViewModel> SearchedForEmployees { get; set; }

        public List<IndividualViewModel> ManagersEmployees { get; set; }
        public int ManagerIndividualID { get; set; }

        #endregion

        #region Constructors

        public ManagerViewModel()
        {
            Manager = new EmployeeIndividualViewModel();
            ApproverDelegates = new List<IndividualDelegateViewModel>();
            EmployeeList = new List<EmployeeIndividualViewModel>();
            SearchedForEmployees = new List<IndividualViewModel>();
            DelegatedToEmployeeList = new List<EmployeeIndividualViewModel>();
        }

        #endregion

    }
}