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
    public class NotesManagingUtilitiesGetNotesBenchmarks : BenchmarkTest
    {
        #region Nested classes

        [InProcessSimpleJob(RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_GetNotes
        {
            private static readonly MidiFile _midiFile = CreateTestFile();

            [Benchmark]
            public void GetNotes_MidiFile()
            {
                var note = _midiFile.GetNotes();
            }

            private static MidiFile CreateTestFile()
            {
                const int trackChunksNumber = 50;
                const int notesPerTrackChunk = 1000;
                const int noteLength = 100;

                var midiFile = new MidiFile();

                for (int i = 0; i < trackChunksNumber; i++)
                {
                    var trackChunk = new TrackChunk();

                    using (var notesManager = trackChunk.ManageNotes())
                    {
                        notesManager.Notes.Add(Enumerable.Range(0, notesPerTrackChunk)
                                                         .Select(j => new Note((SevenBitNumber)(j % SevenBitNumber.MaxValue), noteLength, j)));
                    }

                    midiFile.Chunks.Add(trackChunk);
                }

                return midiFile;
            }
        }

        #endregion

        #region Test methods

        [Test]
        [Description("Benchmark NotesManagingUtilities.GetNotes method.")]
        public void GetNotes()
        {
            RunBenchmarks<Benchmarks_GetNotes>();
        }

        #endregion
    }
}
