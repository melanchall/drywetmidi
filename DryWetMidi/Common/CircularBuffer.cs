using System;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class CircularBuffer<T>
    {
        #region Fields

        private readonly int _capacity;
        private readonly T[] _buffer;

        private int _start;
        private int _count;
        private int _position;

        #endregion

        #region Constructor

        public CircularBuffer(int capacity)
        {
            _buffer = new T[capacity];
            _capacity = capacity;
        }

        #endregion

        #region Properties

        public int AvailableCount => _count - _position;

        public int RewindCount => _position;

        #endregion

        #region Methods

        public void Add(T value)
        {
            if (_count == _capacity)
            {
                _buffer[_start] = value;
                _start = (_start + 1) % _capacity;
            }
            else
            {
                _buffer[(_start + _count) % _capacity] = value;
                _count++;
            }

            _position = _count;
        }

        public void Add(ReadOnlySpan<T> values)
        {
            foreach (var value in values)
            {
                Add(value);
            }
        }

        public int MovePositionForward(Span<T> destination)
        {
            var count = Math.Min(destination.Length, AvailableCount);
            if (count == 0)
                return 0;

            var start = (_start + _position) % _capacity;
            var firstCount = Math.Min(count, _capacity - start);
            _buffer.AsSpan(start, firstCount).CopyTo(destination);
            _buffer.AsSpan(0, count - firstCount).CopyTo(destination.Slice(firstCount));
            _position += count;

            return count;
        }

        public void MovePositionBack(int offset)
        {
            if (offset > _position)
                throw new InvalidOperationException("Failed to move position back beyond the start of the buffer.");

            _position -= offset;
        }

        #endregion
    }
}
