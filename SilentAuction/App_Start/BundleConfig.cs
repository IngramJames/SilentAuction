using System.Web;
using System.Web.Optimization;

namespace SilentAuction
{
	public class BundleConfig
	{
		// For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
		public static void RegisterBundles(BundleCollection bundles)
		{
			bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
						"~/Scripts/jquery-{version}.js"));

			bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
						"~/Scripts/jquery.validate*"));

			// Use the development version of Modernizr to develop with and learn from. Then, when you're
			// ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
			bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
						"~/Scripts/modernizr-*"));

			bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
					  "~/Scripts/bootstrap.js",
					  "~/Scripts/respond.js"));

			bundles.Add(new StyleBundle("~/Content/css").Include(
					  "~/Content/bootstrap.css",
					  "~/Content/site.css"));

			// To install:
			// PM> Install-Package dropzone
			bundles.Add(new ScriptBundle("~/bundles/dropzone").Include(
					 "~/Scripts/dropzone/dropzone.js"));

			bundles.Add(new StyleBundle("~/Content/dropzonescss").Include(
                     "~/Scripts/dropzone/basic.css",
                     "~/Scripts/dropzone/dropzone.css"));

            // lightface (for nice dialog boxes)
            bundles.Add(new ScriptBundle("~/bundles/lightface").Include(
                "~/Content/3rdparty/darkwing-LightFace-9cbc329/dependencies/mootools.js",
                "~/Content/3rdparty/darkwing-LightFace-9cbc329/Source/LightFace.js"));
            bundles.Add(new StyleBundle("~/bundles/lightfacecss").Include("~/Content/3rdparty/darkwing-LightFace-9cbc329/Assets/LightFace.css"));

            // Sortable (for nice sortable and draggable lists)
            bundles.Add(new StyleBundle("~/bundles/sortablecss").Include("~/Content/Sortable.css"));

            // Flasher
            bundles.Add(new ScriptBundle("~/bundles/flasher").Include(
                "~/scripts/Common/Flasher.js",
                "~/scripts/Common/getCssColor.js"
                ));

            bundles.Add(new ScriptBundle("~/bundles/lightbox").Include(
                "~/scripts/lightbox-{version}.js"
                ));

            bundles.Add(new StyleBundle("~/bundles/lightboxcss").Include(
                "~/Content/lightbox.css"
                ));
        }
    }
}
