using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Composing
{
    internal sealed class AddNoteAction : PatternAction
    {
        #region Constructor

        public AddNoteAction(NoteDescriptor noteDescriptor)
        {
            NoteDescriptor = noteDescriptor;
        }

        #endregion

        #region Properties

        public NoteDescriptor NoteDescriptor { get; }

        #endregion

        #region Overrides

        public override PatternActionResult Invoke(long time, PatternContext context)
        {
            if (State == PatternActionState.Excluded)
                return PatternActionResult.DoNothing;

            context.SaveTime(time);

            var noteLength = LengthConverter.ConvertFrom(NoteDescriptor.Length, time, context.TempoMap);
            if (State == PatternActionState.Disabled)
                return new PatternActionResult(time + noteLength);

            var note = new Note(NoteDescriptor.Note.NoteNumber, noteLength, time)
            {
                Channel = context.Channel,
                Velocity = NoteDescriptor.Velocity
            };

            return new PatternActionResult(time + noteLength, new[] { note });
        }

        public override PatternAction Clone()
        {
            return new AddNoteAction(new NoteDescriptor(NoteDescriptor.Note, NoteDescriptor.Velocity, NoteDescriptor.Length.Clone()));
        }

        #endregion
    }
}
