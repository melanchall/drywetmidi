using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides methods for splitting notes.
    /// </summary>
    public static class NotesSplitterUtilities
    {
        #region Methods

        /// <summary>
        /// Splits notes contained in the specified <see cref="TrackChunk"/> by the specified step so
        /// every note will be splitted at points equally distanced from each other starting from
        /// the note's start time.
        /// </summary>
        /// <remarks>
        /// Notes with zero length and notes with length smaller than <paramref name="step"/>
        /// will not be splitted.
        /// </remarks>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to split notes in.</param>
        /// <param name="step">Step to split notes by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is null. -or-
        /// <paramref name="step"/> is null. -or- <paramref name="tempoMap"/> is null.</exception>
        public static void SplitNotesByStep(this TrackChunk trackChunk, ITimeSpan step, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(step), step);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            SplitTrackChunkNotes(trackChunk, (splitter, notes) => splitter.SplitByStep(notes, step, tempoMap));
        }

        /// <summary>
        /// Splits notes contained in the specified collection of <see cref="TrackChunk"/> by the
        /// specified step so every note will be splitted at points equally distanced from each
        /// other starting from the note's start time.
        /// </summary>
        /// <remarks>
        /// Notes with zero length and notes with length smaller than <paramref name="step"/>
        /// will not be splitted.
        /// </remarks>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to split notes in.</param>
        /// <param name="step">Step to split notes by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is null. -or-
        /// <paramref name="step"/> is null. -or- <paramref name="tempoMap"/> is null.</exception>
        public static void SplitNotesByStep(this IEnumerable<TrackChunk> trackChunks, ITimeSpan step, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(step), step);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.SplitNotesByStep(step, tempoMap);
            }
        }

        /// <summary>
        /// Splits notes contained in the specified <see cref="MidiFile"/> by the specified
        /// step so every note will be splitted at points equally distanced from each other
        /// starting from the note's start time.
        /// </summary>
        /// <remarks>
        /// Notes with zero length and notes with length smaller than <paramref name="step"/>
        /// will not be splitted.
        /// </remarks>
        /// <param name="midiFile"><see cref="MidiFile"/> to split notes in.</param>
        /// <param name="step">Step to split notes by.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is null. -or-
        /// <paramref name="step"/> is null.</exception>
        public static void SplitNotesByStep(this MidiFile midiFile, ITimeSpan step)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(step), step);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().SplitNotesByStep(step, tempoMap);
        }

        /// <summary>
        /// Splits notes contained in the specified <see cref="TrackChunk"/> into the specified number
        /// of parts of the equal length.
        /// </summary>
        /// <remarks>
        /// If a note has zero length, it will be splitted into the specified number of parts of zero length.
        /// </remarks>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to split notes in.</param>
        /// <param name="partsNumber">The number of parts to split notes into.</param>
        /// <param name="lengthType">Type of a part's length.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is null. -or-
        /// <paramref name="tempoMap"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="partsNumber"/> is zero or negative.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="lengthType"/> specified an invalid value.</exception>
        public static void SplitNotesByPartsNumber(this TrackChunk trackChunk, int partsNumber, TimeSpanType lengthType, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            SplitTrackChunkNotes(trackChunk, (splitter, notes) => splitter.SplitByPartsNumber(notes, partsNumber, lengthType, tempoMap));
        }

        /// <summary>
        /// Splits notes contained in the specified collection of <see cref="TrackChunk"/> into the
        /// specified number of parts of the equal length.
        /// </summary>
        /// <remarks>
        /// If a note has zero length, it will be splitted into the specified number of parts of zero length.
        /// </remarks>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to split notes in.</param>
        /// <param name="partsNumber">The number of parts to split notes into.</param>
        /// <param name="lengthType">Type of a part's length.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is null. -or-
        /// <paramref name="tempoMap"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="partsNumber"/> is zero or negative.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="lengthType"/> specified an invalid value.</exception>
        public static void SplitNotesByPartsNumber(this IEnumerable<TrackChunk> trackChunks, int partsNumber, TimeSpanType lengthType, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.SplitNotesByPartsNumber(partsNumber, lengthType, tempoMap);
            }
        }

        /// <summary>
        /// Splits notes contained in the specified <see cref="MidiFile"/> into the specified number of
        /// parts of the equal length.
        /// </summary>
        /// <remarks>
        /// If a note has zero length, it will be splitted into the specified number of parts of zero length.
        /// </remarks>
        /// <param name="midiFile"><see cref="MidiFile"/> to split notes in.</param>
        /// <param name="partsNumber">The number of parts to split notes into.</param>
        /// <param name="lengthType">Type of a part's length.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="partsNumber"/> is zero or negative.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="lengthType"/> specified an invalid value.</exception>
        public static void SplitNotesByPartsNumber(this MidiFile midiFile, int partsNumber, TimeSpanType lengthType)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().SplitNotesByPartsNumber(partsNumber, lengthType, tempoMap);
        }

        /// <summary>
        /// Splits notes contained in the specified <see cref="TrackChunk"/> by the specified grid.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to split notes in.</param>
        /// <param name="grid">Grid to split notes by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunk"/> is null. -or-
        /// <paramref name="grid"/> is null. -or- <paramref name="tempoMap"/> is null.</exception>
        public static void SplitNotesByGrid(this TrackChunk trackChunk, IGrid grid, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            SplitTrackChunkNotes(trackChunk, (splitter, notes) => splitter.SplitByGrid(notes, grid, tempoMap));
        }

        /// <summary>
        /// Splits notes contained in the specified collection of <see cref="TrackChunk"/> by the
        /// specified grid.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to split notes in.</param>
        /// <param name="grid">Grid to split notes by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <exception cref="ArgumentNullException"><paramref name="trackChunks"/> is null. -or-
        /// <paramref name="grid"/> is null. -or- <paramref name="tempoMap"/> is null.</exception>
        public static void SplitNotesByGrid(this IEnumerable<TrackChunk> trackChunks, IGrid grid, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.SplitNotesByGrid(grid, tempoMap);
            }
        }

        /// <summary>
        /// Splits notes contained in the specified <see cref="MidiFile"/> by the specified grid.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to split notes in.</param>
        /// <param name="grid">Grid to split notes by.</param>
        /// <exception cref="ArgumentNullException"><paramref name="grid"/> is null.</exception>
        public static void SplitNotesByGrid(this MidiFile midiFile, IGrid grid)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(grid), grid);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().SplitNotesByGrid(grid, tempoMap);
        }

        public static void SplitNotesAtDistance(this TrackChunk trackChunk, ITimeSpan distance, LengthedObjectTarget from, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(distance), distance);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            SplitTrackChunkNotes(trackChunk, (splitter, notes) => splitter.SplitAtDistance(notes, distance, from, tempoMap));
        }

        public static void SplitNotesAtDistance(this IEnumerable<TrackChunk> trackChunks, ITimeSpan distance, LengthedObjectTarget from, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(distance), distance);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.SplitNotesAtDistance(distance, from, tempoMap);
            }
        }

        public static void SplitNotesAtDistance(this MidiFile midiFile, ITimeSpan distance, LengthedObjectTarget from)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(distance), distance);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().SplitNotesAtDistance(distance, from, tempoMap);
        }

        public static void SplitNotesAtDistance(this TrackChunk trackChunk, double ratio, TimeSpanType lengthType, LengthedObjectTarget from, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsOutOfRange(nameof(ratio),
                                         ratio,
                                         LengthedObjectsSplitter<Note>.ZeroRatio,
                                         LengthedObjectsSplitter<Note>.FullLengthRatio,
                                         $"Ratio is out of [{LengthedObjectsSplitter<Note>.ZeroRatio}; {LengthedObjectsSplitter<Note>.FullLengthRatio}] range.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            SplitTrackChunkNotes(trackChunk, (splitter, notes) => splitter.SplitAtDistance(notes, ratio, lengthType, from, tempoMap));
        }

        public static void SplitNotesAtDistance(this IEnumerable<TrackChunk> trackChunks, double ratio, TimeSpanType lengthType, LengthedObjectTarget from, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsOutOfRange(nameof(ratio),
                                         ratio,
                                         LengthedObjectsSplitter<Note>.ZeroRatio,
                                         LengthedObjectsSplitter<Note>.FullLengthRatio,
                                         $"Ratio is out of [{LengthedObjectsSplitter<Note>.ZeroRatio}; {LengthedObjectsSplitter<Note>.FullLengthRatio}] range.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.SplitNotesAtDistance(ratio, lengthType, from, tempoMap);
            }
        }

        public static void SplitNotesAtDistance(this MidiFile midiFile, double ratio, TimeSpanType lengthType, LengthedObjectTarget from)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsOutOfRange(nameof(ratio),
                                         ratio,
                                         LengthedObjectsSplitter<Note>.ZeroRatio,
                                         LengthedObjectsSplitter<Note>.FullLengthRatio,
                                         $"Ratio is out of [{LengthedObjectsSplitter<Note>.ZeroRatio}; {LengthedObjectsSplitter<Note>.FullLengthRatio}] range.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().SplitNotesAtDistance(ratio, lengthType, from, tempoMap);
        }

        private static void SplitTrackChunkNotes(TrackChunk trackChunk, Func<NotesSplitter, IEnumerable<Note>, IEnumerable<Note>> splitOperation)
        {
            using (var notesManager = trackChunk.ManageNotes())
            {
                var notes = notesManager.Notes;

                var notesSplitter = new NotesSplitter();
                var newNotes = splitOperation(notesSplitter, notes).ToList();

                notes.Clear();
                notes.Add(newNotes);
            }
        }

        #endregion
    }
}
