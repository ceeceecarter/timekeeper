using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class PayPeriodEmployee : IMapFrom<NM.Lib.DataLibrary.United.Domain.PayPeriodEmployeeDTO>
    {
        public int PayPeriodID { get; set; }

        public int IndividualId { get; set; }

        public string Icon { get; set; }

        public string LastName { get; set; }

        public string FirstName { get; set; }

        public string Office { get; set; }

        public string FlsaStatus { get; set; }

        public string Delegate { get; set; }

        public DateTime StartDate { get; set; }

        public string Supervisor { get; set; }

        public string TimesheetErrorStatus { get; set; }

        public string TimesheetStatus { get; set; }

        public bool Selected { get; set; }

    }
}