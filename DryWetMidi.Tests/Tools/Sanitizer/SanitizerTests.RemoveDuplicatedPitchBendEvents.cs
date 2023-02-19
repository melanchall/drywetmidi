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
        public void Sanitize_RemoveDuplicatedPitchBendEvents_EmptyFile() => Sanitize(
            midiFile: new MidiFile(),
            settings: null,
            expectedMidiFile: new MidiFile());

        [Test]
        public void Sanitize_RemoveDuplicatedPitchBendEvents_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 20 },
                    new TextEvent("B"))),
            settings: new SanitizingSettings
            {
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 20 },
                    new TextEvent("B"))));

        [Test]
        public void Sanitize_RemoveDuplicatedPitchBendEvents_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 20 },
                    new TextEvent("B"),
                    new PitchBendEvent(),
                    new PitchBendEvent(200),
                    new PitchBendEvent(200) { DeltaTime = 30 },
                    new TextEvent("C"))),
            settings: new SanitizingSettings
            {
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 20 },
                    new TextEvent("B"),
                    new PitchBendEvent(200),
                    new TextEvent("C") { DeltaTime = 30 })));

        [Test]
        public void Sanitize_RemoveDuplicatedPitchBendEvents_3() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 20 },
                    new TextEvent("B"),
                    new PitchBendEvent() { Channel = (FourBitNumber)3 },
                    new PitchBendEvent() { Channel = (FourBitNumber)3 },
                    new PitchBendEvent(200),
                    new PitchBendEvent(200) { DeltaTime = 30 },
                    new TextEvent("C")),
                new TrackChunk(
                    new PitchBendEvent() { DeltaTime = 10 },
                    new PitchBendEvent(200) { DeltaTime = 100 })),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false,
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 20 },
                    new PitchBendEvent() { Channel = (FourBitNumber)3 },
                    new PitchBendEvent(200),
                    new TextEvent("C") { DeltaTime = 30 }),
                new TrackChunk(
                    new PitchBendEvent() { DeltaTime = 10 })));

        [Test]
        public void Sanitize_RemoveDuplicatedPitchBendEvents_4() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent(200),
                    new PitchBendEvent(200) { DeltaTime = 30 },
                    new TextEvent("B")),
                new TrackChunk(
                    new TextEvent("C"),
                    new PitchBendEvent(200) { DeltaTime = 100 })),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false,
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent(200),
                    new TextEvent("B") { DeltaTime = 30 }),
                new TrackChunk(
                    new TextEvent("C"))));

        [Test]
        public void Sanitize_RemoveDuplicatedPitchBendEvents_False_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 20 },
                    new TextEvent("B"))),
            settings: new SanitizingSettings
            {
                RemoveDuplicatedPitchBendEvents = false,
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 20 },
                    new TextEvent("B"))));

        [Test]
        public void Sanitize_RemoveDuplicatedPitchBendEvents_False_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 20 },
                    new TextEvent("B"),
                    new PitchBendEvent(),
                    new PitchBendEvent(200),
                    new PitchBendEvent(200) { DeltaTime = 30 },
                    new TextEvent("C"))),
            settings: new SanitizingSettings
            {
                RemoveDuplicatedPitchBendEvents = false,
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 20 },
                    new TextEvent("B"),
                    new PitchBendEvent(),
                    new PitchBendEvent(200),
                    new PitchBendEvent(200) { DeltaTime = 30 },
                    new TextEvent("C"))));

        [Test]
        public void Sanitize_RemoveDuplicatedPitchBendEvents_False_3() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 20 },
                    new TextEvent("B"),
                    new PitchBendEvent() { Channel = (FourBitNumber)3 },
                    new PitchBendEvent() { Channel = (FourBitNumber)3 },
                    new PitchBendEvent(200),
                    new PitchBendEvent(200) { DeltaTime = 30 },
                    new TextEvent("C")),
                new TrackChunk(
                    new PitchBendEvent() { DeltaTime = 10 },
                    new PitchBendEvent(200) { DeltaTime = 100 })),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false,
                RemoveDuplicatedPitchBendEvents = false,
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent() { DeltaTime = 20 },
                    new TextEvent("B"),
                    new PitchBendEvent() { Channel = (FourBitNumber)3 },
                    new PitchBendEvent() { Channel = (FourBitNumber)3 },
                    new PitchBendEvent(200),
                    new PitchBendEvent(200) { DeltaTime = 30 },
                    new TextEvent("C")),
                new TrackChunk(
                    new PitchBendEvent() { DeltaTime = 10 },
                    new PitchBendEvent(200) { DeltaTime = 100 })));

        [Test]
        public void Sanitize_RemoveDuplicatedPitchBendEvents_False_4() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent(200),
                    new PitchBendEvent(200) { DeltaTime = 30 },
                    new TextEvent("B")),
                new TrackChunk(
                    new TextEvent("C"),
                    new PitchBendEvent(200) { DeltaTime = 100 })),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false,
                RemoveDuplicatedPitchBendEvents = false,
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new PitchBendEvent(200),
                    new PitchBendEvent(200) { DeltaTime = 30 },
                    new TextEvent("B")),
                new TrackChunk(
                    new TextEvent("C"),
                    new PitchBendEvent(200) { DeltaTime = 100 })));

        #endregion
    }
}
