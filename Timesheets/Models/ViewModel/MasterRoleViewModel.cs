using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class MasterRoleViewModel : IMapFrom<NM.Lib.DataLibrary.United.Domain.MasterRoleDTO>
    {
        public int RoleID { get; set; }
        public string Title { get; set; } 
    }
}