using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.IO;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class CsvSerializerTests
    {
        #region Constants

        private static readonly CsvSerializationSettings DefaultSerializationSettings = new CsvSerializationSettings();
        private static readonly CsvSerializationSettings HexBytesSerializationSettings = new CsvSerializationSettings
        {
            BytesArrayFormat = CsvBytesArrayFormat.Hexadecimal,
        };
        private static readonly CsvSerializationSettings NoteLetterSerializationSettings = new CsvSerializationSettings
        {
            NoteFormat = CsvNoteFormat.Letter,
        };

        private static readonly object[][] EventsData = new[]
        {
            new object[] { new NormalSysExEvent(new byte[] { 100, 0, 123 }), "\"100 0 123\"", DefaultSerializationSettings },
            new object[] { new NormalSysExEvent(new byte[] { 100, 0, 123 }), "\"64 00 7B\"", HexBytesSerializationSettings },
            new object[] { new EscapeSysExEvent(new byte[] { 102, 10, 0 }), "\"102 10 0\"", DefaultSerializationSettings },
            new object[] { new EscapeSysExEvent(new byte[] { 102, 10, 0 }), "\"66 0A 00\"", HexBytesSerializationSettings },
            new object[] { new SequenceNumberEvent(23), "23", DefaultSerializationSettings },
            new object[] { new TextEvent("Just want"), "\"Just want\"", DefaultSerializationSettings },
            new object[] { new CopyrightNoticeEvent("to know"), "\"to know\"", DefaultSerializationSettings },
            new object[] { new SequenceTrackNameEvent("whether the tests"), "\"whether the tests\"", DefaultSerializationSettings },
            new object[] { new InstrumentNameEvent("pass or not. "), "\"pass or not. \"", DefaultSerializationSettings },
            new object[] { new LyricEvent("Here some lyric."), "\"Here some lyric.\"", DefaultSerializationSettings },
            new object[] { new MarkerEvent("But there some marker..."), "\"But there some marker...\"", DefaultSerializationSettings },
            new object[] { new CuePointEvent("Just a cue point with\r\nnewline"), "\"Just a cue point with\r\nnewline\"", DefaultSerializationSettings },
            new object[] { new ProgramNameEvent("Program? DryWetMIDI, of course!"), "\"Program? DryWetMIDI, of course!\"", DefaultSerializationSettings },
            new object[] { new DeviceNameEvent("No device"), "\"No device\"", DefaultSerializationSettings },
            new object[] { new ChannelPrefixEvent(34), "34", DefaultSerializationSettings },
            new object[] { new PortPrefixEvent(43), "43", DefaultSerializationSettings },
            new object[] { new SetTempoEvent(123456), "123456", DefaultSerializationSettings },
            new object[] { new SmpteOffsetEvent(SmpteFormat.ThirtyDrop, 5, 4, 3, 2, 1), "\"ThirtyDrop\",5,4,3,2,1", DefaultSerializationSettings },
            new object[] { new TimeSignatureEvent(3, 8, 56, 32), "3,8,56,32", DefaultSerializationSettings },
            new object[] { new KeySignatureEvent(5, 1), "5,1", DefaultSerializationSettings },
            new object[] { new SequencerSpecificEvent(new byte[] { 43, 1, 11, 56 }), "\"43 1 11 56\"", DefaultSerializationSettings },
            new object[] { new SequencerSpecificEvent(new byte[] { 43, 1, 11, 56 }), "\"2B 01 0B 38\"", HexBytesSerializationSettings },
            new object[] { new UnknownMetaEvent(100, new byte[] { 2, 0, 3, 123 }), "100,\"2 0 3 123\"", DefaultSerializationSettings },
            new object[] { new UnknownMetaEvent(100, new byte[] { 2, 0, 3, 123 }), "100,\"02 00 03 7B\"", HexBytesSerializationSettings },
            new object[] { new NoteOffEvent((SevenBitNumber)45, (SevenBitNumber)56) { Channel = (FourBitNumber)5 }, "5,45,56", DefaultSerializationSettings },
            new object[] { new NoteOffEvent((SevenBitNumber)45, (SevenBitNumber)56) { Channel = (FourBitNumber)5 }, "5,A2,56", NoteLetterSerializationSettings },
            new object[] { new NoteOnEvent((SevenBitNumber)54, (SevenBitNumber)65) { Channel = (FourBitNumber)3 }, "3,54,65", DefaultSerializationSettings },
            new object[] { new NoteOnEvent((SevenBitNumber)54, (SevenBitNumber)65) { Channel = (FourBitNumber)3 }, "3,F#3,65", NoteLetterSerializationSettings },
            new object[] { new NoteAftertouchEvent((SevenBitNumber)123, (SevenBitNumber)100) { Channel = (FourBitNumber)2 }, "2,123,100", DefaultSerializationSettings },
            new object[] { new NoteAftertouchEvent((SevenBitNumber)123, (SevenBitNumber)100) { Channel = (FourBitNumber)2 }, "2,D#9,100", NoteLetterSerializationSettings },
            new object[] { new ControlChangeEvent((SevenBitNumber)78, (SevenBitNumber)10) { Channel = (FourBitNumber)1 }, "1,78,10", DefaultSerializationSettings },
            new object[] { new ProgramChangeEvent((SevenBitNumber)98) { Channel = (FourBitNumber)11 }, "11,98", DefaultSerializationSettings },
            new object[] { new ChannelAftertouchEvent((SevenBitNumber)89) { Channel = (FourBitNumber)14 }, "14,89", DefaultSerializationSettings },
            new object[] { new PitchBendEvent(3456) { Channel = (FourBitNumber)7 }, "7,3456", DefaultSerializationSettings },
            new object[] { new TimingClockEvent(), string.Empty, DefaultSerializationSettings },
            new object[] { new StartEvent(), string.Empty, DefaultSerializationSettings },
            new object[] { new ContinueEvent(), string.Empty, DefaultSerializationSettings },
            new object[] { new StopEvent(), string.Empty, DefaultSerializationSettings },
            new object[] { new ActiveSensingEvent(), string.Empty, DefaultSerializationSettings },
            new object[] { new ResetEvent(), string.Empty, DefaultSerializationSettings },
            new object[] { new MidiTimeCodeEvent(MidiTimeCodeComponent.SecondsMsb, (FourBitNumber)3), "\"SecondsMsb\",3", DefaultSerializationSettings },
            new object[] { new SongPositionPointerEvent(13), "13", DefaultSerializationSettings },
            new object[] { new SongSelectEvent((SevenBitNumber)69), "69", DefaultSerializationSettings },
            new object[] { new TuneRequestEvent(), string.Empty, DefaultSerializationSettings },
        };

        private static readonly CsvDeserializationSettings DefaultDeserializationSettings = new CsvDeserializationSettings();
        private static readonly CsvDeserializationSettings HexBytesDeserializationSettings = new CsvDeserializationSettings
        {
            BytesArrayFormat = CsvBytesArrayFormat.Hexadecimal,
        };
        private static readonly CsvDeserializationSettings NoteLetterDeserializationSettings = new CsvDeserializationSettings
        {
            NoteFormat = CsvNoteFormat.Letter,
        };

        private static readonly object[][] EventsDataForDeserialization = new[]
        {
            new object[] { new NormalSysExEvent(new byte[] { 100, 0, 123 }), "\"100 0 123\"", DefaultDeserializationSettings },
            new object[] { new NormalSysExEvent(new byte[] { 100, 0, 123 }), "\"64 00 7B\"", HexBytesDeserializationSettings },
            new object[] { new EscapeSysExEvent(new byte[] { 102, 10, 0 }), "\"102 10 0\"", DefaultDeserializationSettings },
            new object[] { new EscapeSysExEvent(new byte[] { 102, 10, 0 }), "\"66 0A 00\"", HexBytesDeserializationSettings },
            new object[] { new SequenceNumberEvent(23), "23", DefaultDeserializationSettings },
            new object[] { new TextEvent("Just want"), "\"Just want\"", DefaultDeserializationSettings },
            new object[] { new CopyrightNoticeEvent("to know"), "\"to know\"", DefaultDeserializationSettings },
            new object[] { new SequenceTrackNameEvent("whether the tests"), "\"whether the tests\"", DefaultDeserializationSettings },
            new object[] { new InstrumentNameEvent("pass or not. "), "\"pass or not. \"", DefaultDeserializationSettings },
            new object[] { new LyricEvent("Here some lyric."), "\"Here some lyric.\"", DefaultDeserializationSettings },
            new object[] { new MarkerEvent("But there some marker..."), "\"But there some marker...\"", DefaultDeserializationSettings },
            new object[] { new CuePointEvent("Just a cue point with\r\nnewline"), "\"Just a cue point with\r\nnewline\"", DefaultDeserializationSettings },
            new object[] { new ProgramNameEvent("Program? DryWetMIDI, of course!"), "\"Program? DryWetMIDI, of course!\"", DefaultDeserializationSettings },
            new object[] { new DeviceNameEvent("No device"), "\"No device\"", DefaultDeserializationSettings },
            new object[] { new ChannelPrefixEvent(34), "34", DefaultDeserializationSettings },
            new object[] { new PortPrefixEvent(43), "43", DefaultDeserializationSettings },
            new object[] { new SetTempoEvent(123456), "123456", DefaultDeserializationSettings },
            new object[] { new SmpteOffsetEvent(SmpteFormat.ThirtyDrop, 5, 4, 3, 2, 1), "\"ThirtyDrop\",5,4,3,2,1", DefaultDeserializationSettings },
            new object[] { new TimeSignatureEvent(3, 8, 56, 32), "3,8,56,32", DefaultDeserializationSettings },
            new object[] { new KeySignatureEvent(5, 1), "5,1", DefaultDeserializationSettings },
            new object[] { new SequencerSpecificEvent(new byte[] { 43, 1, 11, 56 }), "\"43 1 11 56\"", DefaultDeserializationSettings },
            new object[] { new SequencerSpecificEvent(new byte[] { 43, 1, 11, 56 }), "\"2B 01 0B 38\"", HexBytesDeserializationSettings },
            new object[] { new UnknownMetaEvent(100, new byte[] { 2, 0, 3, 123 }), "100,\"2 0 3 123\"", DefaultDeserializationSettings },
            new object[] { new UnknownMetaEvent(100, new byte[] { 2, 0, 3, 123 }), "100,\"02 00 03 7B\"", HexBytesDeserializationSettings },
            new object[] { new NoteOffEvent((SevenBitNumber)45, (SevenBitNumber)56) { Channel = (FourBitNumber)5 }, "5,45,56", DefaultDeserializationSettings },
            new object[] { new NoteOffEvent((SevenBitNumber)45, (SevenBitNumber)56) { Channel = (FourBitNumber)5 }, "5,A2,56", NoteLetterDeserializationSettings },
            new object[] { new NoteOnEvent((SevenBitNumber)54, (SevenBitNumber)65) { Channel = (FourBitNumber)3 }, "3,54,65", DefaultDeserializationSettings },
            new object[] { new NoteOnEvent((SevenBitNumber)54, (SevenBitNumber)65) { Channel = (FourBitNumber)3 }, "3,F#3,65", NoteLetterDeserializationSettings },
            new object[] { new NoteAftertouchEvent((SevenBitNumber)123, (SevenBitNumber)100) { Channel = (FourBitNumber)2 }, "2,123,100", DefaultDeserializationSettings },
            new object[] { new NoteAftertouchEvent((SevenBitNumber)123, (SevenBitNumber)100) { Channel = (FourBitNumber)2 }, "2,D#9,100", NoteLetterDeserializationSettings },
            new object[] { new ControlChangeEvent((SevenBitNumber)78, (SevenBitNumber)10) { Channel = (FourBitNumber)1 }, "1,78,10", DefaultDeserializationSettings },
            new object[] { new ProgramChangeEvent((SevenBitNumber)98) { Channel = (FourBitNumber)11 }, "11,98", DefaultDeserializationSettings },
            new object[] { new ChannelAftertouchEvent((SevenBitNumber)89) { Channel = (FourBitNumber)14 }, "14,89", DefaultDeserializationSettings },
            new object[] { new PitchBendEvent(3456) { Channel = (FourBitNumber)7 }, "7,3456", DefaultDeserializationSettings },
            new object[] { new TimingClockEvent(), string.Empty, DefaultDeserializationSettings },
            new object[] { new StartEvent(), string.Empty, DefaultDeserializationSettings },
            new object[] { new ContinueEvent(), string.Empty, DefaultDeserializationSettings },
            new object[] { new StopEvent(), string.Empty, DefaultDeserializationSettings },
            new object[] { new ActiveSensingEvent(), string.Empty, DefaultDeserializationSettings },
            new object[] { new ResetEvent(), string.Empty, DefaultDeserializationSettings },
            new object[] { new MidiTimeCodeEvent(MidiTimeCodeComponent.SecondsMsb, (FourBitNumber)3), "\"SecondsMsb\",3", DefaultDeserializationSettings },
            new object[] { new SongPositionPointerEvent(13), "13", DefaultDeserializationSettings },
            new object[] { new SongSelectEvent((SevenBitNumber)69), "69", DefaultDeserializationSettings },
            new object[] { new TuneRequestEvent(), string.Empty, DefaultDeserializationSettings },
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
