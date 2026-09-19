using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class OctaveParser : SimpleParser<Octave>
    {
        internal override bool TryParseInternal(ReadOnlySpan<char> input, out Octave result, out string? error)
        {
            if (!int.TryParse(input, out var octaveNumber))
            {
                error = "Input string has invalid format.";
                result = default!;
                return false;
            }

            if (octaveNumber < Octave.MinOctaveNumber ||
                octaveNumber > Octave.MaxOctaveNumber)
            {
                error = "Octave number is out of range.";
                result = default!;
                return false;
            }

            result = Octave.Get(octaveNumber);
            error = null;
            return true;
        }
    }
}
