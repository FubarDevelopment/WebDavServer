// <copyright file="IEntityTagEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using FubarDev.WebDavServer.Model.Headers;

namespace FubarDev.WebDavServer.FileSystem
{
    /// <summary>
    /// Is implemented when a <see cref="IDocument"/> or <see cref="ICollection"/> allows native <see cref="EntityTag"/> support.
    /// </summary>
    public interface IEntityTagEntry
    {
        /// <summary>
        /// Gets the <see cref="EntityTag"/> for a <see cref="IDocument"/> or <see cref="ICollection"/>
        /// </summary>
        EntityTag ETag { get; }

        /// <summary>
        /// Enforces the update of an <see cref="EntityTag"/>
        /// </summary>
        /// <remarks>
        /// This is usually called when the <see cref="IEntry"/> properties were changed.
        /// </remarks>
        /// <returns>The new <see cref="EntityTag"/></returns>
        EntityTag UpdateETag();
    }
}
