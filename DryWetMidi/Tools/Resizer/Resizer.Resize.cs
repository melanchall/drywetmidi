using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class Resizer
    {
        #region Methods

        public static void Resize(this TrackChunk trackChunk, ITimeSpan length, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            if (!trackChunk.Events.Any())
                return;

            var duration = trackChunk.GetDuration<MidiTimeSpan>(tempoMap);
            if (duration.IsZeroTimeSpan())
                return;

            var ratio = GetRatio(duration, length, tempoMap);

            trackChunk.Resize(ratio);
        }

        public static void Resize(this TrackChunk trackChunk, double ratio)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNegative(nameof(ratio), ratio, "Ratio is negative.");

            trackChunk.ProcessTimedEvents(timedEvent => timedEvent.Time = MathUtilities.RoundToLong(timedEvent.Time * ratio));
        }

        public static void Resize(this IEnumerable<TrackChunk> trackChunks, ITimeSpan length, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(length), length);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            if (!trackChunks.Any(c => c.Events.Any()))
                return;

            var duration = trackChunks.GetDuration<MidiTimeSpan>(tempoMap);
            if (duration.IsZeroTimeSpan())
                return;

            var ratio = GetRatio(duration, length, tempoMap);

            trackChunks.Resize(ratio);
        }

        public static void Resize(this IEnumerable<TrackChunk> trackChunks, double ratio)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNegative(nameof(ratio), ratio, "Ratio is negative.");

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.Resize(ratio);
            }
        }

        public static void Resize(this MidiFile midiFile, ITimeSpan length)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(length), length);

            var tempoMap = midiFile.GetTempoMap();
            midiFile.GetTrackChunks().Resize(length, tempoMap);
        }

        public static void Resize(this MidiFile midiFile, double ratio)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNegative(nameof(ratio), ratio, "Ratio is negative.");

            var tempoMap = midiFile.GetTempoMap();
            midiFile.GetTrackChunks().Resize(ratio);
        }

        private static double GetRatio(MidiTimeSpan duration, ITimeSpan length, TempoMap tempoMap)
        {
            var oldLength = TimeConverter.ConvertTo(duration, length.GetType(), tempoMap);
            return TimeSpanUtilities.Divide(length, oldLength);
        }

        #endregion
    }
}
