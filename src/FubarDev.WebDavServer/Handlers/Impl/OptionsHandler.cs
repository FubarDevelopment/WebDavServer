// <copyright file="OptionsHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    /// <summary>
    /// Implementation of the <see cref="IOptionsHandler"/> interface.
    /// </summary>
    public class OptionsHandler : IOptionsHandler
    {
        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; } = new[] { "OPTIONS" };

        /// <inheritdoc />
        public Task<IWebDavResult> OptionsAsync(string path, CancellationToken cancellationToken)
        {
            return Task.FromResult<IWebDavResult>(new WebDavOptionsResult());
        }

        private class WebDavOptionsResult : WebDavResult
        {
            public WebDavOptionsResult()
                : base(WebDavStatusCode.OK)
            {
            }

            public override Task ExecuteResultAsync(IWebDavResponse response, CancellationToken ct)
            {
                IImmutableDictionary<string, IEnumerable<string>> headers = ImmutableDictionary<string, IEnumerable<string>>.Empty;

                foreach (var webDavClass in response.Dispatcher.SupportedClasses)
                    headers = AddHeaderValues(headers, webDavClass.OptionsResponseHeaders);

                foreach (var header in headers)
                    Headers[header.Key] = header.Value;

                return base.ExecuteResultAsync(response, ct);
            }
        }
    }
}
