using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Linq;
using System.Threading;

namespace Melanchall.CheckDwmApi
{
    internal class SendReceiveViaVirtualDeviceTask : ITask
    {
        private sealed class EventsSentReceivedData
        {
            public MidiEvent SentNoteOnEvent { get; set; }

            public MidiEvent ReceivedNoteOnEvent { get; set; }

            public MidiEvent SentSysExEvent { get; set; }

            public MidiEvent ReceivedSysExEvent { get; set; }
        }

        private const string DeviceName = "TestVirtualMidiDevice";

        private static readonly TimeSpan EventSentReceivedTimeout = TimeSpan.FromSeconds(5);
        private static readonly MidiEventEqualityCheckSettings MidiEventEqualityCheckSettings = new()
        {
            CompareDeltaTimes = false
        };

        public string GetTitle() =>
            "Send and receive MIDI data";

        public string GetDescription() => @"
The tool will send several MIDI events to a virtual MIDI device
and receive them back via the same device.";

        public void Execute(
            ToolOptions toolOptions,
            ReportWriter reportWriter)
        {
            VirtualDevice virtualDevice = null;
            InputDevice inputDevice = null;
            OutputDevice outputDevice = null;

            var deviceName = DeviceName;

            if (OperatingSystem.IsMacOS())
                virtualDevice = VirtualDevice.Create(deviceName);

            if (OperatingSystem.IsWindows() && !toolOptions.NonInteractive)
                deviceName = GetDeviceName();

            var eventsSentReceivedData = new EventsSentReceivedData();

            try
            {
                inputDevice = InputDevice.GetByName(deviceName);
                SubscribeToEventReceived(reportWriter, inputDevice, eventsSentReceivedData);
                inputDevice.StartEventsListening();

                outputDevice = OutputDevice.GetByName(deviceName);
                SubscribeToEventSent(reportWriter, outputDevice, eventsSentReceivedData);

                var noteOnEvent = new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)90)
                {
                    Channel = (FourBitNumber)4
                };

                SendEvent(noteOnEvent, reportWriter, outputDevice);
                CheckEventSent(noteOnEvent, eventsSentReceivedData, reportWriter);
                CheckEventReceived(noteOnEvent, eventsSentReceivedData, reportWriter);

                var sysExEvent = new NormalSysExEvent([0x7E, 0x7F, 0x09, 0x01, 0xF7]);

