using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class MidiLengthConverter : ILengthConverter
    {
        #region ILengthConverter

        public ILength ConvertTo(long length, long time, TempoMap tempoMap)
        {
            ThrowIfLengthArgument.IsNegative(nameof(length), length);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return new MidiLength(length);
        }

        public long ConvertFrom(ILength length, long time, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var midiLength = length as MidiLength;
            if (midiLength == null)
                throw new ArgumentException($"Length is not an instance of the {nameof(MidiLength)}.", nameof(length));

            return midiLength.Length;
        }

        #endregion
    }
}
