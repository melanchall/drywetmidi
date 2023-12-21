using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Performs quantization of objects. The process can be adjusted in many ways by <see cref="QuantizingSettings"/>.
    /// More info in the <see href="xref:a_quantizer">Quantizer</see> article.
    /// </summary>
    public class Quantizer
    {
        #region Methods

        /// <summary>
        /// Quantizes objects using the specified grid.
        /// </summary>
        /// <param name="objects">Objects to quantize.</param>
        /// <param name="grid">Grid to use for quantization.</param>
        /// <param name="tempoMap">Tempo map used to perform time and length conversions.</param>
        /// <param name="settings">Settings according to which <paramref name="objects"/> should be quantized.</param>
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
        public void Quantize(IEnumerable<ITimedObject> objects, IGrid grid, TempoMap tempoMap, QuantizingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(objects), objects);
            ThrowIfArgument.IsNull(nameof(grid), grid);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            settings = settings ?? new QuantizingSettings();
            settings.RandomizingSettings = settings.RandomizingSettings ?? new RandomizingSettings();

            Func<ITimedObject, bool> filter = obj =>
                obj != null && settings.Filter?.Invoke(obj) != false;

            var times = GetGridTimes(objects, filter, grid, tempoMap);

            foreach (var obj in objects.Where(filter))
            {
                QuantizeObject(obj, grid, times, tempoMap, settings);
            }
        }

        /// <summary>
        /// Performs additional actions before the new time will be set to an object after search for
        /// nearest grid time.
        /// </summary>
        /// <param name="obj">Object to set new time for.</param>
        /// <param name="quantizedTime">Holds information about new time for <paramref name="obj"/>.</param>
        /// <param name="grid">Grid to quantize <paramref name="obj"/> by.</param>
        /// <param name="target">Target time (start or end) to update.</param>
        /// <param name="tempoMap">Tempo map used to quantize <paramref name="obj"/>.</param>
        /// <param name="settings">Settings according to which quantization performed.</param>
        /// <returns>An object indicating whether the new time should be set to the object
        /// or not. Also returned object contains that new time.</returns>
        protected virtual TimeProcessingInstruction OnObjectQuantizing(
            ITimedObject obj,
            QuantizedTime quantizedTime,
            IGrid grid,
            LengthedObjectTarget target,
            TempoMap tempoMap,
            QuantizingSettings settings)
        {
            if (quantizedTime == null)
                return TimeProcessingInstruction.Skip;

            var time = quantizedTime.NewTime;
            return TrySetLengthedObjectTime(obj as ILengthedObject, time, target, tempoMap, settings)
                ?? new TimeProcessingInstruction(time);
        }

        /// <summary>
        /// Performs additional actions before the new time will be set to an object after search for
        /// random time within bounds defined by <see cref="RandomizingSettings.Bounds"/>.
        /// </summary>
        /// <param name="obj">Object to set new time for.</param>
        /// <param name="time">New time for <paramref name="obj"/>.</param>
        /// <param name="target">Target time (start or end) to update.</param>
        /// <param name="tempoMap">Tempo map used to quantize <paramref name="obj"/>.</param>
        /// <param name="settings">Settings according to which quantization performed.</param>
        /// <returns>An object indicating whether the new time should be set to the object
        /// or not. Also returned object contains that new time.</returns>
        protected virtual TimeProcessingInstruction OnObjectRandomizing(
            ITimedObject obj,
            long time,
            LengthedObjectTarget target,
            TempoMap tempoMap,
            QuantizingSettings settings)
        {
            return TrySetLengthedObjectTime(obj as ILengthedObject, time, target, tempoMap, settings)
                ?? new TimeProcessingInstruction(time);
        }

        private TimeProcessingInstruction TrySetLengthedObjectTime(
            ILengthedObject obj,
            long time,
            LengthedObjectTarget target,
            TempoMap tempoMap,
            QuantizingSettings settings)
        {
            if (obj == null)
                return null;

            switch (target)
            {
                case LengthedObjectTarget.Start:
                    return CorrectObjectOnStartQuantizing(obj, time, tempoMap, settings);
                case LengthedObjectTarget.End:
                    return CorrectObjectOnEndQuantizing(obj, time, tempoMap, settings);
            }

            return null;
        }

        private void QuantizeObject(
            ITimedObject obj,
            IGrid grid,
            long[] times,
            TempoMap tempoMap,
            QuantizingSettings settings)
        {
            var target = settings.Target;

            if (target.HasFlag(QuantizerTarget.Start) && target.HasFlag(QuantizerTarget.End) && obj is ILengthedObject)
            {
                QuantizeObjectBothEnds(obj, grid, times, tempoMap, settings);
            }
            else
            {
                if (target.HasFlag(QuantizerTarget.Start))
                    QuantizeObjectSingleEnd(obj, grid, times, LengthedObjectTarget.Start, tempoMap, settings);

                if (target.HasFlag(QuantizerTarget.End) && obj is ILengthedObject)
                    QuantizeObjectSingleEnd(obj, grid, times, LengthedObjectTarget.End, tempoMap, settings);
            }
        }

        private void QuantizeObjectBothEnds(
            ITimedObject obj,
            IGrid grid,
            long[] times,
            TempoMap tempoMap,
            QuantizingSettings settings)
        {
            var oldStartTime = GetObjectTime(obj, LengthedObjectTarget.Start);
            var quantizedStartTime = FindNearestTime(
                times,
                oldStartTime,
                settings.DistanceCalculationType,
                settings.QuantizingLevel,
                tempoMap);

            var oldEndTime = GetObjectTime(obj, LengthedObjectTarget.End);
            var quantizedEndTime = FindNearestTime(
                times,
                oldEndTime,
                settings.DistanceCalculationType,
                settings.QuantizingLevel,
                tempoMap);

            if (quantizedStartTime.NewTime > oldEndTime)
            {
                QuantizeObjectTime(obj, quantizedEndTime, grid, LengthedObjectTarget.End, tempoMap, settings);
                QuantizeObjectTime(obj, quantizedStartTime, grid, LengthedObjectTarget.Start, tempoMap, settings);
            }
            else
            {
                QuantizeObjectTime(obj, quantizedStartTime, grid, LengthedObjectTarget.Start, tempoMap, settings);
                QuantizeObjectTime(obj, quantizedEndTime, grid, LengthedObjectTarget.End, tempoMap, settings);
            }

            RandomizeObjectTime(obj, grid, LengthedObjectTarget.Start, tempoMap, settings);
            RandomizeObjectTime(obj, grid, LengthedObjectTarget.End, tempoMap, settings);
        }

        private void QuantizeObjectSingleEnd(
            ITimedObject obj,
            IGrid grid,
            long[] times,
            LengthedObjectTarget target,
            TempoMap tempoMap,
            QuantizingSettings settings)
        {
            var oldTime = GetObjectTime(obj, target);
            var quantizedTime = FindNearestTime(
                times,
                oldTime,
                settings.DistanceCalculationType,
                settings.QuantizingLevel,
                tempoMap);

            QuantizeObjectTime(obj, quantizedTime, grid, target, tempoMap, settings);
            RandomizeObjectTime(obj, grid, target, tempoMap, settings);
        }

        private void QuantizeObjectTime(
            ITimedObject obj,
            QuantizedTime quantizedTime,
            IGrid grid,
            LengthedObjectTarget target,
            TempoMap tempoMap,
            QuantizingSettings settings)
        {
            var instruction = OnObjectQuantizing(obj, quantizedTime, grid, target, tempoMap, settings);
            switch (instruction.Action)
            {
                case TimeProcessingAction.Apply:
                    SetObjectTime(obj, target, instruction.Time);
                    break;
                case TimeProcessingAction.Skip:
                    break;
            }
        }

        private void RandomizeObjectTime(
            ITimedObject obj,
            IGrid grid,
            LengthedObjectTarget target,
            TempoMap tempoMap,
            QuantizingSettings settings)
        {
            var randomizingSettings = settings.RandomizingSettings;
            if (randomizingSettings.Filter?.Invoke(obj) == false)
                return;

            if (randomizingSettings.Bounds != null)
            {
                var time = RandomizeTime(GetObjectTime(obj, target), randomizingSettings.Bounds, tempoMap);
                var instruction = OnObjectRandomizing(obj, time, target, tempoMap, settings);

                switch (instruction.Action)
                {
                    case TimeProcessingAction.Apply:
                        SetObjectTime(obj, target, instruction.Time);
                        break;
                    case TimeProcessingAction.Skip:
                        break;
                }
            }
        }

        private static long GetObjectTime(
            ITimedObject obj,
            LengthedObjectTarget target)
        {
            var time = obj.Time;
            if (target == LengthedObjectTarget.End)
                time += ((ILengthedObject)obj).Length;

            return time;
        }

        private static void SetObjectTime(
            ITimedObject obj,
            LengthedObjectTarget target,
            long time)
        {
            switch (target)
            {
                case LengthedObjectTarget.Start:
                    obj.Time = time;
                    break;
                case LengthedObjectTarget.End:
                    obj.Time = time - ((ILengthedObject)obj).Length;
                    break;
            }
        }

        private static long RandomizeTime(long time, IBounds bounds, TempoMap tempoMap)
        {
            var timeBounds = bounds.GetBounds(time, tempoMap);

            var minTime = Math.Max(0, timeBounds.Item1) - 1;
            var maxTime = timeBounds.Item2;

            var difference = (int)Math.Abs(maxTime - minTime);
            return minTime + Common.Random.Instance.Next(difference) + 1;
        }

        private static long[] GetGridTimes(
            IEnumerable<ITimedObject> objects,
            Func<ITimedObject, bool> filter,
            IGrid grid,
            TempoMap tempoMap)
        {
            var lastTime = objects
                .Where(filter)
                .Select(obj =>
                {
                    var lengthedObject = obj as ILengthedObject;
                    return lengthedObject != null
                        ? lengthedObject.EndTime
                        : obj.Time;
                })
                .DefaultIfEmpty()
                .Max();

            return GetGridTimes(grid, lastTime, tempoMap).ToArray();
        }

        private static IEnumerable<long> GetGridTimes(IGrid grid, long lastTime, TempoMap tempoMap)
        {
            var times = grid.GetTimes(tempoMap);
            if (!times.Any())
                yield break;

            using (var enumerator = times.GetEnumerator())
            {
                var timesAvailable = true;

                while ((timesAvailable = enumerator.MoveNext()) && enumerator.Current < lastTime)
                    yield return enumerator.Current;

                if (timesAvailable)
                    yield return enumerator.Current;
            }
        }

        private static QuantizedTime FindNearestTime(
            long[] grid,
            long time,
            TimeSpanType distanceCalculationType,
            double quantizingLevel,
            TempoMap tempoMap)
        {
            if (grid.Length == 0)
                return null;

            var distanceToGridTime = -1L;
            var convertedDistanceToGridTime = TimeSpanUtilities.GetMaxTimeSpan(distanceCalculationType);
            var gridTime = -1L;

            //

            long[] candidateTimes;
            int bottomGridTimeIndex;

            MathUtilities.GetLastElementBelowThreshold(grid, time, _ => _, out bottomGridTimeIndex);
            if (bottomGridTimeIndex < 0)
            {
                candidateTimes = new[] { grid[0] };
            }
            else
            {
                var topGridTimeIndex = bottomGridTimeIndex + 1;
                candidateTimes = topGridTimeIndex >= grid.Length
                    ? new[] { grid[bottomGridTimeIndex] }
                    : new[] { grid[bottomGridTimeIndex], grid[topGridTimeIndex] };
            }

            //

            foreach (var currentGridTime in candidateTimes)
            {
                var distance = Math.Abs(time - currentGridTime);
                var convertedDistance = LengthConverter.ConvertTo(distance, distanceCalculationType, Math.Min(time, currentGridTime), tempoMap);
                if (convertedDistance.CompareTo(convertedDistanceToGridTime) >= 0)
                    break;

                distanceToGridTime = distance;
                convertedDistanceToGridTime = convertedDistance;
                gridTime = currentGridTime;
            }

            //

            var shift = convertedDistanceToGridTime.Multiply(quantizingLevel);
            var convertedTime = TimeConverter.ConvertTo(time, distanceCalculationType, tempoMap);

            var newTime = TimeConverter.ConvertFrom(
                gridTime > time
                    ? convertedTime.Add(shift, TimeSpanMode.TimeLength)
                    : convertedTime.Subtract(shift, TimeSpanMode.TimeLength),
                tempoMap);

            //

            return new QuantizedTime(
                newTime,
                gridTime,
                shift,
                distanceToGridTime,
                convertedDistanceToGridTime);
        }

        private static TimeProcessingInstruction CorrectObjectOnStartQuantizing(
            ILengthedObject obj,
            long time,
            TempoMap tempoMap,
            QuantizingSettings settings)
        {
            if (settings.FixOppositeEnd || (settings.Target.HasFlag(QuantizerTarget.Start) && settings.Target.HasFlag(QuantizerTarget.End)))
            {
                var endTime = obj.EndTime;

                if (time > endTime)
                {
                    var result = ProcessQuantizingBeyondFixedEnd(
                        ref time,
                        ref endTime,
                        settings.QuantizingBeyondFixedEndPolicy,
                        "Start time is going to be beyond the end one.");
                    if (result != null)
                        return result;
                }

                obj.Length = endTime - time;
            }
            else
            {
                var length = obj.LengthAs(settings.LengthType, tempoMap);
                obj.Length = LengthConverter.ConvertFrom(length, time, tempoMap);
            }

            return new TimeProcessingInstruction(time);
        }

        private static TimeProcessingInstruction CorrectObjectOnEndQuantizing(
            ILengthedObject obj,
            long time,
            TempoMap tempoMap,
            QuantizingSettings settings)
        {
            if (settings.FixOppositeEnd || (settings.Target.HasFlag(QuantizerTarget.Start) && settings.Target.HasFlag(QuantizerTarget.End)))
            {
                var startTime = obj.Time;

                if (time < startTime)
                {
                    var result = ProcessQuantizingBeyondFixedEnd(
                        ref time,
                        ref startTime,
                        settings.QuantizingBeyondFixedEndPolicy,
                        "End time is going to be beyond the start one.");
                    if (result != null)
                        return result;
                }

                obj.Length = time - startTime;
            }
            else
            {
                var length = obj.LengthAs(settings.LengthType, tempoMap);

                var newStartTime = settings.LengthType == TimeSpanType.Midi
                    ? time - obj.Length
                    : TimeConverter.ConvertFrom(((MidiTimeSpan)time).Subtract(length, TimeSpanMode.TimeLength), tempoMap);
                if (newStartTime < 0)
                {
                    switch (settings.QuantizingBeyondZeroPolicy)
                    {
                        case QuantizingBeyondZeroPolicy.Skip:
                            return TimeProcessingInstruction.Skip;
                        case QuantizingBeyondZeroPolicy.Abort:
                            throw new InvalidOperationException("Object is going to be moved beyond zero.");
                        case QuantizingBeyondZeroPolicy.FixAtZero:
                            obj.Length = time;
                            break;
                    }
                }
                else
                {
                    obj.Length = LengthConverter.ConvertFrom(length, newStartTime, tempoMap);
                }
            }

            return new TimeProcessingInstruction(time);
        }

        private static TimeProcessingInstruction ProcessQuantizingBeyondFixedEnd(
            ref long newTime,
            ref long oldTime,
            QuantizingBeyondFixedEndPolicy quantizingBeyondFixedEndPolicy,
            string errorMessage)
        {
            switch (quantizingBeyondFixedEndPolicy)
            {
                case QuantizingBeyondFixedEndPolicy.Skip:
                    return TimeProcessingInstruction.Skip;
                case QuantizingBeyondFixedEndPolicy.Abort:
                    throw new InvalidOperationException(errorMessage);
                case QuantizingBeyondFixedEndPolicy.CollapseAndFix:
                    newTime = oldTime;
                    break;
                case QuantizingBeyondFixedEndPolicy.CollapseAndMove:
                    oldTime = newTime;
                    break;
                case QuantizingBeyondFixedEndPolicy.SwapEnds:
                    var tmp = newTime;
                    newTime = oldTime;
                    oldTime = tmp;
                    break;
            }

            return null;
        }

        #endregion
    }
}
