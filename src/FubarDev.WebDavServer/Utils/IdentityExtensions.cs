// <copyright file="IdentityExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Diagnostics.Contracts;
using System.Security.Principal;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Utils
{
    /// <summary>
    /// Extension methods for the <see cref="IIdentity"/> interface.
    /// </summary>
    public static class IdentityExtensions
    {
        /// <summary>
        /// Returns a value that indicates whether the identity is for an anonymous user.
        /// </summary>
        /// <param name="identity">The identity to check.</param>
        /// <returns><see langword="true"/> when <paramref name="identity"/> is anonymous.</returns>
        [Pure]
        public static bool IsAnonymous(this IIdentity? identity)
        {
            return string.IsNullOrEmpty(identity?.Name) || !identity.IsAuthenticated || identity.AuthenticationType == "anonymous";
        }

        /// <summary>
        /// Gets the owner to be used for locks, based on the current identity.
        /// </summary>
        /// <param name="identity">The identity to create the owner for.</param>
        /// <returns>The owner element.</returns>
        [Pure]
        public static XElement? GetOwnerHref(this IIdentity identity)
        {
            if (string.IsNullOrEmpty(identity.Name))
            {
                return null;
            }

            return new XElement(WebDavXml.Dav + "owner", identity.Name);
        }

        /// <summary>
        /// Gets the owner to be used for locks, based on the current identity.
        /// </summary>
        /// <param name="identity">The identity to create the owner for.</param>
        /// <returns>The owner element.</returns>
        [Pure]
        public static string? GetOwner(this IIdentity identity)
        {
            if (string.IsNullOrEmpty(identity.Name))
            {
                return null;
            }

            return identity.Name;
        }
    }
}
