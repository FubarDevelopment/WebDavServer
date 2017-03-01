// <copyright file="IUserHome.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Account
{
    /// <summary>
    /// Interface that provides a user homoe path for a <see cref="System.Security.Principal.IPrincipal"/>
    /// </summary>
    public interface IUserHome
    {
        /// <summary>
        /// Gets the users home path
        /// </summary>
        [NotNull]
        string HomePath { get; }
    }
}
