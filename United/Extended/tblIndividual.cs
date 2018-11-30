using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NM.Web.WebApplication.Timesheets.United.Models
{
    public partial class tblIndividual
    {
        public void set(tblIndividual m, UnitedEntities db)
        {
            FirstName = m.FirstName;
            LastName = m.LastName;
            Title = m.Title;
            CompanyID = m.CompanyID;
        }
    }
}
