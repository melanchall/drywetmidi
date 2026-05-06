using Melanchall.DryWetMidi.Configuration;
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
        /// <exception cref="PlatformNotSupportedException">Devices watcher API is not supported on the current operating system.</exception>
        public static DevicesWatcher Instance
        {
            get
            {
                if (!LibraryConfiguration.IsDevicesWatcherApiAvailable())
                    throw new PlatformNotSupportedException("Devices watcher API is not supported on the current operating system.");

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
