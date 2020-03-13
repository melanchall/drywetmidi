using System;
using System.IO;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class StreamWrapper : Stream
    {
        #region Fields

        private readonly Stream _stream;
        private readonly CircularBuffer<byte> _buffer;

        private readonly byte[] _peekBuffer = new byte[1];
        private readonly byte[] _skipBytesBuffer = new byte[1024];

        private long _position;

        #endregion

        #region Constructor

        public StreamWrapper(Stream stream, int bufferCapacity)
        {
            _stream = stream;
            _buffer = new CircularBuffer<byte>(bufferCapacity);
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

                var offset = (int)(value - _position);
                if (offset == 0)
                    return;

                if (offset > 0)
                    SkipBytes(offset);
                else
                    _buffer.MovePositionBack(-offset);

                _position = value;
            }
        }

        #endregion

        #region Methods

        public bool IsEndReached()
        {
            var readBytesCount = Read(_peekBuffer, 0, 1);
            if (readBytesCount == 0)
                return true;

            Position--;
            return false;
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var bufferedData = _buffer.MovePositionForward(count);
            Buffer.BlockCopy(bufferedData, 0, buffer, offset, bufferedData.Length);

            offset += bufferedData.Length;
            var readBytesCount = _stream.Read(buffer, offset, count - bufferedData.Length);

            for (int i = 0; i < readBytesCount; i++)
            {
                _buffer.Add(buffer[offset + i]);
            }

            var totalBytesCount = bufferedData.Length + readBytesCount;
            _position += totalBytesCount;
            if (count > 0 && totalBytesCount == 0)
                _position = long.MaxValue;

            return totalBytesCount;
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

        private void SkipBytes(int count)
        {
            while (count > 0)
            {
                var readBytesCount = Read(_skipBytesBuffer, 0, Math.Min(count, _skipBytesBuffer.Length));
                if (readBytesCount == 0)
                    break;

                count -= readBytesCount;
            }
        }

        #endregion
    }
}
