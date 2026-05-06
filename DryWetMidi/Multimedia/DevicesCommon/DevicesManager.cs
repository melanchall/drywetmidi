using Melanchall.DryWetMidi.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class DevicesManager
    {
        #region Fields

        private static volatile DevicesManager _instance;
        private static readonly object _lockObject = new object();

        private bool _cachingRequired = false;

        private readonly HashSet<InputDevice> _inputDevices = new();
        private readonly object _inputDevicesLock = new();

        private readonly HashSet<OutputDevice> _outputDevices = new();
        private readonly object _outputDevicesLock = new();

        #endregion

        #region Constructor

        private DevicesManager()
        {
        }

        #endregion

        #region Properties

        public static DevicesManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new DevicesManager();

                            var sessionHandle = MidiDevicesSession.GetSessionHandle();
                            _instance._cachingRequired = sessionHandle.IsDevicesCachingRequired;

                            if (_instance._cachingRequired)
                            {
                                lock (_instance._inputDevicesLock)
                                {
                                    foreach (var device in _instance.GetAllInputDevicesInternal())
                                    {
                                        _instance._inputDevices.Add(device);
                                    }
                                }

                                lock (_instance._outputDevicesLock)
                                {
                                    foreach (var device in _instance.GetAllOutputDevicesInternal())
                                    {
                                        _instance._outputDevices.Add(device);
                                    }
                                }

                                DevicesWatcher.Instance.DeviceAdded += _instance.OnDeviceAdded;
                                DevicesWatcher.Instance.DeviceRemoved += _instance.OnDeviceRemoved;

                                AppDomain.CurrentDomain.DomainUnload += OnDomainUnloadOrExit;
                                AppDomain.CurrentDomain.ProcessExit += OnDomainUnloadOrExit;
                            }
                        }
                    }
                }

                return _instance;
            }
        }

        #endregion

        #region Methods

        public ICollection<InputDevice> GetAllInputDevices()
        {
            if (_cachingRequired)
            {
                lock (_inputDevicesLock)
                {
                    return _inputDevices.Select(CloneInputDevice).ToArray();
                }
            }

            return GetAllInputDevicesInternal();
        }

        public ICollection<OutputDevice> GetAllOutputDevices()
        {
            if (_cachingRequired)
            {
                lock (_outputDevicesLock)
                {
                    return _outputDevices.Select(CloneOutputDevice).ToArray();
                }
            }

            return GetAllOutputDevicesInternal();
        }

        public InputDevice GetInputDeviceByName(string name)
        {
            if (_cachingRequired)
            {
                lock (_inputDevicesLock)
                {
                    var result = _inputDevices.FirstOrDefault(d => d.Name == name);
                    if (result == null)
                        return null;

                    return CloneInputDevice(result);
                }
            }

            return GetAllInputDevicesInternal().FirstOrDefault(d => d.Name == name);
        }

        public OutputDevice GetOutputDeviceByName(string name)
        {
            if (_cachingRequired)
            {
                lock (_outputDevicesLock)
                {
                    var result = _outputDevices.FirstOrDefault(d => d.Name == name);
                    if (result == null)
                        return null;

                    return CloneOutputDevice(result);
                }
            }

            return GetAllOutputDevicesInternal().FirstOrDefault(d => d.Name == name);
        }

        private static void OnDomainUnloadOrExit(object sender, EventArgs e)
        {
            if (_instance != null)
            {
                lock (_lockObject)
                {
                    if (_instance != null)
                    {
                        DevicesWatcher.Instance.DeviceAdded -= _instance.OnDeviceAdded;
                        DevicesWatcher.Instance.DeviceRemoved -= _instance.OnDeviceRemoved;
                    }
                }
            }
        }

        private InputDevice CloneInputDevice(InputDevice inputDevice)
        {
            InputDeviceApi.Api_CloneInputDeviceInfo(inputDevice.Info.DangerousGetHandle(), out var clonedInfo);
            return new InputDevice(clonedInfo, inputDevice.Context);
        }

        private OutputDevice CloneOutputDevice(OutputDevice outputDevice)
        {
            OutputDeviceApi.Api_CloneOutputDeviceInfo(outputDevice.Info.DangerousGetHandle(), out var clonedInfo);
            return new OutputDevice(clonedInfo, outputDevice.Context);
        }

        private ICollection<InputDevice> GetAllInputDevicesInternal()
        {
            var result = InputDeviceApi.Api_GetDevicesInfo(MidiConfiguration.GetConfigurationHandle(), MidiDevicesSession.GetSessionHandle(), out var devicesInfo, out var size, out var errorCode);
            MidiDeviceUtilities.HandleDevicesNativeApiResult(result, errorCode);

            var devices = new InputDevice[size];

            for (int i = 0; i < size; i++)
            {
                var info = Marshal.ReadIntPtr(devicesInfo, i * IntPtr.Size);
                devices[i] = new InputDevice(info, MidiDevice.CreationContext.User);
            }

            InputDeviceApi.Api_FreeDevicesInfo(devicesInfo, size);

            return devices;
        }

        private ICollection<OutputDevice> GetAllOutputDevicesInternal()
        {
            var result = OutputDeviceApi.Api_GetDevicesInfo(MidiConfiguration.GetConfigurationHandle(), MidiDevicesSession.GetSessionHandle(), out var devicesInfoArray, out var size, out var error);
            MidiDeviceUtilities.HandleDevicesNativeApiResult(result, error);

            var devices = new OutputDevice[size];

            for (int i = 0; i < size; i++)
            {
                var info = Marshal.ReadIntPtr(devicesInfoArray, i * IntPtr.Size);
                devices[i] = new OutputDevice(info, MidiDevice.CreationContext.User);
            }

            OutputDeviceApi.Api_FreeDevicesInfo(devicesInfoArray, size);

            return devices;
        }

        private void OnDeviceAdded(object sender, DeviceAddedRemovedEventArgs e)
        {
            lock (_inputDevicesLock)
            {
                if (e.Device is InputDevice inputDevice)
                    _inputDevices.Add(inputDevice);
            }

            lock (_outputDevicesLock)
            {
                if (e.Device is OutputDevice outputDevice)
                    _outputDevices.Add(outputDevice);
            }
        }

        private void OnDeviceRemoved(object sender, DeviceAddedRemovedEventArgs e)
        {
            lock (_inputDevicesLock)
            {
                if (e.Device is InputDevice inputDevice)
                    _inputDevices.Remove(inputDevice);
            }

            lock (_outputDevicesLock)
            {
                if (e.Device is OutputDevice outputDevice)
                    _outputDevices.Remove(outputDevice);
            }
        }

        #endregion
    }
}
