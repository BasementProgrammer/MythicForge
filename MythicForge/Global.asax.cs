using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using MythicForge.Data;

namespace MythicForge
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // Register the initializer that drops, recreates and reseeds the
            // local database, then force it to run immediately so the app
            // always starts from a clean, fully populated database.
            Database.SetInitializer(new SampleDbInitializer());
            using (var context = new SampleDbContext())
            {
                context.Database.Initialize(force: true);
            }

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
