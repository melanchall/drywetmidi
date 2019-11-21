using System;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Test methods

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackNotes_MoveForwardToNote(bool useOutputDevice)
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.FromSeconds(1);
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromMilliseconds(800);
            var noteOffVelocity = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1500);

            CheckTrackNotes(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), moveFrom),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOnDelay + noteOffDelay - moveTo)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                notesWillBeStarted: new[] { 0 },
                notesWillBeFinished: new[] { 0 },
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackNotes_MoveBackToNote(bool useOutputDevice)
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.Zero;
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromMilliseconds(800);
            var noteOffVelocity = (SevenBitNumber)80;
            var pitchBendDelay = TimeSpan.FromMilliseconds(300);

            var moveFrom = TimeSpan.FromSeconds(1);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackNotes(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOffDelay),
                    new EventToSend(new PitchBendEvent(), pitchBendDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOffDelay),
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), moveFrom - (noteOnDelay + noteOffDelay)),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOnDelay + noteOffDelay - moveTo),
                    new EventToSend(new PitchBendEvent(), pitchBendDelay)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                notesWillBeStarted: new[] { 0, 0 },
                notesWillBeFinished: new[] { 0, 0 },
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackNotes_MoveForwardFromNote(bool useOutputDevice)
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.Zero;
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromMilliseconds(800);
            var noteOffVelocity = (SevenBitNumber)80;
            var pitchBendDelay = TimeSpan.FromMilliseconds(400);

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1000);

            CheckTrackNotes(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOffDelay),
                    new EventToSend(new PitchBendEvent(), pitchBendDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), moveFrom - noteOnDelay),
                    new EventToSend(new PitchBendEvent(), noteOnDelay + noteOffDelay + pitchBendDelay - moveTo)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                notesWillBeStarted: new[] { 0 },
                notesWillBeFinished: new[] { 0 },
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackNotes_MoveBackFromNote(bool useOutputDevice)
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.FromSeconds(1);
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromMilliseconds(400);
            var noteOffVelocity = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(1200);
            var moveTo = TimeSpan.FromMilliseconds(700);

            CheckTrackNotes(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), moveFrom - noteOnDelay),
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay - moveTo),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOffDelay)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                notesWillBeStarted: new[] { 0, 0 },
                notesWillBeFinished: new[] { 0, 0 },
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackNotes_MoveForwardToSameNote(bool useOutputDevice)
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.Zero;
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromSeconds(1);
            var noteOffVelocity = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(200);
            var moveTo = TimeSpan.FromMilliseconds(700);

            CheckTrackNotes(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOnDelay + noteOffDelay - moveTo + moveFrom)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                notesWillBeStarted: new[] { 0 },
                notesWillBeFinished: new[] { 0 },
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackNotes_MoveBackToSameNote(bool useOutputDevice)
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.Zero;
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromSeconds(1);
            var noteOffVelocity = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(700);
            var moveTo = TimeSpan.FromMilliseconds(400);

            CheckTrackNotes(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new EventToSend(new NoteOffEvent(noteNumber, noteOffVelocity), noteOnDelay + noteOffDelay - moveTo + moveFrom)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                notesWillBeStarted: new[] { 0 },
                notesWillBeFinished: new[] { 0 },
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackNotes_MoveForwardFromNoteToNote(bool useOutputDevice)
        {
            var noteNumber1 = (SevenBitNumber)60;
            var noteOnDelay1 = TimeSpan.Zero;
            var noteOnVelocity1 = (SevenBitNumber)100;
            var noteOffDelay1 = TimeSpan.FromSeconds(1);
            var noteOffVelocity1 = (SevenBitNumber)80;

            var noteNumber2 = (SevenBitNumber)70;
            var noteOnDelay2 = TimeSpan.FromMilliseconds(200);
            var noteOnVelocity2 = (SevenBitNumber)95;
            var noteOffDelay2 = TimeSpan.FromMilliseconds(800);
            var noteOffVelocity2 = (SevenBitNumber)85;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1400);

            CheckTrackNotes(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber1, noteOnVelocity1), noteOnDelay1),
                    new EventToSend(new NoteOffEvent(noteNumber1, noteOffVelocity1), noteOffDelay1),
                    new EventToSend(new NoteOnEvent(noteNumber2, noteOnVelocity2), noteOnDelay2),
                    new EventToSend(new NoteOffEvent(noteNumber2, noteOffVelocity2), noteOffDelay2)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber1, noteOnVelocity1), noteOnDelay1),
                    new EventToSend(new NoteOffEvent(noteNumber1, noteOffVelocity1), moveFrom),
                    new EventToSend(new NoteOnEvent(noteNumber2, noteOnVelocity2), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(noteNumber2, noteOffVelocity2), noteOnDelay1 + noteOffDelay1 + noteOnDelay2 + noteOffDelay2 - moveTo)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                notesWillBeStarted: new[] { 0, 1 },
                notesWillBeFinished: new[] { 0, 1 },
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackNotes_MoveBackFromNoteToNote(bool useOutputDevice)
        {
            var noteNumber1 = (SevenBitNumber)60;
            var noteOnDelay1 = TimeSpan.Zero;
            var noteOnVelocity1 = (SevenBitNumber)100;
            var noteOffDelay1 = TimeSpan.FromSeconds(1);
            var noteOffVelocity1 = (SevenBitNumber)80;

            var noteNumber2 = (SevenBitNumber)70;
            var noteOnDelay2 = TimeSpan.FromMilliseconds(200);
            var noteOnVelocity2 = (SevenBitNumber)95;
            var noteOffDelay2 = TimeSpan.FromMilliseconds(800);
            var noteOffVelocity2 = (SevenBitNumber)85;

            var moveFrom = TimeSpan.FromMilliseconds(1400);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackNotes(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber1, noteOnVelocity1), noteOnDelay1),
                    new EventToSend(new NoteOffEvent(noteNumber1, noteOffVelocity1), noteOffDelay1),
                    new EventToSend(new NoteOnEvent(noteNumber2, noteOnVelocity2), noteOnDelay2),
                    new EventToSend(new NoteOffEvent(noteNumber2, noteOffVelocity2), noteOffDelay2)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOnEvent(noteNumber1, noteOnVelocity1), noteOnDelay1),
                    new EventToSend(new NoteOffEvent(noteNumber1, noteOffVelocity1), noteOffDelay1),
                    new EventToSend(new NoteOnEvent(noteNumber2, noteOnVelocity2), noteOnDelay2),
                    new EventToSend(new NoteOffEvent(noteNumber2, noteOffVelocity2), moveFrom - (noteOnDelay1 + noteOffDelay1 + noteOnDelay2)),
                    new EventToSend(new NoteOnEvent(noteNumber1, noteOnVelocity1), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(noteNumber1, noteOffVelocity1), noteOnDelay1 + noteOffDelay1 - moveTo),
                    new EventToSend(new NoteOnEvent(noteNumber2, noteOnVelocity2), noteOnDelay2),
                    new EventToSend(new NoteOffEvent(noteNumber2, noteOffVelocity2), noteOffDelay2)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                notesWillBeStarted: new[] { 0, 1, 0, 1 },
                notesWillBeFinished: new[] { 0, 1, 0, 1 },
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackNotes_StopStart_InterruptNotesOnStop()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.Zero;

            CheckPlaybackStop(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), noteOnDelay),
                    new EventToSend(new NoteOffEvent(), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOnEvent(), noteOnDelay),
                    new EventToSend(new NoteOffEvent(), stopAfter),
                    new EventToSend(new NoteOnEvent(), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), noteOnDelay + noteOffDelay - stopAfter)
                },
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: (context, playback) =>
                {
                    playback.InterruptNotesOnStop = true;
                    playback.TrackNotes = true;
                },
                afterStart: NoPlaybackAction,
                afterStop: NoPlaybackAction,
                afterResume: NoPlaybackAction);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackNotes_StopStart_DontInterruptNotesOnStop()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.Zero;

            CheckPlaybackStop(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOnEvent(), noteOnDelay),
                    new EventToSend(new NoteOffEvent(), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOnEvent(), noteOnDelay),
                    new EventToSend(new NoteOffEvent(), noteOffDelay)
                },
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: (context, playback) =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = true;
                },
                afterStart: NoPlaybackAction,
                afterStop: NoPlaybackAction,
                afterResume: NoPlaybackAction);
        }

        #endregion
    }
}
