// <copyright file="PropertyChange.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer.Tests.Support
{
    /// <summary>
    /// The kind of change that happened to a property.
    /// </summary>
    public enum PropertyChange
    {
        /// <summary>
        /// Property was added.
        /// </summary>
        Added,

        /// <summary>
        /// Property was removed.
        /// </summary>
        Removed,

        /// <summary>
        /// Property was changed.
        /// </summary>
        Changed,
    }
}
