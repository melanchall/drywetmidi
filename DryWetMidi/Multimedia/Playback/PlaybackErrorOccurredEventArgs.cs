using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Provides data for the <see cref="Playback.ErrorOccurred"/>.
    /// </summary>
    public sealed class PlaybackErrorOccurredEventArgs
    {
        #region Constructor

        internal PlaybackErrorOccurredEventArgs(
            PlaybackErrorSite site,
            Exception exception)
        {
            Site = site;
            Exception = exception;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the location within the playback where the error occurred.
        /// </summary>
        public PlaybackErrorSite Site { get; }

        /// <summary>
        /// Gets the exception.
        /// </summary>
        public Exception Exception { get; }

        #endregion
    }
}
