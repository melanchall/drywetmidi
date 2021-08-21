using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Provides methods for splitting lengthed objects.
    /// </summary>
    /// <typeparam name="TObject">The type of objects to split.</typeparam>
    public abstract class LengthedObjectsSplitter<TObject>
        where TObject : ILengthedObject
    {
        #region Constants

        internal const double ZeroRatio = 0.0;
        internal const double FullLengthRatio = 1.0;

        #endregion

        #region Methods

        /// <summary>
        /// Splits objects by the specified step so every object will be split at points
        /// equally distanced from each other starting from the object's start time.
        /// </summary>
        /// <remarks>
        /// Nulls, objects with zero length and objects with length smaller than <paramref name="step"/>
        /// will not be split and will be returned as clones of the input objects.
        /// </remarks>
        /// <param name="objects">Objects to split.</param>
        /// <param name="step">Step to split objects by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <returns>Objects that are result of splitting <paramref name="objects"/> going in the same
        /// order as elements of <paramref name="objects"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="objects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="step"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public IEnumerable<TObject> SplitByStep(IEnumerable<TObject> objects, ITimeSpan step, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(step), step);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var obj in objects)
            {
                if (obj == null)
                {
                    yield return default(TObject);
                    continue;
                }

                if (obj.Length == 0)
                {
                    yield return CloneObject(obj);
                    continue;
                }

                var startTime = obj.Time;
                var endTime = startTime + obj.Length;

                var time = startTime;
                var tail = CloneObject(obj);

                while (time < endTime && tail != null)
                {
                    var convertedStep = LengthConverter.ConvertFrom(step, time, tempoMap);
                    if (convertedStep == 0)
                        throw new InvalidOperationException("Step is too small.");

                    time += convertedStep;

                    var parts = SplitObject(tail, time);
                    yield return parts.LeftPart;

                    tail = parts.RightPart;
                }
            }
        }

        /// <summary>
        /// Splits objects into the specified number of parts of the equal length.
        /// </summary>
        /// <remarks>
        /// Nulls will not be split and will be returned as <c>null</c>s. If an object has zero length,
        /// it will be split into the specified number of parts of zero length.
        /// </remarks>
        /// <param name="objects">Objects to split.</param>
        /// <param name="partsNumber">The number of parts to split objects into.</param>
        /// <param name="lengthType">Type of a part's length.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <returns>Objects that are result of splitting <paramref name="objects"/> going in the same
        /// order as elements of <paramref name="objects"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
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
        public IEnumerable<TObject> SplitByPartsNumber(IEnumerable<TObject> objects, int partsNumber, TimeSpanType lengthType, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNonpositive(nameof(partsNumber), partsNumber, "Parts number is zero or negative.");
            ThrowIfArgument.IsInvalidEnumValue(nameof(lengthType), lengthType);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var obj in objects)
            {
                if (obj == null)
                {
                    yield return default(TObject);
                    continue;
                }

                if (partsNumber == 1)
                {
                    yield return CloneObject(obj);
                    continue;
                }

                if (obj.Length == 0)
                {
                    foreach (var i in Enumerable.Range(0, partsNumber))
                    {
                        yield return CloneObject(obj);
                    }

                    continue;
                }

                var time = obj.Time;
                var tail = CloneObject(obj);

                for (int partsRemaining = partsNumber; partsRemaining > 1 && tail != null; partsRemaining--)
                {
                    var length = tail.LengthAs(lengthType, tempoMap);
                    var partLength = length.Divide(partsRemaining);

                    time += LengthConverter.ConvertFrom(partLength, time, tempoMap);

                    var parts = SplitObject(tail, time);
                    yield return parts.LeftPart;

                    tail = parts.RightPart;
                }

                if (tail != null)
                    yield return tail;
            }
        }

        /// <summary>
        /// Splits objects by the specified grid.
        /// </summary>
        /// <remarks>
        /// Nulls will not be split and will be returned as <c>null</c>s.
        /// </remarks>
        /// <param name="objects">Objects to split.</param>
        /// <param name="grid">Grid to split objects by.</param>
        /// <param name="tempoMap">Tempo map used to calculate times to split by.</param>
        /// <returns>Objects that are result of splitting <paramref name="objects"/> going in the same
        /// order as elements of <paramref name="objects"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
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
        public IEnumerable<TObject> SplitByGrid(IEnumerable<TObject> objects, IGrid grid, TempoMap tempoMap)
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
                    yield return default(TObject);
                    continue;
                }

                var startTime = obj.Time;
                var endTime = startTime + obj.Length;

                var intersectedTimes = times.SkipWhile(t => t <= startTime).TakeWhile(t => t < endTime);
                var tail = CloneObject(obj);

                foreach (var time in intersectedTimes)
                {
                    var parts = SplitObject(tail, time);
                    yield return parts.LeftPart;

                    tail = parts.RightPart;
                }

                yield return tail;
            }
        }

        /// <summary>
        /// Splits objects at the specified distance from an object's start or end.
        /// </summary>
        /// <param name="objects">Objects to split.</param>
        /// <param name="distance">Distance to split objects at.</param>
        /// <param name="from">Point of an object <paramref name="distance"/> should be measured from.</param>
        /// <param name="tempoMap">Tempo map used for distances calculations.</param>
        /// <returns>Objects that are result of splitting <paramref name="objects"/> going in the same
        /// order as elements of <paramref name="objects"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="objects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="distance"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="from"/> specified an invalid value.</exception>
        public IEnumerable<TObject> SplitAtDistance(IEnumerable<TObject> objects, ITimeSpan distance, LengthedObjectTarget from, TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(distance), distance);
            ThrowIfArgument.IsInvalidEnumValue(nameof(from), from);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            foreach (var obj in objects)
            {
                if (obj == null)
                {
                    yield return default(TObject);
                    continue;
                }

                var parts = SplitObjectAtDistance(obj, distance, from, tempoMap);

                if (parts.LeftPart != null)
                    yield return parts.LeftPart;

                if (parts.RightPart != null)
                    yield return parts.RightPart;
            }
        }

        /// <summary>
        /// Splits objects by the specified ratio of an object's length measuring it from
        /// the object's start or end. For example, 0.5 means splitting at the center of an object.
        /// </summary>
        /// <param name="objects">Objects to split.</param>
        /// <param name="ratio">Ratio of an object's length to split by. Valid values are from 0 to 1.</param>
        /// <param name="lengthType">The type an object's length should be processed according to.</param>
        /// <param name="from">Point of an object distance should be measured from.</param>
        /// <param name="tempoMap">Tempo map used for distances calculations.</param>
        /// <returns>Objects that are result of splitting <paramref name="objects"/> going in the same
        /// order as elements of <paramref name="objects"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="objects"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="ratio"/> is out of valid range.</exception>
        /// <exception cref="InvalidEnumArgumentException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="lengthType"/> specified an invalid value.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="from"/> specified an invalid value.</description>
        /// </item>
        /// </list>
        /// </exception>
        public IEnumerable<TObject> SplitAtDistance(IEnumerable<TObject> objects, double ratio, TimeSpanType lengthType, LengthedObjectTarget from, TempoMap tempoMap)
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
                    yield return default(TObject);
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

        /// <summary>
        /// Clones an object by creating a copy of it.
        /// </summary>
        /// <param name="obj">Object to clone.</param>
        /// <returns>Copy of the <paramref name="obj"/>.</returns>
        protected abstract TObject CloneObject(TObject obj);

        /// <summary>
        /// Splits an object by the specified time.
        /// </summary>
        /// <param name="obj">Object to split.</param>
        /// <param name="time">Time to split <paramref name="obj"/> by.</param>
        /// <returns>An object containing left and right parts of the split object.</returns>
        protected abstract SplitLengthedObject<TObject> SplitObject(TObject obj, long time);

        private SplitLengthedObject<TObject> SplitObjectAtDistance(TObject obj, ITimeSpan distance, LengthedObjectTarget from, TempoMap tempoMap)
        {
            var time = from == LengthedObjectTarget.Start
                ? ((MidiTimeSpan)obj.Time).Add(distance, TimeSpanMode.TimeLength)
                : ((MidiTimeSpan)(obj.Time + obj.Length)).Subtract(distance, TimeSpanMode.TimeLength);

            return SplitObject(obj, TimeConverter.ConvertFrom(time, tempoMap));
        }

        #endregion
    }
}
