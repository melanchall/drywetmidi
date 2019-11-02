using System;
using System.Collections;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class GetNotesAndRestsUtilitiesTests
    {
        #region Nested classes

        private sealed class LengthedObjectComparer : IComparer
        {
            #region IComparer

            public int Compare(object x, object y)
            {
                var lengthedObject1 = x as ILengthedObject;
                var lengthedObject2 = y as ILengthedObject;

                if (ReferenceEquals(lengthedObject1, lengthedObject2))
                    return 1;

                if (ReferenceEquals(lengthedObject1, null))
                    return -1;

                if (ReferenceEquals(lengthedObject2, null))
                    return 1;

                var timesDifference = lengthedObject1.Time - lengthedObject2.Time;
                if (timesDifference != 0)
                    return Math.Sign(timesDifference);

                var lengthsDifference = lengthedObject1.Length - lengthedObject2.Length;
                if (lengthsDifference != 0)
                    return Math.Sign(lengthsDifference);

                return TimedObjectEquality.AreEqual(lengthedObject1, lengthedObject2, false) ? 0 : -1;
            }

            #endregion
        }

        #endregion

        #region Test methods

        [OneTimeSetUp]
        public void SetUp()
        {
            TestContext.AddFormatter<ILengthedObject>(obj =>
            {
                var lengthedObject = (ILengthedObject)obj;
                return $"{obj} (T = {lengthedObject.Time}, L = {lengthedObject.Length})";
            });
        }

        [TestCase(10, 10, 50, 50)]
        [TestCase(10, 2, 50, 50)]
        [TestCase(10, 10, 50, 100)]
        [TestCase(10, 2, 50, 100)]
        [Description("Get notes and rests using no separation for rests.")]
        public void GetNotesAndRests_NoSeparation(
            byte channel1,
            byte channel2,
            byte noteNumber1,
            byte noteNumber2)
        {
            var notes = new[]
            {
                new Note((SevenBitNumber)noteNumber1, 100, 10) { Channel = (FourBitNumber)channel2 },
                new Note((SevenBitNumber)noteNumber1, 100, 30) { Channel = (FourBitNumber)channel1 },
                new Note((SevenBitNumber)noteNumber2, 50, 300) { Channel = (FourBitNumber)channel2 },
                new Note((SevenBitNumber)noteNumber1, 500, 1000) { Channel = (FourBitNumber)channel1 },
                new Note((SevenBitNumber)noteNumber2, 150, 1200) { Channel = (FourBitNumber)channel2 },
                new Note((SevenBitNumber)noteNumber1, 1000, 1300) { Channel = (FourBitNumber)channel1 },
                new Note((SevenBitNumber)noteNumber2, 1000, 10000) { Channel = (FourBitNumber)channel2 },
                new Note((SevenBitNumber)noteNumber1, 1000, 100000) { Channel = (FourBitNumber)channel1 },
                new Note((SevenBitNumber)noteNumber2, 10, 100100) { Channel = (FourBitNumber)channel2 },
                new Note((SevenBitNumber)noteNumber1, 10, 110000) { Channel = (FourBitNumber)channel1 },
            };

            var expectedObjects = new ILengthedObject[]
            {
                new Rest(0, 10, null, null),
                new Note((SevenBitNumber)noteNumber1, 100, 10) { Channel = (FourBitNumber)channel2 },
                new Note((SevenBitNumber)noteNumber1, 100, 30) { Channel = (FourBitNumber)channel1 },
                new Rest(130, 170, null, null),
                new Note((SevenBitNumber)noteNumber2, 50, 300) { Channel = (FourBitNumber)channel2 },
                new Rest(350, 650, null, null),
                new Note((SevenBitNumber)noteNumber1, 500, 1000) { Channel = (FourBitNumber)channel1 },
                new Note((SevenBitNumber)noteNumber2, 150, 1200) { Channel = (FourBitNumber)channel2 },
                new Note((SevenBitNumber)noteNumber1, 1000, 1300) { Channel = (FourBitNumber)channel1 },
                new Rest(2300, 7700, null, null),
                new Note((SevenBitNumber)noteNumber2, 1000, 10000) { Channel = (FourBitNumber)channel2 },
                new Rest(11000, 89000, null, null),
                new Note((SevenBitNumber)noteNumber1, 1000, 100000) { Channel = (FourBitNumber)channel1 },
                new Note((SevenBitNumber)noteNumber2, 10, 100100) { Channel = (FourBitNumber)channel2 },
                new Rest(101000, 9000, null, null),
                new Note((SevenBitNumber)noteNumber1, 10, 110000) { Channel = (FourBitNumber)channel1 },
            };

            TestGetNotesAndRests(notes, RestSeparationPolicy.NoSeparation, expectedObjects);
        }

        [TestCase(10, 10)]
        [TestCase(10, 50)]
        [Description("Get notes and rests using separation by channel for rests. Single channel case.")]
        public void GetNotesAndRests_SeparateByChannel_SingleChannel(
            byte noteNumber1,
            byte noteNumber2)
        {
            var channel = (FourBitNumber)10;

            var notes = new[]
            {
                new Note((SevenBitNumber)noteNumber1, 100, 10) { Channel = channel },
                new Note((SevenBitNumber)noteNumber1, 100, 30) { Channel = channel },
                new Note((SevenBitNumber)noteNumber2, 50, 300) { Channel = channel },
                new Note((SevenBitNumber)noteNumber1, 500, 1000) { Channel = channel },
                new Note((SevenBitNumber)noteNumber2, 150, 1200) { Channel = channel },
                new Note((SevenBitNumber)noteNumber1, 1000, 1300) { Channel = channel },
                new Note((SevenBitNumber)noteNumber2, 1000, 10000) { Channel = channel },
            };

            var expectedObjects = new ILengthedObject[]
            {
                new Rest(0, 10, channel, null),
                new Note((SevenBitNumber)noteNumber1, 100, 10) { Channel = channel },
                new Note((SevenBitNumber)noteNumber1, 100, 30) { Channel = channel },
                new Rest(130, 170, channel, null),
                new Note((SevenBitNumber)noteNumber2, 50, 300) { Channel = channel },
                new Rest(350, 650, channel, null),
                new Note((SevenBitNumber)noteNumber1, 500, 1000) { Channel = channel },
                new Note((SevenBitNumber)noteNumber2, 150, 1200) { Channel = channel },
                new Note((SevenBitNumber)noteNumber1, 1000, 1300) { Channel = channel },
                new Rest(2300, 7700, channel, null),
                new Note((SevenBitNumber)noteNumber2, 1000, 10000) { Channel = channel },
            };

            TestGetNotesAndRests(notes, RestSeparationPolicy.SeparateByChannel, expectedObjects);
        }

        [TestCase(10, 10)]
        [TestCase(10, 50)]
        [Description("Get notes and rests using separation by channel for rests. Different channels.")]
        public void GetNotesAndRests_SeparateByChannel_DifferentChannels(
            byte noteNumber1,
            byte noteNumber2)
        {
            var channel1 = (FourBitNumber)10;
            var channel2 = (FourBitNumber)2;

            var notes = new[]
            {
                new Note((SevenBitNumber)noteNumber1, 100, 10) { Channel = channel1 },
                new Note((SevenBitNumber)noteNumber1, 100, 30) { Channel = channel2 },
                new Note((SevenBitNumber)noteNumber2, 50, 300) { Channel = channel1 },
                new Note((SevenBitNumber)noteNumber1, 500, 1000) { Channel = channel2 },
                new Note((SevenBitNumber)noteNumber2, 150, 1200) { Channel = channel1 },
                new Note((SevenBitNumber)noteNumber1, 1000, 1300) { Channel = channel2 },
            };

            var expectedObjects = new ILengthedObject[]
            {
                new Rest(0, 10, channel1, null),
                new Note((SevenBitNumber)noteNumber1, 100, 10) { Channel = channel1 },
                new Rest(0, 30, channel2, null),
                new Note((SevenBitNumber)noteNumber1, 100, 30) { Channel = channel2 },
                new Rest(110, 190, channel1, null),
                new Note((SevenBitNumber)noteNumber2, 50, 300) { Channel = channel1 },
                new Rest(130, 870, channel2, null),
                new Note((SevenBitNumber)noteNumber1, 500, 1000) { Channel = channel2 },
                new Rest(350, 850, channel1, null),
                new Note((SevenBitNumber)noteNumber2, 150, 1200) { Channel = channel1 },
                new Note((SevenBitNumber)noteNumber1, 1000, 1300) { Channel = channel2 },
            };

            TestGetNotesAndRests(notes, RestSeparationPolicy.SeparateByChannel, expectedObjects);
        }

        [TestCase(10, 10)]
        [TestCase(10, 5)]
        [Description("Get notes and rests using separation by note number for rests. Single note number case.")]
        public void GetNotesAndRests_SeparateByNoteNumber_SingleNoteNumber(
            byte channel1,
            byte channel2)
        {
            var noteNumber = (SevenBitNumber)10;

            var notes = new[]
            {
                new Note(noteNumber, 100, 10) { Channel = (FourBitNumber)channel2 },
                new Note(noteNumber, 100, 30) { Channel = (FourBitNumber)channel1 },
                new Note(noteNumber, 50, 300) { Channel = (FourBitNumber)channel2 },
                new Note(noteNumber, 500, 1000) { Channel = (FourBitNumber)channel1 },
                new Note(noteNumber, 150, 1200) { Channel = (FourBitNumber)channel2 },
                new Note(noteNumber, 1000, 1300) { Channel = (FourBitNumber)channel1 },
            };

            var expectedObjects = new ILengthedObject[]
            {
                new Rest(0, 10, null, noteNumber),
                new Note(noteNumber, 100, 10) { Channel = (FourBitNumber)channel2 },
                new Note(noteNumber, 100, 30) { Channel = (FourBitNumber)channel1 },
                new Rest(130, 170, null, noteNumber),
                new Note(noteNumber, 50, 300) { Channel = (FourBitNumber)channel2 },
                new Rest(350, 650, null, noteNumber),
                new Note(noteNumber, 500, 1000) { Channel = (FourBitNumber)channel1 },
                new Note(noteNumber, 150, 1200) { Channel = (FourBitNumber)channel2 },
                new Note(noteNumber, 1000, 1300) { Channel = (FourBitNumber)channel1 },
            };

            TestGetNotesAndRests(notes, RestSeparationPolicy.SeparateByNoteNumber, expectedObjects);
        }

        [TestCase(10, 10)]
        [TestCase(10, 5)]
        [Description("Get notes and rests using separation by note number for rests. Different note numbers case.")]
        public void GetNotesAndRests_SeparateByNoteNumber_DifferentNoteNumbers(
            byte channel1,
            byte channel2)
        {
            var noteNumber1 = (SevenBitNumber)10;
            var noteNumber2 = (SevenBitNumber)100;

            var notes = new[]
            {
                new Note(noteNumber1, 100, 0) { Channel = (FourBitNumber)channel2 },
                new Note(noteNumber2, 100, 30) { Channel = (FourBitNumber)channel1 },
                new Note(noteNumber1, 50, 300) { Channel = (FourBitNumber)channel2 },
                new Note(noteNumber2, 500, 1000) { Channel = (FourBitNumber)channel1 },
            };

            var expectedObjects = new ILengthedObject[]
            {
                new Note(noteNumber1, 100, 0) { Channel = (FourBitNumber)channel2 },
                new Rest(0, 30, null, noteNumber2),
                new Note(noteNumber2, 100, 30) { Channel = (FourBitNumber)channel1 },
                new Rest(100, 200, null, noteNumber1),
                new Note(noteNumber1, 50, 300) { Channel = (FourBitNumber)channel2 },
                new Rest(130, 870, null, noteNumber2),
                new Note(noteNumber2, 500, 1000) { Channel = (FourBitNumber)channel1 },
            };

            TestGetNotesAndRests(notes, RestSeparationPolicy.SeparateByNoteNumber, expectedObjects);
        }

        [Test]
        [Description("Get notes and rests using separation by channel and note number for rests.")]
        public void GetNotesAndRests_SeparateByChannelAndNoteNumber()
        {
            var noteNumber1 = (SevenBitNumber)10;
            var noteNumber2 = (SevenBitNumber)100;
            var channel1 = (FourBitNumber)10;
            var channel2 = (FourBitNumber)2;

            var notes = new[]
            {
                new Note(noteNumber1, 100, 10) { Channel = channel1 },
                new Note(noteNumber2, 100, 30) { Channel = channel1 },
                new Note(noteNumber1, 50, 300) { Channel = channel2 },
                new Note(noteNumber2, 500, 1000) { Channel = channel2 },
                new Note(noteNumber1, 150, 1200) { Channel = channel1 },
                new Note(noteNumber2, 1000, 1300) { Channel = channel1 },
            };

            var expectedObjects = new ILengthedObject[]
            {
                new Rest(0, 10, channel1, noteNumber1),
                new Note(noteNumber1, 100, 10) { Channel = channel1 },
                new Rest(0, 30, channel1, noteNumber2),
                new Note(noteNumber2, 100, 30) { Channel = channel1 },
                new Rest(0, 300, channel2, noteNumber1),
                new Note(noteNumber1, 50, 300) { Channel = channel2 },
                new Rest(0, 1000, channel2, noteNumber2),
                new Note(noteNumber2, 500, 1000) { Channel = channel2 },
                new Rest(110, 1090, channel1, noteNumber1),
                new Note(noteNumber1, 150, 1200) { Channel = channel1 },
                new Rest(130, 1170, channel1, noteNumber2),
                new Note(noteNumber2, 1000, 1300) { Channel = channel1 },
            };

            TestGetNotesAndRests(notes, RestSeparationPolicy.SeparateByChannelAndNoteNumber, expectedObjects);
        }

        #endregion

        #region Private methods

        private static void TestGetNotesAndRests(
            IEnumerable<Note> inputNotes,
            RestSeparationPolicy restSeparationPolicy,
            IEnumerable<ILengthedObject> expectedObjects)
        {
            var actualObjects = inputNotes.GetNotesAndRests(restSeparationPolicy);

            CollectionAssert.AreEqual(
                expectedObjects,
                actualObjects,
                new LengthedObjectComparer());
        }

        #endregion
    }
}
