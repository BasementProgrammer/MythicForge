using System.Data.Entity;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Amazon.XRay.Recorder.Handlers.AspNet;
using Amazon.XRay.Recorder.Handlers.AwsSdk;
using MythicForge.Data;

namespace MythicForge
{
    public class MvcApplication : HttpApplication
    {
        // Service name shown for this application in the AWS X-Ray console.
        private const string XRayServiceName = "MythicForge";

        /// <summary>
        /// Runs for every HttpApplication instance. Hook AWS X-Ray request tracing here so
        /// each incoming request produces an X-Ray segment (with child subsegments for the
        /// AWS SDK calls instrumented in Application_Start).
        /// </summary>
        public override void Init()
        {
            base.Init();
            AWSXRayASPNET.RegisterXRay(this, XRayServiceName);
        }

        protected void Application_Start()
        {
            // Trace all AWS SDK calls (e.g. Amazon Bedrock image generation) as X-Ray
            // subsegments. Must run before any AWS client is created.
            AWSSDKHandler.RegisterXRayForAllServices();

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
