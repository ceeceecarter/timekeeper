using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NM.Web.WebApplication.Timesheets.United.BusinessModel
{
    public class MasterUserRole
    {
        public int MasterUserID { get; set; }
        public int RoleID { get; set; }
        public MasterRole Role { get; set; } 
    }
}
