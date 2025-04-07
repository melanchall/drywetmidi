using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class SanitizerTests
    {
        #region Test methods

        [Test]
        public void Sanitize_NoteMinLength_EmptyFile() => Sanitize(
            midiFile: new MidiFile(),
            settings: null,
            expectedMidiFile: new MidiFile());

        [Test]
        public void Sanitize_NoteMinLength_NoNotes_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk()),
            settings: null,
            expectedMidiFile: new MidiFile());

        [Test]
        public void Sanitize_NoteMinLength_NoNotes_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk()),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk()));

        [Test]
        public void Sanitize_NoteMinLength_NoNotes_3() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(),
                new TrackChunk()),
            settings: null,
            expectedMidiFile: new MidiFile());

        [Test]
        public void Sanitize_NoteMinLength_NoNotes_4() => Sanitize(
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
        public void Sanitize_NoteMinLength_NoNotes_5() => Sanitize(
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
        public void Sanitize_NoteMinLength_NoNotes_6() => Sanitize(
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
        public void Sanitize_NoteMinLength_Default_SingleNote_ZeroLength() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue))),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue))));

        [Test]
        public void Sanitize_NoteMinLength_Default_SingleNote_NonZeroLength() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 20 })),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 20 })));

        [Test]
        public void Sanitize_NoteMinLength_SingleNote_Long_Midi([Values(10, 20)] long noteLength) => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = noteLength })),
            settings: new SanitizingSettings
            {
                NoteMinLength = (MidiTimeSpan)10
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = noteLength })));

        [Test]
        public void Sanitize_NoteMinLength_SingleNote_Long_Musical([Values(2, 1)] long noteLengthDenominator)
        {
            var noteLength = LengthConverter.ConvertFrom(new MusicalTimeSpan(noteLengthDenominator), 0, TempoMap.Default);
            Sanitize(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent(),
                        new NoteOffEvent() { DeltaTime = noteLength })),
                settings: new SanitizingSettings
                {
                    NoteMinLength = MusicalTimeSpan.Half
                },
                expectedMidiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent(),
                        new NoteOffEvent() { DeltaTime = noteLength })));
        }

        [Test]
        public void Sanitize_NoteMinLength_SingleNote_Short_Midi([Values(0, 5)] long noteLength) => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = noteLength })),
            settings: new SanitizingSettings
            {
                NoteMinLength = (MidiTimeSpan)10,
                RemoveEmptyTrackChunks = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk()));

        [Test]
        public void Sanitize_NoteMinLength_SingleNote_Short_Midi_Fail_1([Values(0, 5)] long noteLength) => ClassicAssert.Throws<AssertionException>(() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = noteLength })),
            settings: new SanitizingSettings
            {
                NoteMinLength = (MidiTimeSpan)10
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk())));

        [Test]
        public void Sanitize_NoteMinLength_SingleNote_Short_Midi_Fail_2([Values(0, 5)] long noteLength) => ClassicAssert.Throws<AssertionException>(() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = noteLength })),
            settings: new SanitizingSettings
            {
                NoteMinLength = (MidiTimeSpan)10
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = noteLength }))));

        [Test]
        public void Sanitize_NoteMinLength_SingleNote_Short_Musical([Values(4, 8)] long noteLengthDenominator)
        {
            var noteLength = LengthConverter.ConvertFrom(new MusicalTimeSpan(noteLengthDenominator), 0, TempoMap.Default);
            Sanitize(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                        new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = noteLength })),
                settings: new SanitizingSettings
                {
                    NoteMinLength = MusicalTimeSpan.Half,
                    RemoveEmptyTrackChunks = false
                },
                expectedMidiFile: new MidiFile(
                    new TrackChunk()));
        }

        [Test]
        public void Sanitize_NoteMinLength_MultipleNotes_1() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 20 })),
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 20 })));

        [Test]
        public void Sanitize_NoteMinLength_MultipleNotes_2() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 40 },
                    new TextEvent("A") { DeltaTime = 30 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 10 },
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 20 },
                    new TextEvent("B") { DeltaTime = 70 })),
            settings: new SanitizingSettings
            {
                NoteMinLength = (MidiTimeSpan)20
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 40 },
                    new TextEvent("A") { DeltaTime = 30 },
                    new NoteOnEvent() { DeltaTime = 10 },
                    new NoteOffEvent() { DeltaTime = 20 },
                    new TextEvent("B") { DeltaTime = 70 })));

        [Test]
        public void Sanitize_NoteMinLength_MultipleNotes_3() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 40 },
                    new TextEvent("A") { DeltaTime = 30 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 10 },
                    new TextEvent("B") { DeltaTime = 70 },
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 20 }),
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 5 },
                    new TextEvent("A") { DeltaTime = 30 },
                    new NoteOffEvent() { DeltaTime = 20 })),
            settings: new SanitizingSettings
            {
                NoteMinLength = (MidiTimeSpan)20
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 40 },
                    new TextEvent("A") { DeltaTime = 30 },
                    new TextEvent("B") { DeltaTime = 80 },
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 20 }),
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = 35 })));

        [Test]
        public void Sanitize_NoteMinLength_MultipleNotes_4() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 40 },
                    new TextEvent("A") { DeltaTime = 30 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 10 },
                    new TextEvent("B") { DeltaTime = 70 },
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 20 }),
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 5 },
                    new TextEvent("A") { DeltaTime = 30 },
                    new NoteOffEvent() { DeltaTime = 20 })),
            settings: new SanitizingSettings
            {
                NoteMinLength = (MidiTimeSpan)20,
                RemoveOrphanedNoteOffEvents = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 40 },
                    new TextEvent("A") { DeltaTime = 30 },
                    new TextEvent("B") { DeltaTime = 80 },
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 20 }),
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = 35 },
                    new NoteOffEvent() { DeltaTime = 20 })));

        [Test]
        public void Sanitize_NoteMinLength_MultipleNotes_5() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 5 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 10 }),
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 5 })),
            settings: new SanitizingSettings
            {
                NoteMinLength = (MidiTimeSpan)20,
                RemoveEmptyTrackChunks = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(),
                new TrackChunk()));

        [Test]
        public void Sanitize_NoteMinLength_MultipleNotes_6() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 5 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 10 }),
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 5 })),
            settings: new SanitizingSettings
            {
                NoteMinLength = (MidiTimeSpan)20
            },
            expectedMidiFile: new MidiFile());

        #endregion
    }
}
