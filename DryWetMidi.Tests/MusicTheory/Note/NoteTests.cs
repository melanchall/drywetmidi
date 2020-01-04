using System;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.MusicTheory
{
    [TestFixture]
    public class NoteTests
    {
        #region Test methods

        [Test]
        [Description("Check that notes of the same note number are equal by reference.")]
        public void CheckReferences()
        {
            Assert.AreSame(Note.Get((SevenBitNumber)34), Note.Get((SevenBitNumber)34));
        }

        [Test]
        [Description("Transpose a note up.")]
        public void Transpose_Up()
        {
            var expectedNote = Note.Get((SevenBitNumber)25);
            var actualNote = Note.Get((SevenBitNumber)15)
                                 .Transpose(Interval.FromHalfSteps(10));

            Assert.AreEqual(expectedNote, actualNote);
        }

        [Test]
        [Description("Transpose a note up by maximum value.")]
        public void Transpose_Up_Max()
        {
            var expectedNote = Note.Get(SevenBitNumber.MaxValue);
            var actualNote = Note.Get(SevenBitNumber.MinValue)
                                 .Transpose(Interval.GetUp(SevenBitNumber.MaxValue));

            Assert.AreEqual(expectedNote, actualNote);
        }

        [Test]
        [Description("Transpose a note up going out of the valid range.")]
        public void Transpose_Up_OutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
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

            Assert.AreEqual(expectedNote, actualNote);
        }

        [Test]
        [Description("Transpose a note down by maximum value.")]
        public void Transpose_Down_Max()
        {
            var expectedNote = Note.Get(SevenBitNumber.MinValue);
            var actualNote = Note.Get(SevenBitNumber.MaxValue)
                                 .Transpose(Interval.GetDown(SevenBitNumber.MaxValue));

            Assert.AreEqual(expectedNote, actualNote);
        }

        [Test]
        [Description("Transpose a note down going out of the valid range.")]
        public void Transpose_Down_OutOfRange()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                Note.Get(SevenBitNumber.MinValue)
                    .Transpose(Interval.GetDown(SevenBitNumber.MaxValue));
            });
        }

        [Test]
        [Description("Parse valid note.")]
        public void Parse_Valid_ZeroOctave()
        {
            Parse("C#0", Octave.Get(0).CSharp);
        }

        [Test]
        [Description("Parse valid note of negative octave.")]
        public void Parse_Valid_NegativeOctave()
        {
            Parse("B-1", Octave.Get(-1).B);
        }

        [Test]
        [Description("Parse valid note using 'sharp' word.")]
        public void Parse_Valid_SharpWord()
        {
            Parse("F sharp 3", Octave.Get(3).FSharp);
        }

        [Test]
        [Description("Parse invalid note where octave number is out of range.")]
        public void Parse_Invalid_OctaveIsOutOfRange()
        {
            ParseInvalid<FormatException>("E10");
        }

        [Test]
        [Description("Parse invalid note where an input string is empty.")]
        public void Parse_Invalid_EmptyInputString()
        {
            ParseInvalid<ArgumentException>(string.Empty);
        }

        [Test]
        public void Parse_LetterOnly()
        {
            Parse("D3", Octave.Get(3).D);
        }

        [Test]
        public void Parse_Sharps_1()
        {
            Parse("F##3", Octave.Get(3).G);
        }

        [Test]
        public void Parse_Sharps_2()
        {
            Parse("F#sharp #### 3", Octave.Get(3).B);
        }

        [Test]
        public void Parse_Sharps_3()
        {
            Parse("F# # # ### # # # ### 1", Octave.Get(1).F);
        }

        [Test]
        public void Parse_Flats_1()
        {
            Parse("Fb 1", Octave.Get(1).E);
        }

        [Test]
        public void Parse_Flats_2()
        {
            Parse("Fb flat flat 1", Octave.Get(1).D);
        }

        [Test]
        public void Parse_Flats_3()
        {
            Parse("Fbbbb bbbb bbbb flat 1", Octave.Get(1).E);
        }

        [Test]
        public void Parse_Sharps_Flats_1()
        {
            Parse("C#b4", Octave.Get(4).C);
        }

        [Test]
        public void Parse_Sharps_Flats_2()
        {
            Parse("C#b##4", Octave.Get(4).D);
        }

        [Test]
        public void Parse_Sharps_Flats_3()
        {
            Parse("C#bbb4", Octave.Get(4).ASharp);
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

        #endregion

        #region Private methods

        private static void Parse(string input, Note expectedNote)
        {
            Note.TryParse(input, out var actualNote);
            Assert.AreEqual(expectedNote,
                            actualNote,
                            "TryParse: incorrect result.");

            actualNote = Note.Parse(input);
            Assert.AreEqual(expectedNote,
                            actualNote,
                            "Parse: incorrect result.");

            Assert.AreEqual(expectedNote,
                            Note.Parse(expectedNote.ToString()),
                            "Parse: string representation was not parsed to the original note.");
        }

        private static void ParseInvalid<TException>(string input)
            where TException : Exception
        {
            Assert.Throws<TException>(() => Note.Parse(input));
        }

        #endregion
    }
}
