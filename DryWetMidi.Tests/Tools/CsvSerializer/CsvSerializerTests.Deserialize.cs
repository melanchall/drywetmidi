using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class CsvSerializerTests
    {
        #region Test methods

        [TestCaseSource(nameof(EventsData))]
        public void Deserialize_Event(MidiEvent midiEvent, string expectedCsv, CsvSerializationSettings settings) => CheckDeserialize(
            csvLines: new[] { $"0,\"{midiEvent.EventType}\",0{(string.IsNullOrEmpty(expectedCsv) ? string.Empty : $",{expectedCsv}")}" },
            check: stream =>
            {
                var objects = CsvSerializer.DeserializeObjectsFromCsv(stream, TempoMap.Default, settings).ToArray();
                ClassicAssert.AreEqual(1, objects.Length, "More than one object read.");

                var timedEvent = (TimedEvent)objects.Single();
                MidiAsserts.AreEqual(midiEvent, timedEvent.Event, false, "Invalid event.");
            });

        [Test]
        public void DeserializeFile_Empty() => DeserializeFileAndChunksAndSeparateChunks(
            csvLines: new[]
            {
                $"0,\"MThd\",0,\"Header\",1234",
            },
            settings: null,
            expectedMidiFile: new MidiFile { TimeDivision = new TicksPerQuarterNoteTimeDivision(1234) });

        [Test]
        public void Deserialize() => DeserializeFileAndChunksAndSeparateChunks(
            csvLines: new[]
            {
                $"0,\"MThd\",0,\"Header\",{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote}",
                $"1,\"MTrk\",0,\"Text\",0,\"A\"",
                $"1,\"MTrk\",1,\"Text\",{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote},\"B\"",
            },
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote })));

        [Test]
        public void Deserialize_TimeType() => DeserializeFileAndChunksAndSeparateChunks(
            csvLines: new[]
            {
                $"0,\"MThd\",0,\"Header\",{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote}",
                $"1,\"MTrk\",0,\"Text\",0/1,\"A\"",
                $"1,\"MTrk\",1,\"Text\",1/4,\"B\"",
            },
            settings: new CsvSerializationSettings
            {
                TimeType = TimeSpanType.Musical
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote })));

        [Test]
        public void Deserialize_MultipleTrackChunks() => DeserializeFileAndChunksAndSeparateChunks(
            csvLines: new[]
            {
                $"0,\"MThd\",0,\"Header\",{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote}",
                $"1,\"MTrk\",0,\"Text\",0/1,\"A\"",
                $"1,\"MTrk\",1,\"Text\",1/4,\"B\"",
                $"2,\"MTrk\",0,\"NoteOn\",0/1,4,100,127",
                $"2,\"MTrk\",1,\"NoteOff\",1/4,4,100,0",
            },
            settings: new CsvSerializationSettings
            {
                TimeType = TimeSpanType.Musical
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote }),
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)4 },
                    new NoteOffEvent((SevenBitNumber)100, SevenBitNumber.MinValue) { Channel = (FourBitNumber)4, DeltaTime = TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote })));

        [Test]
        public void Deserialize_Notes() => DeserializeFileAndChunksAndSeparateChunks(
            csvLines: new[]
            {
                $"0,\"MThd\",0,\"Header\",{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote}",
                $"1,\"MTrk\",0,\"Text\",0,\"A\"",
                $"1,\"MTrk\",1,\"Text\",{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote},\"B\"",
                $"2,\"MTrk\",0,\"Note\",0,{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote},4,100,127,0",
            },
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote }),
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)4 },
                    new NoteOffEvent((SevenBitNumber)100, SevenBitNumber.MinValue) { Channel = (FourBitNumber)4, DeltaTime = TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote })));

        [Test]
        public void Deserialize_Notes_Letter() => DeserializeFileAndChunksAndSeparateChunks(
            csvLines: new[]
            {
                $"0,\"MThd\",0,\"Header\",{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote}",
                $"1,\"MTrk\",0,\"Text\",0:0:0:0,\"A\"",
                $"1,\"MTrk\",1,\"Text\",0:0:0:500,\"B\"",
                $"2,\"MTrk\",0,\"Note\",0:0:0:0,1/4,4,E7,127,0",
            },
            settings: new CsvSerializationSettings
            {
                NoteFormat = CsvNoteFormat.Letter,
                TimeType = TimeSpanType.Metric,
                LengthType = TimeSpanType.Musical,
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote }),
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)4 },
                    new NoteOffEvent((SevenBitNumber)100, SevenBitNumber.MinValue) { Channel = (FourBitNumber)4, DeltaTime = TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote })));

        [Test]
        public void Deserialize_AllObjectTypes() => DeserializeFileAndChunksAndSeparateChunks(
            csvLines: new[]
            {
                $"0,\"MThd\",0,\"Header\",{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote}",
                $"1,\"MTrk\",0,\"Text\",0,\"A\"",
                $"1,\"MTrk\",1,\"Text\",100,\"B\"",
                $"2,\"MTrk\",0,\"Note\",0,100,4,E7,127,0",
                $"2,\"MTrk\",1,\"Note\",100,100,3,D3,127,0",
                $"2,\"MTrk\",1,\"Note\",110,100,3,E2,127,0",
            },
            settings: new CsvSerializationSettings
            {
                NoteFormat = CsvNoteFormat.Letter,
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 100 }),
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)4 },
                    new NoteOffEvent((SevenBitNumber)100, SevenBitNumber.MinValue) { Channel = (FourBitNumber)4, DeltaTime = 100 },
                    new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)3 },
                    new NoteOnEvent((SevenBitNumber)40, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)3, DeltaTime = 10 },
                    new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue) { Channel = (FourBitNumber)3, DeltaTime = 90 },
                    new NoteOffEvent((SevenBitNumber)40, SevenBitNumber.MinValue) { Channel = (FourBitNumber)3, DeltaTime = 10 })));

        [Test]
        public void Deserialize_Delimiter() => DeserializeFileAndChunksAndSeparateChunks(
            csvLines: new[]
            {
                $"0 \"MThd\" 0 \"Header\" {TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote}",
                $"1 \"MTrk\" 0 \"Text\" 0/1 \"A\"",
                $"1 \"MTrk\" 1 \"Text\" 1/4 \"B\"",
                $"1 \"MTrk\" 2 \"NormalSysEx\" 1/4 \"9 10 15 255\"",
            },
            settings: new CsvSerializationSettings
            {
                TimeType = TimeSpanType.Musical,
                Delimiter = ' ',
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote },
                    new NormalSysExEvent(new byte[] { 9, 10, 15, 255 }))));

        [Test]
        public void Deserialize_BytesArrayFormat() => DeserializeFileAndChunksAndSeparateChunks(
            csvLines: new[]
            {
                $"0,\"MThd\",0,\"Header\",{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote}",
                $"1,\"MTrk\",0,\"Text\",0,\"A\"",
                $"1,\"MTrk\",1,\"NormalSysEx\",0,\"09 0A 0F FF\"",
            },
            settings: new CsvSerializationSettings
            {
                BytesArrayFormat = CsvBytesArrayFormat.Hexadecimal,
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new NormalSysExEvent(new byte[] { 9, 10, 15, 255 }))));

        [Test]
        public void Deserialize_NewlinesAndQuotes() => DeserializeFileAndChunksAndSeparateChunks(
            csvLines: new[]
            {
                $"0,\"MThd\",0,\"Header\",{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote}",
                $"1,\"MTrk\",0,\"Text\",0,\"\"\"in\"\" \"\"quotes\"\"\"",
                $"1,\"MTrk\",1,\"Text\",0,\"B",
                $"bb\"\"in quotes\"\"\"",
                $"1,\"MTrk\",2,\"Text\",0,\"C\"",
            },
            settings: null,
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("\"in\" \"quotes\""),
                    new TextEvent($"B{Environment.NewLine}bb\"in quotes\""),
                    new TextEvent("C"))),
            checkSeparateChunks: false);

        [Test]
        public void Deserialize_Objects_1() => DeserializeObjects(
            csvLines: new[]
            {
                $"0,\"Text\",0,\"A\"",
                $"1,\"Text\",100,\"B\"",
                $"2,\"Note\",0,100,4,E7,127,1",
                $"3,\"Note\",100,100,3,D3,127,2",
                $"3,\"Note\",110,100,3,E2,125,3",
            },
            settings: new CsvSerializationSettings
            {
                NoteFormat = CsvNoteFormat.Letter,
            },
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A")),
                new TimedEvent(new TextEvent("B"), 100),
                new Note((SevenBitNumber)100, 100, 0) { Channel = (FourBitNumber)4, Velocity = SevenBitNumber.MaxValue, OffVelocity = (SevenBitNumber)1 },
                new Chord(
                    new Note((SevenBitNumber)50, 100, 100) { Channel = (FourBitNumber)3, Velocity = SevenBitNumber.MaxValue, OffVelocity = (SevenBitNumber)2 },
                    new Note((SevenBitNumber)40, 100, 110) { Channel = (FourBitNumber)3, Velocity = (SevenBitNumber)125, OffVelocity = (SevenBitNumber)3 }),
            });

        [Test]
        public void Deserialize_Objects_2() => DeserializeObjects(
            csvLines: new[]
            {
                $"0,\"Text\",0,\"A\"",
                $"1,\"Text\",100,\"B\"",
                $"2,\"Note\",0,100,3,E7,127,1",
                $"2,\"Note\",100,100,3,D3,127,2",
                $"2,\"Note\",110,100,3,E2,125,3",
            },
            settings: new CsvSerializationSettings
            {
                NoteFormat = CsvNoteFormat.Letter,
            },
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A")),
                new TimedEvent(new TextEvent("B"), 100),
                new Chord(
                    new Note((SevenBitNumber)100, 100, 0) { Channel = (FourBitNumber)3, Velocity = SevenBitNumber.MaxValue, OffVelocity = (SevenBitNumber)1 },
                    new Note((SevenBitNumber)50, 100, 100) { Channel = (FourBitNumber)3, Velocity = SevenBitNumber.MaxValue, OffVelocity = (SevenBitNumber)2 },
                    new Note((SevenBitNumber)40, 100, 110) { Channel = (FourBitNumber)3, Velocity = (SevenBitNumber)125, OffVelocity = (SevenBitNumber)3 }),
            });

        [Test]
        public void Deserialize_Objects_3() => DeserializeObjects(
            csvLines: new[]
            {
                $"0,\"Text\",0,\"A\"",
                $"1,\"Text\",100,\"B\"",
                $"2,\"Note\",0,100,3,E7,127,1",
                $"3,\"Note\",100,100,3,D3,127,2",
                $"4,\"Note\",110,100,3,E2,125,3",
            },
            settings: new CsvSerializationSettings
            {
                NoteFormat = CsvNoteFormat.Letter,
            },
            expectedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A")),
                new TimedEvent(new TextEvent("B"), 100),
                new Note((SevenBitNumber)100, 100, 0) { Channel = (FourBitNumber)3, Velocity = SevenBitNumber.MaxValue, OffVelocity = (SevenBitNumber)1 },
                new Note((SevenBitNumber)50, 100, 100) { Channel = (FourBitNumber)3, Velocity = SevenBitNumber.MaxValue, OffVelocity = (SevenBitNumber)2 },
                new Note((SevenBitNumber)40, 100, 110) { Channel = (FourBitNumber)3, Velocity = (SevenBitNumber)125, OffVelocity = (SevenBitNumber)3 },
            });

        #endregion

        #region Private methods

        private void DeserializeObjects(
            string[] csvLines,
            CsvSerializationSettings settings,
            ICollection<ITimedObject> expectedObjects)
        {
            CheckDeserialize(
                csvLines,
                stream =>
                {
                    var actualObjects = CsvSerializer.DeserializeObjectsFromCsv(stream, TempoMap.Default, settings);
                    MidiAsserts.AreEqual(expectedObjects, actualObjects, "Invalid objects.");
                });
        }

        private void DeserializeFileAndChunksAndSeparateChunks(
            string[] csvLines,
            CsvSerializationSettings settings,
            MidiFile expectedMidiFile,
            bool checkSeparateChunks = true)
        {
            DeserializeFile(csvLines, settings, expectedMidiFile);

            var tempoMap = expectedMidiFile.GetTempoMap();

            DeserializeChunks(
                csvLines.Skip(1).ToArray(),
                settings,
                tempoMap,
                expectedMidiFile.Chunks);

            if (checkSeparateChunks)
                DeserializeSeparateChunks(
                    csvLines.Skip(1).ToArray(),
                    settings,
                    tempoMap,
                    expectedMidiFile.Chunks);
        }

        private void DeserializeFile(
            string[] csvLines,
            CsvSerializationSettings settings,
            MidiFile expectedMidiFile)
        {
            CheckDeserialize(
                csvLines,
                stream =>
                {
                    var midiFile = CsvSerializer.DeserializeFileFromCsv(stream, settings);
                    MidiAsserts.AreEqual(expectedMidiFile, midiFile, false, "Invalid file.");
                });
        }

        private void DeserializeChunks(
            string[] csvLines,
            CsvSerializationSettings settings,
            TempoMap tempoMap,
            ICollection<MidiChunk> expectedChunks)
        {
            CheckDeserialize(
                csvLines,
                stream =>
                {
                    var chunks = CsvSerializer.DeserializeChunksFromCsv(stream, tempoMap, settings);
                    MidiAsserts.AreEqual(expectedChunks, chunks, true, "Invalid chunks.");
                });
        }

        private void DeserializeSeparateChunks(
            string[] csvLines,
            CsvSerializationSettings settings,
            TempoMap tempoMap,
            ICollection<MidiChunk> expectedChunks)
        {
            var i = 0;

            foreach (var expectedChunk in expectedChunks)
            {
                var chunkCsvLines = csvLines
                    .Where(l => Regex.Match(l, @"^(\d+?)").Value == (i + 1).ToString())
                    .Select(l => Regex.Replace(l, @"^\d+?", m => "0"))
                    .ToArray();

                CheckDeserialize(
                    chunkCsvLines,
                    stream =>
                    {
                        var chunk = CsvSerializer.DeserializeChunkFromCsv(stream, tempoMap, settings);
                        MidiAsserts.AreEqual(expectedChunk, chunk, true, $"Invalid chunk {i}.");
                    });

                i++;
            }
        }

        private static void CheckDeserialize(
            string[] csvLines,
            Action<Stream> check)
        {
            using (var stream = new MemoryStream())
            using (var streamWriter = new StreamWriter(stream))
            {
                foreach (var line in csvLines)
                {
                    streamWriter.WriteLine(line);
                }

                streamWriter.Flush();
                stream.Seek(0, SeekOrigin.Begin);

                check(stream);
            }
        }

        #endregion
    }
}
