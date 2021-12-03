// <copyright file="WebDavResponseExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using DecaTec.WebDav;

using Xunit;

namespace FubarDev.WebDavServer.Tests
{
    /// <summary>
    /// Extension methods for <see cref="WebDavResponseMessage"/>.
    /// </summary>
    public static class WebDavResponseExtensions
    {
        /// <summary>
        /// Ensures that the response contains a successful status code.
        /// </summary>
        /// <param name="response">The response message to test.</param>
        /// <returns>The response message.</returns>
        public static WebDavResponseMessage EnsureSuccess(this WebDavResponseMessage response)
        {
            return (WebDavResponseMessage)response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Ensures that the response contains a successful status code.
        /// </summary>
        /// <param name="response">The response message to test.</param>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <returns>The response message.</returns>
        public static WebDavResponseMessage EnsureStatusCode(
            this WebDavResponseMessage response,
            WebDavStatusCode statusCode)
        {
            Assert.Equal(statusCode, response.StatusCode);
            return response;
        }
    }
}
