// <copyright file="DeleteResult.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.FileSystem
{
    /// <summary>
    /// The result of a <c>DELETE</c> file system operation.
    /// </summary>
    public class DeleteResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteResult"/> class.
        /// </summary>
        /// <param name="statusCode">The status code for the operation.</param>
        /// <param name="failedEntry">The entry of the failed operation.</param>
        public DeleteResult(WebDavStatusCode statusCode, IEntry? failedEntry)
        {
            FailedEntry = failedEntry;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Gets the status code of the operation.
        /// </summary>
        public WebDavStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the failed entry.
        /// </summary>
        public IEntry? FailedEntry { get; }
    }
}
