// <copyright file="WinRootCompatController.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Mvc;

namespace FubarDev.WebDavServer.Tests.Issues.Issue0
{
    /// <summary>
    /// Restores compatibility with the Windows 7 WebDAV client.
    /// </summary>
    [Route("")]
    public class WinRootCompatController : ControllerBase
    {
        /// <summary>
        /// Just returns status code 200 to make the root directory accessible.
        /// </summary>
        /// <returns>Status code 200.</returns>
        [HttpOptions]
        public IActionResult QueryOptions()
        {
            return Ok();
        }
    }
}
