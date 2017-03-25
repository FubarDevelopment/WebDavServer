// <copyright file="InMemoryFileSystemOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;

namespace FubarDev.WebDavServer.FileSystem.InMemory
{
    /// <summary>
    /// The options for the in-memory file system
    /// </summary>
    public class InMemoryFileSystemOptions
    {
        /// <summary>
        /// The event for the file system initialization
        /// </summary>
        public event EventHandler<InMemoryFileSystemInitializationEventArgs> Initialize;

        /// <summary>
        /// The event for the file system update
        /// </summary>
        public event EventHandler<InMemoryFileSystemInitializationEventArgs> Update;

        /// <summary>
        /// Triggers the <see cref="Initialize"/> event
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="eventArgs">The event arguments</param>
        internal void OnInitialize(object sender, InMemoryFileSystemInitializationEventArgs eventArgs)
        {
            Initialize?.Invoke(sender, eventArgs);
        }

        /// <summary>
        /// Triggers the <see cref="Update"/> event
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="eventArgs">The event arguments</param>
        internal void OnUpdate(object sender, InMemoryFileSystemInitializationEventArgs eventArgs)
        {
            Update?.Invoke(sender, eventArgs);
        }
    }
}
