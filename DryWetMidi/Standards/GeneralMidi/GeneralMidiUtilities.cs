using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Standards
{
    public static class GeneralMidiUtilities
    {
        #region Methods

        public static SevenBitNumber AsSevenBitNumber(this GeneralMidiProgram program)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(program), program);

            return (SevenBitNumber)(byte)program;
        }

        public static SevenBitNumber AsSevenBitNumber(this GeneralMidiPercussion percussion)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return (SevenBitNumber)(byte)percussion;
        }

        public static MidiEvent GetProgramEvent(this GeneralMidiProgram program)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(program), program);

            return new ProgramChangeEvent(program.AsSevenBitNumber());
        }

        public static NoteOnEvent GetNoteOnEvent(this GeneralMidiPercussion percussion, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOnEvent(percussion.AsSevenBitNumber(), velocity);
        }

        public static NoteOffEvent GetNoteOffEvent(this GeneralMidiPercussion percussion, SevenBitNumber velocity)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOffEvent(percussion.AsSevenBitNumber(), velocity);
        }

        #endregion
    }
}
