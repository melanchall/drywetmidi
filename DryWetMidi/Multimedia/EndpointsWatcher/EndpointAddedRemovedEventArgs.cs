using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Provides data for <see cref="EndpointsWatcher.EndpointAdded"/> and
    /// <see cref="EndpointsWatcher.EndpointRemoved"/> events.
    /// </summary>
    public sealed class EndpointAddedRemovedEventArgs : EventArgs
    {
        #region Constructor

        internal EndpointAddedRemovedEventArgs(MidiEndpoint device)
        {
            Endpoint = device;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a MIDI endpoint that has been added or removed.
        /// </summary>
        public MidiEndpoint Endpoint { get; }

        #endregion
    }
}
