using System;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Running;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Benchmarks.Smf.Interaction
{
    [TestClass]
    public class NotesManagingUtilitiesGetNotesBenchmarks
    {
        #region Nested classes

        [ClrJob]
        public class Benchmarks
        {
            private static readonly MidiFile _midiFile = CreateTestFile();

            [Benchmark]
            public void GetNotes_MidiFile()
            {
                var notes = _midiFile.GetNotes();

                for (int i = 0; i < 10; i++)
                {
                    foreach (var note in notes)
                    {
                    }
                }
            }

            private static MidiFile CreateTestFile()
            {
                const int trackChunksNumber = 100;
                const int notesNumberPerTrackChunk = 1000;
                const int noteLength = 100;

                var midiFile = new MidiFile();

                for (int i = 0; i < trackChunksNumber; i++)
                {
                    var trackChunk = new TrackChunk();

                    using (var notesManager = trackChunk.ManageNotes())
                    {
                        notesManager.Notes.Add(Enumerable.Range(0, notesNumberPerTrackChunk)
                                                         .Select(j => new Note((SevenBitNumber)(j % SevenBitNumber.MaxValue), noteLength, j)));
                    }

                    midiFile.Chunks.Add(trackChunk);
                }

                return midiFile;
            }
        }

        #endregion

        #region Test methods

        [TestMethod]
        [Description("Benchmark NotesManagingUtilities.GetNotes method.")]
        public void GetNotes()
        {
            BenchmarkRunner.Run<Benchmarks>();
        }

        #endregion
    }
}
