// <copyright file="IfModifiedSinceHeader.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Props.Converters;

namespace FubarDev.WebDavServer.Model.Headers
{
    public class IfModifiedSinceHeader
    {
        public IfModifiedSinceHeader(DateTime lastWriteTimeUtc)
        {
            LastWriteTimeUtc = lastWriteTimeUtc;
        }

        public DateTime LastWriteTimeUtc { get; }

        public static IfModifiedSinceHeader Parse(string s)
        {
            return new IfModifiedSinceHeader(DateTimeRfc1123Converter.Parse(s));
        }

        public bool IsMatch(DateTime lastWriteTimeUtc)
        {
            return lastWriteTimeUtc > LastWriteTimeUtc;
        }

        public Task<bool> IsMatchAsync(IEntry entry, CancellationToken cancellationToken)
        {
            return Task.FromResult(IsMatch(entry.LastWriteTimeUtc));
        }
    }
}
