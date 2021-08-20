using System;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class MidiDevicesWatcher
    {
        #region Events

        public event EventHandler<DeviceAddedRemovedEventArgs> DeviceAdded;

        public event EventHandler<DeviceAddedRemovedEventArgs> DeviceRemoved;

        #endregion

        #region Fields

        private static MidiDevicesWatcher _instance;
        private static object _lockObject = new object();

        #endregion

        #region Constructor

        private MidiDevicesWatcher()
        {
        }

        #endregion

        #region Properties

        public static MidiDevicesWatcher Instance
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

                            _instance = new MidiDevicesWatcher();

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
