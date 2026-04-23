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

        private EventHandler<DeviceAddedRemovedEventArgs> _deviceAdded;
        private EventHandler<DeviceAddedRemovedEventArgs> _deviceRemoved;

        /// <summary>
        /// Occurs when a MIDI device has been added to the system.
        /// </summary>
        public event EventHandler<DeviceAddedRemovedEventArgs> DeviceAdded
        {
            add
            {
                var hadSubscribers = _deviceAdded != null || _deviceRemoved != null;
                
                _deviceAdded += value;
                if (!hadSubscribers)
                    EnableDevicesWatcher();
            }
            remove
            {
                _deviceAdded -= value;
                if (_deviceAdded == null && _deviceRemoved == null)
                    DisableDevicesWatcher();
            }
        }

        /// <summary>
        /// Occurs when a MIDI device has been removed from the system.
        /// </summary>
        public event EventHandler<DeviceAddedRemovedEventArgs> DeviceRemoved
        {
            add
            {
                var hadSubscribers = _deviceRemoved != null || _deviceAdded != null;

                _deviceRemoved += value;
                if (!hadSubscribers)
                    EnableDevicesWatcher();
            }
            remove
            {
                _deviceRemoved -= value;
                if (_deviceRemoved == null && _deviceAdded == null)
                    DisableDevicesWatcher();
            }
        }

        #endregion

        #region Fields

        private static volatile DevicesWatcher _instance;
        private static readonly object _lockObject = new object();

        private MidiDevicesSessionHandle _sessionHandle;

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
        /// <exception cref="PlatformNotSupportedException">This operation is not supported on the current operating system.</exception>
        public static DevicesWatcher Instance
        {
            get
            {
                Utilities.EnsureOsIsSupported();

                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new DevicesWatcher
                            {
                                _sessionHandle = MidiDevicesSession.GetSessionHandle()
                            };

                            MidiDevicesSession.InputDeviceAdded += _instance.OnInputDeviceAdded;
                            MidiDevicesSession.InputDeviceRemoved += _instance.OnInputDeviceRemoved;
                            MidiDevicesSession.OutputDeviceAdded += _instance.OnOutputDeviceAdded;
                            MidiDevicesSession.OutputDeviceRemoved += _instance.OnOutputDeviceRemoved;

                            AppDomain.CurrentDomain.DomainUnload += OnDomainUnloadOrExit;
                            AppDomain.CurrentDomain.ProcessExit += OnDomainUnloadOrExit;
                        }
                    }
                }

                return _instance;
            }
        }

        #endregion

        #region Methods

        private static void OnDomainUnloadOrExit(object sender, EventArgs e)
        {
            if (_instance != null)
            {
                lock (_lockObject)
                {
                    if (_instance != null)
                    {
                        _instance._sessionHandle = null;

                        MidiDevicesSession.InputDeviceAdded -= _instance.OnInputDeviceAdded;
                        MidiDevicesSession.InputDeviceRemoved -= _instance.OnInputDeviceRemoved;
                        MidiDevicesSession.OutputDeviceAdded -= _instance.OnOutputDeviceAdded;
                        MidiDevicesSession.OutputDeviceRemoved -= _instance.OnOutputDeviceRemoved;
                    }
                }
            }
        }

        private void OnInputDeviceAdded(object sender, IntPtr info)
        {
            _deviceAdded?.Invoke(this, new DeviceAddedRemovedEventArgs(new InputDevice(info, MidiDevice.CreationContext.AddedDevice)));
        }

        private void OnInputDeviceRemoved(object sender, IntPtr info)
        {
            _deviceRemoved?.Invoke(this, new DeviceAddedRemovedEventArgs(new InputDevice(info, MidiDevice.CreationContext.RemovedDevice)));
        }

        private void OnOutputDeviceAdded(object sender, IntPtr info)
        {
            _deviceAdded?.Invoke(this, new DeviceAddedRemovedEventArgs(new OutputDevice(info, MidiDevice.CreationContext.AddedDevice)));
        }

        private void OnOutputDeviceRemoved(object sender, IntPtr info)
        {
            _deviceRemoved?.Invoke(this, new DeviceAddedRemovedEventArgs(new OutputDevice(info, MidiDevice.CreationContext.RemovedDevice)));
        }

        private void EnableDevicesWatcher()
        {
            DevicesWatcherApi.Api_EnableDevicesWatcher(_sessionHandle);
        }

        private void DisableDevicesWatcher()
        {
            DevicesWatcherApi.Api_DisableDevicesWatcher(_sessionHandle);
        }

        #endregion
    }
}
