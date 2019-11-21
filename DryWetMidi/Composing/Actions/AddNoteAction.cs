using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Composing
{
    internal sealed class AddNoteAction : IPatternAction
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

        #region IPatternAction

        public PatternActionResult Invoke(long time, PatternContext context)
        {
            context.SaveTime(time);

            var noteLength = LengthConverter.ConvertFrom(NoteDescriptor.Length, time, context.TempoMap);

            var note = new Note(NoteDescriptor.Note.NoteNumber, noteLength, time)
            {
                Channel = context.Channel,
                Velocity = NoteDescriptor.Velocity
            };

            return new PatternActionResult(time + noteLength, new[] { note });
        }

        #endregion
    }
}
