using System.Linq;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Composing
{
    internal sealed class AddChordAction : IPatternAction
    {
        #region Constructor

        public AddChordAction(ChordDescriptor chordDescriptor)
        {
            ChordDescriptor = chordDescriptor;
        }

        #endregion

        #region Properties

        public ChordDescriptor ChordDescriptor { get; }

        #endregion

        #region IPatternAction

        public PatternActionResult Invoke(long time, PatternContext context)
        {
            context.SaveTime(time);

            var chordLength = LengthConverter.ConvertFrom(ChordDescriptor.Length, time, context.TempoMap);

            return new PatternActionResult(time + chordLength,
                                           ChordDescriptor.Notes.Select(d => new Note(d.NoteNumber, chordLength, time)
                                           {
                                               Channel = context.Channel,
                                               Velocity = ChordDescriptor.Velocity
                                           }));
        }

        #endregion
    }
}
