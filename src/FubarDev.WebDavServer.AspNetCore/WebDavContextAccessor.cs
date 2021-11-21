// <copyright file="WebDavContextAccessor.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using Microsoft.AspNetCore.Http;

namespace FubarDev.WebDavServer.AspNetCore
{
    internal class WebDavContextAccessor : GenericWebDavContextAccessor<WebDavContext>
    {
        public WebDavContextAccessor(IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
        }
    }
}
