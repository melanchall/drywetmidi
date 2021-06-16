using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Devices
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Test methods

        [Retry(RetriesNumber)]
        [TestCase(true, 0)]
        [TestCase(true, 100)]
        [TestCase(false, 0)]
        [TestCase(false, 100)]
        public void TrackProgram_NoProgramChanges_MoveToTime(bool useOutputDevice, int moveFromMs)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var moveFrom = TimeSpan.FromMilliseconds(moveFromMs);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackProgram(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOffEvent(), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOffEvent(), noteOffDelay - (moveTo - moveFrom))
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackProgram_NoProgramChanges_MoveToStart(bool useOutputDevice)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckTrackProgram(
                eventsToSend: new[]
                {
                    new EventToSend(new NoteOffEvent(), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new NoteOffEvent(), noteOffDelay - (moveTo - moveFrom))
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackProgram_ProgramChangeAtZero_MoveToTime(bool useOutputDevice)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackProgram(
                eventsToSend: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), noteOffDelay - (moveTo - moveFrom))
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackProgram_ProgramChangeAtZero_MoveToStart(bool useOutputDevice)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckTrackProgram(
                eventsToSend: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber), TimeSpan.Zero),
                    new EventToSend(new ProgramChangeEvent(programNumber), moveFrom),
                    new EventToSend(new NoteOffEvent(), noteOffDelay - moveTo)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackProgram_FromBeforeProgramChange_ToBeforeProgramChange(bool useOutputDevice)
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackProgram(
                eventsToSend: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - programChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime - (moveTo - moveFrom)),
                    new EventToSend(new NoteOffEvent(), noteOffTime - programChangeTime)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackProgram_FromBeforeProgramChange_ToAfterProgramChange(bool useOutputDevice)
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1000);

            CheckTrackProgram(
                eventsToSend: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - programChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, moveFrom),
                    new EventToSend(new NoteOffEvent(), noteOffTime - (moveTo - moveFrom) - moveFrom)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackProgram_FromAfterProgramChange_ToAfterProgramChange(bool useOutputDevice)
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(1500);

            CheckTrackProgram(
                eventsToSend: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - programChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - programChangeTime - (moveTo - moveFrom))
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackProgram_FromAfterProgramChange_ToBeforeProgramChange(bool useOutputDevice)
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackProgram(
                eventsToSend: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - programChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new EventToSend(new ProgramChangeEvent(SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, moveFrom - programChangeTime),
                    new EventToSend(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime - moveTo),
                    new EventToSend(new NoteOffEvent(), noteOffTime - programChangeTime)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackProgram_FromBeforeProgramChange_ToProgramChange(bool useOutputDevice)
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckTrackProgram(
                eventsToSend: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - programChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, moveFrom),
                    new EventToSend(new NoteOffEvent(), noteOffTime - (moveTo - moveFrom) - moveFrom)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackProgram_FromAfterProgramChange_ToProgramChange(bool useOutputDevice)
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckTrackProgram(
                eventsToSend: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - programChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, programChangeTime),
                    new EventToSend(new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, moveFrom - programChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - programChangeTime)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        #endregion

        #region Private methods

        private void CheckTrackProgram(
            ICollection<EventToSend> eventsToSend,
            ICollection<EventToSend> eventsWillBeSent,
            TimeSpan moveFrom,
            TimeSpan moveTo,
            bool useOutputDevice) =>
            CheckDataTracking(
                p => p.TrackProgram = true,
                eventsToSend,
                eventsWillBeSent,
                moveFrom,
                moveTo,
                useOutputDevice);

        #endregion
    }
}
