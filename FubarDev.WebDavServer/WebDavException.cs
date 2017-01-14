using System;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer
{
    public class WebDavException : Exception
    {
        public WebDavException(WebDavStatusCodes statusCode)
            : base(statusCode.GetReasonPhrase())
        {
            StatusCode = statusCode;
        }

        public WebDavException(WebDavStatusCodes statusCode, Exception innerException)
            : base(statusCode.GetReasonPhrase(innerException.Message))
        {
            StatusCode = statusCode;
        }

        public WebDavStatusCodes StatusCode { get; }
    }
}
