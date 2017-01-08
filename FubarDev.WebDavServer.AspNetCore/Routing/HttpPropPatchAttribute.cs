using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Mvc.Routing;

namespace FubarDev.WebDavServer.AspNetCore.Routing
{
    public class HttpPropPatchAttribute : HttpMethodAttribute
    {
        private static readonly IEnumerable<string> _supportedMethods = new[] { "PROPPATCH" };

        public HttpPropPatchAttribute() : base(_supportedMethods)
        {
        }

        public HttpPropPatchAttribute([NotNull] string template) : base(_supportedMethods, template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));
        }
    }
}
