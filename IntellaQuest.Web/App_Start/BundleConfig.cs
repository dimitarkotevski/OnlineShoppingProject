using System.Web.Optimization;

namespace IntellaQuest.Web
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-3.7.0.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                     "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                     "~/Scripts/bootstrap.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                     "~/Content/bootstrap_theme.css",
                     "~/Content/site.css"));

            bundles.Add(new StyleBundle("~/bundles/vendor").Include(
                     "~/Scripts/angular.js",
                     "~/Scripts/angular-messages.js",
                     "~/Scripts/angular-ui-router.js",
                     "~/Scripts/angular-ui/ui-bootstrap.js",
                     "~/Scripts/angular-ui/ui-bootstrap-tpls.js"
                     ));

            bundles.Add(new Bundle("~/bundles/clientapp").Include(
                "~/my-app/bundles/browser/runtime*",
                "~/my-app/bundles/browser/polyfills*",
                "~/my-app/bundles/browser/main*",
                "~/my-app/bundles/browser/vendor*"));

            bundles.Add(new StyleBundle("~/Content/clientapp").Include(
                      "~/my-app/bundles/browser/styles*"));
        }
    }
}
