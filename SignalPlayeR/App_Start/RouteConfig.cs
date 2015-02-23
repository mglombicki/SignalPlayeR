using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SignalPlayeR
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Songs",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Search", action = "Songs" }
            );

            routes.MapRoute(
                name: "Duration",
                url: "{controller}/{action}/{title}/{artist}",
                defaults: new { controller = "Search", action = "Duration" }
            );
        }
    }
}
