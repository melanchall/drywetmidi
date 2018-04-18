using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public static class ChordsRandomizerUtilities
    {
        #region Methods

        public static void RandomizeChords(this TrackChunk trackChunk, ITimeSpan tolerance, TempoMap tempoMap, long notesTolerance = 0, ChordsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(tolerance), tolerance);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfNotesTolerance.IsNegative(nameof(notesTolerance), notesTolerance);

            trackChunk.RandomizeChords(tolerance, tolerance, tempoMap, notesTolerance, settings);
        }

        public static void RandomizeChords(this TrackChunk trackChunk, ITimeSpan leftTolerance, ITimeSpan rightTolerance, TempoMap tempoMap, long notesTolerance = 0, ChordsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(leftTolerance), leftTolerance);
            ThrowIfArgument.IsNull(nameof(rightTolerance), rightTolerance);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfNotesTolerance.IsNegative(nameof(notesTolerance), notesTolerance);

            using (var chordsManager = trackChunk.ManageChords(notesTolerance))
            {
                new ChordsRandomizer().Randomize(chordsManager.Chords, leftTolerance, rightTolerance, tempoMap, settings);
            }
        }

        public static void RandomizeChords(this IEnumerable<TrackChunk> trackChunks, ITimeSpan tolerance, TempoMap tempoMap, long notesTolerance = 0, ChordsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(tolerance), tolerance);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfNotesTolerance.IsNegative(nameof(notesTolerance), notesTolerance);

            trackChunks.RandomizeChords(tolerance, tolerance, tempoMap, notesTolerance, settings);
        }

        public static void RandomizeChords(this IEnumerable<TrackChunk> trackChunks, ITimeSpan leftTolerance, ITimeSpan rightTolerance, TempoMap tempoMap, long notesTolerance = 0, ChordsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(leftTolerance), leftTolerance);
            ThrowIfArgument.IsNull(nameof(rightTolerance), rightTolerance);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfNotesTolerance.IsNegative(nameof(notesTolerance), notesTolerance);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.RandomizeChords(leftTolerance, rightTolerance, tempoMap, notesTolerance, settings);
            }
        }

        public static void RandomizeChords(this MidiFile midiFile, ITimeSpan tolerance, long notesTolerance = 0, ChordsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(tolerance), tolerance);
            ThrowIfNotesTolerance.IsNegative(nameof(notesTolerance), notesTolerance);

            midiFile.RandomizeChords(tolerance, tolerance, notesTolerance, settings);
        }

        public static void RandomizeChords(this MidiFile midiFile, ITimeSpan leftTolerance, ITimeSpan rightTolerance, long notesTolerance = 0, ChordsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(leftTolerance), leftTolerance);
            ThrowIfArgument.IsNull(nameof(rightTolerance), rightTolerance);
            ThrowIfNotesTolerance.IsNegative(nameof(notesTolerance), notesTolerance);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().RandomizeChords(leftTolerance, rightTolerance, tempoMap, notesTolerance, settings);
        }

        #endregion
    }
}
