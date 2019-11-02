using System.Threading;
using BenchmarkDotNet.Attributes;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Benchmarks.Devices
{
    [TestFixture]
    public sealed class SendReceiveBenchmarks : BenchmarkTest
    {
        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Throughput, warmupCount: 32, targetCount: 32, launchCount: 32, invocationCount: 32)]
        public class Benchmarks
        {
            private InputDevice _inputDevice;
            private OutputDevice _outputDevice;

            private bool _received;
            private bool _sent;

            private readonly MidiEvent _midiEvent = new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)100) { Channel = (FourBitNumber)12 };

            [GlobalSetup]
            public void Setup()
            {
                _outputDevice = OutputDevice.GetByName("MIDI A");
                _outputDevice.EventSent += (_, e) => _sent = true;

                _inputDevice = InputDevice.GetByName("MIDI A");
                _inputDevice.EventReceived += (_, e) => _received = true;
                _inputDevice.StartEventsListening();
            }

            [GlobalCleanup]
            public void Cleanup()
            {
                _inputDevice.Dispose();
                _outputDevice.Dispose();
            }

            [Benchmark]
            public void ReceiveEvent()
            {
                _received = false;

                _outputDevice.SendEvent(_midiEvent);
                SpinWait.SpinUntil(() => _received);
            }

            [Benchmark]
            public void SendEvent()
            {
                _sent = false;

                _outputDevice.SendEvent(_midiEvent);
                SpinWait.SpinUntil(() => _sent);
            }
        }

        [Test]
        public void SendReceiveEvent()
        {
            RunBenchmarks<Benchmarks>();
        }
    }
}
