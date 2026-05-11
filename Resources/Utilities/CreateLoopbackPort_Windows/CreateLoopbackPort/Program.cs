// The program uses API provided by virtualMIDI SDK from Tobias Erichsen
// (tobias-erichsen.de/software/virtualmidi/virtualmidi-sdk.html).

using System;
using System.Runtime.InteropServices;

namespace CreateLoopbackPort
{
	public class Program
	{
		private const uint BufferLength = 65535;

		[DllImport("teVirtualMIDI.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern IntPtr virtualMIDICreatePortEx3(string portName, IntPtr callback, IntPtr dwCallbackInstance, uint maxSysexLength, uint flags, ref Guid manufacturer, ref Guid product);

		[DllImport("teVirtualMIDI.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern bool virtualMIDISendData(IntPtr midiPort, byte[] midiDataBytes, uint length);

		[DllImport("teVirtualMIDI.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern bool virtualMIDIGetData(IntPtr midiPort, [Out] byte[] midiDataBytes, ref uint length);

        [DllImport("teVirtualMIDI.dll", SetLastError = true)]
        private static extern void virtualMIDIClosePort(IntPtr midiPort);

        [DllImport("teVirtualMIDI.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool virtualMIDIShutdown(IntPtr midiPort);

        private static readonly byte[] Buffer = new byte[BufferLength];

		public static void Main(string[] args)
		{
			var manufacturer = Guid.NewGuid();
			var product = Guid.NewGuid();
			var portName = args[0];

            Console.WriteLine($"Creating virtual MIDI port '{portName}'...");
            var portHandle = virtualMIDICreatePortEx3(portName, IntPtr.Zero, IntPtr.Zero, 65535, 12, ref manufacturer, ref product);
            Console.WriteLine($"Virtual MIDI port created and running...");

			var running = true;

            while (running)
            {
				try
				{
					uint length = BufferLength;
					var success = virtualMIDIGetData(portHandle, Buffer, ref length);

					if (success && length > 0)
						virtualMIDISendData(portHandle, Buffer, length);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Exception in loopback loop:{Environment.NewLine}{ex}.");
				}
            }

            virtualMIDIShutdown(portHandle);
			virtualMIDIClosePort(portHandle);

			Console.WriteLine($"Virtual MIDI port '{portName}' closed.");
        }
	}
}
