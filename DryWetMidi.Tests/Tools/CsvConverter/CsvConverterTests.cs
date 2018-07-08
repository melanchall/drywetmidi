using System;
using System.IO;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed class CsvConverterTests
    {
        #region Test methods

        #region Convert to/from CSV
        
        [TestCase(MidiFileCsvLayout.DryWetMidi)]
        [TestCase(MidiFileCsvLayout.MidiCsv)]
        public void ConvertMidiFileToFromCsv(MidiFileCsvLayout layout)
        {
            var settings = new MidiFileCsvConversionSettings
            {
                CsvLayout = layout
            };

            ConvertMidiFileToFromCsv(settings);
        }

        #endregion

        #region CsvToMidiFile

        [TestCase(MidiFileCsvLayout.DryWetMidi, new[] { ",,Header,MultiTrack,1000" })]
        [TestCase(MidiFileCsvLayout.MidiCsv, new[] { ",,Header,1,0,1000" })]
        public void ConvertCsvToMidiFile_NoEvents(MidiFileCsvLayout layout, string[] csvLines)
        {
            var midiFile = ConvertCsvToMidiFile(layout, TimeSpanType.Midi, csvLines);

            Assert.AreEqual(MidiFileFormat.MultiTrack, midiFile.OriginalFormat, "File format is invalid.");
            Assert.AreEqual(new TicksPerQuarterNoteTimeDivision(1000), midiFile.TimeDivision, "Time division is invalid.");
        }

        [TestCase(MidiFileCsvLayout.DryWetMidi, new[] { "0,0,Set Tempo,100000" })]
        [TestCase(MidiFileCsvLayout.MidiCsv, new[] { "0,0,Tempo,100000" })]
        public void ConvertCsvToMidiFile_NoHeader(MidiFileCsvLayout layout, string[] csvLines)
        {
            var midiFile = ConvertCsvToMidiFile(layout, TimeSpanType.Midi, csvLines);

            Assert.AreEqual(new TicksPerQuarterNoteTimeDivision(TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote),
                            midiFile.TimeDivision,
                            "Time division is invalid.");
            Assert.Throws<InvalidOperationException>(() => { var format = midiFile.OriginalFormat; });
        }

        [TestCase(MidiFileCsvLayout.DryWetMidi, true, new[]
        {
            "0, 0, Note On, 10, 50, 120",
            "0, 0, Text, \"Test\"",
            "0, 100, Note On, 7, 50, 110",
            "0, 250, Note Off, 10, 50, 70",
            "0, 1000, Note Off, 7, 50, 80"
        })]
        [TestCase(MidiFileCsvLayout.DryWetMidi, false, new[]
        {
            "0, 0, Note On, 10, 50, 120",
            "0, 0, Text, \"Test\"",
            "0, 100, Note On, 7, 50, 110",
            "0, 250, Note Off, 10, 50, 70",
            "0, 1000, Note Off, 7, 50, 80"
        })]
        [TestCase(MidiFileCsvLayout.MidiCsv, true, new[]
        {
            "0, 0, Note_On_c, 10, 50, 120",
            "0, 0, Text_t, \"Test\"",
            "0, 100, Note_On_c, 7, 50, 110",
            "0, 250, Note_Off_C, 10, 50, 70",
            "0, 1000, Note_Off_c, 7, 50, 80"
        })]
        [TestCase(MidiFileCsvLayout.MidiCsv, false, new[]
        {
            "0, 0, Note_On_c, 10, 50, 120",
            "0, 0, Text_t, \"Test\"",
            "0, 100, Note_On_c, 7, 50, 110",
            "0, 250, Note_Off_C, 10, 50, 70",
            "0, 1000, Note_Off_c, 7, 50, 80"
        })]
        public void ConvertCsvToMidiFile_SingleTrackChunk(MidiFileCsvLayout layout, bool orderEvents, string[] csvLines)
        {
            if (!orderEvents)
            {
                var tmp = csvLines[2];
                csvLines[2] = csvLines[4];
                csvLines[4] = tmp;
            }

            var midiFile = ConvertCsvToMidiFile(layout, TimeSpanType.Midi, csvLines);

            var expectedEvents = new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)120) { Channel = (FourBitNumber)10 }, 0),
                new TimedEvent(new TextEvent("Test"), 0),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)110) { Channel = (FourBitNumber)7 }, 100),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)70) { Channel = (FourBitNumber)10 }, 250),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)80) { Channel = (FourBitNumber)7 }, 1000)
            };

            Assert.AreEqual(1, midiFile.GetTrackChunks().Count(), "Track chunks count is invalid.");
            Assert.IsTrue(TimedEventEquality.AreEqual(expectedEvents, midiFile.GetTimedEvents(), false),
                          "Invalid events.");
        }

        [TestCase(MidiFileCsvLayout.DryWetMidi, true, new[]
        {
            ", , header, singletrack, 500",
            "0, 0:0:0, note on, 10, 50, 120",
            "0, 0:0:0, text, \"Test\"",
            "0, 0:1:0, note on, 7, 50, 110",
            "",
            "0, 0:1:3, set tempo, 300000",
            "0, 0:1:10, note off, 10, 50, 70",
            "",
            "",
            "0, 0:10:3, note off, 7, 50, 80"
        })]
        [TestCase(MidiFileCsvLayout.DryWetMidi, false, new[]
        {
            ", , header, singletrack, 500",
            "0, 0:0:0, note on, 10, 50, 120",
            "0, 0:0:0, text, \"Test\"",
            "0, 0:1:0, note on, 7, 50, 110",
            "",
            "0, 0:1:3, set tempo, 300000",
            "0, 0:1:10, note off, 10, 50, 70",
            "",
            "",
            "0, 0:10:3, note off, 7, 50, 80"
        })]
        [TestCase(MidiFileCsvLayout.MidiCsv, true, new[]
        {
            ", , header, singletrack, 1, 500",
            "0, 0:0:0, note_on_c, 10, 50, 120",
            "0, 0:0:0, text_t, \"Test\"",
            "0, 0:1:0, note_on_c, 7, 50, 110",
            "",
            "0, 0:1:3, tempo, 300000",
            "0, 0:1:10, note_off_c, 10, 50, 70",
            "",
            "",
            "0, 0:10:3, note_off_c, 7, 50, 80"
        })]
        [TestCase(MidiFileCsvLayout.MidiCsv, false, new[]
        {
            ", , header, singletrack, 1, 500",
            "0, 0:0:0, note_on_c, 10, 50, 120",
            "0, 0:0:0, text_t, \"Test\"",
            "0, 0:1:0, note_on_c, 7, 50, 110",
            "",
            "0, 0:1:3, tempo, 300000",
            "0, 0:1:10, note_off_c, 10, 50, 70",
            "",
            "",
            "0, 0:10:3, note_off_c, 7, 50, 80"
        })]
        public void ConvertCsvToMidiFile_SingleTrackChunk_MetricTimes(MidiFileCsvLayout layout, bool orderEvents, string[] csvLines)
        {
            if (!orderEvents)
            {
                var tmp = csvLines[2];
                csvLines[2] = csvLines[5];
                csvLines[5] = tmp;
            }

            var midiFile = ConvertCsvToMidiFile(layout, TimeSpanType.Metric, csvLines);

            TempoMap expectedTempoMap;
            using (var tempoMapManager = new TempoMapManager(new TicksPerQuarterNoteTimeDivision(500)))
            {
                tempoMapManager.SetTempo(new MetricTimeSpan(0, 1, 3), new Tempo(300000));
                expectedTempoMap = tempoMapManager.TempoMap;
            }

            var expectedEvents = new[]
            {
                new TimeAndMidiEvent(new MetricTimeSpan(),
                                     new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)120) { Channel = (FourBitNumber)10 }),
                new TimeAndMidiEvent(new MetricTimeSpan(),
                                     new TextEvent("Test")),
                new TimeAndMidiEvent(new MetricTimeSpan(0, 1, 0),
                                     new NoteOnEvent((SevenBitNumber)50, (SevenBitNumber)110) { Channel = (FourBitNumber)7 }),
                new TimeAndMidiEvent(new MetricTimeSpan(0, 1, 3),
                                     new SetTempoEvent(300000)),
                new TimeAndMidiEvent(new MetricTimeSpan(0, 1, 10),
                                     new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)70) { Channel = (FourBitNumber)10 }),
                new TimeAndMidiEvent(new MetricTimeSpan(0, 10, 3),
                                     new NoteOffEvent((SevenBitNumber)50, (SevenBitNumber)80) { Channel = (FourBitNumber)7 })
            }
            .Select(te => new TimedEvent(te.Event, TimeConverter.ConvertFrom(te.Time, expectedTempoMap)))
            .ToArray();

            Assert.AreEqual(1, midiFile.GetTrackChunks().Count(), "Track chunks count is invalid.");
            CollectionAssert.AreEqual(midiFile.GetTempoMap().Tempo, expectedTempoMap.Tempo, "Invalid tempo map.");
            Assert.AreEqual(new TicksPerQuarterNoteTimeDivision(500), midiFile.TimeDivision, "Invalid time division.");
            Assert.IsTrue(TimedEventEquality.AreEqual(expectedEvents, midiFile.GetTimedEvents(), false),
                          "Invalid events.");
        }

        [TestCase(MidiFileCsvLayout.DryWetMidi, new[]
        {
            "0, 0, Text, \"Test",
            " text with new line\"",
            "0, 100, Marker, \"Marker\"",
            "0, 200, Text, \"Test",
            " text with new line and",
            " new \"\"line again\""
        })]
        [TestCase(MidiFileCsvLayout.MidiCsv, new[]
        {
            "0, 0, Text_t, \"Test",
            " text with new line\"",
            "0, 100, Marker_t, \"Marker\"",
            "0, 200, Text_t, \"Test",
            " text with new line and",
            " new \"\"line again\""
        })]
        public void ConvertCsvToMidiFile_NewLines(MidiFileCsvLayout layout, string[] csvLines)
        {
            var midiFile = ConvertCsvToMidiFile(layout, TimeSpanType.Midi, csvLines);

            var expectedEvents = new[]
            {
                new TimedEvent(new TextEvent($"Test{Environment.NewLine} text with new line"), 0),
                new TimedEvent(new MarkerEvent("Marker"), 100),
                new TimedEvent(new TextEvent($"Test{Environment.NewLine} text with new line and{Environment.NewLine} new \"\"line again"), 200),
            };

            Assert.IsTrue(TimedEventEquality.AreEqual(expectedEvents, midiFile.GetTimedEvents(), false),
                          "Invalid events.");
        }

        #endregion

        #region MidiFileToCsv

        [TestCase(MidiFileCsvLayout.DryWetMidi, new[] { ",,header,,96" })]
        [TestCase(MidiFileCsvLayout.MidiCsv, new[]
        {
            "0,0,header,0,0,96",
            "0,0,end_of_file"
        })]
        public void ConvertMidiFileToCsv_EmptyFile(MidiFileCsvLayout layout, string[] expectedCsvLines)
        {
            var midiFile = new MidiFile();
            ConvertMidiFileToCsv(midiFile, layout, TimeSpanType.Midi, expectedCsvLines);
        }

        [TestCase(MidiFileCsvLayout.DryWetMidi, new[]
        {
            ",,header,,96",
            "0,0,time signature,2,8,24,8",
            "0,345,text,\"Test text\"",
            "0,350,note on,0,23,78",
            "0,450,note off,0,23,90",
            "0,800,sequencer specific,3,1,2,3"
        })]
        [TestCase(MidiFileCsvLayout.MidiCsv, new[]
        {
            "0,0,header,0,1,96",
            "0,0,start_track",
            "0,0,time_signature,2,3,24,8",
            "0,345,text_t,\"Test text\"",
            "0,350,note_on_c,0,23,78",
            "0,450,note_off_c,0,23,90",
            "0,800,sequencer_specific,3,1,2,3",
            "0,800,end_track",
            "0,0,end_of_file",
        })]
        public void ConvertMidiFileToCsv_SingleTrack(MidiFileCsvLayout layout, string[] expectedCsvLines)
        {
            var timedEvents = new[]
            {
                new TimedEvent(new TimeSignatureEvent(2, 8), 0),
                new TimedEvent(new TextEvent("Test text"), 345),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)23, (SevenBitNumber)78), 350),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)23, (SevenBitNumber)90), 450),
                new TimedEvent(new SequencerSpecificEvent(new byte[] { 1, 2, 3 }), 800)
            };

            var midiFile = timedEvents.ToFile();

            ConvertMidiFileToCsv(midiFile, layout, TimeSpanType.Midi, expectedCsvLines);
        }

        [TestCase(MidiFileCsvLayout.DryWetMidi, new[]
        {
            ",,header,,96",
            "0,0,time signature,2,8,24,8",
            "0,345,text,\"Test text\"",
            "0,350,note on,0,23,78",
            "0,450,note off,0,23,90",
            "0,800,sequencer specific,3,1,2,3",
            "1,10,note on,0,30,78",
            "1,20,note off,0,30,90",
        })]
        [TestCase(MidiFileCsvLayout.MidiCsv, new[]
        {
            "0,0,header,1,2,96",
            "0,0,start_track",
            "0,0,time_signature,2,3,24,8",
            "0,345,text_t,\"Test text\"",
            "0,350,note_on_c,0,23,78",
            "0,450,note_off_c,0,23,90",
            "0,800,sequencer_specific,3,1,2,3",
            "0,800,end_track",
            "1,0,start_track",
            "1,10,note_on_c,0,30,78",
            "1,20,note_off_c,0,30,90",
            "1,20,end_track",
            "0,0,end_of_file",
        })]
        public void ConvertMidiFileToCsv_MultipleTrack(MidiFileCsvLayout layout, string[] expectedCsvLines)
        {
            var timedEvents1 = new[]
            {
                new TimedEvent(new TimeSignatureEvent(2, 8), 0),
                new TimedEvent(new TextEvent("Test text"), 345),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)23, (SevenBitNumber)78), 350),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)23, (SevenBitNumber)90), 450),
                new TimedEvent(new SequencerSpecificEvent(new byte[] { 1, 2, 3 }), 800)
            };

            var timedEvents2 = new[]
            {
                new TimedEvent(new NoteOnEvent((SevenBitNumber)30, (SevenBitNumber)78), 10),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)30, (SevenBitNumber)90), 20)
            };

            var midiFile = new MidiFile(
                timedEvents1.ToTrackChunk(),
                timedEvents2.ToTrackChunk());

            ConvertMidiFileToCsv(midiFile, layout, TimeSpanType.Midi, expectedCsvLines);
        }

        #endregion

        #endregion

        #region Private methods

        private static void ConvertMidiFileToFromCsv(MidiFileCsvConversionSettings settings)
        {
            var tempPath = Path.GetTempPath();
            var outputDirectory = Path.Combine(tempPath, Guid.NewGuid().ToString());
            Directory.CreateDirectory(outputDirectory);

            try
            {
                foreach (var filePath in TestFilesProvider.GetValidFiles())
                {
                    var midiFile = MidiFile.Read(filePath);
                    var outputFilePath = Path.Combine(outputDirectory, Path.GetFileName(Path.ChangeExtension(filePath, "csv")));

                    var csvConverter = new CsvConverter();
                    csvConverter.ConvertMidiFileToCsv(midiFile, outputFilePath, true, settings);
                    csvConverter.ConvertCsvToMidiFile(outputFilePath, settings);
                }
            }
            finally
            {
                Directory.Delete(outputDirectory, true);
            }
        }

        private static void ConvertMidiFileToFromCsv(MidiFile midiFile, string outputFilePath, MidiFileCsvConversionSettings settings)
        {
            var csvConverter = new CsvConverter();
            csvConverter.ConvertMidiFileToCsv(midiFile, outputFilePath, true, settings);
            csvConverter.ConvertCsvToMidiFile(outputFilePath, settings);
        }

        private static MidiFile ConvertCsvToMidiFile(MidiFileCsvLayout layout, TimeSpanType timeType, string[] csvLines)
        {
            var filePath = Path.GetTempFileName();
            File.WriteAllLines(filePath, csvLines);

            var settings = new MidiFileCsvConversionSettings
            {
                CsvLayout = layout,
                TimeType = timeType
            };

            try
            {
                var midiFile = new CsvConverter().ConvertCsvToMidiFile(filePath, settings);
                ConvertMidiFileToFromCsv(midiFile, filePath, settings);
                return midiFile;
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        private static void ConvertMidiFileToCsv(MidiFile midiFile, MidiFileCsvLayout layout, TimeSpanType timeType, string[] expectedCsvLines)
        {
            var filePath = Path.GetTempFileName();

            var settings = new MidiFileCsvConversionSettings
            {
                CsvLayout = layout,
                TimeType = timeType
            };

            try
            {
                new CsvConverter().ConvertMidiFileToCsv(midiFile, filePath, true, settings);
                var actualCsvLines = File.ReadAllLines(filePath);
                CollectionAssert.AreEqual(expectedCsvLines, actualCsvLines, StringComparer.OrdinalIgnoreCase);
            }
            finally
            {
                File.Delete(filePath);
            }
        }

        #endregion
    }
}
