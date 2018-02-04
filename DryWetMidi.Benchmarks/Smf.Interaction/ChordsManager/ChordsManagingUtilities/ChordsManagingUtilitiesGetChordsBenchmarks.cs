using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Running;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Benchmarks.Smf.Interaction
{
    [TestClass]
    public class ChordsManagingUtilitiesGetChordsBenchmarks
    {
        #region Nested classes

        [ClrJob]
        public class Benchmarks
        {
            private static readonly IEnumerable<Chord> _midiFileChords = CreateTestFile().GetChords();

            [Benchmark]
            public void GetChords_MidiFile()
            {
                const int iterationsNumber = 10;

                for (int i = 0; i < iterationsNumber; i++)
                {
                    foreach (var chord in _midiFileChords)
                    {
                    }
                }
            }

            private static MidiFile CreateTestFile()
            {
                const int trackChunksNumber = 100;
                const int chordsPerTrackChunk = 1000;
                const int noteLength = 100;
                const int notesPerChord = 10;

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

        [TestMethod]
        [Description("Benchmark ChordsManagingUtilities.GetChords method.")]
        public void GetChords()
        {
            BenchmarkRunner.Run<Benchmarks>();
        }

        #endregion
    }
}
