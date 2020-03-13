using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class CircularBuffer<T>
    {
        #region Fields

        private readonly int _capacity;
        private readonly T[] _buffer;

        private int _start;
        private int _index = -1;
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

        public bool IsFull { get; private set; }

        #endregion

        #region Methods

        public void Add(T value)
        {
            if (_position >= GetItemsCount())
                _position = Math.Min(_position + 1, _capacity);

            if (IsFull || _index == _capacity - 1)
            {
                _start = (_start + 1) % _capacity;
                IsFull = true;
            }

            _index = (_index + 1) % _capacity;
            _buffer[_index] = value;
        }

        public T[] MovePositionForward(int offset)
        {
            var items = GetItems().Skip(_position).Take(offset).ToArray();
            _position += items.Length;
            return items;
        }

        public void MovePositionBack(int offset)
        {
            if (offset > _position)
                throw new InvalidOperationException("Failed to move position back beyond the start of the buffer.");

            _position -= offset;
        }

        private int GetItemsCount()
        {
            return IsFull ? _capacity : _index + 1;
        }

        private IEnumerable<T> GetItems()
        {
            var items = Enumerable.Empty<T>();
            if (IsFull)
            {
                if (_start == 0)
                    return _buffer;

                items = GetItems(_start, _capacity - 1);
            }

            return items.Concat(GetItems(0, _index));
        }

        private IEnumerable<T> GetItems(int start, int end)
        {
            for (int i = start; i <= end; i++)
            {
                yield return _buffer[i];
            }
        }

        #endregion
    }
}
