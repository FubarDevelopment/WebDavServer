// <copyright file="RemoteTargetException.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Engines.Remote
{
    /// <summary>
    /// The exception for a failed remote operation
    /// </summary>
    public class RemoteTargetException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteTargetException"/> class.
        /// </summary>
        public RemoteTargetException()
        {
            Href = new Uri[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteTargetException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        public RemoteTargetException(string message)
            : base(message)
        {
            Href = new Uri[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteTargetException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="innerException">The inner exception</param>
        public RemoteTargetException(string message, Exception innerException)
            : base(message, innerException)
        {
            Href = new Uri[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteTargetException"/> class.
        /// </summary>
        /// <param name="href">The <c>href</c> of the failed operation</param>
        public RemoteTargetException(IReadOnlyCollection<Uri> href)
        {
            Href = href;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteTargetException"/> class.
        /// </summary>
        /// <param name="href">The <c>href</c>s of the failed operation</param>
        public RemoteTargetException(params Uri[] href)
        {
            Href = href;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteTargetException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="href">The <c>href</c>s of the failed operation</param>
        public RemoteTargetException(string message, IReadOnlyCollection<Uri> href)
            : base(message)
        {
            Href = href;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteTargetException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="href">The <c>href</c>s of the failed operation</param>
        public RemoteTargetException(string message, params Uri[] href)
            : base(message)
        {
            Href = href;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteTargetException"/> class.
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="href">The <c>href</c>s of the failed operation</param>
        /// <param name="innerException">The inner exception</param>
        public RemoteTargetException(string message, IReadOnlyCollection<Uri> href, Exception innerException)
            : base(message, innerException)
        {
            Href = href;
        }

        /// <summary>
        /// Gets the <c>href</c>s of the failed operation
        /// </summary>
        [NotNull]
        [ItemNotNull]
        public IReadOnlyCollection<Uri> Href { get; }
    }
}
