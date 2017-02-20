// <copyright file="IIfHttpMatcher.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.FileSystem;

using JetBrains.Annotations;

namespace FubarDev.WebDavServer.Model.Headers
{
    public interface IIfHttpMatcher
    {
        bool IsMatch([NotNull] IEntry entry, EntityTag etag);
    }
}
