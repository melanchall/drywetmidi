using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public static class ChordsSplitterUtilities
    {
        #region Methods

        public static void SplitChordsByStep(this TrackChunk trackChunk, ITimeSpan step, TempoMap tempoMap, long notesTolerance = 0)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(step), step);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfNotesTolerance.IsNegative(nameof(notesTolerance), notesTolerance);

            SplitTrackChunkChords(trackChunk, (splitter, notes) => splitter.SplitByStep(notes, step, tempoMap), notesTolerance);
        }

        public static void SplitChordsByStep(this IEnumerable<TrackChunk> trackChunks, ITimeSpan step, TempoMap tempoMap, long notesTolerance = 0)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(step), step);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfNotesTolerance.IsNegative(nameof(notesTolerance), notesTolerance);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.SplitChordsByStep(step, tempoMap, notesTolerance);
            }
        }

        public static void SplitChordsByStep(this MidiFile midiFile, ITimeSpan step, long notesTolerance = 0)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(step), step);
            ThrowIfNotesTolerance.IsNegative(nameof(notesTolerance), notesTolerance);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().SplitChordsByStep(step, tempoMap, notesTolerance);
        }

        public static void SplitChordsByPartsNumber(this TrackChunk trackChunk, int partsNumber, TimeSpanType lengthType, TempoMap tempoMap, long notesTolerance = 0)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfNotesTolerance.IsNegative(nameof(notesTolerance), notesTolerance);

            SplitTrackChunkChords(trackChunk, (splitter, notes) => splitter.SplitByPartsNumber(notes, partsNumber, lengthType, tempoMap), notesTolerance);
        }

        public static void SplitChordsByPartsNumber(this IEnumerable<TrackChunk> trackChunks, int partsNumber, TimeSpanType lengthType, TempoMap tempoMap, long notesTolerance = 0)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfNotesTolerance.IsNegative(nameof(notesTolerance), notesTolerance);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.SplitChordsByPartsNumber(partsNumber, lengthType, tempoMap, notesTolerance);
            }
        }

        public static void SplitChordsByPartsNumber(this MidiFile midiFile, int partsNumber, TimeSpanType lengthType, long notesTolerance = 0)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfNotesTolerance.IsNegative(nameof(notesTolerance), notesTolerance);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().SplitChordsByPartsNumber(partsNumber, lengthType, tempoMap, notesTolerance);
        }

        public static void SplitChordsByGrid(this TrackChunk trackChunk, IGrid grid, TempoMap tempoMap, long notesTolerance = 0)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfNotesTolerance.IsNegative(nameof(notesTolerance), notesTolerance);

            SplitTrackChunkChords(trackChunk, (splitter, notes) => splitter.SplitByGrid(notes, grid, tempoMap), notesTolerance);
        }

        public static void SplitChordsByGrid(this IEnumerable<TrackChunk> trackChunks, IGrid grid, TempoMap tempoMap, long notesTolerance = 0)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);
            ThrowIfNotesTolerance.IsNegative(nameof(notesTolerance), notesTolerance);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.SplitChordsByGrid(grid, tempoMap, notesTolerance);
            }
        }

        public static void SplitChordsByGrid(this MidiFile midiFile, IGrid grid, long notesTolerance = 0)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfNotesTolerance.IsNegative(nameof(notesTolerance), notesTolerance);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().SplitChordsByGrid(grid, tempoMap, notesTolerance);
        }

        private static void SplitTrackChunkChords(TrackChunk trackChunk, Func<ChordsSplitter, IEnumerable<Chord>, IEnumerable<Chord>> splitOperation, long notesTolerance)
        {
            using (var chordsManager = trackChunk.ManageChords(notesTolerance))
            {
                var chords = chordsManager.Chords;

                var chordsSplitter = new ChordsSplitter();
                var newChords = splitOperation(chordsSplitter, chords);

                chords.Clear();
                chords.Add(newChords);
            }
        }

        #endregion
    }
}
