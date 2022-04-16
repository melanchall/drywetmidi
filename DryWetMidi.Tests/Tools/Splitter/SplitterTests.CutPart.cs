using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class SplitterTests
    {
        #region Test methods

        [Test]
        public void CutPart_EmptyFile() => CutPart(
            midiFile: new MidiFile(),
            partStart: (MidiTimeSpan)100,
            partLength: (MidiTimeSpan)100,
            expectedMidiFile: new MidiFile());

        [Test]
        public void CutPart_EmptyTrackChunks() => CutPart(
            midiFile: new MidiFile(
                new TrackChunk(),
                new TrackChunk()),
            partStart: (MidiTimeSpan)100,
            partLength: (MidiTimeSpan)100,
            expectedMidiFile: new MidiFile());

        [Test]
        public void CutPart_1() => CutPart(
            midiFile: new MidiFile(
                new TrackChunk(new TextEvent("A")),
                new TrackChunk(new NoteOnEvent())),
            partStart: (MidiTimeSpan)100,
            partLength: (MidiTimeSpan)100,
            expectedMidiFile: new MidiFile(
                new TrackChunk(new TextEvent("A")),
                new TrackChunk(new NoteOnEvent())));

        [Test]
        public void CutPart_2() => CutPart(
            midiFile: new MidiFile(
                new TrackChunk(new TextEvent("A") { DeltaTime = 350 }),
                new TrackChunk(new NoteOnEvent() { DeltaTime = 450 })),
            partStart: (MidiTimeSpan)100,
            partLength: (MidiTimeSpan)100,
            expectedMidiFile: new MidiFile(
                new TrackChunk(new TextEvent("A") { DeltaTime = 250 }),
                new TrackChunk(new NoteOnEvent() { DeltaTime = 350 })));

        [Test]
        public void CutPart_3() => CutPart(
            midiFile: new MidiFile(
                new TrackChunk(new TextEvent("A") { DeltaTime = 350 }),
                new TrackChunk(new NoteOnEvent() { DeltaTime = 50 })),
            partStart: (MidiTimeSpan)100,
            partLength: (MidiTimeSpan)100,
            expectedMidiFile: new MidiFile(
                new TrackChunk(new TextEvent("A") { DeltaTime = 250 }),
                new TrackChunk(new NoteOnEvent() { DeltaTime = 50 })));

        [Test]
        public void CutPart_4() => CutPart(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 50 }, new NoteOffEvent { DeltaTime = 400 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 99 }, new NoteOffEvent { DeltaTime = 102 })),
            partStart: (MidiTimeSpan)100,
            partLength: (MidiTimeSpan)100,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 50 }, new NoteOffEvent { DeltaTime = 300 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 99 }, new NoteOffEvent { DeltaTime = 2 })));

        [Test]
        public void CutPart_5() => CutPart(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 100 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 99 }, new NoteOffEvent { DeltaTime = 102 })),
            partStart: (MidiTimeSpan)100,
            partLength: (MidiTimeSpan)100,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 99 }, new NoteOffEvent { DeltaTime = 2 })));

        [Test]
        public void CutPart_6() => CutPart(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 50 }, new NoteOffEvent { DeltaTime = 400 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 70 }, new NoteOffEvent { DeltaTime = 40 }, new NoteOnEvent { DeltaTime = 80 }, new NoteOffEvent { DeltaTime = 40 })),
            partStart: (MidiTimeSpan)100,
            partLength: (MidiTimeSpan)100,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 50 }, new NoteOffEvent { DeltaTime = 300 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 70 }, new NoteOffEvent { DeltaTime = 30 }, new NoteOnEvent(), new NoteOffEvent { DeltaTime = 30 })));

        [Test]
        public void CutPart_7() => CutPart(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 50 }, new NoteOffEvent { DeltaTime = 50 }, new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 50 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 40 }, new NoteOffEvent { DeltaTime = 60 }, new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 60 })),
            partStart: (MidiTimeSpan)100,
            partLength: (MidiTimeSpan)100,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 50 }, new NoteOffEvent { DeltaTime = 50 }, new NoteOnEvent(), new NoteOffEvent { DeltaTime = 50 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 40 }, new NoteOffEvent { DeltaTime = 60 }, new NoteOnEvent(), new NoteOffEvent { DeltaTime = 60 })));

        [Test]
        public void CutPart_DontSplitNotes_1() => CutPart(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 50 }, new NoteOffEvent { DeltaTime = 400 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 99 }, new NoteOffEvent { DeltaTime = 102 })),
            partStart: (MidiTimeSpan)100,
            partLength: (MidiTimeSpan)100,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 50 }, new NoteOffEvent { DeltaTime = 300 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 99 }, new NoteOffEvent { DeltaTime = 2 })),
            settings: new SliceMidiFileSettings
            {
                SplitNotes = false
            });

        [Test]
        public void CutPart_DontSplitNotes_2() => CutPart(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 100 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 99 }, new NoteOffEvent { DeltaTime = 102 })),
            partStart: (MidiTimeSpan)100,
            partLength: (MidiTimeSpan)100,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 99 }, new NoteOffEvent { DeltaTime = 2 })),
            settings: new SliceMidiFileSettings
            {
                SplitNotes = false
            });

        [Test]
        public void CutPart_DontSplitNotes_3() => CutPart(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 50 }, new NoteOffEvent { DeltaTime = 400 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 70 }, new NoteOffEvent { DeltaTime = 40 }, new NoteOnEvent { DeltaTime = 80 }, new NoteOffEvent { DeltaTime = 40 })),
            partStart: (MidiTimeSpan)100,
            partLength: (MidiTimeSpan)100,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 50 }, new NoteOffEvent { DeltaTime = 300 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 70 }, new NoteOffEvent { DeltaTime = 60 })),
            settings: new SliceMidiFileSettings
            {
                SplitNotes = false
            });

        [Test]
        public void CutPart_DontSplitNotes_4() => CutPart(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 50 }, new NoteOffEvent { DeltaTime = 50 }, new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 50 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 40 }, new NoteOffEvent { DeltaTime = 60 }, new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 60 })),
            partStart: (MidiTimeSpan)100,
            partLength: (MidiTimeSpan)100,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 50 }, new NoteOffEvent { DeltaTime = 50 }, new NoteOnEvent(), new NoteOffEvent { DeltaTime = 50 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 40 }, new NoteOffEvent { DeltaTime = 60 }, new NoteOnEvent(), new NoteOffEvent { DeltaTime = 60 })),
            settings: new SliceMidiFileSettings
            {
                SplitNotes = false
            });

        [Test]
        public void CutPart_DontSplitNotes_PreserveTimes_1() => CutPart(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 50 }, new NoteOffEvent { DeltaTime = 400 }, new TextEvent("A") { DeltaTime = 50 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 99 }, new NoteOffEvent { DeltaTime = 102 })),
            partStart: (MidiTimeSpan)100,
            partLength: (MidiTimeSpan)100,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 50 }, new NoteOffEvent { DeltaTime = 400 }, new TextEvent("A") { DeltaTime = 50 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 99 }, new NoteOffEvent { DeltaTime = 102 })),
            settings: new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = true
            });

        [Test]
        public void CutPart_DontSplitNotes_PreserveTimes_2() => CutPart(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 100 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 99 }, new TextEvent("A") { DeltaTime = 2 }, new NoteOffEvent { DeltaTime = 100 })),
            partStart: (MidiTimeSpan)100,
            partLength: (MidiTimeSpan)100,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 99 }, new NoteOffEvent { DeltaTime = 102 })),
            settings: new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = true
            });

        [Test]
        public void CutPart_DontSplitNotes_PreserveTimes_3() => CutPart(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 50 }, new NoteOffEvent { DeltaTime = 400 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 70 }, new NoteOffEvent { DeltaTime = 40 }, new NoteOnEvent { DeltaTime = 80 }, new NoteOffEvent { DeltaTime = 40 })),
            partStart: (MidiTimeSpan)100,
            partLength: (MidiTimeSpan)100,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 50 }, new NoteOffEvent { DeltaTime = 400 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 70 }, new NoteOffEvent { DeltaTime = 160 })),
            settings: new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = true
            });

        [Test]
        public void CutPart_DontSplitNotes_PreserveTimes_4() => CutPart(
            midiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 50 }, new NoteOffEvent { DeltaTime = 50 }, new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 50 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 40 }, new NoteOffEvent { DeltaTime = 60 }, new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 60 })),
            partStart: (MidiTimeSpan)100,
            partLength: (MidiTimeSpan)100,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 50 }, new NoteOffEvent { DeltaTime = 50 }, new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 50 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 40 }, new NoteOffEvent { DeltaTime = 60 }, new NoteOnEvent { DeltaTime = 100 }, new NoteOffEvent { DeltaTime = 60 })),
            settings: new SliceMidiFileSettings
            {
                SplitNotes = false,
                PreserveTimes = true
            });

        #endregion

        #region Private methods

        private void CutPart(
            MidiFile midiFile,
            ITimeSpan partStart,
            ITimeSpan partLength,
            MidiFile expectedMidiFile,
            SliceMidiFileSettings settings = null)
        {
            var newMidiFile = midiFile.CutPart(partStart, partLength, settings);
            MidiAsserts.AreEqual(expectedMidiFile, newMidiFile, false, "Invalid new MIDI file.");
        }

        #endregion
    }
}
