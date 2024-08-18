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
        public void Sanitize_RemoveEventsOnUnusedChannels_EmptyFile() => Sanitize(
            midiFile: new MidiFile(),
            settings: null,
            expectedMidiFile: new MidiFile());

        [Test]
        public void Sanitize_RemoveEventsOnUnusedChannels_AllChannelsInUse() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new ControlChangeEvent() { Channel = (FourBitNumber)7 },
                    new NoteOnEvent(),
                    new NoteOffEvent()),
                new TrackChunk(
                    new NoteOnEvent() { Channel = (FourBitNumber)7 },
                    new NoteOffEvent() { Channel = (FourBitNumber)7 })),
            settings: new SanitizingSettings
            {
                RemoveDuplicatedNotes = false,
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new ControlChangeEvent() { Channel = (FourBitNumber)7 },
                    new NoteOnEvent(),
                    new NoteOffEvent()),
                new TrackChunk(
                    new NoteOnEvent() { Channel = (FourBitNumber)7 },
                    new NoteOffEvent() { Channel = (FourBitNumber)7 })));

        [Test]
        public void Sanitize_RemoveEventsOnUnusedChannels_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new ControlChangeEvent() { Channel = (FourBitNumber)7, DeltaTime = 40 },
                    new NoteOnEvent(),
                    new NoteOffEvent()),
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent())),
            settings: new SanitizingSettings
            {
                RemoveDuplicatedNotes = false,
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent() { DeltaTime = 40 },
                    new NoteOffEvent()),
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent())));

        [Test]
        public void Sanitize_RemoveEventsOnUnusedChannels_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent() { Channel = (FourBitNumber)7, DeltaTime = 40 },
                    new NoteOnEvent(),
                    new NoteOffEvent()),
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent() { Channel = (FourBitNumber) 7 })),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false,
                Trim = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 40 },
                    new NoteOffEvent()),
                new TrackChunk()));

        [Test]
        public void Sanitize_RemoveEventsOnUnusedChannels_3() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent() { Channel = (FourBitNumber)7, DeltaTime = 40 },
                    new NoteOnEvent(),
                    new PitchBendEvent() { Channel = (FourBitNumber)8 },
                    new NoteOffEvent()),
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent())),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 40 },
                    new NoteOffEvent()),
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent())));

        [Test]
        public void Sanitize_RemoveEventsOnUnusedChannels_4() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent(),
                    new ControlChangeEvent() { Channel = (FourBitNumber)7, DeltaTime = 40 },
                    new NoteOnEvent(),
                    new PitchBendEvent() { Channel = (FourBitNumber)8 },
                    new NoteOffEvent()),
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent())),
            settings: new SanitizingSettings
            {
                RemoveDuplicatedPitchBendEvents = false,
                RemoveDuplicatedSetTempoEvents = false,
                RemoveDuplicatedTimeSignatureEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent(),
                    new NoteOnEvent() { DeltaTime = 40 },
                    new NoteOffEvent()),
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent())));

        [Test]
        public void Sanitize_RemoveEventsOnUnusedChannels_5() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent(),
                    new ControlChangeEvent() { Channel = (FourBitNumber)7, DeltaTime = 40 },
                    new NoteOnEvent(),
                    new PitchBendEvent() { Channel = (FourBitNumber)8 },
                    new NoteOffEvent()),
                new TrackChunk(
                    new NoteOnEvent(),
                    new PitchBendEvent() { DeltaTime = 5 },
                    new NoteOffEvent() { DeltaTime = 5 })),
            settings: new SanitizingSettings
            {
                RemoveDuplicatedPitchBendEvents = false,
                RemoveDuplicatedSetTempoEvents = false,
                RemoveDuplicatedTimeSignatureEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent(),
                    new NoteOnEvent() { DeltaTime = 40 },
                    new NoteOffEvent()),
                new TrackChunk(
                    new NoteOnEvent(),
                    new PitchBendEvent() { DeltaTime = 5 },
                    new NoteOffEvent() { DeltaTime = 5 })));

        [Test]
        public void Sanitize_RemoveEventsOnUnusedChannels_False_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new ControlChangeEvent() { Channel = (FourBitNumber)7, DeltaTime = 40 },
                    new NoteOnEvent(),
                    new NoteOffEvent()),
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent())),
            settings: new SanitizingSettings
            {
                RemoveEventsOnUnusedChannels = false,
                RemoveDuplicatedNotes = false,
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new ControlChangeEvent() { Channel = (FourBitNumber)7, DeltaTime = 40 },
                    new NoteOnEvent(),
                    new NoteOffEvent()),
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent())));

        [Test]
        public void Sanitize_RemoveEventsOnUnusedChannels_False_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent() { Channel = (FourBitNumber)7, DeltaTime = 40 },
                    new NoteOnEvent(),
                    new NoteOffEvent()),
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent() { Channel = (FourBitNumber)7 })),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false,
                RemoveEventsOnUnusedChannels = false,
                Trim = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent() { Channel = (FourBitNumber)7, DeltaTime = 40 },
                    new NoteOnEvent(),
                    new NoteOffEvent()),
                new TrackChunk()));

        [Test]
        public void Sanitize_RemoveEventsOnUnusedChannels_False_3() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent() { Channel = (FourBitNumber)7, DeltaTime = 40 },
                    new NoteOnEvent(),
                    new PitchBendEvent() { Channel = (FourBitNumber)8 },
                    new NoteOffEvent() { DeltaTime = 5 }),
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent())),
            settings: new SanitizingSettings
            {
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent() { Channel = (FourBitNumber)7, DeltaTime = 40 },
                    new NoteOnEvent(),
                    new PitchBendEvent() { Channel = (FourBitNumber)8 },
                    new NoteOffEvent() { DeltaTime = 5 }),
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent())));

        [Test]
        public void Sanitize_RemoveEventsOnUnusedChannels_False_4() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent(),
                    new ControlChangeEvent() { Channel = (FourBitNumber)7, DeltaTime = 40 },
                    new NoteOnEvent(),
                    new PitchBendEvent() { Channel = (FourBitNumber)8 },
                    new NoteOffEvent() { DeltaTime = 5 }),
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent())),
            settings: new SanitizingSettings
            {
                RemoveDuplicatedPitchBendEvents = false,
                RemoveDuplicatedSetTempoEvents = false,
                RemoveDuplicatedTimeSignatureEvents = false,
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent(),
                    new ControlChangeEvent() { Channel = (FourBitNumber)7, DeltaTime = 40 },
                    new NoteOnEvent(),
                    new PitchBendEvent() { Channel = (FourBitNumber)8 },
                    new NoteOffEvent() { DeltaTime = 5 }),
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent())));

        [Test]
        public void Sanitize_RemoveEventsOnUnusedChannels_False_5() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent(),
                    new ControlChangeEvent() { Channel = (FourBitNumber)7, DeltaTime = 40 },
                    new NoteOnEvent(),
                    new PitchBendEvent() { Channel = (FourBitNumber)8 },
                    new NoteOffEvent() { DeltaTime = 5 }),
                new TrackChunk(
                    new NoteOnEvent(),
                    new PitchBendEvent(),
                    new NoteOffEvent() { DeltaTime = 5 })),
            settings: new SanitizingSettings
            {
                RemoveDuplicatedPitchBendEvents = false,
                RemoveDuplicatedSetTempoEvents = false,
                RemoveDuplicatedTimeSignatureEvents = false,
                RemoveEventsOnUnusedChannels = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new ControlChangeEvent(),
                    new ControlChangeEvent() { Channel = (FourBitNumber)7, DeltaTime = 40 },
                    new NoteOnEvent(),
                    new PitchBendEvent() { Channel = (FourBitNumber)8 },
                    new NoteOffEvent() { DeltaTime = 5 }),
                new TrackChunk(
                    new NoteOnEvent(),
                    new PitchBendEvent(),
                    new NoteOffEvent() { DeltaTime = 5 })));

        #endregion
    }
}
