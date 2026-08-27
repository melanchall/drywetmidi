using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;
using NUnit.Framework.Internal;
using NUnit.Framework.Legacy;
using System;
using System.Linq;
using System.Reflection.Emit;
using System.Threading;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestFixture]
    public class NoteTests
    {
        private static readonly object[] ParseData_Valid = new[]
        {
            new object[] { "C#0", Octave.Get(0).CSharp },
            new object[] { "C#0  ", Octave.Get(0).CSharp },
            new object[] { "  A+8  ", Octave.Get(8).A },
            new object[] { "B-1", Octave.Get(-1).B },
            new object[] { "F sharp 3", Octave.Get(3).FSharp },
            new object[] { "D3", Octave.Get(3).D },
            new object[] { "F##3", Octave.Get(3).G },
            new object[] { "F#sharp #### 3", Octave.Get(3).B },
            new object[] { "F# # # ### # # # ### 1", Octave.Get(1).F },
            new object[] { "Fb 1", Octave.Get(1).E },
            new object[] { "Fb flat flat 1", Octave.Get(1).D },
            new object[] { "Fbbbb bbbb bbbb flat 1", Octave.Get(1).E },
            new object[] { "  C#b 4", Octave.Get(4).C },
            new object[] { "C#b##4", Octave.Get(4).D },
            new object[] { "C#bbb  4", Octave.Get(4).ASharp },
        }
        .Concat(SevenBitNumber
            .Values
            .Select(Note.Get)
            .Select(n => new object[] { n.ToString(), n }))
        .ToArray();

        private static readonly object[] ParseData_Invalid = new[]
        {
            new object[] { "C#10" },
            new object[] { "C#-2" },
            new object[] { "B-2" },
            new object[] { "aC3" },
            new object[] { "CC5" },
            new object[] { "abcd" },
            new object[] { "Aa" },
            new object[] { "A5C" },
        };

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

        [TestCaseSource(nameof(ParseData_Valid))]
        public void Parse_Valid(string s, Note expectedNote)
        {
            var actualNote = Note.Parse(s);
            ClassicAssert.AreEqual(expectedNote, actualNote, $"Parsed note is invalid.");

            ClassicAssert.AreEqual(
                expectedNote,
                Note.Parse(expectedNote.ToString()),
                "String representation was not parsed to the original note.");
        }

        [TestCaseSource(nameof(ParseData_Valid))]
        public void TryParse_Valid(string s, Note expectedNote)
        {
            ClassicAssert.IsTrue(Note.TryParse(s, out var actualNote), "Failed to parse.");
            ClassicAssert.AreEqual(expectedNote, actualNote, "Parsed note is invalid.");

            ClassicAssert.IsTrue(Note.TryParse(expectedNote.ToString(), out actualNote), "Failed to parse string representation.");
            ClassicAssert.AreEqual(
                expectedNote,
                actualNote,
                "String representation was not parsed to the original note.");
        }

        [TestCaseSource(nameof(ParseData_Invalid))]
        public void Parse_Invalid(string s) =>
            ClassicAssert.Throws<FormatException>(() => Note.Parse(s));

        [TestCaseSource(nameof(ParseData_Invalid))]
        public void TryParse_Invalid(string s) =>
            ClassicAssert.IsFalse(Note.TryParse(s, out var actualNote), "Parsed invalid note.");

        [Test]
        public void Parse_Invalid_EmptyOrNull([Values(null, "", "  ")] string s) =>
            ClassicAssert.Throws<ArgumentException>(() => Note.Parse(s), "Note parsing did not throw an exception.");

        [Test]
        public void TryParse_Invalid_EmptyOrNull([Values(null, "", "  ")] string s) =>
            ClassicAssert.IsFalse(Note.TryParse(s, out var actualNote), $"Parsed invalid value '{s}'.");

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
            ClassicAssert.IsTrue(Note.TryParse(input, out var actualNote), $"TryParse ({label}): failed to parse.");
            ClassicAssert.AreEqual(
                expectedNote,
                actualNote,
                $"TryParse ({label}): incorrect result.");

            actualNote = Note.Parse(input);
            ClassicAssert.AreEqual(
                expectedNote,
                actualNote,
                $"Parse ({label}): incorrect result.");

            ClassicAssert.AreEqual(
                expectedNote,
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
