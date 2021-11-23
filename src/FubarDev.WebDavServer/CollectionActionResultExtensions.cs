// <copyright file="CollectionActionResultExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using FubarDev.WebDavServer.Engines;
using FubarDev.WebDavServer.Engines.Local;
using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// Extension methods for the <see cref="CollectionActionResult"/>.
    /// </summary>
    public static class CollectionActionResultExtensions
    {
        /// <summary>
        /// Evaluate the result of a <see cref="CollectionActionResult"/> and return a <see cref="IWebDavResult"/> implementation object.
        /// </summary>
        /// <param name="collectionResult">The <see cref="CollectionActionResult"/> to evaluate.</param>
        /// <param name="context">The <see cref="IWebDavContext"/> to create the response for.</param>
        /// <returns>The created response.</returns>
        public static IWebDavResult Evaluate(this CollectionActionResult collectionResult, IWebDavContext context)
        {
            if (collectionResult.Status == ActionStatus.Ignored)
            {
                var documentActionStatus =
                    collectionResult.DocumentActionResults!.Select(x => x.Status).Distinct().Single();
                var documentStatus = GetWebDavStatusCode(documentActionStatus);
                if (documentStatus is null)
                {
                    return new WebDavResult(WebDavStatusCode.NoContent);
                }

                return new WebDavResult(documentStatus.Value);
            }

#pragma warning disable SA1102
            var resultsByStatus =
                (from item in collectionResult.Flatten()

                 // Entries with failed parents must be ignored
                 where item.Status != ActionStatus.ParentFailed

                 // Updated collections should be ignored
                 where item.Status != ActionStatus.Updated || item.Target is not CollectionTarget

                 let statusCode = GetWebDavStatusCode(item.Status)
                 where statusCode != null
                 select (StatusCode: statusCode.Value, Item: item))
                .GroupBy(x => x.StatusCode)
                .ToDictionary(x => x.Key, x => x.Select(y => y.Item).ToList());
#pragma warning restore SA1102

            if (resultsByStatus.Count == 0)
            {
                // No results
                return new WebDavResult(WebDavStatusCode.NoContent);
            }

            if (resultsByStatus.Count == 1)
            {
                // All remaining results have the same status
                var resultItem = resultsByStatus.Single();
                return new WebDavResult(resultItem.Key);
            }

            // Ignore "NoContent" status code
            resultsByStatus.Remove(WebDavStatusCode.NoContent);

            var result = new multistatus()
            {
                response = resultsByStatus
                    .SelectMany(x => CreateResponses(x.Key, x.Value, context))
                    .ToArray(),
            };

            if (result.response.Length == 0)
            {
                // No remaining responses -> NoContent
                return new WebDavResult(WebDavStatusCode.NoContent);
            }

            return new WebDavResult<multistatus>(WebDavStatusCode.MultiStatus, result);
        }

        private static WebDavStatusCode? GetWebDavStatusCode(ActionStatus status)
        {
            return status switch
            {
                ActionStatus.Created => WebDavStatusCode.Created,
                ActionStatus.Updated => WebDavStatusCode.NoContent,
                ActionStatus.Overwritten => WebDavStatusCode.NoContent,
                ActionStatus.OverwriteFailed => WebDavStatusCode.Conflict,
                ActionStatus.PropSetFailed => WebDavStatusCode.Conflict,
                ActionStatus.CleanupFailed => WebDavStatusCode.Conflict,
                ActionStatus.CannotOverwrite => WebDavStatusCode.PreconditionFailed,
                ActionStatus.CreateFailed => WebDavStatusCode.Forbidden,
                ActionStatus.TargetDeleteFailed => WebDavStatusCode.Forbidden,
                ActionStatus.ParentFailed => WebDavStatusCode.FailedDependency,
                ActionStatus.Ignored => null,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null),
            };
        }

        private static IEnumerable<response> CreateResponses(
            WebDavStatusCode statusCode,
            IReadOnlyCollection<ActionResult> result,
            IWebDavContext host)
        {
            var propSetFailedItems = result
                .Where(x => x.Status == ActionStatus.PropSetFailed)
                .ToList();
            if (propSetFailedItems.Count != 0)
            {
                yield return CreateResponse(statusCode, ActionStatus.PropSetFailed, propSetFailedItems, host);

                var remaining = result.Where(x => x.Status != ActionStatus.PropSetFailed).ToList();
                if (remaining.Count != 0)
                {
                    yield return CreateResponse(statusCode, remaining.First().Status, propSetFailedItems, host);
                }
            }
            else
            {
                yield return CreateResponse(statusCode, result.First().Status, result, host);
            }
        }

        private static response CreateResponse(
            WebDavStatusCode statusCode,
            ActionStatus status,
            IReadOnlyCollection<ActionResult> result,
            IWebDavContext host)
        {
            var hrefs = result.Select(x => x.Href.OriginalString).Distinct().ToList();
            var items = new List<Tuple<ItemsChoiceType2, object>>();
            var response = new response()
            {
                href = hrefs.First(),
            };

            items.AddRange(hrefs.Skip(1).Select(x => Tuple.Create(ItemsChoiceType2.href, (object)x)));

            switch (status)
            {
                case ActionStatus.PropSetFailed:
                    response.error = new error()
                    {
                        ItemsElementName = new[] { ItemsChoiceType.preservedliveproperties, },
                        Items = new[] { new object(), },
                    };
                    break;
            }

            items.Add(
                Tuple.Create(ItemsChoiceType2.status, (object)new Status(host.RequestProtocol, statusCode).ToString()));

            response.ItemsElementName = items.Select(x => x.Item1).ToArray();
            response.Items = items.Select(x => x.Item2).ToArray();

            return response;
        }
    }
}
