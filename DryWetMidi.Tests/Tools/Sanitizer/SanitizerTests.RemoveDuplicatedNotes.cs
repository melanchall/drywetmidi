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
        public void Sanitize_RemoveDuplicatedNotes_EmptyFile() => Sanitize(
            midiFile: new MidiFile(),
            settings: null,
            expectedMidiFile: new MidiFile());

        [Test]
        public void Sanitize_RemoveDuplicatedNotes_NoNotes_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"))),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"))));

        [Test]
        public void Sanitize_RemoveDuplicatedNotes_NoNotes_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = 10 },
                    new TextEvent("B") { DeltaTime = 15 })),
            settings: new SanitizingSettings
            {
                Trim = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = 10 },
                    new TextEvent("B") { DeltaTime = 15 })));

        [Test]
        public void Sanitize_RemoveDuplicatedNotes_SingleNote_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = 10 },
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 15 },
                    new NoteOffEvent())),
            settings: new SanitizingSettings
            {
                Trim = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = 10 },
                    new NoteOnEvent(),
                    new TextEvent("B") { DeltaTime = 15 },
                    new NoteOffEvent())));

        [Test]
        public void Sanitize_RemoveDuplicatedNotes_SingleNote_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent())),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent())));

        [Test]
        public void Sanitize_RemoveDuplicatedNotes_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent(),
                    new NoteOffEvent())),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent())));

        [Test]
        public void Sanitize_RemoveDuplicatedNotes_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOffEvent())),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent())));

        [Test]
        public void Sanitize_RemoveDuplicatedNotes_3() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOnEvent() { Channel = (FourBitNumber)5 },
                    new NoteOnEvent(),
                    new NoteOnEvent() { Channel = (FourBitNumber)5 },
                    new TextEvent("A") { DeltaTime = 20 },
                    new NoteOffEvent() { Channel = (FourBitNumber)5 },
                    new NoteOffEvent(),
                    new NoteOffEvent() { Channel = (FourBitNumber)5 },
                    new NoteOffEvent())),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOnEvent() { Channel = (FourBitNumber)5 },
                    new TextEvent("A") { DeltaTime = 20 },
                    new NoteOffEvent() { Channel = (FourBitNumber)5 },
                    new NoteOffEvent())));

        #endregion
    }
}
