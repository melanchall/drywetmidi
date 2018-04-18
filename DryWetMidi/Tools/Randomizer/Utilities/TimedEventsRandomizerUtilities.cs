using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public static class TimedEventsRandomizerUtilities
    {
        #region Methods

        public static void RandomizeTimedEvents(this TrackChunk trackChunk, ITimeSpan tolerance, TempoMap tempoMap, TimedEventsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(tolerance), tolerance);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            trackChunk.RandomizeTimedEvents(tolerance, tolerance, tempoMap, settings);
        }

        public static void RandomizeTimedEvents(this TrackChunk trackChunk, ITimeSpan leftTolerance, ITimeSpan rightTolerance, TempoMap tempoMap, TimedEventsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(leftTolerance), leftTolerance);
            ThrowIfArgument.IsNull(nameof(rightTolerance), rightTolerance);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            using (var timedEventsManager = trackChunk.ManageTimedEvents())
            {
                new TimedEventsRandomizer().Randomize(timedEventsManager.Events, leftTolerance, rightTolerance, tempoMap, settings);
            }
        }

        public static void RandomizeTimedEvents(this IEnumerable<TrackChunk> trackChunks, ITimeSpan tolerance, TempoMap tempoMap, TimedEventsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(tolerance), tolerance);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            trackChunks.RandomizeTimedEvents(tolerance, tolerance, tempoMap, settings);
        }

        public static void RandomizeTimedEvents(this IEnumerable<TrackChunk> trackChunks, ITimeSpan leftTolerance, ITimeSpan rightTolerance, TempoMap tempoMap, TimedEventsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(leftTolerance), leftTolerance);
            ThrowIfArgument.IsNull(nameof(rightTolerance), rightTolerance);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.RandomizeTimedEvents(leftTolerance, rightTolerance, tempoMap, settings);
            }
        }

        public static void RandomizeTimedEvents(this MidiFile midiFile, ITimeSpan tolerance, TimedEventsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(tolerance), tolerance);

            midiFile.RandomizeTimedEvents(tolerance, tolerance, settings);
        }

        public static void RandomizeTimedEvents(this MidiFile midiFile, ITimeSpan leftTolerance, ITimeSpan rightTolerance, TimedEventsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(leftTolerance), leftTolerance);
            ThrowIfArgument.IsNull(nameof(rightTolerance), rightTolerance);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().RandomizeTimedEvents(leftTolerance, rightTolerance, tempoMap, settings);
        }

        #endregion
    }
}
