using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class Splitter
    {
        #region Methods

        public static void SplitObjectsByPartsNumber(
            this TrackChunk trackChunk,
            ObjectType objectType,
            int partsNumber,
            TimeSpanType lengthType,
            TempoMap tempoMap,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            SplitTrackChunkObjects(
                trackChunk,
                objectType,
                objectDetectionSettings,
                objects => SplitObjectsByPartsNumber(objects, partsNumber, lengthType, tempoMap));
        }

        public static void SplitObjectsByPartsNumber(
            this IEnumerable<TrackChunk> trackChunks,
            ObjectType objectType,
            int partsNumber,
            TimeSpanType lengthType,
            TempoMap tempoMap,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.SplitObjectsByPartsNumber(objectType, partsNumber, lengthType, tempoMap, objectDetectionSettings);
            }
        }

        public static void SplitObjectsByPartsNumber(
            this MidiFile midiFile,
            ObjectType objectType,
            int partsNumber,
            TimeSpanType lengthType,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().SplitObjectsByPartsNumber(objectType, partsNumber, lengthType, tempoMap, objectDetectionSettings);
        }

        public static IEnumerable<ITimedObject> SplitObjectsByPartsNumber(this IEnumerable<ITimedObject> objects, int partsNumber, TimeSpanType lengthType, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
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

                if (partsNumber == 1)
                {
                    yield return (ILengthedObject)obj.Clone();
                    continue;
                }

                if (lengthedObject.Length == 0)
                {
                    foreach (var i in Enumerable.Range(0, partsNumber))
                    {
                        yield return (ILengthedObject)obj.Clone();
                    }

                    continue;
                }

                var time = obj.Time;
                var tail = (ILengthedObject)obj.Clone();
                var partsYielded = 0;

                for (int partsRemaining = partsNumber; partsRemaining > 1 && tail != null; partsRemaining--)
                {
                    var length = tail.LengthAs(lengthType, tempoMap);
                    var partLength = length.Divide(partsRemaining);

                    time += LengthConverter.ConvertFrom(partLength, time, tempoMap);

                    var parts = tail.Split(time);
                    yield return parts.LeftPart ?? GetZeroLengthObjectAtStart(tail, time);
                    partsYielded++;

                    tail = parts.RightPart;
                }

                if (tail != null)
                {
                    yield return tail;
                    partsYielded++;
                }

                var unyieldedPartsCount = partsNumber - partsYielded;
                if (unyieldedPartsCount > 0)
                {
                    var part = GetZeroLengthObjectAtEnd(lengthedObject, obj.Time + lengthedObject.Length);

                    for (var i = 0; i < unyieldedPartsCount; i++)
                    {
                        yield return (ILengthedObject)part.Clone();
                    }
                }
            }
        }

        #endregion
    }
}
