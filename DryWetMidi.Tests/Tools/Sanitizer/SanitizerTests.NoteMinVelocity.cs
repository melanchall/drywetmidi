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
        public void Sanitize_NoteMinVelocity_EmptyFile() => Sanitize(
            midiFile: new MidiFile(),
            settings: null,
            expectedMidiFile: new MidiFile());

        [Test]
        public void Sanitize_NoteMinVelocity_NoNotes_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk()),
            settings: null,
            expectedMidiFile: new MidiFile());

        [Test]
        public void Sanitize_NoteMinVelocity_NoNotes_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk()),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk()));

        [Test]
        public void Sanitize_NoteMinVelocity_NoNotes_3() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(),
                new TrackChunk()),
            settings: null,
            expectedMidiFile: new MidiFile());

        [Test]
        public void Sanitize_NoteMinVelocity_NoNotes_4() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(),
                new TrackChunk()),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(),
                new TrackChunk()));

        [Test]
        public void Sanitize_NoteMinVelocity_NoNotes_5() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = 20 })),
            settings: new SanitizingSettings
            {
                Trim = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = 20 })));

        [Test]
        public void Sanitize_NoteMinVelocity_NoNotes_6() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = 20 }),
                new TrackChunk(
                    new TextEvent("B"),
                    new ControlChangeEvent((SevenBitNumber)70, SevenBitNumber.MaxValue))),
            settings: new SanitizingSettings
            {
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = 20 }),
                new TrackChunk(
                    new TextEvent("B"),
                    new ControlChangeEvent((SevenBitNumber)70, SevenBitNumber.MaxValue))));

        [Test]
        public void Sanitize_NoteMinVelocity_SingleNote_AboveOrEqual([Values(10, 20)] byte noteVelocity) => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)noteVelocity),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue))),
            settings: new SanitizingSettings
            {
                NoteMinVelocity = (SevenBitNumber)10
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)noteVelocity),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue))));

        [Test]
        public void Sanitize_NoteMinVelocity_SingleNote_Below([Values(10, 29)] byte noteVelocity) => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)noteVelocity),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue))),
            settings: new SanitizingSettings
            {
                NoteMinVelocity = (SevenBitNumber)30,
                RemoveEmptyTrackChunks = false,
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk()));

        [Test]
        public void Sanitize_NoteMinVelocity_MultipleNotes_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { Velocity = (SevenBitNumber)100 },
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)100),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 20 })),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { Velocity = (SevenBitNumber)100 },
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)100),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 20 })));

        [Test]
        public void Sanitize_NoteMinVelocity_MultipleNotes_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { Velocity = (SevenBitNumber)100 },
                    new NoteOffEvent() { DeltaTime = 40 },
                    new TextEvent("A") { DeltaTime = 30 },
                    new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)15),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 10 },
                    new NoteOnEvent() { Velocity = (SevenBitNumber)100 },
                    new NoteOffEvent() { DeltaTime = 20 },
                    new TextEvent("B") { DeltaTime = 70 })),
            settings: new SanitizingSettings
            {
                NoteMinVelocity = (SevenBitNumber)20
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { Velocity = (SevenBitNumber)100 },
                    new NoteOffEvent() { DeltaTime = 40 },
                    new TextEvent("A") { DeltaTime = 30 },
                    new NoteOnEvent() { DeltaTime = 10, Velocity = (SevenBitNumber)100 },
                    new NoteOffEvent() { DeltaTime = 20 },
                    new TextEvent("B") { DeltaTime = 70 })));

        [Test]
        public void Sanitize_NoteMinVelocity_MultipleNotes_3() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { Velocity = (SevenBitNumber)100 },
                    new NoteOffEvent() { DeltaTime = 40 },
                    new TextEvent("A") { DeltaTime = 30 },
                    new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)15),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 10 },
                    new TextEvent("B") { DeltaTime = 70 },
                    new NoteOnEvent() { Velocity = (SevenBitNumber)100 },
                    new NoteOffEvent() { DeltaTime = 20 }),
                new TrackChunk(
                    new NoteOnEvent() { Velocity = (SevenBitNumber)10 },
                    new NoteOffEvent() { DeltaTime = 5 },
                    new TextEvent("A") { DeltaTime = 30 },
                    new NoteOffEvent() { DeltaTime = 20 })),
            settings: new SanitizingSettings
            {
                NoteMinVelocity = (SevenBitNumber)20
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { Velocity = (SevenBitNumber)100 },
                    new NoteOffEvent() { DeltaTime = 40 },
                    new TextEvent("A") { DeltaTime = 30 },
                    new TextEvent("B") { DeltaTime = 80 },
                    new NoteOnEvent() { Velocity = (SevenBitNumber)100 },
                    new NoteOffEvent() { DeltaTime = 20 }),
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = 35 })));

        [Test]
        public void Sanitize_NoteMinVelocity_MultipleNotes_4() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { Velocity = (SevenBitNumber)100 },
                    new NoteOffEvent() { DeltaTime = 40 },
                    new TextEvent("A") { DeltaTime = 30 },
                    new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)15),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 10 },
                    new TextEvent("B") { DeltaTime = 70 },
                    new NoteOnEvent() { Velocity = (SevenBitNumber)100 },
                    new NoteOffEvent() { DeltaTime = 20 }),
                new TrackChunk(
                    new NoteOnEvent() { Velocity = (SevenBitNumber)5 },
                    new NoteOffEvent() { DeltaTime = 5 },
                    new TextEvent("A") { DeltaTime = 30 },
                    new NoteOffEvent() { DeltaTime = 20 })),
            settings: new SanitizingSettings
            {
                NoteMinVelocity = (SevenBitNumber)20,
                RemoveOrphanedNoteOffEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { Velocity = (SevenBitNumber)100 },
                    new NoteOffEvent() { DeltaTime = 40 },
                    new TextEvent("A") { DeltaTime = 30 },
                    new TextEvent("B") { DeltaTime = 80 },
                    new NoteOnEvent() { Velocity = (SevenBitNumber)100 },
                    new NoteOffEvent() { DeltaTime = 20 }),
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = 35 },
                    new NoteOffEvent() { DeltaTime = 20 })));

        [Test]
        public void Sanitize_NoteMinVelocity_MultipleNotes_5() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { Velocity = (SevenBitNumber)5 },
                    new NoteOffEvent() { DeltaTime = 5 },
                    new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)19),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 10 }),
                new TrackChunk(
                    new NoteOnEvent() { Velocity = (SevenBitNumber)18 },
                    new NoteOffEvent() { DeltaTime = 5 })),
            settings: new SanitizingSettings
            {
                NoteMinVelocity = (SevenBitNumber)20,
                RemoveEmptyTrackChunks = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(),
                new TrackChunk()));

        [Test]
        public void Sanitize_NoteMinVelocity_MultipleNotes_6() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { Velocity = (SevenBitNumber)5 },
                    new NoteOffEvent() { DeltaTime = 5 },
                    new NoteOnEvent((SevenBitNumber)70, (SevenBitNumber)19),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 10 }),
                new TrackChunk(
                    new NoteOnEvent() { Velocity = (SevenBitNumber)18 },
                    new NoteOffEvent() { DeltaTime = 5 })),
            settings: new SanitizingSettings
            {
                NoteMinVelocity = (SevenBitNumber)20
            },
            expectedMidiFile: new MidiFile());

        #endregion
    }
}
