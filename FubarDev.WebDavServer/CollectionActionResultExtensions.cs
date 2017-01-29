using System;
using System.Collections.Generic;
using System.Linq;

using FubarDev.WebDavServer.Engines;
using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer
{
    public static class CollectionActionResultExtensions
    {
        public static IWebDavResult Evaluate(this CollectionActionResult collectionResult, IWebDavHost host)
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
                    var response = CreateResponse(resultItem.Key, resultItem.Value, host);
                    if (response.Error != null)
                    {
                        return new WebDavResult<Error>(statusCode, response.Error);
                    }
                }

                return new WebDavResult(statusCode);
            }

            var result = new Multistatus()
            {
                Response = resultsByStatus.Select(x => CreateResponse(x.Key, x.Value, host)).ToArray()
            };

            return new WebDavResult<Multistatus>(WebDavStatusCode.MultiStatus, result);
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

        private static Response CreateResponse(ActionStatus status, IEnumerable<ActionResult> result, IWebDavHost host)
        {
            var hrefs = result.Select(x => x.Href.OriginalString).Distinct().ToList();
            var items = new List<Tuple<ItemsChoiceType2, object>>();
            var response = new Response()
            {
                Href = hrefs.First(),
            };

            items.AddRange(hrefs.Skip(1).Select(x => Tuple.Create(ItemsChoiceType2.Href, (object)x)));

            switch (status)
            {
                case ActionStatus.PropSetFailed:
                    response.Error = new Error()
                    {
                        ItemsElementName = new[] { ItemsChoiceType.PreservedLiveProperties, },
                        Items = new[] { new object(), }
                    };
                    break;
            }

            var statusCode = GetWebDavStatusCode(status);
            items.Add(Tuple.Create(ItemsChoiceType2.Status, (object)new Status(host.RequestProtocol, statusCode).ToString()));

            response.ItemsElementName = items.Select(x => x.Item1).ToArray();
            response.Items = items.Select(x => x.Item2).ToArray();

            return response;
        }
    }
}
