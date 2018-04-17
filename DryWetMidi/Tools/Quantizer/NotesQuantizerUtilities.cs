using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public static class NotesQuantizerUtilities
    {
        #region Methods

        public static void QuantizeNotes(this TrackChunk trackChunk, IGrid grid, TempoMap tempoMap, NotesQuantizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            using (var notesManager = trackChunk.ManageNotes())
            {
                new NotesQuantizer().Quantize(notesManager.Notes, grid, tempoMap, settings);
            }
        }

        public static void QuantizeNotes(this IEnumerable<TrackChunk> trackChunks, IGrid grid, TempoMap tempoMap, NotesQuantizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.QuantizeNotes(grid, tempoMap, settings);
            }
        }

        public static void QuantizeNotes(this MidiFile midiFile, IGrid grid, NotesQuantizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(grid), grid);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().QuantizeNotes(grid, tempoMap, settings);
        }

        #endregion
    }
}
