using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Standards
{
    public static class GeneralMidiUtilities
    {
        #region Methods

        public static SevenBitNumber AsSevenBitNumber(this GeneralMidi.Program program)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(program), program);

            return (SevenBitNumber)(byte)program;
        }

        public static SevenBitNumber AsSevenBitNumber(this GeneralMidi.Percussion percussion)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return (SevenBitNumber)(byte)percussion;
        }

        public static ProgramChangeEvent GetProgramChangeEvent(this GeneralMidi.Program program)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(program), program);

            return new ProgramChangeEvent(program.AsSevenBitNumber());
        }

        public static NoteOnEvent GetNoteOnEvent(this GeneralMidi.Percussion percussion, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOnEvent(percussion.AsSevenBitNumber(), velocity);
        }

        public static NoteOffEvent GetNoteOffEvent(this GeneralMidi.Percussion percussion, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOffEvent(percussion.AsSevenBitNumber(), velocity);
        }

        #endregion
    }
}
