using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Model;

using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace FubarDev.WebDavServer.AspNetCore
{
    public class WebDavIndirectResult : StatusCodeResult
    {
        private readonly IWebDavDispatcher _dispatcher;

        private readonly IWebDavResult _result;

        public WebDavIndirectResult(IWebDavDispatcher dispatcher, IWebDavResult result)
            : base((int)result.StatusCode)
        {
            _dispatcher = dispatcher;
            _result = result;
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            var response = context.HttpContext.Response;

            // Sets the HTTP status code
            await base.ExecuteResultAsync(context).ConfigureAwait(false);

            // Sets the reason phrase
            var responseFeature = context.HttpContext.Features.Get<IHttpResponseFeature>();
            if (responseFeature != null)
                responseFeature.ReasonPhrase = _result.StatusCode.GetReasonPhrase();

            // Writes the XML response
            await _result.ExecuteResultAsync(new WebDavResponse(_dispatcher, response), CancellationToken.None).ConfigureAwait(false);
        }
    }
}
