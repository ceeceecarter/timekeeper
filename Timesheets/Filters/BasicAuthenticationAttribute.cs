using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;
using System.Web.Mvc;
using System.Web.Mvc.Filters;

namespace NM.Web.WebApplication.Timesheets.Filters
{
    public class BasicAuthenticationAttribute : ActionFilterAttribute, IAuthenticationFilter
    {
        void IAuthenticationFilter.OnAuthentication(AuthenticationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            var authorization = request.Headers["Authorization"];
            //No authorization, do nothing
            if (string.IsNullOrEmpty(authorization) || !authorization.Contains("Basic")) return;
            //parse username and password from header
            byte[] encodedDataAsBytes = Convert.FromBase64String(authorization.Replace("Basic", ""));
            string valStringEncode = Encoding.ASCII.GetString(encodedDataAsBytes);

            string userName = valStringEncode.Substring(0, valStringEncode.IndexOf(':'));
            string password = valStringEncode.Substring(valStringEncode.IndexOf(':') + 1);
        }

        void IAuthenticationFilter.OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            
        } 
    }
}