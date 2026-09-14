using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Melanchall.DryWetMidi.Common
{
    internal sealed class EnumBasedLookup<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TKey, TValue>
        where TKey : struct, Enum
    {
#if NETCOREAPP1_0_OR_GREATER || NET5_0_OR_GREATER
        private static readonly int KeySize = Unsafe.SizeOf<TKey>();
#else
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

        public EnumBasedLookup(params (TKey key, TValue value)[] items)
        {
            _itemsCount = items.Length;

            var maxIndex = 0;
            for (int i = 0; i < items.Length; i++)
            {
                int current = ConvertEnumToInt(items[i].key);
                if (current > maxIndex)
                    maxIndex = current;
            }

            _maxSize = maxIndex + 1;

            _usedKeys = new bool[_maxSize];
            _values = new TValue[_maxSize];

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
            if (KeySize == 4)
                return Unsafe.As<TKey, int>(ref enumKey);
            
            if (KeySize == 1)
                return Unsafe.As<TKey, byte>(ref enumKey);
            
            return Unsafe.As<TKey, short>(ref enumKey);
#else
            return CastHelper<int>.Cast(enumKey);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static TKey ConvertIntToEnum(int intKey)
        {
#if NETCOREAPP1_0_OR_GREATER || NET5_0_OR_GREATER
            if (KeySize == 4)
                return Unsafe.As<int, TKey>(ref intKey);

            if (KeySize == 1)
            {
                var byteValue = (byte)intKey;
                return Unsafe.As<byte, TKey>(ref byteValue);
            }

            var shortValue = (short)intKey;
            return Unsafe.As<short, TKey>(ref shortValue);
#else
            return CastHelper<TKey>.Cast(intKey);
#endif
        }
    }
}
