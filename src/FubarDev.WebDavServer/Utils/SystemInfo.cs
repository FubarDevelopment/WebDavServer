// <copyright file="SystemInfo.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;

namespace FubarDev.WebDavServer.Utils
{
    /// <summary>
    /// System information utility class.
    /// </summary>
    public static class SystemInfo
    {
        /// <summary>
        /// The name of the claim for the user home path.
        /// </summary>
        public const string UserHomePathClaim = "DAV:user-home-path";

        /// <summary>
        /// Gets the home path of the user.
        /// </summary>
        /// <param name="principal">The principal to get the home path for.</param>
        /// <param name="homePath">The home path to use.</param>
        /// <param name="anonymousUserName">The user name for the unauthenticated user.</param>
        /// <returns>The home path of the user.</returns>
        public static string GetUserHomePath(IPrincipal principal, string? homePath = null, string? anonymousUserName = null)
        {
            if (principal.Identity.IsAuthenticated && principal is ClaimsPrincipal homePathPrinciple)
            {
                var homePathClaim = homePathPrinciple
                    .Claims
                    .FirstOrDefault(c => c.Type == UserHomePathClaim && !string.IsNullOrEmpty(c.Value));
                if (!string.IsNullOrEmpty(homePathClaim?.Value))
                {
                    return homePathClaim.Value;
                }
            }

            var rootPathInfo = GetHomePath();
            var userName = !principal.Identity.IsAnonymous()
                ? principal.Identity.Name
                : (rootPathInfo.IsProbablyUnix
                    ? (anonymousUserName ?? "anonymous")
                    : (anonymousUserName ?? "Public"));
            var rootPath = Path.Combine(homePath ?? rootPathInfo.RootPath, userName);
            return rootPath;
        }

        /// <summary>
        /// Gets the home path information.
        /// </summary>
        /// <returns>The found home path.</returns>
        public static HomePathInfo GetHomePath()
        {
            var homeEnvVars = new[] { "HOME", "USERPROFILE", "PUBLIC" };
            var home = homeEnvVars.Select(x => Tuple.Create(x, Environment.GetEnvironmentVariable(x))).First(x => !string.IsNullOrEmpty(x.Item2));
            var rootDir = Path.GetDirectoryName(home.Item2);
            return new HomePathInfo(rootDir, home.Item1 == "HOME");
        }

        /// <summary>
        /// Information about the found home path.
        /// </summary>
        public class HomePathInfo
        {
            internal HomePathInfo(string rootPath, bool isProbablyUnix)
            {
                RootPath = rootPath;
                IsProbablyUnix = isProbablyUnix;
            }

            /// <summary>
            /// Gets or sets the root path for all users.
            /// </summary>
            public string RootPath { get; set; }

            /// <summary>
            /// Gets or sets a value indicating whether the root path is probably a unix OS.
            /// </summary>
            public bool IsProbablyUnix { get; set; }
        }
    }
}
