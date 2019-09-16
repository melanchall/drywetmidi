using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Composing
{
    public sealed class ChordDescriptor
    {
        #region Constructor

        public ChordDescriptor(IEnumerable<MusicTheory.Note> notes, SevenBitNumber velocity, ITimeSpan length)
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

        #region Overrides

        public override string ToString()
        {
            return $"{string.Join(" ", Notes)} [{Velocity}]: {Length}";
        }

        #endregion
    }
}
