using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace Melanchall.DryWetMidi.Tests.Utilities
{
    internal static class TimedEventEquality
    {
        #region Nested classes

        private sealed class TimedEventComparer : IEqualityComparer<TimedEvent>
        {
            #region Fields

            private readonly bool _compareDeltaTimes;
            private readonly long _timesEpsilon;

            #endregion

            #region Constructor

            public TimedEventComparer(bool compareDeltaTimes, long timesEpsilon = 0)
            {
                _compareDeltaTimes = compareDeltaTimes;
                _timesEpsilon = timesEpsilon;
            }

            #endregion

            #region IEqualityComparer<Note>

            public bool Equals(TimedEvent timedEvent1, TimedEvent timedEvent2)
            {
                if (ReferenceEquals(timedEvent1, timedEvent2))
                    return true;

                if (ReferenceEquals(null, timedEvent1) || ReferenceEquals(null, timedEvent2))
                    return false;

                string message;
                return Math.Abs(timedEvent1.Time - timedEvent2.Time) <= _timesEpsilon &&
                       MidiEvent.Equals(timedEvent1.Event, timedEvent2.Event, new MidiEventEqualityCheckSettings { CompareDeltaTimes = _compareDeltaTimes }, out message);
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

        public static bool AreEqual(IEnumerable<TimedEvent> timedEvents1, IEnumerable<TimedEvent> timedEvents2, bool compareDeltaTimes, long timesEpsilon = 0)
        {
            if (ReferenceEquals(timedEvents1, timedEvents2))
                return true;

            if (ReferenceEquals(null, timedEvents1) || ReferenceEquals(null, timedEvents2))
                return false;

            return timedEvents1.SequenceEqual(timedEvents2, new TimedEventComparer(compareDeltaTimes, timesEpsilon));
        }

        #endregion
    }
}
