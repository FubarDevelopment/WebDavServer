// <copyright file="WebDavStatusCode.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Net;

namespace FubarDev.WebDavServer.Model
{
    public enum WebDavStatusCode
    {
        OK = HttpStatusCode.OK,
        Created = HttpStatusCode.Created,
        NoContent = HttpStatusCode.NoContent,
        PartialContent = HttpStatusCode.PartialContent,
        MultiStatus = 207,

        BadRequest = HttpStatusCode.BadRequest,
        Forbidden = HttpStatusCode.Forbidden,
        NotFound = HttpStatusCode.NotFound,
        MethodNotAllowed = HttpStatusCode.MethodNotAllowed,
        Conflict = HttpStatusCode.Conflict,
        PreconditionFailed = HttpStatusCode.PreconditionFailed,
        RequestUriTooLong = HttpStatusCode.RequestUriTooLong,
        UnsupportedMediaType = HttpStatusCode.UnsupportedMediaType,
        RequestedRangeNotSatisfiable = HttpStatusCode.RequestedRangeNotSatisfiable,
        UnprocessableEntity = 422,
        Locked = 423,
        FailedDependency = 424,

        NotImplemented = HttpStatusCode.NotImplemented,
        BadGateway = HttpStatusCode.BadGateway,
        InsufficientStorage = 507,
    }
}
