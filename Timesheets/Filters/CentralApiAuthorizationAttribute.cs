using System;
using System.Web.Mvc;
using NM.Lib.CentralApiLibrary.Interface;
using System.Configuration;
using System.Web;
using NM.Lib.CommonLibrary;
using NM.Lib.CentralModel.Domain;

namespace NM.Web.WebApplication.Timesheets.Filters
{
    public class CentralApiAuthorizationAttribute : ActionFilterAttribute, IAuthorizationFilter
    {
        //TODO: Remove this variable once development and testing is done
        public string testUserName = ConfigurationManager.AppSettings["TestUserName"];
        public string testUserDomain = ConfigurationManager.AppSettings["TestUserDomainName"];

        public void OnAuthorization(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.Response.Cookies.Count == 0) 
            {
                var request = filterContext.HttpContext.Request;
                var authorization = request.Headers["Authorization"];
                //var environment = Properties.Settings.Default.CurrentEnvironment.ToUpper();

                //Get Central API AuthToken for the Authenticated Windows User
                AuthToken token = new AuthToken();

                var currentUser = request.LogonUserIdentity.Name.Split(new string[] { "\\" }, StringSplitOptions.None);
                token.ApplicationName = CommonSettings.ApplicationName();
                //token.DomainName = currentUser[0];
                //token.UserName = currentUser[1];
                token.DomainName = testUserDomain == string.Empty ? currentUser[0] : testUserDomain;
                token.UserName = testUserName == string.Empty ? currentUser[1] : testUserName;

                NM.Lib.CentralApiLibrary.CentralFactory cf = new NM.Lib.CentralApiLibrary.CentralFactory(token, NM.Lib.CentralApiLibrary.CentralFactory.ServiceHandler.Auth);
                IAuthServiceHandler authServiceHandler = (IAuthServiceHandler)cf.GetServiceHandler();
                AuthToken result = (AuthToken)authServiceHandler.ValidateUser();

                filterContext.HttpContext.Response.Cookies.Clear();

                //Save MasterUserId in a Response.Cookie()
                if (result != null)
                {
                    filterContext.HttpContext.Response.Cookies.Add(new HttpCookie("MasterUserId") { Value = result.MasterUserId.ToString(), Expires = DateTime.Now.AddDays(-1), Path = "/" });
                    filterContext.HttpContext.Response.Cookies.Add(new HttpCookie("SessionId") { Value = result.SessionID.ToString(), Expires = DateTime.Now.AddDays(-1), Path = "/" });
                }
            }

        }
    }
}
