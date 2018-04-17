using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public static class TimedEventsQuantizerUtilities
    {
        #region Methods

        public static void QuantizeTimedEvents(this TrackChunk trackChunk, IGrid grid, TempoMap tempoMap, TimedEventsQuantizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            using (var timedEventsManager = trackChunk.ManageTimedEvents())
            {
                new TimedEventsQuantizer().Quantize(timedEventsManager.Events, grid, tempoMap, settings);
            }
        }

        public static void QuantizeTimedEvents(this IEnumerable<TrackChunk> trackChunks, IGrid grid, TempoMap tempoMap, TimedEventsQuantizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.QuantizeTimedEvents(grid, tempoMap, settings);
            }
        }

        public static void QuantizeTimedEvents(this MidiFile midiFile, IGrid grid, TimedEventsQuantizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(grid), grid);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().QuantizeTimedEvents(grid, tempoMap, settings);
        }

        #endregion
    }
}
