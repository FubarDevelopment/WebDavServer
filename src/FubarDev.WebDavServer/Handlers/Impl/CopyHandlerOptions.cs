// <copyright file="CopyHandlerOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Handlers.Impl
{
    public class CopyHandlerOptions
    {
        public RecursiveProcessingMode Mode { get; set; }

        public bool OverwriteAsDefault { get; set; } = true;
    }
}
