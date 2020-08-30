using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Benchmarks.Core
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

        #region Nested classes

        public abstract class Benchmarks
        {
            public abstract MidiFileFormat FileFormat { get; }

            public abstract MidiFileSize FileSize { get; }

            [Benchmark]
            public void Read()
            {
                MidiFileReadBenchmarks.Read(FileFormat, FileSize, new ReadingSettings());
            }

            [Benchmark]
            public void Read_DontUseBuffer()
            {
                var settings = new ReadingSettings();
                settings.ReaderSettings.BufferingPolicy = BufferingPolicy.DontUseBuffering;
                MidiFileReadBenchmarks.Read(FileFormat, FileSize, settings);
            }

            [Benchmark]
            public void Read_BufferAllData()
            {
                var settings = new ReadingSettings();
                settings.ReaderSettings.BufferingPolicy = BufferingPolicy.BufferAllData;
                MidiFileReadBenchmarks.Read(FileFormat, FileSize, settings);
            }

            [Benchmark]
            public void Read_UseFixedSizeBuffer()
            {
                var settings = new ReadingSettings();
                settings.ReaderSettings.BufferingPolicy = BufferingPolicy.UseFixedSizeBuffer;
                MidiFileReadBenchmarks.Read(FileFormat, FileSize, settings);
            }
        }

        [InProcessSimpleJob(RunStrategy.Throughput)]
        public class Benchmarks_ReadFile_SingleTrack_Small : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.SingleTrack;

            public override MidiFileSize FileSize => MidiFileSize.Small;
        }

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_ReadFile_SingleTrack_Middle : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.SingleTrack;

            public override MidiFileSize FileSize => MidiFileSize.Middle;
        }

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_ReadFile_SingleTrack_Large : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.SingleTrack;

            public override MidiFileSize FileSize => MidiFileSize.Large;
        }

        [InProcessSimpleJob(RunStrategy.Throughput)]
        public class Benchmarks_ReadFile_MultiTrack_Small : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.MultiTrack;

            public override MidiFileSize FileSize => MidiFileSize.Small;
        }

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_ReadFile_MultiTrack_Middle : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.MultiTrack;

            public override MidiFileSize FileSize => MidiFileSize.Middle;
        }

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_ReadFile_MultiTrack_Large : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.MultiTrack;

            public override MidiFileSize FileSize => MidiFileSize.Large;
        }

        [InProcessSimpleJob(RunStrategy.Throughput)]
        public class Benchmarks_ReadFile_MultiSequence_Small : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.MultiSequence;

            public override MidiFileSize FileSize => MidiFileSize.Small;
        }

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_ReadFile_MultiSequence_Middle : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.MultiSequence;

            public override MidiFileSize FileSize => MidiFileSize.Middle;
        }

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_ReadFile_MultiSequence_Large : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.MultiSequence;

            public override MidiFileSize FileSize => MidiFileSize.Large;
        }

        #endregion

        #region Test methods

        [TestCase(typeof(Benchmarks_ReadFile_SingleTrack_Small))]
        [TestCase(typeof(Benchmarks_ReadFile_SingleTrack_Middle))]
        [TestCase(typeof(Benchmarks_ReadFile_SingleTrack_Large))]
        [TestCase(typeof(Benchmarks_ReadFile_MultiTrack_Small))]
        [TestCase(typeof(Benchmarks_ReadFile_MultiTrack_Middle))]
        [TestCase(typeof(Benchmarks_ReadFile_MultiTrack_Large))]
        [TestCase(typeof(Benchmarks_ReadFile_MultiSequence_Small))]
        [TestCase(typeof(Benchmarks_ReadFile_MultiSequence_Middle))]
        [TestCase(typeof(Benchmarks_ReadFile_MultiSequence_Large))]
        public void ReadFile(Type type)
        {
            var instance = Activator.CreateInstance(type);
            var fileFormat = (MidiFileFormat)type.GetProperty(nameof(Benchmarks.FileFormat)).GetValue(instance);
            var fileSize = (MidiFileSize)type.GetProperty(nameof(Benchmarks.FileSize)).GetValue(instance);

            var eventsCount = GetEventsCount(fileFormat, fileSize, new ReadingSettings());
            RunBenchmarks(type, new MidiFileEventsCountsColumn(eventsCount));
        }

        #endregion

        #region Private methods

        private int[] GetEventsCount(MidiFileFormat midiFileFormat, MidiFileSize midiFileSize, ReadingSettings settings)
        {
            var result = new List<int>();

            foreach (var midiFile in GetFiles(midiFileFormat, midiFileSize, settings))
            {
                var events = midiFile.GetTrackChunks().SelectMany(c => c.Events).ToList();
                result.Add(events.Count);
            }

            return result.ToArray();
        }

        protected static void Read(MidiFileFormat midiFileFormat, MidiFileSize midiFileSize, ReadingSettings settings)
        {
            GetFiles(midiFileFormat, midiFileSize, settings).ToList();
        }

        private static IEnumerable<MidiFile> GetFiles(MidiFileFormat midiFileFormat, MidiFileSize midiFileSize, ReadingSettings settings)
        {
            foreach (var filePath in Directory.GetFiles(Path.Combine(TestFilesProvider.ValidFilesPath, midiFileFormat.ToString(), midiFileSize.ToString())))
            {
                yield return MidiFile.Read(filePath, settings);
            }
        }

        #endregion
    }
}
