// <copyright file="WebDavException.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// The WebDAV exception.
    /// </summary>
    public class WebDavException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavException"/> class.
        /// </summary>
        /// <param name="statusCode">The WebDAV status code.</param>
        public WebDavException(WebDavStatusCode statusCode)
            : base(statusCode.GetReasonPhrase())
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavException"/> class.
        /// </summary>
        /// <param name="statusCode">The WebDAV status code.</param>
        /// <param name="innerException">The inner exception.</param>
        public WebDavException(WebDavStatusCode statusCode, [NotNull] Exception innerException)
            : base(statusCode.GetReasonPhrase(innerException.Message), innerException)
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavException"/> class.
        /// </summary>
        /// <param name="statusCode">The WebDAV status code.</param>
        /// <param name="responseMessage">The reason phrase for the status code.</param>
        public WebDavException(WebDavStatusCode statusCode, string responseMessage)
            : base(statusCode.GetReasonPhrase(responseMessage))
        {
            StatusCode = statusCode;
        }

        /// <summary>
        /// Gets the WebDAV status code.
        /// </summary>
        public WebDavStatusCode StatusCode { get; }
    }
}
