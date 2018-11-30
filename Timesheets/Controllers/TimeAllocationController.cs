using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NM.Web.WebApplication.Timesheets.Infrastructure.Enums;
using NM.Web.WebApplication.Timesheets.Models.ViewModel;
using NM.Web.WebApplication.Timesheets.Filters;
using NM.Lib.DataLibrary.United.Interface;
using NM.Lib.DataLibrary.DataAccess.United;

namespace NM.Web.WebApplication.Timesheets.Controllers 
{
    public class TimeAllocationController : BaseController
    {
        //public Models.ViewModel.EmployeeIndividualViewModel _employee = new Models.ViewModel.EmployeeIndividualViewModel();
        public TimeAllocationController(ITimesheet repo)
        {
            repository = repo;
            //_employee = GetEmployeeIndividual();
        }
        // GET: TimeAllocation
        //[Authorize(Roles ="TimesheetAdmin")]
        public ActionResult Index()
        {
            var employeeIndividual = GetEmployeeIndividual();
            var individual = repository.GetIndividualByMasterUserId(MasterUser.MasterUserID);
            var model = new TimeAllocationViewModel();
            model = GetTimeAllocations(individual.IndividualID);

            return View(model);
        }

        public ActionResult GetIndividualProperties()
        {
            var employeeIndividual = GetEmployeeIndividual();
            var individual = repository.GetIndividualByMasterUserId(MasterUser.MasterUserID);
            var model = new TimeAllocationViewModel();
            model = GetTimeAllocations(individual.IndividualID);

            //var individualProperties;

            return null;
        }

        #region private methods

        private TimeAllocationViewModel GetTimeAllocations(int individualId)
        {
            var timeAllocation = new TimeAllocationViewModel();
            timeAllocation.IndividualPropertyAllocations = GetIndividualPropertyAllocations(individualId);

            return timeAllocation;
        }

        /// <summary>
        /// Mockup individual Property Allocation
        /// </summary>
        /// <param name="individualId"></param>
        /// <returns></returns>
        private List<IndividualPropertyAllocationViewModel> GetIndividualPropertyAllocations(int individualId)
        {
            var individualPropertyAllocations = new List<IndividualPropertyAllocationViewModel>();

            individualPropertyAllocations.Add(new IndividualPropertyAllocationViewModel()
            {
                IndividualPropertyAllocationID = 1,
                IndividualID = 162,
                Active = true,
                AllocationMonth = DateTime.Now,
                IndividualPropertyAllocationDetail = new List<IndividualPropertyAllocationDetailViewModel>()
                 {
                     new IndividualPropertyAllocationDetailViewModel() {
                         IndividualPropertyAllocationDetailID = 1,
                         IndividualID = 162,
                         EntityID = "057508",
                         Active = true,
                         AllocationPercent = 0.71428571M,
                         IndividualPropertyAllocationID = 1612,
                         Building = new BuildingViewModel()
                         {
                             BLDGID="057508",
                             BLDGNAME ="Carlson Tech Center D",
                             CITY = "Plymouth",
                             ENTITYID = "057508"
                         }
                     }
                     ,new IndividualPropertyAllocationDetailViewModel() {
                         IndividualPropertyAllocationDetailID = 2,
                         IndividualID = 162,
                         EntityID = "057507",
                         Active = true,
                         AllocationPercent = 0.71428571M,
                         IndividualPropertyAllocationID = 1612,
                         Building = new BuildingViewModel()
                         {
                             BLDGID="057507",
                             BLDGNAME ="Carlson Tech Center C",
                             CITY = "Plymouth",
                             ENTITYID = "057507"
                         }
                     }
                 }
            });

            return individualPropertyAllocations;
        }

        #endregion private methods
    }
}