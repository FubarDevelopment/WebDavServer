// <copyright file="MoveHandlerOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.DefaultHandlers
{
    public class MoveHandlerOptions
    {
        public RecursiveProcessingMode Mode { get; set; }

        public bool OverwriteAsDefault { get; set; } = true;
    }
}
