// <copyright file="OptionsHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Linq;
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

            public override async Task ExecuteResultAsync(IWebDavResponse response, CancellationToken ct)
            {
                await base.ExecuteResultAsync(response, ct).ConfigureAwait(false);
                response.Headers["Allow"] = response.Dispatcher.SupportedHttpMethods.ToArray();
                response.Headers["Accept-Ranges"] = new[] { "bytes" };
            }
        }
    }
}
