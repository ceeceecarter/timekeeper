using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class TSHoursTypeViewModel : IMapFrom<NM.Lib.DataLibrary.DataAccess.United.tblTSHoursType>
    {
        #region

        public int HoursTypeID { get; set; }
        public string HoursType { get; set; }
        public Nullable<bool> BothFLSA { get; set; }

        #endregion

    }
}