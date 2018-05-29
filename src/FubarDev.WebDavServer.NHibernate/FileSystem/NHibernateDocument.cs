// <copyright file="NHibernateDocument.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.WebDavServer.FileSystem;
using FubarDev.WebDavServer.Model;
using FubarDev.WebDavServer.NHibernate.Models;

using JetBrains.Annotations;

using NHibernate.Linq;

namespace FubarDev.WebDavServer.NHibernate.FileSystem
{
    /// <summary>
    /// A <see cref="NHibernate"/> based implementation of a WebDAV document
    /// </summary>
    internal class NHibernateDocument : NHibernateEntry, IDocument
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NHibernateDocument"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system this document belongs to</param>
        /// <param name="parent">The parent collection</param>
        /// <param name="info">The file information</param>
        /// <param name="path">The root-relative path of this document</param>
        public NHibernateDocument(
            [NotNull] NHibernateFileSystem fileSystem,
            [NotNull] ICollection parent,
            [NotNull] FileEntry info,
            [NotNull] Uri path)
            : base(fileSystem, parent, info, path, null)
        {
        }

        /// <inheritdoc />
        public long Length => Info.Length;

        /// <inheritdoc />
        public Task<Stream> OpenReadAsync(CancellationToken cancellationToken)
        {
            var data = Connection.Get<FileData>(Info.Id);
            var stream = new MemoryStream(data.Data);
            return Task.FromResult<Stream>(stream);
        }

        /// <inheritdoc />
        public Task<Stream> CreateAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<Stream>(new NHibernateBlobWriteStream(Connection, Info));
        }

        /// <inheritdoc />
        public override async Task<DeleteResult> DeleteAsync(CancellationToken cancellationToken)
        {
            using (var trans = Connection.BeginTransaction())
            {
                await Connection.CreateQuery("delete FileData fd where fd.Id=?")
                    .SetParameter(0, Info.Id).ExecuteUpdateAsync(cancellationToken)
                    .ConfigureAwait(false);
                await Connection.DeleteAsync(Info, cancellationToken)
                    .ConfigureAwait(false);
                trans.CommitAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            var propStore = FileSystem.PropertyStore;
            if (propStore != null)
            {
                await propStore.RemoveAsync(this, cancellationToken).ConfigureAwait(false);
            }

            return new DeleteResult(WebDavStatusCode.OK, null);
        }

        /// <inheritdoc />
        public async Task<IDocument> CopyToAsync(ICollection collection, string name, CancellationToken cancellationToken)
        {
            var dir = (NHibernateCollection)collection;

            var invariantName = name.ToLowerInvariant();

            FileEntry targetEntry;
            using (var trans = Connection.BeginTransaction())
            {
                targetEntry = await Connection.Query<FileEntry>()
                    .Where(x => x.InvariantName == invariantName && x.ParentId == dir.Info.Id)
                    .FirstOrDefaultAsync(cancellationToken)
                    .ConfigureAwait(false);
                if (targetEntry != null)
                {
                    targetEntry.Name = name;
                    targetEntry.LastWriteTimeUtc = Info.LastWriteTimeUtc;
                    targetEntry.CreationTimeUtc = Info.CreationTimeUtc;
                    targetEntry.Length = Info.Length;

                    var oldData = targetEntry.Data;
                    if (oldData != null)
                    {
                        oldData.Data = Info.Data?.Data ?? new byte[0];
                        await Connection.UpdateAsync(oldData, cancellationToken)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        var targetData = new FileData()
                        {
                            Id = targetEntry.Id,
                            Data = Info.Data?.Data ?? new byte[0],
                            Entry = targetEntry,
                        };

                        targetEntry.Data = targetData;

                        await Connection.SaveAsync(targetData, cancellationToken)
                            .ConfigureAwait(false);
                    }

                    await Connection.UpdateAsync(targetEntry, cancellationToken)
                        .ConfigureAwait(false);
                }
                else
                {
                    targetEntry = new FileEntry()
                    {
                        Id = Guid.NewGuid(),
                        ParentId = dir.Info.Id,
                        Name = name,
                        InvariantName = name.ToLowerInvariant(),
                        CreationTimeUtc = Info.CreationTimeUtc,
                        LastWriteTimeUtc = Info.LastWriteTimeUtc,
                        ETag = Info.ETag,
                        Length = Info.Length,
                    };

                    var targetData = new FileData()
                    {
                        Id = targetEntry.Id,
                        Data = Info.Data?.Data ?? new byte[0],
                        Entry = targetEntry,
                    };

                    targetEntry.Data = targetData;

                    await Connection.SaveAsync(targetEntry, cancellationToken)
                        .ConfigureAwait(false);
                    await Connection.SaveAsync(targetData, cancellationToken)
                        .ConfigureAwait(false);
                }

                await trans.CommitAsync(cancellationToken).ConfigureAwait(false);
            }

            var doc = new NHibernateDocument(dir.NHibernateFileSystem, dir, targetEntry, dir.Path.Append(name, false));

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

        /// <inheritdoc />
        public async Task<IDocument> MoveToAsync(ICollection collection, string name, CancellationToken cancellationToken)
        {
            var newDoc = await CopyToAsync(collection, name, cancellationToken).ConfigureAwait(false);
            await DeleteAsync(cancellationToken).ConfigureAwait(false);
            return newDoc;
        }
    }
}
