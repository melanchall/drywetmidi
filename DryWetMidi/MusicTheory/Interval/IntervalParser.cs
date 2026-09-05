using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class IntervalParser : SimpleParser<Interval>
    {
        public (Interval? Interval, int Length) TryReadInterval(ReadOnlySpan<char> input)
        {
            var endIndex = 1;

            if (input[0] == '+' || input[0] == '-' || char.IsDigit(input[0]))
            {
                for (; endIndex < input.Length; endIndex++)
                {
                    if (!char.IsDigit(input[endIndex]))
                        break;
                }

                if (!int.TryParse(input.Slice(0, endIndex), out var halfSteps))
                    return (null, 0);

                if (!IntervalUtilities.IsIntervalValid(halfSteps))
                    return (null, 0);

                return (Interval.FromHalfSteps(halfSteps), endIndex);
            }

            IntervalQuality intervalQuality = default;
            var qualityLetter = input[0];
            if (qualityLetter == 'p' || qualityLetter == 'P')
                intervalQuality = IntervalQuality.Perfect;
            else if (qualityLetter == 'm')
                intervalQuality = IntervalQuality.Minor;
            else if (qualityLetter == 'M')
                intervalQuality = IntervalQuality.Major;
            else if (qualityLetter == 'd' || qualityLetter == 'D')
                intervalQuality = IntervalQuality.Diminished;
            else if (qualityLetter == 'a' || qualityLetter == 'A')
                intervalQuality = IntervalQuality.Augmented;
            else
                return (null, 0);

            for (; endIndex < input.Length; endIndex++)
            {
                if (!char.IsDigit(input[endIndex]))
                    break;
            }

            if (!int.TryParse(input.Slice(1, endIndex - 1), out var intervalNumber))
                return (null, 0);

            return (Interval.Get(intervalQuality, intervalNumber), endIndex);
        }

        protected override Interval ParseInternal(ReadOnlySpan<char> input)
        {
            var (interval, length) = TryReadInterval(input);
            if (interval == null || length != input.Length)
                ThrowInvalidFormatError();

            return interval;
        }
    }
}
