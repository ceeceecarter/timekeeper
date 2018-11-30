using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using System.Web.Script.Serialization;

namespace NM.Web.WebApplication.Timesheets.Filters
{
    public class ValidateModelStateAttribute : ActionFilterAttribute 
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var viewData = filterContext.Controller.ViewData;
            if (!viewData.ModelState.IsValid)
            {
                filterContext.Result = new ViewResult
                {
                    ViewData = viewData,
                    TempData = filterContext.Controller.TempData
                };

                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    ProcessAjax(filterContext); 
                }
            }
            base.OnActionExecuting(filterContext);
        }
        protected virtual void ProcessAjax(ActionExecutingContext filterContext)
        {
            var viewData = filterContext.Controller.ViewData;
            string errorData = string.Empty;
            foreach(var item in filterContext.Controller.ControllerContext.RouteData.Values)
            {
                errorData += item.Value.ToString() + " >>> ";
            }
            foreach(var item in viewData.ModelState.Values)
            {
                if(item.Errors.Count() > 0)
                {
                    foreach(var i in item.Errors)
                    {
                        errorData += i.ErrorMessage + " >>> ";
                    }
                }
            }
            errorData = "Invalid ModelState >>> " + errorData;
            
            var errors = new HttpException(errorData);
            var json = new JavaScriptSerializer().Serialize(errors);
            filterContext.Result = new HttpStatusCodeResult((int)HttpStatusCode.BadRequest, json);
        }
    }
}