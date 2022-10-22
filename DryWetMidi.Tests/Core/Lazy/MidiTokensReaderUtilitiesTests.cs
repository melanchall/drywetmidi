using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed class MidiTokensReaderUtilitiesTests
    {
        #region Test methods

        [Test]
        public void EnumerateEvents_NoEvents() => EnumerateEvents(
            initReader: ReadUntilTrackChunk,
            midiFile: new MidiFile(),
            expectedEvents: Array.Empty<MidiEvent>());

        [Test]
        public void EnumerateEvents_SingleTrackChunk_SingleEvent() => EnumerateEvents(
            initReader: ReadUntilTrackChunk,
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"))),
            expectedEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new EndOfTrackEvent()
            },
            format: MidiFileFormat.SingleTrack);

        [Test]
        public void EnumerateEvents_SingleTrackChunk_MultipleEvents() => EnumerateEvents(
            initReader: ReadUntilTrackChunk,
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new ProgramChangeEvent((SevenBitNumber)70) { DeltaTime = 30 })),
            expectedEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ProgramChangeEvent((SevenBitNumber)70) { DeltaTime = 30 },
                new EndOfTrackEvent()
            },
            format: MidiFileFormat.SingleTrack);

        [Test]
        public void EnumerateEvents_MultipleTrackChunks_SingleEvent_ReadFirst() => EnumerateEvents(
            initReader: ReadUntilTrackChunk,
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A")),
                new TrackChunk(
                    new ProgramChangeEvent((SevenBitNumber)70) { DeltaTime = 30 })),
            expectedEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new EndOfTrackEvent()
            });

        [Test]
        public void EnumerateEvents_MultipleTrackChunks_SingleEvent_ReadSecond() => EnumerateEvents(
            initReader: reader =>
            {
                ReadUntilTrackChunk(reader);
                ReadUntilTrackChunk(reader);
            },
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A")),
                new TrackChunk(
                    new ProgramChangeEvent((SevenBitNumber)70) { DeltaTime = 30 })),
            expectedEvents: new MidiEvent[]
            {
                new ProgramChangeEvent((SevenBitNumber)70) { DeltaTime = 30 },
                new EndOfTrackEvent()
            });

        [Test]
        public void EnumerateEvents_MultipleTrackChunks_MultipleEvents_ReadFirst() => EnumerateEvents(
            initReader: ReadUntilTrackChunk,
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue) { DeltaTime = 20 },
                    new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue)),
                new TrackChunk(
                    new ProgramChangeEvent((SevenBitNumber)70) { DeltaTime = 30 },
                    new NoteOnEvent((SevenBitNumber)40, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)40, SevenBitNumber.MinValue) { DeltaTime = 20 })),
            expectedEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue) { DeltaTime = 20 },
                new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue),
                new EndOfTrackEvent()
            });

        [Test]
        public void EnumerateEvents_MultipleTrackChunks_MultipleEvents_ReadSecond() => EnumerateEvents(
            initReader: reader =>
            {
                ReadUntilTrackChunk(reader);
                ReadUntilTrackChunk(reader);
            },
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue) { DeltaTime = 20 },
                    new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue)),
                new TrackChunk(
                    new ProgramChangeEvent((SevenBitNumber)70) { DeltaTime = 30 },
                    new NoteOnEvent((SevenBitNumber)40, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)40, SevenBitNumber.MinValue) { DeltaTime = 20 })),
            expectedEvents: new MidiEvent[]
            {
                new ProgramChangeEvent((SevenBitNumber)70) { DeltaTime = 30 },
                new NoteOnEvent((SevenBitNumber)40, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)40, SevenBitNumber.MinValue) { DeltaTime = 20 },
                new EndOfTrackEvent()
            });

        #endregion

        #region Private methods

        private void EnumerateEvents(
            Action<MidiTokensReader> initReader,
            MidiFile midiFile,
            MidiEvent[] expectedEvents,
            MidiFileFormat format = MidiFileFormat.MultiTrack)
        {
            var filePath = FileOperations.GetTempFilePath();

            try
            {
                midiFile.Write(filePath, true, format);

                using (var reader = MidiFile.ReadLazy(filePath))
                {
                    initReader(reader);

                    var actualEvents = reader.EnumerateEvents().Events.ToArray();
                    MidiAsserts.AreEqual(expectedEvents, actualEvents, true, "Invalid events.");
                }
            }
            finally
            {
                FileOperations.DeleteFile(filePath);
            }
        }

        private void ReadUntilTrackChunk(MidiTokensReader reader)
        {
            while (true)
            {
                var token = reader.ReadToken();
                if (token == null || (token is ChunkHeaderToken chunkHeaderToken && chunkHeaderToken.ChunkId == TrackChunk.Id))
                    return;
            }
        }

        #endregion
    }
}
