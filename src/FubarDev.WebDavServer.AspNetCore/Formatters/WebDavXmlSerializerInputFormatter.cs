// <copyright file="WebDavXmlSerializerInputFormatter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;

namespace FubarDev.WebDavServer.AspNetCore.Formatters
{
    /// <summary>
    /// The formatter for the WebDAV request body.
    /// </summary>
    public class WebDavXmlSerializerInputFormatter : XmlSerializerInputFormatter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WebDavXmlSerializerInputFormatter"/> class.
        /// </summary>
        /// <param name="options">The MVC options.</param>
        public WebDavXmlSerializerInputFormatter(MvcOptions options)
            : base(options)
        {
            SupportedMediaTypes.Add("text/plain");
        }

        /// <inheritdoc />
        public override bool CanRead(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            if (request.ContentType == null)
            {
                var contentLength = request.ContentLength;
                if (contentLength.GetValueOrDefault() == 0)
                {
                    // We allow that the following types have an optional body
                    switch (request.Method)
                    {
                        case "LOCK":
                        case "PROPFIND":
                            return true;
                    }
                }

                // The following HTTP methods should have an XML body
                // even when the content-type wasn't set.
                if (request.Method is "LOCK" or "PROPFIND" or "PROPPATCH")
                {
                    // Workaround for the litmus tool.
                    return true;
                }
            }

            return base.CanRead(context);
        }
    }
}
