using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    internal sealed class NoteInfo
    {
        #region Constructor

        public NoteInfo(NoteName noteName, int octave, ITimeSpan time, ITimeSpan length)
            : this(noteName, octave, time, length, DryWetMidi.Interaction.Note.DefaultVelocity)
        {
        }

        public NoteInfo(NoteName noteName, int octave, ITimeSpan time, ITimeSpan length, SevenBitNumber velocity)
            : this(NoteUtilities.GetNoteNumber(noteName, octave), time, length, velocity)
        {
        }

        public NoteInfo(SevenBitNumber noteNumber, ITimeSpan time, ITimeSpan length, SevenBitNumber velocity)
        {
            NoteNumber = noteNumber;
            Time = time;
            Length = length;
            Velocity = velocity;
        }

        #endregion

        #region Properties

        public SevenBitNumber NoteNumber { get; }

        public ITimeSpan Time { get; }

        public ITimeSpan Length { get; }

        public SevenBitNumber Velocity { get; }

        #endregion
    }
}
