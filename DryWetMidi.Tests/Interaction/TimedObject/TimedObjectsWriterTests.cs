using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using System;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class TimedObjectsWriterTests
    {
        #region Test methods

        [Test]
        public void WriteObject_TimedEvent_Single([Values(0, 100)] long time) => WriteObjects(
            writersActions: (tokensWriter, objectsWriter) =>
            {
                tokensWriter.StartTrackChunk();
                objectsWriter.WriteObject(new TimedEvent(new TextEvent("A"), time));
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = time }))
            {
                _originalFormat = (ushort?)MidiFileFormat.MultiTrack
            });

        [Test]
        public void WriteObject_TimedEvent_Multiple([Values(0, 100)] long time, [Values(0, 10)] long offset) => WriteObjects(
            writersActions: (tokensWriter, objectsWriter) =>
            {
                tokensWriter.StartTrackChunk();
                objectsWriter.WriteObject(new TimedEvent(new NoteOnEvent(), time));
                objectsWriter.WriteObject(new TimedEvent(new NoteOffEvent(), time + offset));
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = time },
                    new NoteOffEvent() { DeltaTime = offset }))
            {
                _originalFormat = (ushort?)MidiFileFormat.MultiTrack
            },
            readingSettings: new ReadingSettings
            {
                SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn
            });

        [Test]
        public void WriteObject_Note_Single([Values(0, 100)] long time, [Values(0, 50)] long length) => WriteObjects(
            writersActions: (tokensWriter, objectsWriter) =>
            {
                tokensWriter.StartTrackChunk();
                objectsWriter.WriteObject(new Note((SevenBitNumber)70, length, time) { Channel = (FourBitNumber)3 });
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity) { DeltaTime = time, Channel = (FourBitNumber)3 },
                    new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity) { DeltaTime = length, Channel = (FourBitNumber)3 }))
            {
                _originalFormat = (ushort?)MidiFileFormat.MultiTrack
            });

        [Test]
        public void WriteObject_Note_Multiple_NonOverlapping(
            [Values(0, 100)] long time,
            [Values(0, 50)] long length1,
            [Values(0, 10)] long offset,
            [Values(0, 80)] long length2) => WriteObjects(
            writersActions: (tokensWriter, objectsWriter) =>
            {
                tokensWriter.StartTrackChunk();
                objectsWriter.WriteObject(new Note((SevenBitNumber)70, length1, time) { Channel = (FourBitNumber)3 });
                objectsWriter.WriteObject(new Note((SevenBitNumber)80, length2, time + length1 + offset) { Channel = (FourBitNumber)2 });
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity) { DeltaTime = time, Channel = (FourBitNumber)3 },
                    new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity) { DeltaTime = length1, Channel = (FourBitNumber)3 },
                    new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity) { DeltaTime = offset, Channel = (FourBitNumber)2 },
                    new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity) { DeltaTime = length2, Channel = (FourBitNumber)2 }))
            {
                _originalFormat = (ushort?)MidiFileFormat.MultiTrack
            });

        [Test]
        public void WriteObject_Note_Multiple_Overlapping_1(
            [Values(0, 100)] long time,
            [Values(50)] long length1,
            [Values(0, 10)] long offset,
            [Values(800)] long length2) => WriteObjects(
            writersActions: (tokensWriter, objectsWriter) =>
            {
                tokensWriter.StartTrackChunk();
                objectsWriter.WriteObject(new Note((SevenBitNumber)70, length1, time) { Channel = (FourBitNumber)3 });
                objectsWriter.WriteObject(new Note((SevenBitNumber)80, length2, time + offset) { Channel = (FourBitNumber)2 });
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity) { DeltaTime = time, Channel = (FourBitNumber)3 },
                    new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity) { DeltaTime = offset, Channel = (FourBitNumber)2 },
                    new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity) { DeltaTime = length1 - offset, Channel = (FourBitNumber)3 },
                    new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity) { DeltaTime = offset + length2 - length1, Channel = (FourBitNumber)2 }))
            {
                _originalFormat = (ushort?)MidiFileFormat.MultiTrack
            });

        [Test]
        public void WriteObject_Note_Multiple_Overlapping_2(
            [Values(0, 100)] long time,
            [Values(500)] long length1,
            [Values(0, 10)] long offset,
            [Values(200)] long length2) => WriteObjects(
            writersActions: (tokensWriter, objectsWriter) =>
            {
                tokensWriter.StartTrackChunk();
                objectsWriter.WriteObject(new Note((SevenBitNumber)70, length1, time) { Channel = (FourBitNumber)3 });
                objectsWriter.WriteObject(new Note((SevenBitNumber)80, length2, time + offset) { Channel = (FourBitNumber)2 });
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity) { DeltaTime = time, Channel = (FourBitNumber)3 },
                    new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity) { DeltaTime = offset, Channel = (FourBitNumber)2 },
                    new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity) { DeltaTime = length2, Channel = (FourBitNumber)2 },
                    new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity) { DeltaTime = length1 - (offset + length2), Channel = (FourBitNumber)3 }))
            {
                _originalFormat = (ushort?)MidiFileFormat.MultiTrack
            });

        [Test]
        public void WriteObject_Chord_Single_1() => WriteObjects(
            writersActions: (tokensWriter, objectsWriter) =>
            {
                tokensWriter.StartTrackChunk();
                objectsWriter.WriteObject(new Chord(
                    new Note((SevenBitNumber)10, 100, 50),
                    new Note((SevenBitNumber)20, 100, 50),
                    new Note((SevenBitNumber)30, 100, 50)));
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)10, Note.DefaultVelocity) { DeltaTime = 50 },
                    new NoteOnEvent((SevenBitNumber)20, Note.DefaultVelocity),
                    new NoteOnEvent((SevenBitNumber)30, Note.DefaultVelocity),
                    new NoteOffEvent((SevenBitNumber)10, Note.DefaultOffVelocity) { DeltaTime = 100 },
                    new NoteOffEvent((SevenBitNumber)20, Note.DefaultOffVelocity),
                    new NoteOffEvent((SevenBitNumber)30, Note.DefaultOffVelocity)))
            {
                _originalFormat = (ushort?)MidiFileFormat.MultiTrack
            });

        [Test]
        public void WriteObject_Chord_Single_2() => WriteObjects(
            writersActions: (tokensWriter, objectsWriter) =>
            {
                tokensWriter.StartTrackChunk();
                objectsWriter.WriteObject(new Chord(
                    new Note((SevenBitNumber)10, 100, 50),
                    new Note((SevenBitNumber)20, 100, 60),
                    new Note((SevenBitNumber)30, 100, 70)));
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)10, Note.DefaultVelocity) { DeltaTime = 50 },
                    new NoteOnEvent((SevenBitNumber)20, Note.DefaultVelocity) { DeltaTime = 10 },
                    new NoteOnEvent((SevenBitNumber)30, Note.DefaultVelocity) { DeltaTime = 10 },
                    new NoteOffEvent((SevenBitNumber)10, Note.DefaultOffVelocity) { DeltaTime = 80 },
                    new NoteOffEvent((SevenBitNumber)20, Note.DefaultOffVelocity) { DeltaTime = 10 },
                    new NoteOffEvent((SevenBitNumber)30, Note.DefaultOffVelocity) { DeltaTime = 10 }))
            {
                _originalFormat = (ushort?)MidiFileFormat.MultiTrack
            });

        [Test]
        public void WriteObject_Chord_Single_3() => WriteObjects(
            writersActions: (tokensWriter, objectsWriter) =>
            {
                tokensWriter.StartTrackChunk();
                objectsWriter.WriteObject(new Chord(
                    new Note((SevenBitNumber)10, 100, 0),
                    new Note((SevenBitNumber)20, 80, 10),
                    new Note((SevenBitNumber)30, 60, 20)));
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)10, Note.DefaultVelocity) { DeltaTime = 0 },
                    new NoteOnEvent((SevenBitNumber)20, Note.DefaultVelocity) { DeltaTime = 10 },
                    new NoteOnEvent((SevenBitNumber)30, Note.DefaultVelocity) { DeltaTime = 10 },
                    new NoteOffEvent((SevenBitNumber)30, Note.DefaultOffVelocity) { DeltaTime = 60 },
                    new NoteOffEvent((SevenBitNumber)20, Note.DefaultOffVelocity) { DeltaTime = 10 },
                    new NoteOffEvent((SevenBitNumber)10, Note.DefaultOffVelocity) { DeltaTime = 10 }))
            {
                _originalFormat = (ushort?)MidiFileFormat.MultiTrack
            });

        [Test]
        public void WriteObjects_Mixed() => WriteObjects(
            writersActions: (tokensWriter, objectsWriter) =>
            {
                tokensWriter.StartTrackChunk();
                objectsWriter.WriteObjects(new ITimedObject[]
                {
                    new Chord(
                        new Note((SevenBitNumber)10, 100, 0),
                        new Note((SevenBitNumber)20, 80, 10),
                        new Note((SevenBitNumber)30, 60, 20)),
                    new TimedEvent(new TextEvent("A"), 5),
                    new Note((SevenBitNumber)80, 30, 300)
                });
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)10, Note.DefaultVelocity) { DeltaTime = 0 },
                    new TextEvent("A") { DeltaTime = 5 },
                    new NoteOnEvent((SevenBitNumber)20, Note.DefaultVelocity) { DeltaTime = 5 },
                    new NoteOnEvent((SevenBitNumber)30, Note.DefaultVelocity) { DeltaTime = 10 },
                    new NoteOffEvent((SevenBitNumber)30, Note.DefaultOffVelocity) { DeltaTime = 60 },
                    new NoteOffEvent((SevenBitNumber)20, Note.DefaultOffVelocity) { DeltaTime = 10 },
                    new NoteOffEvent((SevenBitNumber)10, Note.DefaultOffVelocity) { DeltaTime = 10 },
                    new NoteOnEvent((SevenBitNumber)80, Note.DefaultVelocity) { DeltaTime = 200 },
                    new NoteOffEvent((SevenBitNumber)80, Note.DefaultOffVelocity) { DeltaTime = 30 }))
            {
                _originalFormat = (ushort?)MidiFileFormat.MultiTrack
            });

        [Test]
        public void WriteObject_WrongOrder() => Assert.Throws<InvalidOperationException>(() => WriteObjects(
            writersActions: (tokensWriter, objectsWriter) =>
            {
                tokensWriter.StartTrackChunk();
                objectsWriter.WriteObject(new TimedEvent(new TextEvent("A"), 100));
                objectsWriter.WriteObject(new TimedEvent(new TextEvent("B"), 50));
            },
            expectedMidiFile: new MidiFile()));

        [Test]
        public void WriteObject_MultipleTrackChunks() => WriteObjects(
            writersActions: (tokensWriter, objectsWriter) =>
            {
                tokensWriter.StartTrackChunk();
                objectsWriter.WriteObject(new TimedEvent(new NoteOnEvent(), 100));
                objectsWriter.WriteObject(new TimedEvent(new NoteOffEvent(), 150));

                tokensWriter.StartTrackChunk();
                objectsWriter.WriteObject(new TimedEvent(new TextEvent("A"), 20));
                objectsWriter.WriteObject(new TimedEvent(new ProgramChangeEvent((SevenBitNumber)60), 70));
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new NoteOnEvent() { DeltaTime = 100 },
                    new NoteOffEvent() { DeltaTime = 50 }),
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = 20 },
                    new ProgramChangeEvent((SevenBitNumber)60) { DeltaTime = 50 }))
            {
                _originalFormat = (ushort?)MidiFileFormat.MultiTrack
            },
            readingSettings: new ReadingSettings
            {
                SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn
            });

        #endregion

        #region Private methods

        private void WriteObjects(
            Action<MidiTokensWriter, TimedObjectsWriter> writersActions,
            MidiFile expectedMidiFile,
            WritingSettings settings = null,
            ReadingSettings readingSettings = null,
            MidiFileFormat format = MidiFileFormat.MultiTrack,
            TimeDivision timeDivision = null,
            string message = null)
        {
            var filePath = FileOperations.GetTempFilePath();

            try
            {
                using (var tokensWriter = MidiFile.WriteLazy(filePath, true, format, settings, timeDivision))
                using (var objectsWriter = new TimedObjectsWriter(tokensWriter))
                {
                    writersActions(tokensWriter, objectsWriter);
                }

                var actualMidiFile = MidiFile.Read(filePath, readingSettings);
                MidiAsserts.AreEqual(expectedMidiFile, actualMidiFile, true, $"Invalid file. {message}");
            }
            finally
            {
                FileOperations.DeleteFile(filePath);
            }
        }

        #endregion
    }
}
