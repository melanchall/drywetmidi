using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    public class TimedObjectsComparer : IComparer<ITimedObject>
    {
        #region IComparer<TObject>

        public virtual int Compare(ITimedObject x, ITimedObject y)
        {
            if (ReferenceEquals(x, y))
                return 0;

            if (ReferenceEquals(x, null))
                return -1;

            if (ReferenceEquals(y, null))
                return 1;

            return Math.Sign(x.Time - y.Time);
        }

        #endregion
    }

    // TODO: search for all Comparison on OBS removing
    [Obsolete("OBS13")]
    internal sealed class TimedObjectsComparerOnSameEventTime : TimedObjectsComparer
    {
        #region Fields

        private readonly Comparison<MidiEvent> _sameTimeEventsComparison;

        #endregion

        #region Constructor

        public TimedObjectsComparerOnSameEventTime()
            : this(null)
        {
        }

        public TimedObjectsComparerOnSameEventTime(Comparison<MidiEvent> sameTimeEventsComparison)
        {
            _sameTimeEventsComparison = sameTimeEventsComparison;
        }

        #endregion

        #region Overrides

        public override int Compare(ITimedObject x, ITimedObject y)
        {
            if (ReferenceEquals(x, y))
                return 0;

            if (ReferenceEquals(x, null))
                return -1;

            if (ReferenceEquals(y, null))
                return 1;

            var baseResult = Math.Sign(x.Time - y.Time);
            if (baseResult != 0)
                return baseResult;

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
