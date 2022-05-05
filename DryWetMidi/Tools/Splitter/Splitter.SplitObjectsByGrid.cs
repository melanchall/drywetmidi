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

        public static IEnumerable<ITimedObject> SplitObjectsByGrid(this IEnumerable<ITimedObject> objects, IGrid grid, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            var lastObjectEndTime = objects.Where(o => o != null)
                                           .Select(o => o.Time + ((o as ILengthedObject)?.Length ?? 0))
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

                var lengthedObject = obj as ILengthedObject;
                if (lengthedObject == null)
                {
                    yield return obj;
                    continue;
                }

                var startTime = obj.Time;
                var endTime = startTime + lengthedObject.Length;

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

        #endregion
    }
}
