// <copyright file="WebDavXmlResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer
{
    public class WebDavXmlResult : WebDavResult
    {
        [NotNull]
        private readonly XElement _element;

        public WebDavXmlResult(WebDavStatusCode statusCode, [NotNull] XElement element)
            : base(statusCode)
        {
            _element = element;
        }

        public override async Task ExecuteResultAsync(IWebDavResponse response, CancellationToken ct)
        {
            await base.ExecuteResultAsync(response, ct).ConfigureAwait(false);
            response.Dispatcher.Formatter.Serialize(response.Body, _element);
        }
    }
}
