using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class MidiTimeConverter : ITimeConverter
    {
        #region ITimeConverter

        public ITime ConvertTo(long time, TempoMap tempoMap)
        {
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return new MidiTime(time);
        }

        public long ConvertFrom(ITime time, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var midiTime = time as MidiTime;
            if (midiTime == null)
                throw new ArgumentException($"Time is not an instance of the {nameof(MidiTime)}.", nameof(time));

            return midiTime.Time;
        }

        #endregion
    }
}
