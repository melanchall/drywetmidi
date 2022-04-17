using System;
using System.Collections.Generic;
using System.ComponentModel;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides methods for splitting notes.
    /// </summary>
    [Obsolete("OBS12")]
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
        [Obsolete("OBS12")]
        public static void SplitNotesByStep(this TrackChunk trackChunk, ITimeSpan step, TempoMap tempoMap, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(step), step);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            trackChunk.SplitObjectsByStep(
                ObjectType.Note,
                step,
                tempoMap,
                new ObjectDetectionSettings
                {
                    NoteDetectionSettings = noteDetectionSettings
                });
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
        [Obsolete("OBS12")]
        public static void SplitNotesByStep(this IEnumerable<TrackChunk> trackChunks, ITimeSpan step, TempoMap tempoMap, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(step), step);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            trackChunks.SplitObjectsByStep(
                ObjectType.Note,
                step,
                tempoMap,
                new ObjectDetectionSettings
                {
                    NoteDetectionSettings = noteDetectionSettings
                });
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
        [Obsolete("OBS12")]
        public static void SplitNotesByStep(this MidiFile midiFile, ITimeSpan step, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(step), step);

            midiFile.SplitObjectsByStep(
                ObjectType.Note,
                step,
                new ObjectDetectionSettings
                {
                    NoteDetectionSettings = noteDetectionSettings
                });
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
        [Obsolete("OBS12")]
        public static void SplitNotesByPartsNumber(this TrackChunk trackChunk, int partsNumber, TimeSpanType lengthType, TempoMap tempoMap, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            trackChunk.SplitObjectsByPartsNumber(
                ObjectType.Note,
                partsNumber,
                lengthType,
                tempoMap,
                new ObjectDetectionSettings
                {
                    NoteDetectionSettings = noteDetectionSettings
                });
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
        [Obsolete("OBS12")]
        public static void SplitNotesByPartsNumber(this IEnumerable<TrackChunk> trackChunks, int partsNumber, TimeSpanType lengthType, TempoMap tempoMap, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            trackChunks.SplitObjectsByPartsNumber(
                ObjectType.Note,
                partsNumber,
                lengthType,
                tempoMap,
                new ObjectDetectionSettings
                {
                    NoteDetectionSettings = noteDetectionSettings
                });
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
        [Obsolete("OBS12")]
        public static void SplitNotesByPartsNumber(this MidiFile midiFile, int partsNumber, TimeSpanType lengthType, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);

            midiFile.SplitObjectsByPartsNumber(
                ObjectType.Note,
                partsNumber,
                lengthType,
                new ObjectDetectionSettings
                {
                    NoteDetectionSettings = noteDetectionSettings
                });
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
        [Obsolete("OBS12")]
        public static void SplitNotesByGrid(this TrackChunk trackChunk, IGrid grid, TempoMap tempoMap, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            trackChunk.SplitObjectsByGrid(
                ObjectType.Note,
                grid,
                tempoMap,
                new ObjectDetectionSettings
                {
                    NoteDetectionSettings = noteDetectionSettings
                });
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
        [Obsolete("OBS12")]
        public static void SplitNotesByGrid(this IEnumerable<TrackChunk> trackChunks, IGrid grid, TempoMap tempoMap, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            trackChunks.SplitObjectsByGrid(
                ObjectType.Note,
                grid,
                tempoMap,
                new ObjectDetectionSettings
                {
                    NoteDetectionSettings = noteDetectionSettings
                });
        }

        /// <summary>
        /// Splits notes contained in the specified <see cref="MidiFile"/> by the specified grid.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to split notes in.</param>
        /// <param name="grid">Grid to split notes by.</param>
        /// <param name="noteDetectionSettings">Settings accoridng to which notes should be detected and built.</param>
        /// <exception cref="ArgumentNullException"><paramref name="grid"/> is <c>null</c>.</exception>
        [Obsolete("OBS12")]
        public static void SplitNotesByGrid(this MidiFile midiFile, IGrid grid, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(grid), grid);

            midiFile.SplitObjectsByGrid(
                ObjectType.Note,
                grid,
                new ObjectDetectionSettings
                {
                    NoteDetectionSettings = noteDetectionSettings
                });
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
        [Obsolete("OBS12")]
        public static void SplitNotesAtDistance(this TrackChunk trackChunk, ITimeSpan distance, LengthedObjectTarget from, TempoMap tempoMap, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(distance), distance);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            trackChunk.SplitObjectsAtDistance(
                ObjectType.Note,
                distance,
                from,
                tempoMap,
                new ObjectDetectionSettings
                {
                    NoteDetectionSettings = noteDetectionSettings
                });
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
        [Obsolete("OBS12")]
        public static void SplitNotesAtDistance(this IEnumerable<TrackChunk> trackChunks, ITimeSpan distance, LengthedObjectTarget from, TempoMap tempoMap, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(distance), distance);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            trackChunks.SplitObjectsAtDistance(
                ObjectType.Note,
                distance,
                from,
                tempoMap,
                new ObjectDetectionSettings
                {
                    NoteDetectionSettings = noteDetectionSettings
                });
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
        [Obsolete("OBS12")]
        public static void SplitNotesAtDistance(this MidiFile midiFile, ITimeSpan distance, LengthedObjectTarget from, NoteDetectionSettings noteDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(distance), distance);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);

            midiFile.SplitObjectsAtDistance(
                ObjectType.Note,
                distance,
                from,
                new ObjectDetectionSettings
                {
                    NoteDetectionSettings = noteDetectionSettings
                });
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
        [Obsolete("OBS12")]
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

            trackChunk.SplitObjectsAtDistance(
                ObjectType.Note,
                ratio,
                lengthType,
                from,
                tempoMap,
                new ObjectDetectionSettings
                {
                    NoteDetectionSettings = noteDetectionSettings
                });
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
        [Obsolete("OBS12")]
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

            trackChunks.SplitObjectsAtDistance(
                ObjectType.Note,
                ratio,
                lengthType,
                from,
                tempoMap,
                new ObjectDetectionSettings
                {
                    NoteDetectionSettings = noteDetectionSettings
                });
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
        [Obsolete("OBS12")]
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

            midiFile.SplitObjectsAtDistance(
                ObjectType.Note,
                ratio,
                lengthType,
                from,
                new ObjectDetectionSettings
                {
                    NoteDetectionSettings = noteDetectionSettings
                });
        }

        #endregion
    }
}
