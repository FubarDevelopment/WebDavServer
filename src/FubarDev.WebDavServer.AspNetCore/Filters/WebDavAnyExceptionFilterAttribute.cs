// <copyright file="WebDavAnyExceptionFilterAttribute.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc.Filters;

namespace FubarDev.WebDavServer.AspNetCore.Filters
{
    /// <summary>
    /// Attribute to handle exception filtering for WebDAV requests.
    /// </summary>
    public class WebDavAnyExceptionFilterAttribute : WebDavExceptionFilterAttribute
    {
        /// <inheritdoc />
        public override async Task OnExceptionAsync(ExceptionContext context)
        {
            await base.OnExceptionAsync(context);

            if (context.ExceptionHandled)
            {
                return;
            }

            // Return an internal server error status for all other exceptions
            context.Result = BuildResultForStatusCode(
                context,
                WebDavStatusCode.InternalServerError);
            context.ExceptionHandled = true;
        }
    }
}
