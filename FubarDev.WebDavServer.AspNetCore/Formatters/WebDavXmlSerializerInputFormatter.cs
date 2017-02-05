// <copyright file="WebDavXmlSerializerInputFormatter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc.Formatters;

namespace FubarDev.WebDavServer.AspNetCore.Formatters
{
    public class WebDavXmlSerializerInputFormatter : XmlSerializerInputFormatter
    {
        public override bool CanRead(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            if (request.ContentType == null)
            {
                var contentLength = request.ContentLength;
                if (contentLength.HasValue && contentLength.Value == 0)
                {
                    switch (request.Method)
                    {
                        case "PROPFIND":
                            return true;
                    }
                }
            }

            return base.CanRead(context);
        }
    }
}
