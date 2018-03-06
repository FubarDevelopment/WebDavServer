using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

using FubarDev.WebDavServer.AspNetCore;

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Npam.Interop;

namespace FubarDev.WebDavServer.Sample.AspNetCore.Authentication
{
    public class AnonymousAuthHandler : AuthenticationHandler<AnonymousAuthOptions>
    {
        public AnonymousAuthHandler(IOptionsMonitor<AnonymousAuthOptions> options, ILoggerFactory logger, UrlEncoder encoder, Microsoft.AspNetCore.Authentication.ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var result = HandleFailedAuthenticationAsync(true, Scheme.Name);
            return Task.FromResult(result);
        }

        private AuthenticateResult HandleFailedAuthenticationAsync(bool? allowAnonymousAccess, string authenticationScheme)
        {
            string authorizationHeader = Request.Headers["Authorization"];
            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                return AuthenticateResult.NoResult();
            }
            
            var hostOptions = Context.RequestServices.GetRequiredService<IOptions<WebDavHostOptions>>();
            var allowAnonAccess = allowAnonymousAccess ?? hostOptions.Value.AllowAnonymousAccess;
            if (!allowAnonAccess)
                return AuthenticateResult.NoResult();

            var groups = Enumerable.Empty<Group>();
            var accountInfo = new AccountInfo()
            {
                Username = "anonymous",
                HomeDir = hostOptions.Value.AnonymousHomePath,
            };

            var info = CreateAuthenticationTicketInfo(accountInfo, groups, "anonymous");
            return AuthenticateResult.Success(new AuthenticationTicket(info.principal, info.properties, authenticationScheme));
        }

        private static (ClaimsPrincipal principal, AuthenticationProperties properties) CreateAuthenticationTicketInfo(AccountInfo accountInfo, IEnumerable<Group> groups, string authenticationType)
        {
            var claims = new List<Claim>();
            if (!string.IsNullOrEmpty(accountInfo.HomeDir))
                claims.Add(new Claim(Utils.SystemInfo.UserHomePathClaim, accountInfo.HomeDir));
            claims.Add(new Claim(ClaimsIdentity.DefaultNameClaimType, accountInfo.Username));
            claims.AddRange(groups.Select(x => new Claim(ClaimsIdentity.DefaultRoleClaimType, x.GroupName)));

            var identity = new ClaimsIdentity(claims, authenticationType);
            var principal = new ClaimsPrincipal(identity);
            return (principal, new AuthenticationProperties());
        }
    }
}
