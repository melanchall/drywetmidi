using System.Linq;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Composing
{
    internal sealed class AddChordAction : PatternAction
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

        #region Overrides

        public override PatternActionResult Invoke(long time, PatternContext context)
        {
            if (State == PatternActionState.Excluded)
                return PatternActionResult.DoNothing;

            context.SaveTime(time);

            var chordLength = LengthConverter.ConvertFrom(ChordDescriptor.Length, time, context.TempoMap);
            if (State == PatternActionState.Disabled)
                return new PatternActionResult(time + chordLength);

            return new PatternActionResult(time + chordLength,
                                           ChordDescriptor.Notes.Select(d => new Note(d.NoteNumber, chordLength, time)
                                           {
                                               Channel = context.Channel,
                                               Velocity = ChordDescriptor.Velocity
                                           }));
        }

        public override PatternAction Clone()
        {
            return new AddChordAction(new ChordDescriptor(ChordDescriptor.Notes, ChordDescriptor.Velocity, ChordDescriptor.Length.Clone()));
        }

        #endregion
    }
}
