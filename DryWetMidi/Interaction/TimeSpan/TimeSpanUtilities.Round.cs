using Melanchall.DryWetMidi.Common;
using System;
using System.ComponentModel;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    public static partial class TimeSpanUtilities
    {
        #region Methods

        /// <summary>
        /// Rounds a time span using the specified step and rounding policy.
        /// </summary>
        /// <remarks>
        /// Note that in general case you need to specify a time where the <paramref name="timeSpan"/> starts and so
        /// rounding means to snap the end of the <paramref name="timeSpan"/> to a point of the grid which started
        /// at the <paramref name="time"/>. If you want to treat the <paramref name="timeSpan"/> as just a point in time,
        /// pass a zero time span to the <paramref name="time"/> (for example, <c>(MidiTimeSpan)0</c>).
        /// </remarks>
        /// <param name="timeSpan">Time span to round.</param>
        /// <param name="roundingPolicy">Policy according to which the <paramref name="timeSpan"/> should be rounded.</param>
        /// <param name="time">Time of the <paramref name="timeSpan"/>.</param>
        /// <param name="step">Step to round the <paramref name="timeSpan"/> by.</param>
        /// <param name="tempoMap">Tempo map used to calculate new time span.</param>
        /// <returns>A new time span which is the <paramref name="timeSpan"/> rounded using the passed parameters.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="timeSpan"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="time"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="step"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="roundingPolicy"/> specified an invalid value.</exception>
        public static ITimeSpan Round(
            this ITimeSpan timeSpan,
            TimeSpanRoundingPolicy roundingPolicy,
            ITimeSpan time,
            ITimeSpan step,
            TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);
            ThrowIfArgument.IsInvalidEnumValue(nameof(roundingPolicy), roundingPolicy);
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(step), step);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            if (roundingPolicy == TimeSpanRoundingPolicy.NoRounding || step.IsZeroTimeSpan())
                return timeSpan.Clone();

            var midiTime = TimeConverter.ConvertFrom(time, tempoMap);

            var metricStep = step as MetricTimeSpan;
            if (metricStep != null)
            {
                var metricTimeSpan = TimeSpanConverter.ConvertTo<MetricTimeSpan>(timeSpan, midiTime, tempoMap);
                return Round(
                    timeSpan,
                    metricTimeSpan.TotalMicroseconds,
                    metricStep.TotalMicroseconds,
                    roundingPolicy,
                    factor => new MetricTimeSpan(factor * metricStep.TotalMicroseconds));
            }

            var barBeatTicksStep = step as BarBeatTicksTimeSpan;
            if (barBeatTicksStep != null)
                return RoundBarBeatTicksTimeSpan(
                    timeSpan,
                    midiTime,
                    step,
                    tempoMap,
                    roundingPolicy);

            var barBeatFractionStep = step as BarBeatFractionTimeSpan;
            if (barBeatFractionStep != null)
                return RoundBarBeatFractionTimeSpan(
                    timeSpan,
                    midiTime,
                    step,
                    tempoMap,
                    roundingPolicy);

            var midiStep = TimeSpanConverter.ConvertTo<MidiTimeSpan>(step, midiTime, tempoMap);
            var midiTimeSpan = TimeSpanConverter.ConvertTo<MidiTimeSpan>(timeSpan, midiTime, tempoMap);
            return Round(
                timeSpan,
                midiTimeSpan.TimeSpan,
                midiStep.TimeSpan,
                roundingPolicy,
                factor => TimeSpanConverter.ConvertTo(new MidiTimeSpan(factor * midiStep.TimeSpan), step.GetType(), midiTime, tempoMap));
        }

        private static ITimeSpan RoundBarBeatFractionTimeSpan(
            ITimeSpan timeSpan,
            long time,
            ITimeSpan step,
            TempoMap tempoMap,
            TimeSpanRoundingPolicy roundingPolicy) => RoundBarBeatTimeSpan<BarBeatFractionTimeSpan>(
                timeSpan,
                time,
                step,
                tempoMap,
                roundingPolicy);

        private static ITimeSpan RoundBarBeatTicksTimeSpan(
            ITimeSpan timeSpan,
            long time,
            ITimeSpan step,
            TempoMap tempoMap,
            TimeSpanRoundingPolicy roundingPolicy) => RoundBarBeatTimeSpan<BarBeatTicksTimeSpan>(
                timeSpan,
                time,
                step,
                tempoMap,
                roundingPolicy);

        private static ITimeSpan RoundBarBeatTimeSpan<TTimeSpan>(
            ITimeSpan timeSpan,
            long time,
            ITimeSpan step,
            TempoMap tempoMap,
            TimeSpanRoundingPolicy roundingPolicy)
            where TTimeSpan : ITimeSpan
        {
            var ticks = TimeSpanConverter.ConvertFrom(timeSpan, time, tempoMap);

            // Simple case: there are no time signature changes

            if (typeof(TTimeSpan) != typeof(BarBeatFractionTimeSpan))
            {
                var timeSignatureChanges = tempoMap.GetTimeSignatureChanges();
                if (!timeSignatureChanges.Any() || !timeSignatureChanges.SkipWhile(c => c.Time <= time).TakeWhile(c => c.Time < time + ticks).Any())
                {
                    var stepTicks = TimeSpanConverter.ConvertFrom(step, time, tempoMap);
                    return Round(
                        timeSpan,
                        ticks,
                        stepTicks,
                        roundingPolicy,
                        factor => TimeSpanConverter.ConvertTo<TTimeSpan>((ITimeSpan)new MidiTimeSpan(factor * stepTicks), time, tempoMap));
                }
            }

            // Step 1: calculate approximate factor for the step to include the time span

            var startFactor = 0;
            var endFactor = 1;

            while (true)
            {
                var boundingTimeSpan = step.Multiply(endFactor);
                var boundingTimeSpanTicks = TimeSpanConverter.ConvertFrom(boundingTimeSpan, time, tempoMap);

                if (ticks < boundingTimeSpanTicks)
                    break;

                startFactor = endFactor;
                endFactor *= 2;
            }

            // Step 2: binary search for the factor which gives the time span closest to a grid point

            while (startFactor <= endFactor)
            {
                var middleFactor = (startFactor + endFactor) / 2;
                var middleTicks = TimeSpanConverter.ConvertFrom(step.Multiply(middleFactor), time, tempoMap);

                if (middleTicks > ticks)
                    endFactor = middleFactor - 1;
                else if (middleTicks < ticks)
                    startFactor = middleFactor + 1;
                else if (middleTicks == ticks)
                    return timeSpan.Clone();
            }

            // Step 3: calculate the result

            var resultTicks = 0L;

            if (startFactor > 0)
            {
                var lowerTicks = TimeSpanConverter.ConvertFrom(step.Multiply(startFactor - 1), time, tempoMap);
                var upperTicks = TimeSpanConverter.ConvertFrom(step.Multiply(startFactor), time, tempoMap);
                var gridShift = GetGridShift(
                    roundingPolicy,
                    ticks - lowerTicks,
                    upperTicks - ticks);

                resultTicks = gridShift > 0
                    ? upperTicks
                    : lowerTicks;
            }

            var result = (ITimeSpan)TimeSpanConverter.ConvertTo<TTimeSpan>(
                resultTicks,
                time,
                tempoMap);

            // Step 4: adjust the result for BarBeatFractionTimeSpan to reduce rounding errors

            var barBeatFractionStep = step as BarBeatFractionTimeSpan;
            if (barBeatFractionStep != null && barBeatFractionStep.Beats > 0)
            {
                var barBeatFractionResult = (BarBeatFractionTimeSpan)result;

                var beatFractionString = barBeatFractionStep.Beats.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var beatFractionDigitsCount = beatFractionString.Length - beatFractionString.IndexOf('.') - 1;
                result = new BarBeatFractionTimeSpan(
                    barBeatFractionResult.Bars,
                    Math.Round(barBeatFractionResult.Beats, beatFractionDigitsCount));
            }

            return result;
        }

        private static ITimeSpan Round<TTimeSpan>(
            ITimeSpan timeSpan,
            long x,
            long y,
            TimeSpanRoundingPolicy roundingPolicy,
            Func<long, TTimeSpan> createTimeSpan)
            where TTimeSpan : ITimeSpan
        {
            var quotient = Math.DivRem(x, y, out var reminder);
            if (reminder == 0)
                return timeSpan.Clone();

            var gridShift = GetGridShift(
                roundingPolicy,
                reminder,
                y - reminder);

            return createTimeSpan(quotient + gridShift);
        }

        private static int GetGridShift(
            TimeSpanRoundingPolicy roundingPolicy,
            long distanceToLowerGridPoint,
            long distanceToUpperGridPoint)
        {
            switch (roundingPolicy)
            {
                case TimeSpanRoundingPolicy.RoundUp:
                    return 1;
                case TimeSpanRoundingPolicy.RoundToNearest:
                    return distanceToUpperGridPoint <= distanceToLowerGridPoint ? 1 : 0;
                default:
                    return 0;
            }
        }

        #endregion
    }
}
