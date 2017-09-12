using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public static class LengthUtilities
    {
        #region Constants

        private static readonly Func<string, Tuple<ParsingResult, ILength>>[] Parsers = new Func<string, Tuple<ParsingResult, ILength>>[]
        {
            input => Tuple.Create(MathLengthParser.TryParse(input, out var length), (ILength)length),
            input => Tuple.Create(MidiLengthParser.TryParse(input, out var length), (ILength)length),
            input => Tuple.Create(MetricLengthParser.TryParse(input, out var length), (ILength)length),
            input => Tuple.Create(MusicalLengthParser.TryParse(input, out var length), (ILength)length),
        };

        #endregion

        #region Methods

        public static ITime ToTime(this ILength length)
        {
            ThrowIfArgument.IsNull(nameof(length), length);

            return ((MidiTime)0).Add(length);
        }

        public static bool TryParse(string input, out ILength length)
        {
            length = null;

            if (MathLength.TryParse(input, out var mathLength))
                length = mathLength;
            else if (MidiLength.TryParse(input, out var midiLength))
                length = midiLength;
            else if (MetricLength.TryParse(input, out var metricLength))
                length = metricLength;
            else if (MusicalLength.TryParse(input, out var musicalLength))
                length = musicalLength;

            return length != null;
        }

        public static ILength Parse(string input)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(input), input, "Input string");

            foreach (var parser in Parsers)
            {
                var parsingResult = parser(input);

                var result = parsingResult.Item1;
                var length = parsingResult.Item2;

                if (result.Status == ParsingStatus.Parsed)
                    return length;
                else if (result.Status == ParsingStatus.FormatError)
                    throw result.Exception;
            }

            throw new FormatException("Length has unknown format.");
        }

        internal static ILength Add(ILength length1, ILength length2)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);

            return new MathLength(length1, length2, MathOperation.Add);
        }

        internal static ILength Subtract(ILength length1, ILength length2)
        {
            ThrowIfArgument.IsNull(nameof(length1), length1);
            ThrowIfArgument.IsNull(nameof(length2), length2);

            return new MathLength(length1, length2, MathOperation.Subtract);
        }

        #endregion
    }
}
