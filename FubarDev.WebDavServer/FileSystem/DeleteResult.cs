// <copyright file="DeleteResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.Model;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.FileSystem
{
    public class DeleteResult
    {
        public DeleteResult(WebDavStatusCode statusCode, [CanBeNull] IEntry failedEntry)
        {
            FailedEntry = failedEntry;
            StatusCode = statusCode;
        }

        public WebDavStatusCode StatusCode { get; }

        [CanBeNull]
        public IEntry FailedEntry { get; }
    }
}
