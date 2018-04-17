using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public static class ChordsQuantizerUtilities
    {
        #region Methods

        public static void QuantizeChords(this TrackChunk trackChunk, IGrid grid, TempoMap tempoMap, long notesTolerance = 0, ChordsQuantizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfNotesTolerance.IsNegative(nameof(notesTolerance), notesTolerance);

            using (var chordsManager = trackChunk.ManageChords(notesTolerance))
            {
                new ChordsQuantizer().Quantize(chordsManager.Chords, grid, tempoMap, settings);
            }
        }

        public static void QuantizeChords(this IEnumerable<TrackChunk> trackChunks, IGrid grid, TempoMap tempoMap, long notesTolerance = 0, ChordsQuantizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfNotesTolerance.IsNegative(nameof(notesTolerance), notesTolerance);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.QuantizeChords(grid, tempoMap, notesTolerance, settings);
            }
        }

        public static void QuantizeChords(this MidiFile midiFile, IGrid grid, long notesTolerance = 0, ChordsQuantizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfNotesTolerance.IsNegative(nameof(notesTolerance), notesTolerance);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().QuantizeChords(grid, tempoMap, notesTolerance, settings);
        }

        #endregion
    }
}
