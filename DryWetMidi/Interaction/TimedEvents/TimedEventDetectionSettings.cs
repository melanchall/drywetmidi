using System;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Settings which define how timed events should be detected and built.
    /// </summary>
    /// <seealso cref="TimedEventsManagingUtilities"/>
    /// <seealso cref="GetObjectsUtilities"/>
    public sealed class TimedEventDetectionSettings
    {
        #region Properties

        /// <summary>
        /// Gets or sets custom construction method for <see cref="TimedEvent"/>. If <c>null</c>,
        /// default method will be used (via one of the <see cref="TimedEvent"/>'s constructors).
        /// </summary>
        public Func<TimedEventData, TimedEvent> Constructor { get; set; }

        #endregion
    }
}
