using System;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Provides a way to watch devices adding/removing in the system. More info in the
    /// <see href="xref:a_dev_watcher">Devices watcher</see> article.
    /// </summary>
    public sealed class DevicesWatcher
    {
        #region Events

        /// <summary>
        /// Occurs when a MIDI device has been added to the system.
        /// </summary>
        public event EventHandler<DeviceAddedRemovedEventArgs> DeviceAdded;

        /// <summary>
        /// Occurs when a MIDI device has been removed from the system.
        /// </summary>
        public event EventHandler<DeviceAddedRemovedEventArgs> DeviceRemoved;

        #endregion

        #region Fields

        private static volatile DevicesWatcher _instance;
        private static readonly object _lockObject = new object();

        #endregion

        #region Constructor

        private DevicesWatcher()
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the instance of <see cref="DevicesWatcher"/>.
        /// </summary>
        public static DevicesWatcher Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            MidiDevicesSession.GetSessionHandle();

                            _instance = new DevicesWatcher();

                            MidiDevicesSession.InputDeviceAdded += _instance.OnInputDeviceAdded;
                            MidiDevicesSession.InputDeviceRemoved += _instance.OnInputDeviceRemoved;
                            MidiDevicesSession.OutputDeviceAdded += _instance.OnOutputDeviceAdded;
                            MidiDevicesSession.OutputDeviceRemoved += _instance.OnOutputDeviceRemoved;
                        }
                    }
                }

                return _instance;
            }
        }

        #endregion

        #region Methods

        private void OnInputDeviceAdded(object sender, IntPtr info)
        {
            DeviceAdded?.Invoke(this, new DeviceAddedRemovedEventArgs(new InputDevice(info, MidiDevice.CreationContext.AddedDevice)));
        }

        private void OnInputDeviceRemoved(object sender, IntPtr info)
        {
            DeviceRemoved?.Invoke(this, new DeviceAddedRemovedEventArgs(new InputDevice(info, MidiDevice.CreationContext.RemovedDevice)));
        }

        private void OnOutputDeviceAdded(object sender, IntPtr info)
        {
            DeviceAdded?.Invoke(this, new DeviceAddedRemovedEventArgs(new OutputDevice(info, MidiDevice.CreationContext.AddedDevice)));
        }

        private void OnOutputDeviceRemoved(object sender, IntPtr info)
        {
            DeviceRemoved?.Invoke(this, new DeviceAddedRemovedEventArgs(new OutputDevice(info, MidiDevice.CreationContext.RemovedDevice)));
        }

        #endregion
    }
}
