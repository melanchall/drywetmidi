using System;
using System.Collections.Generic;
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
        [TestCase(true, 0)]
        [TestCase(true, 100)]
        [TestCase(false, 0)]
        [TestCase(false, 100)]
        public void TrackControlValue_NoControlChanges_MoveToTime(bool useOutputDevice, int moveFromMs)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var moveFrom = TimeSpan.FromMilliseconds(moveFromMs);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackControlValue(
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
        public void TrackControlValue_NoControlChanges_MoveToStart(bool useOutputDevice)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckTrackControlValue(
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
        public void TrackControlValue_ControlChangeAtZero_MoveToTime(bool useOutputDevice)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)70;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackControlValue(
                eventsToSend: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), noteOffDelay - (moveTo - moveFrom))
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackControlValue_ControlChangeAtZero_MoveToStart(bool useOutputDevice)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var controlNumber1 = (SevenBitNumber)100;
            var controlValue1 = (SevenBitNumber)70;

            var controlChangeDelay = TimeSpan.FromMilliseconds(800);
            var controlNumber2 = (SevenBitNumber)10;
            var controlValue2 = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckTrackControlValue(
                eventsToSend: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber1, controlValue1), TimeSpan.Zero),
                    new EventToSend(new ControlChangeEvent(controlNumber2, controlValue2) { Channel = (FourBitNumber)10 }, controlChangeDelay),
                    new EventToSend(new NoteOffEvent(), noteOffDelay - controlChangeDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber1, controlValue1), TimeSpan.Zero),
                    new EventToSend(new ControlChangeEvent(controlNumber1, controlValue1), moveFrom),
                    new EventToSend(new ControlChangeEvent(controlNumber2, controlValue2) { Channel = (FourBitNumber)10 }, controlChangeDelay),
                    new EventToSend(new NoteOffEvent(), noteOffDelay - controlChangeDelay)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackControlValue_ControlChangesAtZero_MoveToStart(bool useOutputDevice)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var controlNumber1 = (SevenBitNumber)100;
            var controlValue1 = (SevenBitNumber)70;

            var controlNumber2 = (SevenBitNumber)10;
            var controlValue2 = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            CheckTrackControlValue(
                eventsToSend: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber1, controlValue1), TimeSpan.Zero),
                    new EventToSend(new ControlChangeEvent(controlNumber2, controlValue2), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), noteOffDelay)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber1, controlValue1), TimeSpan.Zero),
                    new EventToSend(new ControlChangeEvent(controlNumber2, controlValue2), TimeSpan.Zero),
                    new EventToSend(new ControlChangeEvent(controlNumber1, controlValue1), moveFrom),
                    new EventToSend(new ControlChangeEvent(controlNumber2, controlValue2), TimeSpan.Zero),
                    new EventToSend(new NoteOffEvent(), noteOffDelay)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackControlValue_FromBeforeControlChange_ToBeforeControlChange(bool useOutputDevice)
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)70;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackControlValue(
                eventsToSend: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - controlChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime - (moveTo - moveFrom)),
                    new EventToSend(new NoteOffEvent(), noteOffTime - controlChangeTime)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackControlValue_FromBeforeControlChange_ToAfterControlChange(bool useOutputDevice)
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)10;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1000);

            CheckTrackControlValue(
                eventsToSend: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - controlChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new EventToSend(new NoteOffEvent(), noteOffTime - (moveTo - moveFrom) - moveFrom)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackControlValue_Default_FromBeforeControlChange_ToAfterControlChange(bool useOutputDevice)
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)0;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1000);

            CheckTrackControlValue(
                eventsToSend: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - controlChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new EventToSend(new NoteOffEvent(), noteOffTime - moveTo)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackControlValue_FromAfterControlChange_ToAfterControlChange(bool useOutputDevice)
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)0;
            var controlValue = (SevenBitNumber)90;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(1500);

            CheckTrackControlValue(
                eventsToSend: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - controlChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - controlChangeTime - (moveTo - moveFrom))
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackControlValue_FromAfterControlChange_ToBeforeControlChange(bool useOutputDevice)
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)50;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);

            CheckTrackControlValue(
                eventsToSend: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - controlChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new EventToSend(new ControlChangeEvent(controlNumber, SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, moveFrom - controlChangeTime),
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime - moveTo),
                    new EventToSend(new NoteOffEvent(), noteOffTime - controlChangeTime)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackControlValue_Default_FromBeforeControlChange_ToControlChange(bool useOutputDevice)
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)0;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckTrackControlValue(
                eventsToSend: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - controlChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, moveFrom),
                    new EventToSend(new NoteOffEvent(), noteOffTime - moveTo)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackControlValue_FromAfterControlChange_ToControlChange(bool useOutputDevice)
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)50;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(800);

            CheckTrackControlValue(
                eventsToSend: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - controlChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, moveFrom - controlChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - controlChangeTime)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice);
        }

        [Retry(RetriesNumber)]
        [TestCase(true)]
        [TestCase(false)]
        public void TrackControlValue_EnableInMiddle_FromBeforeControlChange_ToAfterControlChange(bool useOutputDevice)
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var programChangeTime = TimeSpan.FromSeconds(1);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)10;
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1200);
            var enableAfter = TimeSpan.FromMilliseconds(500);

            CheckTrackControlValueEnabledInMiddle(
                eventsToSend: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new EventToSend(new ProgramChangeEvent(programNumber), programChangeTime - controlChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - programChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, moveFrom + enableAfter),
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
        public void TrackControlValue_EnableInMiddle_FromAfterControlChange_ToBeforeControlChange(bool useOutputDevice)
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)50;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);
            var enableAfter = TimeSpan.FromMilliseconds(150);

            CheckTrackControlValueEnabledInMiddle(
                eventsToSend: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new EventToSend(new NoteOffEvent(), noteOffTime - controlChangeTime)
                },
                eventsWillBeSent: new[]
                {
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime),
                    new EventToSend(new ControlChangeEvent(controlNumber, SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, moveFrom - controlChangeTime + enableAfter),
                    new EventToSend(new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, controlChangeTime - (moveTo + enableAfter)),
                    new EventToSend(new NoteOffEvent(), noteOffTime - controlChangeTime)
                },
                moveFrom: moveFrom,
                moveTo: moveTo,
                useOutputDevice: useOutputDevice,
                enableAfter: enableAfter);
        }

        #endregion

        #region Private methods

        private void CheckTrackControlValue(
            ICollection<EventToSend> eventsToSend,
            ICollection<EventToSend> eventsWillBeSent,
            TimeSpan moveFrom,
            TimeSpan moveTo,
            bool useOutputDevice) =>
            CheckDataTracking(
                p => p.TrackControlValue = true,
                eventsToSend,
                eventsWillBeSent,
                moveFrom,
                moveTo,
                useOutputDevice);

        private void CheckTrackControlValueEnabledInMiddle(
            ICollection<EventToSend> eventsToSend,
            ICollection<EventToSend> eventsWillBeSent,
            TimeSpan moveFrom,
            TimeSpan moveTo,
            bool useOutputDevice,
            TimeSpan enableAfter) =>
            CheckDataTracking(
                p => p.TrackControlValue = false,
                eventsToSend,
                eventsWillBeSent,
                moveFrom,
                moveTo,
                useOutputDevice,
                enableAfter,
                p => p.TrackControlValue = true);

        #endregion
    }
}
