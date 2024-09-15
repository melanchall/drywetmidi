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
        public void Sanitize_RemoveDuplicatedControlChangeEvents_EmptyFile() => Sanitize(
            midiFile: new MidiFile(),
            settings: null,
            expectedMidiFile: new MidiFile());

        [Test]
        public void Sanitize_RemoveDuplicatedControlChangeEvents_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new ControlChangeEvent(ControlName.BreathController.AsSevenBitNumber(), (SevenBitNumber)3) { DeltaTime = 20 },
                    new ControlChangeEvent(ControlName.BreathController.AsSevenBitNumber(), (SevenBitNumber)3) { DeltaTime = 20, Channel = (FourBitNumber)2 },
                    new TextEvent("B"))),
            settings: new SanitizingSettings
            {
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new ControlChangeEvent(ControlName.BreathController.AsSevenBitNumber(), (SevenBitNumber)3) { DeltaTime = 20 },
                    new ControlChangeEvent(ControlName.BreathController.AsSevenBitNumber(), (SevenBitNumber)3) { DeltaTime = 20, Channel = (FourBitNumber)2 },
                    new TextEvent("B"))));

        [Test]
        public void Sanitize_RemoveDuplicatedControlChangeEvents_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 20 },
                    new ControlChangeEvent(ControlName.BreathController.AsSevenBitNumber(), (SevenBitNumber)3) { DeltaTime = 20 },
                    new ControlChangeEvent(ControlName.BreathController.AsSevenBitNumber(), (SevenBitNumber)3) { DeltaTime = 20, Channel = (FourBitNumber)2 },
                    new TextEvent("B"))),
            settings: new SanitizingSettings
            {
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 20 },
                    new ControlChangeEvent(ControlName.BreathController.AsSevenBitNumber(), (SevenBitNumber)3) { DeltaTime = 20 },
                    new ControlChangeEvent(ControlName.BreathController.AsSevenBitNumber(), (SevenBitNumber)3) { DeltaTime = 20, Channel = (FourBitNumber)2 },
                    new TextEvent("B"))));

        [Test]
        public void Sanitize_RemoveDuplicatedControlChangeEvents_3() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 20 },
                    new TextEvent("B"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20) { DeltaTime = 30 },
                    new TextEvent("C"))),
            settings: new SanitizingSettings
            {
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 20 },
                    new TextEvent("B"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20),
                    new TextEvent("C") { DeltaTime = 30 })));

        [Test]
        public void Sanitize_RemoveDuplicatedControlChangeEvents_4() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 20 },
                    new TextEvent("B"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { Channel = (FourBitNumber)3 },
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { Channel = (FourBitNumber)3 },
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20) { DeltaTime = 30 },
                    new TextEvent("C")),
                new TrackChunk(
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 10 },
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20) { DeltaTime = 100 })),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false,
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 20 },
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { Channel = (FourBitNumber)3 },
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20),
                    new TextEvent("C") { DeltaTime = 30 }),
                new TrackChunk(
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 10 })));

        [Test]
        public void Sanitize_RemoveDuplicatedControlChangeEvents_5() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 20 },
                    new TextEvent("B"),
                    new ControlChangeEvent(ControlName.LegatoFootswitch.AsSevenBitNumber(), (SevenBitNumber)30),
                    new ControlChangeEvent(ControlName.LegatoFootswitch.AsSevenBitNumber(), (SevenBitNumber)30),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20) { DeltaTime = 30 },
                    new TextEvent("C")),
                new TrackChunk(
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 10 },
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20) { DeltaTime = 100 })),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false,
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 20 },
                    new ControlChangeEvent(ControlName.LegatoFootswitch.AsSevenBitNumber(), (SevenBitNumber)30),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20),
                    new TextEvent("C") { DeltaTime = 30 }),
                new TrackChunk(
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 10 })));

        [Test]
        public void Sanitize_RemoveDuplicatedControlChangeEvents_6() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new ControlChangeEvent(ControlName.LegatoFootswitch.AsSevenBitNumber(), (SevenBitNumber)30),
                    new ControlChangeEvent(ControlName.LegatoFootswitch.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 30 },
                    new TextEvent("B")),
                new TrackChunk(
                    new TextEvent("C"),
                    new ControlChangeEvent(ControlName.LegatoFootswitch.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 100 })),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false,
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new ControlChangeEvent(ControlName.LegatoFootswitch.AsSevenBitNumber(), (SevenBitNumber)30),
                    new TextEvent("B") { DeltaTime = 30 }),
                new TrackChunk(
                    new TextEvent("C"))));

        [Test]
        public void Sanitize_RemoveDuplicatedControlChangeEvents_False_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 20 },
                    new TextEvent("B"))),
            settings: new SanitizingSettings
            {
                RemoveDuplicatedControlChangeEvents = false,
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 20 },
                    new TextEvent("B"))));

        [Test]
        public void Sanitize_RemoveDuplicatedControlChangeEvents_False_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 20 },
                    new TextEvent("B"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20) { DeltaTime = 30 },
                    new TextEvent("C"))),
            settings: new SanitizingSettings
            {
                RemoveDuplicatedControlChangeEvents = false,
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 20 },
                    new TextEvent("B"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20) { DeltaTime = 30 },
                    new TextEvent("C"))));

        [Test]
        public void Sanitize_RemoveDuplicatedControlChangeEvents_False_3() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 20 },
                    new TextEvent("B"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30),
                    new ControlChangeEvent(ControlName.LegatoFootswitch.AsSevenBitNumber(), (SevenBitNumber)30),
                    new ControlChangeEvent(ControlName.LegatoFootswitch.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 30 },
                    new TextEvent("C"))),
            settings: new SanitizingSettings
            {
                RemoveDuplicatedControlChangeEvents = false,
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 20 },
                    new TextEvent("B"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30),
                    new ControlChangeEvent(ControlName.LegatoFootswitch.AsSevenBitNumber(), (SevenBitNumber)30),
                    new ControlChangeEvent(ControlName.LegatoFootswitch.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 30 },
                    new TextEvent("C"))));

        [Test]
        public void Sanitize_RemoveDuplicatedPitchBendEvents_False_5() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 20 },
                    new TextEvent("B"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { Channel = (FourBitNumber)3 },
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { Channel = (FourBitNumber)3 },
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20) { DeltaTime = 30 },
                    new TextEvent("C")),
                new TrackChunk(
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 10 },
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20) { DeltaTime = 100 })),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false,
                RemoveDuplicatedControlChangeEvents = false,
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 20 },
                    new TextEvent("B"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { Channel = (FourBitNumber)3 },
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { Channel = (FourBitNumber)3 },
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20) { DeltaTime = 30 },
                    new TextEvent("C")),
                new TrackChunk(
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)30) { DeltaTime = 10 },
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20) { DeltaTime = 100 })));

        [Test]
        public void Sanitize_RemoveDuplicatedPitchBendEvents_False_6() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20) { DeltaTime = 30 },
                    new TextEvent("B")),
                new TrackChunk(
                    new TextEvent("C"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20) { DeltaTime = 100 })),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false,
                RemoveDuplicatedControlChangeEvents = false,
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20) { DeltaTime = 30 },
                    new TextEvent("B")),
                new TrackChunk(
                    new TextEvent("C"),
                    new ControlChangeEvent(ControlName.DamperPedal.AsSevenBitNumber(), (SevenBitNumber)20) { DeltaTime = 100 })));

        #endregion
    }
}
