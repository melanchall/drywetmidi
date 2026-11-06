using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using Notes = Melanchall.DryWetMidi.MusicTheory.Notes;
using NoteName = Melanchall.DryWetMidi.MusicTheory.NoteName;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    internal static class PatternTestUtilities
    {
        #region Constants

        public static readonly FourBitNumber Channel = (FourBitNumber)2;

        #endregion

        #region Methods

        public static Pattern CreatePatternWithNoteActionState(PatternActionState state)
        {
            var pattern = new PatternBuilder()
                .Pattern(new PatternBuilder()
                    .Note(Notes.A3)
                    .Note(Notes.C4)
                    .Build())
                .Build();

            pattern.SetNotesState((index, descriptor) => index == 0, state);

            return pattern;
        }

        public static Pattern CreatePatternWithChordActionState(PatternActionState state)
        {
            var pattern = new PatternBuilder()
                .Pattern(new PatternBuilder()
                    .Chord(new[] { Notes.A3, Notes.D4 })
                    .Note(Notes.C4)
                    .Build())
                .Build();

            pattern.SetChordsState((index, descriptor) => index == 0, state);

            return pattern;
        }

        public static void TestActionStateNotes(Pattern pattern, PatternActionState state, int repetitions, bool isChord)
        {
            var expectedNotes = new List<NoteInfo>();
            var currentTime = 0;

            for (var repetitionIndex = 0; repetitionIndex < repetitions; repetitionIndex++)
            {
                if (state == PatternActionState.Enabled)
                {
                    expectedNotes.Add(new NoteInfo(
                        NoteName.A,
                        3,
                        GetTime(currentTime),
                        PatternBuilder.DefaultNoteLength,
                        PatternBuilder.DefaultVelocity));

                    if (isChord)
                    {
                        expectedNotes.Add(new NoteInfo(
                            NoteName.D,
                            4,
                            GetTime(currentTime),
                            PatternBuilder.DefaultNoteLength,
                            PatternBuilder.DefaultVelocity));
                    }
                }

                if (state != PatternActionState.Excluded)
                {
                    currentTime++;
                }

                expectedNotes.Add(new NoteInfo(
                    NoteName.C,
                    4,
                    GetTime(currentTime),
                    PatternBuilder.DefaultNoteLength,
                    PatternBuilder.DefaultVelocity));
                currentTime++;
            }

            TestNotes(pattern, expectedNotes);
        }

        public static MidiFile TestNotes(Pattern pattern, ICollection<NoteInfo> expectedNotesInfos, params (long Time, Tempo Tempo)[] tempoChanges)
        {
            TempoMap tempoMap;
            using (var tempoMapManager = new TempoMapManager())
            {
                foreach (var tempoChange in tempoChanges)
                {
                    tempoMapManager.SetTempo(tempoChange.Time, tempoChange.Tempo);
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
            params (long Time, Tempo Tempo)[] tempoChanges)
        {
            TempoMap tempoMap;
            using (var tempoMapManager = new TempoMapManager())
            {
                foreach (var tempoChange in tempoChanges)
                {
                    tempoMapManager.SetTempo(tempoChange.Time, tempoChange.Tempo);
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
            params (long Time, Tempo Tempo)[] tempoChanges)
        {
            TempoMap tempoMap;
            using (var tempoMapManager = new TempoMapManager())
            {
                foreach (var tempoChange in tempoChanges)
                {
                    tempoMapManager.SetTempo(tempoChange.Time, tempoChange.Tempo);
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

        #region Private methods

        private static ITimeSpan GetTime(int quarterNotes)
        {
            return quarterNotes == 0
                ? null
                : PatternBuilder.DefaultNoteLength.Multiply(quarterNotes);
        }

        #endregion
    }
}
