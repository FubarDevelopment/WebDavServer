// <copyright file="TextFilePropertyStoreOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Props.Store.TextFile
{
    public class TextFilePropertyStoreOptions
    {
        public int EstimatedCost { get; set; } = 10;

        public string RootFolder { get; set; }

        public bool StoreInTargetFileSystem { get; set; }
    }
}
