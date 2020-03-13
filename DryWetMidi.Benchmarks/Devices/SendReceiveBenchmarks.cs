using System.Threading;
using BenchmarkDotNet.Attributes;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;
using System.Linq;
using Melanchall.DryWetMidi.Tests.Common;

namespace Melanchall.DryWetMidi.Benchmarks.Devices
{
    [TestFixture]
    public sealed class SendReceiveBenchmarks : BenchmarkTest
    {
        public abstract class Benchmarks
        {
            protected InputDevice _inputDevice;
            protected OutputDevice _outputDevice;

            protected int _received;
            protected int _sent;

            protected readonly MidiEvent _channelMidiEvent = new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)100) { Channel = (FourBitNumber)12 };
            protected readonly MidiEvent _sysExMidiEvent = new NormalSysExEvent(Enumerable.Range(0, 10).Select(_ => (byte)0x56).Concat(new byte[] { 0xF7 }).ToArray());

            [GlobalSetup]
            public void Setup()
            {
                _outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
                _outputDevice.PrepareForEventsSending();
                _outputDevice.EventSent += (_, e) => _sent++;

                _inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
                _inputDevice.EventReceived += (_, e) => _received++;
                _inputDevice.StartEventsListening();
            }

            [GlobalCleanup]
            public void Cleanup()
            {
                _inputDevice.Dispose();
                _outputDevice.Dispose();
            }
        }

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Throughput, warmupCount: 16, targetCount: 16, launchCount: 16, invocationCount: 16)]
        public class Benchmarks_SendReceive_Channel : Benchmarks
        {
            [Benchmark]
            public void ReceiveEvent()
            {
                _received = 0;

                _outputDevice.SendEvent(_channelMidiEvent);
                SpinWait.SpinUntil(() => _received == 1);
            }

            [Benchmark]
            public void SendEvent()
            {
                _sent = 0;

                _outputDevice.SendEvent(_channelMidiEvent);
                SpinWait.SpinUntil(() => _sent == 1);
            }
        }

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Throughput, warmupCount: 32, targetCount: 32, launchCount: 32, invocationCount: 32)]
        public class Benchmarks_SendReceive_SysEx : Benchmarks
        {
            [Benchmark]
            public void ReceiveEvent()
            {
                _received = 0;

                _outputDevice.SendEvent(_sysExMidiEvent);
                SpinWait.SpinUntil(() => _received == 1);
            }

            [Benchmark]
            public void SendEvent()
            {
                _sent = 0;

                _outputDevice.SendEvent(_sysExMidiEvent);
                SpinWait.SpinUntil(() => _sent == 1);
            }
        }

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_SendReceive_Batch_Channel : Benchmarks
        {
            [Benchmark]
            public void ReceiveEvent()
            {
                _received = 0;

                const int iterationsCount = 100;

                for (var i = 0; i < iterationsCount; i++)
                {
                    _outputDevice.SendEvent(_channelMidiEvent);
                }

                SpinWait.SpinUntil(() => _received == iterationsCount);
            }

            [Benchmark]
            public void SendEvent()
            {
                _sent = 0;

                const int iterationsCount = 100;

                for (var i = 0; i < iterationsCount; i++)
                {
                    _outputDevice.SendEvent(_channelMidiEvent);
                }

                SpinWait.SpinUntil(() => _sent == iterationsCount);
            }
        }

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_SendReceive_Batch_SysEx : Benchmarks
        {
            [Benchmark]
            public void ReceiveEvent()
            {
                _received = 0;

                const int iterationsCount = 100;

                for (var i = 0; i < iterationsCount; i++)
                {
                    _outputDevice.SendEvent(_sysExMidiEvent);
                }

                SpinWait.SpinUntil(() => _received == iterationsCount);
            }

            [Benchmark]
            public void SendEvent()
            {
                _sent = 0;

                const int iterationsCount = 100;

                for (var i = 0; i < iterationsCount; i++)
                {
                    _outputDevice.SendEvent(_sysExMidiEvent);
                }

                SpinWait.SpinUntil(() => _sent == iterationsCount);
            }
        }

        [Test]
        public void SendReceiveEvent_Channel()
        {
            RunBenchmarks<Benchmarks_SendReceive_Channel>();
        }

        // TODO
        // [Test]
        public void SendReceiveEvent_SysEx()
        {
            RunBenchmarks<Benchmarks_SendReceive_SysEx>();
        }

        // TODO
        // [Test]
        public void SendReceiveEvent_Batch_Channel()
        {
            RunBenchmarks<Benchmarks_SendReceive_Batch_Channel>();
        }

        // TODO
        // [Test]
        public void SendReceiveEvent_Batch_SysEx()
        {
            RunBenchmarks<Benchmarks_SendReceive_Batch_SysEx>();
        }
    }
}
