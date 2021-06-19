using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

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
        /// every note will be split at points equally distanced from each other starting from
        /// the note's start time.
        /// </summary>
        /// <remarks>
        /// Notes with zero length and notes with length smaller than <paramref name="step"/>
        /// will not be split.
        /// </remarks>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to split notes in.</param>
        /// <param name="step">Step to split notes by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <param name="noteDetectionSettings">Settings accoridng to which notes should be detected and built.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="step"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void SplitNotesByStep(this TrackChunk trackChunk, ITimeSpan step, TempoMap tempoMap, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(step), step);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            SplitTrackChunkNotes(trackChunk, noteDetectionSettings, (splitter, notes) => splitter.SplitByStep(notes, step, tempoMap));
        }

        /// <summary>
        /// Splits notes contained in the specified collection of <see cref="TrackChunk"/> by the
        /// specified step so every note will be split at points equally distanced from each
        /// other starting from the note's start time.
        /// </summary>
        /// <remarks>
        /// Notes with zero length and notes with length smaller than <paramref name="step"/>
        /// will not be split.
        /// </remarks>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to split notes in.</param>
        /// <param name="step">Step to split notes by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <param name="noteDetectionSettings">Settings accoridng to which notes should be detected and built.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="step"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void SplitNotesByStep(this IEnumerable<TrackChunk> trackChunks, ITimeSpan step, TempoMap tempoMap, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(step), step);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.SplitNotesByStep(step, tempoMap, noteDetectionSettings);
            }
        }

        /// <summary>
        /// Splits notes contained in the specified <see cref="MidiFile"/> by the specified
        /// step so every note will be split at points equally distanced from each other
        /// starting from the note's start time.
        /// </summary>
        /// <remarks>
        /// Notes with zero length and notes with length smaller than <paramref name="step"/>
        /// will not be split.
        /// </remarks>
        /// <param name="midiFile"><see cref="MidiFile"/> to split notes in.</param>
        /// <param name="step">Step to split notes by.</param>
        /// <param name="noteDetectionSettings">Settings accoridng to which notes should be detected and built.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiFile"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="step"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void SplitNotesByStep(this MidiFile midiFile, ITimeSpan step, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(step), step);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().SplitNotesByStep(step, tempoMap, noteDetectionSettings);
        }

        /// <summary>
        /// Splits notes contained in the specified <see cref="TrackChunk"/> into the specified number
        /// of parts of the equal length.
        /// </summary>
        /// <remarks>
        /// If a note has zero length, it will be split into the specified number of parts of zero length.
        /// </remarks>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to split notes in.</param>
        /// <param name="partsNumber">The number of parts to split notes into.</param>
        /// <param name="lengthType">Type of a part's length.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <param name="noteDetectionSettings">Settings accoridng to which notes should be detected and built.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="partsNumber"/> is zero or negative.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="lengthType"/> specified an invalid value.</exception>
        public static void SplitNotesByPartsNumber(this TrackChunk trackChunk, int partsNumber, TimeSpanType lengthType, TempoMap tempoMap, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            SplitTrackChunkNotes(trackChunk, noteDetectionSettings, (splitter, notes) => splitter.SplitByPartsNumber(notes, partsNumber, lengthType, tempoMap));
        }

        /// <summary>
        /// Splits notes contained in the specified collection of <see cref="TrackChunk"/> into the
        /// specified number of parts of the equal length.
        /// </summary>
        /// <remarks>
        /// If a note has zero length, it will be split into the specified number of parts of zero length.
        /// </remarks>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to split notes in.</param>
        /// <param name="partsNumber">The number of parts to split notes into.</param>
        /// <param name="lengthType">Type of a part's length.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <param name="noteDetectionSettings">Settings accoridng to which notes should be detected and built.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="partsNumber"/> is zero or negative.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="lengthType"/> specified an invalid value.</exception>
        public static void SplitNotesByPartsNumber(this IEnumerable<TrackChunk> trackChunks, int partsNumber, TimeSpanType lengthType, TempoMap tempoMap, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.SplitNotesByPartsNumber(partsNumber, lengthType, tempoMap, noteDetectionSettings);
            }
        }

        /// <summary>
        /// Splits notes contained in the specified <see cref="MidiFile"/> into the specified number of
        /// parts of the equal length.
        /// </summary>
        /// <remarks>
        /// If a note has zero length, it will be split into the specified number of parts of zero length.
        /// </remarks>
        /// <param name="midiFile"><see cref="MidiFile"/> to split notes in.</param>
        /// <param name="partsNumber">The number of parts to split notes into.</param>
        /// <param name="lengthType">Type of a part's length.</param>
        /// <param name="noteDetectionSettings">Settings accoridng to which notes should be detected and built.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="partsNumber"/> is zero or negative.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="lengthType"/> specified an invalid value.</exception>
        public static void SplitNotesByPartsNumber(this MidiFile midiFile, int partsNumber, TimeSpanType lengthType, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().SplitNotesByPartsNumber(partsNumber, lengthType, tempoMap, noteDetectionSettings);
        }

        /// <summary>
        /// Splits notes contained in the specified <see cref="TrackChunk"/> by the specified grid.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to split notes in.</param>
        /// <param name="grid">Grid to split notes by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <param name="noteDetectionSettings">Settings accoridng to which notes should be detected and built.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="grid"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void SplitNotesByGrid(this TrackChunk trackChunk, IGrid grid, TempoMap tempoMap, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            SplitTrackChunkNotes(trackChunk, noteDetectionSettings, (splitter, notes) => splitter.SplitByGrid(notes, grid, tempoMap));
        }

        /// <summary>
        /// Splits notes contained in the specified collection of <see cref="TrackChunk"/> by the
        /// specified grid.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to split notes in.</param>
        /// <param name="grid">Grid to split notes by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <param name="noteDetectionSettings">Settings accoridng to which notes should be detected and built.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="grid"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void SplitNotesByGrid(this IEnumerable<TrackChunk> trackChunks, IGrid grid, TempoMap tempoMap, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.SplitNotesByGrid(grid, tempoMap, noteDetectionSettings);
            }
        }

        /// <summary>
        /// Splits notes contained in the specified <see cref="MidiFile"/> by the specified grid.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to split notes in.</param>
        /// <param name="grid">Grid to split notes by.</param>
        /// <param name="noteDetectionSettings">Settings accoridng to which notes should be detected and built.</param>
        /// <exception cref="ArgumentNullException"><paramref name="grid"/> is <c>null</c>.</exception>
        public static void SplitNotesByGrid(this MidiFile midiFile, IGrid grid, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(grid), grid);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().SplitNotesByGrid(grid, tempoMap, noteDetectionSettings);
        }

        /// <summary>
        /// Splits notes contained in the specified <see cref="TrackChunk"/> at the specified distance
        /// from a note's start or end.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to split notes in.</param>
        /// <param name="distance">Distance to split notes at.</param>
        /// <param name="from">Point of a note <paramref name="distance"/> should be measured from.</param>
        /// <param name="tempoMap">Tempo map used for distances calculations.</param>
        /// <param name="noteDetectionSettings">Settings accoridng to which notes should be detected and built.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="distance"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="from"/> specified an invalid value.</exception>
        public static void SplitNotesAtDistance(this TrackChunk trackChunk, ITimeSpan distance, LengthedObjectTarget from, TempoMap tempoMap, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(distance), distance);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            SplitTrackChunkNotes(trackChunk, noteDetectionSettings, (splitter, notes) => splitter.SplitAtDistance(notes, distance, from, tempoMap));
        }

        /// <summary>
        /// Splits notes contained in the specified collection of <see cref="TrackChunk"/> at the specified
        /// distance from a note's start or end.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to split notes in.</param>
        /// <param name="distance">Distance to split notes at.</param>
        /// <param name="from">Point of a note <paramref name="distance"/> should be measured from.</param>
        /// <param name="noteDetectionSettings">Settings accoridng to which notes should be detected and built.</param>
        /// <param name="tempoMap">Tempo map used for distances calculations.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="distance"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="from"/> specified an invalid value.</exception>
        public static void SplitNotesAtDistance(this IEnumerable<TrackChunk> trackChunks, ITimeSpan distance, LengthedObjectTarget from, TempoMap tempoMap, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(distance), distance);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.SplitNotesAtDistance(distance, from, tempoMap, noteDetectionSettings);
            }
        }

        /// <summary>
        /// Splits notes contained in the specified <see cref="MidiFile"/> at the specified distance
        /// from a note's start or end.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to split notes in.</param>
        /// <param name="distance">Distance to split notes at.</param>
        /// <param name="from">Point of a note <paramref name="distance"/> should be measured from.</param>
        /// <param name="noteDetectionSettings">Settings accoridng to which notes should be detected and built.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiFile"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="distance"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="from"/> specified an invalid value.</exception>
        public static void SplitNotesAtDistance(this MidiFile midiFile, ITimeSpan distance, LengthedObjectTarget from, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(distance), distance);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().SplitNotesAtDistance(distance, from, tempoMap, noteDetectionSettings);
        }

        /// <summary>
        /// Splits notes contained in the specified <see cref="TrackChunk"/> by the specified ratio of a
        /// note's length measuring it from the note's start or end. For example, 0.5 means splitting
        /// at the center of a note.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to split notes in.</param>
        /// <param name="ratio">Ratio of a note's length to split by. Valid values are from 0 to 1.</param>
        /// <param name="lengthType">The type a note's length should be processed according to.</param>
        /// <param name="from">Point of a note distance should be measured from.</param>
        /// <param name="tempoMap">Tempo map used for distances calculations.</param>
        /// <param name="noteDetectionSettings">Settings accoridng to which notes should be detected and built.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ratio"/> is out of valid range.</exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="lengthType"/> specified an invalid value.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="from"/> specified an invalid value.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void SplitNotesAtDistance(this TrackChunk trackChunk, double ratio, TimeSpanType lengthType, LengthedObjectTarget from, TempoMap tempoMap, NoteDetectionSettings noteDetectionSettings = null)
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

            SplitTrackChunkNotes(trackChunk, noteDetectionSettings, (splitter, notes) => splitter.SplitAtDistance(notes, ratio, lengthType, from, tempoMap));
        }

        /// <summary>
        /// Splits notes contained in the specified collection of <see cref="TrackChunk"/> by the
        /// specified ratio of a note's length measuring it from the note's start or end.
        /// For example, 0.5 means splitting at the center of a note.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to split notes in.</param>
        /// <param name="ratio">Ratio of a note's length to split by. Valid values are from 0 to 1.</param>
        /// <param name="lengthType">The type a note's length should be processed according to.</param>
        /// <param name="from">Point of a note distance should be measured from.</param>
        /// <param name="tempoMap">Tempo map used for distances calculations.</param>
        /// <param name="noteDetectionSettings">Settings accoridng to which notes should be detected and built.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ratio"/> is out of valid range.</exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="lengthType"/> specified an invalid value.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="from"/> specified an invalid value.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void SplitNotesAtDistance(this IEnumerable<TrackChunk> trackChunks, double ratio, TimeSpanType lengthType, LengthedObjectTarget from, TempoMap tempoMap, NoteDetectionSettings noteDetectionSettings = null)
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
                trackChunk.SplitNotesAtDistance(ratio, lengthType, from, tempoMap, noteDetectionSettings);
            }
        }

        /// <summary>
        /// Splits notes contained in the specified <see cref="MidiFile"/> by the specified ratio of a
        /// note's length measuring it from the note's start or end. For example, 0.5 means splitting
        /// at the center of a note.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to split notes in.</param>
        /// <param name="ratio">Ratio of a note's length to split by. Valid values are from 0 to 1.</param>
        /// <param name="lengthType">The type a note's length should be processed according to.</param>
        /// <param name="from">Point of a note distance should be measured from.</param>
        /// <param name="noteDetectionSettings">Settings accoridng to which notes should be detected and built.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ratio"/> is out of valid range.</exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="lengthType"/> specified an invalid value.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="from"/> specified an invalid value.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void SplitNotesAtDistance(this MidiFile midiFile, double ratio, TimeSpanType lengthType, LengthedObjectTarget from, NoteDetectionSettings noteDetectionSettings = null)
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

            midiFile.GetTrackChunks().SplitNotesAtDistance(ratio, lengthType, from, tempoMap, noteDetectionSettings);
        }

        private static void SplitTrackChunkNotes(TrackChunk trackChunk, NoteDetectionSettings noteDetectionSettings, Func<NotesSplitter, IEnumerable<Note>, IEnumerable<Note>> splitOperation)
        {
            using (var notesManager = trackChunk.ManageNotes(noteDetectionSettings))
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
