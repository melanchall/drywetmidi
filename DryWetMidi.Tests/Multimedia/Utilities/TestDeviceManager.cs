using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    internal static class TestDeviceManager
    {
        internal sealed class LoopbackDevice
        {
            public sealed class OutputEndpoint : IOutputEndpoint
            {
                public event EventHandler<MidiEventSentEventArgs> EventSent;

                public void Dispose()
                {
                }

                public void PrepareForEventsSending()
                {
                }

                public void SendEvent(MidiEvent midiEvent)
                {
                    EventSent?.Invoke(this, new MidiEventSentEventArgs(midiEvent));
                }
            }

            public sealed class InputEndpoint : IInputEndpoint
            {
                public event EventHandler<MidiEventReceivedEventArgs> EventReceived;

                public void Dispose()
                {
                }

                public bool IsListeningForEvents { get; private set; }

                public void StartEventsListening()
                {
                    IsListeningForEvents = true;
                }

                public void StopEventsListening()
                {
                    IsListeningForEvents = false;
                }

                public void FireEventReceived(MidiEvent midiEvent)
                {
                    EventReceived?.Invoke(this, new MidiEventReceivedEventArgs(midiEvent));
                }
            }

            public LoopbackDevice()
            {
                Output.EventSent += (sender, e) => Input.FireEventReceived(e.Event);
            }

            public OutputEndpoint Output { get; } = new OutputEndpoint();

            public InputEndpoint Input { get; } = new InputEndpoint();
        }

        private static readonly Dictionary<string, LoopbackDevice> _devices = new Dictionary<string, LoopbackDevice>();

        public static IInputEndpoint GetInputEndpoint(string endpointName)
        {
            if (!_devices.TryGetValue(endpointName, out var device))
                _devices.Add(endpointName, device = new LoopbackDevice());

            return device.Input;
        }

        public static IOutputEndpoint GetOutputEndpoint(string endpointName)
        {
            if (!_devices.TryGetValue(endpointName, out var device))
                _devices.Add(endpointName, device = new LoopbackDevice());

            return device.Output;
        }
    }
}
