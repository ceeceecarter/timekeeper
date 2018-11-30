using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class MasterUserViewModel : IMapFrom<NM.Lib.DataLibrary.DataAccess.United.tblMasterUser>
    {
        public int MasterUserID { get; set; }
        public string UserName { get; set; }
        public string DomainName { get; set; }
        public string MRIUserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public Nullable<bool> AcceptedUPDirect { get; set; }
        public int Version { get; set; }
        public Nullable<int> ModifiedByMasterUserID { get; set; }
        public Nullable<System.DateTime> ModifiedDatetime { get; set; }
        public int CreatedByMasterUserID { get; set; }
        public System.DateTime CreatedDatetime { get; set; }

        public List<MasterUserRoleViewModel> MasterUserRoles { get; set; }

        public MasterUserViewModel()
        {
            MasterUserRoles = new List<MasterUserRoleViewModel>();
        }
    }
}