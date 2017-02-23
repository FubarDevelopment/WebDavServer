// <copyright file="DotNetFile.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.Props.Dead;
using FubarDev.WebDavServer.Props.Live;

namespace FubarDev.WebDavServer.FileSystem.DotNet
{
    public class DotNetFile : DotNetEntry, IDocument
    {
        public DotNetFile(DotNetFileSystem fileSystem, DotNetDirectory parent, FileInfo info, Uri path)
            : base(fileSystem, parent, info, path)
        {
            FileInfo = info;
        }

        public FileInfo FileInfo { get; }

        public long Length => FileInfo.Length;

        public Task<Stream> OpenReadAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<Stream>(FileInfo.OpenRead());
        }

        public async Task<Stream> CreateAsync(CancellationToken cancellationToken)
        {
            if (FileSystem.PropertyStore != null)
                await FileSystem.PropertyStore.UpdateETagAsync(this, cancellationToken).ConfigureAwait(false);
            return FileInfo.Open(FileMode.Create, FileAccess.Write);
        }

        public override async Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken)
        {
            FileInfo.Delete();

            var propStore = FileSystem.PropertyStore;
            if (propStore != null)
            {
                await propStore.RemoveAsync(this, cancellationToken).ConfigureAwait(false);
            }

            return new DeleteResult(WebDavStatusCode.OK, null);
        }

        public async Task<IDocument> CopyToAsync(ICollection collection, string name, CancellationToken cancellationToken)
        {
            var dir = (DotNetDirectory)collection;
            var targetFileName = System.IO.Path.Combine(dir.DirectoryInfo.FullName, name);
            File.Copy(FileInfo.FullName, targetFileName, true);
            var fileInfo = new FileInfo(targetFileName);
            var doc = new DotNetFile(dir.DotNetFileSystem, dir, fileInfo, dir.Path.Append(fileInfo.Name, false));

            var sourcePropStore = FileSystem.PropertyStore;
            var destPropStore = collection.FileSystem.PropertyStore;
            if (sourcePropStore != null && destPropStore != null)
            {
                var sourceProps = await sourcePropStore.GetAsync(this, cancellationToken).ConfigureAwait(false);
                await destPropStore.RemoveAsync(doc, cancellationToken).ConfigureAwait(false);
                await destPropStore.SetAsync(doc, sourceProps, cancellationToken).ConfigureAwait(false);
            }
            else if (destPropStore != null)
            {
                await destPropStore.RemoveAsync(doc, cancellationToken).ConfigureAwait(false);
            }

            return doc;
        }

        public async Task<IDocument> MoveToAsync(ICollection collection, string name, CancellationToken cancellationToken)
        {
            var sourcePropStore = FileSystem.PropertyStore;
            var destPropStore = collection.FileSystem.PropertyStore;

            IReadOnlyCollection<XElement> sourceProps;
            if (sourcePropStore != null && destPropStore != null)
            {
                sourceProps = await sourcePropStore.GetAsync(this, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                sourceProps = null;
            }

            var dir = (DotNetDirectory)collection;
            var targetFileName = System.IO.Path.Combine(dir.DirectoryInfo.FullName, name);
            if (File.Exists(targetFileName))
                File.Delete(targetFileName);
            File.Move(FileInfo.FullName, targetFileName);
            var fileInfo = new FileInfo(targetFileName);
            var doc = new DotNetFile(dir.DotNetFileSystem, dir, fileInfo, dir.Path.Append(fileInfo.Name, false));

            if (destPropStore != null)
            {
                await destPropStore.RemoveAsync(doc, cancellationToken).ConfigureAwait(false);

                if (sourceProps != null)
                {
                    await destPropStore.SetAsync(doc, sourceProps, cancellationToken).ConfigureAwait(false);
                }
            }

            return doc;
        }

        protected override IEnumerable<ILiveProperty> GetLiveProperties()
        {
            return base.GetLiveProperties()
                .Concat(new ILiveProperty[]
                {
                    new ContentLengthProperty(Length),
                });
        }

        protected override IEnumerable<IDeadProperty> GetPredefinedDeadProperties()
        {
            return base.GetPredefinedDeadProperties()
                .Concat(new[]
                {
                    DotNetFileSystem.DeadPropertyFactory
                        .Create(FileSystem.PropertyStore, this, GetContentLanguageProperty.PropertyName),
                    DotNetFileSystem.DeadPropertyFactory
                        .Create(FileSystem.PropertyStore, this, GetContentTypeProperty.PropertyName),
                });
        }
    }
}
