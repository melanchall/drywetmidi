using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides methods to quantize chords time.
    /// </summary>
    public static class ChordsQuantizerUtilities
    {
        #region Methods

        /// <summary>
        /// Quantizes chords contained in the specified <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to quantize chords in.</param>
        /// <param name="grid">Grid to quantize objects by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to quantize by.</param>
        /// <param name="settings">Settings according to which chords should be quantized.</param>
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
        /// <exception cref="InvalidOperationException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description>Chord is going to be moved beyond zero.</description>
        /// </item>
        /// <item>
        /// <description>Chord's end is going to be moved beyond the chord's fixed end.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void QuantizeChords(this TrackChunk trackChunk, IGrid grid, TempoMap tempoMap, ChordsQuantizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            using (var chordsManager = trackChunk.ManageChords(settings?.ChordDetectionSettings))
            {
                new ChordsQuantizer().Quantize(chordsManager.Chords, grid, tempoMap, settings);
            }
        }

        /// <summary>
        /// Quantizes chords contained in the specified collection of <see cref="TrackChunk"/>.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to quantize chords in.</param>
        /// <param name="grid">Grid to quantize objects by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to quantize by.</param>
        /// <param name="settings">Settings according to which chords should be quantized.</param>
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
        /// <exception cref="InvalidOperationException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description>Chord is going to be moved beyond zero.</description>
        /// </item>
        /// <item>
        /// <description>Chord's end is going to be moved beyond the chord's fixed end.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void QuantizeChords(this IEnumerable<TrackChunk> trackChunks, IGrid grid, TempoMap tempoMap, ChordsQuantizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.QuantizeChords(grid, tempoMap, settings);
            }
        }

        /// <summary>
        /// Quantizes chords contained in the specified <see cref="MidiFile"/>.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to quantize chords in.</param>
        /// <param name="grid">Grid to quantize objects by.</param>
        /// <param name="settings">Settings according to which chords should be quantized.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiFile"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="grid"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description>Chord is going to be moved beyond zero.</description>
        /// </item>
        /// <item>
        /// <description>Chord's end is going to be moved beyond the chord's fixed end.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void QuantizeChords(this MidiFile midiFile, IGrid grid, ChordsQuantizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(grid), grid);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().QuantizeChords(grid, tempoMap, settings);
        }

        #endregion
    }
}
