using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class AddNoteAction : IPatternAction
    {
        #region Constructor

        public AddNoteAction(MusicTheory.Note note, SevenBitNumber velocity, ITimeSpan length)
        {
            Note = note;
            Velocity = velocity;
            Length = length;
        }

        #endregion

        #region Properties

        public MusicTheory.Note Note { get; }

        public SevenBitNumber Velocity { get; }

        public ITimeSpan Length { get; }

        #endregion

        #region IPatternAction

        public PatternActionResult Invoke(long time, PatternContext context)
        {
            context.SaveTime(time);

            var noteLength = LengthConverter.ConvertFrom(Length, time, context.TempoMap);

            var note = new Note(Note.NoteNumber, noteLength, time)
            {
                Channel = context.Channel,
                Velocity = Velocity
            };

            return new PatternActionResult(time + noteLength, new[] { note });
        }

        #endregion
    }
}
