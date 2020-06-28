using System;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Contains arguments for the <see cref="INotifyLengthChanged.LengthChanged"/> event.
    /// </summary>
    public sealed class LengthChangedEventArgs : EventArgs
    {
        #region Constructor

        internal LengthChangedEventArgs(long oldLength, long newLength)
        {
            OldLength = oldLength;
            NewLength = newLength;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the old length of an object.
        /// </summary>
        public long OldLength { get; }

        /// <summary>
        /// Gets the new length of an object.
        /// </summary>
        public long NewLength { get; }

        #endregion
    }
}
