// <copyright file="NHibernateBlobWriteStream.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;

using FubarDev.WebDavServer.NHibernate.Models;

using NHibernate;

namespace FubarDev.WebDavServer.NHibernate.FileSystem
{
    internal class NHibernateBlobWriteStream : Stream
    {
        private readonly ISession _connection;
        private readonly FileEntry _entry;
        private readonly MemoryStream _baseStream = new MemoryStream();

        public NHibernateBlobWriteStream(ISession connection, FileEntry entry)
        {
            _connection = connection;
            _entry = entry;
        }

        /// <inheritdoc />
        public override bool CanRead { get; } = false;

        /// <inheritdoc />
        public override bool CanSeek { get; } = true;

        /// <inheritdoc />
        public override bool CanWrite { get; } = true;

        /// <inheritdoc />
        public override long Length => _baseStream.Length;

        /// <inheritdoc />
        public override long Position
        {
            get => _baseStream.Position;
            set => _baseStream.Position = value;
        }

        /// <inheritdoc />
        public override void Flush()
        {
            SaveData();
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            return _baseStream.Seek(offset, origin);
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            _baseStream.SetLength(value);
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            _baseStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                SaveData();
            }

            base.Dispose(disposing);
        }

        private void SaveData()
        {
            using (var trans = _connection.BeginTransaction())
            {
                var oldLength = _entry.Length;
                var oldModificationTime = _entry.LastWriteTimeUtc;
                var oldETag = _entry.ETag;
                try
                {
                    if (_entry.Data == null)
                    {
                        var data = new FileData()
                        {
                            Id = _entry.Id,
                            Data = _baseStream.ToArray(),
                            Entry = _entry,
                        };

                        _entry.Data = data;

                        _connection.Save(data);
                        _connection.Update(_entry);
                    }
                    else
                    {
                        _entry.Data.Data = _baseStream.ToArray();
                        _connection.Update(_entry.Data);
                    }

                    _entry.Length = _baseStream.Length;
                    _entry.LastWriteTimeUtc = DateTime.UtcNow;

                    _connection.Update(_entry);

                    trans.Commit();
                }
                catch (Exception)
                {
                    _entry.Length = oldLength;
                    _entry.LastWriteTimeUtc = oldModificationTime;
                    _entry.ETag = oldETag;
                    throw;
                }
            }
        }
    }
}
