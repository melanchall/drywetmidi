using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class SplitterTests
    {
        #region Test methods

        [Test]
        public void SplitObjectsAtDistance_Notes_EmptyCollection([Values] LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = Enumerable.Empty<Note>();
            var distance = (MidiTimeSpan)100;
            var expectedObjects = Enumerable.Empty<Note>();
            var actualObjects = Splitter.SplitObjectsAtDistance(inputObjects, distance, from, tempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OfType<ITimedObject>(),
                actualObjects.OfType<ITimedObject>(),
                true,
                0,
                "Objects are invalid.");
        }

        [Test]
        public void SplitObjectsAtDistance_Notes_Nulls([Values] LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = new[] { default(Note), default(Note) };
            var distance = (MidiTimeSpan)100;
            var expectedObjects = new[] { default(Note), default(Note) };
            var actualObjects = Splitter.SplitObjectsAtDistance(inputObjects, distance, from, tempoMap);

            MidiAsserts.AreEqual(
                expectedObjects.OfType<ITimedObject>(),
                actualObjects.OfType<ITimedObject>(),
                true,
                0,
                "Objects are invalid.");
        }

        [Test]
        public void SplitObjectsAtDistance_Notes_ZeroDistance([Values] LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputNotes(1000);
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
        public void SplitObjectsAtDistance_Notes_BigDistance([Values] LengthedObjectTarget from)
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputNotes(1000);
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
        public void SplitObjectsAtDistance_Notes_Start()
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputNotes(1000);
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
        public void SplitObjectsAtDistance_Notes_End()
        {
            var tempoMap = TempoMap.Default;

            var inputObjects = CreateInputNotes(1000);
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
    }
}
