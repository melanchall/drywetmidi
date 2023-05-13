using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides utilities to quantize objects of different types. More info in the
    /// <see href="xref:a_quantizer">Quantizer</see> article.
    /// </summary>
    /// <seealso cref="Quantizer"/>
    public static class QuantizerUtilities
    {
        #region Methods

        /// <summary>
        /// Quantizes objects within a <see cref="TrackChunk"/> using the specified grid and default quantizer.
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to quantize objects within.</param>
        /// <param name="objectType">The type of objects to quantize.</param>
        /// <param name="grid">Grid to use for quantization.</param>
        /// <param name="tempoMap">Tempo map used to perform time and length conversions.</param>
        /// <param name="quantizerSettings">Settings according to which objects should be quantized.</param>
        /// <param name="objectDetectionSettings">Settings according to which objects should be
        /// detected and built.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
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
        public static void QuantizeObjects(
            this TrackChunk trackChunk,
            ObjectType objectType,
            IGrid grid,
            TempoMap tempoMap,
            QuantizingSettings quantizerSettings = null,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            trackChunk.QuantizeObjects(
                new Quantizer(),
                objectType,
                grid,
                tempoMap,
                quantizerSettings,
                objectDetectionSettings);
        }

        /// <summary>
        /// Quantizes objects within a <see cref="TrackChunk"/> using the specified grid and custom quantizer
        /// (see <see href="xref:a_quantizer#custom-quantizing">Quantizer: Custom quantizing</see> article).
        /// </summary>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to quantize objects within.</param>
        /// <param name="quantizer">Quantizer to quantize objects with.</param>
        /// <param name="objectType">The type of objects to quantize.</param>
        /// <param name="grid">Grid to use for quantization.</param>
        /// <param name="tempoMap">Tempo map used to perform time and length conversions.</param>
        /// <param name="quantizerSettings">Settings according to which objects should be quantized.</param>
        /// <param name="objectDetectionSettings">Settings according to which objects should be
        /// detected and built.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunk"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="quantizer"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="grid"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void QuantizeObjects(
            this TrackChunk trackChunk,
            Quantizer quantizer,
            ObjectType objectType,
            IGrid grid,
            TempoMap tempoMap,
            QuantizingSettings quantizerSettings = null,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(quantizer), quantizer);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            using (var objectsManager = new TimedObjectsManager(trackChunk.Events, objectType, objectDetectionSettings ?? new ObjectDetectionSettings()))
            {
                quantizer.Quantize(objectsManager.Objects, grid, tempoMap, quantizerSettings);
            }
        }

        /// <summary>
        /// Quantizes objects within a collection of <see cref="TrackChunk"/> using the specified grid and default quantizer.
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to quantize objects within.</param>
        /// <param name="objectType">The type of objects to quantize.</param>
        /// <param name="grid">Grid to use for quantization.</param>
        /// <param name="tempoMap">Tempo map used to perform time and length conversions.</param>
        /// <param name="quantizerSettings">Settings according to which objects should be quantized.</param>
        /// <param name="objectDetectionSettings">Settings according to which objects should be
        /// detected and built.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
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
        public static void QuantizeObjects(
            this IEnumerable<TrackChunk> trackChunks,
            ObjectType objectType,
            IGrid grid,
            TempoMap tempoMap,
            QuantizingSettings quantizerSettings = null,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            trackChunks.QuantizeObjects(
                new Quantizer(),
                objectType,
                grid,
                tempoMap,
                quantizerSettings,
                objectDetectionSettings);
        }

        /// <summary>
        /// Quantizes objects within a collection of <see cref="TrackChunk"/> using the specified grid and custom quantizer
        /// (see <see href="xref:a_quantizer#custom-quantizing">Quantizer: Custom quantizing</see> article).
        /// </summary>
        /// <param name="trackChunks">Collection of <see cref="TrackChunk"/> to quantize objects within.</param>
        /// <param name="quantizer">Quantizer to quantize objects with.</param>
        /// <param name="objectType">The type of objects to quantize.</param>
        /// <param name="grid">Grid to use for quantization.</param>
        /// <param name="tempoMap">Tempo map used to perform time and length conversions.</param>
        /// <param name="quantizerSettings">Settings according to which objects should be quantized.</param>
        /// <param name="objectDetectionSettings">Settings according to which objects should be
        /// detected and built.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="trackChunks"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="quantizer"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="grid"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void QuantizeObjects(
            this IEnumerable<TrackChunk> trackChunks,
            Quantizer quantizer,
            ObjectType objectType,
            IGrid grid,
            TempoMap tempoMap,
            QuantizingSettings quantizerSettings = null,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(quantizer), quantizer);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.QuantizeObjects(quantizer, objectType, grid, tempoMap, quantizerSettings, objectDetectionSettings);
            }
        }

        /// <summary>
        /// Quantizes objects within a <see cref="MidiFile"/> using the specified grid and default quantizer.
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to quantize objects within.</param>
        /// <param name="objectType">The type of objects to quantize.</param>
        /// <param name="grid">Grid to use for quantization.</param>
        /// <param name="quantizerSettings">Settings according to which objects should be quantized.</param>
        /// <param name="objectDetectionSettings">Settings according to which objects should be
        /// detected and built.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiFile"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="grid"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void QuantizeObjects(
            this MidiFile midiFile,
            ObjectType objectType,
            IGrid grid,
            QuantizingSettings quantizerSettings = null,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(grid), grid);

            midiFile.QuantizeObjects(
                new Quantizer(),
                objectType,
                grid,
                quantizerSettings,
                objectDetectionSettings);
        }

        /// <summary>
        /// Quantizes objects within a <see cref="MidiFile"/> using the specified grid and custom quantizer
        /// (see <see href="xref:a_quantizer#custom-quantizing">Quantizer: Custom quantizing</see> article).
        /// </summary>
        /// <param name="midiFile"><see cref="MidiFile"/> to quantize objects within.</param>
        /// <param name="quantizer">Quantizer to quantize objects with.</param>
        /// <param name="objectType">The type of objects to quantize.</param>
        /// <param name="grid">Grid to use for quantization.</param>
        /// <param name="quantizerSettings">Settings according to which objects should be quantized.</param>
        /// <param name="objectDetectionSettings">Settings according to which objects should be
        /// detected and built.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="midiFile"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="quantizer"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="grid"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static void QuantizeObjects(
            this MidiFile midiFile,
            Quantizer quantizer,
            ObjectType objectType,
            IGrid grid,
            QuantizingSettings quantizerSettings = null,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(quantizer), quantizer);
            ThrowIfArgument.IsNull(nameof(grid), grid);

            var tempoMap = midiFile.GetTempoMap();
            midiFile
                .GetTrackChunks()
                .QuantizeObjects(quantizer, objectType, grid, tempoMap, quantizerSettings, objectDetectionSettings);
        }

        #endregion
    }
}
