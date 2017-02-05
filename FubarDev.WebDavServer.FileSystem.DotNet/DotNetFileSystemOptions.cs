using System;
using System.IO;
using System.Linq;

namespace FubarDev.WebDavServer.FileSystem.DotNet
{
    public class DotNetFileSystemOptions
    {
        public DotNetFileSystemOptions()
        {
            var info = GetHomePath();
            RootPath = info.RootPath;
            AnonymousUserName = info.IsProbablyUnix ? "anonymous" : "Public";
        }

        public string RootPath { get; set; }

        public string AnonymousUserName { get; set; }

        public bool AllowInfiniteDepth { get; set; }

        private static HomePathInfo GetHomePath()
        {
            var homeEnvVars = new[] { "HOME", "USERPROFILE", "PUBLIC" };
            var home = homeEnvVars.Select(x => Tuple.Create(x, Environment.GetEnvironmentVariable(x))).First(x => !string.IsNullOrEmpty(x.Item2));
            var rootDir = Path.GetDirectoryName(home.Item2);
            return new HomePathInfo()
            {
                RootPath = rootDir,
                IsProbablyUnix = home.Item1 == "HOME"
            };
        }

        private class HomePathInfo
        {
            public string RootPath { get; set; }

            public bool IsProbablyUnix { get; set; }
        }
    }
}
