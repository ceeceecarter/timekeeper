using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class MasterUserRoleViewModel : IMapFrom<NM.Lib.DataLibrary.United.Domain.MasterUserRoleDTO>
    {
        public int MasterUserID { get; set; }
        public int RoleID { get; set; }
    }
}