using System;
using System.Collections;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal sealed class SinglePassEnumerable<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> _items;

        public SinglePassEnumerable(IEnumerable<T> items)
        {
            _items = items;
        }

        public SinglePassEnumerable(params T[] items)
            : this((IEnumerable<T>)items)
        {
        }

        public int EnumerationsCount { get; private set; }

        public int DisposalsCount { get; private set; }

        public IEnumerator<T> GetEnumerator()
        {
            EnumerationsCount++;
            if (EnumerationsCount != 1)
                throw new InvalidOperationException("The sequence was enumerated more than once.");

            return Enumerate().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private IEnumerable<T> Enumerate()
        {
            try
            {
                foreach (var item in _items)
                {
                    yield return item;
                }
            }
            finally
            {
                DisposalsCount++;
            }
        }
    }
}
