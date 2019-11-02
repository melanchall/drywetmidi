using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Benchmarks.Smf
{
    [TestFixture]
    public class MidiFileReadBenchmarks : BenchmarkTest
    {
        #region Enums

        public enum MidiFileSize
        {
            Small,
            Middle,
            Large
        }

        #endregion

        #region Constants

        private const string FilesPath = @"..\..\..\..\Resources\MIDI files\Valid";

        #endregion

        #region Nested classes

        public abstract class Benchmarks
        {
            public abstract MidiFileFormat FileFormat { get; }

            public abstract MidiFileSize FileSize { get; }

            [Benchmark]
            public void Read()
            {
                MidiFileReadBenchmarks.Read(FileFormat, FileSize);
            }
        }

        [InProcessSimpleJob(RunStrategy.Throughput)]
        public class Benchmarks_SingleTrack_Small : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.SingleTrack;

            public override MidiFileSize FileSize => MidiFileSize.Small;
        }

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_SingleTrack_Middle : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.SingleTrack;

            public override MidiFileSize FileSize => MidiFileSize.Middle;
        }

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_SingleTrack_Large : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.SingleTrack;

            public override MidiFileSize FileSize => MidiFileSize.Large;
        }

        [InProcessSimpleJob(RunStrategy.Throughput)]
        public class Benchmarks_MultiTrack_Small : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.MultiTrack;

            public override MidiFileSize FileSize => MidiFileSize.Small;
        }

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_MultiTrack_Middle : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.MultiTrack;

            public override MidiFileSize FileSize => MidiFileSize.Middle;
        }

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_MultiTrack_Large : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.MultiTrack;

            public override MidiFileSize FileSize => MidiFileSize.Large;
        }

        [InProcessSimpleJob(RunStrategy.Throughput)]
        public class Benchmarks_MultiSequence_Small : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.MultiSequence;

            public override MidiFileSize FileSize => MidiFileSize.Small;
        }

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_MultiSequence_Middle : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.MultiSequence;

            public override MidiFileSize FileSize => MidiFileSize.Middle;
        }

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_MultiSequence_Large : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.MultiSequence;

            public override MidiFileSize FileSize => MidiFileSize.Large;
        }

        #endregion

        #region Test methods

        [TestCase(typeof(Benchmarks_SingleTrack_Small))]
        [TestCase(typeof(Benchmarks_SingleTrack_Middle))]
        [TestCase(typeof(Benchmarks_SingleTrack_Large))]
        [TestCase(typeof(Benchmarks_MultiTrack_Small))]
        [TestCase(typeof(Benchmarks_MultiTrack_Middle))]
        [TestCase(typeof(Benchmarks_MultiTrack_Large))]
        [TestCase(typeof(Benchmarks_MultiSequence_Small))]
        [TestCase(typeof(Benchmarks_MultiSequence_Middle))]
        [TestCase(typeof(Benchmarks_MultiSequence_Large))]
        public void Read(Type type)
        {
            var instance = Activator.CreateInstance(type);
            var fileFormat = (MidiFileFormat)type.GetProperty(nameof(Benchmarks.FileFormat)).GetValue(instance);
            var fileSize = (MidiFileSize)type.GetProperty(nameof(Benchmarks.FileSize)).GetValue(instance);

            var eventsCount = GetEventsCount(fileFormat, fileSize);
            RunBenchmarks(type, new MidiFileEventsCountsColumn(eventsCount));
        }

        #endregion

        #region Private methods

        private int[] GetEventsCount(MidiFileFormat midiFileFormat, MidiFileSize midiFileSize)
        {
            var result = new List<int>();

            foreach (var midiFile in GetFiles(midiFileFormat, midiFileSize))
            {
                var events = midiFile.GetTrackChunks().SelectMany(c => c.Events).ToList();
                result.Add(events.Count);
            }

            return result.ToArray();
        }

        protected static void Read(MidiFileFormat midiFileFormat, MidiFileSize midiFileSize)
        {
            GetFiles(midiFileFormat, midiFileSize).ToList();
        }

        private static IEnumerable<MidiFile> GetFiles(MidiFileFormat midiFileFormat, MidiFileSize midiFileSize)
        {
            foreach (var filePath in Directory.GetFiles(Path.Combine(FilesPath, midiFileFormat.ToString(), midiFileSize.ToString())))
            {
                yield return MidiFile.Read(filePath);
            }
        }

        #endregion
    }
}
