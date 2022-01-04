// <copyright file="LitmusCompatibilityOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Utils
{
    /// <summary>
    /// Options to configure behavior different from what the litmus tool expects.
    /// </summary>
    /// <seealso href="http://www.webdav.org/neon/litmus/"/>
    public class LitmusCompatibilityOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether overwriting a document with a collection should be forbidden.
        /// </summary>
        public bool ForbidOverwriteOfDocumentWithCollection { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether overwriting a collection with a document should be forbidden.
        /// </summary>
        public bool ForbidOverwriteOfCollectionWithDocument { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the URL encoding of the response HREF should be disabled.
        /// </summary>
        public bool DisableUrlEncodingOfResponseHref { get; set; }
    }
}
