namespace Melanchall.DryWetMidi.Interaction
{
    public sealed class NoteData
    {
        #region Constructor

        internal NoteData(TimedEvent timedNoteOnEvent, TimedEvent timedNoteOffEvent)
        {
            TimedNoteOnEvent = timedNoteOnEvent;
            TimedNoteOffEvent = timedNoteOffEvent;
        }

        #endregion

        #region Properties

        public TimedEvent TimedNoteOnEvent { get; }

        public TimedEvent TimedNoteOffEvent { get; }

        #endregion
    }
}
