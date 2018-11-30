using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Infrastructure.Enums
{
    public class Enumeration
    {
        /// <summary>
        /// Defines the possible Time Pay Period for PayPeriod picklist
        /// </summary>
        public enum TimePayPeriod
        {
            Current,
            Next,
            Previous,
            DateRange
        }

        public enum StatusType
        {
            NonSubmitted = 4,
            Submitted = 3,
            Approved = 2,
            Processed = 1
        }

        public enum HoursType
        {
            Overtime = 0,
            Regular = 1,
            PTO = 2,
            VolunteerPTO = 3,
            JuryDuty = 4,
            Bereavement = 5,
            Unpaid = 6,
            OnCall = 7,
            Mileage = 8,
            Parking = 9,
            Holiday = 10,
            OnCallHoliday = 11,
            OnCallRegular = 12
        }

        public enum FLSAStatusType
        {
            NonExempt = 1,
            Exempt = 0,
            NonApplicable = -1
        }

        public enum EmployeeStatus
        {
            Active = 1, //Start Date in effect and end date is null
            Pending_Active = 2, //Start Date set in the future
            Termination_Pending = 3, //End Date set in the future
            Terminated = 4 //End Date in effect
        }

        public enum TimesheetAction
        {
            Submit = 1,
            Reject = 2,
            Approve = 3   
        }
    }
}
