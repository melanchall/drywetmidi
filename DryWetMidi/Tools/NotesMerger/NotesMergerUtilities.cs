using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public static class NotesMergerUtilities
    {
        #region Methods

        public static void MergeNotes(this TrackChunk trackChunk, TempoMap tempoMap, NotesMergingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            using (var notesManager = trackChunk.ManageNotes())
            {
                new NotesMerger().Merge(notesManager.Notes, tempoMap, settings);
            }
        }

        public static void MergeNotes(this IEnumerable<TrackChunk> trackChunks, TempoMap tempoMap, NotesMergingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks.Where(c => c != null))
            {
                trackChunk.MergeNotes(tempoMap, settings);
            }
        }

        public static void MergeNotes(this MidiFile midiFile, NotesMergingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().MergeNotes(tempoMap, settings);
        }

        #endregion
    }
}
