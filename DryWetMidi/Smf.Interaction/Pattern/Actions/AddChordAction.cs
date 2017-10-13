using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class AddChordAction : IPatternAction
    {
        #region Constructor

        public AddChordAction(IEnumerable<NoteDefinition> noteDefinitions, SevenBitNumber velocity, ITimeSpan length)
        {
            NoteDefinitions = noteDefinitions;
            Velocity = velocity;
            Length = length;
        }

        #endregion

        #region Properties

        public IEnumerable<NoteDefinition> NoteDefinitions { get; }

        public SevenBitNumber Velocity { get; }

        public ITimeSpan Length { get; }

        #endregion

        #region IPatternAction

        public PatternActionResult Invoke(long time, PatternContext context)
        {
            context.SaveTime(time);

            var chordLength = LengthConverter.ConvertFrom(Length, time, context.TempoMap);

            return new PatternActionResult(time + chordLength,
                                           NoteDefinitions.Select(d => new Note(d.NoteNumber, chordLength, time)
                                           {
                                               Channel = context.Channel,
                                               Velocity = Velocity
                                           }));
        }

        #endregion
    }
}
