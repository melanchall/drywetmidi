using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Defines a method that a type implements to compare two objects.
    /// </summary>
    public class TimedObjectsComparer : IComparer<ITimedObject>
    {
        #region IComparer<TObject>

        /// <summary>
        /// Compares two objects and returns a value indicating whether one is less than,
        /// equal to, or greater than the other.
        /// </summary>
        /// <param name="x">The first object to compare.</param>
        /// <param name="y">The second object to compare.</param>
        /// <returns>A signed integer that indicates the relative values of <paramref name="x"/> and
        /// <paramref name="y"/>, as shown in the following table.
        /// <list type="table">
        ///     <listheader>
        ///         <term>Value</term>
        ///         <term>Meaning</term>
        ///     </listheader>
        ///     <item>
        ///         <term>Less than zero</term>
        ///         <term><paramref name="x"/> is less than <paramref name="y"/></term>
        ///     </item>
        ///     <item>
        ///         <term>Zero</term>
        ///         <term><paramref name="x"/> equals <paramref name="y"/></term>
        ///     </item>
        ///     <item>
        ///         <term>Greater than zero</term>
        ///         <term><paramref name="x"/> is greater than <paramref name="y"/></term>
        ///     </item>
        /// </list>
        /// </returns>
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
    [Obsolete("OBS11")]
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
