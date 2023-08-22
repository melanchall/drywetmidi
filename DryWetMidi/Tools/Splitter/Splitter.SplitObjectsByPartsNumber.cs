using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    public static partial class Splitter
    {
        #region Methods

        /// <summary>
        /// Splits objects within a <see cref="TrackChunk"/> into the specified number of parts of the equal length.
        /// More info in the <see href="xref:a_obj_splitting#splitobjectsbypartsnumber">Objects splitting: SplitObjectsByPartsNumber</see>
        /// article.
        /// </summary>
        /// <remarks>
        /// Nulls will not be split and will be returned as <c>null</c>s. If an object has zero length,
        /// it will be split into the specified number of parts of zero length.
        /// </remarks>
        /// <param name="trackChunk"><see cref="TrackChunk"/> to split objects within.</param>
        /// <param name="objectType">The type of objects to split.</param>
        /// <param name="partsNumber">The number of parts to split objects into.</param>
        /// <param name="lengthType">Type of a part's length.</param>
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
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="partsNumber"/> is zero or negative.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="lengthType"/> specified an invalid value.</exception>
        public static void SplitObjectsByPartsNumber(
            this TrackChunk trackChunk,
            ObjectType objectType,
            int partsNumber,
            TimeSpanType lengthType,
            TempoMap tempoMap,
            ObjectDetectionSettings objectDetectionSettings = null,
            Predicate<ITimedObject> filter = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunk), trackChunk);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            SplitTrackChunkObjects(
                trackChunk,
                objectType,
                objectDetectionSettings,
                objects => SplitObjectsByPartsNumber(objects, partsNumber, lengthType, tempoMap, filter));
        }

        /// <summary>
        /// Splits objects within a collection of <see cref="TrackChunk"/> into the specified number of parts of
        /// the equal length. More info in the
        /// <see href="xref:a_obj_splitting#splitobjectsbypartsnumber">Objects splitting: SplitObjectsByPartsNumber</see>
        /// article.
        /// </summary>
        /// <remarks>
        /// Nulls will not be split and will be returned as <c>null</c>s. If an object has zero length,
        /// it will be split into the specified number of parts of zero length.
        /// </remarks>
        /// <param name="trackChunks">A collection of <see cref="TrackChunk"/> to split objects within.</param>
        /// <param name="objectType">The type of objects to split.</param>
        /// <param name="partsNumber">The number of parts to split objects into.</param>
        /// <param name="lengthType">Type of a part's length.</param>
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
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="partsNumber"/> is zero or negative.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="lengthType"/> specified an invalid value.</exception>
        public static void SplitObjectsByPartsNumber(
            this IEnumerable<TrackChunk> trackChunks,
            ObjectType objectType,
            int partsNumber,
            TimeSpanType lengthType,
            TempoMap tempoMap,
            ObjectDetectionSettings objectDetectionSettings = null,
            Predicate<ITimedObject> filter = null)
        {
            ThrowIfArgument.IsNull(nameof(trackChunks), trackChunks);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var trackChunk in trackChunks)
            {
                trackChunk.SplitObjectsByPartsNumber(objectType, partsNumber, lengthType, tempoMap, objectDetectionSettings, filter);
            }
        }

        /// <summary>
        /// Splits objects within a <see cref="MidiFile"/> into the specified number of parts of the equal length.
        /// More info in the <see href="xref:a_obj_splitting#splitobjectsbypartsnumber">Objects splitting: SplitObjectsByPartsNumber</see>
        /// article.
        /// </summary>
        /// <remarks>
        /// Nulls will not be split and will be returned as <c>null</c>s. If an object has zero length,
        /// it will be split into the specified number of parts of zero length.
        /// </remarks>
        /// <param name="midiFile"><see cref="MidiFile"/> to split objects within.</param>
        /// <param name="objectType">The type of objects to split.</param>
        /// <param name="partsNumber">The number of parts to split objects into.</param>
        /// <param name="lengthType">Type of a part's length.</param>
        /// <param name="objectDetectionSettings">Settings according to which objects should be
        /// detected and built.</param>
        /// <param name="filter">Predicate used to determine whether an object should be split or not.
        /// <c>true</c> as a return value of the predicate means an object should be split; <c>false</c>
        /// means don't split it. <c>null</c> (the default value) can be passed to the parameter
        /// to process all objects.</param>
        /// <exception cref="ArgumentNullException"><paramref name="midiFile"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="partsNumber"/> is zero or negative.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="lengthType"/> specified an invalid value.</exception>
        public static void SplitObjectsByPartsNumber(
            this MidiFile midiFile,
            ObjectType objectType,
            int partsNumber,
            TimeSpanType lengthType,
            ObjectDetectionSettings objectDetectionSettings = null,
            Predicate<ITimedObject> filter = null)
        {
            ThrowIfArgument.IsNull(nameof(midiFile), midiFile);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);

            var tempoMap = midiFile.GetTempoMap();
            midiFile
                .GetTrackChunks()
                .SplitObjectsByPartsNumber(objectType, partsNumber, lengthType, tempoMap, objectDetectionSettings, filter);
        }

        /// <summary>
        /// Splits objects into the specified number of parts of the equal length. More info in the
        /// <see href="xref:a_obj_splitting#splitobjectsbypartsnumber">Objects splitting: SplitObjectsByPartsNumber</see>
        /// article.
        /// </summary>
        /// <remarks>
        /// Nulls will not be split and will be returned as <c>null</c>s. If an object has zero length,
        /// it will be split into the specified number of parts of zero length.
        /// </remarks>
        /// <param name="objects">Objects to split.</param>
        /// <param name="partsNumber">The number of parts to split objects into.</param>
        /// <param name="lengthType">Type of a part's length.</param>
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
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="partsNumber"/> is zero or negative.</exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="lengthType"/> specified an invalid value.</exception>
        public static IEnumerable<ITimedObject> SplitObjectsByPartsNumber(
            this IEnumerable<ITimedObject> objects,
            int partsNumber,
            TimeSpanType lengthType,
            TempoMap tempoMap,
            Predicate<ITimedObject> filter = null)
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
                if (lengthedObject == null || filter?.Invoke(obj) == false)
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
                    var part = GetZeroLengthObjectAtEnd(lengthedObject, lengthedObject.EndTime);

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
