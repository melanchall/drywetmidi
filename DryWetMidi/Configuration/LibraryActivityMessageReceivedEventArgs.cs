using System;

namespace Melanchall.DryWetMidi.Configuration
{
    public sealed class LibraryActivityMessageReceivedEventArgs : EventArgs
    {
        #region Constructor

        internal LibraryActivityMessageReceivedEventArgs(string message)
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
