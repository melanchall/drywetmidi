using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Benchmarks.Core
{
    [TestFixture]
    public sealed class MidiFileReadingHandlerBenchmarks : BenchmarkTest
    {
        #region Nested classes

        private sealed class EmptyHandler : ReadingHandler
        {
            public EmptyHandler()
                : base(TargetScope.Event | TargetScope.File | TargetScope.TrackChunk)
            {
            }
        }

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_ReadingHandler
        {
            [Benchmark]
            public void Read_WithoutHandlers()
            {
                var midiFile = MidiFile.Read(TestFilesProvider.GetMiscFile_14000events());
            }

            [Benchmark]
            public void Read_WithEmptyHandler()
            {
                var handler = new EmptyHandler();
                var settings = new ReadingSettings();
                settings.ReadingHandlers.Add(handler);

                var midiFile = MidiFile.Read(TestFilesProvider.GetMiscFile_14000events(), settings);
            }
        }

        #endregion

        #region Test methods

        [Test]
        public void ReadMidiFileUsingHandler()
        {
            RunBenchmarks<Benchmarks_ReadingHandler>();
        }

        #endregion
    }
}
