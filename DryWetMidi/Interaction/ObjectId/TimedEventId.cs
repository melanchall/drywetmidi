using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class TimedEventId
    {
        #region Constructor

        public TimedEventId(MidiEventType eventType)
        {
            EventType = eventType;
        }

        #endregion

        #region Properties

        public MidiEventType EventType { get; }

        #endregion

        #region Overrides

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            var timedEventId = obj as TimedEventId;
            if (ReferenceEquals(timedEventId, null))
                return false;

            return EventType == timedEventId.EventType;
        }

        public override int GetHashCode()
        {
            return (int)EventType;
        }

        #endregion
    }
}
