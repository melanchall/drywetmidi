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
        public void Sanitize_RemoveDuplicatedTimeSignatureEvents_EmptyFile() => Sanitize(
            midiFile: new MidiFile(),
            settings: null,
            expectedMidiFile: new MidiFile());

        [Test]
        public void Sanitize_RemoveDuplicatedTimeSignatureEvents_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TimeSignatureEvent(TimeSignatureEvent.DefaultNumerator, TimeSignatureEvent.DefaultDenominator) { DeltaTime = 20 },
                    new TextEvent("B"))),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 20 })));

        [Test]
        public void Sanitize_RemoveDuplicatedTimeSignatureEvents_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TimeSignatureEvent(TimeSignatureEvent.DefaultNumerator, TimeSignatureEvent.DefaultDenominator) { DeltaTime = 20 },
                    new TextEvent("B"),
                    new TimeSignatureEvent(),
                    new TimeSignatureEvent(3, 4),
                    new TimeSignatureEvent(3, 4) { DeltaTime = 30 },
                    new TextEvent("C"))),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 20 },
                    new TimeSignatureEvent(3, 4),
                    new TextEvent("C") { DeltaTime = 30 })));

        [Test]
        public void Sanitize_RemoveDuplicatedTimeSignatureEvents_3() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TimeSignatureEvent(TimeSignatureEvent.DefaultNumerator, TimeSignatureEvent.DefaultDenominator) { DeltaTime = 20 },
                    new TextEvent("B"),
                    new TimeSignatureEvent(),
                    new TimeSignatureEvent(TimeSignatureEvent.DefaultNumerator, TimeSignatureEvent.DefaultDenominator, 100, TimeSignatureEvent.DefaultThirtySecondNotesPerBeat),
                    new TimeSignatureEvent(TimeSignatureEvent.DefaultNumerator, TimeSignatureEvent.DefaultDenominator, 100, TimeSignatureEvent.DefaultThirtySecondNotesPerBeat) { DeltaTime = 30 },
                    new TextEvent("C")),
                new TrackChunk(
                    new TimeSignatureEvent() { DeltaTime = 10 },
                    new TimeSignatureEvent(TimeSignatureEvent.DefaultNumerator, TimeSignatureEvent.DefaultDenominator, 100, TimeSignatureEvent.DefaultThirtySecondNotesPerBeat) { DeltaTime = 100 })),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 20 },
                    new TimeSignatureEvent(TimeSignatureEvent.DefaultNumerator, TimeSignatureEvent.DefaultDenominator, 100, TimeSignatureEvent.DefaultThirtySecondNotesPerBeat),
                    new TextEvent("C") { DeltaTime = 30 }),
                new TrackChunk()));

        [Test]
        public void Sanitize_RemoveDuplicatedTimeSignatureEvents_4() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TimeSignatureEvent(5, 8),
                    new TimeSignatureEvent(5, 8) { DeltaTime = 30 },
                    new TextEvent("B")),
                new TrackChunk(
                    new TimeSignatureEvent() { DeltaTime = 10 },
                    new TextEvent("C"),
                    new TimeSignatureEvent(5, 8) { DeltaTime = 100 })),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TimeSignatureEvent(5, 8),
                    new TimeSignatureEvent(5, 8) { DeltaTime = 30 },
                    new TextEvent("B")),
                new TrackChunk(
                    new TimeSignatureEvent() { DeltaTime = 10 },
                    new TextEvent("C"))));

        [Test]
        public void Sanitize_RemoveDuplicatedTimeSignatureEvents_False_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TimeSignatureEvent(TimeSignatureEvent.DefaultNumerator, TimeSignatureEvent.DefaultDenominator) { DeltaTime = 20 },
                    new TextEvent("B"))),
            settings: new SanitizingSettings
            {
                RemoveDuplicatedTimeSignatureEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TimeSignatureEvent(TimeSignatureEvent.DefaultNumerator, TimeSignatureEvent.DefaultDenominator) { DeltaTime = 20 },
                    new TextEvent("B"))));

        [Test]
        public void Sanitize_RemoveDuplicatedTimeSignatureEvents_False_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TimeSignatureEvent(TimeSignatureEvent.DefaultNumerator, TimeSignatureEvent.DefaultDenominator) { DeltaTime = 20 },
                    new TextEvent("B"),
                    new TimeSignatureEvent(),
                    new TimeSignatureEvent(3, 4),
                    new TimeSignatureEvent(3, 4) { DeltaTime = 30 },
                    new TextEvent("C"))),
            settings: new SanitizingSettings
            {
                RemoveDuplicatedTimeSignatureEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TimeSignatureEvent(TimeSignatureEvent.DefaultNumerator, TimeSignatureEvent.DefaultDenominator) { DeltaTime = 20 },
                    new TextEvent("B"),
                    new TimeSignatureEvent(),
                    new TimeSignatureEvent(3, 4),
                    new TimeSignatureEvent(3, 4) { DeltaTime = 30 },
                    new TextEvent("C"))));

        [Test]
        public void Sanitize_RemoveDuplicatedTimeSignatureEvents_False_3() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TimeSignatureEvent(TimeSignatureEvent.DefaultNumerator, TimeSignatureEvent.DefaultDenominator) { DeltaTime = 20 },
                    new TextEvent("B"),
                    new TimeSignatureEvent(),
                    new TimeSignatureEvent(TimeSignatureEvent.DefaultNumerator, TimeSignatureEvent.DefaultDenominator, 100, TimeSignatureEvent.DefaultThirtySecondNotesPerBeat),
                    new TimeSignatureEvent(TimeSignatureEvent.DefaultNumerator, TimeSignatureEvent.DefaultDenominator, 100, TimeSignatureEvent.DefaultThirtySecondNotesPerBeat) { DeltaTime = 30 },
                    new TextEvent("C")),
                new TrackChunk(
                    new TimeSignatureEvent() { DeltaTime = 10 },
                    new TimeSignatureEvent(TimeSignatureEvent.DefaultNumerator, TimeSignatureEvent.DefaultDenominator, 100, TimeSignatureEvent.DefaultThirtySecondNotesPerBeat) { DeltaTime = 100 })),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false,
                RemoveDuplicatedTimeSignatureEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TimeSignatureEvent(TimeSignatureEvent.DefaultNumerator, TimeSignatureEvent.DefaultDenominator) { DeltaTime = 20 },
                    new TextEvent("B"),
                    new TimeSignatureEvent(),
                    new TimeSignatureEvent(TimeSignatureEvent.DefaultNumerator, TimeSignatureEvent.DefaultDenominator, 100, TimeSignatureEvent.DefaultThirtySecondNotesPerBeat),
                    new TimeSignatureEvent(TimeSignatureEvent.DefaultNumerator, TimeSignatureEvent.DefaultDenominator, 100, TimeSignatureEvent.DefaultThirtySecondNotesPerBeat) { DeltaTime = 30 },
                    new TextEvent("C")),
                new TrackChunk(
                    new TimeSignatureEvent() { DeltaTime = 10 },
                    new TimeSignatureEvent(TimeSignatureEvent.DefaultNumerator, TimeSignatureEvent.DefaultDenominator, 100, TimeSignatureEvent.DefaultThirtySecondNotesPerBeat) { DeltaTime = 100 })));

        [Test]
        public void Sanitize_RemoveDuplicatedTimeSignatureEvents_False_4() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TimeSignatureEvent(5, 8),
                    new TimeSignatureEvent(5, 8) { DeltaTime = 30 },
                    new TextEvent("B")),
                new TrackChunk(
                    new TimeSignatureEvent() { DeltaTime = 10 },
                    new TextEvent("C"),
                    new TimeSignatureEvent(5, 8) { DeltaTime = 100 })),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false,
                RemoveDuplicatedTimeSignatureEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TimeSignatureEvent(5, 8),
                    new TimeSignatureEvent(5, 8) { DeltaTime = 30 },
                    new TextEvent("B")),
                new TrackChunk(
                    new TimeSignatureEvent() { DeltaTime = 10 },
                    new TextEvent("C"),
                    new TimeSignatureEvent(5, 8) { DeltaTime = 100 })));

        #endregion
    }
}
