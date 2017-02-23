// <copyright file="CollectionActionResultExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;

using FubarDev.WebDavServer.Engines;
using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// Extension methods for the <see cref="CollectionActionResult"/>
    /// </summary>
    public static class CollectionActionResultExtensions
    {
        /// <summary>
        /// Evaluate the result of a <see cref="CollectionActionResult"/> and return a <see cref="IWebDavResult"/> implementation object.
        /// </summary>
        /// <param name="collectionResult">The <see cref="CollectionActionResult"/> to evaluate</param>
        /// <param name="context">The <see cref="IWebDavContext"/> to create the response for</param>
        /// <returns>The created response</returns>
        public static IWebDavResult Evaluate([NotNull] this CollectionActionResult collectionResult, [NotNull] IWebDavContext context)
        {
            if (collectionResult.Status == ActionStatus.Ignored)
            {
                return new WebDavResult(GetWebDavStatusCode(collectionResult.DocumentActionResults.Select(x => x.Status).Single()));
            }

            var resultsByStatus = collectionResult
                .Flatten()
                .Where(x => x.Status != ActionStatus.ParentFailed)
                .GroupBy(x => x.Status)
                .ToDictionary(x => x.Key, x => x.ToList());
            if (resultsByStatus.Count == 1)
            {
                var resultItem = resultsByStatus.Single();
                var statusCode = GetWebDavStatusCode(resultItem.Key);
                if (resultItem.Value.Count == 1)
                {
                    var response = CreateResponse(resultItem.Key, resultItem.Value, context);
                    if (response.error != null)
                    {
                        return new WebDavResult<error>(statusCode, response.error);
                    }
                }

                return new WebDavResult(statusCode);
            }

            var result = new multistatus()
            {
                response = resultsByStatus.Select(x => CreateResponse(x.Key, x.Value, context)).ToArray(),
            };

            return new WebDavResult<multistatus>(WebDavStatusCode.MultiStatus, result);
        }

        private static WebDavStatusCode GetWebDavStatusCode(ActionStatus status)
        {
            switch (status)
            {
                case ActionStatus.Created:
                    return WebDavStatusCode.Created;
                case ActionStatus.Overwritten:
                    return WebDavStatusCode.NoContent;
                case ActionStatus.OverwriteFailed:
                case ActionStatus.PropSetFailed:
                case ActionStatus.CleanupFailed:
                    return WebDavStatusCode.Conflict;
                case ActionStatus.CannotOverwrite:
                    return WebDavStatusCode.PreconditionFailed;
                case ActionStatus.CreateFailed:
                case ActionStatus.TargetDeleteFailed:
                    return WebDavStatusCode.Forbidden;
                case ActionStatus.ParentFailed:
                    return WebDavStatusCode.FailedDependency;
            }

            throw new NotSupportedException();
        }

        [NotNull]
        private static response CreateResponse(ActionStatus status, [NotNull] [ItemNotNull] IEnumerable<ActionResult> result, [NotNull] IWebDavContext host)
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

            var statusCode = GetWebDavStatusCode(status);
            items.Add(Tuple.Create(ItemsChoiceType2.status, (object)new Status(host.RequestProtocol, statusCode).ToString()));

            response.ItemsElementName = items.Select(x => x.Item1).ToArray();
            response.Items = items.Select(x => x.Item2).ToArray();

            return response;
        }
    }
}
