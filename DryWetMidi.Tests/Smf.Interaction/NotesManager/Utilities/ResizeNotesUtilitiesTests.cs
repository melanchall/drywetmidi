using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestFixture]
    public sealed class ResizeNotesUtilitiesTests
    {
        #region Constants

        private static readonly TempoMap TempoMap = GetTempoMap();

        #endregion

        #region Test methods

        [Test]
        [Description("Resize empty notes collection using absolute mode.")]
        public void ResizeNotes_Absolute_EmptyCollection()
        {
            ResizeNotes_Absolute(
                Enumerable.Empty<Note>(),
                Enumerable.Empty<ITimeSpan>(),
                new MetricTimeSpan(0, 2, 0),
                TimeSpanType.Metric,
                TempoMap);
        }

        [Test]
        [Description("Resize zero-length notes group using absolute mode.")]
        public void ResizeNotes_Absolute_ZeroLength()
        {
            ResizeNotes_Absolute(
                GetZeroLengthNotes(),
                new[]
                {
                    new MetricTimeSpan(0, 2, 0),
                    new MetricTimeSpan(0, 2, 0),
                    new MetricTimeSpan(0, 2, 0)
                },
                new MetricTimeSpan(0, 2, 0),
                TimeSpanType.Metric,
                TempoMap);
        }

        [Test]
        [Description("Stretch notes using absolute mode.")]
        public void ResizeNotes_Absolute_Stretch()
        {
            ResizeNotes_Absolute(
                GetNotes(),
                new[]
                {
                    new MetricTimeSpan(0, 1, 2),
                    new MetricTimeSpan(0, 1, 2),
                    new MetricTimeSpan(0, 1, 0),
                    new MetricTimeSpan(0, 2, 0),
                    new MetricTimeSpan(0, 1, 20)
                },
                new MetricTimeSpan(0, 2, 0),
                TimeSpanType.Metric,
                TempoMap);
        }

        [Test]
        [Description("Shrink notes using absolute mode.")]
        public void ResizeNotes_Absolute_Shrink()
        {
            ResizeNotes_Absolute(
                GetNotes(),
                new[]
                {
                    new MetricTimeSpan(0, 0, 0),
                    new MetricTimeSpan(0, 0, 0),
                    new MetricTimeSpan(0, 0, 0),
                    new MetricTimeSpan(0, 0, 50),
                    new MetricTimeSpan(0, 0, 10)
                },
                new MetricTimeSpan(0, 0, 50),
                TimeSpanType.Metric,
                TempoMap);
        }

        [Test]
        [Description("Resize empty notes collection using relative mode.")]
        public void ResizeNotes_Relative_EmptyCollection()
        {
            ResizeNotes_Relative(
                Enumerable.Empty<Note>(),
                Enumerable.Empty<ITimeSpan>(),
                new MetricTimeSpan(0, 2, 0),
                TimeSpanType.Metric,
                TempoMap);
        }

        [Test]
        [Description("Resize zero-length notes group using relative mode.")]
        public void ResizeNotes_Relative_ZeroLength()
        {
            ResizeNotes_Relative(
                GetZeroLengthNotes(),
                new[]
                {
                    new MetricTimeSpan(0, 2, 0),
                    new MetricTimeSpan(0, 2, 0),
                    new MetricTimeSpan(0, 2, 0)
                },
                new MetricTimeSpan(0, 2, 0),
                TimeSpanType.Metric,
                TempoMap);
        }

        [Test]
        [Description("Stretch notes using relative mode.")]
        public void ResizeNotes_Relative_Stretch()
        {
            ResizeNotes_Relative(
                GetNotes(),
                new[]
                {
                    new MetricTimeSpan(0, 0, 4),
                    new MetricTimeSpan(0, 0, 4),
                    new MetricTimeSpan(),
                    new MetricTimeSpan(0, 2, 0),
                    new MetricTimeSpan(0, 0, 40)
                },
                new MetricTimeSpan(0, 2, 0),
                TimeSpanType.Metric,
                TempoMap);
        }

        [Test]
        [Description("Shrink notes using absolute mode.")]
        public void ResizeNotes_Relative_Shrink()
        {
            ResizeNotes_Relative(
                GetNotes(),
                new[]
                {
                    new MetricTimeSpan(0, 0, 1),
                    new MetricTimeSpan(0, 0, 1),
                    new MetricTimeSpan(),
                    new MetricTimeSpan(0, 0, 30),
                    new MetricTimeSpan(0, 0, 10)
                },
                new MetricTimeSpan(0, 0, 30),
                TimeSpanType.Metric,
                TempoMap);
        }

        #endregion

        #region Private methods

        private static void ResizeNotes_Absolute(IEnumerable<Note> notes,
                                                 IEnumerable<ITimeSpan> expectedLengths,
                                                 ITimeSpan length,
                                                 TimeSpanType lengthType,
                                                 TempoMap tempoMap)
        {
            ResizeNotes(notes, expectedLengths, length, lengthType, ResizingMode.Absolute, tempoMap);
        }

        private static void ResizeNotes_Relative(IEnumerable<Note> notes,
                                                 IEnumerable<ITimeSpan> expectedLengths,
                                                 ITimeSpan length,
                                                 TimeSpanType lengthType,
                                                 TempoMap tempoMap)
        {
            ResizeNotes(notes, expectedLengths, length, lengthType, ResizingMode.Relative, tempoMap);
        }

        private static void ResizeNotes(IEnumerable<Note> notes,
                                        IEnumerable<ITimeSpan> expectedLengths,
                                        ITimeSpan length,
                                        TimeSpanType lengthType,
                                        ResizingMode mode,
                                        TempoMap tempoMap)
        {
            notes.ResizeNotes(length, lengthType, mode, tempoMap);

            foreach (var noteLength in notes.Zip(expectedLengths, (n, l) => new { Note = n, Length = l }))
            {
                var note = noteLength.Note;
                var expectedLength = noteLength.Length;

                var convertedLength = LengthConverter.ConvertTo((MidiTimeSpan)note.Length,
                                                                expectedLength.GetType(),
                                                                note.Time,
                                                                tempoMap);
                TimeSpanTestUtilities.AreEqual(expectedLength, convertedLength, "Length is invalid.");
            }
        }

        private static IEnumerable<Note> GetNotes()
        {
            var noteMethods = new NoteMethods();

            return new[]
            {
                noteMethods.Create(new MetricTimeSpan(0, 0, 2), new MetricTimeSpan(0, 0, 2), TempoMap),
                noteMethods.Create(new MetricTimeSpan(), new MetricTimeSpan(0, 0, 2), TempoMap),
                noteMethods.Create(new MetricTimeSpan(0, 0, 2), new MetricTimeSpan(), TempoMap),
                noteMethods.Create(new MetricTimeSpan(), new MetricTimeSpan(0, 1, 0), TempoMap),
                noteMethods.Create(new MetricTimeSpan(0, 0, 10), new MetricTimeSpan(0, 0, 20), TempoMap),
            };
        }

        private static IEnumerable<Note> GetZeroLengthNotes()
        {
            var noteMethods = new NoteMethods();

            return new[]
            {
                noteMethods.Create(new MetricTimeSpan(0, 0, 2), new MetricTimeSpan(), TempoMap),
                noteMethods.Create(new MetricTimeSpan(0, 0, 2), new MetricTimeSpan(), TempoMap),
                noteMethods.Create(new MetricTimeSpan(0, 0, 2), new MetricTimeSpan(), TempoMap)
            };
        }

        private static TempoMap GetTempoMap()
        {
            using (var tempoMapManager = new TempoMapManager())
            {
                tempoMapManager.SetTempo(new MetricTimeSpan(0, 0, 1), Tempo.FromBeatsPerMinute(70));
                tempoMapManager.SetTempo(new MetricTimeSpan(0, 0, 10), Tempo.FromBeatsPerMinute(110));
                tempoMapManager.SetTempo(new MetricTimeSpan(0, 0, 50), Tempo.FromBeatsPerMinute(90));

                return tempoMapManager.TempoMap;
            }
        }

        #endregion
    }
}
