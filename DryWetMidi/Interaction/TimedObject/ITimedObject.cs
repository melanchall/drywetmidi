using System;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Represents an object that has time.
    /// </summary>
    public interface ITimedObject
    {
        #region Properties

        /// <summary>
        /// Gets the start time of an object.
        /// </summary>
        /// <remarks>
        /// Note that the returned value will be in ticks (not seconds, not milliseconds and so on).
        /// Please read <see href="xref:a_time_length">Time and length</see> article to learn how you can
        /// get the time in different representations.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
        long Time { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Clones object by creating a copy of it.
        /// </summary>
        /// <returns>Copy of the object.</returns>
        ITimedObject Clone();

        #endregion
    }
}
