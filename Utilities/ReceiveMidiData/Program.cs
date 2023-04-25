using Melanchall.Common;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.ReceiveMidiData
{
    internal class Program
    {
        private const string AllDevicesName = "ALL DEVICES";

        private static bool _listenToAllDevices = false;

        static void Main(string[] args)
        {
            UiUtilities.WriteHello();
            UiUtilities.WriteUtilityDescription(@"
The tool listens for incoming MIDI data from the selected
input MIDI device and immediately prints the data
");

            UiUtilities.WriteLine("Here the list of all input MIDI devices in the system:");
            UiUtilities.WriteLine();

            var inputDevices = InputDevice.GetAll().Concat(new InputDevice[] { null }).ToArray();
            UiUtilities.WriteNumberedList(inputDevices, d => d?.Name ?? AllDevicesName);
            UiUtilities.WriteLine();

            var inputDevice = UiUtilities.SelectElementByNumber("Select device to listen data from (type number)", inputDevices);
            _listenToAllDevices = inputDevice == null;
            UiUtilities.WriteLine();

            UiUtilities.WriteLine($"Selected device: {inputDevice?.Name ?? AllDevicesName}");
            UiUtilities.WriteLine("Starting listening MIDI data...");

            if (_listenToAllDevices)
                StartEventsListeningOnAllDevices(inputDevices);
            else
                StartEventsListeningOnSpecificDevice(inputDevice);

            UiUtilities.WriteLine("Listening... Press Esc to stop the utility");
            UiUtilities.WriteLine();

            UiUtilities.WaitForOneOfKeys(ConsoleKey.Escape);

            UiUtilities.WriteLine("Releasing the device...");

            if (_listenToAllDevices)
                StopEventsListeningOnAllDevices(inputDevices);
            else
                StopEventsListeningOnSpecificDevice(inputDevice);

            UiUtilities.WriteLine("Exited.");
        }

        private static void StartEventsListeningOnSpecificDevice(InputDevice inputDevice)
        {
            inputDevice.EventReceived += OnEventReceived;
            inputDevice.StartEventsListening();
        }

        private static void StopEventsListeningOnSpecificDevice(InputDevice inputDevice)
        {
            inputDevice.EventReceived -= OnEventReceived;
            inputDevice.Dispose();
        }

        private static void StartEventsListeningOnAllDevices(ICollection<InputDevice> inputDevices)
        {
            foreach (var inputDevice in inputDevices)
            {
                if (inputDevice == null)
                    continue;

                StartEventsListeningOnSpecificDevice(inputDevice);
            }
        }

        private static void StopEventsListeningOnAllDevices(ICollection<InputDevice> inputDevices)
        {
            foreach (var inputDevice in inputDevices)
            {
                if (inputDevice == null)
                    continue;

                StopEventsListeningOnSpecificDevice(inputDevice);
            }
        }

        private static void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
        {
            var deviceName = _listenToAllDevices ? $"{((InputDevice)sender).Name}: " : string.Empty;
            UiUtilities.WriteLine($"{deviceName}{e.Event}");
        }
    }
}