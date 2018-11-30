using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class TSNavigationItemViewModel : IMapFrom<NM.Lib.DataLibrary.DataAccess.United.tblTSNavigation>
    {
        public int TabId { get; set; }
        public string LinkText { get; set; }
        public string ControllerName { get; set; }
        public string ActionName { get; set; }
        public string ElementNameId { get; set; }  
        public bool IsTSUser { get; set; }
        public bool IsTSManager { get; set; }
        public bool IsTSHRAdmin { get; set; }
        public int TabOrder { get; set; }
    }
}