using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestClass]
    public class PatternTests
    {
        private sealed class NoteInfo
        {
            #region Constructor

            public NoteInfo(NoteName noteName, int octave, ITime time, ILength length)
                : this(noteName, octave, time, length, Note.DefaultVelocity)
            {
            }

            public NoteInfo(NoteName noteName, int octave, ITime time, ILength length, SevenBitNumber velocity)
                : this(NoteUtilities.GetNoteNumber(noteName, octave), time, length, velocity)
            {
            }

            public NoteInfo(SevenBitNumber noteNumber, ITime time, ILength length, SevenBitNumber velocity)
            {
                NoteNumber = noteNumber;
                Time = time;
                Length = length;
                Velocity = velocity;
            }

            #endregion

            #region Properties

            public SevenBitNumber NoteNumber { get; }

            public ITime Time { get; }

            public ILength Length { get; }

            public SevenBitNumber Velocity { get; }

            #endregion
        }

        [TestMethod]
        [Description("Add two notes where first one takes default length and velocity and the second takes specified ones.")]
        public void Note_MixedLengthAndVelocity()
        {
            var defaultNoteLength = (MusicalLength)MusicalFraction.Quarter;
            var defaultVelocity = (SevenBitNumber)90;

            var specifiedLength = new MetricLength(0, 0, 10);
            var specifiedVelocity = (SevenBitNumber)95;

            var pattern = new PatternBuilder()
                .DefaultNoteLength(defaultNoteLength)
                .DefaultVelocity(defaultVelocity)

                .Note(OctaveDefinition.Get(0).A)
                .Note(OctaveDefinition.Get(1).C, specifiedVelocity, specifiedLength)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, null, defaultNoteLength, defaultVelocity),
                new NoteInfo(NoteName.C, 1, (MusicalTime)MusicalFraction.Quarter, specifiedLength, specifiedVelocity)
            });
        }

        [TestMethod]
        [Description("Add unkeyed anchor after some time movings, jump to the anchor with MoveToFirstAnchor and add note.")]
        public void MoveToFirstAnchor_Unkeyed_OneUnkeyed()
        {
            var moveTime = new MetricTime(0, 0, 10);
            var step = new MetricLength(0, 0, 11);
            var anchorTime = moveTime + step;

            var pattern = new PatternBuilder()
                .MoveToTime(moveTime)
                .StepForward(step)
                .Anchor()
                .MoveToTime(new MetricTime(0, 0, 30))
                .StepBack(new MetricLength(0, 0, 5))
                .MoveToFirstAnchor()

                .Note(OctaveDefinition.Get(0).A)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, anchorTime, (MusicalLength)MusicalFraction.Quarter)
            });
        }

        [TestMethod]
        [Description("Add unkeyed and keyed anchors after some time movings, jump to an anchor with MoveToFirstAnchor and add note.")]
        public void MoveToFirstAnchor_Unkeyed_OneUnkeyedAndOneKeyed()
        {
            var moveTime = new MetricTime(0, 0, 10);
            var step = new MetricLength(0, 0, 11);
            var anchorTime = moveTime + step;

            var pattern = new PatternBuilder()
                .MoveToTime(moveTime)
                .StepForward(step)
                .Anchor()
                .MoveToTime(new MetricTime(0, 0, 30))
                .Anchor("Test")
                .StepBack(new MetricLength(0, 0, 5))
                .MoveToFirstAnchor()

                .Note(OctaveDefinition.Get(0).A)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, anchorTime, (MusicalLength)MusicalFraction.Quarter)
            });
        }

        [TestMethod]
        [Description("Add unkeyed and keyed anchors after some time movings, jump to an anchor with MoveToFirstAnchor(key) and add note.")]
        public void MoveToFirstAnchor_Keyed_OneUnkeyedAndOneKeyed()
        {
            var anchorTime = new MetricTime(0, 0, 30);

            var pattern = new PatternBuilder()
                .MoveToTime(new MetricTime(0, 0, 10))
                .StepForward(new MetricLength(0, 0, 11))
                .Anchor()
                .MoveToTime(anchorTime)
                .Anchor("Test")
                .StepBack(new MetricLength(0, 0, 5))
                .MoveToFirstAnchor("Test")

                .Note(OctaveDefinition.Get(0).A)

                .Build();

            TestNotes(pattern, new[]
            {
                new NoteInfo(NoteName.A, 0, anchorTime, (MusicalLength)MusicalFraction.Quarter)
            });
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        [Description("Add no anchors and try to jump to an anchor with MoveToFirstAnchor.")]
        public void MoveToFirstAnchor_Unkeyed_NoAnchors()
        {
            var pattern = new PatternBuilder()
                .MoveToTime(new MetricTime(0, 0, 10))
                .StepForward(new MetricLength(0, 0, 11))
                .MoveToTime(new MetricTime(0, 0, 30))
                .StepBack(new MetricLength(0, 0, 5))
                .MoveToFirstAnchor()

                .Note(OctaveDefinition.Get(0).A)

                .Build();
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        [Description("Add unkeyed anchor and try to jump to an anchor with MoveToFirstAnchor(key).")]
        public void MoveToFirstAnchor_Keyed_NoKeyedAnchors()
        {
            var pattern = new PatternBuilder()
                .MoveToTime(new MetricTime(0, 0, 10))
                .StepForward(new MetricLength(0, 0, 11))
                .MoveToTime(new MetricTime(0, 0, 30))
                .Anchor()
                .StepBack(new MetricLength(0, 0, 5))
                .MoveToFirstAnchor("Test")

                .Note(OctaveDefinition.Get(0).A)

                .Build();
        }

        private static void TestNotes(Pattern pattern, ICollection<NoteInfo> expectedNotesInfos)
        {
            var channel = (FourBitNumber)2;

            var midiFile = pattern.ToFile(channel);
            var tempoMap = midiFile.GetTempoMap();

            var expectedNotes = expectedNotesInfos.Select(i =>
            {
                var expectedTime = TimeConverter.ConvertFrom(i.Time ?? new MetricTime(), tempoMap);
                var expectedLength = LengthConverter.ConvertFrom(i.Length, expectedTime, tempoMap);

                return new Note(i.NoteNumber, expectedLength, expectedTime)
                {
                    Velocity = i.Velocity,
                    Channel = channel
                };
            })
            .ToList();

            CollectionAssert.AreEqual(expectedNotes, midiFile.GetNotes().ToList());
        }
    }
}
