using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System;
using System.IO;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    public sealed partial class SanitizerTests
    {
        #region Test methods

        [Test]
        public void Sanitize_PreserveRepeatedControllerCommands(
            [Values(
                ControlName.DataIncrement,
                ControlName.DataDecrement,
                ControlName.AllSoundOff,
                ControlName.ResetAllControllers,
                ControlName.LocalControl,
                ControlName.AllNotesOff,
                ControlName.OmniModeOff,
                ControlName.OmniModeOn,
                ControlName.MonoModeOn,
                ControlName.PolyModeOn)] ControlName controlName)
        {
            var midiFile = new MidiFile(new TrackChunk(
                new ControlChangeEvent(ControlName.RegisteredParameterNumberMsb.AsSevenBitNumber(), (SevenBitNumber)0),
                new ControlChangeEvent(ControlName.RegisteredParameterNumberLsb.AsSevenBitNumber(), (SevenBitNumber)0),
                new ControlChangeEvent(controlName.AsSevenBitNumber(), (SevenBitNumber)0),
                new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)80) { DeltaTime = 10 },
                new ControlChangeEvent(controlName.AsSevenBitNumber(), (SevenBitNumber)0) { DeltaTime = 10 },
                new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)0) { DeltaTime = 10 }));

            SanitizePreservingFile(midiFile);
        }

        [Test]
        public void Sanitize_ControllerResetInvalidatesValues(
            [Values(false, true)] bool removeDuplicatedControlChangeEvents,
            [Values(false, true)] bool usePitchBend)
        {
            var midiFile = new MidiFile(new TrackChunk(
                usePitchBend
                    ? (MidiEvent)new PitchBendEvent(9000)
                    : new ControlChangeEvent(ControlName.Modulation.AsSevenBitNumber(), (SevenBitNumber)64),
                new ControlChangeEvent(ControlName.ResetAllControllers.AsSevenBitNumber(), (SevenBitNumber)0) { DeltaTime = 10 },
                usePitchBend
                    ? (MidiEvent)new PitchBendEvent(9000) { DeltaTime = 10 }
                    : new ControlChangeEvent(ControlName.Modulation.AsSevenBitNumber(), (SevenBitNumber)64) { DeltaTime = 10 }));

            SanitizePreservingFile(
                midiFile,
                new SanitizingSettings
                {
                    RemoveEventsOnUnusedChannels = false,
                    RemoveDuplicatedControlChangeEvents = removeDuplicatedControlChangeEvents
                });
        }

        [Test]
        public void Sanitize_ControllerResetOnlyInvalidatesItsChannel()
        {
            Sanitize(
                new MidiFile(new TrackChunk(
                    new ControlChangeEvent(ControlName.Modulation.AsSevenBitNumber(), (SevenBitNumber)64),
                    new PitchBendEvent(9000),
                    new ControlChangeEvent(ControlName.ResetAllControllers.AsSevenBitNumber(), (SevenBitNumber)0) { Channel = (FourBitNumber)1 },
                    new ControlChangeEvent(ControlName.Modulation.AsSevenBitNumber(), (SevenBitNumber)64),
                    new PitchBendEvent(9000))),
                new SanitizingSettings { RemoveEventsOnUnusedChannels = false },
                new MidiFile(new TrackChunk(
                    new ControlChangeEvent(ControlName.Modulation.AsSevenBitNumber(), (SevenBitNumber)64),
                    new PitchBendEvent(9000),
                    new ControlChangeEvent(ControlName.ResetAllControllers.AsSevenBitNumber(), (SevenBitNumber)0) { Channel = (FourBitNumber)1 })));
        }

        [Test]
        public void Sanitize_FilteredQuietNoteDoesNotHideAudibleDuplicate(
            [Values(false, true)] bool useSeparateTracks)
        {
            var quietTrack = new TrackChunk(
                new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)20),
                new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)0) { DeltaTime = 10 });
            var audibleTrack = new TrackChunk(
                new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)100),
                new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)0) { DeltaTime = 10 });
            var midiFile = useSeparateTracks
                ? new MidiFile(quietTrack, audibleTrack)
                : new MidiFile(new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)20),
                    new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)100),
                    new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)0) { DeltaTime = 10 },
                    new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)0)));

            Sanitize(
                midiFile,
                new SanitizingSettings { NoteMinVelocity = (SevenBitNumber)50 },
                new MidiFile(audibleTrack.Clone()));
        }

        [Test]
        public void Sanitize_RemoveNonAdjacentDuplicateNotes()
        {
            var midiFile = new MidiFile(
                CreateSanitizerNoteTrack(10),
                CreateSanitizerNoteTrack(20),
                CreateSanitizerNoteTrack(10));

            Sanitize(
                midiFile,
                null,
                new MidiFile(CreateSanitizerNoteTrack(10), CreateSanitizerNoteTrack(20)));
        }

        [Test]
        public void Sanitize_RemoveDuplicateNotesAfterOtherTrackTimes()
        {
            var midiFile = new MidiFile(
                new TrackChunk(new NoteOnEvent() { DeltaTime = 20 }, new NoteOffEvent() { DeltaTime = 10 }),
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new NoteOffEvent() { DeltaTime = 10 },
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 10 }));

            Sanitize(
                midiFile,
                null,
                new MidiFile(
                    new TrackChunk(new NoteOnEvent() { DeltaTime = 20 }, new NoteOffEvent() { DeltaTime = 10 }),
                    new TrackChunk(new NoteOnEvent() { DeltaTime = 10 }, new NoteOffEvent() { DeltaTime = 10 })));
        }

        [Test]
        public void Sanitize_PitchBendInterleavedChannels()
        {
            Sanitize(
                new MidiFile(new TrackChunk(
                    new PitchBendEvent(9000),
                    new PitchBendEvent(10000) { Channel = (FourBitNumber)1, DeltaTime = 10 },
                    new PitchBendEvent(9000) { DeltaTime = 10 },
                    new TextEvent("tail") { DeltaTime = 10 })),
                new SanitizingSettings { RemoveEventsOnUnusedChannels = false },
                new MidiFile(new TrackChunk(
                    new PitchBendEvent(9000),
                    new PitchBendEvent(10000) { Channel = (FourBitNumber)1, DeltaTime = 10 },
                    new TextEvent("tail") { DeltaTime = 20 })));
        }

        [Test]
        public void Sanitize_PreserveIndependentSequences(
            [Values(false, true)] bool completeOrphanedNotes)
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new SetTempoEvent(600000),
                    new TimeSignatureEvent(3, 4),
                    new ControlChangeEvent(ControlName.Modulation.AsSevenBitNumber(), (SevenBitNumber)64),
                    new PitchBendEvent(9000),
                    new NoteOnEvent((SevenBitNumber)0, (SevenBitNumber)80),
                    new NoteOffEvent() { DeltaTime = 10 }),
                new TrackChunk(
                    new SetTempoEvent(600000),
                    new TimeSignatureEvent(3, 4),
                    new ControlChangeEvent(ControlName.Modulation.AsSevenBitNumber(), (SevenBitNumber)64),
                    new PitchBendEvent(9000),
                    new NoteOnEvent((SevenBitNumber)0, (SevenBitNumber)80),
                    new NoteOffEvent() { DeltaTime = 10 }));

            using (var stream = new MemoryStream())
            {
                midiFile.Write(stream, MidiFileFormat.MultiSequence);
                stream.Position = 0;
                midiFile = MidiFile.Read(stream);
            }

            SanitizePreservingFile(
                midiFile,
                new SanitizingSettings
                {
                    OrphanedNoteOnEventsPolicy = completeOrphanedNotes
                        ? OrphanedNoteOnEventsPolicy.CompleteNote
                        : OrphanedNoteOnEventsPolicy.Remove
                });
        }

        [Test]
        public void Sanitize_PortScopedNotes(
            [Values(false, true)] bool hasExplicitDefaultPort)
        {
            var firstTrack = CreateSanitizerNoteTrack(10);

            if (hasExplicitDefaultPort)
                firstTrack.Events.Insert(0, new PortPrefixEvent(0));

            var secondTrack = CreateSanitizerNoteTrack(10);
            secondTrack.Events.Insert(0, new PortPrefixEvent(1));

            SanitizePreservingFile(new MidiFile(firstTrack, secondTrack));
        }

        [Test]
        public void Sanitize_PortScopedChannelValues(
            [Values(false, true)] bool usePitchBend,
            [Values(false, true)] bool useSeparateTracks)
        {
            MidiEvent CreateChannelEvent() => usePitchBend
                ? (MidiEvent)new PitchBendEvent(9000)
                : new ControlChangeEvent(ControlName.Modulation.AsSevenBitNumber(), (SevenBitNumber)64);

            var midiFile = useSeparateTracks
                ? new MidiFile(
                    new TrackChunk(new PortPrefixEvent(0), CreateChannelEvent()),
                    new TrackChunk(new PortPrefixEvent(1), CreateChannelEvent()))
                : new MidiFile(new TrackChunk(
                    new PortPrefixEvent(0),
                    CreateChannelEvent(),
                    new PortPrefixEvent(1),
                    CreateChannelEvent()));

            SanitizePreservingFile(
                midiFile,
                new SanitizingSettings { RemoveEventsOnUnusedChannels = false });
        }

        [Test]
        public void Sanitize_RemoveEventsOnUnusedPortChannel(
            [Values(
                MidiEventType.ControlChange,
                MidiEventType.PitchBend,
                MidiEventType.ChannelAftertouch,
                MidiEventType.NoteAftertouch,
                MidiEventType.ProgramChange)] MidiEventType eventType)
        {
            MidiEvent channelEvent;

            switch (eventType)
            {
                case MidiEventType.PitchBend:
                    channelEvent = new PitchBendEvent(9000);
                    break;
                case MidiEventType.ChannelAftertouch:
                    channelEvent = new ChannelAftertouchEvent((SevenBitNumber)64);
                    break;
                case MidiEventType.NoteAftertouch:
                    channelEvent = new NoteAftertouchEvent((SevenBitNumber)60, (SevenBitNumber)64);
                    break;
                case MidiEventType.ProgramChange:
                    channelEvent = new ProgramChangeEvent((SevenBitNumber)10);
                    break;
                case MidiEventType.ControlChange:
                    channelEvent = new ControlChangeEvent(ControlName.Modulation.AsSevenBitNumber(), (SevenBitNumber)64);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(eventType), eventType, "Unexpected channel event type.");
            }

            Sanitize(
                new MidiFile(
                    CreateSanitizerNoteTrack(10),
                    new TrackChunk(new PortPrefixEvent(1), channelEvent)),
                null,
                new MidiFile(
                    CreateSanitizerNoteTrack(10),
                    new TrackChunk(new PortPrefixEvent(1))));
        }

        [Test]
        public void Sanitize_DeduplicateSamePortAcrossTracks(
            [Values(0, 1, 255)] byte port,
            [Values(false, true)] bool removeDuplicatedNotes)
        {
            var firstTrack = CreateSanitizerNoteTrack(10);
            firstTrack.Events.Insert(0, new PortPrefixEvent(port));
            var secondTrack = firstTrack.Clone();
            var expectedSecondTrack = removeDuplicatedNotes
                ? new TrackChunk(new PortPrefixEvent(port))
                : secondTrack.Clone();

            Sanitize(
                new MidiFile(firstTrack, secondTrack),
                new SanitizingSettings { RemoveDuplicatedNotes = removeDuplicatedNotes },
                new MidiFile(firstTrack.Clone(), expectedSecondTrack));
        }

        [Test]
        public void Sanitize_CompleteOrphanWithoutOtherPortClipping()
        {
            Sanitize(
                new MidiFile(
                    new TrackChunk(new PortPrefixEvent(0), new NoteOnEvent()),
                    new TrackChunk(
                        new PortPrefixEvent(1),
                        new NoteOnEvent() { DeltaTime = 5 },
                        new NoteOffEvent() { DeltaTime = 5 })),
                new SanitizingSettings
                {
                    OrphanedNoteOnEventsPolicy = OrphanedNoteOnEventsPolicy.CompleteNote,
                    NoteMaxLengthForOrphanedNoteOnEvent = new MidiTimeSpan(20)
                },
                new MidiFile(
                    new TrackChunk(
                        new PortPrefixEvent(0),
                        new NoteOnEvent(),
                        new NoteOffEvent() { DeltaTime = 20 }),
                    new TrackChunk(
                        new PortPrefixEvent(1),
                        new NoteOnEvent() { DeltaTime = 5 },
                        new NoteOffEvent() { DeltaTime = 5 })));
        }

        [Test]
        public void Sanitize_PortChangeDoesNotHideNotes()
        {
            SanitizePreservingFile(new MidiFile(new TrackChunk(
                new PortPrefixEvent(0),
                new NoteOnEvent(),
                new NoteOffEvent() { DeltaTime = 10 },
                new PortPrefixEvent(1),
                new NoteOnEvent() { DeltaTime = 10 },
                new NoteOffEvent() { DeltaTime = 10 },
                new PortPrefixEvent(0),
                new NoteOnEvent() { DeltaTime = 10 },
                new NoteOffEvent() { DeltaTime = 10 })));
        }

        [Test]
        public void Sanitize_DoNotPairNotesAcrossPorts(
            [Values(false, true)] bool shouldRemoveOrphans)
        {
            var midiFile = new MidiFile(new TrackChunk(
                new NoteOnEvent(),
                new PortPrefixEvent(1),
                new NoteOffEvent() { DeltaTime = 10 }));
            var expectedMidiFile = shouldRemoveOrphans
                ? new MidiFile(new TrackChunk(new PortPrefixEvent(1)))
                : midiFile.Clone();

            Sanitize(
                midiFile,
                new SanitizingSettings
                {
                    OrphanedNoteOnEventsPolicy = shouldRemoveOrphans
                        ? OrphanedNoteOnEventsPolicy.Remove
                        : OrphanedNoteOnEventsPolicy.Ignore,
                    RemoveOrphanedNoteOffEvents = shouldRemoveOrphans,
                    NoteMinLength = new MidiTimeSpan(20)
                },
                expectedMidiFile);
        }

        [Test]
        public void Sanitize_CompleteOrphanOnItsOriginalPort()
        {
            Sanitize(
                new MidiFile(new TrackChunk(
                    new NoteOnEvent(),
                    new PortPrefixEvent(1) { DeltaTime = 5 },
                    new TextEvent("tail") { DeltaTime = 25 })),
                new SanitizingSettings
                {
                    OrphanedNoteOnEventsPolicy = OrphanedNoteOnEventsPolicy.CompleteNote,
                    NoteMaxLengthForOrphanedNoteOnEvent = new MidiTimeSpan(20)
                },
                new MidiFile(new TrackChunk(
                    new NoteOnEvent(),
                    new PortPrefixEvent(1) { DeltaTime = 5 },
                    new PortPrefixEvent(0) { DeltaTime = 15 },
                    new NoteOffEvent(),
                    new PortPrefixEvent(1),
                    new TextEvent("tail") { DeltaTime = 10 })));
        }

        [Test]
        public void Sanitize_IndependentSequenceNoteLengthsUseLocalTempo()
        {
            Sanitize(
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(1000000),
                        new NoteOnEvent(),
                        new NoteOffEvent() { DeltaTime = 100 }),
                    new TrackChunk(
                        new SetTempoEvent(250000),
                        new NoteOnEvent(),
                        new NoteOffEvent() { DeltaTime = 100 }))
                {
                    OriginalFormat = MidiFileFormat.MultiSequence,
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(100)
                },
                new SanitizingSettings { NoteMinLength = new MetricTimeSpan(0, 0, 0, 500) },
                new MidiFile(
                    new TrackChunk(
                        new SetTempoEvent(1000000),
                        new NoteOnEvent(),
                        new NoteOffEvent() { DeltaTime = 100 }),
                    new TrackChunk(new SetTempoEvent(250000)))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(100)
                });
        }

        [Test]
        public void Sanitize_IndependentSequenceUnusedChannels()
        {
            Sanitize(
                new MidiFile(
                    CreateSanitizerNoteTrack(10),
                    new TrackChunk(new ControlChangeEvent()))
                {
                    OriginalFormat = MidiFileFormat.MultiSequence
                },
                null,
                new MidiFile(CreateSanitizerNoteTrack(10)));
        }

        [Test]
        public void Sanitize_IndependentSequenceOrphanCompletion()
        {
            Sanitize(
                new MidiFile(
                    new TrackChunk(new NoteOnEvent()),
                    new TrackChunk(new NoteOnEvent() { DeltaTime = 5 }, new NoteOffEvent() { DeltaTime = 5 }))
                {
                    OriginalFormat = MidiFileFormat.MultiSequence
                },
                new SanitizingSettings
                {
                    OrphanedNoteOnEventsPolicy = OrphanedNoteOnEventsPolicy.CompleteNote,
                    NoteMaxLengthForOrphanedNoteOnEvent = new MidiTimeSpan(20)
                },
                new MidiFile(
                    CreateSanitizerNoteTrack(20),
                    new TrackChunk(new NoteOnEvent() { DeltaTime = 5 }, new NoteOffEvent() { DeltaTime = 5 })));
        }

        [Test]
        public void Sanitize_IndependentSequenceCompletionUsesLocalTempo()
        {
            Sanitize(
                new MidiFile(
                    new TrackChunk(new SetTempoEvent(1000000), new NoteOnEvent()),
                    new TrackChunk(new SetTempoEvent(250000), new NoteOnEvent()))
                {
                    OriginalFormat = MidiFileFormat.MultiSequence,
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(100)
                },
                new SanitizingSettings
                {
                    OrphanedNoteOnEventsPolicy = OrphanedNoteOnEventsPolicy.CompleteNote,
                    NoteMaxLengthForOrphanedNoteOnEvent = new MetricTimeSpan(0, 0, 0, 500)
                },
                new MidiFile(
                    new TrackChunk(new SetTempoEvent(1000000), new NoteOnEvent(), new NoteOffEvent() { DeltaTime = 50 }),
                    new TrackChunk(new SetTempoEvent(250000), new NoteOnEvent(), new NoteOffEvent() { DeltaTime = 200 }))
                {
                    TimeDivision = new TicksPerQuarterNoteTimeDivision(100)
                });
        }

        [Test]
        public void Sanitize_IndependentSequenceTrimming()
        {
            Sanitize(
                new MidiFile(
                    new TrackChunk(new TextEvent("first") { DeltaTime = 10 }),
                    new TrackChunk(new TextEvent("second") { DeltaTime = 20 }))
                {
                    OriginalFormat = MidiFileFormat.MultiSequence
                },
                new SanitizingSettings { Trim = true },
                new MidiFile(
                    new TrackChunk(new TextEvent("first")),
                    new TrackChunk(new TextEvent("second"))));
        }

        [Test]
        public void Sanitize_ResetOnOtherPortDoesNotInvalidateChannelValues()
        {
            Sanitize(
                new MidiFile(new TrackChunk(
                    new PitchBendEvent(9000),
                    new ControlChangeEvent(ControlName.Modulation.AsSevenBitNumber(), (SevenBitNumber)64),
                    new PortPrefixEvent(1),
                    new ControlChangeEvent(ControlName.ResetAllControllers.AsSevenBitNumber(), (SevenBitNumber)0),
                    new PortPrefixEvent(0),
                    new PitchBendEvent(9000),
                    new ControlChangeEvent(ControlName.Modulation.AsSevenBitNumber(), (SevenBitNumber)64))),
                new SanitizingSettings { RemoveEventsOnUnusedChannels = false },
                new MidiFile(new TrackChunk(
                    new PitchBendEvent(9000),
                    new ControlChangeEvent(ControlName.Modulation.AsSevenBitNumber(), (SevenBitNumber)64),
                    new PortPrefixEvent(1),
                    new ControlChangeEvent(ControlName.ResetAllControllers.AsSevenBitNumber(), (SevenBitNumber)0),
                    new PortPrefixEvent(0))));
        }

        [Test]
        public void Sanitize_PreservePressureOnDifferentChannels()
        {
            SanitizePreservingFile(
                new MidiFile(new TrackChunk(
                    new ChannelAftertouchEvent((SevenBitNumber)64),
                    new ChannelAftertouchEvent((SevenBitNumber)64) { Channel = (FourBitNumber)1 },
                    new NoteAftertouchEvent((SevenBitNumber)60, (SevenBitNumber)64),
                    new NoteAftertouchEvent((SevenBitNumber)61, (SevenBitNumber)64),
                    new NoteAftertouchEvent((SevenBitNumber)60, (SevenBitNumber)64) { Channel = (FourBitNumber)1 })),
                new SanitizingSettings { RemoveEventsOnUnusedChannels = false });
        }

        [Test]
        public void Sanitize_CompleteZeroLengthOrphans(
            [Values(0, 10)] long startTime,
            [Values(false, true)] bool hasTrailingEvent,
            [Values] NoteStartDetectionPolicy noteStartDetectionPolicy,
            [Values(false, true)] bool removeOrphanedNoteOffEvents)
        {
            var trackChunk = new TrackChunk(
                new NoteOnEvent() { DeltaTime = startTime },
                new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)80));
            var expectedTrack = new TrackChunk(
                new NoteOnEvent() { DeltaTime = startTime },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)80),
                new NoteOffEvent((SevenBitNumber)70, (SevenBitNumber)0));

            if (hasTrailingEvent)
            {
                trackChunk.Events.Add(new TextEvent("tail") { DeltaTime = 100 });
                expectedTrack.Events.Add(new TextEvent("tail") { DeltaTime = 100 });
            }

            Sanitize(
                new MidiFile(trackChunk),
                new SanitizingSettings
                {
                    OrphanedNoteOnEventsPolicy = OrphanedNoteOnEventsPolicy.CompleteNote,
                    NoteMaxLengthForOrphanedNoteOnEvent = new MidiTimeSpan(0),
                    RemoveOrphanedNoteOffEvents = removeOrphanedNoteOffEvents,
                    NoteDetectionSettings = new NoteDetectionSettings
                    {
                        NoteStartDetectionPolicy = noteStartDetectionPolicy
                    }
                },
                new MidiFile(expectedTrack));
        }

        [Test]
        public void Sanitize_CompleteZeroLengthOrphans_PreservesOverlappingNotePairing()
        {
            Sanitize(
                new MidiFile(new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)100),
                    new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)20) { DeltaTime = 10 },
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 10 })),
                new SanitizingSettings
                {
                    OrphanedNoteOnEventsPolicy = OrphanedNoteOnEventsPolicy.CompleteNote,
                    NoteMaxLengthForOrphanedNoteOnEvent = new MidiTimeSpan(0),
                    NoteMinVelocity = (SevenBitNumber)50,
                    NoteDetectionSettings = new NoteDetectionSettings
                    {
                        NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn
                    }
                },
                new MidiFile(new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)100),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 20 })));
        }

        #endregion

        #region Private methods

        private static TrackChunk CreateSanitizerNoteTrack(long length)
        {
            return new TrackChunk(
                new NoteOnEvent(),
                new NoteOffEvent() { DeltaTime = length });
        }

        private static void SanitizePreservingFile(
            MidiFile midiFile,
            SanitizingSettings settings = null)
        {
            var expectedMidiFile = midiFile.Clone();

            midiFile.Sanitize(settings);

            MidiAsserts.AreEqual(expectedMidiFile, midiFile, false, "Sanitizing changed meaningful events.");
        }

        #endregion
    }
}
