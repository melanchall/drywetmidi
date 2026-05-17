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

        private static bool _listenToAllEndpoints = false;

        static void Main(string[] args)
        {
            UiUtilities.WriteHello();
            UiUtilities.WriteUtilityDescription(@"
The tool listens for incoming MIDI data from the selected
input MIDI endpoint and immediately prints the data");

            UiUtilities.WriteLine("Here the list of all input MIDI endpoints in the system:");
            UiUtilities.WriteLine();

            var inputEndpoints = InputEndpoint.GetAll().Concat(new InputEndpoint[] { null }).ToArray();
            UiUtilities.WriteNumberedList(inputEndpoints, d => d?.Name ?? AllDevicesName);
            UiUtilities.WriteLine();

            var inputEndpoint = UiUtilities.SelectElementByNumber("Select endpoint to listen data from (type number)", inputEndpoints);
            _listenToAllEndpoints = inputEndpoint == null;
            UiUtilities.WriteLine();

            UiUtilities.WriteLine($"Selected endpoint: {inputEndpoint?.Name ?? AllDevicesName}");
            UiUtilities.WriteLine("Starting listening MIDI data...");

            if (_listenToAllEndpoints)
                StartEventsListeningOnAllEndpoints(inputEndpoints);
            else
                StartEventsListeningOnSpecificEndpoint(inputEndpoint);

            UiUtilities.WriteLine("Listening... Press Esc to stop the utility");
            UiUtilities.WriteLine();

            UiUtilities.WaitForOneOfKeys(ConsoleKey.Escape);

            UiUtilities.WriteLine("Releasing the device...");

            if (_listenToAllEndpoints)
                StopEventsListeningOnAllEndpoints(inputEndpoints);
            else
                StopEventsListeningOnSpecificEndpoint(inputEndpoint);

            UiUtilities.WriteLine("Exited.");
        }

        private static void StartEventsListeningOnSpecificEndpoint(InputEndpoint inputEndpoint)
        {
            inputEndpoint.EventReceived += OnEventReceived;
            inputEndpoint.StartEventsListening();
        }

        private static void StopEventsListeningOnSpecificEndpoint(InputEndpoint inputEndpoint)
        {
            inputEndpoint.EventReceived -= OnEventReceived;
            inputEndpoint.Dispose();
        }

        private static void StartEventsListeningOnAllEndpoints(ICollection<InputEndpoint> inputEndpoints)
        {
            foreach (var inputEndpoint in inputEndpoints)
            {
                if (inputEndpoint == null)
                    continue;

                StartEventsListeningOnSpecificEndpoint(inputEndpoint);
            }
        }

        private static void StopEventsListeningOnAllEndpoints(ICollection<InputEndpoint> inputEndpoints)
        {
            foreach (var inputEndpoint in inputEndpoints)
            {
                if (inputEndpoint == null)
                    continue;

                StopEventsListeningOnSpecificEndpoint(inputEndpoint);
            }
        }

        private static void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
        {
            var endpointName = _listenToAllEndpoints ? $"{((InputEndpoint)sender).Name}: " : string.Empty;
            UiUtilities.WriteLine($"{endpointName}{e.Event}");
        }
    }
}