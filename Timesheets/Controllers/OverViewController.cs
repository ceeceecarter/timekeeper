using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NM.Web.WebApplication.Timesheets.Controllers
{
    public class OverViewController : BaseController
    {
        // GET: OverView
        public ActionResult Index()
        {
            Models.ViewModel.OverViewTabViewModel model = new Models.ViewModel.OverViewTabViewModel();
            //Mockup Time Off Summary
            List<Models.ViewModel.OverViewTimeOffSummaryViewModel> timeOffSummary = new List<Models.ViewModel.OverViewTimeOffSummaryViewModel>()
            {
                new Models.ViewModel.OverViewTimeOffSummaryViewModel() { EmployeeInfoID=1, FileNumberId=123, StatusCode="Non-Submitted", TimeOffTotal=0 },
                new Models.ViewModel.OverViewTimeOffSummaryViewModel() { EmployeeInfoID=1, FileNumberId=123, StatusCode="Submitted", TimeOffTotal=0 },
                new Models.ViewModel.OverViewTimeOffSummaryViewModel() { EmployeeInfoID=1, FileNumberId=123, StatusCode="Approved", TimeOffTotal=120 },
                new Models.ViewModel.OverViewTimeOffSummaryViewModel() { EmployeeInfoID=1, FileNumberId=123, StatusCode="Processed", TimeOffTotal=0 }
            };
            model.TimeOffSummary = timeOffSummary;

            return View("OverviewView", model);
        }
    }
} 