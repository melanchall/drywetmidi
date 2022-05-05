using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class SplitterTests
    {
        #region Test methods

        [Test]
        public void SplitObjectsAtDistance_Notes_EmptyCollection([Values] LengthedObjectTarget from) => SplitObjectsAtDistance(
            inputObjects: Array.Empty<Note>(),
            distance: (MidiTimeSpan)100,
            from: from,
            expectedObjects: Array.Empty<Note>());

        [Test]
        public void SplitObjectsAtDistance_Notes_Nulls([Values] LengthedObjectTarget from) => SplitObjectsAtDistance(
            inputObjects: new[] { default(ITimedObject), default(ITimedObject) },
            distance: (MidiTimeSpan)100,
            from: from,
            expectedObjects: new[] { default(ITimedObject), default(ITimedObject) });

        [Test]
        public void SplitObjectsAtDistance_Notes_ZeroDistance([Values] LengthedObjectTarget from)
        {
            var inputObjects = CreateInputNotes(1000);
            SplitObjectsAtDistance(
                inputObjects: inputObjects,
                distance: (MidiTimeSpan)0,
                from: from,
                expectedObjects: inputObjects.Select(o => o.Clone()).ToArray());
        }

        [Test]
        public void SplitObjectsAtDistance_Notes_BigDistance([Values] LengthedObjectTarget from)
        {
            var inputObjects = CreateInputNotes(1000);
            SplitObjectsAtDistance(
                inputObjects: inputObjects,
                distance: (MidiTimeSpan)1000,
                from: from,
                expectedObjects: inputObjects.Select(o => o.Clone()).ToArray());
        }

        [Test]
        public void SplitObjectsAtDistance_Notes_Start()
        {
            var inputObjects = CreateInputNotes(1000);
            SplitObjectsAtDistance(
                inputObjects: inputObjects,
                distance: (MidiTimeSpan)10,
                from: LengthedObjectTarget.Start,
                expectedObjects: inputObjects.SelectMany(o => Split(o, new[] { o.Time + 10 })).ToArray());
        }

        [Test]
        public void SplitObjectsAtDistance_Notes_End()
        {
            var inputObjects = CreateInputNotes(1000);
            SplitObjectsAtDistance(
                inputObjects: inputObjects,
                distance: (MidiTimeSpan)10,
                from: LengthedObjectTarget.End,
                expectedObjects: inputObjects.SelectMany(o => Split(o, new[] { o.Time + o.Length - 10 })).ToArray());
        }

        [Test]
        public void SplitObjectsAtDistance_Notes_ByRatio_EmptyCollection([Values] LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = Enumerable.Empty<Note>();
            var ratio = 0.5;
            var expectedObjects = Enumerable.Empty<Note>();
            var actualObjects = Splitter.SplitObjectsAtDistance(inputObjects, ratio, TimeSpanType.Midi, from, tempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OfType<ITimedObject>(),
                actualObjects.OfType<ITimedObject>(),
                true,
                0,
                "Objects are invalid.");
        }

        [Test]
        public void SplitObjectsAtDistance_Notes_ByRatio_Nulls([Values] LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = new[] { default(Note), default(Note) };
            var ratio = 0.5;
            var expectedObjects = new[] { default(Note), default(Note) };
            var actualObjects = Splitter.SplitObjectsAtDistance(inputObjects, ratio, TimeSpanType.Midi, from, tempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OfType<ITimedObject>(),
                actualObjects.OfType<ITimedObject>(),
                true,
                0,
                "Objects are invalid.");
        }

        [Test]
        public void SplitObjectsAtDistance_Notes_ByRatio_ZeroRatio([Values] LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputNotes(1000);
            var ratio = 0.0;
            var expectedObjects = inputObjects.Select(o => o.Clone());
            var actualObjects = Splitter.SplitObjectsAtDistance(inputObjects, ratio, TimeSpanType.Midi, from, tempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OfType<ITimedObject>(),
                actualObjects.OfType<ITimedObject>(),
                true,
                0,
                "Objects are invalid.");
        }

        [Test]
        public void SplitObjectsAtDistance_Notes_FullLengthRatio([Values] LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputNotes(1000);
            var ratio = 1.0;
            var expectedObjects = inputObjects.Select(o => o.Clone());
            var actualObjects = Splitter.SplitObjectsAtDistance(inputObjects, ratio, TimeSpanType.Midi, from, tempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OfType<ITimedObject>(),
                actualObjects.OfType<ITimedObject>(),
                true,
                0,
                "Objects are invalid.");
        }

        [Test]
        public void SplitObjectsAtDistance_Notes_ByRatio_Start()
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputNotes(1000);
            var ratio = 0.1;
            var expectedObjects = inputObjects.SelectMany(o => Split(o, new[] { o.Time + 100 }));
            var actualObjects = Splitter.SplitObjectsAtDistance(inputObjects, ratio, TimeSpanType.Midi, LengthedObjectTarget.Start, tempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OfType<ITimedObject>(),
                actualObjects.OfType<ITimedObject>(),
                true,
                0,
                "Objects are invalid.");
        }

        [Test]
        public void SplitObjectsAtDistance_Notes_ByRatio_End()
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputNotes(1000);
            var ratio = 0.1;
            var expectedObjects = inputObjects.SelectMany(o => Split(o, new[] { o.Time + o.Length - 100 }));
            var actualObjects = Splitter.SplitObjectsAtDistance(inputObjects, ratio, TimeSpanType.Midi, LengthedObjectTarget.End, tempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OfType<ITimedObject>(),
                actualObjects.OfType<ITimedObject>(),
                true,
                0,
                "Objects are invalid.");
        }

        [Test]
        public void SplitObjectsAtDistance_Chords_EmptyCollection([Values] LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = Enumerable.Empty<Chord>();
            var distance = (MidiTimeSpan)100;
            var expectedObjects = Enumerable.Empty<Chord>();
            var actualObjects = Splitter.SplitObjectsAtDistance(inputObjects, distance, from, tempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OfType<ITimedObject>(),
                actualObjects.OfType<ITimedObject>(),
                true,
                0,
                "Objects are invalid.");
        }

        [Test]
        public void SplitObjectsAtDistance_Chords_Nulls([Values] LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = new[] { default(Chord), default(Chord) };
            var distance = (MidiTimeSpan)100;
            var expectedObjects = new[] { default(Chord), default(Chord) };
            var actualObjects = Splitter.SplitObjectsAtDistance(inputObjects, distance, from, tempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OfType<ITimedObject>(),
                actualObjects.OfType<ITimedObject>(),
                true,
                0,
                "Objects are invalid.");
        }

        [Test]
        public void SplitObjectsAtDistance_Chords_ZeroDistance([Values] LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputChords(1000);
            var distance = (MidiTimeSpan)0;
            var expectedObjects = inputObjects.Select(o => o.Clone());
            var actualObjects = Splitter.SplitObjectsAtDistance(inputObjects, distance, from, tempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OfType<ITimedObject>(),
                actualObjects.OfType<ITimedObject>(),
                true,
                0,
                "Objects are invalid.");
        }

        [Test]
        public void SplitObjectsAtDistance_Chords_BigDistance([Values] LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputChords(1000);
            var distance = (MidiTimeSpan)1000;
            var expectedObjects = inputObjects.Select(o => o.Clone());
            var actualObjects = Splitter.SplitObjectsAtDistance(inputObjects, distance, from, tempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OfType<ITimedObject>(),
                actualObjects.OfType<ITimedObject>(),
                true,
                0,
                "Objects are invalid.");
        }

        [Test]
        public void SplitObjectsAtDistance_Chords_Start()
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputChords(1000);
            var distance = (MidiTimeSpan)10;
            var expectedObjects = inputObjects.SelectMany(o => Split(o, new[] { o.Time + distance }));
            var actualObjects = Splitter.SplitObjectsAtDistance(inputObjects, distance, LengthedObjectTarget.Start, tempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OfType<ITimedObject>(),
                actualObjects.OfType<ITimedObject>(),
                true,
                0,
                "Objects are invalid.");
        }

        [Test]
        public void SplitObjectsAtDistance_Chords_End()
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputChords(1000);
            var distance = (MidiTimeSpan)10;
            var expectedObjects = inputObjects.SelectMany(o => Split(o, new[] { o.Time + o.Length - distance }));
            var actualObjects = Splitter.SplitObjectsAtDistance(inputObjects, distance, LengthedObjectTarget.End, tempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OfType<ITimedObject>(),
                actualObjects.OfType<ITimedObject>(),
                true,
                0,
                "Objects are invalid.");
        }

        [Test]
        public void SplitObjectsAtDistance_Chords_ByRatio_EmptyCollection([Values] LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = Enumerable.Empty<Chord>();
            var ratio = 0.5;
            var expectedObjects = Enumerable.Empty<Chord>();
            var actualObjects = Splitter.SplitObjectsAtDistance(inputObjects, ratio, TimeSpanType.Midi, from, tempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OfType<ITimedObject>(),
                actualObjects.OfType<ITimedObject>(),
                true,
                0,
                "Objects are invalid.");
        }

        [Test]
        public void SplitObjectsAtDistance_Chords_ByRatio_Nulls([Values] LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = new[] { default(Chord), default(Chord) };
            var ratio = 0.5;
            var expectedObjects = new[] { default(Chord), default(Chord) };
            var actualObjects = Splitter.SplitObjectsAtDistance(inputObjects, ratio, TimeSpanType.Midi, from, tempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OfType<ITimedObject>(),
                actualObjects.OfType<ITimedObject>(),
                true,
                0,
                "Objects are invalid.");
        }

        [Test]
        public void SplitObjectsAtDistance_Chords_ByRatio_ZeroRatio([Values] LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputChords(1000);
            var ratio = 0.0;
            var expectedObjects = inputObjects.Select(o => o.Clone());
            var actualObjects = Splitter.SplitObjectsAtDistance(inputObjects, ratio, TimeSpanType.Midi, from, tempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OfType<ITimedObject>(),
                actualObjects.OfType<ITimedObject>(),
                true,
                0,
                "Objects are invalid.");
        }

        [Test]
        public void SplitObjectsAtDistance_Chords_FullLengthRatio([Values] LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputChords(1000);
            var ratio = 1.0;
            var expectedObjects = inputObjects.Select(o => o.Clone());
            var actualObjects = Splitter.SplitObjectsAtDistance(inputObjects, ratio, TimeSpanType.Midi, from, tempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OfType<ITimedObject>(),
                actualObjects.OfType<ITimedObject>(),
                true,
                0,
                "Objects are invalid.");
        }

        [Test]
        public void SplitObjectsAtDistance_Chords_ByRatio_Start()
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputChords(1000);
            var ratio = 0.1;
            var expectedObjects = inputObjects.SelectMany(o => Split(o, new[] { o.Time + 100 }));
            var actualObjects = Splitter.SplitObjectsAtDistance(inputObjects, ratio, TimeSpanType.Midi, LengthedObjectTarget.Start, tempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OfType<ITimedObject>(),
                actualObjects.OfType<ITimedObject>(),
                true,
                0,
                "Objects are invalid.");
        }

        [Test]
        public void SplitObjectsAtDistance_Chords_ByRatio_End()
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputChords(1000);
            var ratio = 0.1;
            var expectedObjects = inputObjects.SelectMany(o => Split(o, new[] { o.Time + o.Length - 100 }));
            var actualObjects = Splitter.SplitObjectsAtDistance(inputObjects, ratio, TimeSpanType.Midi, LengthedObjectTarget.End, tempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OfType<ITimedObject>(),
                actualObjects.OfType<ITimedObject>(),
                true,
                0,
                "Objects are invalid.");
        }

        #endregion

        #region Private methods

        private void SplitObjectsAtDistance(
            ICollection<ITimedObject> inputObjects,
            ITimeSpan distance,
            LengthedObjectTarget from,
            ICollection<ITimedObject> expectedObjects)
        {
            var actualObjects = inputObjects.SplitObjectsAtDistance(distance, from, TempoMap.Default).ToArray();
            MidiAsserts.AreEqual(
                expectedObjects,
                actualObjects,
                true,
                0,
                "Invalid result objects.");
        }

        #endregion
    }
}
