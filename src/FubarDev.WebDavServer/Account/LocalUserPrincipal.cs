// <copyright file="LocalUserPrincipal.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Security.Claims;
using System.Security.Principal;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Account
{
    /// <summary>
    /// A principal for a local user
    /// </summary>
    public class LocalUserPrincipal : ClaimsPrincipal, IUserHome
    {
        private readonly IPrincipal _principal;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalUserPrincipal"/> class.
        /// </summary>
        /// <param name="identity">The identity of the principal</param>
        /// <param name="homePath">The home path of the user</param>
        /// <param name="roles">The roles this user is in</param>
        public LocalUserPrincipal([NotNull] IIdentity identity, [NotNull] string homePath, [NotNull] [ItemNotNull] params string[] roles)
            : this(new GenericPrincipal(identity, roles), homePath)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalUserPrincipal"/> class.
        /// </summary>
        /// <param name="principal">The underlying principal</param>
        /// <param name="homePath">The home path of the user</param>
        public LocalUserPrincipal([NotNull] IPrincipal principal, [NotNull] string homePath)
            : base(principal)
        {
            _principal = principal;
            HomePath = homePath;
        }

        /// <inheritdoc />
        public string HomePath { get; }

        /// <inheritdoc />
        public override bool IsInRole(string role)
        {
            return _principal.IsInRole(role) || base.IsInRole(role);
        }
    }
}
