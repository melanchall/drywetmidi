using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    internal sealed class DataSender : IDisposable
    {
        [DllImport("SendTestData", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int OpenSender(IntPtr portName, out IntPtr handle);

        [DllImport("SendTestData", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int SendData(IntPtr handle, byte[] data, int length, int[] indices, int indicesLength);

        [DllImport("SendTestData", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int CloseSender(IntPtr handle);

        private readonly IntPtr _handle;
        private bool _disposed;

        public DataSender(string portName)
        {
            var portNamePtr = Marshal.StringToHGlobalAnsi(portName);
            OpenSender(portNamePtr, out _handle);
        }

        ~DataSender()
        {
            Dispose(false);
        }
        public void SendData(byte[] data, int length, int[] indices, int indicesLength)
        {
            SendData(_handle, data, length, indices, indicesLength);
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
