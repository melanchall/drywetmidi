using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class RepeaterTests
    {
        #region Test methods

        [Test]
        public void CheckRepeat_MultipleCollections_TimedEvents() => CheckRepeat(
            inputObjects: new[]
            {
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new TimedEvent(new ControlChangeEvent(), 20),
                },
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 10),
                    new TimedEvent(new ControlChangeEvent(), 30),
                }
            },
            repeatsNumber: 1,
            settings: null,
            expectedObjects: new[]
            {
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new TimedEvent(new ControlChangeEvent(), 20),
                    new TimedEvent(new TextEvent("A"), 30),
                    new TimedEvent(new ControlChangeEvent(), 50),
                },
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 10),
                    new TimedEvent(new ControlChangeEvent(), 30),
                    new TimedEvent(new TextEvent("A"), 40),
                    new TimedEvent(new ControlChangeEvent(), 60),
                }
            });

        [Test]
        public void CheckRepeat_MultipleCollections_TimedEventsAndNotes() => CheckRepeat(
            inputObjects: new[]
            {
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new Note((SevenBitNumber)70, 100, 5),
                    new TimedEvent(new ControlChangeEvent(), 20),
                },
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 10),
                    new TimedEvent(new ControlChangeEvent(), 30),
                    new Note((SevenBitNumber)80, 30, 40),
                }
            },
            repeatsNumber: 1,
            settings: null,
            expectedObjects: new[]
            {
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new Note((SevenBitNumber)70, 100, 5),
                    new TimedEvent(new ControlChangeEvent(), 20),
                    new TimedEvent(new TextEvent("A"), 105),
                    new Note((SevenBitNumber)70, 100, 110),
                    new TimedEvent(new ControlChangeEvent(), 125),
                },
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 10),
                    new TimedEvent(new ControlChangeEvent(), 30),
                    new Note((SevenBitNumber)80, 30, 40),
                    new TimedEvent(new TextEvent("A"), 115),
                    new TimedEvent(new ControlChangeEvent(), 135),
                    new Note((SevenBitNumber)80, 30, 145),
                }
            });

        #endregion

        #region Private methods

        private void CheckRepeat(
            ICollection<ICollection<ITimedObject>> inputObjects,
            int repeatsNumber,
            RepeatingSettings settings,
            ICollection<ICollection<ITimedObject>> expectedObjects) =>
            CheckRepeat(
                inputObjects,
                repeatsNumber,
                TempoMap.Default,
                settings,
                expectedObjects);

        private void CheckRepeat(
            ICollection<ICollection<ITimedObject>> inputObjects,
            int repeatsNumber,
            TempoMap tempoMap,
            RepeatingSettings settings,
            ICollection<ICollection<ITimedObject>> expectedObjects)
        {
            var inputTrackChunks = inputObjects.Select(obj => obj.ToTrackChunk()).ToArray();
            var actualTrackChunks = inputTrackChunks.Repeat(repeatsNumber, tempoMap, settings);
            MidiAsserts.AreEqual(expectedObjects.Select(obj => obj.ToTrackChunk()).ToArray(), actualTrackChunks, true, "Invalid result track chunks.");

            //

            var inputFile = new MidiFile(inputObjects.Select(obj => obj.ToTrackChunk()));
            var actualFile = inputFile.Repeat(repeatsNumber, settings);
            MidiAsserts.AreEqual(new MidiFile(expectedObjects.Select(obj => obj.ToTrackChunk())), actualFile, true, "Invalid result file.");
        }

        #endregion
    }
}
