using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class TimeUtilities
    {
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
            return ParseMidiTime(input) ?? ((ITime)ParseMetricTime(input) ?? ParseMusicalTime(input));
        }

        private static MidiTime ParseMidiTime(string input)
        {
            var parsingResult = MidiTimeParser.TryParse(input, out var time);
            if (parsingResult == MidiTimeParser.ParsingResult.Parsed)
                return time;
            else if (parsingResult != MidiTimeParser.ParsingResult.NotMatched)
                throw MidiTimeParser.GetException(parsingResult, nameof(input));

            return null;
        }

        private static MetricTime ParseMetricTime(string input)
        {
            var parsingResult = MetricTimeParser.TryParse(input, out var time);
            if (parsingResult == MetricTimeParser.ParsingResult.Parsed)
                return time;
            else if (parsingResult != MetricTimeParser.ParsingResult.NotMatched)
                throw MetricTimeParser.GetException(parsingResult, nameof(input));

            return null;
        }

        private static MusicalTime ParseMusicalTime(string input)
        {
            var parsingResult = MusicalTimeParser.TryParse(input, out var time);
            if (parsingResult == MusicalTimeParser.ParsingResult.Parsed)
                return time;
            else if (parsingResult != MusicalTimeParser.ParsingResult.NotMatched)
                throw MusicalTimeParser.GetException(parsingResult, nameof(input));

            return null;
        }

        #endregion
    }
}
