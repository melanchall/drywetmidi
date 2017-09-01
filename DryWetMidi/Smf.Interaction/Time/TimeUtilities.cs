using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class TimeUtilities
    {
        #region Constants

        private static readonly Func<string, Tuple<ParsingResult, ITime>>[] Parsers = new Func<string, Tuple<ParsingResult, ITime>>[]
        {
            input => Tuple.Create(MidiTimeParser.TryParse(input, out var time), (ITime)time),
            input => Tuple.Create(MetricTimeParser.TryParse(input, out var time), (ITime)time),
            input => Tuple.Create(MusicalTimeParser.TryParse(input, out var time), (ITime)time),
        };

        #endregion

        #region Methods

        public static ITime Add(this ITime time, ILength length)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(length), length);

            return new MathTime(time, length, MathOperation.Add);
        }

        public static ITime Subtract(this ITime time, ILength length)
        {
            ThrowIfArgument.IsNull(nameof(time), time);
            ThrowIfArgument.IsNull(nameof(length), length);

            return new MathTime(time, length, MathOperation.Subtract);
        }

        public static bool TryParse(string input, out ITime time)
        {
            time = null;

            if (MidiTime.TryParse(input, out var midiTime))
                time = midiTime;
            else if (MetricTime.TryParse(input, out var metricTime))
                time = metricTime;
            else if (MusicalTime.TryParse(input, out var musicalTime))
                time = musicalTime;

            return time != null;
        }

        public static ITime Parse(string input)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(input), input, "Input string");

            foreach (var parser in Parsers)
            {
                var parsingResult = parser(input);

                var result = parsingResult.Item1;
                var time = parsingResult.Item2;

                if (result.Status == ParsingStatus.Parsed)
                    return time;
                else if (result.Status == ParsingStatus.FormatError)
                    throw result.Exception;
            }

            throw new FormatException("Time has unknown format.");
        }

        #endregion
    }
}
