using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal sealed class PatternActionResult
    {
        #region Constants

        public static readonly PatternActionResult DoNothing = new PatternActionResult();

        #endregion

        #region Constructor

        public PatternActionResult()
        {
        }

        public PatternActionResult(long? time)
            : this(time, null, null)
        {
        }

        public PatternActionResult(long? time, IEnumerable<Note> notes)
            : this(time, notes, null)
        {
        }

        public PatternActionResult(long? time, IEnumerable<TimedEvent> events)
            : this(time, null, events)
        {
        }

        public PatternActionResult(long? time, IEnumerable<Note> notes, IEnumerable<TimedEvent> events)
        {
            Time = time;
            Notes = notes;
            Events = events;
        }

        #endregion

        #region Properties

        public long? Time { get; }

        public IEnumerable<Note> Notes { get; }

        public IEnumerable<TimedEvent> Events { get; }

        #endregion
    }
}
