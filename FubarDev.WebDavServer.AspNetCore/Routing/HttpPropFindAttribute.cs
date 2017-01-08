using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Mvc.Routing;

namespace FubarDev.WebDavServer.AspNetCore.Routing
{
    public class HttpPropFindAttribute : HttpMethodAttribute
    {
        private static readonly IEnumerable<string> _supportedMethods = new[] { "PROPFIND" };

        public HttpPropFindAttribute() : base(_supportedMethods)
        {
        }

        public HttpPropFindAttribute([NotNull] string template) : base(_supportedMethods, template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));
        }
    }
}
