// <copyright file="SimpleAuthWebDavController.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.AspNetCore;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FubarDev.WebDavServer.Tests.Support.Controllers
{
    [Route("{*path}")]
    [Authorize]
    public class SimpleAuthWebDavController : WebDavControllerBase
    {
        public SimpleAuthWebDavController(
            IWebDavContext context,
            IWebDavDispatcher dispatcher,
            ILoggerFactory? loggerFactory = null)
            : base(context, dispatcher, loggerFactory)
        {
        }
    }
}
