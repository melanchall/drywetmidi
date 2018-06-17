using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal static class TimedEventEquality
    {
        #region Nested classes

        private sealed class TimedEventComparer : IEqualityComparer<TimedEvent>
        {
            #region Fields

            private readonly bool _compareDeltaTimes;

            #endregion

            #region Constructor

            public TimedEventComparer(bool compareDeltaTimes)
            {
                _compareDeltaTimes = compareDeltaTimes;
            }

            #endregion

            #region IEqualityComparer<Note>

            public bool Equals(TimedEvent timedEvent1, TimedEvent timedEvent2)
            {
                if (ReferenceEquals(timedEvent1, timedEvent2))
                    return true;

                if (ReferenceEquals(null, timedEvent1) || ReferenceEquals(null, timedEvent2))
                    return false;

                return timedEvent1.Time == timedEvent2.Time &&
                       MidiEventEquality.AreEqual(timedEvent1.Event, timedEvent2.Event, _compareDeltaTimes);
            }

            public int GetHashCode(TimedEvent timedEvent)
            {
                return timedEvent.Time.GetHashCode() ^ timedEvent.Event.GetHashCode();
            }

            #endregion
        }

        #endregion

        #region Methods

        public static bool AreEqual(TimedEvent timedEvent1, TimedEvent timedEvent2, bool compareDeltaTimes)
        {
            return new TimedEventComparer(compareDeltaTimes).Equals(timedEvent1, timedEvent2);
        }

        public static bool AreEqual(IEnumerable<TimedEvent> timedEvents1, IEnumerable<TimedEvent> timedEvents2, bool compareDeltaTimes)
        {
            if (ReferenceEquals(timedEvents1, timedEvents2))
                return true;

            if (ReferenceEquals(null, timedEvents1) || ReferenceEquals(null, timedEvents2))
                return false;

            return timedEvents1.SequenceEqual(timedEvents2, new TimedEventComparer(compareDeltaTimes));
        }

        #endregion
    }
}
