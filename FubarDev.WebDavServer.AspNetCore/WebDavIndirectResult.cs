using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace FubarDev.WebDavServer.AspNetCore
{
    public class WebDavIndirectResult : StatusCodeResult
    {
        private readonly IWebDavResult _result;

        public WebDavIndirectResult(IWebDavResult result)
            : base((int)result.StatusCode)
        {
            _result = result;
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            await base.ExecuteResultAsync(context).ConfigureAwait(false);
            await _result.WriteResponseAsync(context.HttpContext.Response.Body, CancellationToken.None).ConfigureAwait(false);
        }
    }
}
