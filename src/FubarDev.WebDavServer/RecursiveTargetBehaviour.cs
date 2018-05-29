// <copyright file="RecursiveTargetBehaviour.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

namespace FubarDev.WebDavServer
{
    /// <summary>
    /// Define the behaviour, when a target already exists
    /// </summary>
    public enum RecursiveTargetBehaviour
    {
        /// <summary>
        /// Overwrite the existing target entry
        /// </summary>
        Overwrite,

        /// <summary>
        /// Delete the existing target entry before copying the contents from the source
        /// </summary>
        DeleteTarget,
    }
}
