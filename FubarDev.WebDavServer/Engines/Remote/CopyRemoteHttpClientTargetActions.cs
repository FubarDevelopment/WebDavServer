// <copyright file="CopyRemoteHttpClientTargetActions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;

namespace FubarDev.WebDavServer.Engines.Remote
{
    public class CopyRemoteHttpClientTargetActions : RemoteHttpClientTargetActions
    {
        public CopyRemoteHttpClientTargetActions(HttpClient httpClient)
            : base(httpClient)
        {
        }

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

        public override Task ExecuteAsync(ICollection source, CancellationToken cancellationToken)
        {
            return Task.FromResult(0);
        }
    }
}
