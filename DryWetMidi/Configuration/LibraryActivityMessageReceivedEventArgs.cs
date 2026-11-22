using System;

namespace Melanchall.DryWetMidi.Configuration
{
    /// <summary>
    /// Provides data for the <see cref="LibraryConfiguration.LibraryActivityMessageReceived"/> event.
    /// </summary>
    public sealed class LibraryActivityMessageReceivedEventArgs : EventArgs
    {
        #region Constructor

        internal LibraryActivityMessageReceivedEventArgs(string message)
        {
            Message = message;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the activity message received from the native MIDI API.
        /// </summary>
        public string Message { get; set; }

        #endregion

        #region Overrides

        /// <inheritdoc/>
        public override string ToString() =>
            Message;

        #endregion
    }
}
