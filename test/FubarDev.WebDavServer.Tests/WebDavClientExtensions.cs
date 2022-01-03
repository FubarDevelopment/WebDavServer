// <copyright file="WebDavClientExtensions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Net.Http.Headers;
using System.Text;

using DecaTec.WebDav;

using idunno.Authentication.Basic;

namespace FubarDev.WebDavServer.Tests
{
    /// <summary>
    /// Extension methods for the <see cref="WebDavClient"/>.
    /// </summary>
    public static class WebDavClientExtensions
    {
        /// <summary>
        /// Set the authentication header for the WebDAV client.
        /// </summary>
        /// <param name="client">The client to set the authentication header for.</param>
        /// <param name="username">The user name.</param>
        /// <param name="password">The password.</param>
        /// <returns>The updated WebDAV client.</returns>
        public static WebDavClient UseAuthentication(
            this WebDavClient client,
            string username,
            string password)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                BasicAuthenticationDefaults.AuthenticationScheme,
                Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));
            return client;
        }
    }
}
