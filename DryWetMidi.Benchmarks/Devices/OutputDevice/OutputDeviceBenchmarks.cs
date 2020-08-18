using System.Threading;
using BenchmarkDotNet.Attributes;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Benchmarks.Devices
{
    [TestFixture]
    public sealed class OutputDeviceBenchmarks : BenchmarkTest
    {
        #region Nested classes

        public abstract class Benchmarks_OutputDevice_SendEvent
        {
            private OutputDevice _outputDevice;
            private MidiEvent _event;

            [GlobalSetup]
            public void GlobalSetup()
            {
                _event = GetMidiEvent();
                _outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);
            }

            [GlobalCleanup]
            public void GlobalCleanup()
            {
                _outputDevice.Dispose();
            }

            [IterationSetup]
            public void IterationSetup()
            {
                Thread.Sleep(250);
            }

            [Benchmark]
            public void SendEvent()
            {
                _outputDevice.SendEvent(_event);
            }

            [Benchmark]
            public void SendMultipleEvents()
            {
                for (var i = 0; i < 10; i++)
                {
                    _outputDevice.SendEvent(_event);
                }
            }

            protected abstract MidiEvent GetMidiEvent();
        }

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Throughput)]
        public class Benchmarks_OutputDevice_SendEvent_Channel : Benchmarks_OutputDevice_SendEvent
        {
            protected override MidiEvent GetMidiEvent() => new NoteOnEvent();
        }

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Throughput)]
        public class Benchmarks_OutputDevice_SendEvent_SysEx : Benchmarks_OutputDevice_SendEvent
        {
            protected override MidiEvent GetMidiEvent() => new NormalSysExEvent(new byte[] { 0x15, 0x2F });
        }

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Throughput)]
        public class Benchmarks_OutputDevice_SendEvent_SystemCommon : Benchmarks_OutputDevice_SendEvent
        {
            protected override MidiEvent GetMidiEvent() => new TuneRequestEvent();
        }

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Throughput)]
        public class Benchmarks_OutputDevice_SendEvent_SystemRealTime : Benchmarks_OutputDevice_SendEvent
        {
            protected override MidiEvent GetMidiEvent() => new StopEvent();
        }

        #endregion

        #region Test methods

        [Test]
        public void SendEvent_Channel()
        {
            RunBenchmarks<Benchmarks_OutputDevice_SendEvent_Channel>();
        }

        [Test]
        public void SendEvent_SysEx()
        {
            RunBenchmarks<Benchmarks_OutputDevice_SendEvent_SysEx>();
        }

        [Test]
        public void SendEvent_SystemCommon()
        {
            RunBenchmarks<Benchmarks_OutputDevice_SendEvent_SystemCommon>();
        }

        [Test]
        public void SendEvent_SystemRealTime()
        {
            RunBenchmarks<Benchmarks_OutputDevice_SendEvent_SystemRealTime>();
        }

        #endregion
    }
}
