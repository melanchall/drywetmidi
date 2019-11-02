using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class ResizeNotesUtilitiesTests
    {
        #region Constants

        private static readonly TempoMap TempoMap = GetTempoMap();
        private static readonly NoteMethods NoteMethods = new NoteMethods();

        #endregion

        #region Test methods

        [Test]
        public void ResizeNotes_EmptyCollection()
        {
            ResizeNotes(
                Enumerable.Empty<Note>(),
                Enumerable.Empty<TimeAndLength>(),
                new MetricTimeSpan(0, 2, 0),
                TimeSpanType.Metric,
                TempoMap);
        }

        [Test]
        public void ResizeNotes_ZeroLength()
        {
            ResizeNotes(
                new[]
                {
                    NoteMethods.Create(0, 0),
                    null,
                    null,
                    NoteMethods.Create(10, 0),
                    NoteMethods.Create(100, 0)
                },
                new[]
                {
                    new TimeAndLength((MidiTimeSpan)0, (MidiTimeSpan)0),
                    new TimeAndLength((MidiTimeSpan)100, (MidiTimeSpan)0),
                    new TimeAndLength((MidiTimeSpan)1000, (MidiTimeSpan)0)
                },
                (MidiTimeSpan)1000,
                TimeSpanType.Midi,
                TempoMap);
        }

        [Test]
        public void ResizeNotes_Stretch_Midi()
        {
            ResizeNotes(
                new[]
                {
                    NoteMethods.Create(0, 10),
                    NoteMethods.Create(10, 20),
                    NoteMethods.Create(100, 400)
                },
                new[]
                {
                    new TimeAndLength((MidiTimeSpan)0, (MidiTimeSpan)40),
                    new TimeAndLength((MidiTimeSpan)40, (MidiTimeSpan)80),
                    new TimeAndLength((MidiTimeSpan)400, (MidiTimeSpan)1600)
                },
                (MidiTimeSpan)2000,
                TimeSpanType.Midi,
                TempoMap);
        }

        [Test]
        public void ResizeNotes_Stretch_Metric()
        {
            var tempoMap = TempoMap;

            ResizeNotes(
                new[]
                {
                    NoteMethods.Create(new MetricTimeSpan(0, 0, 0), new MetricTimeSpan(0, 0, 15), tempoMap),
                    NoteMethods.Create(new MetricTimeSpan(0, 0, 10), new MetricTimeSpan(0, 0, 1), tempoMap),
                    NoteMethods.Create(new MetricTimeSpan(0, 1, 0), new MetricTimeSpan(0, 2, 0), tempoMap)
                },
                new[]
                {
                    new TimeAndLength(new MetricTimeSpan(0, 0, 0), new MetricTimeSpan(0, 0, 22, 500)),
                    new TimeAndLength(new MetricTimeSpan(0, 0, 15), new MetricTimeSpan(0, 0, 1, 500)),
                    new TimeAndLength(new MetricTimeSpan(0, 1, 30), new MetricTimeSpan(0, 3, 0))
                },
                new MetricTimeSpan(0, 4, 30),
                TimeSpanType.Metric,
                TempoMap);
        }

        [Test]
        public void ResizeNotes_Shrink_Midi()
        {
            ResizeNotes(
                new[]
                {
                    NoteMethods.Create(0, 10),
                    NoteMethods.Create(10, 20),
                    NoteMethods.Create(100, 400)
                },
                new[]
                {
                    new TimeAndLength((MidiTimeSpan)0, (MidiTimeSpan)2),
                    new TimeAndLength((MidiTimeSpan)2, (MidiTimeSpan)4),
                    new TimeAndLength((MidiTimeSpan)20, (MidiTimeSpan)80)
                },
                (MidiTimeSpan)100,
                TimeSpanType.Midi,
                TempoMap);
        }

        [Test]
        public void ResizeNotes_Shrink_Metric()
        {
            var tempoMap = TempoMap;

            ResizeNotes(
                new[]
                {
                    NoteMethods.Create(new MetricTimeSpan(0, 0, 0), new MetricTimeSpan(0, 0, 15), tempoMap),
                    NoteMethods.Create(new MetricTimeSpan(0, 0, 10), new MetricTimeSpan(0, 0, 1), tempoMap),
                    NoteMethods.Create(new MetricTimeSpan(0, 1, 0), new MetricTimeSpan(0, 1, 0), tempoMap)
                },
                new[]
                {
                    new TimeAndLength(new MetricTimeSpan(0, 0, 0), new MetricTimeSpan(0, 0, 3, 750)),
                    new TimeAndLength(new MetricTimeSpan(0, 0, 2, 500), new MetricTimeSpan(0, 0, 0, 250)),
                    new TimeAndLength(new MetricTimeSpan(0, 0, 15), new MetricTimeSpan(0, 0, 15))
                },
                new MetricTimeSpan(0, 0, 30),
                TimeSpanType.Metric,
                TempoMap);
        }

        [Test]
        public void ResizeNotesByRatio_EmptyCollection()
        {
            ResizeNotesByRatio(
                Enumerable.Empty<Note>(),
                Enumerable.Empty<TimeAndLength>(),
                2.0,
                TimeSpanType.Metric,
                TempoMap);
        }

        [Test]
        public void ResizeNotesByRatio_ZeroLength()
        {
            ResizeNotesByRatio(
                new[]
                {
                    NoteMethods.Create(0, 0),
                    null,
                    null,
                    NoteMethods.Create(10, 0),
                    NoteMethods.Create(100, 0)
                },
                new[]
                {
                    new TimeAndLength((MidiTimeSpan)0, (MidiTimeSpan)0),
                    new TimeAndLength((MidiTimeSpan)100, (MidiTimeSpan)0),
                    new TimeAndLength((MidiTimeSpan)1000, (MidiTimeSpan)0)
                },
                10.0,
                TimeSpanType.Midi,
                TempoMap);
        }

        [Test]
        public void ResizeNotesByRatio_Stretch_Midi()
        {
            ResizeNotesByRatio(
                new[]
                {
                    NoteMethods.Create(0, 10),
                    NoteMethods.Create(10, 20),
                    NoteMethods.Create(100, 400)
                },
                new[]
                {
                    new TimeAndLength((MidiTimeSpan)0, (MidiTimeSpan)40),
                    new TimeAndLength((MidiTimeSpan)40, (MidiTimeSpan)80),
                    new TimeAndLength((MidiTimeSpan)400, (MidiTimeSpan)1600)
                },
                4.0,
                TimeSpanType.Midi,
                TempoMap);
        }

        [Test]
        public void ResizeNotesByRatio_Stretch_Metric()
        {
            var tempoMap = TempoMap;

            ResizeNotesByRatio(
                new[]
                {
                    NoteMethods.Create(new MetricTimeSpan(0, 0, 0), new MetricTimeSpan(0, 0, 15), tempoMap),
                    NoteMethods.Create(new MetricTimeSpan(0, 0, 10), new MetricTimeSpan(0, 0, 1), tempoMap),
                    NoteMethods.Create(new MetricTimeSpan(0, 1, 0), new MetricTimeSpan(0, 2, 0), tempoMap)
                },
                new[]
                {
                    new TimeAndLength(new MetricTimeSpan(0, 0, 0), new MetricTimeSpan(0, 0, 22, 500)),
                    new TimeAndLength(new MetricTimeSpan(0, 0, 15), new MetricTimeSpan(0, 0, 1, 500)),
                    new TimeAndLength(new MetricTimeSpan(0, 1, 30), new MetricTimeSpan(0, 3, 0))
                },
                1.5,
                TimeSpanType.Metric,
                TempoMap);
        }

        [Test]
        public void ResizeNotesByRatio_Shrink_Midi()
        {
            ResizeNotesByRatio(
                new[]
                {
                    NoteMethods.Create(0, 10),
                    NoteMethods.Create(10, 20),
                    NoteMethods.Create(100, 400)
                },
                new[]
                {
                    new TimeAndLength((MidiTimeSpan)0, (MidiTimeSpan)2),
                    new TimeAndLength((MidiTimeSpan)2, (MidiTimeSpan)4),
                    new TimeAndLength((MidiTimeSpan)20, (MidiTimeSpan)80)
                },
                0.2,
                TimeSpanType.Midi,
                TempoMap);
        }

        [Test]
        public void ResizeNotesByRatio_Shrink_Metric()
        {
            var tempoMap = TempoMap;

            ResizeNotesByRatio(
                new[]
                {
                    NoteMethods.Create(new MetricTimeSpan(0, 0, 0), new MetricTimeSpan(0, 0, 15), tempoMap),
                    NoteMethods.Create(new MetricTimeSpan(0, 0, 10), new MetricTimeSpan(0, 0, 1), tempoMap),
                    NoteMethods.Create(new MetricTimeSpan(0, 1, 0), new MetricTimeSpan(0, 1, 0), tempoMap)
                },
                new[]
                {
                    new TimeAndLength(new MetricTimeSpan(0, 0, 0), new MetricTimeSpan(0, 0, 3, 750)),
                    new TimeAndLength(new MetricTimeSpan(0, 0, 2, 500), new MetricTimeSpan(0, 0, 0, 250)),
                    new TimeAndLength(new MetricTimeSpan(0, 0, 15), new MetricTimeSpan(0, 0, 15))
                },
                0.25,
                TimeSpanType.Metric,
                TempoMap);
        }

        #endregion

        #region Private methods

        private static void ResizeNotes(IEnumerable<Note> notes,
                                        IEnumerable<TimeAndLength> expectedTimesAndLengths,
                                        ITimeSpan length,
                                        TimeSpanType lengthType,
                                        TempoMap tempoMap)
        {
            notes.ResizeNotes(length, lengthType, tempoMap);
            CheckNotes(notes, expectedTimesAndLengths, tempoMap);
        }

        private static void ResizeNotesByRatio(IEnumerable<Note> notes,
                                               IEnumerable<TimeAndLength> expectedTimesAndLengths,
                                               double ratio,
                                               TimeSpanType lengthType,
                                               TempoMap tempoMap)
        {
            notes.ResizeNotes(ratio, lengthType, tempoMap);
            CheckNotes(notes, expectedTimesAndLengths, tempoMap);
        }

        private static void CheckNotes(IEnumerable<Note> notes, IEnumerable<TimeAndLength> expectedTimesAndLengths, TempoMap tempoMap)
        {
            var notesTimesAndLengths = notes.Where(n => n != null)
                                                        .Zip(expectedTimesAndLengths, (n, tl) => new
                                                        {
                                                            Note = n,
                                                            TimeAndLength = tl
                                                        });

            foreach (var noteTimeAndLength in notesTimesAndLengths)
            {
                var note = noteTimeAndLength.Note;
                var expectedTime = noteTimeAndLength.TimeAndLength.Time;
                var expectedLength = noteTimeAndLength.TimeAndLength.Length;

                var convertedLength = LengthConverter.ConvertTo((MidiTimeSpan)note.Length,
                                                                expectedLength.GetType(),
                                                                note.Time,
                                                                tempoMap);
                Assert.AreEqual(expectedLength, convertedLength, "Length is invalid.");

                var convertedTime = TimeConverter.ConvertTo((MidiTimeSpan)note.Time,
                                                            expectedTime.GetType(),
                                                            tempoMap);
                Assert.AreEqual(expectedTime, convertedTime, "Time is invalid.");
            }
        }

        private static TempoMap GetTempoMap()
        {
            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(new MetricTimeSpan(0, 0, 1), Tempo.FromBeatsPerMinute(60));
                tempoMapManager.SetTempo(new MetricTimeSpan(0, 0, 10), Tempo.FromBeatsPerMinute(150));
                tempoMapManager.SetTempo(new MetricTimeSpan(0, 0, 50), Tempo.FromBeatsPerMinute(100));

                return tempoMapManager.TempoMap;
            }
        }

        #endregion
    }
}
