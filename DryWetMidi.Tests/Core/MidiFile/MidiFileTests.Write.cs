using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed partial class MidiFileTests
    {
        #region Test methods

        [Test]
        public async Task WritePrimitives_AsyncParity([Values] bool buffering)
        {
            using var synchronousOutput = new MemoryStream();
            using (var writer = new MidiWriter(synchronousOutput, new WriterSettings { UseBuffering = buffering, BufferSize = 3 }))
            {
                WritePrimitivePayload(writer);
            }

            using var output = new MidiFileAsyncTests.AsyncOnlyStream(seekable: false);
            var asynchronous = new MidiWriter(output, new WriterSettings { UseBuffering = buffering, BufferSize = 3 });

            await asynchronous.WriteByteAsync(0xAB, CancellationToken.None);
            await asynchronous.WriteSByteAsync(-12, CancellationToken.None);
            await asynchronous.WriteWordAsync(0x1234, CancellationToken.None);
            await asynchronous.WriteDwordAsync(0x89ABCDEF, CancellationToken.None);
            await asynchronous.WriteInt16Async(-12345, CancellationToken.None);
            await asynchronous.Write3ByteDwordAsync(0x123456, CancellationToken.None);
            await asynchronous.WriteStringAsync("A\u00E9Z!", CancellationToken.None);
            await asynchronous.WriteBytesAsync(new byte[] { 99, 1, 2, 3, 99 }, 1, 3, CancellationToken.None);
            await asynchronous.WriteVlqNumberAsync(int.MaxValue, CancellationToken.None);
            await asynchronous.WriteVlqNumberAsync(long.MaxValue, CancellationToken.None);
            await asynchronous.WriteBytesAsync(new byte[] { 42 }, CancellationToken.None);
            await asynchronous.WriteBytesAsync(Array.Empty<byte>(), CancellationToken.None);
            await asynchronous.WriteStringAsync(null, CancellationToken.None);
            await asynchronous.WriteStringAsync(string.Empty, CancellationToken.None);
            await asynchronous.DisposeAsync();

            ClassicAssert.AreEqual(synchronousOutput.ToArray(), output.ToArray(), "Async primitive bytes differ from synchronous output.");
            ClassicAssert.AreEqual(synchronousOutput.Length, asynchronous.Length, "Async writer length differs from the serialized length.");
            ClassicAssert.Greater(output.AsyncWrites, 0, "Async primitives did not use async transport.");
            ClassicAssert.IsFalse(output.IsDisposed, "Async primitive writer disposal closed the caller-owned stream.");
        }

        [Test]
        public async Task WritePrimitives_AsyncArgumentValidation()
        {
            using var output = new MidiFileAsyncTests.AsyncOnlyStream();
            await using var writer = new MidiWriter(output, new WriterSettings { UseBuffering = false });

            var nullException = Assert.ThrowsAsync<ArgumentNullException>(
                async () => await writer.WriteBytesAsync(null),
                "Async byte writing accepted null bytes.");
            var offsetException = Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await writer.WriteBytesAsync(new byte[2], -1, 1),
                "Async byte writing accepted a negative offset.");
            var lengthException = Assert.ThrowsAsync<ArgumentOutOfRangeException>(
                async () => await writer.WriteBytesAsync(new byte[2], 1, 2),
                "Async byte writing accepted an out-of-range length.");

            ClassicAssert.AreEqual("bytes", nullException.ParamName, "Null-byte validation reported an invalid parameter name.");
            ClassicAssert.AreEqual("offset", offsetException.ParamName, "Offset validation reported an invalid parameter name.");
            ClassicAssert.AreEqual("length", lengthException.ParamName, "Length validation reported an invalid parameter name.");
            ClassicAssert.AreEqual(0, output.AsyncWrites, "Invalid primitive writes touched the transport.");
            ClassicAssert.AreEqual(0, writer.Length, "Invalid primitive writes changed the writer length.");
        }

        [Test]
        public async Task WritePrimitives_AsyncCancellationAndDisposal()
        {
            using var output = new MidiFileAsyncTests.AsyncOnlyStream();
            var writer = new MidiWriter(output, new WriterSettings { UseBuffering = false });
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            Assert.CatchAsync<OperationCanceledException>(
                async () => await writer.WriteByteAsync(42, cancellation.Token),
                "A pre-canceled primitive byte write did not fail.");
            Assert.CatchAsync<OperationCanceledException>(
                async () => await writer.WriteWordAsync(42, cancellation.Token),
                "A pre-canceled primitive number write did not fail.");
            Assert.CatchAsync<OperationCanceledException>(
                async () => await writer.WriteVlqNumberAsync(42, cancellation.Token),
                "A pre-canceled primitive VLQ write did not fail.");
            ClassicAssert.AreEqual(0, output.AsyncWrites, "Pre-canceled primitives touched the transport.");
            ClassicAssert.AreEqual(0, writer.Length, "Pre-canceled primitives changed the writer length.");

            await writer.DisposeAsync();

            Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await writer.WriteByteAsync(42),
                "Disposed primitive byte writing did not fail.");
            Assert.ThrowsAsync<ObjectDisposedException>(
                async () => await writer.WriteStringAsync("test"),
                "Disposed primitive string writing did not fail.");
            ClassicAssert.IsFalse(output.IsDisposed, "Writer disposal closed the caller-owned stream.");
        }

        [Test]
        public void Write_SmpteTimeDivision()
        {
            MidiFileTestUtilities.Write(
                midiFile: new MidiFile { TimeDivision = new SmpteTimeDivision(SmpteFormat.Thirty, 80) },
                action: filePath =>
                {
                    var bytes = FileOperations.ReadAllFileBytes(filePath);
                    var timeDivisionBytes = new byte[2];
                    Buffer.BlockCopy(bytes, 12, timeDivisionBytes, 0, 2);
                    CollectionAssert.AreEqual(
                        new byte[] { 0xE2, 0x50 },
                        timeDivisionBytes,
                        "Invalid time division bytes.");
                });
        }

        [Test]
        public void Write_Compression_NoCompression()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50)));

            Write(
                midiFile,
                settings => { },
                (fileInfo1, fileInfo2) => ClassicAssert.AreEqual(fileInfo1.Length, fileInfo2.Length, "File size is invalid."));
        }

        [Test]
        public void Write_NoteOffAsSilentNoteOn()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50)));

            Write(
                midiFile,
                settings => settings.NoteOffAsSilentNoteOn = true,
                (fileInfo1, fileInfo2) =>
                {
                    var newMidiFile = MidiFile.Read(fileInfo2.FullName, new ReadingSettings { SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn });
                    CollectionAssert.IsEmpty(newMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<NoteOffEvent>(), "There are Note Off events.");
                });
        }

        [Test]
        public void Write_UseRunningStatus()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)51),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)51)));

            Write(
                midiFile,
                settings => settings.UseRunningStatus = true,
                (fileInfo1, fileInfo2) => ClassicAssert.Less(fileInfo2.Length, fileInfo1.Length, "File size is invalid."));
        }

        [Test]
        public void Write_DeleteUnknownMetaEvents()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new UnknownMetaEvent(254)));

            Write(
                midiFile,
                settings => settings.DeleteUnknownMetaEvents = true,
                (fileInfo1, fileInfo2) =>
                {
                    var originalMidiFile = MidiFile.Read(fileInfo1.FullName);
                    CollectionAssert.IsNotEmpty(
                        originalMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<UnknownMetaEvent>(),
                        "There are no Unknown Meta events in original file.");

                    var newMidiFile = MidiFile.Read(fileInfo2.FullName);
                    CollectionAssert.IsEmpty(
                        newMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<UnknownMetaEvent>(),
                        "There are Unknown Meta events in new file.");
                });
        }

        [Test]
        public void Write_DeleteDefaultKeySignature()
        {
            var nonDefaultKeySignatureEvent = new KeySignatureEvent(-5, 1);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new UnknownMetaEvent(254),
                    new KeySignatureEvent(),
                    nonDefaultKeySignatureEvent));

            Write(
                midiFile,
                settings => settings.DeleteDefaultKeySignature = true,
                (fileInfo1, fileInfo2) =>
                {
                    var originalMidiFile = MidiFile.Read(fileInfo1.FullName);
                    ClassicAssert.AreEqual(
                        2,
                        originalMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<KeySignatureEvent>().Count(),
                        "Invalid count of Key Signature events in original file.");

                    var newMidiFile = MidiFile.Read(fileInfo2.FullName);
                    var keySignatureEvents = newMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<KeySignatureEvent>().ToArray();
                    ClassicAssert.AreEqual(
                        1,
                        keySignatureEvents.Length,
                        "Invalid count of Key Signature events in new file.");

                    MidiAsserts.AreEqual(keySignatureEvents[0], nonDefaultKeySignatureEvent, false, "Invalid Key Signature event.");
                });
        }

        [Test]
        public void Write_DeleteDefaultSetTempo()
        {
            var nonDefaultSetTempoEvent = new SetTempoEvent(100000);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new UnknownMetaEvent(254),
                    new SetTempoEvent(),
                    nonDefaultSetTempoEvent));

            Write(
                midiFile,
                settings => settings.DeleteDefaultSetTempo = true,
                (fileInfo1, fileInfo2) =>
                {
                    var originalMidiFile = MidiFile.Read(fileInfo1.FullName);
                    ClassicAssert.AreEqual(
                        2,
                        originalMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<SetTempoEvent>().Count(),
                        "Invalid count of Set Tempo events in original file.");

                    var newMidiFile = MidiFile.Read(fileInfo2.FullName);
                    var setTempoEvents = newMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<SetTempoEvent>().ToArray();
                    ClassicAssert.AreEqual(
                        1,
                        setTempoEvents.Length,
                        "Invalid count of Set Tempo events in new file.");

                    MidiAsserts.AreEqual(setTempoEvents[0], nonDefaultSetTempoEvent, false, "Invalid Set Tempo event.");
                });
        }

        [Test]
        public void Write_DeleteDefaultKeySignature_DeleteDefaultSetTempo()
        {
            var nonDefaultKeySignatureEvent = new KeySignatureEvent(-5, 1);
            var nonDefaultSetTempoEvent = new SetTempoEvent(100000);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new UnknownMetaEvent(254),
                    new KeySignatureEvent(),
                    nonDefaultKeySignatureEvent,
                    new SetTempoEvent(),
                    nonDefaultSetTempoEvent));

            Write(
                midiFile,
                settings =>
                {
                    settings.DeleteDefaultKeySignature = true;
                    settings.DeleteDefaultSetTempo = true;
                },
                (fileInfo1, fileInfo2) =>
                {
                    var originalMidiFile = MidiFile.Read(fileInfo1.FullName);
                    var newMidiFile = MidiFile.Read(fileInfo2.FullName);

                    //

                    ClassicAssert.AreEqual(
                        2,
                        originalMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<KeySignatureEvent>().Count(),
                        "Invalid count of Key Signature events in original file.");

                    var keySignatureEvents = newMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<KeySignatureEvent>().ToArray();
                    ClassicAssert.AreEqual(
                        1,
                        keySignatureEvents.Length,
                        "Invalid count of Key Signature events in new file.");

                    MidiAsserts.AreEqual(keySignatureEvents[0], nonDefaultKeySignatureEvent, false, "Invalid Key Signature event.");

                    //

                    ClassicAssert.AreEqual(
                        2,
                        originalMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<SetTempoEvent>().Count(),
                        "Invalid count of Set Tempo events in original file.");

                    var setTempoEvents = newMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<SetTempoEvent>().ToArray();
                    ClassicAssert.AreEqual(
                        1,
                        setTempoEvents.Length,
                        "Invalid count of Set Tempo events in new file.");

                    MidiAsserts.AreEqual(setTempoEvents[0], nonDefaultSetTempoEvent, false, "Invalid Set Tempo event.");
                });
        }

        [Test]
        public void Write_DeleteDefaultTimeSignature()
        {
            var nonDefaultTimeSignatureEvent = new TimeSignatureEvent(2, 16);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new UnknownMetaEvent(254),
                    new TimeSignatureEvent(),
                    nonDefaultTimeSignatureEvent));

            Write(
                midiFile,
                settings => settings.DeleteDefaultTimeSignature = true,
                (fileInfo1, fileInfo2) =>
                {
                    var originalMidiFile = MidiFile.Read(fileInfo1.FullName);
                    ClassicAssert.AreEqual(
                        2,
                        originalMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<TimeSignatureEvent>().Count(),
                        "Invalid count of Time Signature events in original file.");

                    var newMidiFile = MidiFile.Read(fileInfo2.FullName);
                    var timeSignatureEvents = newMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<TimeSignatureEvent>().ToArray();
                    ClassicAssert.AreEqual(
                        1,
                        timeSignatureEvents.Length,
                        "Invalid count of Time Signature events in new file.");

                    MidiAsserts.AreEqual(timeSignatureEvents[0], nonDefaultTimeSignatureEvent, false, "Invalid Time Signature event.");
                });
        }

        [Test]
        public void Write_DeleteUnknownChunks()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50)),
                new UnknownChunk("abcd"));

            Write(
                midiFile,
                settings => settings.DeleteUnknownChunks = true,
                (fileInfo1, fileInfo2) =>
                {
                    var originalMidiFile = MidiFile.Read(fileInfo1.FullName);
                    CollectionAssert.IsNotEmpty(
                        originalMidiFile.Chunks.OfType<UnknownChunk>(),
                        "There are no Unknown chunks in original file.");

                    var newMidiFile = MidiFile.Read(fileInfo2.FullName);
                    CollectionAssert.IsEmpty(
                        newMidiFile.Chunks.OfType<UnknownChunk>(),
                        "There are Unknown chunks in new file.");
                });
        }

        [Test]
        public void Write_WriteHeaderChunk()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50)));

            Write(
                midiFile,
                settings => { },
                (fileInfo1, fileInfo2) =>
                {
                    var originalMidiFile = MidiFile.Read(fileInfo1.FullName);
                    var newMidiFile = MidiFile.Read(fileInfo2.FullName);
                    MidiAsserts.AreEqual(originalMidiFile, newMidiFile, true, "New file is invalid.");
                });
        }

        [Test]
        public void Write_DontWriteHeaderChunk()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50)));

            Write(
                midiFile,
                settings => settings.WriteHeaderChunk = false,
                (fileInfo1, fileInfo2) =>
                {
                    var originalMidiFile = MidiFile.Read(fileInfo1.FullName);
                    originalMidiFile.TimeDivision = null;

                    ClassicAssert.Throws<NoHeaderChunkException>(() => MidiFile.Read(fileInfo2.FullName));
                    var newMidiFile = MidiFile.Read(fileInfo2.FullName, new ReadingSettings
                    {
                        NoHeaderChunkPolicy = NoHeaderChunkPolicy.Ignore
                    });

                    MidiAsserts.AreEqual(originalMidiFile, newMidiFile, false, "New file is invalid.");
                });
        }

        [Test]
        public void Write_StreamIsNotDisposed()
        {
            var midiFile = new MidiFile();

            using (var streamToWrite = new MemoryStream())
            {
                midiFile.Write(streamToWrite);
                ClassicAssert.DoesNotThrow(() => { var l = streamToWrite.Length; }, "Synchronous writing disposed the caller-owned stream.");
            }

            using (var streamToWrite = new MemoryStream())
            {
                midiFile.Clone().WriteAsync(
                    streamToWrite,
                    cancellationToken: System.Threading.CancellationToken.None).Wait();

                ClassicAssert.DoesNotThrow(() => { var l = streamToWrite.Length; }, "Async writing disposed the caller-owned stream.");
            }
        }

        [Test]
        public void Write_DifferentDeltaTimes()
        {
            var originalMidiEvents = new MidiEvent[]
            {
                new TextEvent("A") { DeltaTime = 0 },
                new TextEvent("B") { DeltaTime = 10 },
                new TextEvent("C") { DeltaTime = 100 },
                new TextEvent("D") { DeltaTime = 1000 },
                new TextEvent("E") { DeltaTime = 10000 },
                new TextEvent("F") { DeltaTime = 100000 }
            };

            var midiFile = new MidiFile(new TrackChunk(originalMidiEvents));

            Write(
                midiFile,
                settings => { },
                (fileInfo1, fileInfo2) =>
                {
                    var newMidiFile = MidiFile.Read(fileInfo2.FullName);
                    MidiAsserts.AreEqual(midiFile, newMidiFile, false, "File is invalid.");
                });
        }

        [Test]
        public void Write_SingleTrack_EmptyCollection() => CheckWritingWithFormat_SingleTrack(
            chunks: Enumerable.Empty<MidiChunk>(),
            expectedChunks: Enumerable.Empty<MidiChunk>());

        [Test]
        public void Write_SingleTrack_SingleChunk_TrackChunk() => CheckWritingWithFormat_SingleTrack(
            chunks: new[] { new TrackChunk() },
            expectedChunks: new[] { new TrackChunk() });

        [Test]
        public void Write_SingleTrack_SingleChunk_UnknownChunk() => CheckWritingWithFormat_SingleTrack(
            chunks: new[] { new UnknownChunk("abcd") },
            expectedChunks: new[] { new UnknownChunk("abcd") },
            readingSettings: new ReadingSettings
            {
                ZeroLengthDataPolicy = ZeroLengthDataPolicy.ReadAsNull
            });

        [Test]
        public void Write_SingleTrack_SingleChunk_CustomChunk() => CheckWritingWithFormat_SingleTrack(
            chunks: new[] { new CustomChunk(1, "A", 2) },
            expectedChunks: new[] { new CustomChunk(1, "A", 2) },
            readingSettings: new ReadingSettings
            {
                CustomChunkTypes = new ChunkTypesCollection
                {
                    { typeof(CustomChunk), CustomChunk.Id }
                }
            });

        [Test]
        public void Write_SingleTrack_MultipleChunks_TrackChunks_FarEventInSecondChunk() => CheckWritingWithFormat_SingleTrack(
            chunks: new[]
            {
                new TrackChunk(new TextEvent("A")),
                new TrackChunk(new TextEvent("B") { DeltaTime = 100 })
            },
            expectedChunks: new[]
            {
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 100 })
            });

        [Test]
        public void Write_SingleTrack_MultipleChunks_TrackChunks_FarEventInFirstChunk() => CheckWritingWithFormat_SingleTrack(
            chunks: new[]
            {
                new TrackChunk(new TextEvent("A") { DeltaTime = 100 }),
                new TrackChunk(new TextEvent("B"))
            },
            expectedChunks: new[]
            {
                new TrackChunk(
                    new TextEvent("B"),
                    new TextEvent("A") { DeltaTime = 100 })
            });

        [Test]
        public void Write_SingleTrack_MultipleChunks_TrackChunks_MetaAndChannelEvents() => CheckWritingWithFormat_SingleTrack(
            chunks: new[]
            {
                new TrackChunk(new NoteOnEvent(), new TextEvent("A")),
                new TrackChunk(new TextEvent("B"), new NoteOffEvent() { DeltaTime = 100 })
            },
            expectedChunks: new[]
            {
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B"),
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 100 })
            },
            writingSettings: new WritingSettings
            {
                NoteOffAsSilentNoteOn = false
            },
            readingSettings: new ReadingSettings
            {
                SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn
            });

        [Test]
        public void Write_SingleTrack_MultipleChunks_TrackChunks_ChannelPrefix() => CheckWritingWithFormat_SingleTrack(
            chunks: new[]
            {
                new TrackChunk(new TextEvent("A"), new ChannelPrefixEvent()),
                new TrackChunk(new TextEvent("B"))
            },
            expectedChunks: new[]
            {
                new TrackChunk(
                    new TextEvent("A"),
                    new ChannelPrefixEvent(),
                    new TextEvent("B"))
            });

        [Test]
        public void Write_SingleTrack_MultipleChunks_TrackChunksAndNonTrackChunks() => CheckWritingWithFormat_SingleTrack(
            chunks: new MidiChunk[]
            {
                new TrackChunk(new TextEvent("A"), new ChannelPrefixEvent(), new NoteOffEvent() { DeltaTime = 50 }),
                new UnknownChunk("abcd"),
                new TrackChunk(new NoteOnEvent(), new TextEvent("B")),
                new CustomChunk(1, "A", 2),
                new TrackChunk(new NoteOnEvent() { DeltaTime = 25 })
            },
            expectedChunks: new MidiChunk[]
            {
                new TrackChunk(
                    new TextEvent("A"),
                    new ChannelPrefixEvent(),
                    new TextEvent("B"),
                    new NoteOnEvent(),
                    new NoteOnEvent() { DeltaTime = 25 },
                    new NoteOffEvent() { DeltaTime = 25 }),
                new UnknownChunk("abcd"),
                new CustomChunk(1, "A", 2)
            },
            writingSettings: new WritingSettings
            {
                NoteOffAsSilentNoteOn = false
            },
            readingSettings: new ReadingSettings
            {
                SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn,
                ZeroLengthDataPolicy = ZeroLengthDataPolicy.ReadAsNull,
                CustomChunkTypes = new ChunkTypesCollection
                {
                    { typeof(CustomChunk), CustomChunk.Id }
                }
            });

        [Test]
        public void Write_MultiTrack_EmptyCollection() => CheckWritingWithFormat_MultiTrack(
            chunks: Enumerable.Empty<MidiChunk>(),
            expectedChunks: Enumerable.Empty<MidiChunk>());

        [Test]
        public void Write_MultiTrack_SingleChunk_TrackChunk() => CheckWritingWithFormat_MultiTrack(
            chunks: new[] { new TrackChunk() },
            expectedChunks: new[] { new TrackChunk() });

        [Test]
        public void Write_MultiTrack_SingleChunk_TrackChunk_NonChannelOnly() => CheckWritingWithFormat_MultiTrack(
            chunks: new[]
            {
                new TrackChunk(
                    new TextEvent("A"),
                    new NormalSysExEvent(new byte[] { 1, 2, 3 }))
            },
            expectedChunks: new[]
            {
                new TrackChunk(
                    new TextEvent("A"),
                    new NormalSysExEvent(new byte[] { 1, 2, 3 }))
            });

        [Test]
        public void Write_MultiTrack_SingleChunk_TrackChunk_ChannelOnly_SingleChannel() => CheckWritingWithFormat_MultiTrack(
            chunks: new[]
            {
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent())
            },
            expectedChunks: new[]
            {
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent())
            },
            writingSettings: new WritingSettings
            {
                NoteOffAsSilentNoteOn = false
            },
            readingSettings: new ReadingSettings
            {
                SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn
            });

        [Test]
        public void Write_MultiTrack_SingleChunk_TrackChunk_ChannelOnly_MultipleChannels() => CheckWritingWithFormat_MultiTrack(
            chunks: new[]
            {
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOnEvent { DeltaTime = 100, Channel = (FourBitNumber)1 },
                    new NoteOffEvent(),
                    new NoteOffEvent { Channel = (FourBitNumber)1 })
            },
            expectedChunks: new[]
            {
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 100, Channel = (FourBitNumber)1 },
                    new NoteOffEvent { Channel = (FourBitNumber)1 })
            },
            writingSettings: new WritingSettings
            {
                NoteOffAsSilentNoteOn = false
            },
            readingSettings: new ReadingSettings
            {
                SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn
            });

        [Test]
        public void Write_MultiTrack_SingleChunk_TrackChunk_ChannelOnly_AllChannels() => CheckWritingWithFormat_MultiTrack(
            chunks: new[]
            {
                new TrackChunk(
                    FourBitNumber.Values.SelectMany(channel => new MidiEvent[]
                    {
                        new NoteOnEvent { DeltaTime = 10, Channel = channel },
                        new NoteOffEvent { DeltaTime = 10, Channel = channel }
                    }))
            },
            expectedChunks: FourBitNumber.Values.Select(channel => new TrackChunk(
                new NoteOnEvent { DeltaTime = (channel * 20) + 10, Channel = channel },
                new NoteOffEvent { DeltaTime = 10, Channel = channel })),
            writingSettings: new WritingSettings
            {
                NoteOffAsSilentNoteOn = false
            },
            readingSettings: new ReadingSettings
            {
                SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn
            });

        [Test]
        public void Write_MultiTrack_SingleChunk_TrackChunk_Mixed() => CheckWritingWithFormat_MultiTrack(
            chunks: new[]
            {
                new TrackChunk(
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOnEvent { DeltaTime = 100, Channel = (FourBitNumber)1 },
                    new NoteOffEvent(),
                    new NoteOffEvent { Channel = (FourBitNumber)1 },
                    new TextEvent("B"),
                    new NormalSysExEvent(new byte[] { 1, 2, 3 }))
            },
            expectedChunks: new[]
            {
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 100 },
                    new NormalSysExEvent(new byte[] { 1, 2, 3 })),
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 }),
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 100, Channel = (FourBitNumber)1 },
                    new NoteOffEvent { Channel = (FourBitNumber)1 })
            },
            writingSettings: new WritingSettings
            {
                NoteOffAsSilentNoteOn = false
            },
            readingSettings: new ReadingSettings
            {
                SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn
            });

        [Test]
        public void Write_MultiTrack_SingleChunk_TrackChunk_ChannelPrefix() => CheckWritingWithFormat_MultiTrack(
            chunks: new[]
            {
                new TrackChunk(
                    new ChannelPrefixEvent(1),
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOnEvent { DeltaTime = 100, Channel = (FourBitNumber)1 },
                    new NoteOffEvent(),
                    new NoteOffEvent { Channel = (FourBitNumber)1 },
                    new TextEvent("B"),
                    new NormalSysExEvent(new byte[] { 1, 2, 3 }))
            },
            expectedChunks: new[]
            {
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 100 },
                    new NormalSysExEvent(new byte[] { 1, 2, 3 })),
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 }),
                new TrackChunk(
                    new ChannelPrefixEvent(1),
                    new TextEvent("A"),
                    new NoteOnEvent { DeltaTime = 100, Channel = (FourBitNumber)1 },
                    new NoteOffEvent { Channel = (FourBitNumber)1 })
            },
            writingSettings: new WritingSettings
            {
                NoteOffAsSilentNoteOn = false
            },
            readingSettings: new ReadingSettings
            {
                SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn
            });

        [Test]
        public void Write_MultiTrack_SingleChunk_UnknownChunk() => CheckWritingWithFormat_MultiTrack(
            chunks: new[] { new UnknownChunk("abcd") },
            expectedChunks: new[] { new UnknownChunk("abcd") },
            readingSettings: new ReadingSettings
            {
                ZeroLengthDataPolicy = ZeroLengthDataPolicy.ReadAsNull
            });

        [Test]
        public void Write_MultiTrack_MultipleChunks_EmptyTrackChunks() => CheckWritingWithFormat_MultiTrack(
            chunks: new[]
            {
                new TrackChunk(),
                new TrackChunk()
            },
            expectedChunks: new[]
            {
                new TrackChunk(),
                new TrackChunk()
            });

        [Test]
        public void Write_MultiTrack_MultipleChunks_Mixed() => CheckWritingWithFormat_MultiTrack(
            chunks: new MidiChunk[]
            {
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new TextEvent("A")),
                new UnknownChunk("abcd"),
                new TrackChunk(
                    new TextEvent("A"))
            },
            expectedChunks: new MidiChunk[]
            {
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new TextEvent("A")),
                new UnknownChunk("abcd"),
                new TrackChunk(
                    new TextEvent("A"))
            },
            writingSettings: new WritingSettings
            {
                NoteOffAsSilentNoteOn = false
            },
            readingSettings: new ReadingSettings
            {
                SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn,
                ZeroLengthDataPolicy = ZeroLengthDataPolicy.ReadAsNull
            });

        [Test]
        public void Write_MultiSequence_EmptyCollection() => CheckWritingWithFormat_MultiSequence(
            chunks: Enumerable.Empty<MidiChunk>(),
            expectedChunks: Enumerable.Empty<MidiChunk>());

        [Test]
        public void Write_MultiSequence_SingleChunk_TrackChunk() => CheckWritingWithFormat_MultiSequence(
            chunks: new[] { new TrackChunk() },
            expectedChunks: new[] { new TrackChunk() });

        [Test]
        public void Write_MultiSequence_SingleChunk_TrackChunk_NoSequenceNumber() => CheckWritingWithFormat_MultiSequence(
            chunks: new[]
            {
                new TrackChunk(new TextEvent("A"))
            },
            expectedChunks: new[]
            {
                new TrackChunk(new TextEvent("A"))
            });

        [Test]
        public void Write_MultiSequence_SingleChunk_UnknownChunk() => CheckWritingWithFormat_MultiSequence(
            chunks: new[] { new UnknownChunk("abcd") },
            expectedChunks: new[] { new UnknownChunk("abcd") },
            readingSettings: new ReadingSettings
            {
                ZeroLengthDataPolicy = ZeroLengthDataPolicy.ReadAsNull
            });

        [Test]
        public void Write_MultiSequence_SingleChunk_CustomChunk() => CheckWritingWithFormat_MultiSequence(
            chunks: new[] { new CustomChunk(1, "A", 2) },
            expectedChunks: new[] { new CustomChunk(1, "A", 2) },
            readingSettings: new ReadingSettings
            {
                CustomChunkTypes = new ChunkTypesCollection
                {
                    { typeof(CustomChunk), CustomChunk.Id }
                }
            });

        [Test]
        public void Write_MultiSequence_MultipleChunks_TrackChunks_FarEventInSecondChunk_NoSequenceNumbers() => CheckWritingWithFormat_MultiSequence(
            chunks: new[]
            {
                new TrackChunk(new TextEvent("A")),
                new TrackChunk(new TextEvent("B") { DeltaTime = 100 })
            },
            expectedChunks: new[]
            {
                new TrackChunk(new TextEvent("A")),
                new TrackChunk(new TextEvent("B") { DeltaTime = 100 })
            });

        [Test]
        public void Write_MultiSequence_MultipleChunks_TrackChunks_FarEventInFirstChunk_NoSequenceNumbers() => CheckWritingWithFormat_MultiSequence(
            chunks: new[]
            {
                new TrackChunk(new TextEvent("A") { DeltaTime = 100 }),
                new TrackChunk(new TextEvent("B"))
            },
            expectedChunks: new[]
            {
                new TrackChunk(new TextEvent("A") { DeltaTime = 100 }),
                new TrackChunk(new TextEvent("B"))
            });

        [Test]
        public void Write_MultiSequence_MultipleChunks_TrackChunks_MetaAndChannelEvents_NoSequenceNumbers() => CheckWritingWithFormat_MultiSequence(
            chunks: new[]
            {
                new TrackChunk(new NoteOnEvent(), new TextEvent("A")),
                new TrackChunk(new TextEvent("B"), new NoteOffEvent() { DeltaTime = 100 })
            },
            expectedChunks: new[]
            {
                new TrackChunk(new NoteOnEvent(), new TextEvent("A")),
                new TrackChunk(new TextEvent("B"), new NoteOffEvent() { DeltaTime = 100 })
            },
            writingSettings: new WritingSettings
            {
                NoteOffAsSilentNoteOn = false
            },
            readingSettings: new ReadingSettings
            {
                SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn
            });

        [Test]
        public void Write_MultiSequence_MultipleChunks_TrackChunks_ChannelPrefix_NoSequenceNumbers() => CheckWritingWithFormat_MultiSequence(
            chunks: new[]
            {
                new TrackChunk(new TextEvent("A"), new ChannelPrefixEvent()),
                new TrackChunk(new TextEvent("B"))
            },
            expectedChunks: new[]
            {
                new TrackChunk(new TextEvent("A"), new ChannelPrefixEvent()),
                new TrackChunk(new TextEvent("B"))
            });

        [Test]
        public void Write_MultiSequence_MultipleChunks_TrackChunksAndNonTrackChunks_NoSequenceNumbers() => CheckWritingWithFormat_MultiSequence(
            chunks: new MidiChunk[]
            {
                new TrackChunk(new TextEvent("A"), new ChannelPrefixEvent(), new NoteOffEvent() { DeltaTime = 50 }),
                new UnknownChunk("abcd"),
                new TrackChunk(new NoteOnEvent(), new TextEvent("B")),
                new CustomChunk(1, "A", 2),
                new TrackChunk(new NoteOnEvent() { DeltaTime = 25 })
            },
            expectedChunks: new MidiChunk[]
            {
                new TrackChunk(new TextEvent("A"), new ChannelPrefixEvent(), new NoteOffEvent() { DeltaTime = 50 }),
                new TrackChunk(new NoteOnEvent(), new TextEvent("B")),
                new TrackChunk(new NoteOnEvent() { DeltaTime = 25 }),
                new UnknownChunk("abcd"),
                new CustomChunk(1, "A", 2),
            },
            writingSettings: new WritingSettings
            {
                NoteOffAsSilentNoteOn = false
            },
            readingSettings: new ReadingSettings
            {
                SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn,
                ZeroLengthDataPolicy = ZeroLengthDataPolicy.ReadAsNull,
                CustomChunkTypes = new ChunkTypesCollection
                {
                    { typeof(CustomChunk), CustomChunk.Id }
                }
            });

        [Test]
        public void Write_MultiSequence_MultipleChunks_Mixed_WithSequenceNumbers() => CheckWritingWithFormat_MultiSequence(
            chunks: new MidiChunk[]
            {
                new TrackChunk(new SequenceNumberEvent(1), new TextEvent("A"), new ChannelPrefixEvent(), new NoteOffEvent() { DeltaTime = 50 }),
                new UnknownChunk("abcd"),
                new TrackChunk(new SequenceNumberEvent(1), new NoteOnEvent(), new TextEvent("B")),
                new CustomChunk(1, "A", 2),
                new TrackChunk(new NoteOnEvent() { DeltaTime = 25 })
            },
            expectedChunks: new MidiChunk[]
            {
                new TrackChunk(
                    new SequenceNumberEvent(1),
                    new TextEvent("A"),
                    new ChannelPrefixEvent(),
                    new SequenceNumberEvent(1),
                    new TextEvent("B"),
                    new NoteOnEvent(),
                    new NoteOffEvent() { DeltaTime = 50 }),
                new TrackChunk(new NoteOnEvent() { DeltaTime = 25 }),
                new UnknownChunk("abcd"),
                new CustomChunk(1, "A", 2),
            },
            writingSettings: new WritingSettings
            {
                NoteOffAsSilentNoteOn = false
            },
            readingSettings: new ReadingSettings
            {
                SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn,
                ZeroLengthDataPolicy = ZeroLengthDataPolicy.ReadAsNull,
                CustomChunkTypes = new ChunkTypesCollection
                {
                    { typeof(CustomChunk), CustomChunk.Id }
                }
            });

        #endregion

        #region Private methods

        private static void WritePrimitivePayload(MidiWriter writer)
        {
            writer.WriteByte(0xAB);
            writer.WriteSByte(-12);
            writer.WriteWord(0x1234);
            writer.WriteDword(0x89ABCDEF);
            writer.WriteInt16(-12345);
            writer.Write3ByteDword(0x123456);
            writer.WriteString("A\u00E9Z!");
            writer.WriteBytes(new byte[] { 99, 1, 2, 3, 99 }, 1, 3);
            writer.WriteVlqNumber(int.MaxValue);
            writer.WriteVlqNumber(long.MaxValue);
            writer.WriteBytes(new byte[] { 42 });
        }

        private void CheckWritingWithFormat_MultiSequence(
            IEnumerable<MidiChunk> chunks,
            IEnumerable<MidiChunk> expectedChunks,
            WritingSettings writingSettings = null,
            ReadingSettings readingSettings = null) => CheckWritingWithFormat(
            chunks,
            MidiFileFormat.MultiSequence,
            expectedChunks,
            writingSettings,
            readingSettings);

        private void CheckWritingWithFormat_MultiTrack(
            IEnumerable<MidiChunk> chunks,
            IEnumerable<MidiChunk> expectedChunks,
            WritingSettings writingSettings = null,
            ReadingSettings readingSettings = null) => CheckWritingWithFormat(
            chunks,
            MidiFileFormat.MultiTrack,
            expectedChunks,
            writingSettings,
            readingSettings);

        private void CheckWritingWithFormat_SingleTrack(
            IEnumerable<MidiChunk> chunks,
            IEnumerable<MidiChunk> expectedChunks,
            WritingSettings writingSettings = null,
            ReadingSettings readingSettings = null) => CheckWritingWithFormat(
            chunks,
            MidiFileFormat.SingleTrack,
            expectedChunks,
            writingSettings,
            readingSettings);

        private void CheckWritingWithFormat(
            IEnumerable<MidiChunk> chunks,
            MidiFileFormat format,
            IEnumerable<MidiChunk> expectedChunks,
            WritingSettings writingSettings,
            ReadingSettings readingSettings)
        {
            var midiFile = MidiFileTestUtilities.Read(
                new MidiFile(chunks),
                writingSettings,
                readingSettings,
                format);

            ClassicAssert.AreEqual(format, midiFile.OriginalFormat, "Invalid original format.");
            MidiAsserts.AreEqual(expectedChunks, midiFile.Chunks, true, "Chunks are invalid.");
        }

        private void Write(MidiFile midiFile, Action<WritingSettings> setupCompression, Action<FileInfo, FileInfo> fileInfosAction)
        {
            MidiFileTestUtilities.Write(
                midiFile,
                filePath =>
                {
                    var fileInfo = new FileInfo(filePath);

                    var writingSettings = new WritingSettings();
                    setupCompression(writingSettings);

                    MidiFileTestUtilities.Write(
                        midiFile,
                        filePath2 =>
                        {
                            var fileInfo2 = new FileInfo(filePath2);

                            fileInfosAction(fileInfo, fileInfo2);
                        },
                        writingSettings);
                },
                new WritingSettings());
        }

        #endregion
    }
}
