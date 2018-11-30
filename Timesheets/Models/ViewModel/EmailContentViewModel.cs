using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NM.Web.WebApplication.Timesheets.Infrastructure.Mapping;


namespace NM.Web.WebApplication.Timesheets.Models.ViewModel
{
    public class EmailContentViewModel : IMapFrom<NM.Lib.DataLibrary.DataAccess.United.tblTSEmailContent>
    {
        public int EmailContentID { get; set; }
        public string EmainContentName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool showOnEdit { get; set; } 
    }
}