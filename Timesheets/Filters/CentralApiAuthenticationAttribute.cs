using System;
using System.Web.Mvc;
using System.Web.Mvc.Filters;
using System.Configuration;
using System.Web;
using NM.Lib.CommonLibrary;
using NM.Lib.CentralModel.Domain;
using NM.Lib.CentralApiLibrary.Interface;

namespace NM.Web.WebApplication.Timesheets.Filters
{
    public class CentralApiAuthenticationAttribute: ActionFilterAttribute, IAuthenticationFilter
    {
        //TODO: Remove this variable once development and testing is done
        public string testUserName = ConfigurationManager.AppSettings["TestUserName"];

        void IAuthenticationFilter.OnAuthentication(AuthenticationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            var authorization = request.Headers["Authorization"]; 

            //Get Central API AuthToken for the Authenticated Windows User
            AuthToken token = new AuthToken();
            var currentUser = request.LogonUserIdentity.Name.Split(new string[] { "\\" }, StringSplitOptions.None);
            token.ApplicationName = CommonSettings.ApplicationName();
            token.DomainName = currentUser[0];
            token.UserName = currentUser[1];
            token.UserName = testUserName; //change the variable value in the web.config (for testing)

            NM.Lib.CentralApiLibrary.CentralFactory cf = new NM.Lib.CentralApiLibrary.CentralFactory(token, NM.Lib.CentralApiLibrary.CentralFactory.ServiceHandler.Auth);
            IAuthServiceHandler authServiceHandler = (IAuthServiceHandler)cf.GetServiceHandler();
            AuthToken result = (AuthToken)authServiceHandler.ValidateUser();

            //Save MasterUserId in a Response.SeCookie()
            if (result != null)
            {
                filterContext.HttpContext.Response.Cookies.Add(new HttpCookie("MasterUserId") { Value = result.MasterUserId.ToString(), Expires = DateTime.Now.AddDays(-1),Path="/" });
                filterContext.HttpContext.Response.Cookies.Add(new HttpCookie("SessionId") { Value = result.SessionID.ToString(), Expires = DateTime.Now.AddDays(-1), Path="/" });
            }
        }

        void IAuthenticationFilter.OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {

        }
    }
}