                SendEvent(sysExEvent, reportWriter, outputDevice);
                CheckEventSent(sysExEvent, eventsSentReceivedData, reportWriter);
                CheckEventReceived(sysExEvent, eventsSentReceivedData, reportWriter);
            }
            finally
            {
                if (virtualDevice == null)
                {
                    inputDevice?.Dispose();
                    outputDevice?.Dispose();
                }
                else
                {
                    virtualDevice?.Dispose();
                }
            }
        }

        private string GetDeviceName()
        {
            Console.WriteLine("You need to have a virtual MIDI device in your system.");

            string[] GetCandidateDevicesNames() => InputDevice.GetAll()
                .Select(d => d.Name)
                .Intersect(OutputDevice.GetAll().Select(d => d.Name))
                .ToArray();

            string[] candidateDevicesNames = null;

            while (!(candidateDevicesNames = GetCandidateDevicesNames()).Any())
            {
                Console.WriteLine("Unfortunately there are no candidate devices found.");
                Console.WriteLine("Please, install a virtual MIDI device (e.g., loopMIDI, loopBe1, and so on).");
                Console.WriteLine("Press Enter when you have a device installed...");
                Console.ReadLine();
            }

            Console.WriteLine("Candidate virtual MIDI devices:");

            for (var i = 0; i < candidateDevicesNames.Length; i++)
            {
                Console.WriteLine($"  [{i + 1}] {candidateDevicesNames[i]}");
            }

            while (true)
            {
                Console.Write("Please, enter the number of the device to use: ");
                var input = Console.ReadLine();

                if (int.TryParse(input, out var deviceNumber) &&
                    deviceNumber >= 1 &&
                    deviceNumber <= candidateDevicesNames.Length)
                {
                    var deviceName = candidateDevicesNames[deviceNumber - 1];
                    Console.WriteLine($"Device '{deviceName}' will be used.");
                    return deviceName;
                }

                Console.WriteLine("Invalid device number.");
            }
        }

        private void CheckEventReceived(
            MidiEvent midiEvent,
            EventsSentReceivedData eventsData,
            ReportWriter reportWriter)
        {
            reportWriter.WriteOperationTitle($"Checking that MIDI event '{midiEvent}' was received...");

            var received = SpinWait.SpinUntil(
                () => MidiEvent.Equals(
                    midiEvent is NoteOnEvent ? eventsData.ReceivedNoteOnEvent : eventsData.ReceivedSysExEvent,
                    midiEvent,
                    MidiEventEqualityCheckSettings),
                EventSentReceivedTimeout);
            
            if (!received)
                throw new TaskFailedException("MIDI event was not received.");

            reportWriter.WriteOperationSubTitle("received");
        }

        private void CheckEventSent(
            MidiEvent midiEvent,
            EventsSentReceivedData eventsData,
            ReportWriter reportWriter)
        {
            reportWriter.WriteOperationTitle($"Checking that MIDI event '{midiEvent}' was sent...");

            var sent = SpinWait.SpinUntil(
                () => MidiEvent.Equals(
                    midiEvent is NoteOnEvent ? eventsData.SentNoteOnEvent : eventsData.SentSysExEvent,
                    midiEvent,
                    MidiEventEqualityCheckSettings),
                EventSentReceivedTimeout);
            
            if (!sent)
                throw new TaskFailedException("MIDI event was not sent.");

            reportWriter.WriteOperationSubTitle("sent");
        }

        private void SendEvent(
            MidiEvent midiEvent,
            ReportWriter reportWriter,
            OutputDevice outputDevice)
        {
            reportWriter.WriteOperationTitle($"Sending MIDI event '{midiEvent}'...");

            try
            {
                outputDevice.SendEvent(midiEvent);
                reportWriter.WriteOperationSubTitle("sent");
            }
            catch (Exception ex)
            {
                throw new TaskFailedException("Failed to send MIDI event.", ex);
            }
        }

        private void SubscribeToEventReceived(
            ReportWriter reportWriter,
            InputDevice inputDevice,
            EventsSentReceivedData eventsData)
        {
            reportWriter.WriteOperationTitle("Subscribing to MIDI event received...");

            try
            {
                inputDevice.EventReceived += (_, e) =>
                {
                    reportWriter.WriteEventInfo($"MIDI event received: '{e.Event}'.");
                    if (e.Event is NoteOnEvent noteOnEvent)
                        eventsData.ReceivedNoteOnEvent = noteOnEvent;
                    else if (e.Event is SysExEvent sysExEvent)
                        eventsData.ReceivedSysExEvent = sysExEvent;
                };
                reportWriter.WriteOperationSubTitle("subscribed");
            }
            catch (Exception ex)
            {
                throw new TaskFailedException("Failed to subscribe to MIDI event received.", ex);
            }
        }

        private void SubscribeToEventSent(
            ReportWriter reportWriter,
            OutputDevice outputDevice,
            EventsSentReceivedData eventsData)
        {
            reportWriter.WriteOperationTitle("Subscribing to MIDI event sent...");
            
            try
            {
                outputDevice.EventSent += (_, e) =>
                {
                    reportWriter.WriteEventInfo($"MIDI event sent: '{e.Event}'.");
                    if (e.Event is NoteOnEvent noteOnEvent)
                        eventsData.SentNoteOnEvent = noteOnEvent;
                    else if (e.Event is SysExEvent sysExEvent)
                        eventsData.SentSysExEvent = sysExEvent;
                };
                reportWriter.WriteOperationSubTitle("subscribed");
            }
            catch (Exception ex)
            {
                throw new TaskFailedException("Failed to subscribe to MIDI event sent.", ex);
            }
        }
    }
}
