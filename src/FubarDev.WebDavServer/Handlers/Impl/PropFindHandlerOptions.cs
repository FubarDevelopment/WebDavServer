﻿// <copyright file="PropFindHandlerOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Handlers.Impl
{
    /// <summary>
    /// Options for <see cref="PropFindHandler"/>.
    /// </summary>
    public class PropFindHandlerOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether the PROPFIND handler should return absolute href values.
        /// </summary>
        public bool UseAbsoluteHref { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the handler should omit the <c>owner</c> element of an <c>activelock</c> element.
        /// </summary>
        public bool OmitLockOwner { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the handler should omit the <c>locktoken</c> element of an <c>activelock</c> element.
        /// </summary>
        public bool OmitLockToken { get; set; }
    }
}
