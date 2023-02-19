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
        public void Sanitize_RemoveDuplicatedSetTempoEvents_EmptyFile() => Sanitize(
            midiFile: new MidiFile(),
            settings: null,
            expectedMidiFile: new MidiFile());

        [Test]
        public void Sanitize_RemoveDuplicatedSetTempoEvents_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote) { DeltaTime = 20 },
                    new TextEvent("B"))),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 20 })));

        [Test]
        public void Sanitize_RemoveDuplicatedSetTempoEvents_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote) { DeltaTime = 20 },
                    new TextEvent("B"),
                    new SetTempoEvent(),
                    new SetTempoEvent(200),
                    new SetTempoEvent(200) { DeltaTime = 30 },
                    new TextEvent("C"))),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 20 },
                    new SetTempoEvent(200),
                    new TextEvent("C") { DeltaTime = 30 })));

        [Test]
        public void Sanitize_RemoveDuplicatedSetTempoEvents_3() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote) { DeltaTime = 20 },
                    new TextEvent("B"),
                    new SetTempoEvent(),
                    new SetTempoEvent(200),
                    new SetTempoEvent(200) { DeltaTime = 30 },
                    new TextEvent("C")),
                new TrackChunk(
                    new SetTempoEvent() { DeltaTime = 10 },
                    new SetTempoEvent(200) { DeltaTime = 100 })),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 20 },
                    new SetTempoEvent(200),
                    new TextEvent("C") { DeltaTime = 30 }),
                new TrackChunk()));

        [Test]
        public void Sanitize_RemoveDuplicatedSetTempoEvents_4() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SetTempoEvent(200),
                    new SetTempoEvent(200) { DeltaTime = 30 },
                    new TextEvent("B")),
                new TrackChunk(
                    new SetTempoEvent() { DeltaTime = 10 },
                    new TextEvent("C"),
                    new SetTempoEvent(200) { DeltaTime = 100 })),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SetTempoEvent(200),
                    new SetTempoEvent(200) { DeltaTime = 30 },
                    new TextEvent("B")),
                new TrackChunk(
                    new SetTempoEvent() { DeltaTime = 10 },
                    new TextEvent("C"))));

        [Test]
        public void Sanitize_RemoveDuplicatedSetTempoEvents_False_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote) { DeltaTime = 20 },
                    new TextEvent("B"))),
            settings: new SanitizingSettings
            {
                RemoveDuplicatedSetTempoEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote) { DeltaTime = 20 },
                    new TextEvent("B"))));

        [Test]
        public void Sanitize_RemoveDuplicatedSetTempoEvents_False_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote) { DeltaTime = 20 },
                    new TextEvent("B"),
                    new SetTempoEvent(),
                    new SetTempoEvent(200),
                    new SetTempoEvent(200) { DeltaTime = 30 },
                    new TextEvent("C"))),
            settings: new SanitizingSettings
            {
                RemoveDuplicatedSetTempoEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote) { DeltaTime = 20 },
                    new TextEvent("B"),
                    new SetTempoEvent(),
                    new SetTempoEvent(200),
                    new SetTempoEvent(200) { DeltaTime = 30 },
                    new TextEvent("C"))));

        [Test]
        public void Sanitize_RemoveDuplicatedSetTempoEvents_False_3() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote) { DeltaTime = 20 },
                    new TextEvent("B"),
                    new SetTempoEvent(),
                    new SetTempoEvent(200),
                    new SetTempoEvent(200) { DeltaTime = 30 },
                    new TextEvent("C")),
                new TrackChunk(
                    new SetTempoEvent() { DeltaTime = 10 },
                    new SetTempoEvent(200) { DeltaTime = 100 })),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false,
                RemoveDuplicatedSetTempoEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SetTempoEvent(SetTempoEvent.DefaultMicrosecondsPerQuarterNote) { DeltaTime = 20 },
                    new TextEvent("B"),
                    new SetTempoEvent(),
                    new SetTempoEvent(200),
                    new SetTempoEvent(200) { DeltaTime = 30 },
                    new TextEvent("C")),
                new TrackChunk(
                    new SetTempoEvent() { DeltaTime = 10 },
                    new SetTempoEvent(200) { DeltaTime = 100 })));

        [Test]
        public void Sanitize_RemoveDuplicatedSetTempoEvents_False_4() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SetTempoEvent(200),
                    new SetTempoEvent(200) { DeltaTime = 30 },
                    new TextEvent("B")),
                new TrackChunk(
                    new SetTempoEvent() { DeltaTime = 10 },
                    new TextEvent("C"),
                    new SetTempoEvent(200) { DeltaTime = 100 })),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false,
                RemoveDuplicatedSetTempoEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SetTempoEvent(200),
                    new SetTempoEvent(200) { DeltaTime = 30 },
                    new TextEvent("B")),
                new TrackChunk(
                    new SetTempoEvent() { DeltaTime = 10 },
                    new TextEvent("C"),
                    new SetTempoEvent(200) { DeltaTime = 100 })));

        #endregion
    }
}
