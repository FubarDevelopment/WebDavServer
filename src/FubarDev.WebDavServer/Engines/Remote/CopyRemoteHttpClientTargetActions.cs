// <copyright file="CopyRemoteHttpClientTargetActions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Engines.Remote
{
    /// <summary>
    /// The <see cref="ITargetActions{TCollection,TDocument,TMissing}"/> implementation that copies entries between servers.
    /// </summary>
    public class CopyRemoteHttpClientTargetActions : RemoteHttpClientTargetActions, IRemoteCopyTargetActions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CopyRemoteHttpClientTargetActions"/> class.
        /// </summary>
        /// <param name="context">The current WebDAV context.</param>
        /// <param name="httpClient">The <see cref="HttpClient"/> to use.</param>
        public CopyRemoteHttpClientTargetActions(IWebDavContext context, HttpClient httpClient)
            : base(context, httpClient)
        {
        }

        /// <inheritdoc />
        public override async Task<RemoteDocumentTarget> ExecuteAsync(IDocument source, RemoteMissingTarget destination, CancellationToken cancellationToken)
        {
            using (var stream = await source.OpenReadAsync(cancellationToken).ConfigureAwait(false))
            {
                var content = new StreamContent(stream);
                using (var response = await Client
                    .PutAsync(destination.DestinationUrl, content, cancellationToken)
                    .ConfigureAwait(false))
                {
                    response.EnsureSuccessStatusCode();
                }
            }

            return new RemoteDocumentTarget(destination.Parent, destination.Name, destination.DestinationUrl, this);
        }

        /// <inheritdoc />
        public override async Task<ActionResult> ExecuteAsync(IDocument source, RemoteDocumentTarget destination, CancellationToken cancellationToken)
        {
            try
            {
                using (var stream = await source.OpenReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    var content = new StreamContent(stream);
                    var request = new HttpRequestMessage(HttpMethod.Put, destination.DestinationUrl)
                    {
                        Content = content,
                        Headers =
                        {
                            { "Overwrite", "T" },
                        },
                    };

                    using (var response = await Client
                        .SendAsync(request, cancellationToken)
                        .ConfigureAwait(false))
                    {
                        response.EnsureSuccessStatusCode();
                    }
                }
            }
            catch (Exception ex)
            {
                return new ActionResult(ActionStatus.OverwriteFailed, destination)
                {
                    Exception = ex,
                };
            }

            return new ActionResult(ActionStatus.Overwritten, destination);
        }

        /// <inheritdoc />
        public override Task CleanupAsync(
            ICollection source,
            RemoteCollectionTarget destination,
            IEnumerable<ActionResult> childResults,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
