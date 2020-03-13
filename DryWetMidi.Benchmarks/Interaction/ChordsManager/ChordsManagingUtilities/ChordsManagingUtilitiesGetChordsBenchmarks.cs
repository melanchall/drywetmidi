using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Benchmarks.Interaction
{
    [TestFixture]
    public class ChordsManagingUtilitiesGetChordsBenchmarks : BenchmarkTest
    {
        #region Nested classes

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_GetChords
        {
            private static readonly MidiFile _midiFile = CreateTestFile();

            [Benchmark]
            public void GetChords_MidiFile()
            {
                var chords = _midiFile.GetChords();
            }

            private static MidiFile CreateTestFile()
            {
                const int trackChunksNumber = 10;
                const int chordsPerTrackChunk = 1000;
                const int noteLength = 100;
                const int notesPerChord = 5;

                var midiFile = new MidiFile();

                for (int i = 0; i < trackChunksNumber; i++)
                {
                    var trackChunk = new TrackChunk();

                    using (var chordsManager = trackChunk.ManageChords())
                    {
                        for (int j = 0; j < chordsPerTrackChunk; j++)
                        {
                            var noteNumber = (SevenBitNumber)(j % SevenBitNumber.MaxValue);
                            var notes = Enumerable.Range(0, notesPerChord).Select(_ => new Note(noteNumber, noteLength));

                            chordsManager.Chords.Add(new Chord(notes, j));
                        }
                    }

                    midiFile.Chunks.Add(trackChunk);
                }

                return midiFile;
            }
        }

        #endregion

        #region Test methods

        [Test]
        [Description("Benchmark ChordsManagingUtilities.GetChords method.")]
        public void GetChords()
        {
            RunBenchmarks<Benchmarks_GetChords>();
        }

        #endregion
    }
}
