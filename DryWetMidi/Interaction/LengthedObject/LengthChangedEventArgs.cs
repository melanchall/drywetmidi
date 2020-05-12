using System;

namespace Melanchall.DryWetMidi.Interaction
{
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

        public long OldLength { get; }

        public long NewLength { get; }

        #endregion
    }
}
