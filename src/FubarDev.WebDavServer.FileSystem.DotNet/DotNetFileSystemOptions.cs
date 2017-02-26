// <copyright file="DotNetFileSystemOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Linq;

namespace FubarDev.WebDavServer.FileSystem.DotNet
{
    /// <summary>
    /// The options for the <see cref="DotNetFileSystemFactory"/> and <see cref="DotNetFileSystem"/>
    /// </summary>
    public class DotNetFileSystemOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetFileSystemOptions"/> class.
        /// </summary>
        public DotNetFileSystemOptions()
        {
            var info = GetHomePath();
            RootPath = info.RootPath;
            AnonymousUserName = info.IsProbablyUnix ? "anonymous" : "Public";
        }

        /// <summary>
        /// Gets or sets the home path for all users
        /// </summary>
        public string RootPath { get; set; }

        /// <summary>
        /// Gets or sets the path name for the anonymous user
        /// </summary>
        public string AnonymousUserName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether infinite path depth is allowed
        /// </summary>
        public bool AllowInfiniteDepth { get; set; }

        private static HomePathInfo GetHomePath()
        {
            var homeEnvVars = new[] { "HOME", "USERPROFILE", "PUBLIC" };
            var home = homeEnvVars.Select(x => Tuple.Create(x, Environment.GetEnvironmentVariable(x))).First(x => !string.IsNullOrEmpty(x.Item2));
            var rootDir = Path.GetDirectoryName(home.Item2);
            return new HomePathInfo()
            {
                RootPath = rootDir,
                IsProbablyUnix = home.Item1 == "HOME",
            };
        }

        private class HomePathInfo
        {
            public string RootPath { get; set; }

            public bool IsProbablyUnix { get; set; }
        }
    }
}
