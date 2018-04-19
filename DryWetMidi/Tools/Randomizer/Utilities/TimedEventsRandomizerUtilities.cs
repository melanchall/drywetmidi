using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public static class TimedEventsRandomizerUtilities
    {
        #region Methods

        public static void RandomizeTimedEvents(this TrackChunk trackChunk, IBounds bounds, TempoMap tempoMap, TimedEventsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(bounds), bounds);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            using (var timedEventsManager = trackChunk.ManageTimedEvents())
            {
                new TimedEventsRandomizer().Randomize(timedEventsManager.Events, bounds, tempoMap, settings);
            }
        }

        public static void RandomizeTimedEvents(this IEnumerable<TrackChunk> trackChunks, IBounds bounds, TempoMap tempoMap, TimedEventsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(bounds), bounds);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.RandomizeTimedEvents(bounds, tempoMap, settings);
            }
        }

        public static void RandomizeTimedEvents(this MidiFile midiFile, IBounds bounds, TimedEventsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(bounds), bounds);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().RandomizeTimedEvents(bounds, tempoMap, settings);
        }

        #endregion
    }
}
