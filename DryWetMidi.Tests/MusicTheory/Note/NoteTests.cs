using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using NUnit.Framework.Internal;
using NUnit.Framework.Legacy;
using System;
using System.Linq;
using System.Threading;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestFixture]
    public class NoteTests
    {
        private static readonly object[] NotesStringsToNotes_Prepared = new[]
        {
            new object[] { "C#0", Octave.Get(0).CSharp },
            new object[] { "B-1", Octave.Get(-1).B },
            new object[] { "F sharp 3", Octave.Get(3).FSharp },
            new object[] { "D3", Octave.Get(3).D },
            new object[] { "F##3", Octave.Get(3).G },
            new object[] { "F#sharp #### 3", Octave.Get(3).B },
            new object[] { "F# # # ### # # # ### 1", Octave.Get(1).F },
            new object[] { "Fb 1", Octave.Get(1).E },
            new object[] { "Fb flat flat 1", Octave.Get(1).D },
            new object[] { "Fbbbb bbbb bbbb flat 1", Octave.Get(1).E },
            new object[] { "C#b 4", Octave.Get(4).C },
            new object[] { "C#b##4", Octave.Get(4).D },
            new object[] { "C#bbb  4", Octave.Get(4).ASharp },
        };

        private static readonly object[] NotesStringsToNotes_All = SevenBitNumber
            .Values
            .Select(Note.Get)
            .Select(n => new object[] { n.ToString(), n })
            .ToArray();

        #region Test methods

        [Test]
        [Description("Check that notes of the same note number are equal by reference.")]
        public void CheckReferences()
        {
            ClassicAssert.AreSame(Note.Get((SevenBitNumber)34), Note.Get((SevenBitNumber)34));
        }

        [Test]
        [Description("Transpose a note up.")]
        public void Transpose_Up()
        {
            var expectedNote = Note.Get((SevenBitNumber)25);
            var actualNote = Note.Get((SevenBitNumber)15)
                                 .Transpose(Interval.FromHalfSteps(10));

            ClassicAssert.AreEqual(expectedNote, actualNote);
        }

        [Test]
        [Description("Transpose a note up by maximum value.")]
        public void Transpose_Up_Max()
        {
            var expectedNote = Note.Get(SevenBitNumber.MaxValue);
            var actualNote = Note.Get(SevenBitNumber.MinValue)
                                 .Transpose(Interval.GetUp(SevenBitNumber.MaxValue));

            ClassicAssert.AreEqual(expectedNote, actualNote);
        }

        [Test]
        [Description("Transpose a note up going out of the valid range.")]
        public void Transpose_Up_OutOfRange()
        {
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() =>
            {
                Note.Get(SevenBitNumber.MaxValue)
                    .Transpose(Interval.GetUp(SevenBitNumber.MaxValue));
            });
        }

        [Test]
        [Description("Transpose a note down.")]
        public void Transpose_Down()
        {
            var expectedNote = Note.Get((SevenBitNumber)25);
            var actualNote = Note.Get((SevenBitNumber)35)
                                 .Transpose(Interval.FromHalfSteps(-10));

            ClassicAssert.AreEqual(expectedNote, actualNote);
        }

        [Test]
        [Description("Transpose a note down by maximum value.")]
        public void Transpose_Down_Max()
        {
            var expectedNote = Note.Get(SevenBitNumber.MinValue);
            var actualNote = Note.Get(SevenBitNumber.MaxValue)
                                 .Transpose(Interval.GetDown(SevenBitNumber.MaxValue));

            ClassicAssert.AreEqual(expectedNote, actualNote);
        }

        [Test]
        [Description("Transpose a note down going out of the valid range.")]
        public void Transpose_Down_OutOfRange()
        {
            ClassicAssert.Throws<ArgumentOutOfRangeException>(() =>
            {
                Note.Get(SevenBitNumber.MinValue)
                    .Transpose(Interval.GetDown(SevenBitNumber.MaxValue));
            });
        }

        [TestCaseSource(nameof(NotesStringsToNotes_Prepared))]
        public void Parse_Valid_Prepared(string s, Note expectedNote) =>
            Parse(s, expectedNote);

        [TestCaseSource(nameof(NotesStringsToNotes_All))]
        public void Parse_Valid_All(string s, Note expectedNote) =>
            Parse(s, expectedNote);

        [Test]
        public void Parse_Invalid_OctaveIsOutOfRange()
        {
            ParseInvalid<FormatException>("E10");
        }

        [Test]
        public void Parse_Invalid_EmptyInputString()
        {
            ParseInvalid<ArgumentException>(string.Empty);
        }

        [Test]
        public void SortNotes()
        {
            var notes = new[]
            {
                Notes.A2,
                Notes.B0,
                Notes.ASharp2,
                Notes.CSharp3,
                Notes.G2,
                Notes.G1
            };

            var sortedNotes = notes.OrderBy(i => i).ToArray();

            CollectionAssert.AreEqual(
                new[]
                {
                    Notes.B0,
                    Notes.G1,
                    Notes.G2,
                    Notes.A2,
                    Notes.ASharp2,
                    Notes.CSharp3
                },
                sortedNotes,
                "Notes are sorted incorrectly.");
        }

        [Test]
        public void GetNotesFromDifferentThreads()
        {
            var noteNumbers = SevenBitNumber.Values.ToArray();
            var reversedNoteNumbers = SevenBitNumber.Values.Reverse().ToArray();

            var thread1 = new Thread(() =>
            {
                foreach (var noteNumber in noteNumbers)
                {
                    var note = Note.Get(noteNumber);
                }
            });

            var thread2 = new Thread(() =>
            {
                foreach (var noteNumber in reversedNoteNumbers)
                {
                    var note = Note.Get(noteNumber);
                }
            });

            thread1.Start();
            thread2.Start();

            thread1.Join();
            thread2.Join();
        }

        #endregion

        #region Private methods

        private static void Parse(string input, Note expectedNote)
        {
            Parse(input, expectedNote, "original");
            Parse(input.ToLower(), expectedNote, "lower");
            Parse(input.ToUpper(), expectedNote, "upper");
        }

        private static void Parse(string input, Note expectedNote, string label)
        {
            Note.TryParse(input, out var actualNote);
            ClassicAssert.AreEqual(expectedNote,
                            actualNote,
                            $"TryParse ({label}): incorrect result.");

            actualNote = Note.Parse(input);
            ClassicAssert.AreEqual(expectedNote,
                            actualNote,
                            $"Parse ({label}): incorrect result.");

            ClassicAssert.AreEqual(expectedNote,
                            Note.Parse(expectedNote.ToString()),
                            $"Parse ({label}): string representation was not parsed to the original note.");
        }

        private static void ParseInvalid<TException>(string input)
            where TException : Exception
        {
            ClassicAssert.Throws<TException>(() => Note.Parse(input));
        }

        #endregion
    }
}
