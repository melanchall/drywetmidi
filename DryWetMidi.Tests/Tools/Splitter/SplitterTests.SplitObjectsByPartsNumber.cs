using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    // TODO: check metric length type
    [TestFixture]
    public sealed partial class SplitterTests
    {
        #region Test methods

        [Test]
        public void SplitObjectsByPartsNumber_EmptyCollection([Values] TimeSpanType lengthType) => CheckSplitObjectsByPartsNumber(
            inputObjects: Enumerable.Empty<ILengthedObject>(),
            partsNumber: 10,
            lengthType: lengthType,
            expectedObjects: Enumerable.Empty<ILengthedObject>());

        [Test]
        public void SplitObjectsByPartsNumber_Nulls([Values] TimeSpanType lengthType) => CheckSplitObjectsByPartsNumber(
            inputObjects: new[]
            {
                default(ILengthedObject),
                default(ILengthedObject)
            },
            partsNumber: 10,
            lengthType: lengthType,
            expectedObjects: new[]
            {
                default(ILengthedObject),
                default(ILengthedObject)
            });

        [Test]
        public void SplitObjectsByPartsNumber_OnePart([Values] TimeSpanType lengthType) => CheckSplitObjectsByPartsNumber(
            inputObjects: new ILengthedObject[]
            {
                new Note((SevenBitNumber)70, 100, 20),
                new Chord(
                    new Note((SevenBitNumber)80, 100, 0),
                    new Note((SevenBitNumber)90, 150, 10))
            },
            partsNumber: 1,
            lengthType: lengthType,
            expectedObjects: new ILengthedObject[]
            {
                new Note((SevenBitNumber)70, 100, 20),
                new Chord(
                    new Note((SevenBitNumber)80, 100, 0),
                    new Note((SevenBitNumber)90, 150, 10))
            });

        [Test]
        public void SplitObjectsByPartsNumber_ZeroLength([Values] TimeSpanType lengthType) => CheckSplitObjectsByPartsNumber(
            inputObjects: new ILengthedObject[]
            {
                new Note((SevenBitNumber)70, 0, 20),
                new Chord(
                    new Note((SevenBitNumber)80, 0, 10),
                    new Note((SevenBitNumber)90, 0, 10))
            },
            partsNumber: 2,
            lengthType: lengthType,
            expectedObjects: new ILengthedObject[]
            {
                new Note((SevenBitNumber)70, 0, 20),
                new Note((SevenBitNumber)70, 0, 20),
                new Chord(
                    new Note((SevenBitNumber)80, 0, 10),
                    new Note((SevenBitNumber)90, 0, 10)),
                new Chord(
                    new Note((SevenBitNumber)80, 0, 10),
                    new Note((SevenBitNumber)90, 0, 10))
            });

        [Test]
        public void SplitObjectsByPartsNumber_EqualDivision_SingleNote_Midi() => CheckSplitObjectsByPartsNumber(
            inputObjects: new ILengthedObject[]
            {
                new Note((SevenBitNumber)70, 30, 20),
            },
            partsNumber: 5,
            lengthType: TimeSpanType.Midi,
            expectedObjects: new ILengthedObject[]
            {
                new Note((SevenBitNumber)70, 6, 20),
                new Note((SevenBitNumber)70, 6, 26),
                new Note((SevenBitNumber)70, 6, 32),
                new Note((SevenBitNumber)70, 6, 38),
                new Note((SevenBitNumber)70, 6, 44),
            });

        [Test]
        public void SplitObjectsByPartsNumber_EqualDivision_Midi() => CheckSplitObjectsByPartsNumber(
            inputObjects: new ILengthedObject[]
            {
                new Note((SevenBitNumber)70, 30, 20),
                new Chord(
                    new Note((SevenBitNumber)80, 40, 0),
                    new Note((SevenBitNumber)90, 50, 10))
            },
            partsNumber: 2,
            lengthType: TimeSpanType.Midi,
            expectedObjects: new ILengthedObject[]
            {
                new Note((SevenBitNumber)70, 15, 20),
                new Note((SevenBitNumber)70, 15, 35),
                new Chord(
                    new Note((SevenBitNumber)80, 30, 0),
                    new Note((SevenBitNumber)90, 20, 10)),
                new Chord(
                    new Note((SevenBitNumber)80, 10, 30),
                    new Note((SevenBitNumber)90, 30, 30))
            });

        [Test]
        public void SplitObjectsByPartsNumber_EqualDivision_BarBeatFraction()
        {
            var oneAndHalfBeatLength = LengthConverter.ConvertFrom(new BarBeatFractionTimeSpan(0, 1.5), 0, TempoMap.Default);
            CheckSplitObjectsByPartsNumber(
                inputObjects: new ILengthedObject[]
                {
                    new Note(
                        (SevenBitNumber)70,
                        LengthConverter.ConvertFrom(new BarBeatFractionTimeSpan(0, 3), 20, TempoMap.Default),
                        20),
                },
                partsNumber: 2,
                lengthType: TimeSpanType.BarBeatFraction,
                expectedObjects: new ILengthedObject[]
                {
                    new Note(
                        (SevenBitNumber)70,
                        oneAndHalfBeatLength,
                        20),
                    new Note(
                        (SevenBitNumber)70,
                        oneAndHalfBeatLength,
                        20 + oneAndHalfBeatLength),
                });
        }

        [Test]
        public void SplitObjectsByPartsNumber_UnequalDivision_SingleNote_Midi() => CheckSplitObjectsByPartsNumber(
            inputObjects: new ILengthedObject[]
            {
                new Note((SevenBitNumber)70, 30, 20),
            },
            partsNumber: 8,
            lengthType: TimeSpanType.Midi,
            expectedObjects: new ILengthedObject[]
            {
                new Note((SevenBitNumber)70, 4, 20),
                new Note((SevenBitNumber)70, 4, 24),
                new Note((SevenBitNumber)70, 4, 28),
                new Note((SevenBitNumber)70, 4, 32),
                new Note((SevenBitNumber)70, 4, 36),
                new Note((SevenBitNumber)70, 3, 40),
                new Note((SevenBitNumber)70, 4, 43),
                new Note((SevenBitNumber)70, 3, 47),
            });

        [Test]
        public void SplitObjectsByPartsNumber_UnequalDivision_SingleNote_Midi_SmallLength() => CheckSplitObjectsByPartsNumber(
            inputObjects: new ILengthedObject[]
            {
                new Note((SevenBitNumber)70, 5, 0),
            },
            partsNumber: 8,
            lengthType: TimeSpanType.Midi,
            expectedObjects: new ILengthedObject[]
            {
                new Note((SevenBitNumber)70, 1, 0),
                new Note((SevenBitNumber)70, 1, 1),
                new Note((SevenBitNumber)70, 1, 2),
                new Note((SevenBitNumber)70, 0, 3),
                new Note((SevenBitNumber)70, 1, 3),
                new Note((SevenBitNumber)70, 0, 4),
                new Note((SevenBitNumber)70, 1, 4),
                new Note((SevenBitNumber)70, 0, 5),
            });

        [Test]
        public void SplitObjectsByPartsNumber_UnequalDivision_SingleChord_Midi_SmallLength_1() => CheckSplitObjectsByPartsNumber(
            inputObjects: new ILengthedObject[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, 3, 0),
                    new Note((SevenBitNumber)80, 3, 0)),
            },
            partsNumber: 5,
            lengthType: TimeSpanType.Midi,
            expectedObjects: new ILengthedObject[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, 1, 0),
                    new Note((SevenBitNumber)80, 1, 0)),
                new Chord(
                    new Note((SevenBitNumber)70, 1, 1),
                    new Note((SevenBitNumber)80, 1, 1)),
                new Chord(
                    new Note((SevenBitNumber)70, 0, 2),
                    new Note((SevenBitNumber)80, 0, 2)),
                new Chord(
                    new Note((SevenBitNumber)70, 1, 2),
                    new Note((SevenBitNumber)80, 1, 2)),
                new Chord(
                    new Note((SevenBitNumber)70, 0, 3),
                    new Note((SevenBitNumber)80, 0, 3)),
            });

        [Test]
        public void SplitObjectsByPartsNumber_UnequalDivision_SingleChord_Midi_SmallLength_2() => CheckSplitObjectsByPartsNumber(
            inputObjects: new ILengthedObject[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, 3, 0),
                    new Note((SevenBitNumber)80, 3, 1)),
            },
            partsNumber: 5,
            lengthType: TimeSpanType.Midi,
            expectedObjects: new ILengthedObject[]
            {
                new Chord(
                    new Note((SevenBitNumber)70, 1, 0)),
                new Chord(
                    new Note((SevenBitNumber)70, 1, 1),
                    new Note((SevenBitNumber)80, 1, 1)),
                new Chord(
                    new Note((SevenBitNumber)70, 1, 2),
                    new Note((SevenBitNumber)80, 1, 2)),
                new Chord(
                    new Note((SevenBitNumber)80, 1, 3)),
                new Chord(
                    new Note((SevenBitNumber)80, 0, 4)),
            });

        #endregion

        #region Private methods

        private void CheckSplitObjectsByPartsNumber(
            IEnumerable<ILengthedObject> inputObjects,
            int partsNumber,
            TimeSpanType lengthType,
            IEnumerable<ILengthedObject> expectedObjects)
        {
            var actualObjects = inputObjects.SplitObjectsByPartsNumber(partsNumber, lengthType, TempoMap.Default).ToArray();
            MidiAsserts.AreEqual(
                expectedObjects,
                actualObjects,
                true,
                0,
                "Invalid result objects.");
        }

        // TODO: tests
        private void CheckSplitObjectsByPartsNumber_TrackChunk(
            IEnumerable<ILengthedObject> inputObjects,
            ObjectType objectType,
            int partsNumber,
            TimeSpanType lengthType,
            IEnumerable<TimedEvent> expectedEvents,
            ObjectDetectionSettings settings = null)
        {
            var trackChunk = inputObjects.ToTrackChunk();
            trackChunk.SplitObjectsByPartsNumber(objectType, partsNumber, lengthType, TempoMap.Default, settings);
            MidiAsserts.AreEqual(
                expectedEvents.ToTrackChunk(),
                trackChunk,
                true,
                "Invalid result track chunk.");

            var trackChunks = new[] { inputObjects.ToTrackChunk() };
            trackChunks.SplitObjectsByPartsNumber(objectType, partsNumber, lengthType, TempoMap.Default, settings);
            MidiAsserts.AreEqual(
                new[] { expectedEvents.ToTrackChunk() },
                trackChunks,
                true,
                "Invalid result track chunks.");

            var midiFile = inputObjects.ToFile();
            midiFile.SplitObjectsByPartsNumber(objectType, partsNumber, lengthType, settings);
            MidiAsserts.AreEqual(
                expectedEvents.ToFile(),
                midiFile,
                true,
                "Invalid result track file.");
        }

        #endregion
    }
}
