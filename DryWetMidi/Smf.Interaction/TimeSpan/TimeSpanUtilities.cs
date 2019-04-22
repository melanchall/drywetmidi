using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Provides useful utilities for working with <see cref="ITimeSpan"/>.
    /// </summary>
    public static class TimeSpanUtilities
    {
        #region Delegates

        private delegate Tuple<ParsingResult, ITimeSpan> Parser(string input);

        #endregion

        #region Constants

        private static readonly Dictionary<TimeSpanType, Parser> Parsers = new Dictionary<TimeSpanType, Parser>
            {
                [TimeSpanType.Midi] = input =>
                {
                    MidiTimeSpan timeSpan;
                    return Tuple.Create(MidiTimeSpanParser.TryParse(input, out timeSpan), (ITimeSpan)timeSpan);
                },
                [TimeSpanType.BarBeat] = input =>
                {
                    BarBeatTimeSpan timeSpan;
                    return Tuple.Create(BarBeatTimeSpanParser.TryParse(input, out timeSpan), (ITimeSpan)timeSpan);
                },
                [TimeSpanType.Metric] = input =>
                {
                    MetricTimeSpan timeSpan;
                    return Tuple.Create(MetricTimeSpanParser.TryParse(input, out timeSpan), (ITimeSpan)timeSpan);
                },
                [TimeSpanType.Musical] = input =>
                {
                    MusicalTimeSpan timeSpan;
                    return Tuple.Create(MusicalTimeSpanParser.TryParse(input, out timeSpan), (ITimeSpan)timeSpan);
                }
            };

        private static readonly Dictionary<TimeSpanType, ITimeSpan> MaximumTimeSpans = new Dictionary<TimeSpanType, ITimeSpan>
        {
            [TimeSpanType.Midi] = new MidiTimeSpan(long.MaxValue),
            [TimeSpanType.Metric] = new MetricTimeSpan(TimeSpan.MaxValue),
            [TimeSpanType.Musical] = new MusicalTimeSpan(long.MaxValue, 1),
            [TimeSpanType.BarBeat] = new BarBeatTimeSpan(long.MaxValue, long.MaxValue, long.MaxValue)
        };

        private static readonly Dictionary<TimeSpanType, ITimeSpan> ZeroTimeSpans = new Dictionary<TimeSpanType, ITimeSpan>
        {
            [TimeSpanType.Midi] = new MidiTimeSpan(),
            [TimeSpanType.Metric] = new MetricTimeSpan(),
            [TimeSpanType.Musical] = new MusicalTimeSpan(),
            [TimeSpanType.BarBeat] = new BarBeatTimeSpan()
        };

        #endregion

        #region Methods

        /// <summary>
        /// Converts the string representation of a time span to its <see cref="ITimeSpan"/> equivalent.
        /// A return value indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="input">A string containing a time span to convert.</param>
        /// <param name="timeSpan">When this method returns, contains the <see cref="ITimeSpan"/>
        /// equivalent of the time span contained in <paramref name="input"/>, if the conversion succeeded, or
        /// null if the conversion failed. The conversion fails if the <paramref name="input"/> is null or
        /// <see cref="String.Empty"/>, or is not of the correct format. This parameter is passed uninitialized;
        /// any value originally supplied in result will be overwritten.</param>
        /// <returns>true if <paramref name="input"/> was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string input, out ITimeSpan timeSpan)
        {
            timeSpan = null;

            foreach (var parser in Parsers.Values)
            {
                if (TryParse(input, parser, out timeSpan))
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
        /// null if the conversion failed. The conversion fails if the <paramref name="input"/> is null or
        /// <see cref="String.Empty"/>, or is not of the correct format. This parameter is passed uninitialized;
        /// any value originally supplied in result will be overwritten.</param>
        /// <returns>true if <paramref name="input"/> was converted successfully; otherwise, false.</returns>
        public static bool TryParse(string input, TimeSpanType timeSpanType, out ITimeSpan timeSpan)
        {
            return TryParse(input, Parsers[timeSpanType], out timeSpan);
        }

        /// <summary>
        /// Converts the string representation of a time span to its <see cref="ITimeSpan"/> equivalent.
        /// </summary>
        /// <param name="input">A string containing a time span to convert.</param>
        /// <returns>A <see cref="ITimeSpan"/> equivalent to the time span contained in <paramref name="input"/>.</returns>
        /// <exception cref="ArgumentException"><paramref name="input"/> is null or contains white-spaces only.</exception>
        /// <exception cref="FormatException"><paramref name="input"/> has invalid format.</exception>
        public static ITimeSpan Parse(string input)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(input), input, "Input string");

            foreach (var parser in Parsers.Values)
            {
                var parsingResult = parser(input);

                var result = parsingResult.Item1;
                var timeSpan = parsingResult.Item2;

                if (result.Status == ParsingStatus.Parsed)
                    return timeSpan;
                else if (result.Status == ParsingStatus.FormatError)
                    throw result.Exception;
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

        private static bool TryParse(string input, Parser parser, out ITimeSpan timeSpan)
        {
            timeSpan = null;

            var parsingResult = parser(input);
            if (parsingResult.Item1.Status == ParsingStatus.Parsed)
            {
                timeSpan = parsingResult.Item2;
                return true;
            }

            return false;
        }

        #endregion
    }
}
