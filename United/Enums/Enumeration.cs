using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NM.Web.WebApplication.Timesheets.United.Enums
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
            Regular = 1,
            PTO = 2,
            VolunteerPTO = 3,
            JuryDuty = 4,
            Bereavement = 5,
            Unpaid = 6,
            OnCall = 7,
            Mileage = 8,
            Parking = 9,
            Holiday = 10
        }
    }
}
