using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NM.Web.WebApplication.Timesheets.United.Models
{
    public partial class tblTSEmployeeInfo
    {
        public void set(tblTSEmployeeInfo m, UnitedEntities db)
        {
            DateOfWelcomeEmail = m.DateOfWelcomeEmail;
            FileNumber = m.FileNumber;
        }
    }
}
