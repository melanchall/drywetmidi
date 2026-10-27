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
using System.Threading;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class CsvSerializerTests
    {
        #region Nested classes

        public enum CsvOverloadFamily
        {
            File,
            Chunks,
            Chunk,
            Objects
        }

        private sealed class AsyncOnlyStream : Stream
        {
            private readonly MemoryStream _storage;
            internal int Reads;
            internal int Writes;
            internal int Flushes;
            internal bool FailRead;
            internal bool FailWrite;
            internal bool BlockRead;
            internal bool BlockWrite;
            internal bool CompleteSynchronously;
            internal Action OnRead;
            internal Action OnWrite;
            internal bool Disposed;

            internal AsyncOnlyStream(byte[] bytes = null)
            {
                _storage = bytes == null ? new MemoryStream() : new MemoryStream(bytes);
            }

            internal byte[] ToArray() => _storage.ToArray();

            public override bool CanRead => true;

            public override bool CanSeek => false;

            public override bool CanWrite => true;

            public override long Length => throw new NotSupportedException();

            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override void Flush() => throw new InvalidOperationException("Synchronous flush.");

            public override int Read(byte[] buffer, int offset, int count) => throw new InvalidOperationException("Synchronous read.");

            public override void Write(byte[] buffer, int offset, int count) => throw new InvalidOperationException("Synchronous write.");

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            public override void SetLength(long value) => throw new NotSupportedException();

            public override async Task<int> ReadAsync(
                byte[] buffer,
                int offset,
                int count,
                CancellationToken cancellationToken)
            {
                Reads++;
                OnRead?.Invoke();

                if (!CompleteSynchronously)
                    await Task.Yield();

                if (BlockRead)
                    await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                if (FailRead)
                    throw new IOException("Read failure.");

                return await _storage.ReadAsync(buffer, offset, Math.Min(count, 17), cancellationToken).ConfigureAwait(false);
            }

            public override async Task WriteAsync(
                byte[] buffer,
                int offset,
                int count,
                CancellationToken cancellationToken)
            {
                Writes++;

                if (!CompleteSynchronously)
                    await Task.Yield();

                OnWrite?.Invoke();

                if (BlockWrite)
                    await Task.Delay(Timeout.Infinite, cancellationToken).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                if (FailWrite)
                    throw new IOException("Write failure.");

                await _storage.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            }

            public override Task FlushAsync(CancellationToken cancellationToken)
            {
                Flushes++;
                cancellationToken.ThrowIfCancellationRequested();

                return Task.CompletedTask;
            }

            protected override void Dispose(bool disposing)
            {
                Disposed = true;

                if (disposing)
                    _storage.Dispose();

                base.Dispose(disposing);
            }
        }

        #endregion

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
        public async Task Csv_StreamOverloads_OnlyUseAsyncIo([Values] CsvOverloadFamily family)
        {
            var track = CreateFile().GetTrackChunks().First();
            var file = new MidiFile(track);
            using var expected = new MemoryStream();
            using var output = new AsyncOnlyStream();

            switch (family)
            {
                case CsvOverloadFamily.File:
                    file.SerializeToCsv(expected);
                    await file.SerializeToCsvAsync(output);
                    break;
                case CsvOverloadFamily.Chunks:
                    file.Chunks.SerializeToCsv(expected, TempoMap.Default);
                    await file.Chunks.SerializeToCsvAsync(output, TempoMap.Default);
                    break;
                case CsvOverloadFamily.Chunk:
                    track.SerializeToCsv(expected, TempoMap.Default);
                    await track.SerializeToCsvAsync(output, TempoMap.Default);
                    break;
                case CsvOverloadFamily.Objects:
                    track.GetObjects(ObjectType.Note | ObjectType.TimedEvent).SerializeToCsv(expected, TempoMap.Default);
                    await track.GetObjects(ObjectType.Note | ObjectType.TimedEvent).SerializeToCsvAsync(output, TempoMap.Default);
                    break;
            }

            ClassicAssert.AreEqual(expected.ToArray(), output.ToArray(), "Async CSV output bytes differ.");
            ClassicAssert.Greater(output.Writes, 0, "CSV serialization did not use async writes.");
            ClassicAssert.AreEqual(1, output.Flushes, "CSV serialization did not flush exactly once.");

            using var input = new AsyncOnlyStream(output.ToArray());

            switch (family)
            {
                case CsvOverloadFamily.File:
                    var restoredFile = await CsvSerializer.DeserializeFileFromCsvAsync(input);
                    ClassicAssert.AreEqual(file.GetNotes().Count, restoredFile.GetNotes().Count, "CSV file round-trip note count differs.");
                    break;
                case CsvOverloadFamily.Chunks:
                    var chunks = await CsvSerializer.DeserializeChunksFromCsvAsync(input, TempoMap.Default);
                    ClassicAssert.AreEqual(file.Chunks.Count, chunks.Count, "CSV chunks round-trip count differs.");
                    break;
                case CsvOverloadFamily.Chunk:
                    ClassicAssert.IsInstanceOf<TrackChunk>(await CsvSerializer.DeserializeChunkFromCsvAsync(input, TempoMap.Default), "CSV chunk round-trip returned an invalid type.");
                    break;
                case CsvOverloadFamily.Objects:
                    var objects = await CsvSerializer.DeserializeObjectsFromCsvAsync(input, TempoMap.Default);
                    ClassicAssert.AreEqual(track.GetNotes().Count, objects.OfType<Note>().Count(), "CSV objects round-trip note count differs.");
                    break;
            }

            ClassicAssert.Greater(input.Reads, 1, "CSV deserialization did not use incremental async reads.");
            ClassicAssert.IsFalse(input.Disposed, "CSV deserialization disposed the caller-owned stream.");
            ClassicAssert.IsFalse(output.Disposed, "CSV serialization disposed the caller-owned stream.");
        }

        [Test]
        public async Task Csv_PathOverloads_RoundTrip([Values] CsvOverloadFamily family)
        {
            var path = Path.Combine(TestContext.CurrentContext.WorkDirectory, "tools-async-" + Guid.NewGuid() + ".csv");
            var file = CreateFile();
            var track = file.GetTrackChunks().First();

            try
            {
                switch (family)
                {
                    case CsvOverloadFamily.File:
                        await file.SerializeToCsvAsync(path, false);
                        ClassicAssert.AreEqual(file.GetNotes().Count, (await CsvSerializer.DeserializeFileFromCsvAsync(path)).GetNotes().Count, "CSV file path round-trip note count differs.");
                        break;
                    case CsvOverloadFamily.Chunks:
                        await file.Chunks.SerializeToCsvAsync(path, false, TempoMap.Default);
                        ClassicAssert.AreEqual(file.Chunks.Count, (await CsvSerializer.DeserializeChunksFromCsvAsync(path, TempoMap.Default)).Count, "CSV chunks path round-trip count differs.");
                        break;
                    case CsvOverloadFamily.Chunk:
                        await track.SerializeToCsvAsync(path, false, TempoMap.Default);
                        ClassicAssert.IsInstanceOf<TrackChunk>(await CsvSerializer.DeserializeChunkFromCsvAsync(path, TempoMap.Default), "CSV chunk path round-trip returned an invalid type.");
                        break;
                    case CsvOverloadFamily.Objects:
                        await file.GetNotes().SerializeToCsvAsync(path, false, TempoMap.Default);
                        ClassicAssert.AreEqual(file.GetNotes().Count, (await CsvSerializer.DeserializeObjectsFromCsvAsync(path, TempoMap.Default)).Count, "CSV objects path round-trip count differs.");
                        break;
                }

                Assert.ThrowsAsync<IOException>(() => file.SerializeToCsvAsync(path, false), "CSV serialization overwrote a file without permission.");

                await file.SerializeToCsvAsync(path, true);
            }
            finally
            {
                File.Delete(path);
            }
        }

        [Test]
        public async Task Csv_LargeInput_IsIncremental()
        {
            var file = new MidiFile(new TrackChunk(Enumerable.Range(0, 3000).Select(i => new TextEvent("line\n\"quoted\",é" + i) { DeltaTime = i % 2 })));
            using var output = new AsyncOnlyStream();

            await file.SerializeToCsvAsync(output);

            ClassicAssert.Greater(output.Writes, 1, "Large CSV serialization did not write incrementally.");

            using var input = new AsyncOnlyStream(output.ToArray());
            var restored = await CsvSerializer.DeserializeFileFromCsvAsync(input);

            ClassicAssert.AreEqual(3000, restored.GetTrackChunks().First().Events.Count, "Large CSV round-trip event count differs.");
        }

        [Test]
        public async Task Csv_MultiRecordObjects_AndFormatting_MatchSync()
        {
            var objects = new ITimedObject[]
            {
                new Chord(new[]
                {
                    new Note((SevenBitNumber)60, 100, 10),
                    new Note((SevenBitNumber)64, 80, 10)
                }),
                new PitchBendSensitivityParameter { Time = 30 },
                new TimedEvent(new NormalSysExEvent(new byte[] { 0, 1, 127 }), 40),
                new TimedEvent(new TextEvent("unicode é 🎼\n\"quoted\";value"), 50)
            };
            var settings = new CsvSerializationSettings
            {
                Delimiter = ';',
                BytesArrayFormat = CsvBytesArrayFormat.Hexadecimal,
                NoteFormat = CsvNoteFormat.Letter,
                TimeType = TimeSpanType.Metric,
                LengthType = TimeSpanType.Metric
            };
            using var expected = new MemoryStream();
            using var actual = new AsyncOnlyStream();

            objects.SerializeToCsv(expected, TempoMap.Default, settings);
            await objects.SerializeToCsvAsync(actual, TempoMap.Default, settings);

            ClassicAssert.AreEqual(expected.ToArray(), actual.ToArray(), "Async CSV multi-record formatting differs.");

            using var input = new AsyncOnlyStream(actual.ToArray());
            var readSettings = new CsvDeserializationSettings
            {
                Delimiter = ';',
                BytesArrayFormat = CsvBytesArrayFormat.Hexadecimal,
                NoteFormat = CsvNoteFormat.Letter,
                TimeType = TimeSpanType.Metric,
                LengthType = TimeSpanType.Metric
            };
            expected.Position = 0;

            var expectedObjects = CsvSerializer.DeserializeObjectsFromCsv(expected, TempoMap.Default, readSettings);
            var actualObjects = await CsvSerializer.DeserializeObjectsFromCsvAsync(input, TempoMap.Default, readSettings);

            ClassicAssert.AreEqual(expectedObjects.Select(obj => (obj.GetType(), obj.Time)), actualObjects.Select(obj => (obj.GetType(), obj.Time)), "Async CSV multi-record object types or times differ.");
        }

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

                    var asyncSource = midiFile.Clone();
                    asyncSource.SerializeToCsvAsync(
                        outputFilePath,
                        true,
                        null,
                        cancellationToken: System.Threading.CancellationToken.None).Wait();
                    var asyncConvertedFile = CsvSerializer.DeserializeFileFromCsvAsync(
                        outputFilePath,
                        null,
                        cancellationToken: System.Threading.CancellationToken.None).Result;
                    MidiAsserts.AreEqual(asyncSource, asyncConvertedFile, false, $"Asynchronous conversion of '{filePath}' is invalid.");
                }
            }
            finally
            {
                Directory.Delete(outputDirectory, true);
            }
        }

        #endregion

        #region Private methods

        private static void CheckImmediatelyCompletedCsvIo(bool writing)
        {
            using var expected = new MemoryStream();
            CreateFile().SerializeToCsv(expected);

            Exception failure = null;
            var callingThreadId = 0;
            var firstIoThreadId = 0;
            var ioStayedOnCallingThread = true;
            var thread = new Thread(() =>
            {
                try
                {
                    callingThreadId = Thread.CurrentThread.ManagedThreadId;

                    void ObserveIo()
                    {
                        var currentThreadId = Thread.CurrentThread.ManagedThreadId;

                        if (firstIoThreadId == 0)
                            firstIoThreadId = currentThreadId;

                        ioStayedOnCallingThread &= currentThreadId == callingThreadId;
                    }

                    using var stream = new AsyncOnlyStream(writing ? null : expected.ToArray())
                    {
                        CompleteSynchronously = true,
                        OnRead = ObserveIo,
                        OnWrite = ObserveIo
                    };

                    if (writing)
                    {
                        CreateFile().SerializeToCsvAsync(stream).GetAwaiter().GetResult();

                        ClassicAssert.AreEqual(expected.ToArray(), stream.ToArray(), "Inline CSV export differs from synchronous output.");
                    }
                    else
                    {
                        var restored = CsvSerializer.DeserializeFileFromCsvAsync(stream).GetAwaiter().GetResult();

                        MidiAsserts.AreEqual(CreateFile(), restored, false, "Inline CSV import differs from the original MIDI file.");
                    }
                }
                catch (Exception exception)
                {
                    failure = exception;
                }
            })
            {
                IsBackground = true
            };

            thread.Start();

            ClassicAssert.IsTrue(thread.Join(TimeSpan.FromSeconds(10)), "Immediately completed CSV I/O did not finish.");
            ClassicAssert.IsNull(failure, $"Immediately completed CSV I/O failed: {failure}");
            ClassicAssert.AreEqual(callingThreadId, firstIoThreadId, "CSV moved to a worker before its first stream operation.");
            ClassicAssert.IsTrue(ioStayedOnCallingThread, "CSV scheduled a worker despite synchronously completed stream operations.");
        }

        private static void CheckCsvIoErrors(bool writing)
        {
            using var stream = new AsyncOnlyStream { FailRead = !writing, FailWrite = writing };

            Assert.ThrowsAsync<IOException>(async () =>
            {
                if (writing)
                    await CreateFile().SerializeToCsvAsync(stream).WaitAsync(TimeSpan.FromSeconds(10));
                else
                    await CsvSerializer.DeserializeFileFromCsvAsync(stream).WaitAsync(TimeSpan.FromSeconds(10));
            }, "CSV stream failure did not propagate without deadlock.");
        }

        private static void CheckCsvCancellation(bool writing)
        {
            using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
            using var stream = new AsyncOnlyStream { BlockRead = !writing, BlockWrite = writing };

            Assert.CatchAsync<OperationCanceledException>(async () =>
            {
                if (writing)
                    await CreateFile().SerializeToCsvAsync(stream, cancellationToken: cancellation.Token).WaitAsync(TimeSpan.FromSeconds(10));
                else
                    await CsvSerializer.DeserializeFileFromCsvAsync(stream, cancellationToken: cancellation.Token).WaitAsync(TimeSpan.FromSeconds(10));
            }, "CSV cancellation did not interrupt pending I/O.");
        }

        private static MidiFile CreateFile() => new MidiFile(new TrackChunk(
            new SequenceTrackNameEvent("quoted \"track\"\nsecond line"),
            new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)100),
            new NoteOffEvent((SevenBitNumber)60, (SevenBitNumber)64) { DeltaTime = 100 },
            new NoteOnEvent((SevenBitNumber)62, (SevenBitNumber)90) { DeltaTime = 20 },
            new NoteOffEvent((SevenBitNumber)62, (SevenBitNumber)64) { DeltaTime = 100 }));

        #endregion
    }
}
