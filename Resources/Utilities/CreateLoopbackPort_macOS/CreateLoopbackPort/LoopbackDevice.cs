using System;
using System.Runtime.InteropServices;

namespace CreateLoopbackPort
{
    internal sealed class LoopbackDevice
    {
        #region Extern functions

        private delegate void Callback(IntPtr pktlist, IntPtr readProcRefCon, IntPtr srcConnRefCon);

        [DllImport("LoopbackAPI")]
        private static extern int CreateLoopbackPort(IntPtr sessionHandle, IntPtr portName, Callback callback, out IntPtr info);

        [DllImport("LoopbackAPI")]
        private static extern int SendDataBack(IntPtr pktlist, IntPtr info);

        #endregion

        #region Fields

        private readonly string _name;
        private readonly IntPtr _namePointer;
        private readonly Callback _callback;
        private readonly IntPtr _info;

        #endregion

        #region Constructor

        public LoopbackDevice(IntPtr sessionHandle, string deviceName)
        {
            _name = deviceName;
            _namePointer = Marshal.StringToHGlobalAnsi(deviceName);
            _callback = HandleData;

            Logger.Write($"Creating port '{_name}'...");
            var result = CreateLoopbackPort(sessionHandle, _namePointer, _callback, out _info);
            Logger.WriteLine($"{result}");
        }

        #endregion

        #region Methods

        private void HandleData(IntPtr pktlist, IntPtr readProcRefCon, IntPtr srcConnRefCon)
        {
            SendDataBack(pktlist, readProcRefCon);
        }

        #endregion
    }
}
