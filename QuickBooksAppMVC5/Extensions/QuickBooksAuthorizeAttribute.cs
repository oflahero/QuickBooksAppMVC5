using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;

namespace QuickBooksAppMVC5.Extensions
{
    public class QuickBooksAuthorizeAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            base.OnAuthorization(filterContext);
            if (filterContext.Result == null)
            {
                var httpContext = filterContext.HttpContext;
                var currentUser = httpContext.GetOwinContext().GetUserManager<ApplicationUserManager>().FindById(
                    httpContext.User.Identity.GetUserId());

                if (currentUser != null && httpContext.User.Identity.IsAuthenticated)
                {
                    var currentController = (string)filterContext.RouteData.Values["controller"];

                    if (currentUser.OAuthProfileId == null && currentController != "OAuth")
                    {
                        filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary()
                                { {"controller", "OAuth"},{"action", "Initiate"}, {"returnURL", filterContext.HttpContext.Request.Url.AbsoluteUri } });
                    }
                }
            }
        }
    }
}