using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class TimedObjectsComparer<TObject> : IComparer<TObject>
        where TObject : ITimedObject
    {
        #region Fields

        private readonly Comparison<MidiEvent> _sameTimeEventsComparison;

        #endregion

        #region Constructor

        public TimedObjectsComparer()
            : this(null)
        {
        }

        public TimedObjectsComparer(Comparison<MidiEvent> sameTimeEventsComparison)
        {
            _sameTimeEventsComparison = sameTimeEventsComparison;
        }

        #endregion

        #region IComparer<TObject>

        public int Compare(TObject x, TObject y)
        {
            if (ReferenceEquals(x, y))
                return 0;

            if (ReferenceEquals(x, null))
                return -1;

            if (ReferenceEquals(y, null))
                return 1;

            var timeDeltaSign = Math.Sign(x.Time - y.Time);
            if (timeDeltaSign != 0)
                return timeDeltaSign;

            //

            var timedEventX = x as TimedEvent;
            var timedEventY = y as TimedEvent;
            if (timedEventX == null || timedEventY == null)
                return 0;

            //

            return _sameTimeEventsComparison?.Invoke(timedEventX.Event, timedEventY.Event) ?? 0;
        }

        #endregion
    }
}
