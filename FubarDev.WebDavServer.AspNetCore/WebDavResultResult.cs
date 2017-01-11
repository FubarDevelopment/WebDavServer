using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace FubarDev.WebDavServer.AspNetCore
{
    public class WebDavResultResult<T> : StatusCodeResult
    {
        private readonly WebDavResult<T> _result;

        public WebDavResultResult(WebDavResult<T> result) 
            : base((int)result.StatusCode)
        {
            _result = result;
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            await base.ExecuteResultAsync(context);
            await _result.WriteResponseAsync(context.HttpContext.Response.Body, CancellationToken.None);
        }
    }
}
