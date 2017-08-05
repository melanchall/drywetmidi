namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class AddNoteAction : IPatternAction
    {
        #region Constructor

        public AddNoteAction(NoteDefinition noteDefinition, SevenBitNumber velocity, ILength length)
        {
            NoteDefinition = noteDefinition;
            Velocity = velocity;
            Length = length;
        }

        #endregion

        #region Properties

        public NoteDefinition NoteDefinition { get; }

        public SevenBitNumber Velocity { get; }

        public ILength Length { get; }

        #endregion

        #region IPatternAction

        public PatternActionResult Invoke(long time, PatternContext context)
        {
            context.SaveTime(time);

            var noteLength = LengthConverter.ConvertFrom(Length, time, context.TempoMap);

            var note = new Note(NoteDefinition.NoteNumber, noteLength, time)
            {
                Channel = context.Channel,
                Velocity = Velocity
            };

            return new PatternActionResult(time + noteLength, new[] { note });
        }

        #endregion
    }
}
