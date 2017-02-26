// <copyright file="DotNetEntry.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.Props;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Live;

namespace FubarDev.WebDavServer.FileSystem.DotNet
{
    /// <summary>
    /// A .NET <see cref="System.IO"/> based implementation of a WebDAV entry (collection or document)
    /// </summary>
    public abstract class DotNetEntry : IEntry
    {
        private readonly DotNetDirectory _parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="DotNetEntry"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system this entry belongs to</param>
        /// <param name="parent">The parent collection</param>
        /// <param name="info">The file system information</param>
        /// <param name="path">The root-relative path of this entry</param>
        protected DotNetEntry(DotNetFileSystem fileSystem, DotNetDirectory parent, FileSystemInfo info, Uri path)
        {
            _parent = parent;
            Info = info;
            DotNetFileSystem = fileSystem;
            Path = path;
        }

        /// <summary>
        /// Gets the file system information of this entry
        /// </summary>
        public FileSystemInfo Info { get; }

        /// <summary>
        /// Gets the file system this entry belongs to
        /// </summary>
        public DotNetFileSystem DotNetFileSystem { get; }

        /// <inheritdoc />
        public string Name => Info.Name;

        /// <inheritdoc />
        public IFileSystem RootFileSystem => DotNetFileSystem;

        /// <inheritdoc />
        public IFileSystem FileSystem => DotNetFileSystem;

        /// <inheritdoc />
        public ICollection Parent => _parent;

        /// <inheritdoc />
        public Uri Path { get; }

        /// <inheritdoc />
        public DateTime LastWriteTimeUtc => Info.LastWriteTimeUtc;

        /// <inheritdoc />
        public DateTime CreationTimeUtc => Info.CreationTimeUtc;

        /// <inheritdoc />
        public IAsyncEnumerable<IUntypedReadableProperty> GetProperties(int? maxCost)
        {
            return new EntryProperties(this, GetLiveProperties(), GetPredefinedDeadProperties(), FileSystem.PropertyStore, maxCost);
        }

        /// <inheritdoc />
        public abstract Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Gets the default live properties of this entry
        /// </summary>
        /// <returns>The enumeration of live properties belonging to this entry</returns>
        protected virtual IEnumerable<ILiveProperty> GetLiveProperties()
        {
            var properties = new List<ILiveProperty>()
            {
                this.GetResourceTypeProperty(),
                new LockDiscoveryProperty(this),
                new SupportedLockProperty(this),
                new LastModifiedProperty(LastWriteTimeUtc, SetLastWriteTimeUtcAsync),
                new CreationDateProperty(CreationTimeUtc, SetCreateTimeUtcAsync),
            };
            return properties;
        }

        /// <summary>
        /// Gets the default dead properties of this entry
        /// </summary>
        /// <returns>The enumeration of dead properties belonging to this entry</returns>
        protected virtual IEnumerable<IDeadProperty> GetPredefinedDeadProperties()
        {
            yield return DotNetFileSystem
                .DeadPropertyFactory.Create(FileSystem.PropertyStore, this, DisplayNameProperty.PropertyName);
            yield return new GetETagProperty(FileSystem.PropertyStore, this);
        }

        private Task SetCreateTimeUtcAsync(DateTime value, CancellationToken cancellationToken)
        {
            Info.CreationTimeUtc = value;
            return Task.FromResult(0);
        }

        private Task SetLastWriteTimeUtcAsync(DateTime timestamp, CancellationToken ct)
        {
            Info.LastWriteTimeUtc = timestamp;
            return Task.FromResult(0);
        }
    }
}
