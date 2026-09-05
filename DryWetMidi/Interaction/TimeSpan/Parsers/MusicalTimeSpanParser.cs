using Melanchall.DryWetMidi.Common;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class MusicalTimeSpanParser : SimpleParser<MusicalTimeSpan>
    {
        #region Constants

        private static readonly Dictionary<char, (int, int)> Fractions = new Dictionary<char, (int, int)>
        {
            ['w'] = (1, 1),
            ['h'] = (1, 2),
            ['q'] = (1, 4),
            ['e'] = (1, 8),
            ['s'] = (1, 16),
        };

        private static readonly Dictionary<char, (int TupletNotesCount, int TupletSpaceSize)> Tuplets = new Dictionary<char, (int, int)>
        {
            ['t'] = (3, 2),
            ['d'] = (2, 3),
        };

        #endregion

        #region Methods

        protected override MusicalTimeSpan ParseInternal(ReadOnlySpan<char> input)
        {
            (int, int) fraction;
            var numerator = 1L;
            var denominator = 0L;

            if (!Fractions.TryGetValue(input[0], out fraction))
            {
                var dividerIndex = input.IndexOf('/');
                if (dividerIndex < 0)
                    ThrowInvalidFormatError();

                if (dividerIndex > 0 && !long.TryParse(input.Slice(0, dividerIndex), out numerator))
                    ThrowInvalidFormatError();

                var i = dividerIndex + 1;

                for (; i < input.Length && char.IsDigit(input[i]); i++) { }

                if (!long.TryParse(input.Slice(dividerIndex + 1, i - dividerIndex - 1), out denominator))
                    ThrowInvalidFormatError();

                input = input.Slice(i).Trim();
            }
            else
            {
                numerator = fraction.Item1;
                denominator = fraction.Item2;
                input = input.Slice(1).Trim();
            }

            //

            if (input.IsEmpty)
                return new MusicalTimeSpan(numerator, denominator);

            //

            var tupletNotesCount = 1;
            var tupletSpaceSize = 1;

            if (input[0] == '[')
            {
                var endIndex = input.IndexOf(']');
                if (endIndex < 0 || endIndex == 1)
                    ThrowInvalidFormatError();

                var tupletSpan = input.Slice(1, endIndex - 1).Trim();
                var tupletDividerIndex = tupletSpan.IndexOf(':');
                if (tupletDividerIndex < 0)
                    ThrowInvalidFormatError();

                if (!int.TryParse(tupletSpan.Slice(0, tupletDividerIndex).Trim(), out tupletNotesCount) || tupletNotesCount < 1)
                    ThrowInvalidFormatError();

                if (!int.TryParse(tupletSpan.Slice(tupletDividerIndex + 1).Trim(), out tupletSpaceSize) || tupletSpaceSize < 1)
                    ThrowInvalidFormatError();

                input = input.Slice(endIndex + 1).Trim();
            }
            else if (Tuplets.TryGetValue(input[0], out var tuplet))
            {
                tupletNotesCount = tuplet.Item1;
                tupletSpaceSize = tuplet.Item2;
                input = input.Slice(1).Trim();
            }

            //

            if (input.IsEmpty)
                return new MusicalTimeSpan(numerator, denominator).Tuplet(tupletNotesCount, tupletSpaceSize);

            //

            var dotsCount = 0;
            while (dotsCount < input.Length && input[dotsCount] == '.')
            {
                dotsCount++;
            }

            //

            if (dotsCount < input.Length)
                ThrowInvalidFormatError();

            //

            return new MusicalTimeSpan(numerator, denominator).Dotted(dotsCount).Tuplet(tupletNotesCount, tupletSpaceSize);
        }

        #endregion
    }
}
