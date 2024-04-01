using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class SanitizerTests
    {
        #region Test methods

        [Test]
        public void Sanitize_RemoveOrphanedNoteOnEvents_EmptyFile() => Sanitize(
            midiFile: new MidiFile(),
            settings: null,
            expectedMidiFile: new MidiFile());

        [Test]
        public void Sanitize_RemoveOrphanedNoteOnEvents_NoNoteEvents_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"))),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"))));

        [Test]
        public void Sanitize_RemoveOrphanedNoteOnEvents_NoNoteEvents_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A")),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 20 },
                    new TextEvent("B"))),
            settings: new SanitizingSettings
            {
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A")),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 20 },
                    new TextEvent("B"))));

        [Test]
        public void Sanitize_RemoveOrphanedNoteOnEvents_NoOrphaned_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOffEvent()),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 20 },
                    new TextEvent("B"))),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOffEvent()),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 20 },
                    new TextEvent("B"))));

        [Test]
        public void Sanitize_RemoveOrphanedNoteOnEvents_NoOrphaned_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 20 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 10 }),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 20 },
                    new TextEvent("A"))),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 20 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 10 }),
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 20 },
                    new TextEvent("A"))));

        [Test]
        public void Sanitize_RemoveOrphanedNoteOnEvents_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOffEvent() { DeltaTime = 20 },
                    new NoteOnEvent())),
            settings: new SanitizingSettings
            {
                RemoveOrphanedNoteOffEvents = false,
                Trim = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOffEvent() { DeltaTime = 20 })));

        [Test]
        public void Sanitize_RemoveOrphanedNoteOnEvents_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new ControlChangeEvent() { DeltaTime = 20 }),
                new TrackChunk(
                    new TextEvent("A"),
                    new NoteOnEvent() { DeltaTime = 30 },
                    new TextEvent("B"))),
            settings: new SanitizingSettings
            {
                RemoveOrphanedNoteOffEvents = false,
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 30 }),
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 30 })));

        [Test]
        public void Sanitize_RemoveOrphanedNoteOnEvents_3() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 })),
            settings: new SanitizingSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn
                },
                Trim = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new NoteOffEvent() { DeltaTime = 35 })));

        [Test]
        public void Sanitize_RemoveOrphanedNoteOnEvents_4() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new TextEvent("A") { DeltaTime = 5 },
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 })),
            settings: new SanitizingSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn
                },
                Trim = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = 15 },
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 })));

        [Test]
        public void Sanitize_RemoveOrphanedNoteOnEvents_5() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 })),
            settings: new SanitizingSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn
                },
                Trim = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 25 },
                    new NoteOffEvent() { DeltaTime = 20 })));

        [Test]
        public void Sanitize_RemoveOrphanedNoteOnEvents_False_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOffEvent() { DeltaTime = 20 },
                    new NoteOnEvent())),
            settings: new SanitizingSettings
            {
                RemoveOrphanedNoteOffEvents = false,
                RemoveOrphanedNoteOnEvents = false,
                Trim = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOffEvent() { DeltaTime = 20 },
                    new NoteOnEvent())));

        [Test]
        public void Sanitize_RemoveOrphanedNoteOnEvents_False_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new ControlChangeEvent() { DeltaTime = 20 }),
                new TrackChunk(
                    new TextEvent("A"),
                    new NoteOnEvent() { DeltaTime = 30 },
                    new TextEvent("B"))),
            settings: new SanitizingSettings
            {
                RemoveOrphanedNoteOffEvents = false,
                RemoveOrphanedNoteOnEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new ControlChangeEvent() { DeltaTime = 20 }),
                new TrackChunk(
                    new TextEvent("A"),
                    new NoteOnEvent() { DeltaTime = 30 },
                    new TextEvent("B"))));

        [Test]
        public void Sanitize_RemoveOrphanedNoteOnEvents_False_3() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 })),
            settings: new SanitizingSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn
                },
                RemoveOrphanedNoteOnEvents = false,
                Trim = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 })));

        [Test]
        public void Sanitize_RemoveOrphanedNoteOnEvents_False_4() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new TextEvent("A") { DeltaTime = 5 },
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 })),
            settings: new SanitizingSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn
                },
                RemoveOrphanedNoteOnEvents = false,
                Trim = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new TextEvent("A") { DeltaTime = 5 },
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 })));

        [Test]
        public void Sanitize_RemoveOrphanedNoteOnEvents_False_5() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 })),
            settings: new SanitizingSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn
                },
                RemoveOrphanedNoteOnEvents = false,
                Trim = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 10 },
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 })));

        #endregion
    }
}
