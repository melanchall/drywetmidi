using System.Threading;
using BenchmarkDotNet.Attributes;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Benchmarks.Devices
{
    [TestFixture]
    public sealed class InputDeviceBenchmarks : BenchmarkTest
    {
        #region Nested classes

        public abstract class Benchmarks_InputDevice_SendReceiveEvent
        {
            private OutputDevice _outputDevice;
            private InputDevice _inputDevice;
            private MidiEvent _event;

            private ManualResetEvent _manualResetEvent;

            [GlobalSetup]
            public void GlobalSetup()
            {
                _event = GetMidiEvent();

                _outputDevice = OutputDevice.GetByName(MidiDevicesNames.DeviceA);

                _inputDevice = InputDevice.GetByName(MidiDevicesNames.DeviceA);
                _inputDevice.EventReceived += OnEventReceived;
                _inputDevice.StartEventsListening();
            }

            [GlobalCleanup]
            public void GlobalCleanup()
            {
                _inputDevice.EventReceived -= OnEventReceived;
                _inputDevice.Dispose();
                _outputDevice.Dispose();
            }

            [IterationSetup]
            public void IterationSetup()
            {
                Thread.Sleep(500);
                _manualResetEvent?.Dispose();
                _manualResetEvent = new ManualResetEvent(false);
            }

            [Benchmark]
            public void SendReceiveEvent()
            {
                _outputDevice.SendEvent(_event);
                _manualResetEvent.WaitOne();
            }

            protected abstract MidiEvent GetMidiEvent();

            private void OnEventReceived(object sender, MidiEventReceivedEventArgs e)
            {
                _manualResetEvent.Set();
            }
        }

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Throughput)]
        public class Benchmarks_InputDevice_SendReceiveEvent_Channel : Benchmarks_InputDevice_SendReceiveEvent
        {
            protected override MidiEvent GetMidiEvent() => new NoteOnEvent();
        }

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Throughput)]
        public class Benchmarks_InputDevice_SendReceiveEvent_SysEx : Benchmarks_InputDevice_SendReceiveEvent
        {
            protected override MidiEvent GetMidiEvent() => new NormalSysExEvent(new byte[] { 0x15, 0x2F, 0xF7 });
        }

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Throughput)]
        public class Benchmarks_InputDevice_SendReceiveEvent_SystemCommon : Benchmarks_InputDevice_SendReceiveEvent
        {
            protected override MidiEvent GetMidiEvent() => new TuneRequestEvent();
        }

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Throughput)]
        public class Benchmarks_InputDevice_SendReceiveEvent_SystemRealTime : Benchmarks_InputDevice_SendReceiveEvent
        {
            protected override MidiEvent GetMidiEvent() => new StopEvent();
        }

        #endregion

        #region Test methods

        [Test]
        public void SendReceiveEvent_Channel()
        {
            RunBenchmarks<Benchmarks_InputDevice_SendReceiveEvent_Channel>();
        }

        [Test]
        public void SendReceiveEvent_SysEx()
        {
            RunBenchmarks<Benchmarks_InputDevice_SendReceiveEvent_SysEx>();
        }

        [Test]
        public void SendReceiveEvent_SystemCommon()
        {
            RunBenchmarks<Benchmarks_InputDevice_SendReceiveEvent_SystemCommon>();
        }

        [Test]
        public void SendReceiveEvent_SystemRealTime()
        {
            RunBenchmarks<Benchmarks_InputDevice_SendReceiveEvent_SystemRealTime>();
        }

        #endregion
    }
}
