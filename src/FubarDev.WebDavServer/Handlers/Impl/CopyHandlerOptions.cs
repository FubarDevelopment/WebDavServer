// <copyright file="CopyHandlerOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Handlers.Impl
{
    /// <summary>
    /// Options for the <see cref="CopyHandler"/>.
    /// </summary>
    public class CopyHandlerOptions
    {
        /// <summary>
        /// Gets or sets the mode that determines the method used to copy files.
        /// </summary>
        public RecursiveProcessingMode Mode { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the destination of a copy operation is overwriteable when
        /// the client doesn't specify the <see cref="Model.Headers.OverwriteHeader"/>.
        /// </summary>
        public bool OverwriteAsDefault { get; set; } = true;
    }
}
