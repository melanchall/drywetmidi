using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class MidiTimeSpanConverter : ITimeSpanConverter
    {
        #region ITimeSpanConverter

        public ITimeSpan ConvertTo(long timeSpan, long time, TempoMap tempoMap)
        {
            ThrowIfLengthArgument.IsNegative(nameof(timeSpan), timeSpan);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            return (MidiTimeSpan)timeSpan;
        }

        public long ConvertFrom(ITimeSpan timeSpan, long time, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);
            ThrowIfTimeArgument.IsNegative(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var midiTimeSpan = timeSpan as MidiTimeSpan;
            if (midiTimeSpan == null)
                throw new ArgumentException($"Time span is not an instance of the {nameof(MidiTimeSpan)}.", nameof(timeSpan));

            return midiTimeSpan.TimeSpan;
        }

        #endregion
    }
}
