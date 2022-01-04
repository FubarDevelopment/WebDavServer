// <copyright file="PropFindHandlerOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Diagnostics.CodeAnalysis;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    /// <summary>
    /// Options for <see cref="PropFindHandler"/>.
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global", Justification = "This is an options class")]
    public class PropFindHandlerOptions
    {
        /// <summary>
        /// Gets or sets the maximum cost for default properties.
        /// </summary>
        public int MaxDefaultCost { get; set; }

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
