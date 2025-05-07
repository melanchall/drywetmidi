﻿using System;
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
        private static IntPtr _handle;

        private static MidiDevicesSessionApi.InputDeviceCallback _inputDeviceCallback;
        private static MidiDevicesSessionApi.OutputDeviceCallback _outputDeviceCallback;

        #endregion

        #region Methods

        public static IntPtr GetSessionHandle()
        {
            if (_handle == IntPtr.Zero)
            {
                lock (_lockObject)
                {
                    if (_handle == IntPtr.Zero)
                    {
                        var name = Guid.NewGuid().ToString();
                        _name = Marshal.StringToHGlobalAuto(name);

                        var apiType = CommonApiProvider.Api.Api_GetApiType();
                        var result = default(MidiDevicesSessionApi.SESSION_OPENRESULT);

                        switch (apiType)
                        {
                            case CommonApi.API_TYPE.API_TYPE_MAC:
                                _inputDeviceCallback = InputDeviceCallback;
                                _outputDeviceCallback = OutputDeviceCallback;
                                result = MidiDevicesSessionApiProvider.Api.Api_OpenSession_Mac(_name, _inputDeviceCallback, _outputDeviceCallback, out _handle);
                                break;
                            case CommonApi.API_TYPE.API_TYPE_WIN:
                                result = MidiDevicesSessionApiProvider.Api.Api_OpenSession_Win(_name, out _handle);
                                break;
                        }

                        NativeApiUtilities.HandleDevicesNativeApiResult(result);

                        AppDomain.CurrentDomain.DomainUnload += OnDomainUnloadOrExit;
                        AppDomain.CurrentDomain.ProcessExit += OnDomainUnloadOrExit;
                    }
                }
            }

            return _handle;
        }

        private static void OnDomainUnloadOrExit(object sender, EventArgs e)
        {
            if (_handle != IntPtr.Zero)
            {
                lock (_lockObject)
                {
                    if (_handle != IntPtr.Zero)
                    {
                        MidiDevicesSessionApiProvider.Api.Api_CloseSession(_handle);
                        _handle = IntPtr.Zero;
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
