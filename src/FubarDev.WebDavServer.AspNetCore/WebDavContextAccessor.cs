// <copyright file="WebDavContextAccessor.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.AspNetCore.Contexts;
using FubarDev.WebDavServer.Utils;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.AspNetCore
{
    internal class WebDavContextAccessor : GenericWebDavContextAccessor<IWebDavContext>
    {
        private readonly bool _useUnescapedContext;

        public WebDavContextAccessor(
            IHttpContextAccessor httpContextAccessor,
            IOptions<LitmusCompatibilityOptions> litmusCompatibilityOptions)
            : base(httpContextAccessor)
        {
            _useUnescapedContext = litmusCompatibilityOptions.Value.UseUnescapedUri;
        }

        /// <inheritdoc />
        protected override IWebDavContext BuildContext(HttpContext httpContext)
        {
            if (_useUnescapedContext)
            {
                return ActivatorUtilities.CreateInstance<UnescapedWebDavContext>(
                    httpContext.RequestServices,
                    httpContext);
            }

            return ActivatorUtilities.CreateInstance<EscapedWebDavContext>(
                httpContext.RequestServices,
                httpContext);
        }
    }
}
