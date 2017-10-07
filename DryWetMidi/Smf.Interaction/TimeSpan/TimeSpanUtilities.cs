using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class TimeSpanUtilities
    {
        #region Constants

        private static readonly Func<string, Tuple<ParsingResult, ITimeSpan>>[] Parsers = new Func<string, Tuple<ParsingResult, ITimeSpan>>[]
        {
            input => Tuple.Create(MidiTimeSpanParser.TryParse(input, out var timeSpan), (ITimeSpan)timeSpan),
            input => Tuple.Create(BarBeatTimeSpanParser.TryParse(input, out var timeSpan), (ITimeSpan)timeSpan),
            input => Tuple.Create(MetricTimeSpanParser.TryParse(input, out var timeSpan), (ITimeSpan)timeSpan),
            input => Tuple.Create(MusicalTimeSpanParser.TryParse(input, out var timeSpan), (ITimeSpan)timeSpan),
        };

        #endregion

        #region Methods

        public static bool TryParse(string input, out ITimeSpan timeSpan)
        {
            timeSpan = null;

            if (MidiTimeSpan.TryParse(input, out var midiTimeSpan))
                timeSpan = midiTimeSpan;
            else if (BarBeatTimeSpan.TryParse(input, out var barBeatTimeSpan))
                timeSpan = barBeatTimeSpan;
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
