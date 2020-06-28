using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    internal static class PatternTestUtilities
    {
        #region Constants

        public static readonly FourBitNumber Channel = (FourBitNumber)2;

        #endregion

        #region Methods

        public static MidiFile TestNotes(Pattern pattern, ICollection<NoteInfo> expectedNotesInfos, params Tuple<long, Tempo>[] tempoChanges)
        {
            TempoMap tempoMap;
            using (var tempoMapManager = new TempoMapManager())
            {
                foreach (var tempoChange in tempoChanges)
                {
                    tempoMapManager.SetTempo(tempoChange.Item1, tempoChange.Item2);
                }

                tempoMap = tempoMapManager.TempoMap;
            }

            var midiFile = pattern.ToFile(tempoMap, Channel);

            var expectedNotes = expectedNotesInfos.Select(i =>
            {
                var expectedTime = TimeConverter.ConvertFrom(i.Time ?? new MetricTimeSpan(), tempoMap);
                var expectedLength = LengthConverter.ConvertFrom(i.Length, expectedTime, tempoMap);

                return new DryWetMidi.Interaction.Note(i.NoteNumber, expectedLength, expectedTime)
                {
                    Velocity = i.Velocity,
                    Channel = Channel
                };
            })
            .OrderBy(n => n.Time)
            .ToArray();

            var actualNotes = midiFile.GetNotes().ToArray();
            Assert.AreEqual(expectedNotes.Length, actualNotes.Length, "Notes count is invalid.");

            var j = 0;
            foreach (var expectedActual in expectedNotes.Zip(actualNotes, (e, a) => new { Expected = e, Actual = a }))
            {
                var expectedNote = expectedActual.Expected;
                var actualNote = expectedActual.Actual;

                Assert.IsTrue(NoteEquality.AreEqual(expectedNote, actualNote), $"Note {j} is invalid. Expected: {expectedNote}; actual: {actualNote}.");
                j++;
            }

            return midiFile;
        }

        public static void TestTimedEvents(
            Pattern pattern,
            ICollection<TimedEventInfo> expectedTimedEventsInfos,
            params Tuple<long, Tempo>[] tempoChanges)
        {
            TempoMap tempoMap;
            using (var tempoMapManager = new TempoMapManager())
            {
                foreach (var tempoChange in tempoChanges)
                {
                    tempoMapManager.SetTempo(tempoChange.Item1, tempoChange.Item2);
                }

                tempoMap = tempoMapManager.TempoMap;
            }

            var midiFile = pattern.ToFile(tempoMap, Channel);

            var expectedTimedEvents = expectedTimedEventsInfos.Select(i =>
                new TimedEvent(i.Event,
                               TimeConverter.ConvertFrom(i.Time ?? new MidiTimeSpan(), tempoMap)));

            var actualTimedEvents = midiFile.GetTimedEvents();

            foreach (var expectedEvent in expectedTimedEvents)
            {
                Assert.IsTrue(actualTimedEvents.Any(actual => TimedEventEquality.AreEqual(expectedEvent, actual, false)),
                              $"There are no event: {expectedEvent}");
            }
        }

        public static void TestTimedEventsWithExactOrder(
            Pattern pattern,
            ICollection<TimedEventInfo> expectedTimedEventsInfos,
            params Tuple<long, Tempo>[] tempoChanges)
        {
            TempoMap tempoMap;
            using (var tempoMapManager = new TempoMapManager())
            {
                foreach (var tempoChange in tempoChanges)
                {
                    tempoMapManager.SetTempo(tempoChange.Item1, tempoChange.Item2);
                }

                tempoMap = tempoMapManager.TempoMap;
            }

            var midiFile = pattern.ToFile(tempoMap, Channel);

            var expectedTimedEvents = expectedTimedEventsInfos.Select(i =>
                new TimedEvent(i.Event, TimeConverter.ConvertFrom(i.Time ?? new MidiTimeSpan(), tempoMap)));

            var actualTimedEvents = midiFile.GetTimedEvents();

            Assert.IsTrue(TimedEventEquality.AreEqual(expectedTimedEvents, actualTimedEvents, false), "Events have invalid order.");
        }

        #endregion
    }
}
