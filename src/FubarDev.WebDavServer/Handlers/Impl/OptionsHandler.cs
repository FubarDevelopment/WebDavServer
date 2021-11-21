// <copyright file="OptionsHandler.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.Handlers.Impl
{
    /// <summary>
    /// Implementation of the <see cref="IOptionsHandler"/> interface.
    /// </summary>
    public class OptionsHandler : IOptionsHandler
    {
        private readonly IFileSystem _rootFileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="OptionsHandler"/> class.
        /// </summary>
        /// <param name="rootFileSystem">The root file system.</param>
        public OptionsHandler(IFileSystem rootFileSystem)
        {
            _rootFileSystem = rootFileSystem;
        }

        /// <inheritdoc />
        public IEnumerable<string> HttpMethods { get; } = new[] { "OPTIONS" };

        /// <inheritdoc />
        public async Task<IWebDavResult> OptionsAsync(string path, CancellationToken cancellationToken)
        {
            var selectionResult = await _rootFileSystem.SelectAsync(path, cancellationToken).ConfigureAwait(false);
            var targetFileSystem = selectionResult.TargetFileSystem;
            return new WebDavOptionsResult(targetFileSystem);
        }

        private class WebDavOptionsResult : WebDavResult
        {
            private readonly IFileSystem _targetFileSystem;

            public WebDavOptionsResult(IFileSystem targetFileSystem)
                : base(WebDavStatusCode.OK)
            {
                _targetFileSystem = targetFileSystem;
            }

            public override Task ExecuteResultAsync(IWebDavResponse response, CancellationToken ct)
            {
                IImmutableDictionary<string, IEnumerable<string>> headers = ImmutableDictionary<string, IEnumerable<string>>.Empty;

                foreach (var webDavClass in response.Context.Dispatcher.SupportedClasses)
                {
                    headers = AddHeaderValues(headers, webDavClass.OptionsResponseHeaders);
                }

                if (_targetFileSystem.SupportsRangedRead)
                {
                    Headers["Accept-Ranges"] = new[] { "bytes" };
                }

                foreach (var header in headers)
                {
                    Headers[header.Key] = header.Value;
                }

                Headers["MS-Author-Via"] = new[] { "DAV" };

                return base.ExecuteResultAsync(response, ct);
            }
        }
    }
}
