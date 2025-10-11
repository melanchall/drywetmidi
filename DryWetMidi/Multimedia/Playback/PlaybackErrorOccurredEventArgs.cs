using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    public sealed class PlaybackErrorOccurredEventArgs
    {
        #region Constructor

        internal PlaybackErrorOccurredEventArgs(
            PlaybackSite site,
            Exception exception)
        {
            Site = site;
            Exception = exception;
        }

        #endregion

        #region Properties

        public PlaybackSite Site { get; }

        public Exception Exception { get; }

        #endregion
    }
}
