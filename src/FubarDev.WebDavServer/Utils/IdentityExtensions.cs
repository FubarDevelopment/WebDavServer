// <copyright file="IdentityExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Security.Principal;

namespace FubarDev.WebDavServer.Utils
{
    /// <summary>
    /// Extension methods for the <see cref="IIdentity"/> interface
    /// </summary>
    public static class IdentityExtensions
    {
        /// <summary>
        /// Returns a value that indicates whether the identity is for an anonymous user.
        /// </summary>
        /// <param name="identity">The identity to check</param>
        /// <returns><see langword="true"/> when <paramref name="identity"/> is anonymous</returns>
        public static bool IsAnonymous(this IIdentity identity)
        {
            return string.IsNullOrEmpty(identity.Name) || !identity.IsAuthenticated || identity.AuthenticationType == "anonymous";
        }
    }
}
