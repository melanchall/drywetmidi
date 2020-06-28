using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Standards;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    public sealed class PlaybackUtilitiesTests
    {
        #region Constants

        private const int RetriesNumber = 3;

        #endregion

        #region Nested classes

        private sealed class ReceivedNote
        {
            #region Constructor

            public ReceivedNote(Note note, TimeSpan time)
            {
                Note = note;
                Time = time;
            }

            #endregion

            #region Properties

            public Note Note { get; }

            public TimeSpan Time { get; }

            #endregion
        }

        #endregion

        #region Test methods

        [Retry(RetriesNumber)]
        [Test]
        public void CheckNotesPlayback_ProgramNumber()
        {
            var programNumber = (SevenBitNumber)100;
            CheckNotesPlayback(
                (notes, tempoMap, outputDevice) => notes.GetPlayback(tempoMap, outputDevice, programNumber),
                channel => new[] { new ProgramChangeEvent(programNumber) { Channel = channel } });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckNotesPlayback_GeneralMidiProgram()
        {
            var generalMidiProgram = GeneralMidiProgram.Agogo;
            CheckNotesPlayback(
                (notes, tempoMap, outputDevice) => notes.GetPlayback(tempoMap, outputDevice, generalMidiProgram),
                channel => new[] { generalMidiProgram.GetProgramEvent(channel) });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckNotesPlayback_GeneralMidi2Program()
        {
            var generalMidi2Program = GeneralMidi2Program.AnalogSynthBrass2;
            CheckNotesPlayback(
                (notes, tempoMap, outputDevice) => notes.GetPlayback(tempoMap, outputDevice, generalMidi2Program),
                channel => generalMidi2Program.GetProgramEvents(channel));
        }

        #endregion

        #region Methods

        private static void CheckNotesPlayback(Func<IEnumerable<Note>, TempoMap, OutputDevice, Playback> playbackGetter,
                                               Func<FourBitNumber, IEnumerable<MidiEvent>> programEventsGetter)
        {
            var tempoMap = TempoMap.Default;
            var channel1 = (FourBitNumber)5;
            var channel2 = (FourBitNumber)7;

            var notes1 = new PatternBuilder()
                .Note(DryWetMidi.MusicTheory.NoteName.A, new MetricTimeSpan(0, 0, 5))
                .MoveToTime(new MetricTimeSpan(0, 0, 1))
                .Note(DryWetMidi.MusicTheory.NoteName.B, new MetricTimeSpan(0, 0, 1))
                .Note(DryWetMidi.MusicTheory.NoteName.C, new MetricTimeSpan(0, 0, 1))
                .Build()
                .ToTrackChunk(tempoMap, channel1)
                .GetNotes();
            var notes2 = new PatternBuilder()
                .StepForward(new MetricTimeSpan(0, 0, 2))
                .Note(DryWetMidi.MusicTheory.NoteName.D, new MetricTimeSpan(0, 0, 5))
                .Build()
                .ToTrackChunk(tempoMap, channel2)
                .GetNotes();
            var notes = notes1.Concat(notes2).ToList();

            CheckNotesPlayback(notes, (m, d) => playbackGetter(notes, m, d), programEventsGetter);
        }

        private static void CheckNotesPlayback(IEnumerable<Note> notes,
                                               Func<TempoMap, OutputDevice, Playback> playbackGetter,
                                               Func<FourBitNumber, IEnumerable<MidiEvent>> programEventsGetter)
        {
            var tempoMap = TempoMap.Default;
            var stopwatch = new Stopwatch();

            var receivedNotesStarted = new List<ReceivedNote>();
            var receivedNotesFinished = new List<ReceivedNote>();

            var expectedReceivedNotesStarted = notes.OrderBy(n => n.Time)
                                                    .Select(n => new ReceivedNote(n, n.TimeAs<MetricTimeSpan>(tempoMap)))
                                                    .ToList();
            var expectedReceivedNotesFinished = notes.OrderBy(n => n.Time + n.Length)
                                                     .Select(n => new ReceivedNote(n, n.TimeAs<MetricTimeSpan>(tempoMap) + n.LengthAs<MetricTimeSpan>(tempoMap)))
                                                     .ToList();

            var sentEvents = new List<SentEvent>();

            using (var outputDevice = OutputDevice.GetByName(SendReceiveUtilities.DeviceToTestOnName))
            {
                outputDevice.EventSent += (_, e) => sentEvents.Add(new SentEvent(e.Event, stopwatch.Elapsed));

                using (var playback = playbackGetter(tempoMap, outputDevice))
                {
                    playback.NotesPlaybackStarted += (_, e) => receivedNotesStarted.AddRange(e.Notes.Select(n => new ReceivedNote(n, stopwatch.Elapsed)));
                    playback.NotesPlaybackFinished += (_, e) => receivedNotesFinished.AddRange(e.Notes.Select(n => new ReceivedNote(n, stopwatch.Elapsed)));

                    stopwatch.Start();
                    playback.Play();

                    Assert.IsFalse(playback.IsRunning, "Playback is running after completed.");
                }
            }

            CompareReceivedNotes(receivedNotesStarted, expectedReceivedNotesStarted);
            CompareReceivedNotes(receivedNotesFinished, expectedReceivedNotesFinished);

            var expectedProgramEvents = notes.Select(n => n.Channel).Distinct().SelectMany(c => programEventsGetter(c)).ToList();

            CheckProgramEvents(sentEvents, expectedProgramEvents);
        }

        private static void CompareReceivedNotes(IReadOnlyList<ReceivedNote> receivedNotes, IReadOnlyList<ReceivedNote> expectedReceivedNotes)
        {
            Assert.AreEqual(expectedReceivedNotes.Count, receivedNotes.Count, "Count of received notes is invalid.");

            for (var i = 0; i < receivedNotes.Count; i++)
            {
                var receivedNote = receivedNotes[i];
                var expectedReceivedNote = expectedReceivedNotes[i];

                Assert.AreSame(expectedReceivedNote.Note, receivedNote.Note, $"Received note {receivedNote.Note} is not {expectedReceivedNote.Note}.");

                var offsetFromExpectedTime = (receivedNote.Time - expectedReceivedNote.Time).Duration();
                Assert.LessOrEqual(
                    offsetFromExpectedTime,
                    SendReceiveUtilities.MaximumEventSendReceiveDelay,
                    $"Note was received at wrong time (at {receivedNote.Time} instead of {expectedReceivedNote.Time}).");
            }
        }

        private static void CheckProgramEvents(IReadOnlyList<SentEvent> sentEvents, IReadOnlyList<MidiEvent> expectedProgramEvents)
        {
            foreach (var programEvent in expectedProgramEvents)
            {
                string message;
                var sentEvent = sentEvents.FirstOrDefault(e => MidiEvent.Equals(e.Event, programEvent, new MidiEventEqualityCheckSettings { CompareDeltaTimes = false }, out message));
                Assert.IsNotNull(sentEvent, $"Program event {programEvent} was not sent.");

                Assert.LessOrEqual(
                    sentEvent.Time,
                    SendReceiveUtilities.MaximumEventSendReceiveDelay,
                    $"Program event was sent at wrong time (at {sentEvent.Time} instead of zero).");
            }
        }

        #endregion
    }
}
