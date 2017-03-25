// <copyright file="DefaultMountPointManager.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;

namespace FubarDev.WebDavServer.FileSystem.Mount
{
    /// <summary>
    /// The default <see cref="IMountPointManager"/> class
    /// </summary>
    public class DefaultMountPointManager : IMountPointManager
    {
        private readonly IDictionary<Uri, IFileSystem> _mountPoints = new Dictionary<Uri, IFileSystem>();

        /// <inheritdoc />
        public int Count => _mountPoints.Count;

        /// <inheritdoc />
        public IEnumerator<Uri> GetEnumerator()
        {
            return _mountPoints.Keys.GetEnumerator();
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public bool TryGetMountPoint(Uri path, out IFileSystem destination)
        {
            return _mountPoints.TryGetValue(path, out destination);
        }

        /// <inheritdoc />
        public void Mount(Uri source, IFileSystem destination)
        {
            _mountPoints.Add(source, destination);
        }

        /// <inheritdoc />
        public void Unmount(Uri source)
        {
            if (!_mountPoints.Remove(source))
                throw new InvalidOperationException();
        }
    }
}
