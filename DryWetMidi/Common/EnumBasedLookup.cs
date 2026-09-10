using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class EnumBasedLookup<TKey, TValue>
        where TKey : struct, Enum
    {
        private readonly TValue[] _items;

        public EnumBasedLookup(params (TKey Key, TValue Value)[] items)
        {
            var maxValue = items.Max(i => Convert.ToInt32(i.Key));

            _items = new TValue[maxValue + 1];
            foreach (var (key, value) in items)
            {
                _items[Convert.ToInt32(key)] = value;
            }
        }

        public TValue[] Items => _items;

        public TValue this[TKey key] => _items[Convert.ToInt32(key)];
    }
}
