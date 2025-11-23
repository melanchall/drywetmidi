using Melanchall.Common;
using Melanchall.DryWetMidi.Multimedia;
using System;

namespace Melanchall.SendMidiData
{
    internal class Program
    {
        static void Main(string[] args)
        {
            UiUtilities.WriteHello();
            UiUtilities.WriteUtilityDescription(@"
The tool sends note and other events to the selected
output MIDI device");

            UiUtilities.WriteLine("Here the list of all output MIDI devices in the system:");
            UiUtilities.WriteLine();

            var outputDevices = OutputDevice.GetAll();
            UiUtilities.WriteNumberedList(outputDevices, d => d.Name);
            UiUtilities.WriteLine();

            var outputDevice = UiUtilities.SelectElementByNumber("Select device to send data to (type number)", outputDevices);
            UiUtilities.WriteLine();

            UiUtilities.WriteLine($"Selected device: {outputDevice.Name}");
            outputDevice.EventSent += OnEventSent;

            while (true)
            {
                var operations = Enum.GetValues<Operation>();
                UiUtilities.WriteNumberedList(operations, o => o.ToString());
                UiUtilities.WriteLine();

                var operation = UiUtilities.SelectElementByNumber("Select operation (type number)", operations);
                UiUtilities.WriteLine();

                if (operation == Operation.Exit)
                    break;

                while (true)
                {
                    var result = DataSender.SendData(outputDevice);
                    if (result == SendResult.Sent)
                        break;
                }
            }

            UiUtilities.WriteLine("Releasing the device...");
            outputDevice.EventSent -= OnEventSent;
            outputDevice.Dispose();
            UiUtilities.WriteLine("Exited.");
        }

        private static void OnEventSent(object sender, MidiEventSentEventArgs e)
        {
            UiUtilities.WriteLine($"Event sent: {e.Event}");
        }
    }
}