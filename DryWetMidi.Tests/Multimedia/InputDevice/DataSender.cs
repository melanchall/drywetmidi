using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    internal sealed class DataSender : IDisposable
    {
        private enum OPENRESULT
        {
            OPENRESULT_OK = 0,

            OPENRESULT_FAILEDCREATECLIENT = 1,
            OPENRESULT_FAILEDCREATEPORT = 2,
            OPENRESULT_FAILEDFINDPORT = 3
        }

        private enum CLOSERESULT
        {
            CLOSERESULT_OK = 0
        }

        private enum SENDRESULT
        {
            SENDRESULT_OK = 0,
            SENDRESULT_FAILEDSEND = 1
        }

        [DllImport("SendTestData", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern OPENRESULT OpenSender(IntPtr portName, out IntPtr handle);

        [DllImport("SendTestData", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern SENDRESULT SendData(IntPtr handle, byte[] data, int length, int[] indices, int indicesLength);

        [DllImport("SendTestData", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern CLOSERESULT CloseSender(IntPtr handle);

        private readonly IntPtr _handle;
        private bool _disposed;

        public DataSender(string portName)
        {
            var portNamePtr = Marshal.StringToHGlobalAnsi(portName);
            var result = OpenSender(portNamePtr, out _handle);
            if (result != OPENRESULT.OPENRESULT_OK)
                throw new InvalidOperationException($"Failed to open data sender: {result}.");
        }

        ~DataSender()
        {
            Dispose(false);
        }

        public void SendData(byte[] data, int length, int[] indices, int indicesLength)
        {
            var result = SendData(_handle, data, length, indices, indicesLength);
            if (result != SENDRESULT.SENDRESULT_OK)
                throw new InvalidOperationException($"Failed to send data: {result}.");
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
            }

            CloseSender(_handle);
            _disposed = true;
        }
    }
}
