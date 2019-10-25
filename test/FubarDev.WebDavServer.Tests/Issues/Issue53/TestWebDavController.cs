// <copyright file="TestWebDavController.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.AspNetCore;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Tests.Issues.Issue53
{
    /// <summary>
    /// The WebDAV controller.
    /// </summary>
    [Route("_dav/{*path}")]
    public class TestWebDavController : WebDavControllerBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestWebDavController"/> class.
        /// </summary>
        /// <param name="context">The WebDAV context.</param>
        /// <param name="dispatcher">The WebDAV method dispatcher.</param>
        /// <param name="responseLogger">The logger for the response messages.</param>
        public TestWebDavController(
            IWebDavContext context,
            IWebDavDispatcher dispatcher,
            ILogger<WebDavIndirectResult>? responseLogger = null)
            : base(context, dispatcher, responseLogger)
        {
        }
    }
}
