using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class TimedEventsComparer : IComparer<TimedEvent>
    {
        #region Fields

        private readonly Comparison<MidiEvent> _sameTimeEventsComparison;

        #endregion

        #region Constructor

        internal TimedEventsComparer(Comparison<MidiEvent> sameTimeEventsComparison)
        {
            _sameTimeEventsComparison = sameTimeEventsComparison;
        }

        #endregion

        #region IComparer<TimedEvent>

        public int Compare(TimedEvent x, TimedEvent y)
        {
            if (x == null && y == null)
                return 0;
            else if (x == null)
                return -1;
            else if (y == null)
                return 1;

            //

            var timeDeltaSign = Math.Sign(x.Time - y.Time);
            if (timeDeltaSign != 0)
                return timeDeltaSign;

            //

            return _sameTimeEventsComparison?.Invoke(x.Event, y.Event) ?? 0;
        }

        #endregion
    }
}
