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
output MIDI endpoint");

            UiUtilities.WriteLine("Here the list of all output MIDI endpoints in the system:");
            UiUtilities.WriteLine();

            var outputEndpoints = OutputEndpoint.GetAll();
            UiUtilities.WriteNumberedList(outputEndpoints, d => d.Name);
            UiUtilities.WriteLine();

            var outputEndpoint = UiUtilities.SelectElementByNumber("Select endpoint to send data to (type number)", outputEndpoints);
            UiUtilities.WriteLine();

            UiUtilities.WriteLine($"Selected endpoint: {outputEndpoint.Name}");
            outputEndpoint.EventSent += OnEventSent;

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
                    var result = DataSender.SendData(outputEndpoint);
                    if (result == SendResult.Sent)
                        break;
                }
            }

            UiUtilities.WriteLine("Releasing the endpoint...");
            outputEndpoint.EventSent -= OnEventSent;
            outputEndpoint.Dispose();
            UiUtilities.WriteLine("Exited.");
        }

        private static void OnEventSent(object sender, MidiEventSentEventArgs e)
        {
            UiUtilities.WriteLine($"Event sent: {e.Event}");
        }
    }
}