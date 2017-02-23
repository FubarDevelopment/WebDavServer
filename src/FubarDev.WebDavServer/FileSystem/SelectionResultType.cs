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
        MissingDocumentOrCollection,

        /// <summary>
        /// A collection was missing
        /// </summary>
        MissingCollection
    }
}
