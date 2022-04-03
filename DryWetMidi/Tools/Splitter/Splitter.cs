using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    public static class Splitter
    {
        #region Constants

        private const double ZeroRatio = 0.0;
        private const double FullLengthRatio = 1.0;

        #endregion

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

        public static void SplitObjectsByGrid(
            this TrackChunk trackChunk,
            ObjectType objectType,
            IGrid grid,
            TempoMap tempoMap,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            SplitTrackChunkObjects(
                trackChunk,
                objectType,
                objectDetectionSettings,
                objects => SplitObjectsByGrid(objects, grid, tempoMap));
        }

        public static void SplitObjectsByGrid(
            this IEnumerable<TrackChunk> trackChunks,
            ObjectType objectType,
            IGrid grid,
            TempoMap tempoMap,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.SplitObjectsByGrid(objectType, grid, tempoMap, objectDetectionSettings);
            }
        }

        public static void SplitObjectsByGrid(
            this MidiFile midiFile,
            ObjectType objectType,
            IGrid grid,
            ObjectDetectionSettings objectDetectionSettings = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(grid), grid);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().SplitObjectsByGrid(objectType, grid, tempoMap, objectDetectionSettings);
        }

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
                                         LengthedObjectsSplitter<Note>.ZeroRatio,
                                         LengthedObjectsSplitter<Note>.FullLengthRatio,
                                         $"Ratio is out of [{LengthedObjectsSplitter<Note>.ZeroRatio}; {LengthedObjectsSplitter<Note>.FullLengthRatio}] range.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().SplitObjectsAtDistance(objectType, ratio, lengthType, from, tempoMap, objectDetectionSettings);
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

        public static IEnumerable<ILengthedObject> SplitObjectsByPartsNumber(this IEnumerable<ILengthedObject> objects, int partsNumber, TimeSpanType lengthType, TempoMap tempoMap)
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

                if (partsNumber == 1)
                {
                    yield return (ILengthedObject)obj.Clone();
                    continue;
                }

                if (obj.Length == 0)
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
                    var part = GetZeroLengthObjectAtEnd(obj, obj.Time + obj.Length);

                    for (var i = 0; i < unyieldedPartsCount; i++)
                    {
                        yield return (ILengthedObject)part.Clone();
                    }
                }
            }
        }

        public static IEnumerable<ILengthedObject> SplitObjectsByGrid(this IEnumerable<ILengthedObject> objects, IGrid grid, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var lastObjectEndTime = objects.Where(o => o != null)
                                           .Select(o => o.Time + o.Length)
                                           .DefaultIfEmpty()
                                           .Max();
            var times = grid.GetTimes(tempoMap)
                            .TakeWhile(t => t < lastObjectEndTime)
                            .Distinct()
                            .ToList();
            times.Sort();

            foreach (var obj in objects)
            {
                if (obj == null)
                {
                    yield return null;
                    continue;
                }

                var startTime = obj.Time;
                var endTime = startTime + obj.Length;

                var intersectedTimes = times.SkipWhile(t => t <= startTime).TakeWhile(t => t < endTime);
                var tail = (ILengthedObject)obj.Clone();

                foreach (var time in intersectedTimes)
                {
                    var parts = tail.Split(time);
                    yield return parts.LeftPart;

                    tail = parts.RightPart;
                }

                yield return tail;
            }
        }

        public static IEnumerable<ILengthedObject> SplitObjectsAtDistance(this IEnumerable<ILengthedObject> objects, ITimeSpan distance, LengthedObjectTarget from, TempoMap tempoMap)
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

                var parts = SplitObjectAtDistance(obj, distance, from, tempoMap);

                if (parts.LeftPart != null)
                    yield return parts.LeftPart;

                if (parts.RightPart != null)
                    yield return parts.RightPart;
            }
        }

        public static IEnumerable<ILengthedObject> SplitObjectsAtDistance(this IEnumerable<ILengthedObject> objects, double ratio, TimeSpanType lengthType, LengthedObjectTarget from, TempoMap tempoMap)
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

                var distance = obj.LengthAs(lengthType, tempoMap).Multiply(ratio);
                var parts = SplitObjectAtDistance(obj, distance, from, tempoMap);

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

        private static void SplitTrackChunkObjects(
            TrackChunk trackChunk,
            ObjectType objectType,
            ObjectDetectionSettings objectDetectionSettings,
            Func<IEnumerable<ILengthedObject>, IEnumerable<ILengthedObject>> splitOperation)
        {
            using (var objectsManager = new TimedObjectsManager(trackChunk.Events, objectType, objectDetectionSettings))
            {
                var objects = objectsManager.Objects;
                var newObjects = splitOperation(objects.Cast<ILengthedObject>()).ToList();

                objects.Clear();
                objects.Add(newObjects);
            }
        }

        private static ILengthedObject GetZeroLengthObjectAtStart(ILengthedObject baseObject, long time)
        {
            var chord = baseObject as Chord;
            if (chord != null)
                return new Chord(chord.Notes.Where(n => n.Time == time).Select(n =>
                {
                    var note = (Note)n.Clone();
                    note.Length = 0;
                    return note;
                }));

            var result = (ILengthedObject)baseObject.Clone();
            result.Length = 0;
            return result;
        }

        private static ILengthedObject GetZeroLengthObjectAtEnd(ILengthedObject baseObject, long time)
        {
            var chord = baseObject as Chord;
            if (chord != null)
                return new Chord(chord.Notes.Where(n => n.Time + n.Length == time).Select(n =>
                {
                    var note = (Note)n.Clone();
                    note.Time = time;
                    note.Length = 0;
                    return note;
                }));

            var result = (ILengthedObject)baseObject.Clone();
            result.Time = time;
            result.Length = 0;
            return result;
        }

        #endregion
    }
}
