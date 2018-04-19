using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public static class ChordsRandomizerUtilities
    {
        #region Methods

        public static void RandomizeChords(this TrackChunk trackChunk, IBounds bounds, TempoMap tempoMap, long notesTolerance = 0, ChordsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(bounds), bounds);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfNotesTolerance.IsNegative(nameof(notesTolerance), notesTolerance);

            using (var chordsManager = trackChunk.ManageChords(notesTolerance))
            {
                new ChordsRandomizer().Randomize(chordsManager.Chords, bounds, tempoMap, settings);
            }
        }

        public static void RandomizeChords(this IEnumerable<TrackChunk> trackChunks, IBounds bounds, TempoMap tempoMap, long notesTolerance = 0, ChordsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(bounds), bounds);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfNotesTolerance.IsNegative(nameof(notesTolerance), notesTolerance);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.RandomizeChords(bounds, tempoMap, notesTolerance, settings);
            }
        }

        public static void RandomizeChords(this MidiFile midiFile, IBounds bounds, long notesTolerance = 0, ChordsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(bounds), bounds);
            ThrowIfNotesTolerance.IsNegative(nameof(notesTolerance), notesTolerance);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().RandomizeChords(bounds, tempoMap, notesTolerance, settings);
        }

        #endregion
    }
}
