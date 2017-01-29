using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Handlers;
using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer.DefaultHandlers
{
    public class OptionsHandler : IOptionsHandler
    {
        public IEnumerable<string> HttpMethods { get; } = new[] { "OPTIONS" };

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
            }
        }
    }
}
