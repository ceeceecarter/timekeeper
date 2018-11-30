using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel 
{
    public class TSStatusTypeViewModel : IMapFrom<NM.Lib.DataLibrary.DataAccess.United.tblTSStatusType>
    {
        #region Properties

        public int StatusTypeID { get; set; }
        public string StatusType { get; set; }

        #endregion
    }
}