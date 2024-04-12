using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class RestsUtilitiesTests
    {
        #region Constants

        private const int SameKey = 123;

        #endregion

        #region Test methods

        [TestCase(10, 10, 50, 50)]
        [TestCase(10, 2, 50, 50)]
        [TestCase(10, 10, 50, 100)]
        [TestCase(10, 2, 50, 100)]
        public void GetRests_Notes_SameKey(
            byte channel1,
            byte channel2,
            byte noteNumber1,
            byte noteNumber2)
        {
            GetRests(
                keySelector: obj => SameKey,
                inputObjects: new ITimedObject[]
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
                },
                expectedRests: new []
                {
                    new Rest(0, 10, SameKey),
                    new Rest(130, 170, SameKey),
                    new Rest(350, 650, SameKey),
                    new Rest(2300, 7700, SameKey),
                    new Rest(11000, 89000, SameKey),
                    new Rest(101000, 9000, SameKey),
                });
        }

        [TestCase(10, 10)]
        [TestCase(10, 50)]
        public void GetRests_Notes_RestsByChannel_SameChannel(
            byte noteNumber1,
            byte noteNumber2)
        {
            var channel = (FourBitNumber)10;

            GetRests(
                keySelector: obj => (obj as Note)?.Channel,
                inputObjects: new ITimedObject[]
                {
                    new Note((SevenBitNumber)noteNumber1, 100, 10) { Channel = channel },
                    new Note((SevenBitNumber)noteNumber1, 100, 30) { Channel = channel },
                    new Note((SevenBitNumber)noteNumber2, 50, 300) { Channel = channel },
                    new Note((SevenBitNumber)noteNumber1, 500, 1000) { Channel = channel },
                    new Note((SevenBitNumber)noteNumber2, 150, 1200) { Channel = channel },
                    new Note((SevenBitNumber)noteNumber1, 1000, 1300) { Channel = channel },
                    new Note((SevenBitNumber)noteNumber2, 1000, 10000) { Channel = channel },
                },
                expectedRests: new[]
                {
                    new Rest(0, 10, channel),
                    new Rest(130, 170, channel),
                    new Rest(350, 650, channel),
                    new Rest(2300, 7700, channel),
                });
        }

        [TestCase(10, 10)]
        [TestCase(10, 50)]
        public void GetRests_Notes_RestsByChannel_DifferentChannels(
            byte noteNumber1,
            byte noteNumber2)
        {
            var channel1 = (FourBitNumber)10;
            var channel2 = (FourBitNumber)2;

            GetRests(
                keySelector: obj => (obj as Note)?.Channel,
                inputObjects: new ITimedObject[]
                {
                    new Note((SevenBitNumber)noteNumber1, 100, 10) { Channel = channel1 },
                    new Note((SevenBitNumber)noteNumber1, 100, 30) { Channel = channel2 },
                    new Note((SevenBitNumber)noteNumber2, 50, 300) { Channel = channel1 },
                    new Note((SevenBitNumber)noteNumber1, 500, 1000) { Channel = channel2 },
                    new Note((SevenBitNumber)noteNumber2, 150, 1200) { Channel = channel1 },
                    new Note((SevenBitNumber)noteNumber1, 1000, 1300) { Channel = channel2 },
                },
                expectedRests: new[]
                {
                    new Rest(0, 10, channel1),
                    new Rest(0, 30, channel2),
                    new Rest(110, 190, channel1),
                    new Rest(130, 870, channel2),
                    new Rest(350, 850, channel1),
                });
        }

        [TestCase(10, 10)]
        [TestCase(10, 5)]
        public void GetRests_Notes_RestsByNoteNumber_SameNoteNumber(
            byte channel1,
            byte channel2)
        {
            var noteNumber = (SevenBitNumber)10;

            GetRests(
                keySelector: obj => (obj as Note)?.NoteNumber,
                inputObjects: new ITimedObject[]
                {
                    new Note(noteNumber, 100, 10) { Channel = (FourBitNumber)channel2 },
                    new Note(noteNumber, 100, 30) { Channel = (FourBitNumber)channel1 },
                    new Note(noteNumber, 50, 300) { Channel = (FourBitNumber)channel2 },
                    new Note(noteNumber, 500, 1000) { Channel = (FourBitNumber)channel1 },
                    new Note(noteNumber, 150, 1200) { Channel = (FourBitNumber)channel2 },
                    new Note(noteNumber, 1000, 1300) { Channel = (FourBitNumber)channel1 },
                },
                expectedRests: new[]
                {
                    new Rest(0, 10, noteNumber),
                    new Rest(130, 170, noteNumber),
                    new Rest(350, 650, noteNumber),
                });
        }

        [TestCase(10, 10)]
        [TestCase(10, 5)]
        public void GetRests_Notes_RestsByNoteNumber_DifferentNoteNumbers(
            byte channel1,
            byte channel2)
        {
            var noteNumber1 = (SevenBitNumber)10;
            var noteNumber2 = (SevenBitNumber)100;

            GetRests(
                keySelector: obj => (obj as Note)?.NoteNumber,
                inputObjects: new ITimedObject[]
                {
                    new Note(noteNumber1, 100, 0) { Channel = (FourBitNumber)channel2 },
                    new Note(noteNumber2, 100, 30) { Channel = (FourBitNumber)channel1 },
                    new Note(noteNumber1, 50, 300) { Channel = (FourBitNumber)channel2 },
                    new Note(noteNumber2, 500, 1000) { Channel = (FourBitNumber)channel1 },
                },
                expectedRests: new[]
                {
                    new Rest(0, 30, noteNumber2),
                    new Rest(100, 200, noteNumber1),
                    new Rest(130, 870, noteNumber2),
                });
        }

        [Test]
        public void GetRests_Notes_RestsByChannelAndNoteNumber()
        {
            var noteNumber1 = (SevenBitNumber)10;
            var noteNumber2 = (SevenBitNumber)100;
            var channel1 = (FourBitNumber)10;
            var channel2 = (FourBitNumber)2;

            GetRests(
                keySelector: obj => ((obj as Note)?.Channel, (obj as Note)?.NoteNumber),
                inputObjects: new ITimedObject[]
                {
                    new Note(noteNumber1, 100, 10) { Channel = channel1 },
                    new Note(noteNumber2, 100, 30) { Channel = channel1 },
                    new Note(noteNumber1, 50, 300) { Channel = channel2 },
                    new Note(noteNumber2, 500, 1000) { Channel = channel2 },
                    new Note(noteNumber1, 150, 1200) { Channel = channel1 },
                    new Note(noteNumber2, 1000, 1300) { Channel = channel1 },
                },
                expectedRests: new[]
                {
                    new Rest(0, 10, (channel1, noteNumber1)),
                    new Rest(0, 30, (channel1, noteNumber2)),
                    new Rest(0, 300, (channel2, noteNumber1)),
                    new Rest(0, 1000, (channel2, noteNumber2)),
                    new Rest(110, 1090, (channel1, noteNumber1)),
                    new Rest(130, 1170, (channel1, noteNumber2)),
                });
        }

        [Test]
        public void GetRests_Notes_RestsByChannelAndNoteNumber_WithTimedEvents()
        {
            var noteNumber1 = (SevenBitNumber)10;
            var noteNumber2 = (SevenBitNumber)100;
            var channel1 = (FourBitNumber)10;
            var channel2 = (FourBitNumber)2;

            GetRests(
                keySelector: obj => obj is Note
                    ? (((Note)obj).Channel, ((Note)obj).NoteNumber)
                    : ((FourBitNumber, SevenBitNumber)?)null,
                inputObjects: new ITimedObject[]
                {
                    new Note(noteNumber1, 100, 10) { Channel = channel1 },
                    new TimedEvent(new TextEvent("A"), 15),
                    new Note(noteNumber2, 100, 30) { Channel = channel1 },
                    new TimedEvent(new TextEvent("B"), 120),
                    new Note(noteNumber1, 50, 300) { Channel = channel2 },
                    new Note(noteNumber2, 500, 1000) { Channel = channel2 },
                    new TimedEvent(new TextEvent("C"), 1100),
                    new TimedEvent(new TextEvent("D"), 1150),
                    new Note(noteNumber1, 150, 1200) { Channel = channel1 },
                    new Note(noteNumber2, 1000, 1300) { Channel = channel1 },
                    new TimedEvent(new TextEvent("E"), 2000),
                },
                expectedRests: new[]
                {
                    new Rest(0, 10, (channel1, noteNumber1)),
                    new Rest(0, 30, (channel1, noteNumber2)),
                    new Rest(0, 300, (channel2, noteNumber1)),
                    new Rest(0, 1000, (channel2, noteNumber2)),
                    new Rest(110, 1090, (channel1, noteNumber1)),
                    new Rest(130, 1170, (channel1, noteNumber2)),
                });
        }

        [Test]
        public void GetRests_TimedEvents_1()
        {
            GetRests(
                keySelector: obj => ((TextEvent)((TimedEvent)obj).Event).Text.Contains("A"),
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 15),
                    new TimedEvent(new TextEvent("B"), 120),
                    new TimedEvent(new TextEvent("CA"), 1100),
                    new TimedEvent(new TextEvent("D"), 1150),
                    new TimedEvent(new TextEvent("E"), 2000),
                },
                expectedRests: new[]
                {
                    new Rest(0, 15, true),
                    new Rest(0, 120, false),
                    new Rest(15, 1085, true),
                    new Rest(120, 1030, false),
                    new Rest(1150, 850, false),
                });
        }

        [Test]
        public void GetRests_TimedEvents_2()
        {
            GetRests(
                keySelector: obj => obj is TimedEvent ? SameKey : (int?)null,
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 15),
                    new TimedEvent(new TextEvent("B"), 120),
                    new TimedEvent(new TextEvent("C"), 1100),
                    new TimedEvent(new TextEvent("D"), 1150),
                    new TimedEvent(new TextEvent("E"), 2000),
                },
                expectedRests: new[]
                {
                    new Rest(0, 15, SameKey),
                    new Rest(15, 105, SameKey),
                    new Rest(120, 980, SameKey),
                    new Rest(1100, 50, SameKey),
                    new Rest(1150, 850, SameKey),
                });
        }

        [Test]
        public void GetRests_TimedEvents_3()
        {
            GetRests(
                keySelector: obj => obj is TimedEvent ? SameKey : (int?)null,
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 15),
                    new TimedEvent(new TextEvent("B"), 120),
                    new Note((SevenBitNumber)100, 20, 130),
                    new TimedEvent(new TextEvent("C"), 1100),
                    new TimedEvent(new TextEvent("D"), 1150),
                    new TimedEvent(new TextEvent("E"), 2000),
                    new Note((SevenBitNumber)100, 20, 2000),
                },
                expectedRests: new[]
                {
                    new Rest(0, 15, SameKey),
                    new Rest(15, 105, SameKey),
                    new Rest(120, 980, SameKey),
                    new Rest(1100, 50, SameKey),
                    new Rest(1150, 850, SameKey),
                });
        }

        [TestCase(10, 10)]
        [TestCase(10, 50)]
        public void GetRests_NotesAndChords_RestsByChannel_SameChannel(byte noteNumber1, byte noteNumber2)
        {
            var channel = (FourBitNumber)10;

            GetRests(
                keySelector: obj => (obj as Note)?.Channel ?? (obj as Chord)?.Channel,
                inputObjects: new ITimedObject[]
                {
                    new Note((SevenBitNumber)noteNumber1, 100, 10) { Channel = channel },
                    new Chord(
                        new Note((SevenBitNumber)noteNumber1, 100, 30),
                        new Note((SevenBitNumber)(noteNumber1 + 1), 100, 40)) { Channel = channel },
                    new Note((SevenBitNumber)noteNumber2, 50, 300) { Channel = channel },
                    new Note((SevenBitNumber)noteNumber1, 500, 1000) { Channel = channel },
                    new Note((SevenBitNumber)noteNumber2, 150, 1200) { Channel = channel },
                    new Note((SevenBitNumber)noteNumber1, 1000, 1300) { Channel = channel },
                    new Chord(
                        new Note((SevenBitNumber)noteNumber2, 1000, 10000),
                        new Note((SevenBitNumber)(noteNumber2 + 1), 1000, 10010),
                        new Note((SevenBitNumber)(noteNumber2 + 2), 1000, 10010)) { Channel = channel },
                },
                expectedRests: new[]
                {
                    new Rest(0, 10, channel),
                    new Rest(140, 160, channel),
                    new Rest(350, 650, channel),
                    new Rest(2300, 7700, channel),
                });
        }

        [Test]
        public void GetRests_TimedEvents_NullKeySelector()
        {
            GetRests(
                keySelector: null,
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 15),
                    new TimedEvent(new TextEvent("B"), 120),
                    new TimedEvent(new TextEvent("CA"), 1100),
                    new TimedEvent(new TextEvent("D"), 1150),
                    new TimedEvent(new TextEvent("E"), 2000),
                },
                expectedRests: Array.Empty<Rest>());
        }

        [Test]
        public void GetRests_Notes_NullKeySelector()
        {
            var noteNumber1 = (SevenBitNumber)10;
            var noteNumber2 = (SevenBitNumber)100;
            var channel1 = (FourBitNumber)10;
            var channel2 = (FourBitNumber)2;

            GetRests(
                keySelector: null,
                inputObjects: new ITimedObject[]
                {
                    new Note(noteNumber1, 100, 10) { Channel = channel1 },
                    new Note(noteNumber2, 100, 30) { Channel = channel1 },
                    new Note(noteNumber1, 50, 300) { Channel = channel2 },
                    new Note(noteNumber2, 500, 1000) { Channel = channel2 },
                    new Note(noteNumber1, 150, 1200) { Channel = channel1 },
                    new Note(noteNumber2, 1000, 1300) { Channel = channel1 },
                },
                expectedRests: Array.Empty<Rest>());
        }

        [Test]
        public void GetRests_UnsortedRandomCollection()
        {
            GetRests(
                keySelector: obj => obj is TimedEvent ? SameKey : (int?)null,
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("B"), 120),
                    new TimedEvent(new TextEvent("A"), 15),
                    new TimedEvent(new TextEvent("C"), 1100),
                    new Note((SevenBitNumber)100, 20, 130),
                    new TimedEvent(new TextEvent("E"), 2000),
                    new TimedEvent(new TextEvent("D"), 1150),
                    new Note((SevenBitNumber)100, 20, 2000),
                },
                expectedRests: new[]
                {
                    new Rest(0, 15, SameKey),
                    new Rest(15, 105, SameKey),
                    new Rest(120, 980, SameKey),
                    new Rest(1100, 50, SameKey),
                    new Rest(1150, 850, SameKey),
                });
        }

        [Test]
        public void GetRests_AfterGetObjects()
        {
            var rawObjects = new ITimedObject[]
            {
                new TimedEvent(new TextEvent("B"), 120),
                new TimedEvent(new TextEvent("A"), 15),
                new TimedEvent(new TextEvent("C"), 1100),
                new Note((SevenBitNumber)100, 20, 130),
                new TimedEvent(new TextEvent("E"), 2000),
                new TimedEvent(new TextEvent("D"), 1150),
                new Note((SevenBitNumber)100, 20, 2000),
            };

            GetRests(
                keySelector: obj => obj is TimedEvent ? SameKey : (int?)null,
                inputObjects: rawObjects.GetObjects(ObjectType.Note | ObjectType.TimedEvent),
                expectedRests: new[]
                {
                    new Rest(0, 15, SameKey),
                    new Rest(15, 105, SameKey),
                    new Rest(120, 980, SameKey),
                    new Rest(1100, 50, SameKey),
                    new Rest(1150, 850, SameKey),
                });
        }

        #endregion

        #region Private methods

        private void GetRests(
            Func<ITimedObject, object> keySelector,
            IEnumerable<ITimedObject> inputObjects,
            IEnumerable<Rest> expectedRests)
        {
            var actualRests = inputObjects
                .GetRests(new RestDetectionSettings
                {
                    KeySelector = keySelector
                })
                .ToArray();

            MidiAsserts.AreEqual(expectedRests, actualRests, true, 0, "Rests are invalid.");
        }

        #endregion
    }
}
