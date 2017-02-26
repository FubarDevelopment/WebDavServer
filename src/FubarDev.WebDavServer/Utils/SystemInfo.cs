// <copyright file="SystemInfo.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Linq;
using System.Security.Principal;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Utils
{
    /// <summary>
    /// System information utility class
    /// </summary>
    public static class SystemInfo
    {
        /// <summary>
        /// Gets the home path of the user
        /// </summary>
        /// <param name="identity">The user to get the home path for</param>
        /// <param name="anonymousUserName">The user name for the unauthenticated user</param>
        /// <returns>The home path of the user</returns>
        [NotNull]
        public static string GetUserHomePath([NotNull] IIdentity identity, string anonymousUserName = null)
        {
            var rootPathInfo = GetHomePath();
            var userName = identity.IsAuthenticated
                ? identity.Name
                : (rootPathInfo.IsProbablyUnix ? (anonymousUserName ?? "anonymous") : (anonymousUserName ?? "Public"));
            var rootPath = Path.Combine(rootPathInfo.RootPath, userName);
            return rootPath;
        }

        /// <summary>
        /// Gets the home path information
        /// </summary>
        /// <returns>The found home path</returns>
        [NotNull]
        public static HomePathInfo GetHomePath()
        {
            var homeEnvVars = new[] { "HOME", "USERPROFILE", "PUBLIC" };
            var home = homeEnvVars.Select(x => Tuple.Create(x, Environment.GetEnvironmentVariable(x))).First(x => !string.IsNullOrEmpty(x.Item2));
            var rootDir = Path.GetDirectoryName(home.Item2);
            return new HomePathInfo(rootDir, home.Item1 == "HOME");
        }

        /// <summary>
        /// Information about the found home path
        /// </summary>
        public class HomePathInfo
        {
            internal HomePathInfo(string rootPath, bool isProbablyUnix)
            {
                RootPath = rootPath;
                IsProbablyUnix = isProbablyUnix;
            }

            /// <summary>
            /// Gets the root path for all users
            /// </summary>
            public string RootPath { get; set; }

            /// <summary>
            /// Gets a value indicating whether the root path is probably a unix OS
            /// </summary>
            public bool IsProbablyUnix { get; set; }
        }
    }
}
