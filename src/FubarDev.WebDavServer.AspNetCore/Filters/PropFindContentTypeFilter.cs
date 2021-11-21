// <copyright file="PropFindContentTypeFilter.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Utils;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace FubarDev.WebDavServer.AspNetCore.Filters
{
    /// <summary>
    /// A special filter that turns HTTP 415 into HTTP 400 for PROPFIND.
    /// </summary>
    public class PropFindContentTypeFilter : IAsyncAlwaysRunResultFilter
    {
        private readonly bool _enabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="PropFindContentTypeFilter"/> class.
        /// </summary>
        /// <param name="litmusCompatOptions">Compatibility options for the litmus tool.</param>
        public PropFindContentTypeFilter(
            IOptions<LitmusCompatibilityOptions> litmusCompatOptions)
        {
            _enabled = !litmusCompatOptions.Value.PropFindContentTypeErrorIs415;
        }

        /// <inheritdoc />
        public Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (_enabled)
            {
                UpdateExecutionResult(context);
            }

            return next();
        }

        private void UpdateExecutionResult(ResultExecutingContext context)
        {
            if (context.Controller is not WebDavControllerBase)
            {
                return;
            }

            if (!string.Equals(
                    context.HttpContext.Request.Method,
                    "PROPFIND",
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (context.Result is UnsupportedMediaTypeResult)
            {
                context.Result = new BadRequestResult();
            }
        }
    }
}
