using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NM.Web.WebApplication.Timesheets.United.Models
{
    public partial class tblTSTimesheetHour
    {
        public void set(tblTSTimesheetHour m, UnitedEntities db)
        {
            Hours = m.Hours;
            HoursTypeID = m.HoursTypeID;
            Date = m.Date;
            TimeStart = m.TimeStart;
            TimeEnd = m.TimeEnd;
            StatusTypeId = m.StatusTypeId;
            SubmitDate = m.SubmitDate;
            SubmitUser = m.SubmitUser;
            ApproveDate = m.ApproveDate;
            ApproveUser = m.ApproveUser;
        }
    }
}
