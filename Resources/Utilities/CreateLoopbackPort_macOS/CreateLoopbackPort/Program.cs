using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace CreateLoopbackPort
{
    class Program
    {
        [DllImport("LoopbackAPI")]
        private static extern int OpenSession(IntPtr name, out IntPtr handle);

        private static LoopbackDevice[] _devices;

        static void Main(string[] args)
        {
            Logger.Write("Creating session...");

            var clientName = Guid.NewGuid().ToString();
            var clientNamePointer = Marshal.StringToHGlobalAnsi(clientName);
            var result = OpenSession(clientNamePointer, out var sessionHandle);

            Logger.WriteLine($"{result}");

            Logger.WriteLine("Creating ports...");

            _devices = new LoopbackDevice[args.Length];
            var i = 0;

            foreach (var portName in args)
            {
                _devices[i] = new LoopbackDevice(sessionHandle, portName);
                i++;
            }

            Logger.WriteLine("Sleeping...");

            while (true)
			{
				Thread.Sleep(1);
			}
        }
    }
}
