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
    public abstract class DotNetEntry : IEntry
    {
        private readonly DotNetDirectory _parent;

        protected DotNetEntry(DotNetFileSystem fileSystem, DotNetDirectory parent, FileSystemInfo info, Uri path)
        {
            _parent = parent;
            Info = info;
            DotNetFileSystem = fileSystem;
            Path = path;
        }

        public FileSystemInfo Info { get; }

        public DotNetFileSystem DotNetFileSystem { get; }

        public string Name => Info.Name;

        public IFileSystem RootFileSystem => DotNetFileSystem;

        public IFileSystem FileSystem => DotNetFileSystem;

        public ICollection Parent => _parent;

        public Uri Path { get; }

        public DateTime LastWriteTimeUtc => Info.LastWriteTimeUtc;

        public DateTime CreationTimeUtc => Info.CreationTimeUtc;

        public IAsyncEnumerable<IUntypedReadableProperty> GetProperties(int? maxCost)
        {
            return new EntryProperties(this, GetLiveProperties(), GetPredefinedDeadProperties(), FileSystem.PropertyStore, maxCost);
        }

        public abstract Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken);

        protected virtual IEnumerable<ILiveProperty> GetLiveProperties()
        {
            var properties = new List<ILiveProperty>()
            {
                this.GetResourceTypeProperty(),
                new LockDiscoveryProperty(FileSystem.LockManager, this),
                new SupportedLockProperty(this),
                new LastModifiedProperty(ct => Task.FromResult(LastWriteTimeUtc), SetLastWriteTimeUtcAsync),
                new CreationDateProperty(ct => Task.FromResult(CreationTimeUtc), SetCreateTimeUtcAsync),
            };
            return properties;
        }

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
