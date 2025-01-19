using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Nested classes

        private sealed class OutputDeviceWithMetadataRegistration : IOutputDevice
        {
            public event EventHandler<MidiEventSentEventArgs> EventSent;

            public List<(MidiEvent, object)> Metadata { get; } = new List<(MidiEvent, object)>();

            public void PrepareForEventsSending()
            {
            }

            public void SendEvent(MidiEvent midiEvent)
            {
            }

            public bool SendEventWithMetadata(MidiEvent midiEvent, object metadata)
            {
                Metadata.Add((midiEvent, metadata));
                return true;
            }

            public void Dispose()
            {
            }
        }

        private sealed class PlaybackWithMetadataRegistration : Playback
        {
            public PlaybackWithMetadataRegistration(
                IEnumerable<ITimedObject> timedObjects,
                TempoMap tempoMap,
                IOutputDevice outputDevice,
                PlaybackSettings playbackSettings = null)
                : base(timedObjects, tempoMap, outputDevice, playbackSettings)
            {
            }

            protected override bool TryPlayEvent(MidiEvent midiEvent, object metadata)
            {
                return ((OutputDeviceWithMetadataRegistration)OutputDevice).SendEventWithMetadata(midiEvent, metadata);
            }
        }

        private sealed class TimedEventWithTrackChunkIndex : TimedEvent, IMetadata
        {
            public TimedEventWithTrackChunkIndex(MidiEvent midiEvent, long time, int trackChunkIndex)
                : base(midiEvent, time)
            {
                Metadata = trackChunkIndex;
            }

            public object Metadata { get; set; }

            public override ITimedObject Clone()
            {
                return new TimedEventWithTrackChunkIndex(Event.Clone(), Time, (int)Metadata);
            }
        }

        #endregion

        #region Test methods

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_EventPlayed()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 50 },
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent { DeltaTime = 50 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 25 },
                    new NoteOffEvent { DeltaTime = 50 },
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent { DeltaTime = 50 }));

            var timedEvents = GetTimedEvents(midiFile);
            var outputDevice = new OutputDeviceWithMetadataRegistration();

            var trackChunkIndices = new List<int>();

            using (var playback = new PlaybackWithMetadataRegistration(timedEvents, midiFile.GetTempoMap(), outputDevice))
            {
                playback.EventPlayed += (_, e) => trackChunkIndices.Add((int)e.Metadata);

                playback.Start();

                var timeout = (TimeSpan)midiFile.GetDuration<MetricTimeSpan>() + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                var playbackFinished = WaitOperations.Wait(() => !playback.IsRunning, timeout);
                Assert.IsTrue(playbackFinished, "Playback not finished.");
            }

            CollectionAssert.AreEqual(
                new[] { 0, 1, 0, 1, 0, 1, 0, 1 },
                trackChunkIndices,
                "Invalid track chunk indices registered.");
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NormalPlayback()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 50 },
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent { DeltaTime = 50 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 25 },
                    new NoteOffEvent { DeltaTime = 50 },
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent { DeltaTime = 50 }));

            var timedEvents = GetTimedEvents(midiFile);
            var outputDevice = new OutputDeviceWithMetadataRegistration();

            using (var playback = new PlaybackWithMetadataRegistration(timedEvents, midiFile.GetTempoMap(), outputDevice))
            {
                playback.Start();

                var timeout = (TimeSpan)midiFile.GetDuration<MetricTimeSpan>() + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                var playbackFinished = WaitOperations.Wait(() => !playback.IsRunning, timeout);
                Assert.IsTrue(playbackFinished, "Playback not finished.");
            }

            CheckRegisteredMetadata(
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(), 0),
                    (new NoteOnEvent(), 1),
                    (new NoteOffEvent(), 0),
                    (new NoteOffEvent(), 1),
                    (new NoteOnEvent(), 0),
                    (new NoteOnEvent(), 1),
                    (new NoteOffEvent(), 0),
                    (new NoteOffEvent(), 1),
                },
                actualMetadata: outputDevice.Metadata.ToArray());
        }

        [Retry(RetriesNumber)]
        [TestCase(0)]
        [TestCase(100)]
        public void CheckPlaybackMetadata_TrackControlValue_NoControlChanges_MoveToTime(int moveFromMs)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var moveFrom = TimeSpan.FromMilliseconds(moveFromMs);
            var moveTo = TimeSpan.FromMilliseconds(500);

            var midiFile = new MidiFile(
                new TrackChunk(),
                new TrackChunk(new TextEvent { DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOffDelay, TempoMap.Default) }));

            CheckTrackControlValueWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackControlValue_NoControlChanges_MoveToStart()
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            var midiFile = new MidiFile(
                new TrackChunk(new TextEvent()),
                new TrackChunk(new ProgramChangeEvent { DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOffDelay, TempoMap.Default) }));

            CheckTrackControlValueWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new TextEvent(), 0),
                    (new TextEvent(), 0),
                    (new ProgramChangeEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackControlValue_ControlChangeAtZero_MoveToTime()
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)70;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            var midiFile = new MidiFile(
                new TrackChunk(new ControlChangeEvent(controlNumber, controlValue)),
                new TrackChunk(new TextEvent { DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOffDelay, TempoMap.Default) }));

            CheckTrackControlValueWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new ControlChangeEvent(controlNumber, controlValue), 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackControlValue_ControlChangeAtZero_MoveToStart()
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var controlNumber1 = (SevenBitNumber)100;
            var controlValue1 = (SevenBitNumber)70;

            var controlChangeDelay = TimeSpan.FromMilliseconds(800);
            var controlNumber2 = (SevenBitNumber)10;
            var controlValue2 = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            var midiFile = new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent(controlNumber1, controlValue1)),
                new TrackChunk(
                    new ControlChangeEvent(controlNumber2, controlValue2)
                    {
                        Channel = (FourBitNumber)10,
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)controlChangeDelay, TempoMap.Default)
                    },
                    new TextEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOffDelay - controlChangeDelay), TempoMap.Default)
                    }));

            CheckTrackControlValueWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new ControlChangeEvent(controlNumber1, controlValue1), 0),
                    (new ControlChangeEvent(controlNumber1, controlValue1), 0),
                    (new ControlChangeEvent(controlNumber2, controlValue2) { Channel = (FourBitNumber)10 }, 1),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackControlValue_ControlChangesAtZero_MoveToStart()
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var controlNumber1 = (SevenBitNumber)100;
            var controlValue1 = (SevenBitNumber)70;

            var controlNumber2 = (SevenBitNumber)10;
            var controlValue2 = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            var midiFile = new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent(controlNumber1, controlValue1)),
                new TrackChunk(
                    new ControlChangeEvent(controlNumber2, controlValue2),
                    new TextEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOffDelay, TempoMap.Default)
                    }));

            CheckTrackControlValueWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new ControlChangeEvent(controlNumber1, controlValue1), 0),
                    (new ControlChangeEvent(controlNumber2, controlValue2), 1),
                    (new ControlChangeEvent(controlNumber1, controlValue1), 0),
                    (new ControlChangeEvent(controlNumber2, controlValue2), 1),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackControlValue_FromBeforeControlChange_ToBeforeControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)70;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent(controlNumber, controlValue)
                    {
                        Channel = (FourBitNumber)4,
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)controlChangeTime, TempoMap.Default)
                    }),
                new TrackChunk(
                    new TextEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOffTime - controlChangeTime), TempoMap.Default)
                    }));

            CheckTrackControlValueWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackControlValue_FromBeforeControlChange_ToAfterControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)10;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1000);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent(controlNumber, controlValue)
                    {
                        Channel = (FourBitNumber)4,
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)controlChangeTime, TempoMap.Default)
                    }),
                new TrackChunk(
                    new TextEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOffTime - controlChangeTime), TempoMap.Default)
                    }));

            CheckTrackControlValueWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackControlValue_Default_FromBeforeControlChange_ToAfterControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)0;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1000);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent(controlNumber, controlValue)
                    {
                        Channel = (FourBitNumber)4,
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)controlChangeTime, TempoMap.Default)
                    }),
                new TrackChunk(
                    new TextEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOffTime - controlChangeTime), TempoMap.Default)
                    }));

            CheckTrackControlValueWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackControlValue_FromAfterControlChange_ToAfterControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)0;
            var controlValue = (SevenBitNumber)90;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(1500);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent(controlNumber, controlValue)
                    {
                        Channel = (FourBitNumber)4,
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)controlChangeTime, TempoMap.Default)
                    }),
                new TrackChunk(
                    new TextEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOffTime - controlChangeTime), TempoMap.Default)
                    }));

            CheckTrackControlValueWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackControlValue_FromAfterControlChange_ToBeforeControlChange()
        {
            var controlChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var controlNumber = (SevenBitNumber)100;
            var controlValue = (SevenBitNumber)50;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent(controlNumber, controlValue)
                    {
                        Channel = (FourBitNumber)4,
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)controlChangeTime, TempoMap.Default)
                    }),
                new TrackChunk(
                    new TextEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOffTime - controlChangeTime), TempoMap.Default)
                    }));

            CheckTrackControlValueWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, 0),
                    (new ControlChangeEvent(controlNumber, SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, null),
                    (new ControlChangeEvent(controlNumber, controlValue) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(0)]
        [TestCase(100)]
        public void CheckPlaybackMetadata_TrackPitchValue_NoPitchBend_MoveToTime(int moveFromMs)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var moveFrom = TimeSpan.FromMilliseconds(moveFromMs);
            var moveTo = TimeSpan.FromMilliseconds(500);

            var midiFile = new MidiFile(
                new TrackChunk(),
                new TrackChunk(
                    new TextEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOffDelay, TempoMap.Default)
                    }));

            CheckTrackPitchValueWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackPitchValue_NoPitchBend_MoveToStart()
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            var midiFile = new MidiFile(
                new TrackChunk(),
                new TrackChunk(
                    new TextEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOffDelay, TempoMap.Default)
                    }));

            CheckTrackPitchValueWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackPitchValue_PitchBendAtZero_MoveToTime()
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new PitchBendEvent(pitchValue)),
                new TrackChunk(
                    new TextEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOffDelay, TempoMap.Default)
                    }));

            CheckTrackPitchValueWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new PitchBendEvent(pitchValue), 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackPitchValue_PitchBendAtZero_MoveToStart()
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            var midiFile = new MidiFile(
                new TrackChunk(
                    new PitchBendEvent(pitchValue)),
                new TrackChunk(
                    new TextEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOffDelay, TempoMap.Default)
                    }));

            CheckTrackPitchValueWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new PitchBendEvent(pitchValue), 0),
                    (new PitchBendEvent(pitchValue), 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackPitchValue_FromBeforePitchBend_ToBeforePitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new PitchBendEvent(pitchValue)
                    {
                        Channel = (FourBitNumber)4,
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)pitchBendTime, TempoMap.Default)
                    }),
                new TrackChunk(
                    new TextEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOffTime - pitchBendTime), TempoMap.Default)
                    }));

            CheckTrackPitchValueWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackPitchValue_FromBeforePitchBend_ToAfterPitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1000);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new PitchBendEvent(pitchValue)
                    {
                        Channel = (FourBitNumber)4,
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)pitchBendTime, TempoMap.Default)
                    }),
                new TrackChunk(
                    new TextEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOffTime - pitchBendTime), TempoMap.Default)
                    }));

            CheckTrackPitchValueWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackPitchValue_FromAfterPitchBend_ToAfterPitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(1500);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new PitchBendEvent(pitchValue)
                    {
                        Channel = (FourBitNumber)4,
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)pitchBendTime, TempoMap.Default)
                    }),
                new TrackChunk(
                    new TextEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOffTime - pitchBendTime), TempoMap.Default)
                    }));

            CheckTrackPitchValueWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackPitchValue_FromAfterPitchBend_ToBeforePitchBend()
        {
            var pitchBendTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var pitchValue = (ushort)234;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new PitchBendEvent(pitchValue)
                    {
                        Channel = (FourBitNumber)4,
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)pitchBendTime, TempoMap.Default)
                    }),
                new TrackChunk(
                    new TextEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOffTime - pitchBendTime), TempoMap.Default)
                    }));

            CheckTrackPitchValueWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, 0),
                    (new PitchBendEvent() { Channel = (FourBitNumber)4 }, null),
                    (new PitchBendEvent(pitchValue) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [TestCase(0)]
        [TestCase(100)]
        public void CheckPlaybackMetadata_TrackProgram_NoProgramChanges_MoveToTime(int moveFromMs)
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var moveFrom = TimeSpan.FromMilliseconds(moveFromMs);
            var moveTo = TimeSpan.FromMilliseconds(500);

            var midiFile = new MidiFile(
                new TrackChunk(),
                new TrackChunk(
                    new TextEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOffDelay, TempoMap.Default)
                    }));

            CheckTrackProgramWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackProgram_NoProgramChanges_MoveToStart()
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            var midiFile = new MidiFile(
                new TrackChunk(),
                new TrackChunk(
                    new TextEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOffDelay, TempoMap.Default)
                    }));

            CheckTrackProgramWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackProgram_ProgramChangeAtZero_MoveToTime()
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new ProgramChangeEvent(programNumber)),
                new TrackChunk(
                    new TextEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOffDelay, TempoMap.Default)
                    }));

            CheckTrackProgramWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new ProgramChangeEvent(programNumber), 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackProgram_ProgramChangeAtZero_MoveToStart()
        {
            var noteOffDelay = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.Zero;

            var midiFile = new MidiFile(
                new TrackChunk(
                    new ProgramChangeEvent(programNumber)),
                new TrackChunk(
                    new TextEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOffDelay, TempoMap.Default)
                    }));

            CheckTrackProgramWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new ProgramChangeEvent(programNumber), 0),
                    (new ProgramChangeEvent(programNumber), 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackProgram_FromBeforeProgramChange_ToBeforeProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(100);
            var moveTo = TimeSpan.FromMilliseconds(500);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new ProgramChangeEvent(programNumber)
                    {
                        Channel = (FourBitNumber)4,
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)programChangeTime, TempoMap.Default)
                    }),
                new TrackChunk(
                    new TextEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOffTime - programChangeTime), TempoMap.Default)
                    }));

            CheckTrackProgramWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackProgram_FromBeforeProgramChange_ToAfterProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1000);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new ProgramChangeEvent(programNumber)
                    {
                        Channel = (FourBitNumber)4,
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)programChangeTime, TempoMap.Default)
                    }),
                new TrackChunk(
                    new TextEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOffTime - programChangeTime), TempoMap.Default)
                    }));

            CheckTrackProgramWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackProgram_FromAfterProgramChange_ToAfterProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(1500);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new ProgramChangeEvent(programNumber)
                    {
                        Channel = (FourBitNumber)4,
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)programChangeTime, TempoMap.Default)
                    }),
                new TrackChunk(
                    new TextEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOffTime - programChangeTime), TempoMap.Default)
                    }));

            CheckTrackProgramWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackProgram_FromAfterProgramChange_ToBeforeProgramChange()
        {
            var programChangeTime = TimeSpan.FromMilliseconds(800);
            var noteOffTime = TimeSpan.FromSeconds(2);
            var programNumber = (SevenBitNumber)100;

            var moveFrom = TimeSpan.FromMilliseconds(1000);
            var moveTo = TimeSpan.FromMilliseconds(500);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new ProgramChangeEvent(programNumber)
                    {
                        Channel = (FourBitNumber)4,
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)programChangeTime, TempoMap.Default)
                    }),
                new TrackChunk(
                    new TextEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOffTime - programChangeTime), TempoMap.Default)
                    }));

            CheckTrackProgramWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, 0),
                    (new ProgramChangeEvent(SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 }, null),
                    (new ProgramChangeEvent(programNumber) { Channel = (FourBitNumber)4 }, 0),
                    (new TextEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackNotes_MoveForwardToNote()
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.FromSeconds(1);
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromMilliseconds(800);
            var noteOffVelocity = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1500);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(noteNumber, noteOnVelocity)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOnDelay, TempoMap.Default)
                    }),
                new TrackChunk(
                    new NoteOffEvent(noteNumber, noteOffVelocity)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOnDelay + noteOffDelay), TempoMap.Default)
                    }));

            CheckTrackNotesWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(noteNumber, noteOnVelocity), 0),
                    (new NoteOffEvent(noteNumber, noteOffVelocity), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackNotes_MoveBackToNote()
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.Zero;
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromMilliseconds(800);
            var noteOffVelocity = (SevenBitNumber)80;
            var pitchBendDelay = TimeSpan.FromMilliseconds(300);

            var moveFrom = TimeSpan.FromSeconds(1);
            var moveTo = TimeSpan.FromMilliseconds(500);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(noteNumber, noteOnVelocity)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOnDelay, TempoMap.Default)
                    }),
                new TrackChunk(
                    new NoteOffEvent(noteNumber, noteOffVelocity)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOnDelay + noteOffDelay), TempoMap.Default)
                    },
                    new PitchBendEvent()
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)pitchBendDelay, TempoMap.Default)
                    }));

            CheckTrackNotesWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(noteNumber, noteOnVelocity), 0),
                    (new NoteOffEvent(noteNumber, noteOffVelocity), 1),
                    (new NoteOnEvent(noteNumber, noteOnVelocity), 0),
                    (new NoteOffEvent(noteNumber, noteOffVelocity), 1),
                    (new PitchBendEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackNotes_MoveForwardFromNote()
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.Zero;
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromMilliseconds(800);
            var noteOffVelocity = (SevenBitNumber)80;
            var pitchBendDelay = TimeSpan.FromMilliseconds(400);

            var moveFrom = TimeSpan.FromMilliseconds(500);
            var moveTo = TimeSpan.FromMilliseconds(1000);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(noteNumber, noteOnVelocity)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOnDelay, TempoMap.Default)
                    }),
                new TrackChunk(
                    new NoteOffEvent(noteNumber, noteOffVelocity)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOnDelay + noteOffDelay), TempoMap.Default)
                    },
                    new PitchBendEvent()
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)pitchBendDelay, TempoMap.Default)
                    }));

            CheckTrackNotesWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(noteNumber, noteOnVelocity), 0),
                    (new NoteOffEvent(noteNumber, noteOffVelocity), 1),
                    (new PitchBendEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackNotes_MoveBackFromNote()
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.FromSeconds(1);
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromMilliseconds(400);
            var noteOffVelocity = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(1200);
            var moveTo = TimeSpan.FromMilliseconds(700);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(noteNumber, noteOnVelocity)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOnDelay, TempoMap.Default)
                    }),
                new TrackChunk(
                    new NoteOffEvent(noteNumber, noteOffVelocity)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOnDelay + noteOffDelay), TempoMap.Default)
                    }));

            CheckTrackNotesWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(noteNumber, noteOnVelocity), 0),
                    (new NoteOffEvent(noteNumber, noteOffVelocity), 1),
                    (new NoteOnEvent(noteNumber, noteOnVelocity), 0),
                    (new NoteOffEvent(noteNumber, noteOffVelocity), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackNotes_MoveForwardToSameNote()
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.Zero;
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromSeconds(1);
            var noteOffVelocity = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(200);
            var moveTo = TimeSpan.FromMilliseconds(700);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(noteNumber, noteOnVelocity)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOnDelay, TempoMap.Default)
                    }),
                new TrackChunk(
                    new NoteOffEvent(noteNumber, noteOffVelocity)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOnDelay + noteOffDelay), TempoMap.Default)
                    }));

            CheckTrackNotesWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(noteNumber, noteOnVelocity), 0),
                    (new NoteOffEvent(noteNumber, noteOffVelocity), 1),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackNotes_MoveBackToSameNote()
        {
            var noteNumber = (SevenBitNumber)60;
            var noteOnDelay = TimeSpan.Zero;
            var noteOnVelocity = (SevenBitNumber)100;
            var noteOffDelay = TimeSpan.FromSeconds(1);
            var noteOffVelocity = (SevenBitNumber)80;

            var moveFrom = TimeSpan.FromMilliseconds(700);
            var moveTo = TimeSpan.FromMilliseconds(400);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(noteNumber, noteOnVelocity)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOnDelay, TempoMap.Default)
                    }),
                new TrackChunk(
                    new NoteOffEvent(noteNumber, noteOffVelocity)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOnDelay + noteOffDelay), TempoMap.Default)
                    }));

            CheckTrackNotesWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(noteNumber, noteOnVelocity), 0),
                    (new NoteOffEvent(noteNumber, noteOffVelocity), 1),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackNotes_MoveForwardFromNoteToNote()
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

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(noteNumber1, noteOnVelocity1)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOnDelay1, TempoMap.Default)
                    },
                    new NoteOffEvent(noteNumber1, noteOffVelocity1)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOffDelay1, TempoMap.Default)
                    }),
                new TrackChunk(
                    new NoteOnEvent(noteNumber2, noteOnVelocity2)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOnDelay1 + noteOffDelay1 + noteOnDelay2), TempoMap.Default)
                    },
                    new NoteOffEvent(noteNumber2, noteOffVelocity2)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOffDelay2, TempoMap.Default)
                    }));

            CheckTrackNotesWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(noteNumber1, noteOnVelocity1), 0),
                    (new NoteOffEvent(noteNumber1, noteOffVelocity1), 0),
                    (new NoteOnEvent(noteNumber2, noteOnVelocity2), 1),
                    (new NoteOffEvent(noteNumber2, noteOffVelocity2), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackNotes_MoveBackFromNoteToNote()
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

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(noteNumber1, noteOnVelocity1)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOnDelay1, TempoMap.Default)
                    },
                    new NoteOffEvent(noteNumber1, noteOffVelocity1)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOffDelay1, TempoMap.Default)
                    }),
                new TrackChunk(
                    new NoteOnEvent(noteNumber2, noteOnVelocity2)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOnDelay1 + noteOffDelay1 + noteOnDelay2), TempoMap.Default)
                    },
                    new NoteOffEvent(noteNumber2, noteOffVelocity2)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOffDelay2, TempoMap.Default)
                    }));

            CheckTrackNotesWithMetadata(
                midiFile,
                moveFrom: moveFrom,
                moveTo: moveTo,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(noteNumber1, noteOnVelocity1), 0),
                    (new NoteOffEvent(noteNumber1, noteOffVelocity1), 0),
                    (new NoteOnEvent(noteNumber2, noteOnVelocity2), 1),
                    (new NoteOffEvent(noteNumber2, noteOffVelocity2), 1),
                    (new NoteOnEvent(noteNumber1, noteOnVelocity1), 0),
                    (new NoteOffEvent(noteNumber1, noteOffVelocity1), 0),
                    (new NoteOnEvent(noteNumber2, noteOnVelocity2), 1),
                    (new NoteOffEvent(noteNumber2, noteOffVelocity2), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackNotes_StopStart_InterruptNotesOnStop()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.Zero;

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent()
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOnDelay, TempoMap.Default)
                    }),
                new TrackChunk(
                    new NoteOffEvent()
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOnDelay + noteOffDelay), TempoMap.Default)
                    }));

            CheckTrackNotesInterruptingNotesWithMetadata(
                midiFile,
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                interruptNotesOnStop: true,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(), 0),
                    (new NoteOffEvent(), 1),
                    (new NoteOnEvent(), 0),
                    (new NoteOffEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_TrackNotes_StopStart_DontInterruptNotesOnStop()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.Zero;

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent()
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOnDelay, TempoMap.Default)
                    }),
                new TrackChunk(
                    new NoteOffEvent()
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(noteOnDelay + noteOffDelay), TempoMap.Default)
                    }));

            CheckTrackNotesInterruptingNotesWithMetadata(
                midiFile,
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                interruptNotesOnStop: false,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(), 0),
                    (new NoteOffEvent(), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_ReturnNull_ReturnSkipNote()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent()
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap.Default)
                    }),
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(TimeSpan.FromSeconds(1) + TimeSpan.FromMilliseconds(500)), TempoMap.Default)
                    },
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap.Default)
                    }));

            CheckNoteCallbackWithMetadata(
                midiFile,
                changeCallbackAfter: TimeSpan.FromMilliseconds(500),
                noteCallback: (d, rt, rl, t) => null,
                secondNoteCallback: (d, rt, rl, t) => NotePlaybackData.SkipNote,
                expectedMetadata: new (MidiEvent, object)[0]);
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_ReturnNull_ReturnOriginal()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent()
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap.Default)
                    }),
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(TimeSpan.FromSeconds(1) + TimeSpan.FromMilliseconds(500)), TempoMap.Default)
                    },
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap.Default)
                    }));

            CheckNoteCallbackWithMetadata(
                midiFile,
                changeCallbackAfter: TimeSpan.FromMilliseconds(500),
                noteCallback: (d, rt, rl, t) => null,
                secondNoteCallback: (d, rt, rl, t) => d,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80), 1),
                    (new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_ReturnOriginal_ReturnNull()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent()
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap.Default)
                    }),
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(TimeSpan.FromSeconds(1) + TimeSpan.FromMilliseconds(500)), TempoMap.Default)
                    },
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap.Default)
                    }));

            CheckNoteCallbackWithMetadata(
                midiFile,
                changeCallbackAfter: TimeSpan.FromMilliseconds(500),
                noteCallback: (d, rt, rl, t) => d,
                secondNoteCallback: (d, rt, rl, t) => null,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(), 0),
                    (new NoteOffEvent(), 0)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_Transpose()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent()
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromSeconds(1), TempoMap.Default)
                    }),
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)80)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(TimeSpan.FromSeconds(1) + TimeSpan.FromMilliseconds(500)), TempoMap.Default)
                    },
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)0)
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromMilliseconds(500), TempoMap.Default)
                    }));

            CheckNoteCallbackWithMetadata(
                midiFile,
                changeCallbackAfter: TimeSpan.FromMilliseconds(500),
                noteCallback: NoteCallback,
                secondNoteCallback: NoteCallback,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOnEvent((SevenBitNumber)(100 + TransposeBy), (SevenBitNumber)80), 1),
                    (new NoteOffEvent((SevenBitNumber)(100 + TransposeBy), (SevenBitNumber)0), 1)
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_InterruptNotesOnStop()
        {
            var noteOnDelay = TimeSpan.Zero;
            var noteOffDelay = TimeSpan.FromSeconds(2);

            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(400);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent()),
                new TrackChunk(
                    new NoteOffEvent
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)noteOffDelay, TempoMap.Default)
                    }));

            CheckNoteCallbackWithMetadata(
                midiFile,
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: (context, playback) =>
                {
                    playback.InterruptNotesOnStop = true;
                    playback.NoteCallback = NoteCallback;
                },
                afterStart: NoPlaybackAction,
                afterStop: NoPlaybackAction,
                afterResume: NoPlaybackAction,
                runningAfterResume: null,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 1),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 1),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_MoveToStart()
        {
            var stopAfter = TimeSpan.FromMilliseconds(500);
            var stopPeriod = TimeSpan.FromSeconds(1);

            var firstEventTime = TimeSpan.Zero;
            var lastEventTime = TimeSpan.FromSeconds(5);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(400);
            var secondAfterResumeDelay = TimeSpan.FromSeconds(1);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(500);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent()
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)firstEventTime, TempoMap.Default)
                    }),
                new TrackChunk(
                    new NoteOffEvent()
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(firstEventTime + lastEventTime), TempoMap.Default)
                    }));

            CheckNoteCallbackWithMetadata(
                midiFile,
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: (context, playback) => playback.NoteCallback = NoteCallback,
                afterStart: NoPlaybackAction,
                afterStop: (context, playback) => playback.MoveToStart(),
                afterResume: (context, playback) => CheckCurrentTime(playback, TimeSpan.Zero, "stopped"),
                runningAfterResume: new[]
                {
                    Tuple.Create<TimeSpan, PlaybackAction>(firstAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, firstAfterResumeDelay, "resumed")),
                    Tuple.Create<TimeSpan, PlaybackAction>(secondAfterResumeDelay, (context, playback) =>
                    {
                        playback.MoveToStart();
                        CheckCurrentTime(playback, TimeSpan.Zero, "resumed");
                    }),
                    Tuple.Create<TimeSpan, PlaybackAction>(thirdAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, thirdAfterResumeDelay, "resumed"))
                },
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 1),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_MoveForward()
        {
            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(500);

            var stepAfterStop = TimeSpan.FromSeconds(1);
            var stepAfterResumed = TimeSpan.FromMilliseconds(500);

            var firstEventTime = TimeSpan.Zero;
            var lastEventTime = TimeSpan.FromSeconds(8);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(600);
            var secondAfterResumeDelay = TimeSpan.FromSeconds(1);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(500);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent()
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)firstEventTime, TempoMap.Default)
                    }),
                new TrackChunk(
                    new NoteOffEvent()
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)(firstEventTime + lastEventTime), TempoMap.Default)
                    }));

            CheckNoteCallbackWithMetadata(
                midiFile,
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: (context, playback) => playback.NoteCallback = NoteCallback,
                afterStart: NoPlaybackAction,
                afterStop: (context, playback) => playback.MoveForward((MetricTimeSpan)stepAfterStop),
                afterResume: (context, playback) => CheckCurrentTime(playback, stopAfter + stepAfterStop, "stopped"),
                runningAfterResume: new[]
                {
                    Tuple.Create<TimeSpan, PlaybackAction>(firstAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, stopAfter + firstAfterResumeDelay + stepAfterStop, "resumed")),
                    Tuple.Create<TimeSpan, PlaybackAction>(secondAfterResumeDelay, (context, playback) =>
                    {
                        playback.MoveForward((MetricTimeSpan)stepAfterResumed);
                        CheckCurrentTime(playback, stopAfter + firstAfterResumeDelay + secondAfterResumeDelay + stepAfterStop + stepAfterResumed, "resumed");
                    }),
                    Tuple.Create<TimeSpan, PlaybackAction>(thirdAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, stopAfter + firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay + stepAfterStop + stepAfterResumed, "resumed"))
                },
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 1),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_MoveForward_BeyondDuration()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(2);

            var stepAfterStop = TimeSpan.FromSeconds(10);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent()),
                new TrackChunk(
                    new NoteOffEvent()
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromSeconds(4), TempoMap.Default)
                    }));

            CheckNoteCallbackWithMetadata(
                midiFile,
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: (context, playback) => playback.NoteCallback = NoteCallback,
                afterStart: NoPlaybackAction,
                afterStop: (context, playback) => playback.MoveForward((MetricTimeSpan)stepAfterStop),
                afterResume: (context, playback) => CheckCurrentTime(playback, TimeSpan.FromSeconds(4), "stopped"),
                runningAfterResume: null,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 1),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_MoveBack()
        {
            var stopAfter = TimeSpan.FromSeconds(1);
            var stopPeriod = TimeSpan.FromMilliseconds(500);

            var stepAfterStop = TimeSpan.FromMilliseconds(300);
            var stepAfterResumed = TimeSpan.FromMilliseconds(500);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(500);
            var secondAfterResumeDelay = TimeSpan.FromSeconds(1);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(500);

            var lastEventTime = TimeSpan.FromMilliseconds(5500);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent()),
                new TrackChunk(
                    new NoteOffEvent()
                    {
                        DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)lastEventTime, TempoMap.Default)
                    }));

            CheckNoteCallbackWithMetadata(
                midiFile,
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: (context, playback) => playback.NoteCallback = NoteCallback,
                afterStart: NoPlaybackAction,
                afterStop: (context, playback) => playback.MoveBack((MetricTimeSpan)stepAfterStop),
                afterResume: (context, playback) => CheckCurrentTime(playback, stopAfter - stepAfterStop, "stopped"),
                runningAfterResume: new[]
                {
                    Tuple.Create<TimeSpan, PlaybackAction>(firstAfterResumeDelay, (context, playback) =>
                    {
                        Assert.IsTrue(playback.IsRunning, "Playback is not running after resumed.");
                        CheckCurrentTime(playback, stopAfter + firstAfterResumeDelay - stepAfterStop, "resumed on first span");
                    }),
                    Tuple.Create<TimeSpan, PlaybackAction>(secondAfterResumeDelay, (context, playback) =>
                    {
                        playback.MoveBack((MetricTimeSpan)stepAfterResumed);
                        CheckCurrentTime(playback, stopAfter + firstAfterResumeDelay + secondAfterResumeDelay - stepAfterStop - stepAfterResumed, "resumed on second span");
                    }),
                    Tuple.Create<TimeSpan, PlaybackAction>(thirdAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, stopAfter + firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay - stepAfterStop - stepAfterResumed, "resumed on third span"))
                },
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 1),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_MoveBack_BeyondZero()
        {
            var stopAfter = TimeSpan.FromSeconds(3);
            var stopPeriod = TimeSpan.FromSeconds(2);

            var stepAfterStop = TimeSpan.FromSeconds(5);
            var stepAfterResumed = TimeSpan.FromMilliseconds(500);

            var lastEventTime = TimeSpan.FromSeconds(5);

            var firstAfterResumeDelay = TimeSpan.FromMilliseconds(500);
            var secondAfterResumeDelay = TimeSpan.FromSeconds(1);
            var thirdAfterResumeDelay = TimeSpan.FromMilliseconds(500);

            var midiFile = new MidiFile(
               new TrackChunk(
                   new NoteOnEvent()),
               new TrackChunk(
                   new NoteOffEvent()
                   {
                       DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)lastEventTime, TempoMap.Default)
                   }));

            CheckNoteCallbackWithMetadata(
                midiFile,
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: (context, playback) => playback.NoteCallback = NoteCallback,
                afterStart: NoPlaybackAction,
                afterStop: (context, playback) => playback.MoveBack((MetricTimeSpan)stepAfterStop),
                afterResume: (context, playback) => CheckCurrentTime(playback, TimeSpan.Zero, "stopped"),
                runningAfterResume: new[]
                {
                    Tuple.Create<TimeSpan, PlaybackAction>(firstAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, firstAfterResumeDelay, "resumed")),
                    Tuple.Create<TimeSpan, PlaybackAction>(secondAfterResumeDelay, (context, playback) =>
                    {
                        playback.MoveBack((MetricTimeSpan)stepAfterResumed);
                        CheckCurrentTime(playback, firstAfterResumeDelay + secondAfterResumeDelay - stepAfterResumed, "resumed");
                    }),
                    Tuple.Create<TimeSpan, PlaybackAction>(thirdAfterResumeDelay, (context, playback) => CheckCurrentTime(playback, firstAfterResumeDelay + secondAfterResumeDelay + thirdAfterResumeDelay - stepAfterResumed, "resumed"))
                },
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 1),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_MoveToTime()
        {
            var stopAfter = TimeSpan.FromSeconds(4);
            var stopPeriod = TimeSpan.FromSeconds(2);

            var midiFile = new MidiFile(
               new TrackChunk(
                   new NoteOnEvent()),
               new TrackChunk(
                   new NoteOffEvent()
                   {
                       DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromSeconds(10), TempoMap.Default)
                   }));

            CheckNoteCallbackWithMetadata(
                midiFile,
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: (context, playback) => playback.NoteCallback = NoteCallback,
                afterStart: NoPlaybackAction,
                afterStop: (context, playback) => playback.MoveToTime(new MetricTimeSpan(0, 0, 1)),
                afterResume: (context, playback) => CheckCurrentTime(playback, TimeSpan.FromSeconds(1), "stopped"),
                runningAfterResume: new[]
                {
                    Tuple.Create<TimeSpan, PlaybackAction>(TimeSpan.FromSeconds(1), (context, playback) => CheckCurrentTime(playback, TimeSpan.FromSeconds(2), "resumed")),
                    Tuple.Create<TimeSpan, PlaybackAction>(TimeSpan.FromSeconds(2), (context, playback) =>
                    {
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 8));
                        CheckCurrentTime(playback, TimeSpan.FromSeconds(8), "resumed");
                    }),
                    Tuple.Create<TimeSpan, PlaybackAction>(TimeSpan.FromSeconds(1), (context, playback) => CheckCurrentTime(playback, TimeSpan.FromSeconds(9), "resumed"))
                },
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 1),
                });
        }

        [Retry(RetriesNumber)]
        [Test]
        public void CheckPlaybackMetadata_NoteCallback_MoveToTime_BeyondDuration()
        {
            var stopAfter = TimeSpan.FromSeconds(2);
            var stopPeriod = TimeSpan.FromSeconds(2);

            var midiFile = new MidiFile(
               new TrackChunk(
                   new NoteOnEvent()),
               new TrackChunk(
                   new NoteOffEvent()
                   {
                       DeltaTime = TimeConverter.ConvertFrom((MetricTimeSpan)TimeSpan.FromSeconds(4), TempoMap.Default)
                   }));

            CheckNoteCallbackWithMetadata(
                midiFile,
                stopAfter: stopAfter,
                stopPeriod: stopPeriod,
                setupPlayback: (context, playback) => playback.NoteCallback = NoteCallback,
                afterStart: NoPlaybackAction,
                afterStop: (context, playback) => playback.MoveToTime(new MetricTimeSpan(0, 0, 10)),
                afterResume: (context, playback) => CheckCurrentTime(playback, TimeSpan.FromSeconds(4), "stopped"),
                runningAfterResume: null,
                expectedMetadata: new (MidiEvent, object)[]
                {
                    (new NoteOnEvent(TransposeBy, SevenBitNumber.MinValue), 0),
                    (new NoteOffEvent(TransposeBy, SevenBitNumber.MinValue), 1),
                });
        }

        #endregion

        #region Private methods

        private void CheckTrackControlValueWithMetadata(
            MidiFile midiFile,
            TimeSpan moveFrom,
            TimeSpan moveTo,
            (MidiEvent, object)[] expectedMetadata)
        {
            CheckTrackDataWithMetadata(
                p => p.TrackControlValue = true,
                midiFile,
                moveFrom,
                moveTo,
                expectedMetadata);
        }

        private void CheckTrackProgramWithMetadata(
            MidiFile midiFile,
            TimeSpan moveFrom,
            TimeSpan moveTo,
            (MidiEvent, object)[] expectedMetadata)
        {
            CheckTrackDataWithMetadata(
                p => { },
                midiFile,
                moveFrom,
                moveTo,
                expectedMetadata);
        }

        private void CheckTrackPitchValueWithMetadata(
            MidiFile midiFile,
            TimeSpan moveFrom,
            TimeSpan moveTo,
            (MidiEvent, object)[] expectedMetadata)
        {
            CheckTrackDataWithMetadata(
                p => p.TrackPitchValue = true,
                midiFile,
                moveFrom,
                moveTo,
                expectedMetadata);
        }

        private void CheckTrackDataWithMetadata(
            Action<Playback> setupPlayback,
            MidiFile midiFile,
            TimeSpan moveFrom,
            TimeSpan moveTo,
            (MidiEvent, object)[] expectedMetadata)
        {
            var playbackContext = new PlaybackContext();
            var stopwatch = playbackContext.Stopwatch;
            var tempoMap = playbackContext.TempoMap;

            var timedEvents = GetTimedEvents(midiFile);
            var outputDevice = new OutputDeviceWithMetadataRegistration();

            using (var playback = new PlaybackWithMetadataRegistration(timedEvents, tempoMap, outputDevice))
            {
                setupPlayback(playback);

                stopwatch.Start();
                playback.Start();

                WaitOperations.Wait(() => stopwatch.Elapsed >= moveFrom);
                playback.MoveToTime((MetricTimeSpan)moveTo);

                stopwatch.Stop();

                var timeout = (TimeSpan)midiFile.GetDuration<MetricTimeSpan>() + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                var playbackFinished = WaitOperations.Wait(() => !playback.IsRunning, timeout);
                Assert.IsTrue(playbackFinished, "Playback not finished.");
            }

            CheckRegisteredMetadata(
                expectedMetadata: expectedMetadata,
                actualMetadata: outputDevice.Metadata.ToArray());
        }

        private void CheckTrackNotesWithMetadata(
            MidiFile midiFile,
            TimeSpan moveFrom,
            TimeSpan moveTo,
            (MidiEvent, object)[] expectedMetadata)
        {
            var playbackContext = new PlaybackContext();
            var stopwatch = playbackContext.Stopwatch;
            var tempoMap = playbackContext.TempoMap;

            var timedEvents = GetTimedEvents(midiFile);
            var outputDevice = new OutputDeviceWithMetadataRegistration();

            using (var playback = new PlaybackWithMetadataRegistration(timedEvents, tempoMap, outputDevice))
            {
                playback.TrackNotes = true;

                stopwatch.Start();
                playback.Start();

                WaitOperations.Wait(() => stopwatch.Elapsed >= moveFrom);
                playback.MoveToTime((MetricTimeSpan)moveTo);

                stopwatch.Stop();

                var timeout = (TimeSpan)midiFile.GetDuration<MetricTimeSpan>() + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                var playbackFinished = WaitOperations.Wait(() => !playback.IsRunning, timeout);
                Assert.IsTrue(playbackFinished, "Playback not finished.");
            }

            CheckRegisteredMetadata(
                expectedMetadata: expectedMetadata,
                actualMetadata: outputDevice.Metadata.ToArray());
        }

        private void CheckTrackNotesInterruptingNotesWithMetadata(
            MidiFile midiFile,
            TimeSpan stopAfter,
            TimeSpan stopPeriod,
            bool interruptNotesOnStop,
            (MidiEvent, object)[] expectedMetadata)
        {
            var playbackContext = new PlaybackContext();

            var stopwatch = playbackContext.Stopwatch;
            var tempoMap = playbackContext.TempoMap;

            var timedEvents = GetTimedEvents(midiFile);
            var outputDevice = new OutputDeviceWithMetadataRegistration();

            using (var playback = new PlaybackWithMetadataRegistration(timedEvents, tempoMap, outputDevice))
            {
                playback.InterruptNotesOnStop = interruptNotesOnStop;
                playback.TrackNotes = true;

                stopwatch.Start();
                playback.Start();

                WaitOperations.Wait(() => stopwatch.Elapsed >= stopAfter);
                playback.Stop();

                WaitOperations.Wait(stopPeriod);
                playback.Start();

                stopwatch.Stop();

                var timeout = (TimeSpan)midiFile.GetDuration<MetricTimeSpan>() + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                var playbackFinished = WaitOperations.Wait(() => !playback.IsRunning, timeout);
                Assert.IsTrue(playbackFinished, "Playback not finished.");
            }

            CheckRegisteredMetadata(
                expectedMetadata: expectedMetadata,
                actualMetadata: outputDevice.Metadata.ToArray());
        }

        private void CheckNoteCallbackWithMetadata(
            MidiFile midiFile,
            TimeSpan changeCallbackAfter,
            NoteCallback noteCallback,
            NoteCallback secondNoteCallback,
            (MidiEvent, object)[] expectedMetadata)
        {
            var playbackContext = new PlaybackContext();
            var stopwatch = playbackContext.Stopwatch;
            var tempoMap = playbackContext.TempoMap;

            var timedEvents = GetTimedEvents(midiFile);
            var outputDevice = new OutputDeviceWithMetadataRegistration();

            using (var playback = new PlaybackWithMetadataRegistration(timedEvents, tempoMap, outputDevice))
            {
                playback.NoteCallback = noteCallback;

                stopwatch.Start();
                playback.Start();

                WaitOperations.Wait(() => stopwatch.Elapsed >= changeCallbackAfter);
                playback.NoteCallback = secondNoteCallback;

                var timeout = (TimeSpan)midiFile.GetDuration<MetricTimeSpan>() + SendReceiveUtilities.MaximumEventSendReceiveDelay;
                var playbackFinished = WaitOperations.Wait(() => !playback.IsRunning, timeout);
                Assert.IsTrue(playbackFinished, "Playback not finished.");
            }

            CheckRegisteredMetadata(
                expectedMetadata: expectedMetadata,
                actualMetadata: outputDevice.Metadata.ToArray());
        }

        private void CheckNoteCallbackWithMetadata(
            MidiFile midiFile,
            TimeSpan stopAfter,
            TimeSpan stopPeriod,
            PlaybackAction setupPlayback,
            PlaybackAction afterStart,
            PlaybackAction afterStop,
            PlaybackAction afterResume,
            IEnumerable<Tuple<TimeSpan, PlaybackAction>> runningAfterResume,
            (MidiEvent, object)[] expectedMetadata)
        {
            var playbackContext = new PlaybackContext();
            var stopwatch = playbackContext.Stopwatch;
            var tempoMap = playbackContext.TempoMap;

            var timedEvents = GetTimedEvents(midiFile);
            var outputDevice = new OutputDeviceWithMetadataRegistration();

            using (var playback = new PlaybackWithMetadataRegistration(timedEvents, tempoMap, outputDevice))
            {
                playback.InterruptNotesOnStop = false;
                setupPlayback(playbackContext, playback);

                stopwatch.Start();
                playback.Start();

                afterStart(playbackContext, playback);

                WaitOperations.Wait(() => stopwatch.Elapsed >= stopAfter);
                playback.Stop();

                afterStop(playbackContext, playback);

                WaitOperations.Wait(stopPeriod);
                playback.Start();

                afterResume(playbackContext, playback);

                if (runningAfterResume != null)
                {
                    foreach (var check in runningAfterResume)
                    {
                        WaitOperations.Wait(check.Item1);
                        check.Item2(playbackContext, playback);
                    }
                }

                stopwatch.Stop();
                WaitOperations.Wait(() => !playback.IsRunning);
            }

            CheckRegisteredMetadata(
                expectedMetadata: expectedMetadata,
                actualMetadata: outputDevice.Metadata.ToArray());
        }

        private static IEnumerable<TimedEvent> GetTimedEvents(MidiFile midiFile) => midiFile
            .GetTrackChunks()
            .SelectMany((c, i) => c.GetTimedEvents().Select(e => new TimedEventWithTrackChunkIndex(e.Event, e.Time, i)))
            .OrderBy(e => e.Time);

        private static void CheckRegisteredMetadata(
            (MidiEvent, object)[] expectedMetadata,
            (MidiEvent, object)[] actualMetadata)
        {
            Assert.AreEqual(expectedMetadata.Length, actualMetadata.Length, "Count of metadata records is invalid.");

            for (var i = 0; i < expectedMetadata.Length; i++)
            {
                var expectedRecord = expectedMetadata[i];
                var actualRecord = actualMetadata[i];

                MidiAsserts.AreEqual(expectedRecord.Item1, actualRecord.Item1, false, $"Record {i}: Event is invalid.");
                Assert.AreEqual(expectedRecord.Item2, actualRecord.Item2, $"Record {i}: Metadata is invalid.");
            }
        }

        #endregion
    }
}
