using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public static class NotesRandomizerUtilities
    {
        #region Methods

        public static void RandomizeNotes(this TrackChunk trackChunk, ITimeSpan tolerance, TempoMap tempoMap, NotesRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(tolerance), tolerance);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            trackChunk.RandomizeNotes(tolerance, tolerance, tempoMap, settings);
        }

        public static void RandomizeNotes(this TrackChunk trackChunk, ITimeSpan leftTolerance, ITimeSpan rightTolerance, TempoMap tempoMap, NotesRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(leftTolerance), leftTolerance);
            ThrowIfArgument.IsNull(nameof(rightTolerance), rightTolerance);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            using (var notesManager = trackChunk.ManageNotes())
            {
                new NotesRandomizer().Randomize(notesManager.Notes, leftTolerance, rightTolerance, tempoMap, settings);
            }
        }

        public static void RandomizeNotes(this IEnumerable<TrackChunk> trackChunks, ITimeSpan tolerance, TempoMap tempoMap, NotesRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(tolerance), tolerance);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            trackChunks.RandomizeNotes(tolerance, tolerance, tempoMap, settings);
        }

        public static void RandomizeNotes(this IEnumerable<TrackChunk> trackChunks, ITimeSpan leftTolerance, ITimeSpan rightTolerance, TempoMap tempoMap, NotesRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(leftTolerance), leftTolerance);
            ThrowIfArgument.IsNull(nameof(rightTolerance), rightTolerance);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.RandomizeNotes(leftTolerance, rightTolerance, tempoMap, settings);
            }
        }

        public static void RandomizeNotes(this MidiFile midiFile, ITimeSpan tolerance, NotesRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(tolerance), tolerance);

            midiFile.RandomizeNotes(tolerance, tolerance, settings);
        }

        public static void RandomizeNotes(this MidiFile midiFile, ITimeSpan leftTolerance, ITimeSpan rightTolerance, NotesRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(leftTolerance), leftTolerance);
            ThrowIfArgument.IsNull(nameof(rightTolerance), rightTolerance);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().RandomizeNotes(leftTolerance, rightTolerance, tempoMap, settings);
        }

        #endregion
    }
}
