// The program uses API provided by virtualMIDI SDK from Tobias Erichsen
// (tobias-erichsen.de/software/virtualmidi/virtualmidi-sdk.html).

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace CreateLoopbackPort
{
	public class Program
	{
		private const uint SysExBufferLength = 65535;

		[DllImport("teVirtualMIDI.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern IntPtr virtualMIDICreatePortEx3(string portName, IntPtr callback, IntPtr dwCallbackInstance, uint maxSysexLength, uint flags, ref Guid manufacturer, ref Guid product);

		[DllImport("teVirtualMIDI.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern bool virtualMIDISendData(IntPtr midiPort, byte[] midiDataBytes, uint length);

		[DllImport("teVirtualMIDI.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern bool virtualMIDIGetData(IntPtr midiPort, [Out] byte[] midiDataBytes, ref uint length);

		public static void Main(string[] args)
		{
			var manufacturer = new Guid("aa4e075f-3504-4aab-9b06-9a4104a91cf0");
			var product = new Guid("bb4e075f-3504-4aab-9b06-9a4104a91cf0");

			var portHandle = virtualMIDICreatePortEx3(args[0], IntPtr.Zero, IntPtr.Zero, 65535, 12, ref manufacturer, ref product);

			var thread = new Thread(() =>
			{
				while (true)
				{
					var commandBytes = GetCommandBytes(portHandle);
					SendCommandBytes(portHandle, commandBytes);
				}
			});

			thread.Start();
			Console.ReadKey();
		}

		private static void SendCommandBytes(IntPtr portHandle, byte[] commandBytes)
		{
			if (commandBytes == null || commandBytes.Length == 0)
				return;

			virtualMIDISendData(portHandle, commandBytes, (uint)commandBytes.Length);
		}

		private static byte[] GetCommandBytes(IntPtr portHandle)
		{
			var length = SysExBufferLength;
			var buffer = new byte[length];

			virtualMIDIGetData(portHandle, buffer, ref length);

			var commandBytes = new byte[length];
			Array.Copy(buffer, commandBytes, length);
			return commandBytes;
		}
	}
}
