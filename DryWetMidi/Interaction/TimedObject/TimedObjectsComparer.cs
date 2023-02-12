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
}
