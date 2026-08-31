using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal sealed class OctaveParser : SimpleParser<Octave>
    {
        #region Constants

        private const string OctaveNumberGroupName = "o";

        private static readonly string OctaveNumberGroup = GetIntegerNumberGroup(OctaveNumberGroupName);

        private static readonly string[] Patterns = new[]
        {
            OctaveNumberGroup
        };

        private const string OctaveIsOutOfRange = "Octave number is out of range.";

        #endregion

        #region Methods

        protected override Octave ParseInternal(string input)
        {
            var match = Match(input, Patterns);
            if (match == null)
                ThrowInvalidFormatError();

            if (!ParseInt(match, OctaveNumberGroupName, Octave.Middle.Number, out var octaveNumber) ||
                octaveNumber < Octave.MinOctaveNumber ||
                octaveNumber > Octave.MaxOctaveNumber)
                ThrowError(OctaveIsOutOfRange);

            return Octave.Get(octaveNumber);
        }

        #endregion
    }
}
