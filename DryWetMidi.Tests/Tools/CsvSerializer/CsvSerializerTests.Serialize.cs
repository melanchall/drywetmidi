using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
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
        public void Serialize_Event(MidiEvent midiEvent, string expectedCsv, CsvSerializationSettings settings)
        {
            using (var stream = new MemoryStream())
            {
                var timedEvent = new TimedEvent(midiEvent);
                new[] { timedEvent }.SerializeToCsv(stream, TempoMap.Default, settings);

                stream.Seek(0, SeekOrigin.Begin);

                using (var streamReader = new StreamReader(stream))
                {
                    var csv = streamReader.ReadToEnd().Trim();
                    ClassicAssert.AreEqual($"0,\"{midiEvent.EventType}\",0{(string.IsNullOrEmpty(expectedCsv) ? string.Empty : $",{expectedCsv}")}", csv, "Invalid CSV.");
                }
            }
        }

        [Test]
        public void Serialize_Empty([Values(null, MidiFileFormat.MultiTrack)] MidiFileFormat? originalFormat) => Serialize(
            midiFile: new MidiFile(),
            originalFormat: originalFormat,
            settings: null,
            objectType: ObjectType.TimedEvent,
            objectDetectionSettings: null,
            expectedCsvLines: new[]
            {
                $"0,\"MThd\",0,\"Header\",{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote}",
            });

        [Test]
        public void Serialize() => Serialize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote })),
            originalFormat: null,
            settings: null,
            objectType: ObjectType.TimedEvent,
            objectDetectionSettings: null,
            expectedCsvLines: new[]
            {
                $"0,\"MThd\",0,\"Header\",{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote}",
                $"1,\"MTrk\",0,\"Text\",0,\"A\"",
                $"1,\"MTrk\",1,\"Text\",{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote},\"B\"",
            });

        [Test]
        public void Serialize_TimeType() => Serialize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote })),
            originalFormat: null,
            settings: new CsvSerializationSettings
            {
                TimeType = TimeSpanType.Musical
            },
            objectType: ObjectType.TimedEvent,
            objectDetectionSettings: null,
            expectedCsvLines: new[]
            {
                $"0,\"MThd\",0,\"Header\",{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote}",
                $"1,\"MTrk\",0,\"Text\",0/1,\"A\"",
                $"1,\"MTrk\",1,\"Text\",1/4,\"B\"",
            });

        [Test]
        public void Serialize_MultipleTrackChunks() => Serialize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote }),
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)4 },
                    new NoteOffEvent((SevenBitNumber)100, SevenBitNumber.MinValue) { Channel = (FourBitNumber)4, DeltaTime = TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote })),
            originalFormat: null,
            settings: new CsvSerializationSettings
            {
                TimeType = TimeSpanType.Musical
            },
            objectType: ObjectType.TimedEvent,
            objectDetectionSettings: null,
            expectedCsvLines: new[]
            {
                $"0,\"MThd\",0,\"Header\",{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote}",
                $"1,\"MTrk\",0,\"Text\",0/1,\"A\"",
                $"1,\"MTrk\",1,\"Text\",1/4,\"B\"",
                $"2,\"MTrk\",0,\"NoteOn\",0/1,4,100,127",
                $"2,\"MTrk\",1,\"NoteOff\",1/4,4,100,0",
            });

        [Test]
        public void Serialize_Notes() => Serialize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote }),
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)4 },
                    new NoteOffEvent((SevenBitNumber)100, SevenBitNumber.MinValue) { Channel = (FourBitNumber)4, DeltaTime = TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote })),
            originalFormat: null,
            settings: null,
            objectType: ObjectType.TimedEvent | ObjectType.Note,
            objectDetectionSettings: null,
            expectedCsvLines: new[]
            {
                $"0,\"MThd\",0,\"Header\",{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote}",
                $"1,\"MTrk\",0,\"Text\",0,\"A\"",
                $"1,\"MTrk\",1,\"Text\",{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote},\"B\"",
                $"2,\"MTrk\",0,\"Note\",0,{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote},4,100,127,0",
            });

        [Test]
        public void Serialize_Notes_Letter() => Serialize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote }),
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)4 },
                    new NoteOffEvent((SevenBitNumber)100, SevenBitNumber.MinValue) { Channel = (FourBitNumber)4, DeltaTime = TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote })),
            originalFormat: null,
            settings: new CsvSerializationSettings
            {
                NoteFormat = CsvNoteFormat.Letter,
                TimeType = TimeSpanType.Metric,
                LengthType = TimeSpanType.Musical,
            },
            objectType: ObjectType.TimedEvent | ObjectType.Note,
            objectDetectionSettings: null,
            expectedCsvLines: new[]
            {
                $"0,\"MThd\",0,\"Header\",{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote}",
                $"1,\"MTrk\",0,\"Text\",0:0:0:0,\"A\"",
                $"1,\"MTrk\",1,\"Text\",0:0:0:500,\"B\"",
                $"2,\"MTrk\",0,\"Note\",0:0:0:0,1/4,4,E7,127,0",
            });

        [Test]
        public void Serialize_AllObjectTypes() => Serialize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 100 }),
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)4 },
                    new NoteOffEvent((SevenBitNumber)100, SevenBitNumber.MinValue) { Channel = (FourBitNumber)4, DeltaTime = 100 },
                    new NoteOnEvent((SevenBitNumber)50, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)3 },
                    new NoteOnEvent((SevenBitNumber)40, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)3, DeltaTime = 10 },
                    new NoteOffEvent((SevenBitNumber)50, SevenBitNumber.MinValue) { Channel = (FourBitNumber)3, DeltaTime = 90 },
                    new NoteOffEvent((SevenBitNumber)40, SevenBitNumber.MinValue) { Channel = (FourBitNumber)3, DeltaTime = 10 })),
            originalFormat: null,
            settings: new CsvSerializationSettings
            {
                NoteFormat = CsvNoteFormat.Letter,
            },
            objectType: ObjectType.TimedEvent | ObjectType.Note | ObjectType.Chord,
            objectDetectionSettings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesMinCount = 2,
                    NotesTolerance = 10
                }
            },
            expectedCsvLines: new[]
            {
                $"0,\"MThd\",0,\"Header\",{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote}",
                $"1,\"MTrk\",0,\"Text\",0,\"A\"",
                $"1,\"MTrk\",1,\"Text\",100,\"B\"",
                $"2,\"MTrk\",0,\"Note\",0,100,4,E7,127,0",
                $"2,\"MTrk\",1,\"Note\",100,100,3,D3,127,0",
                $"2,\"MTrk\",1,\"Note\",110,100,3,E2,127,0",
            });

        [Test]
        public void Serialize_Delimiter() => Serialize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote },
                    new NormalSysExEvent(new byte[] { 9, 10, 15, 255 }))),
            originalFormat: null,
            settings: new CsvSerializationSettings
            {
                TimeType = TimeSpanType.Musical,
                Delimiter = ' ',
            },
            objectType: ObjectType.TimedEvent,
            objectDetectionSettings: null,
            expectedCsvLines: new[]
            {
                $"0 \"MThd\" 0 \"Header\" {TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote}",
                $"1 \"MTrk\" 0 \"Text\" 0/1 \"A\"",
                $"1 \"MTrk\" 1 \"Text\" 1/4 \"B\"",
                $"1 \"MTrk\" 2 \"NormalSysEx\" 1/4 \"9 10 15 255\"",
            });

        [Test]
        public void Serialize_BytesArrayFormat() => Serialize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"),
                    new NormalSysExEvent(new byte[] { 9, 10, 15, 255 }))),
            originalFormat: null,
            settings: new CsvSerializationSettings
            {
                BytesArrayFormat = CsvBytesArrayFormat.Hexadecimal,
            },
            objectType: ObjectType.TimedEvent,
            objectDetectionSettings: null,
            expectedCsvLines: new[]
            {
                $"0,\"MThd\",0,\"Header\",{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote}",
                $"1,\"MTrk\",0,\"Text\",0,\"A\"",
                $"1,\"MTrk\",1,\"NormalSysEx\",0,\"09 0A 0F FF\"",
            });

        [Test]
        public void Serialize_NewlinesAndQuotes() => Serialize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("\"in\" \"quotes\""),
                    new TextEvent($"B{Environment.NewLine}bb\"in quotes\""),
                    new TextEvent("C"))),
            originalFormat: null,
            settings: null,
            objectType: ObjectType.TimedEvent,
            objectDetectionSettings: null,
            expectedCsvLines: new[]
            {
                $"0,\"MThd\",0,\"Header\",{TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote}",
                $"1,\"MTrk\",0,\"Text\",0,\"\"\"in\"\" \"\"quotes\"\"\"",
                $"1,\"MTrk\",1,\"Text\",0,\"B",
                $"bb\"\"in quotes\"\"\"",
                $"1,\"MTrk\",2,\"Text\",0,\"C\"",
            },
            checkSeparateChunks: false);

        #endregion

        #region Private methods

        private void Serialize(
            MidiFile midiFile,
            MidiFileFormat? originalFormat,
            CsvSerializationSettings settings,
            ObjectType objectType,
            ObjectDetectionSettings objectDetectionSettings,
            string[] expectedCsvLines,
            bool checkSeparateChunks = true)
        {
            if (originalFormat != null)
                midiFile = MidiFileTestUtilities.Read(midiFile, null, null, originalFormat);

            var filePath = FileOperations.GetTempFilePath();

            try
            {
                Serialize_File(
                    midiFile,
                    filePath,
                    settings,
                    objectType,
                    objectDetectionSettings,
                    expectedCsvLines);

                Serialize_ChunksFromFile(
                    midiFile,
                    filePath,
                    settings,
                    objectType,
                    objectDetectionSettings,
                    expectedCsvLines);

                if (checkSeparateChunks)
                {
                    Serialize_ChunkFromFile(
                        midiFile,
                        filePath,
                        settings,
                        objectType,
                        objectDetectionSettings,
                        expectedCsvLines);

                    Serialize_ObjectsFromFile(
                        midiFile,
                        filePath,
                        settings,
                        objectType,
                        objectDetectionSettings,
                        expectedCsvLines);
                }
            }
            finally
            {
                FileOperations.DeleteFile(filePath);
            }
        }

        private void Serialize_File(
            MidiFile midiFile,
            string filePath,
            CsvSerializationSettings settings,
            ObjectType objectType,
            ObjectDetectionSettings objectDetectionSettings,
            string[] expectedCsvLines)
        {
            midiFile.SerializeToCsv(filePath, true, settings, objectType, objectDetectionSettings);
            var csvLines = GetCsvLines(filePath);

            CollectionAssert.AreEqual(expectedCsvLines, csvLines, "Invalid CSV lines for file.");
        }

        private void Serialize_ChunksFromFile(
            MidiFile midiFile,
            string filePath,
            CsvSerializationSettings settings,
            ObjectType objectType,
            ObjectDetectionSettings objectDetectionSettings,
            string[] expectedCsvLines)
        {
            var tempoMap = midiFile.GetTempoMap();
            midiFile.Chunks.SerializeToCsv(filePath, true, tempoMap, settings, objectType, objectDetectionSettings);
            var csvLines = GetCsvLines(filePath);

            CollectionAssert.AreEqual(
                expectedCsvLines
                    .Skip(1)
                    .Select(l => Regex.Replace(l, @"^\d+?", m => $"{int.Parse(m.Value) - 1}")),
                csvLines,
                "Invalid CSV lines for chunks.");
        }

        private void Serialize_ChunkFromFile(
            MidiFile midiFile,
            string filePath,
            CsvSerializationSettings settings,
            ObjectType objectType,
            ObjectDetectionSettings objectDetectionSettings,
            string[] expectedCsvLines)
        {
            var i = 0;
            var tempoMap = midiFile.GetTempoMap();

            foreach (var chunk in midiFile.Chunks)
            {
                chunk.SerializeToCsv(filePath, true, tempoMap, settings, objectType, objectDetectionSettings);
                var csvLines = GetCsvLines(filePath);

                CollectionAssert.AreEqual(
                    expectedCsvLines
                        .Skip(1)
                        .Where(l => Regex.Match(l, @"^(\d+?)").Value == (i + 1).ToString())
                        .Select(l => Regex.Replace(l, @"^\d+?", m => "0")),
                    csvLines,
                    $"Invalid CSV lines for chunk {i}.");

                i++;
            }
        }

        private void Serialize_ObjectsFromFile(
            MidiFile midiFile,
            string filePath,
            CsvSerializationSettings settings,
            ObjectType objectType,
            ObjectDetectionSettings objectDetectionSettings,
            string[] expectedCsvLines)
        {
            var i = 0;
            var tempoMap = midiFile.GetTempoMap();
            var delimiter = settings?.Delimiter ?? ',';

            foreach (var chunk in midiFile.Chunks)
            {
                var objects = ((TrackChunk)chunk).GetObjects(objectType, objectDetectionSettings);

                objects.SerializeToCsv(filePath, true, tempoMap, settings);
                var csvLines = GetCsvLines(filePath);

                CollectionAssert.AreEqual(
                    expectedCsvLines
                        .Skip(1)
                        .Where(l => Regex.Match(l, @"^(\d+?)").Value == (i + 1).ToString())
                        .Select(l => Regex.Replace(l, $@"^.+?{delimiter}.+?{delimiter}", m => string.Empty)),
                    csvLines,
                    $"Invalid CSV lines for objects of chunk {i}.");

                i++;
            }
        }

        private static string[] GetCsvLines(string filePath) => FileOperations
            .ReadAllFileLines(filePath)
            .Select(l => l.Trim())
            .ToArray();

        #endregion
    }
}
