using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class StreamWrapper : Stream
    {
        #region Constants

        private const int MinimumBufferCapacity = 4;

        #endregion

        #region Fields

        private readonly Stream _stream;
        private readonly CircularBuffer<byte> _buffer;

        private readonly byte[] _peekBuffer = new byte[1];
        private readonly byte[] _skipBytesBuffer = new byte[1024];

        private long _position;
        private bool _hasPeekedByte;
        private bool _endReached;

        #endregion

        #region Constructor

        public StreamWrapper(Stream stream, int bufferCapacity)
        {
            _stream = stream;

            // Detecting a non-RIFF file requires replaying its four-byte chunk ID.
            _buffer = new CircularBuffer<byte>(Math.Max(MinimumBufferCapacity, bufferCapacity));
        }

        #endregion

        #region Properties

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => false;

        public override long Length => long.MaxValue;

        public override long Position
        {
            get { return _position; }
            set
            {
                ThrowIfArgument.IsNegative(nameof(value), value, "Position is negative.");

                var offset = value - _position;
                if (offset == 0)
                    return;

                if (offset > 0)
                    SkipBytes(offset);
                else
                {
                    if (-offset > _buffer.RewindCount)
                        throw new InvalidOperationException("Failed to move position back beyond the start of the buffer.");

                    _buffer.MovePositionBack((int)-offset);
                    _position = value;
                }
            }
        }

        internal long? KnownLength { get; private set; }

        #endregion

        #region Methods

        public bool IsEndReached()
        {
            if (_buffer.AvailableCount > 0 || _hasPeekedByte)
                return false;

            if (_endReached)
                return true;

            var count = _stream.Read(_peekBuffer, 0, 1);
            _hasPeekedByte = count > 0;
            RecordRead(count);

            return _endReached;
        }

        internal async ValueTask<bool> IsEndReachedAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_buffer.AvailableCount > 0 || _hasPeekedByte)
                return false;

            if (_endReached)
                return true;

            var count = await _stream.ReadAsync(_peekBuffer, 0, 1, cancellationToken).ConfigureAwait(false);
            _hasPeekedByte = count > 0;
            RecordRead(count);

            return _endReached;
        }

        internal async ValueTask SetPositionAsync(long position, CancellationToken cancellationToken)
        {
            if (position <= _position)
            {
                Position = position;
                return;
            }

            while (_position < position)
            {
                var count = await ReadAsync(
                    _skipBytesBuffer,
                    0,
                    (int)Math.Min(position - _position, _skipBytesBuffer.Length),
                    cancellationToken).ConfigureAwait(false);

                if (count == 0)
                    break;
            }
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var bufferedCount = ReadBuffered(buffer.AsSpan(offset, count));
            if (bufferedCount > 0 || count == 0 || _endReached)
                return bufferedCount;

            var read = await _stream.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            _buffer.Add(buffer.AsSpan(offset, read));
            _position += read;
            RecordRead(read);

            return read;
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return Read(buffer.AsSpan(offset, count));
        }

        public override int Read(Span<byte> buffer)
        {
            var bufferedCount = ReadBuffered(buffer);
            if (bufferedCount > 0 || buffer.IsEmpty || _endReached)
                return bufferedCount;

            var read = _stream.Read(buffer);
            _buffer.Add(buffer.Slice(0, read));
            _position += read;
            RecordRead(read);

            return read;
        }

        public override int ReadByte()
        {
            Span<byte> buffer = stackalloc byte[1];

            return Read(buffer) == 0 ? -1 : buffer[0];
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        private int ReadBuffered(Span<byte> destination)
        {
            var count = _buffer.MovePositionForward(destination);
            if (count > 0)
            {
                _position += count;
                return count;
            }

            if (destination.IsEmpty || !_hasPeekedByte)
                return 0;

            destination[0] = _peekBuffer[0];
            _buffer.Add(_peekBuffer[0]);
            _hasPeekedByte = false;
            _position++;

            return 1;
        }

        private void RecordRead(int count)
        {
            if (count != 0)
                return;

            _endReached = true;
            KnownLength = _position;
        }

        private void SkipBytes(long count)
        {
            while (count > 0)
            {
                var readBytesCount = Read(_skipBytesBuffer, 0, (int)Math.Min(count, _skipBytesBuffer.Length));
                if (readBytesCount == 0)
                    break;

                count -= readBytesCount;
            }
        }

        #endregion
    }
}
