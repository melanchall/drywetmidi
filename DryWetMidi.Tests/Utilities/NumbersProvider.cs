using System;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal sealed class NumbersProvider
    {
        #region Fields

        private readonly Random _random = new Random();

        #endregion

        #region Methods

        public SevenBitNumber GetNonDefaultSevenBitNumber() => (SevenBitNumber)_random.Next(1, SevenBitNumber.MaxValue + 1);

        public FourBitNumber GetNonDefaultFourBitNumber() => (FourBitNumber)_random.Next(1, FourBitNumber.MaxValue + 1);

        public byte[] GetNonDefaultBytesArray(int arrayMinLength, int arrayMaxLength, byte maxValue) =>
            Enumerable.Range(0, _random.Next(arrayMinLength, arrayMaxLength + 1)).Select(_ => (byte)_random.Next(maxValue)).ToArray();

        public byte GetNonDefaultByte() =>
            GetNonDefaultByte(v => true);

        public byte GetNonDefaultByte(Predicate<byte> predicate) =>
            GetNonDefaultByte(byte.MaxValue, predicate);

        public byte GetNonDefaultByte(byte maxValue, Predicate<byte> predicate)
        {
            var validValues = Enumerable.Range(1, maxValue).Select(v => (byte)v).Where(v => predicate(v)).ToArray();
            return validValues[_random.Next(0, validValues.Length)];
        }

        public ushort GetNonDefaultUShort() => GetNonDefaultUShort(ushort.MaxValue);

        public ushort GetNonDefaultUShort(ushort maxValue) => (ushort)_random.Next(1, maxValue + 1);

        #endregion
    }
}
