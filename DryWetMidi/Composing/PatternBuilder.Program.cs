using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Standards;
using System.ComponentModel;

namespace Melanchall.DryWetMidi.Composing
{
    public sealed partial class PatternBuilder
    {
        #region Methods

        /// <summary>
        /// Inserts <see cref="ProgramChangeEvent"/> to specify an instrument that will be used by following notes.
        /// </summary>
        /// <param name="programNumber">The number of a MIDI program.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        public PatternBuilder ProgramChange(SevenBitNumber programNumber)
        {
            return AddAction(new SetProgramNumberAction(programNumber));
        }

        /// <summary>
        /// Inserts <see cref="ProgramChangeEvent"/> to specify an instrument that will be used by following notes.
        /// </summary>
        /// <param name="program">The General MIDI Level 1 program.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="program"/> specified an invalid value.</exception>
        public PatternBuilder ProgramChange(GeneralMidiProgram program)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(program), program);

            return AddAction(new SetGeneralMidiProgramAction(program));
        }

        /// <summary>
        /// Inserts <see cref="ProgramChangeEvent"/> to specify an instrument that will be used by following notes.
        /// </summary>
        /// <param name="program">The General MIDI Level 2 program.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="program"/> specified an invalid value.</exception>
        public PatternBuilder ProgramChange(GeneralMidi2Program program)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(program), program);

            return AddAction(new SetGeneralMidi2ProgramAction(program));
        }

        #endregion
    }
}
