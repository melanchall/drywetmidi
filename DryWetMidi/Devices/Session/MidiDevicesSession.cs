using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Devices
{
    internal static class MidiDevicesSession
    {
        #region Fields

        private static readonly object _lockObject = new object();

        private static IntPtr _name;
        private static IntPtr _handle;
        private static int _clientsCount;

        #endregion

        #region Methods

        public static IntPtr GetSessionHandle()
        {
            lock (_lockObject)
            {
                if (_handle == IntPtr.Zero)
                {
                    var name = Guid.NewGuid().ToString();
                    _name = Marshal.StringToHGlobalAuto(name);

                    MidiDevicesSessionApiProvider.Api.Api_OpenSession(_name, out _handle);
                }

                _clientsCount++;

                return _handle;
            }
        }

        public static void ExitSession()
        {
            lock (_lockObject)
            {
                if (_handle == IntPtr.Zero)
                    return;

                _clientsCount--;

                if (_clientsCount == 0)
                {
                    MidiDevicesSessionApiProvider.Api.Api_CloseSession(_handle);
                    Marshal.FreeHGlobal(_name);

                    _name = IntPtr.Zero;
                    _handle = IntPtr.Zero;
                }
            }
        }

        #endregion
    }
}
