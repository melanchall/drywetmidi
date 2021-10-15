using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Provides data for <see cref="DevicesWatcher.DeviceAdded"/> and
    /// <see cref="DevicesWatcher.DeviceRemoved"/> events.
    /// </summary>
    public sealed class DeviceAddedRemovedEventArgs : EventArgs
    {
        #region Constructor

        internal DeviceAddedRemovedEventArgs(MidiDevice device)
        {
            Device = device;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a device that has been added or removed.
        /// </summary>
        public MidiDevice Device { get; }

        #endregion
    }
}
