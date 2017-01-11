using System;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer
{
    public class WebDavException : Exception
    {
        public WebDavException(WebDavStatusCodes statusCode)
            : base(GetMessageForStatusCode(statusCode))
        {
            StatusCode = statusCode;
        }

        public WebDavStatusCodes StatusCode { get; }

        private static string GetMessageForStatusCode(WebDavStatusCodes statusCode)
        {
            switch (statusCode)
            {
                case WebDavStatusCodes.NotFound:
                    return "Not Found";
            }

            throw new NotImplementedException();
        }
    }
}