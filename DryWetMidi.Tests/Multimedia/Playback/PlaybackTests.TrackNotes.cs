using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Test methods

        [Retry(RetriesNumber)]
        [Test]
        public void TrackNotes_MoveForwardToNote_1()
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.FromSeconds(1);
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromMilliseconds(800);
            var noteOffVelocity = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1500);

            CheckNotesTracking(
                initialTimedObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent(noteNumber, noteOnVelocity))
                        .SetTime((MetricTimeSpan)noteOnDelay, TempoMap),
                    new TimedEvent(new NoteOffEvent(noteNumber, noteOffVelocity))
                        .SetTime((MetricTimeSpan)(noteOnDelay + noteOffDelay), TempoMap),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(noteNumber, noteOnVelocity), moveFrom),
                    new SentReceivedEvent(new NoteOffEvent(noteNumber, noteOffVelocity), noteOnDelay + noteOffDelay - moveTo + moveFrom)
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                notesWillBeStarted: new[] { 0 },
                notesWillBeFinished: new[] { 0 });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackNotes_MoveForwardToNote_2()
        {
            //  1         2
            // -|--AAAAAAA|AAAAAAA
            // -|---BBB---|-------
            // -|----CCC--|-EEE---
            // -|-----DDD-|--FFF--

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(1000);

            CheckNotesTracking(
                initialTimedObjects: new[]
                {
                    new Note((SevenBitNumber)100)
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap)
                        .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(1400), TempoMap),
                    new Note((SevenBitNumber)90)
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap)
                        .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap),
                    new Note((SevenBitNumber)80)
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap)
                        .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap),
                    new Note((SevenBitNumber)70)
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(600), TempoMap)
                        .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap),
                    new Note((SevenBitNumber)80)
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1100), TempoMap)
                        .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap),
                    new Note((SevenBitNumber)70)
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1200), TempoMap)
                        .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)100, Note.DefaultVelocity), moveFrom),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), moveFrom + TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), moveFrom + TimeSpan.FromMilliseconds(200)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), moveFrom + TimeSpan.FromMilliseconds(400)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), moveFrom + TimeSpan.FromMilliseconds(500)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)100, Note.DefaultOffVelocity), moveFrom + TimeSpan.FromMilliseconds(700)),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                notesWillBeStarted: new[] { 0, 4, 5 },
                notesWillBeFinished: new[] { 4, 5, 0 });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackNotes_MoveForwardToNote_3()
        {
            //  1        2
            // -|--AAAAAA|AAAAAA
            // -|---BBB--|------
            // -|----CCC-|EEE---
            // -|-----DDD|-FFF--

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(900);

            CheckNotesTracking(
                initialTimedObjects: new[]
                {
                    new Note((SevenBitNumber)100)
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap)
                        .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(1200), TempoMap),
                    new Note((SevenBitNumber)90)
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap)
                        .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap),
                    new Note((SevenBitNumber)80)
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap)
                        .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap),
                    new Note((SevenBitNumber)70)
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(600), TempoMap)
                        .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap),
                    new Note((SevenBitNumber)80)
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(900), TempoMap)
                        .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap),
                    new Note((SevenBitNumber)70)
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1000), TempoMap)
                        .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)100, Note.DefaultVelocity), moveFrom),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), moveFrom + TimeSpan.FromMilliseconds(0)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), moveFrom + TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), moveFrom + TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), moveFrom + TimeSpan.FromMilliseconds(400)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)100, Note.DefaultOffVelocity), moveFrom + TimeSpan.FromMilliseconds(600)),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                notesWillBeStarted: new[] { 0, 4, 5 },
                notesWillBeFinished: new[] { 4, 5, 0 });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackNotes_MoveForwardToNote_SendNoteOffEventsForNonActiveNotes()
        {
            //  1        2
            // -|--AAAAAA|AAAAAA
            // -|---BBB--|------
            // -|----CCC-|EEE---
            // -|-----DDD|-FFF--

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(900);

            CheckNotesTracking(
                initialTimedObjects: new[]
                {
                    new Note((SevenBitNumber)100)
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap)
                        .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(1200), TempoMap),
                    new Note((SevenBitNumber)90)
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(400), TempoMap)
                        .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap),
                    new Note((SevenBitNumber)80)
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap)
                        .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap),
                    new Note((SevenBitNumber)70)
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(600), TempoMap)
                        .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap),
                    new Note((SevenBitNumber)80)
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(900), TempoMap)
                        .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap),
                    new Note((SevenBitNumber)70)
                        .SetTime((MetricTimeSpan)TimeSpan.FromMilliseconds(1000), TempoMap)
                        .SetLength((MetricTimeSpan)TimeSpan.FromMilliseconds(300), TempoMap),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)100, Note.DefaultVelocity), moveFrom),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), moveFrom + TimeSpan.FromMilliseconds(0)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity), moveFrom + TimeSpan.FromMilliseconds(0)),
                    new SentReceivedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), moveFrom + TimeSpan.FromMilliseconds(100)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity), moveFrom + TimeSpan.FromMilliseconds(300)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), moveFrom + TimeSpan.FromMilliseconds(400)),
                    new SentReceivedEvent(new NoteOffEvent((SevenBitNumber)100, Note.DefaultOffVelocity), moveFrom + TimeSpan.FromMilliseconds(600)),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                notesWillBeStarted: new[] { 0, 4, 5 },
                notesWillBeFinished: new[] { 3, 4, 5, 0 },
                setupPlayback: playback => playback.SendNoteOffEventsForNonActiveNotes = true);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackNotes_MoveBackToNote()
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.Zero;
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromMilliseconds(800);
            var noteOffVelocity = (SevenBitNumber)80;
            var pitchBendDelay = TimeSpan.FromMilliseconds(300);

            var moveFrom = TimeSpan.FromSeconds(1);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckNotesTracking(
                initialTimedObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent(noteNumber, noteOnVelocity))
                        .SetTime((MetricTimeSpan)noteOnDelay, TempoMap),
                    new TimedEvent(new NoteOffEvent(noteNumber, noteOffVelocity))
                        .SetTime((MetricTimeSpan)(noteOnDelay + noteOffDelay), TempoMap),
                    new TimedEvent(new PitchBendEvent())
                        .SetTime((MetricTimeSpan)(noteOnDelay + noteOffDelay + pitchBendDelay), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new SentReceivedEvent(new NoteOffEvent(noteNumber, noteOffVelocity), noteOnDelay + noteOffDelay),
                    new SentReceivedEvent(new NoteOnEvent(noteNumber, noteOnVelocity), moveFrom),
                    new SentReceivedEvent(new NoteOffEvent(noteNumber, noteOffVelocity), noteOnDelay + noteOffDelay - moveTo + moveFrom),
                    new SentReceivedEvent(new PitchBendEvent(), noteOnDelay + noteOffDelay - moveTo + moveFrom + pitchBendDelay)
                },
                notesWillBeStarted: new[] { 0, 0 },
                notesWillBeFinished: new[] { 0, 0 });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackNotes_MoveForwardFromNote()
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.Zero;
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromMilliseconds(800);
            var noteOffVelocity = (SevenBitNumber)80;
            var pitchBendDelay = TimeSpan.FromMilliseconds(400);

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1000);

            CheckNotesTracking(
                initialTimedObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent(noteNumber, noteOnVelocity))
                        .SetTime((MetricTimeSpan)noteOnDelay, TempoMap),
                    new TimedEvent(new NoteOffEvent(noteNumber, noteOffVelocity))
                        .SetTime((MetricTimeSpan)(noteOnDelay + noteOffDelay), TempoMap),
                    new TimedEvent(new PitchBendEvent())
                        .SetTime((MetricTimeSpan)(noteOnDelay + noteOffDelay + pitchBendDelay), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new SentReceivedEvent(new NoteOffEvent(noteNumber, noteOffVelocity), moveFrom),
                    new SentReceivedEvent(new PitchBendEvent(), noteOnDelay + noteOffDelay + pitchBendDelay - moveTo + moveFrom)
                },
                notesWillBeStarted: new[] { 0 },
                notesWillBeFinished: new[] { 0 });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackNotes_MoveBackFromNote()
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.FromSeconds(1);
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromMilliseconds(400);
            var noteOffVelocity = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(1200);
            var moveTo = TimeSpan.FromMilliseconds(700);

            CheckNotesTracking(
                initialTimedObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent(noteNumber, noteOnVelocity))
                        .SetTime((MetricTimeSpan)noteOnDelay, TempoMap),
                    new TimedEvent(new NoteOffEvent(noteNumber, noteOffVelocity))
                        .SetTime((MetricTimeSpan)(noteOnDelay + noteOffDelay), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new SentReceivedEvent(new NoteOffEvent(noteNumber, noteOffVelocity), moveFrom),
                    new SentReceivedEvent(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay - moveTo + moveFrom),
                    new SentReceivedEvent(new NoteOffEvent(noteNumber, noteOffVelocity), noteOffDelay + noteOnDelay - moveTo + moveFrom)
                },
                notesWillBeStarted: new[] { 0, 0 },
                notesWillBeFinished: new[] { 0, 0 });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackNotes_MoveForwardToSameNote()
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.Zero;
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromSeconds(1);
            var noteOffVelocity = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(200);
            var moveTo = TimeSpan.FromMilliseconds(700);

            CheckNotesTracking(
                initialTimedObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent(noteNumber, noteOnVelocity))
                        .SetTime((MetricTimeSpan)noteOnDelay, TempoMap),
                    new TimedEvent(new NoteOffEvent(noteNumber, noteOffVelocity))
                        .SetTime((MetricTimeSpan)(noteOnDelay + noteOffDelay), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new SentReceivedEvent(new NoteOffEvent(noteNumber, noteOffVelocity), noteOnDelay + noteOnDelay + noteOffDelay - moveTo + moveFrom)
                },
                notesWillBeStarted: new[] { 0 },
                notesWillBeFinished: new[] { 0 });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackNotes_MoveBackToSameNote()
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.Zero;
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromSeconds(1);
            var noteOffVelocity = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(700);
            var moveTo = TimeSpan.FromMilliseconds(400);

            CheckNotesTracking(
                initialTimedObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent(noteNumber, noteOnVelocity))
                        .SetTime((MetricTimeSpan)noteOnDelay, TempoMap),
                    new TimedEvent(new NoteOffEvent(noteNumber, noteOffVelocity))
                        .SetTime((MetricTimeSpan)(noteOnDelay + noteOffDelay), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(noteNumber, noteOnVelocity), noteOnDelay),
                    new SentReceivedEvent(new NoteOffEvent(noteNumber, noteOffVelocity), noteOnDelay + noteOnDelay + noteOffDelay - moveTo + moveFrom)
                },
                notesWillBeStarted: new[] { 0 },
                notesWillBeFinished: new[] { 0 });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackNotes_MoveForwardFromNoteToNote()
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

            CheckNotesTracking(
                initialTimedObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent(noteNumber1, noteOnVelocity1))
                        .SetTime((MetricTimeSpan)noteOnDelay1, TempoMap),
                    new TimedEvent(new NoteOffEvent(noteNumber1, noteOffVelocity1))
                        .SetTime((MetricTimeSpan)(noteOnDelay1 + noteOffDelay1), TempoMap),
                    new TimedEvent(new NoteOnEvent(noteNumber2, noteOnVelocity2))
                        .SetTime((MetricTimeSpan)(noteOnDelay1 + noteOffDelay1 + noteOnDelay2), TempoMap),
                    new TimedEvent(new NoteOffEvent(noteNumber2, noteOffVelocity2))
                        .SetTime((MetricTimeSpan)(noteOnDelay1 + noteOffDelay1 + noteOnDelay2 + noteOffDelay2), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(noteNumber1, noteOnVelocity1), noteOnDelay1),
                    new SentReceivedEvent(new NoteOffEvent(noteNumber1, noteOffVelocity1), noteOnDelay1 + moveFrom),
                    new SentReceivedEvent(new NoteOnEvent(noteNumber2, noteOnVelocity2), noteOnDelay1 + moveFrom),
                    new SentReceivedEvent(new NoteOffEvent(noteNumber2, noteOffVelocity2), noteOnDelay1 + moveFrom + noteOnDelay1 + noteOffDelay1 + noteOnDelay2 + noteOffDelay2 - moveTo)
                },
                notesWillBeStarted: new[] { 0, 1 },
                notesWillBeFinished: new[] { 0, 1 });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackNotes_MoveBackFromNoteToNote()
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

            CheckNotesTracking(
                initialTimedObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent(noteNumber1, noteOnVelocity1))
                        .SetTime((MetricTimeSpan)noteOnDelay1, TempoMap),
                    new TimedEvent(new NoteOffEvent(noteNumber1, noteOffVelocity1))
                        .SetTime((MetricTimeSpan)(noteOnDelay1 + noteOffDelay1), TempoMap),
                    new TimedEvent(new NoteOnEvent(noteNumber2, noteOnVelocity2))
                        .SetTime((MetricTimeSpan)(noteOnDelay1 + noteOffDelay1 + noteOnDelay2), TempoMap),
                    new TimedEvent(new NoteOffEvent(noteNumber2, noteOffVelocity2))
                        .SetTime((MetricTimeSpan)(noteOnDelay1 + noteOffDelay1 + noteOnDelay2 + noteOffDelay2), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(moveFrom, p => p.MoveToTime((MetricTimeSpan)moveTo)),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(noteNumber1, noteOnVelocity1), noteOnDelay1),
                    new SentReceivedEvent(new NoteOffEvent(noteNumber1, noteOffVelocity1), noteOnDelay1 + noteOffDelay1),
                    new SentReceivedEvent(new NoteOnEvent(noteNumber2, noteOnVelocity2), noteOnDelay1 + noteOffDelay1 + noteOnDelay2),
                    new SentReceivedEvent(new NoteOffEvent(noteNumber2, noteOffVelocity2), moveFrom),
                    new SentReceivedEvent(new NoteOnEvent(noteNumber1, noteOnVelocity1), moveFrom),
                    new SentReceivedEvent(new NoteOffEvent(noteNumber1, noteOffVelocity1), noteOnDelay1 + noteOffDelay1 - moveTo + moveFrom),
                    new SentReceivedEvent(new NoteOnEvent(noteNumber2, noteOnVelocity2), noteOnDelay1 + noteOffDelay1 - moveTo + moveFrom + noteOnDelay2),
                    new SentReceivedEvent(new NoteOffEvent(noteNumber2, noteOffVelocity2), noteOnDelay1 + noteOffDelay1 - moveTo + moveFrom + noteOnDelay2 + noteOffDelay2)
                },
                notesWillBeStarted: new[] { 0, 1, 0, 1 },
                notesWillBeFinished: new[] { 0, 1, 0, 1 });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackNotes_StopStart_InterruptNotesOnStop()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.Zero;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent())
                        .SetTime((MetricTimeSpan)noteOnDelay, TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)(noteOnDelay + noteOffDelay), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p => p.Stop()),
                    new PlaybackAction(stopPeriod, p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(), noteOnDelay),
                    new SentReceivedEvent(new NoteOffEvent(), noteOnDelay + stopAfter),
                    new SentReceivedEvent(new NoteOnEvent(), noteOnDelay + stopAfter),
                    new SentReceivedEvent(new NoteOffEvent(), noteOnDelay + noteOffDelay + noteOnDelay)
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = true;
                    playback.TrackNotes = true;
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void TrackNotes_StopStart_DontInterruptNotesOnStop()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.Zero;

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: new[]
                {
                    new TimedEvent(new NoteOnEvent())
                        .SetTime((MetricTimeSpan)noteOnDelay, TempoMap),
                    new TimedEvent(new NoteOffEvent())
                        .SetTime((MetricTimeSpan)(noteOnDelay + noteOffDelay), TempoMap),
                },
                actions: new[]
                {
                    new PlaybackAction(stopAfter, p => p.Stop()),
                    new PlaybackAction(stopPeriod, p => p.Start()),
                },
                expectedReceivedEvents: new[]
                {
                    new SentReceivedEvent(new NoteOnEvent(), noteOnDelay),
                    new SentReceivedEvent(new NoteOffEvent(), noteOnDelay + noteOffDelay),
                },
                setupPlayback: playback =>
                {
                    playback.InterruptNotesOnStop = false;
                    playback.TrackNotes = true;
                });
        }

        #endregion

        #region Private methods

        private void CheckNotesTracking(
            ITimedObject[] initialTimedObjects,
            PlaybackAction[] actions,
            ICollection<SentReceivedEvent> expectedReceivedEvents,
            IEnumerable<int> notesWillBeStarted,
            IEnumerable<int> notesWillBeFinished,
            Action<Playback> setupPlayback = null)
        {
            var notes = initialTimedObjects.GetObjects(ObjectType.Note).Cast<Note>().ToArray();
            var notesStarted = new List<Note>();
            var notesFinished = new List<Note>();

            CheckPlayback(
                useOutputDevice: false,
                initialPlaybackObjects: initialTimedObjects,
                actions: actions,
                expectedReceivedEvents: expectedReceivedEvents,
                setupPlayback: playback =>
                {
                    setupPlayback?.Invoke(playback);

                    playback.TrackNotes = true;
                    playback.NotesPlaybackStarted += (_, e) => notesStarted.AddRange(e.Notes);
                    playback.NotesPlaybackFinished += (_, e) => notesFinished.AddRange(e.Notes);
                });

            MidiAsserts.AreEqual(notesStarted, notesWillBeStarted.Select(i => notes[i]), "Invalid notes started.");
            MidiAsserts.AreEqual(notesFinished, notesWillBeFinished.Select(i => notes[i]), "Invalid notes finished.");
        }

        #endregion
    }
}
