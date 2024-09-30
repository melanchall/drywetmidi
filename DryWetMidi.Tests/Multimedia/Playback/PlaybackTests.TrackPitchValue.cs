using System;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    // TODO: check tracking disabled
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Test methods

        [Retry(RetriesNumber)]
        [TestCase(true, 0)]
        [TestCase(true, 100)]
        [TestCase(false, 0)]
        [TestCase(false, 100)]
        public void TrackPitchValue_NoPitchBend_MoveToTime(bool useOutputDevice, int moveFromMs)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var moveFrom = TimeSpan.FromMilliseconds(moveFromMs);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackPitchValue(
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
        public void TrackPitchValue_NoPitchBend_MoveToStart(bool useOutputDevice)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckTrackPitchValue(
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
        public void TrackPitchValue_PitchBendAtZero_MoveToTime(bool useOutputDevice)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackPitchValue(
                eventsToSend: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), noteOffDelay - (moveTo - moveFrom))
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackPitchValue_PitchBendAtZero_MoveToStart(bool useOutputDevice)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckTrackPitchValue(
                eventsToSend: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue), TimeSpan.Zero),
                    new EventToSend(new PitchBendEvent(pitchValue), moveFrom),
                    new EventToSend(new NoteOffEvent(), noteOffDelay - moveTo)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackPitchValue_FromBeforePitchBend_ToBeforePitchBend(bool useOutputDevice)
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackPitchValue(
                eventsToSend: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - pitchBendTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime - (moveTo - moveFrom)),
                    new EventToSend(new NoteOffEvent(), noteOffTime - pitchBendTime)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackPitchValue_FromBeforePitchBend_ToAfterPitchBend(bool useOutputDevice)
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1000);

            CheckTrackPitchValue(
                eventsToSend: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - pitchBendTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new EventToSend(new NoteOffEvent(), noteOffTime - (moveTo - moveFrom) - moveFrom)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackPitchValue_FromAfterPitchBend_ToAfterPitchBend(bool useOutputDevice)
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(1500);

            CheckTrackPitchValue(
                eventsToSend: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - pitchBendTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - pitchBendTime - (moveTo - moveFrom))
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackPitchValue_FromAfterPitchBend_ToBeforePitchBend(bool useOutputDevice)
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackPitchValue(
                eventsToSend: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - pitchBendTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new EventToSend(new PitchBendEvent(SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, moveFrom - pitchBendTime),
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime - moveTo),
                    new EventToSend(new NoteOffEvent(), noteOffTime - pitchBendTime)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackPitchValue_FromBeforePitchBend_ToPitchBend(bool useOutputDevice)
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckTrackPitchValue(
                eventsToSend: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - pitchBendTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new EventToSend(new NoteOffEvent(), noteOffTime - (moveTo - moveFrom) - moveFrom)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackPitchValue_FromAfterPitchBend_ToPitchBend(bool useOutputDevice)
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckTrackPitchValue(
                eventsToSend: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - pitchBendTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, moveFrom - pitchBendTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - pitchBendTime)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackPitchValue_EnableInMiddle_FromBeforePitchBend_ToAfterPitchBend(bool useOutputDevice)
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var programChangeTime = TimeSpan.FromSeconds(1);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1200);
            var enableAfter = TimeSpan.FromMilliseconds(500);

            CheckTrackPitchValueEnabledInMiddle(
                eventsToSend: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new EventToSend(new ProgramChangeEvent(programNumber), programChangeTime - pitchBendTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - programChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, moveFrom + enableAfter),
                    new EventToSend(new NoteOffEvent(), noteOffTime - (moveTo + enableAfter))
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice,
                enableAfter: enableAfter);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackPitchValue_EnableInMiddle_FromAfterPitchBend_ToBeforePitchBend(bool useOutputDevice)
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);
            var enableAfter = TimeSpan.FromMilliseconds(150);

            CheckTrackPitchValueEnabledInMiddle(
                eventsToSend: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - pitchBendTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime),
                    new EventToSend(new PitchBendEvent(SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, moveFrom - pitchBendTime + enableAfter),
                    new EventToSend(new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, pitchBendTime - (moveTo + enableAfter)),
                    new EventToSend(new NoteOffEvent(), noteOffTime - pitchBendTime)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice,
                enableAfter: enableAfter);
        }

        #endregion

        #region Private methods

        private void CheckTrackPitchValue(
            ICollection<EventToSend> eventsToSend,
            ICollection<EventToSend> eventsWillBeSent,
            TimeSpan moveFrom,
            TimeSpan moveTo,
            bool useOutputDevice) =>
            CheckDataTracking(
                p => p.TrackPitchValue = true,
                eventsToSend,
                eventsWillBeSent,
                moveFrom,
                moveTo,
                useOutputDevice);

        private void CheckTrackPitchValueEnabledInMiddle(
            ICollection<EventToSend> eventsToSend,
            ICollection<EventToSend> eventsWillBeSent,
            TimeSpan moveFrom,
            TimeSpan moveTo,
            bool useOutputDevice,
            TimeSpan enableAfter) =>
            CheckDataTracking(
                p => p.TrackPitchValue = false,
                eventsToSend,
                eventsWillBeSent,
                moveFrom,
                moveTo,
                useOutputDevice,
                enableAfter,
                p => p.TrackPitchValue = true);

        #endregion
    }
}
