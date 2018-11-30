using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class TimeAllocationViewModel
    {
        public List<IndividualPropertyAllocationViewModel> IndividualPropertyAllocations { get; set; }
        public int IndividualId { get; set; }
        public string MonthAllocated { get; set; }
        
        public TimeAllocationViewModel()
        {
            IndividualPropertyAllocations = new List<IndividualPropertyAllocationViewModel>();
        }
    }

    public class IndividualPropertyAllocationViewModel
    {
        public int IndividualPropertyAllocationID { get; set; }
        public int IndividualID { get; set; }
        public DateTime AllocationMonth { get; set; }
        public DateTime DateSubmittedToManager { get; set; }
        public bool Active { get; set; } 

        public List<IndividualPropertyAllocationDetailViewModel> IndividualPropertyAllocationDetail { get; set; }
    }

    public class IndividualPropertyAllocationDetailViewModel
    {
        public int IndividualPropertyAllocationDetailID { get; set; }
        public int IndividualID { get; set; }
        public string EntityID { get; set; }
        public decimal AllocationPercent { get; set; }
        public int IndividualPropertyAllocationID { get; set; }
        public bool Active { get; set; }

        public virtual BuildingViewModel Building { get; set; }
    }

    public class BuildingViewModel
    {
        public string BLDGID { get; set; }
        public string BLDGNAME { get; set; }
        public string CITY { get; set; }
        public string ENTITYID { get; set; }

    }
}