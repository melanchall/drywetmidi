using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class Splitter
    {
        #region Methods

        /// <summary>
        /// Splits objects within a <see cref="TrackChunk"/> by the specified grid. More info in the
        /// <see href="xref:a_obj_splitting#splitobjectsbygrid">Objects splitting: SplitObjectsByGrid</see> article.
        /// </summary>
        /// <remarks>
        /// Nulls will not be split and will be returned as <c>null</c>s.
        /// </remarks>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to split objects within.</param>
        /// <param name="objectType">The type of objects to split.</param>
        /// <param name="grid">Grid to split objects by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <param name="objectDetectionSettings">Settings according to which objects should be
        /// detected and built.</param>
        /// <param name="filter">Predicate used to determine whether an object should be split or not.
        /// <c>true</c> as a return value of the predicate means an object should be split; <c>false</c>
        /// means don't split it. <c>null</c> (the default value) can be passed to the parameter
        /// to process all objects.</param>
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
        public static void SplitObjectsByGrid(
            this TrackChunk trackChunk,
            ObjectType objectType,
            IGrid grid,
            TempoMap tempoMap,
            ObjectDetectionSettings objectDetectionSettings = null,
            Predicate<ITimedObject> filter = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            SplitTrackChunkObjects(
                trackChunk,
                objectType,
                objectDetectionSettings,
                objects => SplitObjectsByGrid(objects, grid, tempoMap, filter));
        }

        /// <summary>
        /// Splits objects within a collection of <see cref="TrackChunk"/> by the specified grid. More info in the
        /// <see href="xref:a_obj_splitting#splitobjectsbygrid">Objects splitting: SplitObjectsByGrid</see> article.
        /// </summary>
        /// <remarks>
        /// Nulls will not be split and will be returned as <c>null</c>s.
        /// </remarks>
        /// <param name="trackChunks">A collection of <see cref="TrackChunk"/> to split objects within.</param>
        /// <param name="objectType">The type of objects to split.</param>
        /// <param name="grid">Grid to split objects by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <param name="objectDetectionSettings">Settings according to which objects should be
        /// detected and built.</param>
        /// <param name="filter">Predicate used to determine whether an object should be split or not.
        /// <c>true</c> as a return value of the predicate means an object should be split; <c>false</c>
        /// means don't split it. <c>null</c> (the default value) can be passed to the parameter
        /// to process all objects.</param>
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
        public static void SplitObjectsByGrid(
            this IEnumerable<TrackChunk> trackChunks,
            ObjectType objectType,
            IGrid grid,
            TempoMap tempoMap,
            ObjectDetectionSettings objectDetectionSettings = null,
            Predicate<ITimedObject> filter = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.SplitObjectsByGrid(objectType, grid, tempoMap, objectDetectionSettings, filter);
            }
        }

        /// <summary>
        /// Splits objects within a <see cref="MidiFile"/> by the specified grid. More info in the
        /// <see href="xref:a_obj_splitting#splitobjectsbygrid">Objects splitting: SplitObjectsByGrid</see> article.
        /// </summary>
        /// <remarks>
        /// Nulls will not be split and will be returned as <c>null</c>s.
        /// </remarks>
        /// <param name="midiFile"><see cref="MidiFile"/> to split objects within.</param>
        /// <param name="objectType">The type of objects to split.</param>
        /// <param name="grid">Grid to split objects by.</param>
        /// <param name="objectDetectionSettings">Settings according to which objects should be
        /// detected and built.</param>
        /// <param name="filter">Predicate used to determine whether an object should be split or not.
        /// <c>true</c> as a return value of the predicate means an object should be split; <c>false</c>
        /// means don't split it. <c>null</c> (the default value) can be passed to the parameter
        /// to process all objects.</param>
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
        public static void SplitObjectsByGrid(
            this MidiFile midiFile,
            ObjectType objectType,
            IGrid grid,
            ObjectDetectionSettings objectDetectionSettings = null,
            Predicate<ITimedObject> filter = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNull(nameof(grid), grid);

            var tempoMap = midiFile.GetTempoMap();

            midiFile.GetTrackChunks().SplitObjectsByGrid(objectType, grid, tempoMap, objectDetectionSettings, filter);
        }

        /// <summary>
        /// Splits objects by the specified grid. More info in the
        /// <see href="xref:a_obj_splitting#splitobjectsbygrid">Objects splitting: SplitObjectsByGrid</see> article.
        /// </summary>
        /// <remarks>
        /// Nulls will not be split and will be returned as <c>null</c>s.
        /// </remarks>
        /// <param name="objects">Objects to split.</param>
        /// <param name="grid">Grid to split objects by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <param name="filter">Predicate used to determine whether an object should be split or not.
        /// <c>true</c> as a return value of the predicate means an object should be split; <c>false</c>
        /// means don't split it. <c>null</c> (the default value) can be passed to the parameter
        /// to process all objects.</param>
        /// <returns>Objects that are result of splitting <paramref name="objects"/> going in the same
        /// order as elements of <paramref name="objects"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="objects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="grid"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static IEnumerable<ITimedObject> SplitObjectsByGrid(
            this IEnumerable<ITimedObject> objects,
            IGrid grid,
            TempoMap tempoMap,
            Predicate<ITimedObject> filter = null)
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
                if (lengthedObject == null || filter?.Invoke(obj) == false)
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
