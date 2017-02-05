// <copyright file="WebDavException.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

using FubarDev.WebDavServer.Model;

namespace FubarDev.WebDavServer
{
    public class WebDavException : Exception
    {
        public WebDavException(WebDavStatusCode statusCode)
            : base(statusCode.GetReasonPhrase())
        {
            StatusCode = statusCode;
        }

        public WebDavException(WebDavStatusCode statusCode, Exception innerException)
            : base(statusCode.GetReasonPhrase(innerException.Message))
        {
            StatusCode = statusCode;
        }

        public WebDavException(WebDavStatusCode statusCode, string responseMessage)
            : base(statusCode.GetReasonPhrase(responseMessage))
        {
            StatusCode = statusCode;
        }

        public WebDavStatusCode StatusCode { get; }
    }
}
