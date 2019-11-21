using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace ScrapingMVC
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            Application["Totaluser"] = 0;


        }

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
        public class NoDirectAccessAttribute : ActionFilterAttribute
        {
            public override void OnActionExecuting(ActionExecutingContext filterContext)
            {
                if (filterContext.HttpContext.Request.UrlReferrer == null ||
                            filterContext.HttpContext.Request.Url.Host != filterContext.HttpContext.Request.UrlReferrer.Host)
                {
                    filterContext.Result = new RedirectToRouteResult(new
                                   RouteValueDictionary(new { controller = "Home", action = "Dashboard", area = "" }));
                }
            }
        }
        protected void Application_BeginRequest()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));
            Response.Cache.SetNoStore();
        }

        public class NoCacheAttribute : ActionFilterAttribute
        {
            public override void OnResultExecuting(ResultExecutingContext filterContext)
            {
                var response = filterContext.HttpContext.Response;
                response.Cache.SetExpires(DateTime.UtcNow.AddDays(-1));
                response.Cache.SetValidUntilExpires(false);
                response.Cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
                response.Cache.SetCacheability(HttpCacheability.NoCache);
                response.Cache.SetNoStore();
            }
        }

        protected void Session_Start()
        {
            Application.Lock();
            Application["Totaluser"] = (int)Application["Totaluser"] + 1;
            Application.UnLock();
        }  
    }
}
