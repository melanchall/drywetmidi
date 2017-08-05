using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class PatternActionResult
    {
        #region Constructor

        public static readonly PatternActionResult DoNothing = new PatternActionResult();

        #endregion

        #region Constructor

        public PatternActionResult()
        {
        }

        public PatternActionResult(long? time)
            : this(time, null)
        {
        }

        public PatternActionResult(long? time, IEnumerable<Note> notes)
        {
            Time = time;
            Notes = notes;
        }

        #endregion

        #region Properties

        public long? Time { get; }

        public IEnumerable<Note> Notes { get; }

        #endregion
    }
}
