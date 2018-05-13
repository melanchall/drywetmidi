using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Provides useful utilities for working with <see cref="ITimeSpan"/>.
    /// </summary>
    public static class TimeSpanUtilities
    {
        #region Constants

        private static readonly Func<string, Tuple<ParsingResult, ITimeSpan>>[] Parsers = new Func<string, Tuple<ParsingResult, ITimeSpan>>[]
        {
            input =>
            {
                MidiTimeSpan timeSpan;
                return Tuple.Create(MidiTimeSpanParser.TryParse(input, out timeSpan), (ITimeSpan)timeSpan);
            },
            input =>
            {
                BarBeatTimeSpan timeSpan;
                return Tuple.Create(BarBeatTimeSpanParser.TryParse(input, out timeSpan), (ITimeSpan)timeSpan);
            },
            input =>
            {
                MetricTimeSpan timeSpan;
                return Tuple.Create(MetricTimeSpanParser.TryParse(input, out timeSpan), (ITimeSpan)timeSpan);
            },
            input =>
            {
                MusicalTimeSpan timeSpan;
                return Tuple.Create(MusicalTimeSpanParser.TryParse(input, out timeSpan), (ITimeSpan)timeSpan);
            },
        };

        private static readonly Dictionary<TimeSpanType, ITimeSpan> MaximumTimeSpans = new Dictionary<TimeSpanType, ITimeSpan>
        {
            [TimeSpanType.Midi] = new MidiTimeSpan(long.MaxValue),
            [TimeSpanType.Metric] = new MetricTimeSpan(TimeSpan.MaxValue),
            [TimeSpanType.Musical] = new MusicalTimeSpan(long.MaxValue, 1),
            [TimeSpanType.BarBeat] = new BarBeatTimeSpan(long.MaxValue, long.MaxValue, long.MaxValue)
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

            foreach (var parser in Parsers)
            {
                var parsingResult = parser(input);
                if (parsingResult.Item1.Status == ParsingStatus.Parsed)
                {
                    timeSpan = parsingResult.Item2;
                    return true;
                }
            }

            return timeSpan != null;
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

            foreach (var parser in Parsers)
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

        public static ITimeSpan GetMaxTimeSpan(TimeSpanType timeSpanType)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(timeSpanType), timeSpanType);

            return MaximumTimeSpans[timeSpanType];
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

        #endregion
    }
}
