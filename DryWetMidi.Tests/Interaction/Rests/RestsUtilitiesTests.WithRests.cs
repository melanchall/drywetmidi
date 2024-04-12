using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class RestsUtilitiesTests
    {
        #region Test methods

        [TestCase(10, 10, 50, 50)]
        [TestCase(10, 2, 50, 50)]
        [TestCase(10, 10, 50, 100)]
        [TestCase(10, 2, 50, 100)]
        public void WithRests_Notes_SameKey(byte channel1, byte channel2, byte noteNumber1, byte noteNumber2) => WithRests(
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
            outputObjects: new ITimedObject[]
            {
                new Rest(0, 10, SameKey),
                new Note((SevenBitNumber)noteNumber1, 100, 10) { Channel = (FourBitNumber)channel2 },
                new Note((SevenBitNumber)noteNumber1, 100, 30) { Channel = (FourBitNumber)channel1 },
                new Rest(130, 170, SameKey),
                new Note((SevenBitNumber)noteNumber2, 50, 300) { Channel = (FourBitNumber)channel2 },
                new Rest(350, 650, SameKey),
                new Note((SevenBitNumber)noteNumber1, 500, 1000) { Channel = (FourBitNumber)channel1 },
                new Note((SevenBitNumber)noteNumber2, 150, 1200) { Channel = (FourBitNumber)channel2 },
                new Note((SevenBitNumber)noteNumber1, 1000, 1300) { Channel = (FourBitNumber)channel1 },
                new Rest(2300, 7700, SameKey),
                new Note((SevenBitNumber)noteNumber2, 1000, 10000) { Channel = (FourBitNumber)channel2 },
                new Rest(11000, 89000, SameKey),
                new Note((SevenBitNumber)noteNumber1, 1000, 100000) { Channel = (FourBitNumber)channel1 },
                new Note((SevenBitNumber)noteNumber2, 10, 100100) { Channel = (FourBitNumber)channel2 },
                new Rest(101000, 9000, SameKey),
                new Note((SevenBitNumber)noteNumber1, 10, 110000) { Channel = (FourBitNumber)channel1 },
            });

        [TestCase(10, 10)]
        [TestCase(10, 50)]
        public void WithRests_Notes_RestsByChannel_SameChannel(byte noteNumber1, byte noteNumber2)
        {
            var channel = (FourBitNumber)10;

            WithRests(
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
                outputObjects: new ITimedObject[]
                {
                    new Rest(0, 10, channel),
                    new Note((SevenBitNumber)noteNumber1, 100, 10) { Channel = channel },
                    new Note((SevenBitNumber)noteNumber1, 100, 30) { Channel = channel },
                    new Rest(130, 170, channel),
                    new Note((SevenBitNumber)noteNumber2, 50, 300) { Channel = channel },
                    new Rest(350, 650, channel),
                    new Note((SevenBitNumber)noteNumber1, 500, 1000) { Channel = channel },
                    new Note((SevenBitNumber)noteNumber2, 150, 1200) { Channel = channel },
                    new Note((SevenBitNumber)noteNumber1, 1000, 1300) { Channel = channel },
                    new Rest(2300, 7700, channel),
                    new Note((SevenBitNumber)noteNumber2, 1000, 10000) { Channel = channel },
                });
        }

        [TestCase(10, 10)]
        [TestCase(10, 50)]
        public void WithRests_Notes_RestsByChannel_DifferentChannels(byte noteNumber1, byte noteNumber2)
        {
            var channel1 = (FourBitNumber)10;
            var channel2 = (FourBitNumber)2;

            WithRests(
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
                outputObjects: new ITimedObject[]
                {
                    new Rest(0, 10, channel1),
                    new Rest(0, 30, channel2),
                    new Note((SevenBitNumber)noteNumber1, 100, 10) { Channel = channel1 },
                    new Note((SevenBitNumber)noteNumber1, 100, 30) { Channel = channel2 },
                    new Rest(110, 190, channel1),
                    new Rest(130, 870, channel2),
                    new Note((SevenBitNumber)noteNumber2, 50, 300) { Channel = channel1 },
                    new Rest(350, 850, channel1),
                    new Note((SevenBitNumber)noteNumber1, 500, 1000) { Channel = channel2 },
                    new Note((SevenBitNumber)noteNumber2, 150, 1200) { Channel = channel1 },
                    new Note((SevenBitNumber)noteNumber1, 1000, 1300) { Channel = channel2 },
                });
        }

        [TestCase(10, 10)]
        [TestCase(10, 5)]
        public void WithRests_Notes_RestsByNoteNumber_SameNoteNumber(byte channel1, byte channel2)
        {
            var noteNumber = (SevenBitNumber)10;

            WithRests(
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
                outputObjects: new ITimedObject[]
                {
                    new Rest(0, 10, noteNumber),
                    new Note(noteNumber, 100, 10) { Channel = (FourBitNumber)channel2 },
                    new Note(noteNumber, 100, 30) { Channel = (FourBitNumber)channel1 },
                    new Rest(130, 170, noteNumber),
                    new Note(noteNumber, 50, 300) { Channel = (FourBitNumber)channel2 },
                    new Rest(350, 650, noteNumber),
                    new Note(noteNumber, 500, 1000) { Channel = (FourBitNumber)channel1 },
                    new Note(noteNumber, 150, 1200) { Channel = (FourBitNumber)channel2 },
                    new Note(noteNumber, 1000, 1300) { Channel = (FourBitNumber)channel1 },
                });
        }

        [TestCase(10, 10)]
        [TestCase(10, 5)]
        public void WithRests_Notes_RestsByNoteNumber_DifferentNoteNumbers(byte channel1, byte channel2)
        {
            var noteNumber1 = (SevenBitNumber)10;
            var noteNumber2 = (SevenBitNumber)100;

            WithRests(
                keySelector: obj => (obj as Note)?.NoteNumber,
                inputObjects: new ITimedObject[]
                {
                    new Note(noteNumber1, 100, 0) { Channel = (FourBitNumber)channel2 },
                    new Note(noteNumber2, 100, 30) { Channel = (FourBitNumber)channel1 },
                    new Note(noteNumber1, 50, 300) { Channel = (FourBitNumber)channel2 },
                    new Note(noteNumber2, 500, 1000) { Channel = (FourBitNumber)channel1 },
                },
                outputObjects: new ITimedObject[]
                {
                    new Note(noteNumber1, 100, 0) { Channel = (FourBitNumber)channel2 },
                    new Rest(0, 30, noteNumber2),
                    new Note(noteNumber2, 100, 30) { Channel = (FourBitNumber)channel1 },
                    new Rest(100, 200, noteNumber1),
                    new Rest(130, 870, noteNumber2),
                    new Note(noteNumber1, 50, 300) { Channel = (FourBitNumber)channel2 },
                    new Note(noteNumber2, 500, 1000) { Channel = (FourBitNumber)channel1 },
                });
        }

        [Test]
        public void WithRests_Notes_RestsByChannelAndNoteNumber()
        {
            var noteNumber1 = (SevenBitNumber)10;
            var noteNumber2 = (SevenBitNumber)100;
            var channel1 = (FourBitNumber)10;
            var channel2 = (FourBitNumber)2;

            WithRests(
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
                outputObjects: new ITimedObject[]
                {
                    new Rest(0, 10, (channel1, noteNumber1)),
                    new Rest(0, 30, (channel1, noteNumber2)),
                    new Rest(0, 300, (channel2, noteNumber1)),
                    new Rest(0, 1000, (channel2, noteNumber2)),
                    new Note(noteNumber1, 100, 10) { Channel = channel1 },
                    new Note(noteNumber2, 100, 30) { Channel = channel1 },
                    new Rest(110, 1090, (channel1, noteNumber1)),
                    new Rest(130, 1170, (channel1, noteNumber2)),
                    new Note(noteNumber1, 50, 300) { Channel = channel2 },
                    new Note(noteNumber2, 500, 1000) { Channel = channel2 },
                    new Note(noteNumber1, 150, 1200) { Channel = channel1 },
                    new Note(noteNumber2, 1000, 1300) { Channel = channel1 },
                });
        }

        [Test]
        public void WithRests_Notes_RestsByChannelAndNoteNumber_WithTimedEvents()
        {
            var noteNumber1 = (SevenBitNumber)10;
            var noteNumber2 = (SevenBitNumber)100;
            var channel1 = (FourBitNumber)10;
            var channel2 = (FourBitNumber)2;

            WithRests(
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
                outputObjects: new ITimedObject[]
                {
                    new Rest(0, 10, (channel1, noteNumber1)),
                    new Rest(0, 30, (channel1, noteNumber2)),
                    new Rest(0, 300, (channel2, noteNumber1)),
                    new Rest(0, 1000, (channel2, noteNumber2)),
                    new Note(noteNumber1, 100, 10) { Channel = channel1 },
                    new TimedEvent(new TextEvent("A"), 15),
                    new Note(noteNumber2, 100, 30) { Channel = channel1 },
                    new Rest(110, 1090, (channel1, noteNumber1)),
                    new TimedEvent(new TextEvent("B"), 120),
                    new Rest(130, 1170, (channel1, noteNumber2)),
                    new Note(noteNumber1, 50, 300) { Channel = channel2 },
                    new Note(noteNumber2, 500, 1000) { Channel = channel2 },
                    new TimedEvent(new TextEvent("C"), 1100),
                    new TimedEvent(new TextEvent("D"), 1150),
                    new Note(noteNumber1, 150, 1200) { Channel = channel1 },
                    new Note(noteNumber2, 1000, 1300) { Channel = channel1 },
                    new TimedEvent(new TextEvent("E"), 2000),
                });
        }

        [Test]
        public void WithRests_TimedEvents_1()
        {
            WithRests(
                keySelector: obj => ((TextEvent)((TimedEvent)obj).Event).Text.Contains("A"),
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 15),
                    new TimedEvent(new TextEvent("B"), 120),
                    new TimedEvent(new TextEvent("CA"), 1100),
                    new TimedEvent(new TextEvent("D"), 1150),
                    new TimedEvent(new TextEvent("E"), 2000),
                },
                outputObjects: new ITimedObject[]
                {
                    new Rest(0, 15, true),
                    new Rest(0, 120, false),
                    new TimedEvent(new TextEvent("A"), 15),
                    new Rest(15, 1085, true),
                    new TimedEvent(new TextEvent("B"), 120),
                    new Rest(120, 1030, false),
                    new TimedEvent(new TextEvent("CA"), 1100),
                    new TimedEvent(new TextEvent("D"), 1150),
                    new Rest(1150, 850, false),
                    new TimedEvent(new TextEvent("E"), 2000),
                });
        }

        [Test]
        public void WithRests_TimedEvents_2()
        {
            WithRests(
                keySelector: obj => obj is TimedEvent ? SameKey : (int?)null,
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 15),
                    new TimedEvent(new TextEvent("B"), 120),
                    new TimedEvent(new TextEvent("C"), 1100),
                    new TimedEvent(new TextEvent("D"), 1150),
                    new TimedEvent(new TextEvent("E"), 2000),
                },
                outputObjects: new ITimedObject[]
                {
                    new Rest(0, 15, SameKey),
                    new TimedEvent(new TextEvent("A"), 15),
                    new Rest(15, 105, SameKey),
                    new TimedEvent(new TextEvent("B"), 120),
                    new Rest(120, 980, SameKey),
                    new TimedEvent(new TextEvent("C"), 1100),
                    new Rest(1100, 50, SameKey),
                    new TimedEvent(new TextEvent("D"), 1150),
                    new Rest(1150, 850, SameKey),
                    new TimedEvent(new TextEvent("E"), 2000),
                });
        }

        [Test]
        public void WithRests_TimedEvents_3()
        {
            WithRests(
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
                outputObjects: new ITimedObject[]
                {
                    new Rest(0, 15, SameKey),
                    new TimedEvent(new TextEvent("A"), 15),
                    new Rest(15, 105, SameKey),
                    new TimedEvent(new TextEvent("B"), 120),
                    new Rest(120, 980, SameKey),
                    new Note((SevenBitNumber)100, 20, 130),
                    new TimedEvent(new TextEvent("C"), 1100),
                    new Rest(1100, 50, SameKey),
                    new TimedEvent(new TextEvent("D"), 1150),
                    new Rest(1150, 850, SameKey),
                    new TimedEvent(new TextEvent("E"), 2000),
                    new Note((SevenBitNumber)100, 20, 2000),
                });
        }

        [TestCase(10, 10)]
        [TestCase(10, 50)]
        public void WithRests_NotesAndChords_RestsByChannel_SameChannel(byte noteNumber1, byte noteNumber2)
        {
            var channel = (FourBitNumber)10;

            WithRests(
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
                outputObjects: new ITimedObject[]
                {
                    new Rest(0, 10, channel),
                    new Note((SevenBitNumber)noteNumber1, 100, 10) { Channel = channel },
                    new Chord(
                        new Note((SevenBitNumber)noteNumber1, 100, 30),
                        new Note((SevenBitNumber)(noteNumber1 + 1), 100, 40)) { Channel = channel },
                    new Rest(140, 160, channel),
                    new Note((SevenBitNumber)noteNumber2, 50, 300) { Channel = channel },
                    new Rest(350, 650, channel),
                    new Note((SevenBitNumber)noteNumber1, 500, 1000) { Channel = channel },
                    new Note((SevenBitNumber)noteNumber2, 150, 1200) { Channel = channel },
                    new Note((SevenBitNumber)noteNumber1, 1000, 1300) { Channel = channel },
                    new Rest(2300, 7700, channel),
                    new Chord(
                        new Note((SevenBitNumber)noteNumber2, 1000, 10000),
                        new Note((SevenBitNumber)(noteNumber2 + 1), 1000, 10010),
                        new Note((SevenBitNumber)(noteNumber2 + 2), 1000, 10010)) { Channel = channel },
                });
        }

        [Test]
        public void WithRests_TimedEvents_NullKeySelector()
        {
            WithRests(
                keySelector: null,
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 15),
                    new TimedEvent(new TextEvent("B"), 120),
                    new TimedEvent(new TextEvent("CA"), 1100),
                    new TimedEvent(new TextEvent("D"), 1150),
                    new TimedEvent(new TextEvent("E"), 2000),
                },
                outputObjects: new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 15),
                    new TimedEvent(new TextEvent("B"), 120),
                    new TimedEvent(new TextEvent("CA"), 1100),
                    new TimedEvent(new TextEvent("D"), 1150),
                    new TimedEvent(new TextEvent("E"), 2000),
                });
        }

        [Test]
        public void WithRests_Notes_NullKeySelector()
        {
            var noteNumber1 = (SevenBitNumber)10;
            var noteNumber2 = (SevenBitNumber)100;
            var channel1 = (FourBitNumber)10;
            var channel2 = (FourBitNumber)2;

            WithRests(
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
                outputObjects: new ITimedObject[]
                {
                    new Note(noteNumber1, 100, 10) { Channel = channel1 },
                    new Note(noteNumber2, 100, 30) { Channel = channel1 },
                    new Note(noteNumber1, 50, 300) { Channel = channel2 },
                    new Note(noteNumber2, 500, 1000) { Channel = channel2 },
                    new Note(noteNumber1, 150, 1200) { Channel = channel1 },
                    new Note(noteNumber2, 1000, 1300) { Channel = channel1 },
                });
        }

        [Test]
        public void WithRests_UnsortedRandomCollection()
        {
            WithRests(
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
                outputObjects: new ITimedObject[]
                {
                    new Rest(0, 15, SameKey),
                    new TimedEvent(new TextEvent("A"), 15),
                    new Rest(15, 105, SameKey),
                    new TimedEvent(new TextEvent("B"), 120),
                    new Rest(120, 980, SameKey),
                    new Note((SevenBitNumber)100, 20, 130),
                    new TimedEvent(new TextEvent("C"), 1100),
                    new Rest(1100, 50, SameKey),
                    new TimedEvent(new TextEvent("D"), 1150),
                    new Rest(1150, 850, SameKey),
                    new TimedEvent(new TextEvent("E"), 2000),
                    new Note((SevenBitNumber)100, 20, 2000),
                });
        }

        [Test]
        public void WithRests_AfterGetObjects()
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

            WithRests(
                keySelector: obj => obj is TimedEvent ? SameKey : (int?)null,
                inputObjects: rawObjects.GetObjects(ObjectType.Note | ObjectType.TimedEvent),
                outputObjects: new ITimedObject[]
                {
                    new Rest(0, 15, SameKey),
                    new TimedEvent(new TextEvent("A"), 15),
                    new Rest(15, 105, SameKey),
                    new TimedEvent(new TextEvent("B"), 120),
                    new Rest(120, 980, SameKey),
                    new Note((SevenBitNumber)100, 20, 130),
                    new TimedEvent(new TextEvent("C"), 1100),
                    new Rest(1100, 50, SameKey),
                    new TimedEvent(new TextEvent("D"), 1150),
                    new Rest(1150, 850, SameKey),
                    new TimedEvent(new TextEvent("E"), 2000),
                    new Note((SevenBitNumber)100, 20, 2000),
                });
        }

        #endregion

        #region Private methods

        private void WithRests(
            Func<ITimedObject, object> keySelector,
            IEnumerable<ITimedObject> inputObjects,
            IEnumerable<ITimedObject> outputObjects)
        {
            var actualObjects = inputObjects
                .WithRests(new RestDetectionSettings
                {
                    KeySelector = keySelector
                });

            MidiAsserts.AreEqual(outputObjects, actualObjects, true, 0, "Objects are invalid.");
        }

        #endregion
    }
}
