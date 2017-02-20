// <copyright file="PropFindHandlerOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Handlers.Impl
{
    /// <summary>
    /// Options for <see cref="PropFindHandler"/>
    /// </summary>
    public class PropFindHandlerOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the PROPFIND handler should return absolute href values.
        /// </summary>
        public bool UseAbsoluteHref { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the handler should omit the <code>owner</code> element of an <code>activelock</code> element.
        /// </summary>
        public bool OmitLockOwner { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the handler should omit the <code>locktoken</code> element of an <code>activelock</code> element.
        /// </summary>
        public bool OmitLockToken { get; set; }
    }
}
