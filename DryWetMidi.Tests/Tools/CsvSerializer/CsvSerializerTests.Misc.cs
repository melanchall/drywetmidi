using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class CsvSerializerTests
    {
        #region Constants

        private static readonly CsvSerializationSettings DefaultSettings = new CsvSerializationSettings();
        private static readonly CsvSerializationSettings HexBytesSettings = new CsvSerializationSettings
        {
            BytesArrayFormat = CsvBytesArrayFormat.Hexadecimal,
        };
        private static readonly CsvSerializationSettings NoteLetterSettings = new CsvSerializationSettings
        {
            NoteFormat = CsvNoteFormat.Letter,
        };

        private static readonly object[][] EventsData = new[]
        {
            new object[] { new NormalSysExEvent(new byte[] { 100, 0, 123 }), "\"100 0 123\"", DefaultSettings },
            new object[] { new NormalSysExEvent(new byte[] { 100, 0, 123 }), "\"64 00 7B\"", HexBytesSettings },
            new object[] { new EscapeSysExEvent(new byte[] { 102, 10, 0 }), "\"102 10 0\"", DefaultSettings },
            new object[] { new EscapeSysExEvent(new byte[] { 102, 10, 0 }), "\"66 0A 00\"", HexBytesSettings },
            new object[] { new SequenceNumberEvent(23), "23", DefaultSettings },
            new object[] { new TextEvent("Just want"), "\"Just want\"", DefaultSettings },
            new object[] { new CopyrightNoticeEvent("to know"), "\"to know\"", DefaultSettings },
            new object[] { new SequenceTrackNameEvent("whether the tests"), "\"whether the tests\"", DefaultSettings },
            new object[] { new InstrumentNameEvent("pass or not. "), "\"pass or not. \"", DefaultSettings },
            new object[] { new LyricEvent("Here some lyric."), "\"Here some lyric.\"", DefaultSettings },
            new object[] { new MarkerEvent("But there some marker..."), "\"But there some marker...\"", DefaultSettings },
            new object[] { new CuePointEvent("Just a cue point with\r\nnewline"), "\"Just a cue point with\r\nnewline\"", DefaultSettings },
            new object[] { new ProgramNameEvent("Program? DryWetMIDI, of course!"), "\"Program? DryWetMIDI, of course!\"", DefaultSettings },
            new object[] { new DeviceNameEvent("No device"), "\"No device\"", DefaultSettings },
            new object[] { new ChannelPrefixEvent(34), "34", DefaultSettings },
            new object[] { new PortPrefixEvent(43), "43", DefaultSettings },
            new object[] { new SetTempoEvent(123456), "123456", DefaultSettings },
            new object[] { new SmpteOffsetEvent(SmpteFormat.ThirtyDrop, 5, 4, 3, 2, 1), "\"ThirtyDrop\",5,4,3,2,1", DefaultSettings },
            new object[] { new TimeSignatureEvent(3, 8, 56, 32), "3,8,56,32", DefaultSettings },
            new object[] { new KeySignatureEvent(5, 1), "5,1", DefaultSettings },
            new object[] { new SequencerSpecificEvent(new byte[] { 43, 1, 11, 56 }), "\"43 1 11 56\"", DefaultSettings },
            new object[] { new SequencerSpecificEvent(new byte[] { 43, 1, 11, 56 }), "\"2B 01 0B 38\"", HexBytesSettings },
            new object[] { new UnknownMetaEvent(100, new byte[] { 2, 0, 3, 123 }), "100,\"2 0 3 123\"", DefaultSettings },
            new object[] { new UnknownMetaEvent(100, new byte[] { 2, 0, 3, 123 }), "100,\"02 00 03 7B\"", HexBytesSettings },
            new object[] { new NoteOffEvent((SevenBitNumber)45, (SevenBitNumber)56) { Channel = (FourBitNumber)5 }, "5,45,56", DefaultSettings },
            new object[] { new NoteOffEvent((SevenBitNumber)45, (SevenBitNumber)56) { Channel = (FourBitNumber)5 }, "5,A2,56", NoteLetterSettings },
            new object[] { new NoteOnEvent((SevenBitNumber)54, (SevenBitNumber)65) { Channel = (FourBitNumber)3 }, "3,54,65", DefaultSettings },
            new object[] { new NoteOnEvent((SevenBitNumber)54, (SevenBitNumber)65) { Channel = (FourBitNumber)3 }, "3,F#3,65", NoteLetterSettings },
            new object[] { new NoteAftertouchEvent((SevenBitNumber)123, (SevenBitNumber)100) { Channel = (FourBitNumber)2 }, "2,123,100", DefaultSettings },
            new object[] { new NoteAftertouchEvent((SevenBitNumber)123, (SevenBitNumber)100) { Channel = (FourBitNumber)2 }, "2,D#9,100", NoteLetterSettings },
            new object[] { new ControlChangeEvent((SevenBitNumber)78, (SevenBitNumber)10) { Channel = (FourBitNumber)1 }, "1,78,10", DefaultSettings },
            new object[] { new ProgramChangeEvent((SevenBitNumber)98) { Channel = (FourBitNumber)11 }, "11,98", DefaultSettings },
            new object[] { new ChannelAftertouchEvent((SevenBitNumber)89) { Channel = (FourBitNumber)14 }, "14,89", DefaultSettings },
            new object[] { new PitchBendEvent(3456) { Channel = (FourBitNumber)7 }, "7,3456", DefaultSettings },
            new object[] { new TimingClockEvent(), string.Empty, DefaultSettings },
            new object[] { new StartEvent(), string.Empty, DefaultSettings },
            new object[] { new ContinueEvent(), string.Empty, DefaultSettings },
            new object[] { new StopEvent(), string.Empty, DefaultSettings },
            new object[] { new ActiveSensingEvent(), string.Empty, DefaultSettings },
            new object[] { new ResetEvent(), string.Empty, DefaultSettings },
            new object[] { new MidiTimeCodeEvent(MidiTimeCodeComponent.SecondsMsb, (FourBitNumber)3), "\"SecondsMsb\",3", DefaultSettings },
            new object[] { new SongPositionPointerEvent(13), "13", DefaultSettings },
            new object[] { new SongSelectEvent((SevenBitNumber)69), "69", DefaultSettings },
            new object[] { new TuneRequestEvent(), string.Empty, DefaultSettings },
        };

        #endregion

        #region Test methods

        [Test]
        public void SerializeDeserialize_AllEventsTypesChecked()
        {
            var allEventsTypes = Enum
                .GetValues(typeof(MidiEventType))
                .Cast<MidiEventType>()
                .Except(new[] { MidiEventType.EndOfTrack, MidiEventType.CustomMeta })
                .ToArray();
            var checkedEventsTypes = EventsData
                .Select(d => ((MidiEvent)d.First()).EventType)
                .Distinct()
                .ToArray();

            CollectionAssert.AreEquivalent(allEventsTypes, checkedEventsTypes, "Some events types are not checked.");
        }

        [Test]
        public void SerializeDeserialize_ValidFiles()
        {
            var tempPath = Path.GetTempPath();
            var outputDirectory = Path.Combine(tempPath, Guid.NewGuid().ToString());
            Directory.CreateDirectory(outputDirectory);

            try
            {
                foreach (var filePath in TestFilesProvider.GetValidFilesPaths())
                {
                    var midiFile = MidiFile.Read(filePath);
                    var outputFilePath = Path.Combine(outputDirectory, Path.GetFileName(Path.ChangeExtension(filePath, "csv")));

                    midiFile.SerializeToCsv(outputFilePath, true, null);
                    var convertedFile = CsvSerializer.DeserializeFileFromCsv(outputFilePath, null);

                    MidiAsserts.AreEqual(midiFile, convertedFile, false, $"Conversion of '{filePath}' is invalid.");
                }
            }
            finally
            {
                Directory.Delete(outputDirectory, true);
            }
        }

        #endregion
    }
}
