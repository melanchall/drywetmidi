using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    internal static class OctaveParser
    {
        #region Constants

        private const string OctaveNumberGroupName = "o";

        private static readonly string OctaveNumberGroup = ParsingUtilities.GetIntegerNumberGroup(OctaveNumberGroupName);

        private static readonly string[] Patterns = new[]
        {
            OctaveNumberGroup
        };

        private const string OctaveIsOutOfRange = "Octave number is out of range.";

        #endregion

        #region Methods

        internal static ParsingResult TryParse(string input, out Octave octave)
        {
            octave = null;

            if (string.IsNullOrWhiteSpace(input))
                return ParsingResult.EmptyInputString;

            var match = ParsingUtilities.Match(input, Patterns);
            if (match == null)
                return ParsingResult.NotMatched;

            int octaveNumber;
            if (!ParsingUtilities.ParseInt(match, OctaveNumberGroupName, Octave.Middle.Number, out octaveNumber) ||
                octaveNumber < Octave.MinOctaveNumber ||
                octaveNumber > Octave.MaxOctaveNumber)
                return ParsingResult.Error(OctaveIsOutOfRange);

            octave = Octave.Get(octaveNumber);
            return ParsingResult.Parsed;
        }

        #endregion
    }
}
