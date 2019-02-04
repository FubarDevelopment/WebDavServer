﻿// <copyright file="HttpUnlockAttribute.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Mvc.Routing;

namespace FubarDev.WebDavServer.AspNetCore.Routing
{
    /// <summary>
    /// The WebDAV HTTP UNLOCK method.
    /// </summary>
    public class HttpUnlockAttribute : HttpMethodAttribute
    {
        private static readonly IEnumerable<string> _supportedMethods = new[] { "UNLOCK" };

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpUnlockAttribute"/> class.
        /// </summary>
        public HttpUnlockAttribute()
            : base(_supportedMethods)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpUnlockAttribute"/> class.
        /// </summary>
        /// <param name="template">The route template. May not be null.</param>
        public HttpUnlockAttribute([NotNull] string template)
            : base(_supportedMethods, template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }
        }
    }
}
