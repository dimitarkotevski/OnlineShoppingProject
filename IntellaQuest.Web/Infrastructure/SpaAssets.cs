using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Hosting;

namespace IntellaQuest.Web.Infrastructure
{
    /// <summary>
    /// Resolves the hashed file names produced by the Angular CLI build
    /// (my-app/bundles/browser), mirroring the wildcard bundles of the old MVC site.
    /// </summary>
    public sealed class SpaAssets
    {
        private const string RequestBase = "/my-app/bundles/browser/";

        public SpaAssets(IWebHostEnvironment environment)
        {
            var directory = Path.Combine(environment.ContentRootPath, "my-app", "bundles", "browser");

            Styles = Find(directory, "styles*.css");
            Polyfills = Find(directory, "polyfills*.js");
            Scripts = Find(directory, "scripts*.js");
            Main = Find(directory, "main*.js");
        }

        public string Styles { get; }

        public string Polyfills { get; }

        public string Scripts { get; }

        public string Main { get; }

        private static string Find(string directory, string pattern)
        {
            if (!Directory.Exists(directory))
            {
                return null;
            }

            var file = Directory.GetFiles(directory, pattern).OrderBy(f => f).LastOrDefault();
            return file == null ? null : RequestBase + Path.GetFileName(file);
        }
    }
}
