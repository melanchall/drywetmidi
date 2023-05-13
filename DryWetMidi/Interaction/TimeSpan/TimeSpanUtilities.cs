using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides useful utilities for working with <see cref="ITimeSpan"/>.
    /// </summary>
    public static class TimeSpanUtilities
    {
        #region Constants

        private static readonly Dictionary<TimeSpanType, Parsing<ITimeSpan>> Parsers =
            new Dictionary<TimeSpanType, Parsing<ITimeSpan>>
            {
                [TimeSpanType.Midi] = GetParsing<MidiTimeSpan>(MidiTimeSpanParser.TryParse),
                [TimeSpanType.BarBeatTicks] = GetParsing<BarBeatTicksTimeSpan>(BarBeatTicksTimeSpanParser.TryParse),
                [TimeSpanType.BarBeatFraction] = GetParsing<BarBeatFractionTimeSpan>(BarBeatFractionTimeSpanParser.TryParse),
                [TimeSpanType.Metric] = GetParsing<MetricTimeSpan>(MetricTimeSpanParser.TryParse),
                [TimeSpanType.Musical] = GetParsing<MusicalTimeSpan>(MusicalTimeSpanParser.TryParse)
            };

        private static readonly Dictionary<TimeSpanType, ITimeSpan> MaximumTimeSpans = new Dictionary<TimeSpanType, ITimeSpan>
        {
            [TimeSpanType.Midi] = new MidiTimeSpan(long.MaxValue),
            [TimeSpanType.Metric] = new MetricTimeSpan(TimeSpan.MaxValue),
            [TimeSpanType.Musical] = new MusicalTimeSpan(long.MaxValue, 1),
            [TimeSpanType.BarBeatTicks] = new BarBeatTicksTimeSpan(long.MaxValue, long.MaxValue, long.MaxValue),
            [TimeSpanType.BarBeatFraction] = new BarBeatFractionTimeSpan(long.MaxValue, double.MaxValue)
        };

        private static readonly Dictionary<TimeSpanType, ITimeSpan> ZeroTimeSpans = new Dictionary<TimeSpanType, ITimeSpan>
        {
            [TimeSpanType.Midi] = new MidiTimeSpan(),
            [TimeSpanType.Metric] = new MetricTimeSpan(),
            [TimeSpanType.Musical] = new MusicalTimeSpan(),
            [TimeSpanType.BarBeatTicks] = new BarBeatTicksTimeSpan(),
            [TimeSpanType.BarBeatFraction] = new BarBeatFractionTimeSpan()
        };

        #endregion

        #region Methods

        /// <summary>
        /// Rounds a time span using the specified step and rounding policy.
        /// </summary>
        /// <param name="timeSpan">Time span to round.</param>
        /// <param name="roundingPolicy">Policy according to which the <paramref name="timeSpan"/> should be rounded.</param>
        /// <param name="time">Time os the <paramref name="timeSpan"/>.</param>
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
        /// <description><paramref name="step"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempoMap"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="roundingPolicy"/> specified an invalid value.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        public static ITimeSpan Round(
            this ITimeSpan timeSpan,
            TimeSpanRoundingPolicy roundingPolicy,
            long time,
            ITimeSpan step,
            TempoMap tempoMap)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);
            ThrowIfArgument.IsInvalidEnumValue(nameof(roundingPolicy), roundingPolicy);
            ThrowIfArgument.IsNegative(nameof(time), time, "Time is negative.");
            ThrowIfArgument.IsNull(nameof(step), step);
            ThrowIfArgument.IsNull(nameof(tempoMap), tempoMap);

            if (roundingPolicy == TimeSpanRoundingPolicy.NoRounding || step.IsZeroTimeSpan())
                return timeSpan.Clone();

            var gridShift = roundingPolicy == TimeSpanRoundingPolicy.RoundUp
                ? 1
                : 0;

            var metricStep = step as MetricTimeSpan;
            if (metricStep != null)
            {
                var metricTimeSpan = TimeSpanConverter.ConvertTo<MetricTimeSpan>(timeSpan, time, tempoMap);
                return Round(
                    timeSpan,
                    metricTimeSpan.TotalMicroseconds,
                    metricStep.TotalMicroseconds,
                    quotient => new MetricTimeSpan((quotient + gridShift) * metricStep.TotalMicroseconds));
            }

            var barBeatTicksStep = step as BarBeatTicksTimeSpan;
            if (barBeatTicksStep != null)
            {
                var barBeatTicksTimeSpan = TimeSpanConverter.ConvertTo<BarBeatTicksTimeSpan>(timeSpan, time, tempoMap);

                var stepParts = new[] { barBeatTicksStep.Bars, barBeatTicksStep.Beats, barBeatTicksStep.Ticks };
                var timeSpanParts = new[] { barBeatTicksTimeSpan.Bars, barBeatTicksTimeSpan.Beats, barBeatTicksTimeSpan.Ticks };

                var reminders = new long[3];
                var quotients = reminders
                    .Select((_, i) => stepParts[i] != 0 ? Math.DivRem(timeSpanParts[i], stepParts[i], out reminders[i]) : stepParts[i])
                    .ToArray();

                var flags = new bool[3];
                var zeroes = new bool[3];

                if (stepParts[1] == 0 && stepParts[2] == 0)
                {
                    flags[0] = timeSpanParts[1] != 0 || timeSpanParts[2] != 0;
                    zeroes[1] = zeroes[2] = true;
                }
                else if (stepParts[2] == 0)
                {
                    flags[1] = timeSpanParts[2] != 0;
                    zeroes[2] = true;
                }

                var resultParts = quotients
                    .Select((q, i) => zeroes[i] ? 0 : (reminders[i] == 0 && !flags[i] ? timeSpanParts[i] : (q + gridShift) * stepParts[i]))
                    .ToArray();

                return new BarBeatTicksTimeSpan(
                    resultParts[0],
                    resultParts[1],
                    resultParts[2]);
            }

            var barBeatFractionStep = step as BarBeatFractionTimeSpan;
            if (barBeatFractionStep != null)
            {
                var barBeatFractionTimeSpan = TimeSpanConverter.ConvertTo<BarBeatFractionTimeSpan>(timeSpan, time, tempoMap);

                var barsRemainder = 0L;
                var barsQuotient = barBeatFractionStep.Bars != 0
                    ? Math.DivRem(barBeatFractionTimeSpan.Bars, barBeatFractionStep.Bars, out barsRemainder)
                    : 0;

                var beatsQuotient = barBeatFractionStep.Beats >= 0.00001
                    ? (long)Math.Truncate(barBeatFractionTimeSpan.Beats / barBeatFractionStep.Beats)
                    : 0;
                var beatsRemainder = barBeatFractionStep.Beats >= 0.00001
                    ? barBeatFractionTimeSpan.Beats % barBeatFractionStep.Beats
                    : 0;

                var flag = false;
                var zeroBeats = false;

                if (barBeatFractionStep.Beats < 0.00001)
                {
                    flag = barBeatFractionTimeSpan.Beats >= 0.00001;
                    zeroBeats = true;
                }

                return new BarBeatFractionTimeSpan(
                    barsRemainder == 0 && !flag ? barBeatFractionTimeSpan.Bars : (barsQuotient + gridShift) * barBeatFractionStep.Bars,
                    zeroBeats ? 0 : (beatsRemainder < 0.00001 ? barBeatFractionTimeSpan.Beats : (beatsQuotient + gridShift) * barBeatFractionStep.Beats));
            }

            var midiStep = TimeSpanConverter.ConvertTo<MidiTimeSpan>(step, time, tempoMap);
            var midiTimeSpan = TimeSpanConverter.ConvertTo<MidiTimeSpan>(timeSpan, time, tempoMap);
            return Round(
                timeSpan,
                midiTimeSpan.TimeSpan,
                midiStep.TimeSpan,
                quotient => TimeSpanConverter.ConvertTo(new MidiTimeSpan((quotient + gridShift) * midiStep.TimeSpan), step.GetType(), time, tempoMap));
        }

        /// <summary>
        /// Converts the string representation of a time span to its <see cref="ITimeSpan"/> equivalent.
        /// A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="input">A string containing a time span to convert.</param>
        /// <param name="timeSpan">When this method returns, contains the <see cref="ITimeSpan"/>
        /// equivalent of the time span contained in <paramref name="input"/>, if the conversion succeeded, or
        /// <c>null</c> if the conversion failed. The conversion fails if the <paramref name="input"/> is <c>null</c> or
        /// <see cref="String.Empty"/>, or is not of the correct format. This parameter is passed uninitialized;
        /// any value originally supplied in result will be overwritten.</param>
        /// <returns><c>true</c> if <paramref name="input"/> was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string input, out ITimeSpan timeSpan)
        {
            timeSpan = null;

            foreach (var parser in Parsers.Values)
            {
                if (ParsingUtilities.TryParse(input, parser, out timeSpan))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Converts the string representation of a time span to its <see cref="ITimeSpan"/> equivalent using
        /// the specified type of time span. A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="input">A string containing a time span to convert.</param>
        /// <param name="timeSpanType">the type of time span to convert <paramref name="input"/> to.</param>
        /// <param name="timeSpan">When this method returns, contains the <see cref="ITimeSpan"/>
        /// equivalent of the time span contained in <paramref name="input"/>, if the conversion succeeded, or
        /// <c>null</c> if the conversion failed. The conversion fails if the <paramref name="input"/> is <c>null</c> or
        /// <see cref="String.Empty"/>, or is not of the correct format. This parameter is passed uninitialized;
        /// any value originally supplied in result will be overwritten.</param>
        /// <returns><c>true</c> if <paramref name="input"/> was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string input, TimeSpanType timeSpanType, out ITimeSpan timeSpan)
        {
            return ParsingUtilities.TryParse(input, Parsers[timeSpanType], out timeSpan);
        }

        /// <summary>
        /// Converts the string representation of a time span to its <see cref="ITimeSpan"/> equivalent.
        /// </summary>
        /// <param name="input">A string containing a time span to convert.</param>
        /// <returns>A <see cref="ITimeSpan"/> equivalent to the time span contained in <paramref name="input"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="input"/> is <c>null</c> or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="input"/> has invalid format.</exception>
        public static ITimeSpan Parse(string input)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(input), input, "Input string");

            foreach (var parser in Parsers.Values)
            {
                ITimeSpan timeSpan;
                var parsingResult = parser(input, out timeSpan);

                if (parsingResult.Status == ParsingStatus.Parsed)
                    return timeSpan;
                else if (parsingResult.Status == ParsingStatus.FormatError)
                    throw parsingResult.Exception;
            }

            throw new FormatException("Time span has unknown format.");
        }

        /// <summary>
        /// Gets an object that represents maximum value of time span defined by the specified
        /// time span type.
        /// </summary>
        /// <param name="timeSpanType">The type of time span to get maximum value.</param>
        /// <returns>An object that represents maximum value of time span defined by <paramref name="timeSpanType"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="timeSpanType"/> specified an
        /// invalid value.</exception>
        public static ITimeSpan GetMaxTimeSpan(TimeSpanType timeSpanType)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(timeSpanType), timeSpanType);

            return MaximumTimeSpans[timeSpanType];
        }

        /// <summary>
        /// Gets an object that represents zero value of time span defined by the specified
        /// time span type.
        /// </summary>
        /// <param name="timeSpanType">The type of time span to get zero value.</param>
        /// <returns>An object that represents zero value of time span defined by <paramref name="timeSpanType"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="timeSpanType"/> specified an
        /// invalid value.</exception>
        public static ITimeSpan GetZeroTimeSpan(TimeSpanType timeSpanType)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(timeSpanType), timeSpanType);

            return ZeroTimeSpans[timeSpanType];
        }

        /// <summary>
        /// Gets an object that represents zero value of time span defined by the specified
        /// time span type.
        /// </summary>
        /// <typeparam name="TTimeSpan">The type of time span to get zero value.</typeparam>
        /// <returns>An object that represents zero value of time span defined by <typeparamref name="TTimeSpan"/>.</returns>
        public static TTimeSpan GetZeroTimeSpan<TTimeSpan>()
            where TTimeSpan : ITimeSpan
        {
            return (TTimeSpan)ZeroTimeSpans.Values.FirstOrDefault(timeSpan => timeSpan is TTimeSpan);
        }

        /// <summary>
        /// Gets a value indicating whether the specified time span is zero or not.
        /// </summary>
        /// <param name="timeSpan">Time span to check.</param>
        /// <returns><c>true</c> if the <paramref name="timeSpan"/> represents zero time span;
        /// otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeSpan"/> is <c>null</c>.</exception>
        public static bool IsZeroTimeSpan(this ITimeSpan timeSpan)
        {
            ThrowIfArgument.IsNull(nameof(timeSpan), timeSpan);

            var mathTimeSpan = timeSpan as MathTimeSpan;
            return mathTimeSpan == null
                ? ZeroTimeSpans.Values.Contains(timeSpan)
                : (mathTimeSpan.TimeSpan1.IsZeroTimeSpan() && mathTimeSpan.TimeSpan2.IsZeroTimeSpan());
        }

        internal static double Divide(ITimeSpan timeSpan1, ITimeSpan timeSpan2)
        {
            var metricTimeSpan = timeSpan1 as MetricTimeSpan;
            if (metricTimeSpan != null)
                return metricTimeSpan.Divide(timeSpan2 as MetricTimeSpan);

            var midiTimeSpan = timeSpan1 as MidiTimeSpan;
            if (midiTimeSpan != null)
                return midiTimeSpan.Divide(timeSpan2 as MidiTimeSpan);

            var musicalTimeSpan = timeSpan1 as MusicalTimeSpan;
            if (musicalTimeSpan != null)
                return musicalTimeSpan.Divide(timeSpan2 as MusicalTimeSpan);

            throw new NotSupportedException($"Dividing of time span of the '{timeSpan1.GetType()}' type is not supported.");
        }

        internal static ITimeSpan Add(ITimeSpan timeSpan1, ITimeSpan timeSpan2, TimeSpanMode mode)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(mode), mode);

            if (mode == TimeSpanMode.TimeTime)
                throw new ArgumentException("Times cannot be added.", nameof(mode));

            return new MathTimeSpan(timeSpan1, timeSpan2, MathOperation.Add, mode);
        }

        internal static ITimeSpan Subtract(ITimeSpan timeSpan1, ITimeSpan timeSpan2, TimeSpanMode mode)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(mode), mode);

            return new MathTimeSpan(timeSpan1, timeSpan2, MathOperation.Subtract, mode);
        }

        private static Parsing<ITimeSpan> GetParsing<TTimeSpan>(Parsing<TTimeSpan> parsing)
            where TTimeSpan : ITimeSpan
        {
            return (string input, out ITimeSpan timeSpan) =>
            {
                TTimeSpan result;
                var parsingResult = parsing(input, out result);
                timeSpan = result;
                return parsingResult;
            };
        }

        private static ITimeSpan Round<TTimeSpan>(
            ITimeSpan timeSpan,
            long x,
            long y,
            Func<long, TTimeSpan> createTimeSpan)
            where TTimeSpan : ITimeSpan
        {
            long reminder;
            var quotient = Math.DivRem(x, y, out reminder);
            return reminder == 0
                ? timeSpan.Clone()
                : createTimeSpan(quotient);
        }

        #endregion
    }
}
