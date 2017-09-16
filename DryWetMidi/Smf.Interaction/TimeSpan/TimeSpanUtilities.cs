using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class TimeSpanUtilities
    {
        #region Constants

        private static readonly Func<string, Tuple<ParsingResult, ITimeSpan>>[] Parsers = new Func<string, Tuple<ParsingResult, ITimeSpan>>[]
        {
            input => Tuple.Create(MathTimeSpanParser.TryParse(input, out var length), (ITimeSpan)length),
            input => Tuple.Create(MidiTimeSpanParser.TryParse(input, out var length), (ITimeSpan)length),
            input => Tuple.Create(MetricTimeSpanParser.TryParse(input, out var length), (ITimeSpan)length),
            input => Tuple.Create(MusicalTimeSpanParser.TryParse(input, out var length), (ITimeSpan)length),
        };

        #endregion

        #region Methods

        public static bool TryParse(string input, out ITimeSpan timeSpan)
        {
            timeSpan = null;

            if (MathTimeSpan.TryParse(input, out var mathTimeSpan))
                timeSpan = mathTimeSpan;
            else if (MidiTimeSpan.TryParse(input, out var midiTimeSpan))
                timeSpan = midiTimeSpan;
            else if (MetricTimeSpan.TryParse(input, out var metricTimeSpan))
                timeSpan = metricTimeSpan;
            else if (MusicalTimeSpan.TryParse(input, out var musicalTimeSpan))
                timeSpan = musicalTimeSpan;

            return timeSpan != null;
        }

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

        internal static ITimeSpan Add(ITimeSpan timeSpan1, ITimeSpan timeSpan2)
        {
            return new MathTimeSpan(timeSpan1, timeSpan2, MathOperation.Add);
        }

        internal static ITimeSpan Subtract(ITimeSpan timeSpan1, ITimeSpan timeSpan2)
        {
            return new MathTimeSpan(timeSpan1, timeSpan2, MathOperation.Subtract);
        }

        #endregion
    }
}
