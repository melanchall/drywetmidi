using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class EnumBasedLookup<TKey, TValue>
        where TKey : struct, Enum
    {
#if !(NETCOREAPP1_0_OR_GREATER || NET5_0_OR_GREATER)
        private static class CastHelper<TTarget>
        {
            public static readonly Func<object, TTarget> Cast = FlagsHelper();

            private static Func<object, TTarget> FlagsHelper() => o => (TTarget)o;
        }
#endif

        private readonly bool[] _usedKeys;
        private readonly TValue[] _values;
        private readonly int _maxSize;

        private TKey[]? _denseKeys;
        private TValue[]? _denseValues;
        private readonly int _itemsCount;

        private static readonly int CachedMaxEnumValues = GetMaxEnumValue();

        public EnumBasedLookup(params (TKey key, TValue value)[] items)
        {
            _maxSize = CachedMaxEnumValues;
            _usedKeys = new bool[_maxSize];
            _values = new TValue[_maxSize];
            _itemsCount = items.Length;

            for (int i = 0; i < items.Length; i++)
            {
                var (key, value) = items[i];
                var index = ConvertEnumToInt(key);

                if ((uint)index < (uint)_maxSize)
                {
                    _usedKeys[index] = true;
                    _values[index] = value;
                }
            }
        }

        public TValue this[TKey key]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _values[ConvertEnumToInt(key)];
        }

        public ICollection<TKey> Keys => _denseKeys ??= GenerateKeys();

        public ICollection<TValue> Values => _denseValues ??= GenerateValues();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetValue(TKey key, out TValue? value)
        {
            var index = ConvertEnumToInt(key);
            if ((uint)index < (uint)_maxSize && _usedKeys[index])
            {
                value = _values[index];
                return true;
            }

            value = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(TKey key)
        {
            var index = ConvertEnumToInt(key);
            return (uint)index < (uint)_maxSize && _usedKeys[index];
        }

        private TKey[] GenerateKeys()
        {
            var keys = new List<TKey>(_itemsCount);

            for (var i = 0; i < _usedKeys.Length; i++)
            {
                if (_usedKeys[i])
                    keys.Add(ConvertIntToEnum(i));
            }

            return [.. keys];
        }

        private TValue[] GenerateValues()
        {
            var values = new List<TValue>(_itemsCount);

            for (var i = 0; i < _usedKeys.Length; i++)
            {
                if (_usedKeys[i])
                    values.Add(_values[i]);
            }

            return [.. values];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int ConvertEnumToInt(TKey enumKey)
        {
#if NETCOREAPP1_0_OR_GREATER || NET5_0_OR_GREATER
            return Unsafe.As<TKey, int>(ref enumKey);
#else
            return CastHelper<int>.Cast(enumKey);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TKey ConvertIntToEnum(int intKey)
        {
#if NETCOREAPP1_0_OR_GREATER || NET5_0_OR_GREATER
            return Unsafe.As<int, TKey>(ref intKey);
#else
            return CastHelper<TKey>.Cast(intKey);
#endif
        }

        private static int GetMaxEnumValue()
        {
#if NET7_0_OR_GREATER
            var values = Enum.GetValues<TKey>();
#else
            var values = Enum.GetValues(typeof(TKey));
#endif
            if (values.Length == 0)
                return 0;

            var max = 0;
            foreach (var val in values)
            {
                var current = Convert.ToInt32(val);
                if (current > max)
                    max = current;
            }

            return max + 1;
        }
    }
}
