using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class MidiFileUtilities
    {
        #region Methods

        public static void ShiftEvents(this MidiFile midiFile, ITimeSpan distance)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(distance), distance);

            midiFile.GetTrackChunks().ShiftEvents(distance, midiFile.TimeDivision);
        }

        public static void Resize(this MidiFile midiFile, ITimeSpan length)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(length), length);

            // TODO: create method to get absolute time (or last time...)
            var lastTime = midiFile.GetTimedEvents().LastOrDefault()?.Time;
            if (lastTime.GetValueOrDefault() == 0)
                return;

            var tempoMap = midiFile.GetTempoMap();

            var oldLength = TimeConverter.ConvertTo((MidiTimeSpan)lastTime.Value, length.GetType(), tempoMap);
            var ratio = TimeSpanUtilities.Divide(length, oldLength);

            ResizeByRatio(midiFile, ratio);
        }

        public static void Resize(this MidiFile midiFile, double ratio)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNegative(nameof(ratio), ratio, "Ratio is negative");

            ResizeByRatio(midiFile, ratio);
        }

        private static void ResizeByRatio(MidiFile midiFile, double ratio)
        {
            midiFile.ProcessTimedEvents(e => e.Time = MathUtilities.RoundToLong(e.Time * ratio));
        }

        #endregion
    }
}
