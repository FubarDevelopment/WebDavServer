// <copyright file="StreamView.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FubarDev.WebDavServer.Utils
{
    /// <summary>
    /// This is some kind of a "View" to an underlying stream
    /// </summary>
    public class StreamView : Stream
    {
        private readonly Stream _baseStream;
        private long _position;

        private StreamView(Stream baseStream, long startPosition, long length)
        {
            _baseStream = baseStream;
            Offset = startPosition;
            Length = length;
        }

        /// <inheritdoc />
        public override bool CanRead { get; } = true;

        /// <inheritdoc />
        public override bool CanSeek => _baseStream.CanSeek;

        /// <inheritdoc />
        public override bool CanWrite { get; } = false;

        /// <inheritdoc />
        public override long Length { get; }

        /// <inheritdoc />
        public override long Position
        {
            get
            {
                return _position;
            }

            set
            {
                if (_position == value)
                    return;
                _baseStream.Seek(value - _position, SeekOrigin.Current);
                _position = value;
            }
        }

        private long Offset { get; }

        /// <summary>
        /// Creates a new stream view
        /// </summary>
        /// <remarks>
        /// The <paramref name="baseStream"/> must be at position 0.
        /// </remarks>
        /// <param name="baseStream">The underlying stream</param>
        /// <param name="position">The start position</param>
        /// <param name="length">The length of the data to be read from the underlying stream</param>
        /// <param name="ct">The cancellation token</param>
        /// <returns>The new stream view</returns>
        public static async Task<StreamView> CreateAsync(
            Stream baseStream,
            long position,
            long length,
            CancellationToken ct)
        {
            if (baseStream.CanSeek)
            {
                baseStream.Seek(position, SeekOrigin.Begin);
            }
            else
            {
                await SkipAsync(baseStream, position, ct).ConfigureAwait(false);
            }

            return new StreamView(baseStream, position, length);
        }

        /// <inheritdoc />
        public override void Flush()
        {
            _baseStream.Flush();
        }

        /// <inheritdoc />
        public override int Read(byte[] buffer, int offset, int count)
        {
            var remaining = Math.Min(Length - _position, count);
            if (remaining == 0)
                return 0;
            var readCount = _baseStream.Read(buffer, offset, (int)remaining);
            _position += readCount;
            return readCount;
        }

        /// <inheritdoc />
        public override long Seek(long offset, SeekOrigin origin)
        {
            long result;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    var newPosFromBegin = Offset + offset;
                    if (newPosFromBegin < Offset)
                        newPosFromBegin = Offset;
                    if (newPosFromBegin > Offset + Length)
                        newPosFromBegin = Offset + Length;
                    result = _baseStream.Seek(newPosFromBegin, origin);
                    _position = offset;
                    break;
                case SeekOrigin.Current:
                    var newPosFromCurrent = Offset + _position + offset;
                    if (newPosFromCurrent < Offset)
                        newPosFromCurrent = Offset;
                    if (newPosFromCurrent > Offset + Length)
                        newPosFromCurrent = Offset + Length;
                    var newOffset = newPosFromCurrent - (Offset + _position);
                    result = _baseStream.Seek(newOffset, SeekOrigin.Current);
                    _position = newPosFromCurrent - Offset;
                    break;
                case SeekOrigin.End:
                    result = _baseStream.Seek(Offset + Length + offset, SeekOrigin.Begin);
                    _position = Length + offset;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return result;
        }

        /// <inheritdoc />
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
                _baseStream.Dispose();
        }

        private static async Task SkipAsync(Stream baseStream, long count, CancellationToken ct)
        {
            var buffer = new byte[65536];
            while (count != 0)
            {
                var blockSize = Math.Min(65536, count);
                await baseStream.ReadAsync(buffer, 0, (int)blockSize, ct).ConfigureAwait(false);
                count -= blockSize;
            }
        }
    }
}
