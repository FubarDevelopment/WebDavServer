using System.Security.Principal;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;

namespace FubarDev.WebDavServer.Sample.AspNetCore.Middlewares
{
    public class ImpersonationMiddleware
    {
        private readonly RequestDelegate _next;

        public ImpersonationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        // ReSharper disable once UnusedMember.Local
        public async Task Invoke(HttpContext context)
        {
            if (!(context.User.Identity is WindowsIdentity identity) || !identity.IsAuthenticated)
            {
                await _next(context);
            }
            else
            {
                await WindowsIdentity.RunImpersonated(
                    identity.AccessToken,
                    async () => { await _next(context); });
            }
        }
    }
}
