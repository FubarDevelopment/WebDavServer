// <copyright file="ICollectionNode.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System.Collections.Generic;

namespace FubarDev.WebDavServer.FileSystem
{
    /// <summary>
    /// A node representing all found children of a <see cref="ICollection"/>
    /// </summary>
    public interface ICollectionNode
    {
        /// <summary>
        /// Gets the collection this node is for
        /// </summary>
        ICollection Collection { get; }

        /// <summary>
        /// Gets the name of the node (usually the same as <see cref="IEntry.Name"/> of <see cref="Collection"/>)
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the sub collection nodes
        /// </summary>
        IReadOnlyCollection<ICollectionNode> Nodes { get; }

        /// <summary>
        /// Gets the documents of this collection node
        /// </summary>
        IReadOnlyCollection<IDocument> Documents { get; }
    }
}
