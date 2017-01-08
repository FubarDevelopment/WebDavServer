using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using Microsoft.AspNetCore.Mvc.Routing;

namespace FubarDev.WebDavServer.AspNetCore.Routing
{
    public class HttpCopyAttribute : HttpMethodAttribute
    {
        private static readonly IEnumerable<string> _supportedMethods = new[] { "COPY" };

        public HttpCopyAttribute() : base(_supportedMethods)
        {
        }

        public HttpCopyAttribute([NotNull] string template) : base(_supportedMethods, template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));
        }
    }
}
