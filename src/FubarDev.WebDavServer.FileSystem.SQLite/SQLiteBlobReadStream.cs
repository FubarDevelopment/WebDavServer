// <copyright file="SQLiteBlobReadStream.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;

using SQLitePCL;

namespace FubarDev.WebDavServer.FileSystem.SQLite
{
    internal class SQLiteBlobReadStream : Stream
    {
        private readonly sqlite3_blob _blob;
        private readonly sqlite3 _db;
        private long _position;

        public SQLiteBlobReadStream(sqlite3 db, sqlite3_blob blob)
        {
            _db = db;
            Length = raw.sqlite3_blob_bytes(blob);
            _blob = blob;
        }

        /// <inheritdoc />
        public override bool CanRead => true;

        /// <inheritdoc />
        public override bool CanSeek => true;

        /// <inheritdoc />
        public override bool CanWrite => false;

        /// <inheritdoc />
        public override long Length { get; }

        /// <inheritdoc />
        public override long Position
        {
            get { return _position; }
            set { _position = value; }
        }

        /// <inheritdoc />
        public override void Flush()
        {
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    _position = offset;
                    break;
                case SeekOrigin.Current:
                    _position += offset;
                    break;
                case SeekOrigin.End:
                    _position = Length + offset;
                    break;
            }

            return _position;
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            var remaining = (int)Math.Min(count, Length - _position);
            if (remaining <= 0)
                return 0;
            var rc = raw.sqlite3_blob_read(_blob, buffer, offset, remaining, (int)_position);
            if (rc != 0)
                throw new SQLiteFileSystemException(_db);
            _position += remaining;
            return remaining;
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                raw.sqlite3_blob_close(_blob);
            }

            base.Dispose(disposing);
        }
    }
}
