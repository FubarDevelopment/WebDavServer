// <copyright file="WebDavStatusCode.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Net;

namespace FubarDev.WebDavServer.Model
{
    /// <summary>
    /// The status codes used by the WebDAV server
    /// </summary>
    public enum WebDavStatusCode
    {
        /// <summary>
        /// The <c>OK</c> (200) status code
        /// </summary>
        OK = HttpStatusCode.OK,

        /// <summary>
        /// The <c>Created</c> (201) status code
        /// </summary>
        Created = HttpStatusCode.Created,

        /// <summary>
        /// The <c>No Content</c> (204) status code
        /// </summary>
        NoContent = HttpStatusCode.NoContent,

        /// <summary>
        /// The <c>Partial Content</c> (206) status code
        /// </summary>
        PartialContent = HttpStatusCode.PartialContent,

        /// <summary>
        /// The <c>Multi-Status</c> (207) status code
        /// </summary>
        MultiStatus = 207,

        /// <summary>
        /// The <c>Not Modified</c> (304) status code
        /// </summary>
        NotModified = HttpStatusCode.NotModified,

        /// <summary>
        /// The <c>Bad Request</c> (400) status code
        /// </summary>
        BadRequest = HttpStatusCode.BadRequest,

        /// <summary>
        /// The <c>Forbidden</c> (403) status code
        /// </summary>
        Forbidden = HttpStatusCode.Forbidden,

        /// <summary>
        /// The <c>Not Found</c> (404) status code
        /// </summary>
        NotFound = HttpStatusCode.NotFound,

        /// <summary>
        /// The <c>Method Not Allowed</c> (405) status code
        /// </summary>
        MethodNotAllowed = HttpStatusCode.MethodNotAllowed,

        /// <summary>
        /// The <c>Conflict</c> (409) status code
        /// </summary>
        Conflict = HttpStatusCode.Conflict,

        /// <summary>
        /// The <c>Precondition Failed</c> (412) status code
        /// </summary>
        PreconditionFailed = HttpStatusCode.PreconditionFailed,

        /// <summary>
        /// The <c>Request URI Too Long</c> (414) status code
        /// </summary>
        RequestUriTooLong = HttpStatusCode.RequestUriTooLong,

        /// <summary>
        /// The <c>Unsupported Media Type</c> (415) status code
        /// </summary>
        UnsupportedMediaType = HttpStatusCode.UnsupportedMediaType,

        /// <summary>
        /// The <c>Requested Range Not Satisfiable</c> (416) status code
        /// </summary>
        RequestedRangeNotSatisfiable = HttpStatusCode.RequestedRangeNotSatisfiable,

        /// <summary>
        /// The <c>Unprocessable Entity</c> (422) status code
        /// </summary>
        UnprocessableEntity = 422,

        /// <summary>
        /// The <c>Locked</c> (423) status code
        /// </summary>
        Locked = 423,

        /// <summary>
        /// The <c>Failed Dependency</c> (424) status code
        /// </summary>
        FailedDependency = 424,

        /// <summary>
        /// The <c>Not Implemented</c> (501) status code
        /// </summary>
        NotImplemented = HttpStatusCode.NotImplemented,

        /// <summary>
        /// The <c>Bad Gateway</c> (502) status code
        /// </summary>
        BadGateway = HttpStatusCode.BadGateway,

        /// <summary>
        /// The <c>Insufficient Storage</c> (507) status code
        /// </summary>
        InsufficientStorage = 507,
    }
}
