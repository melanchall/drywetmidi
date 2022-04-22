using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tools
{
    public static class QuantizerUtilities
    {
        #region Methods

        public static void QuantizeObjects(
            this TrackChunk trackChunk,
            ObjectType objectType,
            IGrid grid,
            TempoMap tempoMap,
            QuantizerSettings quantizerSettings = null,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            using (var objectsManager = new TimedObjectsManager(trackChunk.Events, objectType, objectDetectionSettings ?? new ObjectDetectionSettings()))
            {
                new Quantizer().Quantize(objectsManager.Objects, grid, tempoMap, quantizerSettings);
            }
        }

        public static void QuantizeObjects(
            this IEnumerable<TrackChunk> trackChunks,
            ObjectType objectType,
            IGrid grid,
            TempoMap tempoMap,
            QuantizerSettings quantizerSettings = null,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.QuantizeObjects(objectType, grid, tempoMap, quantizerSettings, objectDetectionSettings);
            }
        }

        public static void QuantizeObjects(
            this MidiFile midiFile,
            ObjectType objectType,
            IGrid grid,
            QuantizerSettings quantizerSettings = null,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(grid), grid);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().QuantizeObjects(objectType, grid, tempoMap, quantizerSettings, objectDetectionSettings);
        }

        #endregion
    }
}
