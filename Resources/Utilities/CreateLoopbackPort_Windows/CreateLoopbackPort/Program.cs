// The program uses API provided by virtualMIDI SDK from Tobias Erichsen
// (tobias-erichsen.de/software/virtualmidi/virtualmidi-sdk.html).

using System;
using System.Runtime.InteropServices;
using System.Threading;

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

		private static byte[] Buffer = new byte[BufferLength];

		public static void Main(string[] args)
		{
			var manufacturer = Guid.NewGuid();
			var product = Guid.NewGuid();
			var portName = args[0];

            Console.WriteLine($"Creating virtual MIDI port '{portName}'...");
            var portHandle = virtualMIDICreatePortEx3(portName, IntPtr.Zero, IntPtr.Zero, 65535, 12, ref manufacturer, ref product);
            Console.WriteLine($"Virtual MIDI port created.");

            while (true)
            {
                var commandBytesLength = GetCommandBytesLength(portHandle);
                SendCommandBytes(portHandle, commandBytesLength);
            }
		}

		private static void SendCommandBytes(IntPtr portHandle, uint commandBytesLength)
		{
			if (commandBytesLength == 0)
				return;

			virtualMIDISendData(portHandle, Buffer, commandBytesLength);
		}

		private static uint GetCommandBytesLength(IntPtr portHandle)
		{
			uint length = BufferLength;
			virtualMIDIGetData(portHandle, Buffer, ref length);
			return length;
		}
	}
}
