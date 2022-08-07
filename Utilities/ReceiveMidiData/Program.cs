using Melanchall.Common;
using Melanchall.DryWetMidi.Multimedia;
using System;

namespace Melanchall.ReceiveMidiData
{
    internal class Program
    {
        static void Main(string[] args)
        {
            UiUtilities.WriteHello();
            UiUtilities.WriteUtilityDescription(@"
The tool listens for incoming MIDI data from the selected
input MIDI device and immediately prints the data
");

            UiUtilities.WriteLine("Here the list of all input MIDI devices in the system:");
            UiUtilities.WriteLine();

            var inputDevices = InputDevice.GetAll();
            UiUtilities.WriteNumberedList(inputDevices, d => d.Name);
            UiUtilities.WriteLine();

            var inputDevice = UiUtilities.SelectElementByNumber("Select device to listen data from (type number)", inputDevices);
            UiUtilities.WriteLine();

            UiUtilities.WriteLine($"Selected device: {inputDevice.Name}");
            UiUtilities.WriteLine("Starting listening MIDI data...");
            inputDevice.EventReceived += OnEventReceived;
            inputDevice.StartEventsListening();
            UiUtilities.WriteLine("Listening... Press Esc to stop the utility");
            UiUtilities.WriteLine();

            UiUtilities.WaitForOneOfKeys(ConsoleKey.Escape);

            UiUtilities.WriteLine("Releasing the device...");
            inputDevice.EventReceived -= OnEventReceived;
            inputDevice.Dispose();
            UiUtilities.WriteLine("Exited.");
        }

        private static void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
        {
            UiUtilities.WriteLine(e.Event.ToString());
        }
    }
}