using System.IO;
using System.Linq;
using System.Web.Hosting;
using System.Web.Optimization;

namespace MythicForge
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            // The jQuery/Bootstrap/Modernizr client libraries come from NuGet content
            // packages and may not be present in a clean CI build/deployment. Only
            // register includes for assets that actually exist on disk so bundle
            // resolution never throws "Directory does not exist" and the (server
            // rendered) pages work regardless. Unresolved @Scripts.Render calls for
            // bundles that weren't registered render as no-ops.
            var scriptsDir = HostingEnvironment.MapPath("~/Scripts");
            if (scriptsDir != null && Directory.Exists(scriptsDir))
            {
                if (Directory.EnumerateFiles(scriptsDir, "jquery-*.js").Any())
                    bundles.Add(new ScriptBundle("~/bundles/jquery").Include("~/Scripts/jquery-{version}.js"));

                if (Directory.EnumerateFiles(scriptsDir, "jquery.validate*").Any())
                    bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include("~/Scripts/jquery.validate*"));

                if (Directory.EnumerateFiles(scriptsDir, "modernizr-*").Any())
                    bundles.Add(new ScriptBundle("~/bundles/modernizr").Include("~/Scripts/modernizr-*"));

                if (Directory.EnumerateFiles(scriptsDir, "bootstrap*").Any())
                    bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include("~/Scripts/bootstrap*"));
            }

            var contentDir = HostingEnvironment.MapPath("~/Content");
            var css = new StyleBundle("~/Content/css");
            if (contentDir != null)
            {
                if (File.Exists(Path.Combine(contentDir, "bootstrap.css")))
                    css.Include("~/Content/bootstrap.css");
                if (File.Exists(Path.Combine(contentDir, "Site.css")))
                    css.Include("~/Content/Site.css");
            }
            bundles.Add(css);

            BundleTable.EnableOptimizations = false;
        }
    }
}
