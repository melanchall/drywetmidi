using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class BuildObjectsUtilitiesTests
    {
        #region Test methods

        [TestCase(10, 10, 50, 50)]
        [TestCase(10, 2, 50, 50)]
        [TestCase(10, 10, 50, 100)]
        [TestCase(10, 2, 50, 100)]
        public void BuildNotesAndRests_NoSeparation_FromNotes(
            byte channel1,
            byte channel2,
            byte noteNumber1,
            byte noteNumber2)
        {
            CheckBuildingNotesAndRests(
                restSeparationPolicy: RestSeparationPolicy.NoSeparation,
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
                });
        }

        [TestCase(10, 10)]
        [TestCase(10, 50)]
        public void BuildNotesAndRests_SeparateByChannel_SingleChannel_FromNotes(
            byte noteNumber1,
            byte noteNumber2)
        {
            var channel = (FourBitNumber)10;

            CheckBuildingNotesAndRests(
                restSeparationPolicy: RestSeparationPolicy.SeparateByChannel,
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
                });
        }

        [TestCase(10, 10)]
        [TestCase(10, 50)]
        public void BuildNotesAndRests_SeparateByChannel_DifferentChannels_FromNotes(
            byte noteNumber1,
            byte noteNumber2)
        {
            var channel1 = (FourBitNumber)10;
            var channel2 = (FourBitNumber)2;

            CheckBuildingNotesAndRests(
                restSeparationPolicy: RestSeparationPolicy.SeparateByChannel,
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
                    new Rest(0, 10, channel1, null),
                    new Rest(0, 30, channel2, null),
                    new Note((SevenBitNumber)noteNumber1, 100, 10) { Channel = channel1 },
                    new Note((SevenBitNumber)noteNumber1, 100, 30) { Channel = channel2 },
                    new Rest(110, 190, channel1, null),
                    new Rest(130, 870, channel2, null),
                    new Note((SevenBitNumber)noteNumber2, 50, 300) { Channel = channel1 },
                    new Rest(350, 850, channel1, null),
                    new Note((SevenBitNumber)noteNumber1, 500, 1000) { Channel = channel2 },
                    new Note((SevenBitNumber)noteNumber2, 150, 1200) { Channel = channel1 },
                    new Note((SevenBitNumber)noteNumber1, 1000, 1300) { Channel = channel2 },
                });
        }

        [TestCase(10, 10)]
        [TestCase(10, 5)]
        public void BuildNotesAndRests_SeparateByNoteNumber_SingleNoteNumber_FromNotes(
            byte channel1,
            byte channel2)
        {
            var noteNumber = (SevenBitNumber)10;

            CheckBuildingNotesAndRests(
                restSeparationPolicy: RestSeparationPolicy.SeparateByNoteNumber,
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
                    new Rest(0, 10, null, noteNumber),
                    new Note(noteNumber, 100, 10) { Channel = (FourBitNumber)channel2 },
                    new Note(noteNumber, 100, 30) { Channel = (FourBitNumber)channel1 },
                    new Rest(130, 170, null, noteNumber),
                    new Note(noteNumber, 50, 300) { Channel = (FourBitNumber)channel2 },
                    new Rest(350, 650, null, noteNumber),
                    new Note(noteNumber, 500, 1000) { Channel = (FourBitNumber)channel1 },
                    new Note(noteNumber, 150, 1200) { Channel = (FourBitNumber)channel2 },
                    new Note(noteNumber, 1000, 1300) { Channel = (FourBitNumber)channel1 },
                });
        }

        [TestCase(10, 10)]
        [TestCase(10, 5)]
        public void BuildNotesAndRests_SeparateByNoteNumber_DifferentNoteNumbers_FromNotes(
            byte channel1,
            byte channel2)
        {
            var noteNumber1 = (SevenBitNumber)10;
            var noteNumber2 = (SevenBitNumber)100;

            CheckBuildingNotesAndRests(
                restSeparationPolicy: RestSeparationPolicy.SeparateByNoteNumber,
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
                    new Rest(0, 30, null, noteNumber2),
                    new Note(noteNumber2, 100, 30) { Channel = (FourBitNumber)channel1 },
                    new Rest(100, 200, null, noteNumber1),
                    new Rest(130, 870, null, noteNumber2),
                    new Note(noteNumber1, 50, 300) { Channel = (FourBitNumber)channel2 },
                    new Note(noteNumber2, 500, 1000) { Channel = (FourBitNumber)channel1 },
                });
        }

        [Test]
        public void BuildNotesAndRests_SeparateByChannelAndNoteNumber_FromNotes()
        {
            var noteNumber1 = (SevenBitNumber)10;
            var noteNumber2 = (SevenBitNumber)100;
            var channel1 = (FourBitNumber)10;
            var channel2 = (FourBitNumber)2;

            CheckBuildingNotesAndRests(
                restSeparationPolicy: RestSeparationPolicy.SeparateByChannelAndNoteNumber,
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
                    new Rest(0, 10, channel1, noteNumber1),
                    new Rest(0, 30, channel1, noteNumber2),
                    new Rest(0, 300, channel2, noteNumber1),
                    new Rest(0, 1000, channel2, noteNumber2),
                    new Note(noteNumber1, 100, 10) { Channel = channel1 },
                    new Note(noteNumber2, 100, 30) { Channel = channel1 },
                    new Rest(110, 1090, channel1, noteNumber1),
                    new Rest(130, 1170, channel1, noteNumber2),
                    new Note(noteNumber1, 50, 300) { Channel = channel2 },
                    new Note(noteNumber2, 500, 1000) { Channel = channel2 },
                    new Note(noteNumber1, 150, 1200) { Channel = channel1 },
                    new Note(noteNumber2, 1000, 1300) { Channel = channel1 },
                });
        }

        [TestCase(10, 10, 50, 50)]
        [TestCase(10, 2, 50, 50)]
        [TestCase(10, 10, 50, 100)]
        [TestCase(10, 2, 50, 100)]
        public void BuildNotesAndRests_NoSeparation_FromNotesAndTimedEvents(
            byte channel1,
            byte channel2,
            byte noteNumber1,
            byte noteNumber2)
        {
            CheckBuildingNotesAndRests(
                restSeparationPolicy: RestSeparationPolicy.NoSeparation,
                inputObjects: new ITimedObject[]
                {
                    new Note((SevenBitNumber)noteNumber1, 100, 10) { Channel = (FourBitNumber)channel2 },
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)noteNumber1, Note.DefaultVelocity) { Channel = (FourBitNumber)channel1 }, 30),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)noteNumber1, SevenBitNumber.MinValue) { Channel = (FourBitNumber)channel1 }, 130),
                    new Note((SevenBitNumber)noteNumber2, 50, 300) { Channel = (FourBitNumber)channel2 },
                    new Note((SevenBitNumber)noteNumber1, 500, 1000) { Channel = (FourBitNumber)channel1 },
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)noteNumber2, Note.DefaultVelocity) { Channel = (FourBitNumber)channel2 }, 1200),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)noteNumber2, SevenBitNumber.MinValue) { Channel = (FourBitNumber)channel2 }, 1350),
                    new Note((SevenBitNumber)noteNumber1, 1000, 1300) { Channel = (FourBitNumber)channel1 },
                    new Note((SevenBitNumber)noteNumber2, 1000, 10000) { Channel = (FourBitNumber)channel2 },
                    new Note((SevenBitNumber)noteNumber1, 1000, 100000) { Channel = (FourBitNumber)channel1 },
                    new Note((SevenBitNumber)noteNumber2, 10, 100100) { Channel = (FourBitNumber)channel2 },
                    new Note((SevenBitNumber)noteNumber1, 10, 110000) { Channel = (FourBitNumber)channel1 },
                },
                outputObjects: new ITimedObject[]
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
                });
        }

        [TestCase(10, 10)]
        [TestCase(10, 50)]
        public void BuildNotesAndRests_SeparateByChannel_SingleChannel_FromNotesAndTimedEvents(
            byte noteNumber1,
            byte noteNumber2)
        {
            var channel = (FourBitNumber)10;

            CheckBuildingNotesAndRests(
                restSeparationPolicy: RestSeparationPolicy.SeparateByChannel,
                inputObjects: new ITimedObject[]
                {
                    new Note((SevenBitNumber)noteNumber1, 100, 10) { Channel = channel },
                    new Note((SevenBitNumber)noteNumber1, 100, 30) { Channel = channel },
                    new Note((SevenBitNumber)noteNumber2, 50, 300) { Channel = channel },
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)noteNumber1, Note.DefaultVelocity) { Channel = channel }, 1000),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)noteNumber1, SevenBitNumber.MinValue) { Channel = channel }, 1500),
                    new Note((SevenBitNumber)noteNumber2, 150, 1200) { Channel = channel },
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)noteNumber1, Note.DefaultVelocity) { Channel = channel }, 1300),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)noteNumber1, SevenBitNumber.MinValue) { Channel = channel }, 2300),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)noteNumber2, Note.DefaultVelocity) { Channel = channel }, 10000),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)noteNumber2, SevenBitNumber.MinValue) { Channel = channel }, 11000),
                },
                outputObjects: new ITimedObject[]
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
                });
        }

        [TestCase(10, 10)]
        [TestCase(10, 50)]
        public void BuildNotesAndRests_SeparateByChannel_DifferentChannels_FromNotesAndTimedEvents(
            byte noteNumber1,
            byte noteNumber2)
        {
            var channel1 = (FourBitNumber)10;
            var channel2 = (FourBitNumber)2;

            CheckBuildingNotesAndRests(
                restSeparationPolicy: RestSeparationPolicy.SeparateByChannel,
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)noteNumber1, Note.DefaultVelocity) { Channel = channel1 }, 10),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)noteNumber1, SevenBitNumber.MinValue) { Channel = channel1 }, 110),
                    new Note((SevenBitNumber)noteNumber1, 100, 30) { Channel = channel2 },
                    new Note((SevenBitNumber)noteNumber2, 50, 300) { Channel = channel1 },
                    new Note((SevenBitNumber)noteNumber1, 500, 1000) { Channel = channel2 },
                    new Note((SevenBitNumber)noteNumber2, 150, 1200) { Channel = channel1 },
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)noteNumber1, Note.DefaultVelocity) { Channel = channel2 }, 1300),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)noteNumber1, SevenBitNumber.MinValue) { Channel = channel2 }, 2300),
                },
                outputObjects: new ITimedObject[]
                {
                    new Rest(0, 10, channel1, null),
                    new Rest(0, 30, channel2, null),
                    new Note((SevenBitNumber)noteNumber1, 100, 10) { Channel = channel1 },
                    new Note((SevenBitNumber)noteNumber1, 100, 30) { Channel = channel2 },
                    new Rest(110, 190, channel1, null),
                    new Rest(130, 870, channel2, null),
                    new Note((SevenBitNumber)noteNumber2, 50, 300) { Channel = channel1 },
                    new Rest(350, 850, channel1, null),
                    new Note((SevenBitNumber)noteNumber1, 500, 1000) { Channel = channel2 },
                    new Note((SevenBitNumber)noteNumber2, 150, 1200) { Channel = channel1 },
                    new Note((SevenBitNumber)noteNumber1, 1000, 1300) { Channel = channel2 },
                });
        }

        [TestCase(10, 10)]
        [TestCase(10, 5)]
        public void BuildNotesAndRests_SeparateByNoteNumber_SingleNoteNumber_FromNotesAndTimedEvents(
            byte channel1,
            byte channel2)
        {
            var noteNumber = (SevenBitNumber)10;

            CheckBuildingNotesAndRests(
                restSeparationPolicy: RestSeparationPolicy.SeparateByNoteNumber,
                inputObjects: new ITimedObject[]
                {
                    new Note(noteNumber, 100, 10) { Channel = (FourBitNumber)channel2 },
                    new Note(noteNumber, 100, 30) { Channel = (FourBitNumber)channel1 },
                    new TimedEvent(new NoteOnEvent(noteNumber, Note.DefaultVelocity) { Channel = (FourBitNumber)channel2 }, 300),
                    new TimedEvent(new NoteOffEvent(noteNumber, SevenBitNumber.MinValue) { Channel = (FourBitNumber)channel2 }, 350),
                    new TimedEvent(new NoteOnEvent(noteNumber, Note.DefaultVelocity) { Channel = (FourBitNumber)channel1 }, 1000),
                    new TimedEvent(new NoteOffEvent(noteNumber, SevenBitNumber.MinValue) { Channel = (FourBitNumber)channel1 }, 1500),
                    new Note(noteNumber, 150, 1200) { Channel = (FourBitNumber)channel2 },
                    new Note(noteNumber, 1000, 1300) { Channel = (FourBitNumber)channel1 },
                },
                outputObjects: new ITimedObject[]
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
                });
        }

        [TestCase(10, 10)]
        [TestCase(10, 5)]
        public void BuildNotesAndRests_SeparateByNoteNumber_DifferentNoteNumbers_FromNotesAndTimedEvents(
            byte channel1,
            byte channel2)
        {
            var noteNumber1 = (SevenBitNumber)10;
            var noteNumber2 = (SevenBitNumber)100;

            CheckBuildingNotesAndRests(
                restSeparationPolicy: RestSeparationPolicy.SeparateByNoteNumber,
                inputObjects: new ITimedObject[]
                {
                    new Note(noteNumber1, 100, 0) { Channel = (FourBitNumber)channel2 },
                    new Note(noteNumber2, 100, 30) { Channel = (FourBitNumber)channel1 },
                    new Note(noteNumber1, 50, 300) { Channel = (FourBitNumber)channel2 },
                    new TimedEvent(new NoteOnEvent(noteNumber2, Note.DefaultVelocity) { Channel = (FourBitNumber)channel1 }, 1000),
                    new TimedEvent(new NoteOffEvent(noteNumber2, SevenBitNumber.MinValue) { Channel = (FourBitNumber)channel1 }, 1500),
                },
                outputObjects: new ITimedObject[]
                {
                    new Note(noteNumber1, 100, 0) { Channel = (FourBitNumber)channel2 },
                    new Rest(0, 30, null, noteNumber2),
                    new Note(noteNumber2, 100, 30) { Channel = (FourBitNumber)channel1 },
                    new Rest(100, 200, null, noteNumber1),
                    new Rest(130, 870, null, noteNumber2),
                    new Note(noteNumber1, 50, 300) { Channel = (FourBitNumber)channel2 },
                    new Note(noteNumber2, 500, 1000) { Channel = (FourBitNumber)channel1 },
                });
        }

        [Test]
        public void BuildNotesAndRests_SeparateByChannelAndNoteNumber_FromNotesAndTimedEvents()
        {
            var noteNumber1 = (SevenBitNumber)10;
            var noteNumber2 = (SevenBitNumber)100;
            var channel1 = (FourBitNumber)10;
            var channel2 = (FourBitNumber)2;

            CheckBuildingNotesAndRests(
                restSeparationPolicy: RestSeparationPolicy.SeparateByChannelAndNoteNumber,
                inputObjects: new ITimedObject[]
                {
                    new TimedEvent(new NoteOnEvent(noteNumber1, Note.DefaultVelocity) { Channel = channel1 }, 10),
                    new TimedEvent(new NoteOffEvent(noteNumber1, SevenBitNumber.MinValue) { Channel = channel1 }, 110),
                    new Note(noteNumber2, 100, 30) { Channel = channel1 },
                    new Note(noteNumber1, 50, 300) { Channel = channel2 },
                    new Note(noteNumber2, 500, 1000) { Channel = channel2 },
                    new Note(noteNumber1, 150, 1200) { Channel = channel1 },
                    new Note(noteNumber2, 1000, 1300) { Channel = channel1 },
                },
                outputObjects: new ITimedObject[]
                {
                    new Rest(0, 10, channel1, noteNumber1),
                    new Rest(0, 30, channel1, noteNumber2),
                    new Rest(0, 300, channel2, noteNumber1),
                    new Rest(0, 1000, channel2, noteNumber2),
                    new Note(noteNumber1, 100, 10) { Channel = channel1 },
                    new Note(noteNumber2, 100, 30) { Channel = channel1 },
                    new Rest(110, 1090, channel1, noteNumber1),
                    new Rest(130, 1170, channel1, noteNumber2),
                    new Note(noteNumber1, 50, 300) { Channel = channel2 },
                    new Note(noteNumber2, 500, 1000) { Channel = channel2 },
                    new Note(noteNumber1, 150, 1200) { Channel = channel1 },
                    new Note(noteNumber2, 1000, 1300) { Channel = channel1 },
                });
        }

        #endregion

        #region Private methods

        private void CheckBuildingNotesAndRests(
            RestSeparationPolicy restSeparationPolicy,
            IEnumerable<ITimedObject> inputObjects,
            IEnumerable<ITimedObject> outputObjects)
        {
            CheckObjectsBuilding(
                inputObjects,
                outputObjects,
                new ObjectsBuildingSettings
                {
                    BuildNotes = true,
                    BuildRests = true,
                    RestBuilderSettings = new RestBuilderSettings
                    {
                        RestSeparationPolicy = restSeparationPolicy
                    }
                });
        }

        #endregion
    }
}
