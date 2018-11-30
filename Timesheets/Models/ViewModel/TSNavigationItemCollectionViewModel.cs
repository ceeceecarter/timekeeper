using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class TSNavigationItemCollectionViewModel
    {
        public List<TSNavigationItemViewModel> NavigationItems { get; set; }
        public Models.ViewModel.EmployeeIndividualViewModel EmployeeIndividual { get; set; }

        public TSNavigationItemCollectionViewModel()
        {
            NavigationItems = new List<TSNavigationItemViewModel>();
        }
    }
}