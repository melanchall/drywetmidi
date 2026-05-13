using System;

namespace Melanchall.DryWetMidi.Configuration
{
    public sealed class NativeApiActivityRecordReceivedEventArgs : EventArgs
    {
        #region Constructor

        internal NativeApiActivityRecordReceivedEventArgs(string message)
        {
            Message = message;
        }

        #endregion

        #region Properties

        public string Message { get; set; }

        #endregion

        #region Overrides

        public override string ToString() =>
            Message;

        #endregion
    }
}
