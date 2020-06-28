using System;
using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Benchmarks.Core
{
    [TestFixture]
    public class MidiFileWriteBenchmarks : BenchmarkTest
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
            private Dictionary<MidiFile, string> _files;

            public abstract MidiFileFormat FileFormat { get; }

            public abstract MidiFileSize FileSize { get; }

            [GlobalSetup]
            public void GlobalSetup()
            {
                _files = GetFiles(FileFormat, FileSize);
            }

            [GlobalCleanup]
            public void GlobalCleanup()
            {
                foreach (var newFilePath in _files.Values)
                {
                    File.Delete(newFilePath);
                }
            }

            [Benchmark]
            public void Write()
            {
                foreach (var file in _files)
                {
                    file.Key.Write(file.Value, overwriteFile: true, format: FileFormat);
                }
            }
        }

        [InProcessSimpleJob(RunStrategy.Throughput)]
        public class Benchmarks_WriteFile_SingleTrack_Small : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.SingleTrack;

            public override MidiFileSize FileSize => MidiFileSize.Small;
        }

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_WriteFile_SingleTrack_Middle : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.SingleTrack;

            public override MidiFileSize FileSize => MidiFileSize.Middle;
        }

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_WriteFile_SingleTrack_Large : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.SingleTrack;

            public override MidiFileSize FileSize => MidiFileSize.Large;
        }

        [InProcessSimpleJob(RunStrategy.Throughput)]
        public class Benchmarks_WriteFile_MultiTrack_Small : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.MultiTrack;

            public override MidiFileSize FileSize => MidiFileSize.Small;
        }

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_WriteFile_MultiTrack_Middle : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.MultiTrack;

            public override MidiFileSize FileSize => MidiFileSize.Middle;
        }

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_WriteFile_MultiTrack_Large : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.MultiTrack;

            public override MidiFileSize FileSize => MidiFileSize.Large;
        }

        [InProcessSimpleJob(RunStrategy.Throughput)]
        public class Benchmarks_WriteFile_MultiSequence_Small : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.MultiSequence;

            public override MidiFileSize FileSize => MidiFileSize.Small;
        }

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_WriteFile_MultiSequence_Middle : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.MultiSequence;

            public override MidiFileSize FileSize => MidiFileSize.Middle;
        }

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_WriteFile_MultiSequence_Large : Benchmarks
        {
            public override MidiFileFormat FileFormat => MidiFileFormat.MultiSequence;

            public override MidiFileSize FileSize => MidiFileSize.Large;
        }

        #endregion

        #region Test methods

        [TestCase(typeof(Benchmarks_WriteFile_SingleTrack_Small))]
        [TestCase(typeof(Benchmarks_WriteFile_SingleTrack_Middle))]
        [TestCase(typeof(Benchmarks_WriteFile_SingleTrack_Large))]
        [TestCase(typeof(Benchmarks_WriteFile_MultiTrack_Small))]
        [TestCase(typeof(Benchmarks_WriteFile_MultiTrack_Middle))]
        [TestCase(typeof(Benchmarks_WriteFile_MultiTrack_Large))]
        [TestCase(typeof(Benchmarks_WriteFile_MultiSequence_Small))]
        [TestCase(typeof(Benchmarks_WriteFile_MultiSequence_Middle))]
        [TestCase(typeof(Benchmarks_WriteFile_MultiSequence_Large))]
        public void WriteFile(Type type)
        {
            var instance = Activator.CreateInstance(type);
            var fileFormat = (MidiFileFormat)type.GetProperty(nameof(Benchmarks.FileFormat)).GetValue(instance);

            RunBenchmarks(type);
        }

        #endregion

        #region Private methods

        protected static Dictionary<MidiFile, string> GetFiles(MidiFileFormat midiFileFormat, MidiFileSize midiFileSize)
        {
            var result = new Dictionary<MidiFile, string>();

            foreach (var filePath in Directory.GetFiles(Path.Combine(FilesPath, midiFileFormat.ToString(), midiFileSize.ToString())))
            {
                var midiFile = MidiFile.Read(filePath);
                var newFilePath = Path.GetTempFileName();
                result.Add(midiFile, newFilePath);
            }

            return result;
        }

        #endregion
    }
}
