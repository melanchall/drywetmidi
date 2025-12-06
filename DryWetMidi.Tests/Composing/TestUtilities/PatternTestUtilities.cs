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

                return new Note(i.NoteNumber, expectedLength, expectedTime)
                {
                    Velocity = i.Velocity,
                    Channel = Channel
                };
            })
            .OrderBy(n => n.Time)
            .ToArray();

            var actualNotes = midiFile.GetNotes().ToArray();
            MidiAsserts.AreEqual(expectedNotes, actualNotes, "Notes are invalid.");

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
                               TimeConverter.ConvertFrom(i.Time ?? new MidiTimeSpan(), tempoMap))).ToArray();

            var actualTimedEvents = midiFile.GetTimedEvents();

            MidiAsserts.AreEqual(
                expectedTimedEvents,
                actualTimedEvents.Where(e => expectedTimedEvents.Any(ee => ee.Event.EventType == e.Event.EventType)).ToArray(),
                false,
                0,
                "Events are invalid.");
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

            MidiAsserts.AreEqual(expectedTimedEvents, actualTimedEvents, false, 0, "Events have invalid order.");
        }

        #endregion
    }
}
