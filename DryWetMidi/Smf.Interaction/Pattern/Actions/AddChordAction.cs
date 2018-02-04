using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class AddChordAction : IPatternAction
    {
        #region Constructor

        public AddChordAction(IEnumerable<MusicTheory.Note> notes, SevenBitNumber velocity, ITimeSpan length)
        {
            Notes = notes;
            Velocity = velocity;
            Length = length;
        }

        #endregion

        #region Properties

        public IEnumerable<MusicTheory.Note> Notes { get; }

        public SevenBitNumber Velocity { get; }

        public ITimeSpan Length { get; }

        #endregion

        #region IPatternAction

        public PatternActionResult Invoke(long time, PatternContext context)
        {
            context.SaveTime(time);

            var chordLength = LengthConverter.ConvertFrom(Length, time, context.TempoMap);

            return new PatternActionResult(time + chordLength,
                                           Notes.Select(d => new Note(d.NoteNumber, chordLength, time)
                                           {
                                               Channel = context.Channel,
                                               Velocity = Velocity
                                           }));
        }

        #endregion
    }
}
