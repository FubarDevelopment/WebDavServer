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
        /// The <code>OK</code> (200) status code
        /// </summary>
        OK = HttpStatusCode.OK,

        /// <summary>
        /// The <code>Created</code> (201) status code
        /// </summary>
        Created = HttpStatusCode.Created,

        /// <summary>
        /// The <code>No Content</code> (204) status code
        /// </summary>
        NoContent = HttpStatusCode.NoContent,

        /// <summary>
        /// The <code>Partial Content</code> (206) status code
        /// </summary>
        PartialContent = HttpStatusCode.PartialContent,

        /// <summary>
        /// The <code>Multi-Status</code> (207) status code
        /// </summary>
        MultiStatus = 207,

        /// <summary>
        /// The <code>Not Modified</code> (304) status code
        /// </summary>
        NotModified = HttpStatusCode.NotModified,

        /// <summary>
        /// The <code>Bad Request</code> (400) status code
        /// </summary>
        BadRequest = HttpStatusCode.BadRequest,

        /// <summary>
        /// The <code>Forbidden</code> (403) status code
        /// </summary>
        Forbidden = HttpStatusCode.Forbidden,

        /// <summary>
        /// The <code>Not Found</code> (404) status code
        /// </summary>
        NotFound = HttpStatusCode.NotFound,

        /// <summary>
        /// The <code>Method Not Allowed</code> (405) status code
        /// </summary>
        MethodNotAllowed = HttpStatusCode.MethodNotAllowed,

        /// <summary>
        /// The <code>Conflict</code> (409) status code
        /// </summary>
        Conflict = HttpStatusCode.Conflict,

        /// <summary>
        /// The <code>Precondition Failed</code> (412) status code
        /// </summary>
        PreconditionFailed = HttpStatusCode.PreconditionFailed,

        /// <summary>
        /// The <code>Request URI Too Long</code> (414) status code
        /// </summary>
        RequestUriTooLong = HttpStatusCode.RequestUriTooLong,

        /// <summary>
        /// The <code>Unsupported Media Type</code> (415) status code
        /// </summary>
        UnsupportedMediaType = HttpStatusCode.UnsupportedMediaType,

        /// <summary>
        /// The <code>Requested Range Not Satisfiable</code> (416) status code
        /// </summary>
        RequestedRangeNotSatisfiable = HttpStatusCode.RequestedRangeNotSatisfiable,

        /// <summary>
        /// The <code>Unprocessable Entity</code> (422) status code
        /// </summary>
        UnprocessableEntity = 422,

        /// <summary>
        /// The <code>Locked</code> (423) status code
        /// </summary>
        Locked = 423,

        /// <summary>
        /// The <code>Failed Dependency</code> (424) status code
        /// </summary>
        FailedDependency = 424,

        /// <summary>
        /// The <code>Not Implemented</code> (501) status code
        /// </summary>
        NotImplemented = HttpStatusCode.NotImplemented,

        /// <summary>
        /// The <code>Bad Gateway</code> (502) status code
        /// </summary>
        BadGateway = HttpStatusCode.BadGateway,

        /// <summary>
        /// The <code>Insufficient Storage</code> (507) status code
        /// </summary>
        InsufficientStorage = 507,
    }
}
