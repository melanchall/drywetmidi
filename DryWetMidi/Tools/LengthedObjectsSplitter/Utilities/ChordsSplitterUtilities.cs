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
    /// Provides methods for splitting chords.
    /// </summary>
    public static class ChordsSplitterUtilities
    {
        #region Methods

        /// <summary>
        /// Splits chords contained in the specified <see cref="TrackChunk"/> by the specified step so
        /// every chord will be split at points equally distanced from each other starting from
        /// the chord's start time.
        /// </summary>
        /// <remarks>
        /// Chords with zero length and chords with length smaller than <paramref name="step"/>
        /// will not be split.
        /// </remarks>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to split chords in.</param>
        /// <param name="step">Step to split chords by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <param name="chordDetectionSettings">Settings accoridng to which chords should be detected and built.</param>
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
        public static void SplitChordsByStep(this TrackChunk trackChunk, ITimeSpan step, TempoMap tempoMap, ChordDetectionSettings chordDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(step), step);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            SplitTrackChunkChords(trackChunk, chordDetectionSettings, (splitter, chords) => splitter.SplitByStep(chords, step, tempoMap));
        }

        /// <summary>
        /// Splits chords contained in the specified collection of <see cref="TrackChunk"/> by the
        /// specified step so every chord will be split at points equally distanced from each
        /// other starting from the chord's start time.
        /// </summary>
        /// <remarks>
        /// Chords with zero length and chords with length smaller than <paramref name="step"/>
        /// will not be split.
        /// </remarks>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to split chords in.</param>
        /// <param name="step">Step to split chords by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <param name="chordDetectionSettings">Settings accoridng to which chords should be detected and built.</param>
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
        public static void SplitChordsByStep(this IEnumerable<TrackChunk> trackChunks, ITimeSpan step, TempoMap tempoMap, ChordDetectionSettings chordDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(step), step);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.SplitChordsByStep(step, tempoMap, chordDetectionSettings);
            }
        }

        /// <summary>
        /// Splits chords contained in the specified <see cref="MidiFile"/> by the specified
        /// step so every chord will be split at points equally distanced from each other
        /// starting from the chord's start time.
        /// </summary>
        /// <remarks>
        /// Chords with zero length and chords with length smaller than <paramref name="step"/>
        /// will not be split.
        /// </remarks>
        /// <param name="midiFile"><see cref="MidiFile"/> to split chords in.</param>
        /// <param name="step">Step to split chords by.</param>
        /// <param name="chordDetectionSettings">Settings accoridng to which chords should be detected and built.</param>
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
        public static void SplitChordsByStep(this MidiFile midiFile, ITimeSpan step, ChordDetectionSettings chordDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(step), step);

            var tempoMap = midiFile.GetTempoMap();
            midiFile.GetTrackChunks().SplitChordsByStep(step, tempoMap, chordDetectionSettings);
        }

        /// <summary>
        /// Splits chords contained in the specified <see cref="TrackChunk"/> into the specified number
        /// of parts of the equal length.
        /// </summary>
        /// <remarks>
        /// If a chord has zero length, it will be split into the specified number of parts of zero length.
        /// </remarks>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to split chords in.</param>
        /// <param name="partsNumber">The number of parts to split chords into.</param>
        /// <param name="lengthType">Type of a part's length.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <param name="chordDetectionSettings">Settings accoridng to which chords should be detected and built.</param>
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
        public static void SplitChordsByPartsNumber(this TrackChunk trackChunk, int partsNumber, TimeSpanType lengthType, TempoMap tempoMap, ChordDetectionSettings chordDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            SplitTrackChunkChords(trackChunk, chordDetectionSettings, (splitter, chords) => splitter.SplitByPartsNumber(chords, partsNumber, lengthType, tempoMap));
        }

        /// <summary>
        /// Splits chords contained in the specified collection of <see cref="TrackChunk"/> into the
        /// specified number of parts of the equal length.
        /// </summary>
        /// <remarks>
        /// If a chord has zero length, it will be split into the specified number of parts of zero length.
        /// </remarks>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to split chords in.</param>
        /// <param name="partsNumber">The number of parts to split chords into.</param>
        /// <param name="lengthType">Type of a part's length.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <param name="chordDetectionSettings">Settings accoridng to which chords should be detected and built.</param>
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
        public static void SplitChordsByPartsNumber(this IEnumerable<TrackChunk> trackChunks, int partsNumber, TimeSpanType lengthType, TempoMap tempoMap, ChordDetectionSettings chordDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.SplitChordsByPartsNumber(partsNumber, lengthType, tempoMap, chordDetectionSettings);
            }
        }

        /// <summary>
        /// Splits chords contained in the specified <see cref="MidiFile"/> into the specified number of
        /// parts of the equal length.
        /// </summary>
        /// <remarks>
        /// If a chord has zero length, it will be split into the specified number of parts of zero length.
        /// </remarks>
        /// <param name="midiFile"><see cref="MidiFile"/> to split chords in.</param>
        /// <param name="partsNumber">The number of parts to split chords into.</param>
        /// <param name="lengthType">Type of a part's length.</param>
        /// <param name="chordDetectionSettings">Settings accoridng to which chords should be detected and built.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="partsNumber"/> is zero or negative.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="lengthType"/> specified an invalid value.</exception>
        public static void SplitChordsByPartsNumber(this MidiFile midiFile, int partsNumber, TimeSpanType lengthType, ChordDetectionSettings chordDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().SplitChordsByPartsNumber(partsNumber, lengthType, tempoMap, chordDetectionSettings);
        }

        /// <summary>
        /// Splits chords contained in the specified <see cref="TrackChunk"/> by the specified grid.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to split chords in.</param>
        /// <param name="grid">Grid to split chords by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <param name="chordDetectionSettings">Settings accoridng to which chords should be detected and built.</param>
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
        public static void SplitChordsByGrid(this TrackChunk trackChunk, IGrid grid, TempoMap tempoMap, ChordDetectionSettings chordDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            SplitTrackChunkChords(trackChunk, chordDetectionSettings, (splitter, chords) => splitter.SplitByGrid(chords, grid, tempoMap));
        }

        /// <summary>
        /// Splits chords contained in the specified collection of <see cref="TrackChunk"/> by the
        /// specified grid.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to split chords in.</param>
        /// <param name="grid">Grid to split chords by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <param name="chordDetectionSettings">Settings accoridng to which chords should be detected and built.</param>
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
        public static void SplitChordsByGrid(this IEnumerable<TrackChunk> trackChunks, IGrid grid, TempoMap tempoMap, ChordDetectionSettings chordDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.SplitChordsByGrid(grid, tempoMap, chordDetectionSettings);
            }
        }

        /// <summary>
        /// Splits chords contained in the specified <see cref="MidiFile"/> by the specified grid.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to split chords in.</param>
        /// <param name="grid">Grid to split chords by.</param>
        /// <param name="settings">Settings accoridng to which chords should be detected and built.</param>
        /// <exception cref="ArgumentNullException"><paramref name="grid"/> is <c>null</c>.</exception>
        public static void SplitChordsByGrid(this MidiFile midiFile, IGrid grid, ChordDetectionSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(grid), grid);

            var tempoMap = midiFile.GetTempoMap();
            midiFile.GetTrackChunks().SplitChordsByGrid(grid, tempoMap, settings);
        }

        /// <summary>
        /// Splits chords contained in the specified <see cref="TrackChunk"/> at the specified distance
        /// from a chord's start or end.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to split chords in.</param>
        /// <param name="distance">Distance to split chords at.</param>
        /// <param name="from">Point of a chord <paramref name="distance"/> should be measured from.</param>
        /// <param name="tempoMap">Tempo map used for distances calculations.</param>
        /// <param name="chordDetectionSettings">Settings accoridng to which chords should be detected and built.</param>
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
        public static void SplitChordsAtDistance(this TrackChunk trackChunk, ITimeSpan distance, LengthedObjectTarget from, TempoMap tempoMap, ChordDetectionSettings chordDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(distance), distance);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            SplitTrackChunkChords(trackChunk, chordDetectionSettings, (splitter, chords) => splitter.SplitAtDistance(chords, distance, from, tempoMap));
        }

        /// <summary>
        /// Splits chords contained in the specified collection of <see cref="TrackChunk"/> at the specified
        /// distance from a chord's start or end.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to split chords in.</param>
        /// <param name="distance">Distance to split chords at.</param>
        /// <param name="from">Point of a chord <paramref name="distance"/> should be measured from.</param>
        /// <param name="tempoMap">Tempo map used for distances calculations.</param>
        /// <param name="chordDetectionSettings">Settings accoridng to which chords should be detected and built.</param>
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
        public static void SplitChordsAtDistance(this IEnumerable<TrackChunk> trackChunks, ITimeSpan distance, LengthedObjectTarget from, TempoMap tempoMap, ChordDetectionSettings chordDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(distance), distance);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.SplitChordsAtDistance(distance, from, tempoMap, chordDetectionSettings);
            }
        }

        /// <summary>
        /// Splits chords contained in the specified <see cref="MidiFile"/> at the specified distance
        /// from a chord's start or end.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to split chords in.</param>
        /// <param name="distance">Distance to split chords at.</param>
        /// <param name="from">Point of a chord <paramref name="distance"/> should be measured from.</param>
        /// <param name="chordDetectionSettings">Settings accoridng to which chords should be detected and built.</param>
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
        public static void SplitChordsAtDistance(this MidiFile midiFile, ITimeSpan distance, LengthedObjectTarget from, ChordDetectionSettings chordDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(distance), distance);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);

            var tempoMap = midiFile.GetTempoMap();
            midiFile.GetTrackChunks().SplitChordsAtDistance(distance, from, tempoMap, chordDetectionSettings);
        }

        /// <summary>
        /// Splits chords contained in the specified <see cref="TrackChunk"/> by the specified ratio of a
        /// chord's length measuring it from the chord's start or end. For example, 0.5 means splitting
        /// at the center of a chord.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to split chords in.</param>
        /// <param name="ratio">Ratio of a chord's length to split by. Valid values are from 0 to 1.</param>
        /// <param name="lengthType">The type a chord's length should be processed according to.</param>
        /// <param name="from">Point of a chord distance should be measured from.</param>
        /// <param name="tempoMap">Tempo map used for distances calculations.</param>
        /// <param name="chordDetectionSettings">Settings accoridng to which chords should be detected and built.</param>
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
        public static void SplitChordsAtDistance(this TrackChunk trackChunk, double ratio, TimeSpanType lengthType, LengthedObjectTarget from, TempoMap tempoMap, ChordDetectionSettings chordDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsOutOfRange(nameof(ratio),
                                         ratio,
                                         LengthedObjectsSplitter<Chord>.ZeroRatio,
                                         LengthedObjectsSplitter<Chord>.FullLengthRatio,
                                         $"Ratio is out of [{LengthedObjectsSplitter<Chord>.ZeroRatio}; {LengthedObjectsSplitter<Chord>.FullLengthRatio}] range.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            SplitTrackChunkChords(trackChunk, chordDetectionSettings, (splitter, chords) => splitter.SplitAtDistance(chords, ratio, lengthType, from, tempoMap));
        }

        /// <summary>
        /// Splits chords contained in the specified collection of <see cref="TrackChunk"/> by the
        /// specified ratio of a chord's length measuring it from the chord's start or end.
        /// For example, 0.5 means splitting at the center of a chord.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to split chords in.</param>
        /// <param name="ratio">Ratio of a chord's length to split by. Valid values are from 0 to 1.</param>
        /// <param name="lengthType">The type a chord's length should be processed according to.</param>
        /// <param name="from">Point of a chord distance should be measured from.</param>
        /// <param name="tempoMap">Tempo map used for distances calculations.</param>
        /// <param name="chordDetectionSettings">Settings accoridng to which chords should be detected and built.</param>
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
        public static void SplitChordsAtDistance(this IEnumerable<TrackChunk> trackChunks, double ratio, TimeSpanType lengthType, LengthedObjectTarget from, TempoMap tempoMap, ChordDetectionSettings chordDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsOutOfRange(nameof(ratio),
                                         ratio,
                                         LengthedObjectsSplitter<Chord>.ZeroRatio,
                                         LengthedObjectsSplitter<Chord>.FullLengthRatio,
                                         $"Ratio is out of [{LengthedObjectsSplitter<Chord>.ZeroRatio}; {LengthedObjectsSplitter<Chord>.FullLengthRatio}] range.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.SplitChordsAtDistance(ratio, lengthType, from, tempoMap, chordDetectionSettings);
            }
        }

        /// <summary>
        /// Splits chords contained in the specified <see cref="MidiFile"/> by the specified ratio of a
        /// chord's length measuring it from the chord's start or end. For example, 0.5 means splitting
        /// at the center of a chord.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to split chords in.</param>
        /// <param name="ratio">Ratio of a chord's length to split by. Valid values are from 0 to 1.</param>
        /// <param name="lengthType">The type a chord's length should be processed according to.</param>
        /// <param name="from">Point of a chord distance should be measured from.</param>
        /// <param name="chordDetectionSettings">Settings accoridng to which chords should be detected and built.</param>
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
        public static void SplitChordsAtDistance(this MidiFile midiFile, double ratio, TimeSpanType lengthType, LengthedObjectTarget from, ChordDetectionSettings chordDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsOutOfRange(nameof(ratio),
                                         ratio,
                                         LengthedObjectsSplitter<Chord>.ZeroRatio,
                                         LengthedObjectsSplitter<Chord>.FullLengthRatio,
                                         $"Ratio is out of [{LengthedObjectsSplitter<Chord>.ZeroRatio}; {LengthedObjectsSplitter<Chord>.FullLengthRatio}] range.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);

            var tempoMap = midiFile.GetTempoMap();
            midiFile.GetTrackChunks().SplitChordsAtDistance(ratio, lengthType, from, tempoMap, chordDetectionSettings);
        }

        private static void SplitTrackChunkChords(TrackChunk trackChunk, ChordDetectionSettings chordDetectionSettings, Func<ChordsSplitter, IEnumerable<Chord>, IEnumerable<Chord>> splitOperation)
        {
            using (var chordsManager = trackChunk.ManageChords(chordDetectionSettings))
            {
                var chords = chordsManager.Chords;

                var chordsSplitter = new ChordsSplitter();
                var newChords = splitOperation(chordsSplitter, chords).ToList();

                chords.Clear();
                chords.Add(newChords);
            }
        }

        #endregion
    }
}
