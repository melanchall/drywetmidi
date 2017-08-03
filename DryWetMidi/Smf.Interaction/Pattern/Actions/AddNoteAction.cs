namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class AddNoteAction : IPatternAction
    {
        #region Constructor

        public AddNoteAction(NoteDefinition noteDefinition, ILength length)
        {
            NoteDefinition = noteDefinition;
            Length = length;
        }

        #endregion

        #region Properties

        public NoteDefinition NoteDefinition { get; }

        public ILength Length { get; }

        #endregion

        #region IPatternAction

        public PatternActionResult Invoke(long time, PatternContext context)
        {
            context.SaveTime(time);

            var noteLength = LengthConverter.ConvertFrom(Length, time, context.TempoMap);

            var note = new Note(NoteDefinition.NoteNumber, noteLength, time);
            note.Channel = context.Channel;

            return new PatternActionResult(time + noteLength, new[] { note });
        }

        #endregion
    }
}
