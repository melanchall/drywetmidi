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
        public void Sanitize_RemoveDuplicatedSequenceTrackNameEvents_EmptyFile() => Sanitize(
            midiFile: new MidiFile(),
            settings: null,
            expectedMidiFile: new MidiFile());

        [Test]
        public void Sanitize_RemoveDuplicatedSequenceTrackNameEvents_EmptyTrackChunks() => Sanitize(
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
        public void Sanitize_RemoveDuplicatedSequenceTrackNameEvents_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new SequenceTrackNameEvent("Name"),
                    new TextEvent("A"))),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new SequenceTrackNameEvent("Name"),
                    new TextEvent("A"))));

        [Test]
        public void Sanitize_RemoveDuplicatedSequenceTrackNameEvents_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"))),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"))));

        [Test]
        public void Sanitize_RemoveDuplicatedSequenceTrackNameEvents_3() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("C"))),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"),
                    new TextEvent("C") { DeltaTime = 10 })));

        [Test]
        public void Sanitize_RemoveDuplicatedSequenceTrackNameEvents_4() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"),
                    new SequenceTrackNameEvent("SomeName") { DeltaTime = 34 },
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("C"))),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"),
                    new SequenceTrackNameEvent("SomeName") { DeltaTime = 34 },
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("C"))));

        [Test]
        public void Sanitize_RemoveDuplicatedSequenceTrackNameEvents_5() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"),
                    new SequenceTrackNameEvent("SomeName") { DeltaTime = 34 },
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new SequenceTrackNameEvent("SomeName") { DeltaTime = 5 },
                    new TextEvent("C"))),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"),
                    new SequenceTrackNameEvent("SomeName") { DeltaTime = 34 },
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new SequenceTrackNameEvent("SomeName") { DeltaTime = 5 },
                    new TextEvent("C"))));

        [Test]
        public void Sanitize_RemoveDuplicatedSequenceTrackNameEvents_6() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"),
                    new SequenceTrackNameEvent("SomeName") { DeltaTime = 34 },
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new SequenceTrackNameEvent("SomeName") { DeltaTime = 5 },
                    new TextEvent("C")),
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new SequenceTrackNameEvent("Name") { DeltaTime = 5 },
                    new TextEvent("B"),
                    new TextEvent("C"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 3 })),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"),
                    new SequenceTrackNameEvent("SomeName") { DeltaTime = 34 },
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new SequenceTrackNameEvent("SomeName") { DeltaTime = 5 },
                    new TextEvent("C")),
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B") { DeltaTime = 5 },
                    new TextEvent("C"))));

        [Test]
        public void Sanitize_RemoveDuplicatedSequenceTrackNameEvents_7() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("C"))),
            settings: new SanitizingSettings
            {
                RemoveEventsOnUnusedChannels = false,
                RemoveDuplicatedPitchBendEvents = false,
                RemoveDuplicatedSetTempoEvents = false,
                RemoveDuplicatedTimeSignatureEvents = false,
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"),
                    new TextEvent("C") { DeltaTime = 10 })));

        [Test]
        public void Sanitize_RemoveDuplicatedSequenceTrackNameEvents_8(
            [Values(0, 100)] long firstDeltaTime,
            [Values(0, 100)] long secondDeltaTime) => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new SequenceTrackNameEvent("Name") { DeltaTime = firstDeltaTime }),
                new TrackChunk(
                    new SequenceTrackNameEvent("Name") { DeltaTime = secondDeltaTime })),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new SequenceTrackNameEvent("Name") { DeltaTime = firstDeltaTime }),
                new TrackChunk(
                    new SequenceTrackNameEvent("Name") { DeltaTime = secondDeltaTime })));

        [Test]
        public void Sanitize_RemoveDuplicatedSequenceTrackNameEvents_9(
            [Values(0, 100)] long firstDeltaTime,
            [Values(0, 100)] long secondDeltaTime,
            [Values(0, 100)] long duplicatedEventDeltaTime) => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new SequenceTrackNameEvent("Name") { DeltaTime = firstDeltaTime },
                    new SequenceTrackNameEvent("Name") { DeltaTime = firstDeltaTime + duplicatedEventDeltaTime }),
                new TrackChunk(
                    new SequenceTrackNameEvent("Name") { DeltaTime = secondDeltaTime },
                    new SequenceTrackNameEvent("Name") { DeltaTime = secondDeltaTime + duplicatedEventDeltaTime })),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new SequenceTrackNameEvent("Name") { DeltaTime = firstDeltaTime }),
                new TrackChunk(
                    new SequenceTrackNameEvent("Name") { DeltaTime = secondDeltaTime })));

        [Test]
        public void Sanitize_DontRemoveDuplicatedSequenceTrackNameEvents_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new SequenceTrackNameEvent("Name"),
                    new TextEvent("A"))),
            settings: new SanitizingSettings
            {
                RemoveDuplicatedSequenceTrackNameEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new SequenceTrackNameEvent("Name"),
                    new TextEvent("A"))));

        [Test]
        public void Sanitize_DontRemoveDuplicatedSequenceTrackNameEvents_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"))),
            settings: new SanitizingSettings
            {
                RemoveDuplicatedSequenceTrackNameEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"))));

        [Test]
        public void Sanitize_DontRemoveDuplicatedSequenceTrackNameEvents_3() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("C"))),
            settings: new SanitizingSettings
            {
                RemoveDuplicatedSequenceTrackNameEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("C"))));

        [Test]
        public void Sanitize_DontRemoveDuplicatedSequenceTrackNameEvents_4() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"),
                    new SequenceTrackNameEvent("SomeName") { DeltaTime = 34 },
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("C"))),
            settings: new SanitizingSettings
            {
                RemoveDuplicatedSequenceTrackNameEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"),
                    new SequenceTrackNameEvent("SomeName") { DeltaTime = 34 },
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("C"))));

        [Test]
        public void Sanitize_DontRemoveDuplicatedSequenceTrackNameEvents_5() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"),
                    new SequenceTrackNameEvent("SomeName") { DeltaTime = 34 },
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new SequenceTrackNameEvent("SomeName") { DeltaTime = 5 },
                    new TextEvent("C"))),
            settings: new SanitizingSettings
            {
                RemoveDuplicatedSequenceTrackNameEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"),
                    new SequenceTrackNameEvent("SomeName") { DeltaTime = 34 },
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new SequenceTrackNameEvent("SomeName") { DeltaTime = 5 },
                    new TextEvent("C"))));

        [Test]
        public void Sanitize_DontRemoveDuplicatedSequenceTrackNameEvents_6() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"),
                    new SequenceTrackNameEvent("SomeName") { DeltaTime = 34 },
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new SequenceTrackNameEvent("SomeName") { DeltaTime = 5 },
                    new TextEvent("C")),
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new SequenceTrackNameEvent("Name") { DeltaTime = 5 },
                    new TextEvent("B"),
                    new TextEvent("C"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 3 })),
            settings: new SanitizingSettings
            {
                RemoveDuplicatedSequenceTrackNameEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"),
                    new SequenceTrackNameEvent("SomeName") { DeltaTime = 34 },
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new SequenceTrackNameEvent("SomeName") { DeltaTime = 5 },
                    new TextEvent("C")),
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new SequenceTrackNameEvent("Name") { DeltaTime = 5 },
                    new TextEvent("B"),
                    new TextEvent("C"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 3 })));

        [Test]
        public void Sanitize_DontRemoveDuplicatedSequenceTrackNameEvents_7() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("C"))),
            settings: new SanitizingSettings
            {
                RemoveEventsOnUnusedChannels = false,
                RemoveDuplicatedPitchBendEvents = false,
                RemoveDuplicatedSetTempoEvents = false,
                RemoveDuplicatedTimeSignatureEvents = false,
                RemoveDuplicatedSequenceTrackNameEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("B"),
                    new SequenceTrackNameEvent("Name") { DeltaTime = 10 },
                    new TextEvent("C"))));

        [Test]
        public void Sanitize_DontRemoveDuplicatedSequenceTrackNameEvents_8(
            [Values(0, 100)] long firstDeltaTime,
            [Values(0, 100)] long secondDeltaTime) => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new SequenceTrackNameEvent("Name") { DeltaTime = firstDeltaTime }),
                new TrackChunk(
                    new SequenceTrackNameEvent("Name") { DeltaTime = secondDeltaTime })),
            settings: new SanitizingSettings
            {
                RemoveDuplicatedSequenceTrackNameEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new SequenceTrackNameEvent("Name") { DeltaTime = firstDeltaTime }),
                new TrackChunk(
                    new SequenceTrackNameEvent("Name") { DeltaTime = secondDeltaTime })));

        [Test]
        public void Sanitize_DontRemoveDuplicatedSequenceTrackNameEvents_9(
            [Values(0, 100)] long firstDeltaTime,
            [Values(0, 100)] long secondDeltaTime,
            [Values(0, 100)] long duplicatedEventDeltaTime) => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new SequenceTrackNameEvent("Name") { DeltaTime = firstDeltaTime },
                    new SequenceTrackNameEvent("Name") { DeltaTime = firstDeltaTime + duplicatedEventDeltaTime }),
                new TrackChunk(
                    new SequenceTrackNameEvent("Name") { DeltaTime = secondDeltaTime },
                    new SequenceTrackNameEvent("Name") { DeltaTime = secondDeltaTime + duplicatedEventDeltaTime })),
            settings: new SanitizingSettings
            {
                RemoveDuplicatedSequenceTrackNameEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new SequenceTrackNameEvent("Name") { DeltaTime = firstDeltaTime },
                    new SequenceTrackNameEvent("Name") { DeltaTime = firstDeltaTime + duplicatedEventDeltaTime }),
                new TrackChunk(
                    new SequenceTrackNameEvent("Name") { DeltaTime = secondDeltaTime },
                    new SequenceTrackNameEvent("Name") { DeltaTime = secondDeltaTime + duplicatedEventDeltaTime })));

        #endregion
    }
}
