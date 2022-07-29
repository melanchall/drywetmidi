using System;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal sealed class NumbersProvider
    {
        #region Methods

        public SevenBitNumber GetNonDefaultSevenBitNumber() => (SevenBitNumber)DryWetMidi.Common.Random.Instance.Next(1, SevenBitNumber.MaxValue + 1);

        public FourBitNumber GetNonDefaultFourBitNumber() => (FourBitNumber)DryWetMidi.Common.Random.Instance.Next(1, FourBitNumber.MaxValue + 1);

        public byte[] GetNonDefaultBytesArray(int arrayMinLength, int arrayMaxLength, byte maxValue) =>
            Enumerable.Range(0, DryWetMidi.Common.Random.Instance.Next(arrayMinLength, arrayMaxLength + 1)).Select(_ => (byte)DryWetMidi.Common.Random.Instance.Next(maxValue)).ToArray();

        public byte GetNonDefaultByte() =>
            GetNonDefaultByte(v => true);

        public byte GetNonDefaultByte(Predicate<byte> predicate) =>
            GetNonDefaultByte(byte.MaxValue, predicate);

        public byte GetNonDefaultByte(byte maxValue, Predicate<byte> predicate)
        {
            var validValues = Enumerable.Range(1, maxValue).Select(v => (byte)v).Where(v => predicate(v)).ToArray();
            return validValues[DryWetMidi.Common.Random.Instance.Next(0, validValues.Length)];
        }

        public ushort GetNonDefaultUShort() => GetNonDefaultUShort(ushort.MaxValue);

        public ushort GetNonDefaultUShort(ushort maxValue) => (ushort)DryWetMidi.Common.Random.Instance.Next(1, maxValue + 1);

        #endregion
    }
}
