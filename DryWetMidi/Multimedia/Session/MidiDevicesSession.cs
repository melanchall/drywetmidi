using Melanchall.DryWetMidi.Common;
using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal static class MidiDevicesSession
    {
        #region Events

        internal static event EventHandler<IntPtr> InputDeviceAdded;
        internal static event EventHandler<IntPtr> InputDeviceRemoved;
        internal static event EventHandler<IntPtr> OutputDeviceAdded;
        internal static event EventHandler<IntPtr> OutputDeviceRemoved;

        #endregion

        #region Fields

        private static readonly object _lockObject = new object();

        private static IntPtr _name;
        private static MidiDevicesSessionHandle _handle;

        private static MidiDevicesSessionApi.InputDeviceCallback _inputDeviceCallback;
        private static MidiDevicesSessionApi.OutputDeviceCallback _outputDeviceCallback;

        #endregion

        #region Properties

#if TEST
        internal static TestCheckpoints TestCheckpoints { get; set; }
#endif

        #endregion

        #region Methods

        public static MidiDevicesSessionHandle GetSessionHandle()
        {
            if (_handle == null || _handle.IsInvalid)
            {
                lock (_lockObject)
                {
                    if (_handle == null || _handle.IsInvalid)
                    {
                        var name = Guid.NewGuid().ToString();
                        _name = Marshal.StringToHGlobalAuto(name);

                        var apiType = CommonApi.Api_GetApiType();
                        var result = default(MidiDevicesSessionApi.SESSION_OPENRESULT);
                        int errorCode = 0;

                        var rawHandle = IntPtr.Zero;

                        switch (apiType)
                        {
                            case CommonApi.API_TYPE.API_TYPE_MAC:
                                _inputDeviceCallback = InputDeviceCallback;
                                _outputDeviceCallback = OutputDeviceCallback;
                                result = MidiDevicesSessionApi.Api_OpenSession_Mac(_name, _inputDeviceCallback, _outputDeviceCallback, out rawHandle, out errorCode);
                                break;
                            case CommonApi.API_TYPE.API_TYPE_WIN:
                                result = MidiDevicesSessionApi.Api_OpenSession_Win(_name, out rawHandle, out errorCode);
                                break;
                        }

                        NativeApiUtilities.HandleDevicesNativeApiResult(result, errorCode);
                        
                        _handle = new MidiDevicesSessionHandle(rawHandle);

#if TEST
                        _handle.TestCheckpoints = TestCheckpoints;
#endif

                        AppDomain.CurrentDomain.DomainUnload += OnDomainUnloadOrExit;
                        AppDomain.CurrentDomain.ProcessExit += OnDomainUnloadOrExit;
                    }
                }
            }

            return _handle;
        }

        private static void OnDomainUnloadOrExit(object sender, EventArgs e)
        {
            if (_handle != null && !_handle.IsInvalid)
            {
                lock (_lockObject)
                {
                    if (_handle != null && !_handle.IsInvalid)
                    {
                        _handle?.Dispose();
                        _handle = null;

                        if (_name != IntPtr.Zero)
                        {
                            Marshal.FreeHGlobal(_name);
                            _name = IntPtr.Zero;
                        }
                    }
                }
            }
        }

        private static void InputDeviceCallback(IntPtr info, bool operation)
        {
            if (operation)
                InputDeviceAdded?.Invoke(null, info);
            else
                InputDeviceRemoved?.Invoke(null, info);
        }

        private static void OutputDeviceCallback(IntPtr info, bool operation)
        {
            if (operation)
                OutputDeviceAdded?.Invoke(null, info);
            else
                OutputDeviceRemoved?.Invoke(null, info);
        }

        #endregion
    }
}
