using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public static class NotesRandomizerUtilities
    {
        #region Methods

        public static void RandomizeNotes(this TrackChunk trackChunk, IBounds bounds, TempoMap tempoMap, NotesRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(bounds), bounds);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            using (var notesManager = trackChunk.ManageNotes())
            {
                new NotesRandomizer().Randomize(notesManager.Notes, bounds, tempoMap, settings);
            }
        }

        public static void RandomizeNotes(this IEnumerable<TrackChunk> trackChunks, IBounds bounds, TempoMap tempoMap, NotesRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(bounds), bounds);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.RandomizeNotes(bounds, tempoMap, settings);
            }
        }

        public static void RandomizeNotes(this MidiFile midiFile, IBounds bounds, NotesRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(bounds), bounds);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().RandomizeNotes(bounds, tempoMap, settings);
        }

        #endregion
    }
}
