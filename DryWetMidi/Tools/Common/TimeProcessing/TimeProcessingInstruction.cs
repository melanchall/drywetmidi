using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Tools
{
    /// <summary>
    /// Holds the new time that will be set to an object is being processed and action
    /// indicating whether quantization should be cancelled or not.
    /// </summary>
    public sealed class TimeProcessingInstruction
    {
        #region Constants

        /// <summary>
        /// Indicates that object should be skipped and new time shouldn't be set.
        /// </summary>
        public static readonly TimeProcessingInstruction Skip =
            new TimeProcessingInstruction(TimeProcessingAction.Skip, InvalidTime);

        private const long InvalidTime = -1;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TimeProcessingInstruction"/> with the
        /// specified time. This time will be set to an object is being processed.
        /// </summary>
        /// <param name="time">The new time of an object.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        public TimeProcessingInstruction(long time)
            : this(TimeProcessingAction.Apply, time)
        {
            ThrowIfArgument.IsNegative(nameof(time), time, "Time is negative.");
        }

        private TimeProcessingInstruction(TimeProcessingAction quantizingInstruction, long time)
        {
            Action = quantizingInstruction;
            Time = time;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets an action indicating whether processing should be cancelled or not.
        /// </summary>
        public TimeProcessingAction Action { get; }

        /// <summary>
        /// Gets the new time of an object.
        /// </summary>
        public long Time { get; }

        #endregion
    }
}
