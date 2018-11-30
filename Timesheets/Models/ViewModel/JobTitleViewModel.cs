using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class JobTitleViewModel : IMapFrom<NM.Lib.DataLibrary.United.Domain.JobTitleDTO>
    {

        #region Properties

        public int JobTitleId { get; set; }
        public string JobTitleName { get; set; }

        #endregion

    }
}
