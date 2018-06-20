using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    // TODO: test velocities merging
    [TestFixture]
    public sealed class NotesMergerTests : LengthedObjectsToolTests<Note>
    {
        #region Constructor

        public NotesMergerTests()
            : base(new NoteMethods())
        {
        }

        #endregion

        #region Test methods

        [Test]
        [Description("Merge notes of the same channel and of the same pitch. All notes will be merged into one.")]
        public void Merge_SingleChannel_SingleNoteNumber_AllOverlapped()
        {
            var noteNumber = (SevenBitNumber)100;
            var channel = (FourBitNumber)10;
            var tempoMap = TempoMap.Default;

            Merge(
                CreateNotes(
                    new[] { "0; 0:0:2", "0:0:1; 0:2:0", "0:1:0; 0:0:30" },
                    noteNumber,
                    channel,
                    tempoMap),
                CreateNotes(
                    new[] { "0; 0:2:1" },
                    noteNumber,
                    channel,
                    tempoMap));
        }

        [Test]
        [Description("Merge notes of the same channel and of different pitches. All notes of the same pitch will be merged into one.")]
        public void Merge_SingleChannel_DifferentNoteNumbers_AllOverlapped()
        {
            var noteNumber1 = (SevenBitNumber)30;
            var noteNumber2 = (SevenBitNumber)10;
            var channel = (FourBitNumber)10;
            var tempoMap = TempoMap.Default;

            var inputNotes1 = CreateNotes(
                new[] { "0; 0:0:2", "0:0:1; 0:2:0", "0:1:0; 0:0:30" },
                noteNumber1,
                channel,
                tempoMap);

            var expectedNotes1 = CreateNotes(
                new[] { "0; 0:2:1" },
                noteNumber1,
                channel,
                tempoMap);

            var inputNotes2 = CreateNotes(
                new[] { "0:0:10; 0:0:2", "0:0:1; 0:2:30", "0:1:0; 0:0:30" },
                noteNumber2,
                channel,
                tempoMap);

            var expectedNotes2 = CreateNotes(
                new[] { "0:0:1; 0:2:30" },
                noteNumber2,
                channel,
                tempoMap);

            Merge(inputNotes1.Concat(inputNotes2), expectedNotes1.Concat(expectedNotes2));
        }

        [Test]
        [Description("Merge notes of different channels and pitches. All notes of the same pitch and channel will be merged into one.")]
        public void Merge_DifferentChannels_DifferentNoteNumbers_AllOverlapped()
        {
            var noteNumber1 = (SevenBitNumber)30;
            var channel1 = (FourBitNumber)3;
            var noteNumber2 = (SevenBitNumber)10;
            var channel2 = (FourBitNumber)12;
            var tempoMap = TempoMap.Default;

            var inputNotes1 = CreateNotes(
                new[] { "0; 0:0:2", "0:0:1; 0:2:0", "0:1:0; 0:0:30" },
                noteNumber1,
                channel1,
                tempoMap);

            var expectedNotes1 = CreateNotes(
                new[] { "0; 0:2:1" },
                noteNumber1,
                channel1,
                tempoMap);

            var inputNotes2 = CreateNotes(
                new[] { "0:0:10; 0:0:2", "0:0:1; 0:2:30", "0:1:0; 0:0:30" },
                noteNumber2,
                channel2,
                tempoMap);

            var expectedNotes2 = CreateNotes(
                new[] { "0:0:1; 0:2:30" },
                noteNumber2,
                channel2,
                tempoMap);

            Merge(inputNotes1.Concat(inputNotes2), expectedNotes1.Concat(expectedNotes2));
        }

        [Test]
        [Description("Merge notes of different channels and pitches.")]
        public void Merge_DifferentChannels_DifferentNoteNumbers()
        {
            var noteNumber1 = (SevenBitNumber)30;
            var channel1 = (FourBitNumber)3;
            var noteNumber2 = (SevenBitNumber)10;
            var channel2 = (FourBitNumber)12;
            var noteNumber3 = (SevenBitNumber)11;
            var channel3 = (FourBitNumber)10;
            var tempoMap = TempoMap.Default;

            var inputNotes1 = CreateNotes(
                new[] { "0; 0:0:2", "0:0:1; 0:2:0", "0:1:0; 0:0:30" },
                noteNumber1,
                channel1,
                tempoMap);

            var expectedNotes1 = CreateNotes(
                new[] { "0; 0:2:1" },
                noteNumber1,
                channel1,
                tempoMap);

            var inputNotes2 = CreateNotes(
                new[] { "0:0:10; 0:0:2", "0:0:1; 0:2:30", "0:1:0; 0:0:30" },
                noteNumber2,
                channel2,
                tempoMap);

            var expectedNotes2 = CreateNotes(
                new[] { "0:0:1; 0:2:30" },
                noteNumber2,
                channel2,
                tempoMap);

            var inputNotes3 = CreateNotes(
                new[] { "0:0:10; 0:0:2", "0:1:0; 0:2:30", "1:0:10; 1:0:30" },
                noteNumber3,
                channel3,
                tempoMap);

            var expectedNotes3 = CreateNotes(
                new[] { "0:0:10; 0:0:2", "0:1:0; 0:2:30", "1:0:10; 1:0:30" },
                noteNumber3,
                channel3,
                tempoMap);

            Merge(inputNotes1.Concat(inputNotes2).Concat(inputNotes3),
                  expectedNotes1.Concat(expectedNotes2).Concat(expectedNotes3));
        }

        #endregion

        #region Private methods

        private void Merge(IEnumerable<Note> inputNotes, IEnumerable<Note> expectedNotes)
        {
            var actualNotes = inputNotes.Merge();
            ObjectMethods.AssertCollectionsAreEqual(expectedNotes.OrderBy(n => n.Time),
                                                    actualNotes.OrderBy(n => n.Time));
        }

        private IEnumerable<Note> CreateNotes(string[] timesAndLengths,
                                              SevenBitNumber noteNumber,
                                              FourBitNumber channel,
                                              TempoMap tempoMap)
        {
            var notes = ObjectMethods.CreateCollection(tempoMap, timesAndLengths);

            foreach (var note in notes)
            {
                note.NoteNumber = noteNumber;
                note.Channel = channel;
            }

            return notes;
        }

        #endregion
    }
}
