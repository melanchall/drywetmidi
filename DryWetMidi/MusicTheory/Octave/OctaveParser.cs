using Melanchall.DryWetMidi.Common;
using System;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class OctaveParser : SimpleParser<Octave>
    {
        internal override Regex[] GetRegexes()
        {
            throw new System.NotImplementedException();
        }

        protected override Octave ParseInternal(ReadOnlySpan<char> input)
        {
            if (!int.TryParse(input, out var octaveNumber))
                ThrowInvalidFormatError();

            if (octaveNumber < Octave.MinOctaveNumber ||
                octaveNumber > Octave.MaxOctaveNumber)
                ThrowError("Octave number is out of range.");

            return Octave.Get(octaveNumber);
        }
    }
}
