using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace CreateLoopbackPort
{
    class Program
    {
        private delegate void Callback(IntPtr pktlist, IntPtr readProcRefCon, IntPtr srcConnRefCon);

        [DllImport("LoopbackAPI")]
        private static extern int CreateLoopbackPort(string portName, Callback callback, out IntPtr info);

        [DllImport("LoopbackAPI")]
        private static extern void SendDataBack(IntPtr pktlist, IntPtr info);

        static void Main(string[] args)
        {
            Console.WriteLine("Creating virtual ports...");

            foreach (var portName in args)
            {
                var result = CreateLoopbackPort(portName, HandleData, out var info);
                Console.WriteLine($"Port '{portName}' created ({result}).");
            }

            Console.WriteLine("Sleeping...");
			Thread.Sleep(int.MaxValue);
        }

        private static void HandleData(IntPtr pktlist, IntPtr readProcRefCon, IntPtr srcConnRefCon)
        {
            SendDataBack(pktlist, readProcRefCon);
        }
    }
}
