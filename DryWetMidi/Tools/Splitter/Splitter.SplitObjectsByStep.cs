using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class Splitter
    {
        #region Methods

        public static void SplitObjectsByStep(
            this TrackChunk trackChunk,
            ObjectType objectType,
            ITimeSpan step,
            TempoMap tempoMap,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(step), step);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            SplitTrackChunkObjects(
                trackChunk,
                objectType,
                objectDetectionSettings,
                objects => SplitObjectsByStep(objects, step, tempoMap));
        }

        public static void SplitObjectsByStep(
            this IEnumerable<TrackChunk> trackChunks,
            ObjectType objectType,
            ITimeSpan step,
            TempoMap tempoMap,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(step), step);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.SplitObjectsByStep(objectType, step, tempoMap, objectDetectionSettings);
            }
        }

        public static void SplitObjectsByStep(
            this MidiFile midiFile,
            ObjectType objectType,
            ITimeSpan step,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(step), step);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().SplitObjectsByStep(objectType, step, tempoMap, objectDetectionSettings);
        }

        public static IEnumerable<ILengthedObject> SplitObjectsByStep(this IEnumerable<ILengthedObject> objects, ITimeSpan step, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(step), step);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var obj in objects)
            {
                if (obj == null)
                {
                    yield return null;
                    continue;
                }

                if (obj.Length == 0)
                {
                    yield return (ILengthedObject)obj.Clone();
                    continue;
                }

                var startTime = obj.Time;
                var endTime = startTime + obj.Length;

                var time = startTime;
                var tail = (ILengthedObject)obj.Clone();

                while (time < endTime && tail != null)
                {
                    var convertedStep = LengthConverter.ConvertFrom(step, time, tempoMap);
                    if (convertedStep == 0)
                        throw new InvalidOperationException("Step is too small.");

                    time += convertedStep;

                    var parts = tail.Split(time);
                    yield return parts.LeftPart;

                    tail = parts.RightPart;
                }
            }
        }

        #endregion
    }
}
