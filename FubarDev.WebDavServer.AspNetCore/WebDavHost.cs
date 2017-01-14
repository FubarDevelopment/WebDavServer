using System;
using System.Text;

using Microsoft.AspNetCore.Http;

namespace FubarDev.WebDavServer.AspNetCore
{
    public class WebDavHost : IWebDavHost
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        private readonly Lazy<Uri> _baseUrl;

        public WebDavHost(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _baseUrl = new Lazy<Uri>(() => BuildBaseUrl(httpContextAccessor.HttpContext));
        }

        public Uri BaseUrl => _baseUrl.Value;

        public string RequestProtocol => _httpContextAccessor.HttpContext.Request.Protocol;

        private static Uri BuildBaseUrl(HttpContext httpContext)
        {
            var request = httpContext.Request;
            var pathBase = request.PathBase.ToString();
            var result = new StringBuilder()
                .Append(request.Scheme).Append("://").Append(request.Host).Append(pathBase);
            if (!pathBase.EndsWith("/", StringComparison.Ordinal))
                result.Append("/");
            return new Uri(result.ToString());
        }
    }
}
