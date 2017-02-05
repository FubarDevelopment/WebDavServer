// <copyright file="HttpMkColAttribute.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Mvc.Routing;

namespace FubarDev.WebDavServer.AspNetCore.Routing
{
    public class HttpMkColAttribute : HttpMethodAttribute
    {
        private static readonly IEnumerable<string> _supportedMethods = new[] { "MKCOL" };

        public HttpMkColAttribute()
            : base(_supportedMethods)
        {
        }

        public HttpMkColAttribute([NotNull] string template)
            : base(_supportedMethods, template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));
        }
    }
}
