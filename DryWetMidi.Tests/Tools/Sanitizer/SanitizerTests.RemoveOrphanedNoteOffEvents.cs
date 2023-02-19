using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class SanitizerTests
    {
        #region Test methods

        [Test]
        public void Sanitize_RemoveOrphanedNoteOffEvents_EmptyFile() => Sanitize(
            midiFile: new MidiFile(),
            settings: null,
            expectedMidiFile: new MidiFile());

        [Test]
        public void Sanitize_RemoveOrphanedNoteOffEvents_NoNoteEvents_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"))),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"))));

        [Test]
        public void Sanitize_RemoveOrphanedNoteOffEvents_NoNoteEvents_2() => Sanitize(
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
        public void Sanitize_RemoveOrphanedNoteOffEvents_NoOrphaned_1() => Sanitize(
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
        public void Sanitize_RemoveOrphanedNoteOffEvents_NoOrphaned_2() => Sanitize(
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
        public void Sanitize_RemoveOrphanedNoteOffEvents_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOffEvent() { DeltaTime = 20 },
                    new NoteOnEvent())),
            settings: new SanitizingSettings
            {
                RemoveOrphanedNoteOnEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 20 })));

        [Test]
        public void Sanitize_RemoveOrphanedNoteOffEvents_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOffEvent() { DeltaTime = 10 },
                    new ControlChangeEvent() { DeltaTime = 20 }),
                new TrackChunk(
                    new TextEvent("A"),
                    new NoteOffEvent() { DeltaTime = 30 },
                    new TextEvent("B"))),
            settings: new SanitizingSettings
            {
                RemoveOrphanedNoteOnEvents = false,
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent() { DeltaTime = 30 }),
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 30 })));

        [Test]
        public void Sanitize_RemoveOrphanedNoteOffEvents_3() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 },
                    new NoteOffEvent() { DeltaTime = 25 },
                    new NoteOffEvent() { DeltaTime = 30 })),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 })));

        [Test]
        public void Sanitize_RemoveOrphanedNoteOffEvents_4() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 15 },
                    new TextEvent("A") { DeltaTime = 5 },
                    new NoteOffEvent() { DeltaTime = 20 },
                    new TextEvent("B") { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 25 },
                    new NoteOffEvent() { DeltaTime = 30 },
                    new TextEvent("C") { DeltaTime = 40 })),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 15 },
                    new TextEvent("A") { DeltaTime = 5 },
                    new NoteOffEvent() { DeltaTime = 20 },
                    new TextEvent("B") { DeltaTime = 15 },
                    new TextEvent("C") { DeltaTime = 95 })));

        [Test]
        public void Sanitize_RemoveOrphanedNoteOffEvents_False_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOffEvent() { DeltaTime = 20 },
                    new NoteOnEvent())),
            settings: new SanitizingSettings
            {
                RemoveOrphanedNoteOnEvents = false,
                RemoveOrphanedNoteOffEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOffEvent() { DeltaTime = 20 },
                    new NoteOnEvent())));

        [Test]
        public void Sanitize_RemoveOrphanedNoteOffEvents_False_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOffEvent() { DeltaTime = 10 },
                    new ControlChangeEvent() { DeltaTime = 20 }),
                new TrackChunk(
                    new TextEvent("A"),
                    new NoteOffEvent() { DeltaTime = 30 },
                    new TextEvent("B"))),
            settings: new SanitizingSettings
            {
                RemoveOrphanedNoteOnEvents = false,
                RemoveOrphanedNoteOffEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOffEvent() { DeltaTime = 10 },
                    new ControlChangeEvent() { DeltaTime = 20 }),
                new TrackChunk(
                    new TextEvent("A"),
                    new NoteOffEvent() { DeltaTime = 30 },
                    new TextEvent("B"))));

        [Test]
        public void Sanitize_RemoveOrphanedNoteOffEvents_False_3() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 },
                    new NoteOffEvent() { DeltaTime = 25 },
                    new NoteOffEvent() { DeltaTime = 30 })),
            settings: new SanitizingSettings
            {
                RemoveOrphanedNoteOffEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 20 },
                    new NoteOffEvent() { DeltaTime = 25 },
                    new NoteOffEvent() { DeltaTime = 30 })));

        [Test]
        public void Sanitize_RemoveOrphanedNoteOffEvents_False_4() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 15 },
                    new TextEvent("A") { DeltaTime = 5 },
                    new NoteOffEvent() { DeltaTime = 20 },
                    new TextEvent("B") { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 25 },
                    new NoteOffEvent() { DeltaTime = 30 },
                    new TextEvent("C") { DeltaTime = 40 })),
            settings: new SanitizingSettings
            {
                RemoveOrphanedNoteOffEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 15 },
                    new TextEvent("A") { DeltaTime = 5 },
                    new NoteOffEvent() { DeltaTime = 20 },
                    new TextEvent("B") { DeltaTime = 15 },
                    new NoteOffEvent() { DeltaTime = 25 },
                    new NoteOffEvent() { DeltaTime = 30 },
                    new TextEvent("C") { DeltaTime = 40 })));

        #endregion
    }
}
