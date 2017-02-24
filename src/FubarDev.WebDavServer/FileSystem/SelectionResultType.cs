// <copyright file="SelectionResultType.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.FileSystem
{
    /// <summary>
    /// The status of a selection result
    /// </summary>
    /// <seealso cref="IFileSystem.SelectAsync"/>
    public enum SelectionResultType
    {
        /// <summary>
        /// A collection was found
        /// </summary>
        FoundCollection,

        /// <summary>
        /// A document was found
        /// </summary>
        FoundDocument,

        /// <summary>
        /// A document or collection was missing
        /// </summary>
        /// <remarks>
        /// This is different from <see cref="MissingCollection"/>, because the last missing part doesn't contain a <code>/</code>
        /// at the end and may therefore be a file or a collection.
        /// </remarks>
        MissingDocumentOrCollection,

        /// <summary>
        /// A collection was missing
        /// </summary>
        /// <remarks>
        /// This is different from <see cref="MissingDocumentOrCollection"/>, because the last missing part contains a <code>/</code>
        /// at the end and is therefore clearly a collection.
        /// </remarks>
        MissingCollection
    }
}
