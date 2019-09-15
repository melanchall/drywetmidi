using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Smf.Interaction
{
    [TestFixture]
    public sealed partial class PatternTests
    {
        #region Nested classes

        private sealed class NoteInfo
        {
            #region Constructor

            public NoteInfo(NoteName noteName, int octave, ITimeSpan time, ITimeSpan length)
                : this(noteName, octave, time, length, DryWetMidi.Smf.Interaction.Note.DefaultVelocity)
            {
            }

            public NoteInfo(NoteName noteName, int octave, ITimeSpan time, ITimeSpan length, SevenBitNumber velocity)
                : this(NoteUtilities.GetNoteNumber(noteName, octave), time, length, velocity)
            {
            }

            public NoteInfo(SevenBitNumber noteNumber, ITimeSpan time, ITimeSpan length, SevenBitNumber velocity)
            {
                NoteNumber = noteNumber;
                Time = time;
                Length = length;
                Velocity = velocity;
            }

            #endregion

            #region Properties

            public SevenBitNumber NoteNumber { get; }

            public ITimeSpan Time { get; }

            public ITimeSpan Length { get; }

            public SevenBitNumber Velocity { get; }

            #endregion
        }

        private sealed class TimedEventInfo
        {
            #region Constructor

            public TimedEventInfo(MidiEvent midiEvent, ITimeSpan time)
            {
                Event = midiEvent;
                Time = time;
            }

            #endregion

            #region Properties

            public MidiEvent Event { get; }

            public ITimeSpan Time { get; }

            #endregion
        }

        #endregion

        #region Constants

        private static readonly FourBitNumber Channel = (FourBitNumber)2;

        #endregion

        #region Private methods

        private static MidiFile TestNotes(Pattern pattern, ICollection<NoteInfo> expectedNotesInfos, params Tuple<long, Tempo>[] tempoChanges)
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

                return new DryWetMidi.Smf.Interaction.Note(i.NoteNumber, expectedLength, expectedTime)
                {
                    Velocity = i.Velocity,
                    Channel = Channel
                };
            })
            .OrderBy(n => n.Time)
            .ToArray();

            var actualNotes = midiFile.GetNotes();
            Assert.IsTrue(NoteEquality.AreEqual(expectedNotes, actualNotes), "Notes are invalid.");

            return midiFile;
        }

        private static void TestTimedEvents(
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

        private static void TestTimedEventsWithExactOrder(
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
