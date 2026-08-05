using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using MythicForge.Data;
using MythicForge.Services;

namespace MythicForge
{
    public class MvcApplication : HttpApplication
    {
        protected void Application_Start()
        {
            // Start OpenTelemetry tracing before any outgoing HTTP call is made, so
            // HttpClient instrumentation captures them (this replaces the former AWS X-Ray
            // tracing). Incoming request tracing is handled by the TelemetryHttpModule
            // registered in Web.config plus AddAspNetInstrumentation(). Telemetry runs in
            // every environment.
            OpenTelemetryConfig.Initialize();

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

        protected void Application_End()
        {
            // Flush and dispose the tracer provider so buffered spans are exported when
            // the application shuts down or the app pool recycles.
            OpenTelemetryConfig.Shutdown();
        }
    }
}
