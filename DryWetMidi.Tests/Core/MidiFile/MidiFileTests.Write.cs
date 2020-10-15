using System;
using System.IO;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed partial class MidiFileTests
    {
        #region Test methods

        [Obsolete("OBS1")]
        [Test]
        public void Write_Compression_NoCompression_Obsolete()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50)));

            Write_Compression_Obsolete(
                midiFile,
                CompressionPolicy.NoCompression,
                (fileInfo1, fileInfo2) => Assert.AreEqual(fileInfo1.Length, fileInfo2.Length, "File size is invalid."));
        }

        [Obsolete("OBS1")]
        [Test]
        public void Write_Compression_NoteOffAsSilentNoteOn_Obsolete()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50)));

            Write_Compression_Obsolete(
                midiFile,
                CompressionPolicy.NoteOffAsSilentNoteOn,
                (fileInfo1, fileInfo2) =>
                {
                    var newMidiFile = MidiFile.Read(fileInfo2.FullName, new ReadingSettings { SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn });
                    CollectionAssert.IsEmpty(newMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<NoteOffEvent>(), "There are Note Off events.");
                });
        }

        [Obsolete("OBS1")]
        [Test]
        public void Write_Compression_UseRunningStatus_Obsolete()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)51),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)51)));

            Write_Compression_Obsolete(
                midiFile,
                CompressionPolicy.UseRunningStatus,
                (fileInfo1, fileInfo2) => Assert.Less(fileInfo2.Length, fileInfo1.Length, "File size is invalid."));
        }

        [Obsolete("OBS1")]
        [Test]
        public void Write_Compression_DeleteUnknownMetaEvents_Obsolete()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new UnknownMetaEvent(254)));

            Write_Compression_Obsolete(
                midiFile,
                CompressionPolicy.DeleteUnknownMetaEvents,
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

        [Obsolete("OBS1")]
        [Test]
        public void Write_Compression_DeleteDefaultKeySignature_Obsolete()
        {
            var nonDefaultKeySignatureEvent = new KeySignatureEvent(-5, 1);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new UnknownMetaEvent(254),
                    new KeySignatureEvent(),
                    nonDefaultKeySignatureEvent));

            Write_Compression_Obsolete(
                midiFile,
                CompressionPolicy.DeleteDefaultKeySignature,
                (fileInfo1, fileInfo2) =>
                {
                    var originalMidiFile = MidiFile.Read(fileInfo1.FullName);
                    Assert.AreEqual(
                        2,
                        originalMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<KeySignatureEvent>().Count(),
                        "Invalid count of Key Signature events in original file.");

                    var newMidiFile = MidiFile.Read(fileInfo2.FullName);
                    var keySignatureEvents = newMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<KeySignatureEvent>().ToArray();
                    Assert.AreEqual(
                        1,
                        keySignatureEvents.Length,
                        "Invalid count of Key Signature events in new file.");

                    MidiAsserts.AreEventsEqual(keySignatureEvents[0], nonDefaultKeySignatureEvent, false, "Invalid Key Signature event.");
                });
        }

        [Obsolete("OBS1")]
        [Test]
        public void Write_Compression_DeleteDefaultSetTempo_Obsolete()
        {
            var nonDefaultSetTempoEvent = new SetTempoEvent(100000);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new UnknownMetaEvent(254),
                    new SetTempoEvent(),
                    nonDefaultSetTempoEvent));

            Write_Compression_Obsolete(
                midiFile,
                CompressionPolicy.DeleteDefaultSetTempo,
                (fileInfo1, fileInfo2) =>
                {
                    var originalMidiFile = MidiFile.Read(fileInfo1.FullName);
                    Assert.AreEqual(
                        2,
                        originalMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<SetTempoEvent>().Count(),
                        "Invalid count of Set Tempo events in original file.");

                    var newMidiFile = MidiFile.Read(fileInfo2.FullName);
                    var setTempoEvents = newMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<SetTempoEvent>().ToArray();
                    Assert.AreEqual(
                        1,
                        setTempoEvents.Length,
                        "Invalid count of Set Tempo events in new file.");

                    MidiAsserts.AreEventsEqual(setTempoEvents[0], nonDefaultSetTempoEvent, false, "Invalid Set Tempo event.");
                });
        }

        [Obsolete("OBS1")]
        [Test]
        public void Write_Compression_DeleteDefaultTimeSignature_Obsolete()
        {
            var nonDefaultTimeSignatureEvent = new TimeSignatureEvent(2, 16);

            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new UnknownMetaEvent(254),
                    new TimeSignatureEvent(),
                    nonDefaultTimeSignatureEvent));

            Write_Compression_Obsolete(
                midiFile,
                CompressionPolicy.DeleteDefaultTimeSignature,
                (fileInfo1, fileInfo2) =>
                {
                    var originalMidiFile = MidiFile.Read(fileInfo1.FullName);
                    Assert.AreEqual(
                        2,
                        originalMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<TimeSignatureEvent>().Count(),
                        "Invalid count of Time Signature events in original file.");

                    var newMidiFile = MidiFile.Read(fileInfo2.FullName);
                    var timeSignatureEvents = newMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<TimeSignatureEvent>().ToArray();
                    Assert.AreEqual(
                        1,
                        timeSignatureEvents.Length,
                        "Invalid count of Time Signature events in new file.");

                    MidiAsserts.AreEventsEqual(timeSignatureEvents[0], nonDefaultTimeSignatureEvent, false, "Invalid Time Signature event.");
                });
        }

        [Obsolete("OBS1")]
        [Test]
        public void Write_Compression_DeleteUnknownChunks_Obsolete()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50)),
                new UnknownChunk("abcd"));

            Write_Compression_Obsolete(
                midiFile,
                CompressionPolicy.DeleteUnknownChunks,
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
        public void Write_Compression_NoCompression()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)100, (SevenBitNumber)50),
                    new NoteOffEvent((SevenBitNumber)100, (SevenBitNumber)50)));

            Write(
                midiFile,
                settings => { },
                (fileInfo1, fileInfo2) => Assert.AreEqual(fileInfo1.Length, fileInfo2.Length, "File size is invalid."));
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
                (fileInfo1, fileInfo2) => Assert.Less(fileInfo2.Length, fileInfo1.Length, "File size is invalid."));
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
                    Assert.AreEqual(
                        2,
                        originalMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<KeySignatureEvent>().Count(),
                        "Invalid count of Key Signature events in original file.");

                    var newMidiFile = MidiFile.Read(fileInfo2.FullName);
                    var keySignatureEvents = newMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<KeySignatureEvent>().ToArray();
                    Assert.AreEqual(
                        1,
                        keySignatureEvents.Length,
                        "Invalid count of Key Signature events in new file.");

                    MidiAsserts.AreEventsEqual(keySignatureEvents[0], nonDefaultKeySignatureEvent, false, "Invalid Key Signature event.");
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
                    Assert.AreEqual(
                        2,
                        originalMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<SetTempoEvent>().Count(),
                        "Invalid count of Set Tempo events in original file.");

                    var newMidiFile = MidiFile.Read(fileInfo2.FullName);
                    var setTempoEvents = newMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<SetTempoEvent>().ToArray();
                    Assert.AreEqual(
                        1,
                        setTempoEvents.Length,
                        "Invalid count of Set Tempo events in new file.");

                    MidiAsserts.AreEventsEqual(setTempoEvents[0], nonDefaultSetTempoEvent, false, "Invalid Set Tempo event.");
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

                    Assert.AreEqual(
                        2,
                        originalMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<KeySignatureEvent>().Count(),
                        "Invalid count of Key Signature events in original file.");

                    var keySignatureEvents = newMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<KeySignatureEvent>().ToArray();
                    Assert.AreEqual(
                        1,
                        keySignatureEvents.Length,
                        "Invalid count of Key Signature events in new file.");

                    MidiAsserts.AreEventsEqual(keySignatureEvents[0], nonDefaultKeySignatureEvent, false, "Invalid Key Signature event.");

                    //

                    Assert.AreEqual(
                        2,
                        originalMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<SetTempoEvent>().Count(),
                        "Invalid count of Set Tempo events in original file.");

                    var setTempoEvents = newMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<SetTempoEvent>().ToArray();
                    Assert.AreEqual(
                        1,
                        setTempoEvents.Length,
                        "Invalid count of Set Tempo events in new file.");

                    MidiAsserts.AreEventsEqual(setTempoEvents[0], nonDefaultSetTempoEvent, false, "Invalid Set Tempo event.");
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
                    Assert.AreEqual(
                        2,
                        originalMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<TimeSignatureEvent>().Count(),
                        "Invalid count of Time Signature events in original file.");

                    var newMidiFile = MidiFile.Read(fileInfo2.FullName);
                    var timeSignatureEvents = newMidiFile.GetTrackChunks().SelectMany(c => c.Events).OfType<TimeSignatureEvent>().ToArray();
                    Assert.AreEqual(
                        1,
                        timeSignatureEvents.Length,
                        "Invalid count of Time Signature events in new file.");

                    MidiAsserts.AreEventsEqual(timeSignatureEvents[0], nonDefaultTimeSignatureEvent, false, "Invalid Time Signature event.");
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
                    MidiAsserts.AreFilesEqual(originalMidiFile, newMidiFile, true, "New file is invalid.");
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

                    Assert.Throws<NoHeaderChunkException>(() => MidiFile.Read(fileInfo2.FullName));
                    var newMidiFile = MidiFile.Read(fileInfo2.FullName, new ReadingSettings
                    {
                        NoHeaderChunkPolicy = NoHeaderChunkPolicy.Ignore
                    });

                    MidiAsserts.AreFilesEqual(originalMidiFile, newMidiFile, false, "New file is invalid.");
                });
        }

        [Test]
        public void Write_StreamIsNotDisposed()
        {
            var midiFile = new MidiFile();

            using (var streamToWrite = new MemoryStream())
            {
                midiFile.Write(streamToWrite);
                Assert.DoesNotThrow(() => { var l = streamToWrite.Length; });
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
                    MidiAsserts.AreFilesEqual(midiFile, newMidiFile, false, "File is invalid.");
                });
        }

        #endregion

        #region Private methods

        private void Write_Compression_Obsolete(MidiFile midiFile, CompressionPolicy compressionPolicy, Action<FileInfo, FileInfo> fileInfosAction)
        {
            MidiFileTestUtilities.Write(
                midiFile,
                filePath =>
                {
                    var fileInfo = new FileInfo(filePath);

                    MidiFileTestUtilities.Write(
                        midiFile,
                        filePath2 =>
                        {
                            var fileInfo2 = new FileInfo(filePath2);

                            fileInfosAction(fileInfo, fileInfo2);
                        },
                        new WritingSettings { CompressionPolicy = compressionPolicy });
                },
                new WritingSettings { CompressionPolicy = CompressionPolicy.NoCompression });
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
