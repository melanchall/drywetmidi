using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides methods to randomize chords time.
    /// </summary>
    public static class ChordsRandomizerUtilities
    {
        #region Methods

        /// <summary>
        /// Randomizes chords contained in the specified <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to randomize chords in.</param>
        /// <param name="bounds">Bounds to randomize time within.</param>
        /// <param name="tempoMap">Tempo map used to calculate time bounds to randomize within.</param>
        /// <param name="settings">Settings according to which chords should be randomized.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="bounds"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void RandomizeChords(this TrackChunk trackChunk, IBounds bounds, TempoMap tempoMap, ChordsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(bounds), bounds);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            using (var chordsManager = trackChunk.ManageChords(settings?.ChordDetectionSettings))
            {
                new ChordsRandomizer().Randomize(chordsManager.Chords, bounds, tempoMap, settings);
            }
        }

        /// <summary>
        /// Randomizes chords contained in the specified collection of <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to randomize chords in.</param>
        /// <param name="bounds">Bounds to randomize time within.</param>
        /// <param name="tempoMap">Tempo map used to calculate time bounds to randomize within.</param>
        /// <param name="settings">Settings according to which chords should be randomized.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="bounds"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void RandomizeChords(this IEnumerable<TrackChunk> trackChunks, IBounds bounds, TempoMap tempoMap, ChordsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(bounds), bounds);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.RandomizeChords(bounds, tempoMap, settings);
            }
        }

        /// <summary>
        /// Randomizes chords contained in the specified <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to randomize chords in.</param>
        /// <param name="bounds">Bounds to randomize time within.</param>
        /// <param name="settings">Settings according to which chords should be randomized.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiFile"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="bounds"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void RandomizeChords(this MidiFile midiFile, IBounds bounds, ChordsRandomizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(bounds), bounds);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().RandomizeChords(bounds, tempoMap, settings);
        }

        #endregion
    }
}
