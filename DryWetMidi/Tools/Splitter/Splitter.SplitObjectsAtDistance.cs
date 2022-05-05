using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class Splitter
    {
        #region Constants

        private const double ZeroRatio = 0.0;
        private const double FullLengthRatio = 1.0;

        #endregion

        #region Methods

        public static void SplitObjectsAtDistance(
            this TrackChunk trackChunk,
            ObjectType objectType,
            ITimeSpan distance,
            LengthedObjectTarget from,
            TempoMap tempoMap,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(distance), distance);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            SplitTrackChunkObjects(
                trackChunk,
                objectType,
                objectDetectionSettings,
                objects => SplitObjectsAtDistance(objects, distance, from, tempoMap));
        }

        public static void SplitObjectsAtDistance(
            this IEnumerable<TrackChunk> trackChunks,
            ObjectType objectType,
            ITimeSpan distance,
            LengthedObjectTarget from,
            TempoMap tempoMap,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(distance), distance);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.SplitObjectsAtDistance(objectType, distance, from, tempoMap, objectDetectionSettings);
            }
        }

        public static void SplitObjectsAtDistance(
            this MidiFile midiFile,
            ObjectType objectType,
            ITimeSpan distance,
            LengthedObjectTarget from,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(distance), distance);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().SplitObjectsAtDistance(objectType, distance, from, tempoMap, objectDetectionSettings);
        }

        public static void SplitObjectsAtDistance(
            this TrackChunk trackChunk,
            ObjectType objectType,
            double ratio,
            TimeSpanType lengthType,
            LengthedObjectTarget from,
            TempoMap tempoMap,
            ObjectDetectionSettings objectDetectionSettings = null)
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

            SplitTrackChunkObjects(
                trackChunk,
                objectType,
                objectDetectionSettings,
                objects => SplitObjectsAtDistance(objects, ratio, lengthType, from, tempoMap));
        }

        public static void SplitObjectsAtDistance(
            this IEnumerable<TrackChunk> trackChunks,
            ObjectType objectType,
            double ratio,
            TimeSpanType lengthType,
            LengthedObjectTarget from,
            TempoMap tempoMap,
            ObjectDetectionSettings objectDetectionSettings = null)
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
                trackChunk.SplitObjectsAtDistance(objectType, ratio, lengthType, from, tempoMap, objectDetectionSettings);
            }
        }

        public static void SplitObjectsAtDistance(
            this MidiFile midiFile,
            ObjectType objectType,
            double ratio,
            TimeSpanType lengthType,
            LengthedObjectTarget from,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsOutOfRange(nameof(ratio),
                                         ratio,
                                         ZeroRatio,
                                         FullLengthRatio,
                                         $"Ratio is out of [{ZeroRatio}; {FullLengthRatio}] range.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().SplitObjectsAtDistance(objectType, ratio, lengthType, from, tempoMap, objectDetectionSettings);
        }

        public static IEnumerable<ITimedObject> SplitObjectsAtDistance(this IEnumerable<ITimedObject> objects, ITimeSpan distance, LengthedObjectTarget from, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(distance), distance);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var obj in objects)
            {
                if (obj == null)
                {
                    yield return null;
                    continue;
                }

                var lengthedObject = obj as ILengthedObject;
                if (lengthedObject == null)
                {
                    yield return obj;
                    continue;
                }

                var parts = SplitObjectAtDistance(lengthedObject, distance, from, tempoMap);

                if (parts.LeftPart != null)
                    yield return parts.LeftPart;

                if (parts.RightPart != null)
                    yield return parts.RightPart;
            }
        }

        public static IEnumerable<ITimedObject> SplitObjectsAtDistance(this IEnumerable<ITimedObject> objects, double ratio, TimeSpanType lengthType, LengthedObjectTarget from, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsOutOfRange(nameof(ratio),
                                         ratio,
                                         ZeroRatio,
                                         FullLengthRatio,
                                         $"Ratio is out of [{ZeroRatio}; {FullLengthRatio}] range.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var obj in objects)
            {
                if (obj == null)
                {
                    yield return null;
                    continue;
                }

                var lengthedObject = obj as ILengthedObject;
                if (lengthedObject == null)
                {
                    yield return obj;
                    continue;
                }

                var distance = lengthedObject.LengthAs(lengthType, tempoMap).Multiply(ratio);
                var parts = SplitObjectAtDistance(lengthedObject, distance, from, tempoMap);

                if (parts.LeftPart != null)
                    yield return parts.LeftPart;

                if (parts.RightPart != null)
                    yield return parts.RightPart;
            }
        }

        private static SplitLengthedObject SplitObjectAtDistance(ILengthedObject obj, ITimeSpan distance, LengthedObjectTarget from, TempoMap tempoMap)
        {
            var time = from == LengthedObjectTarget.Start
                ? ((MidiTimeSpan)obj.Time).Add(distance, TimeSpanMode.TimeLength)
                : ((MidiTimeSpan)(obj.Time + obj.Length)).Subtract(distance, TimeSpanMode.TimeLength);

            return obj.Split(TimeConverter.ConvertFrom(time, tempoMap));
        }

        #endregion
    }
}
