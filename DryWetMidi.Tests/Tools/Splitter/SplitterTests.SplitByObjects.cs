using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class SplitterTests
    {
        #region Test methods

        [Test]
        public void SplitByObjects_EmptyFile() => SplitByObjects(
            timedObjects: Array.Empty<ITimedObject>(),
            objectType: ObjectType.Note,
            expectedObjects: Array.Empty<ICollection<ITimedObject>>(),
            settings: new SplitByObjectsSettings
            {
                KeySelector = obj => ((Note)obj).GetObjectId(),
            });

        [Test]
        public void SplitByObjects_Notes_NoteNumberAndChannel() => SplitByObjects(
            timedObjects: new[]
            {
                new Note((SevenBitNumber)100, 20, 0),
                new Note((SevenBitNumber)50, 10, 10),
                new Note((SevenBitNumber)100, 30, 30),
                new Note((SevenBitNumber)50, 40, 100),
                new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
            },
            objectType: ObjectType.Note,
            expectedObjects: new[]
            {
                new[]
                {
                    new Note((SevenBitNumber)100, 20, 0),
                    new Note((SevenBitNumber)100, 30, 30),
                },
                new[]
                {
                    new Note((SevenBitNumber)50, 10, 10),
                    new Note((SevenBitNumber)50, 40, 100),
                },
                new[]
                {
                    new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
                },
            },
            settings: new SplitByObjectsSettings
            {
                KeySelector = obj => ((Note)obj).GetObjectId(),
            });

        [Test]
        public void SplitByObjects_Notes_Default() => SplitByObjects(
            timedObjects: new[]
            {
                new Note((SevenBitNumber)100, 20, 0),
                new Note((SevenBitNumber)50, 10, 10),
                new Note((SevenBitNumber)100, 30, 30),
                new Note((SevenBitNumber)50, 40, 100),
                new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
            },
            objectType: ObjectType.Note,
            expectedObjects: new[]
            {
                new[]
                {
                    new Note((SevenBitNumber)100, 20, 0),
                    new Note((SevenBitNumber)100, 30, 30),
                },
                new[]
                {
                    new Note((SevenBitNumber)50, 10, 10),
                    new Note((SevenBitNumber)50, 40, 100),
                },
                new[]
                {
                    new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
                },
            });

        [Test]
        public void SplitByObjects_NotesAndTimedEvents_Default() => SplitByObjects(
            timedObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)100, 20, 0),
                new Note((SevenBitNumber)50, 10, 10),
                new TimedEvent(new TextEvent("A"), 15),
                new Note((SevenBitNumber)100, 30, 30),
                new TimedEvent(new TextEvent("B"), 35),
                new Note((SevenBitNumber)50, 40, 100),
                new TimedEvent(new ProgramChangeEvent(SevenBitNumber.MaxValue), 105),
                new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
                new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70), 205),
            },
            objectType: ObjectType.Note | ObjectType.TimedEvent,
            expectedObjects: new[]
            {
                new[]
                {
                    new Note((SevenBitNumber)100, 20, 0),
                    new Note((SevenBitNumber)100, 30, 30),
                },
                new[]
                {
                    new Note((SevenBitNumber)50, 10, 10),
                    new Note((SevenBitNumber)50, 40, 100),
                },
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 15),
                    new TimedEvent(new TextEvent("B"), 35),
                },
                new ITimedObject[]
                {
                    new TimedEvent(new ProgramChangeEvent(SevenBitNumber.MaxValue), 105),
                    new TimedEvent(new ProgramChangeEvent((SevenBitNumber)70), 205),
                },
                new[]
                {
                    new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
                },
            });

        [Test]
        public void SplitByObjects_Notes_NoteNumber() => SplitByObjects(
            timedObjects: new[]
            {
                new Note((SevenBitNumber)100, 20, 0),
                new Note((SevenBitNumber)50, 10, 10),
                new Note((SevenBitNumber)100, 30, 30),
                new Note((SevenBitNumber)50, 40, 100),
                new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
            },
            objectType: ObjectType.Note,
            expectedObjects: new[]
            {
                new[]
                {
                    new Note((SevenBitNumber)100, 20, 0),
                    new Note((SevenBitNumber)100, 30, 30),
                },
                new[]
                {
                    new Note((SevenBitNumber)50, 10, 10),
                    new Note((SevenBitNumber)50, 40, 100),
                    new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
                },
            },
            settings: new SplitByObjectsSettings
            {
                KeySelector = obj => ObjectIdUtilities.GetObjectId(((Note)obj).NoteNumber),
            });

        [Test]
        public void SplitByObjects_Notes_Channel() => SplitByObjects(
            timedObjects: new[]
            {
                new Note((SevenBitNumber)100, 20, 0),
                new Note((SevenBitNumber)50, 10, 10),
                new Note((SevenBitNumber)100, 30, 30),
                new Note((SevenBitNumber)50, 40, 100),
                new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
            },
            objectType: ObjectType.Note,
            expectedObjects: new[]
            {
                new[]
                {
                    new Note((SevenBitNumber)100, 20, 0),
                    new Note((SevenBitNumber)50, 10, 10),
                    new Note((SevenBitNumber)100, 30, 30),
                    new Note((SevenBitNumber)50, 40, 100),
                },
                new[]
                {
                    new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
                }
            },
            settings: new SplitByObjectsSettings
            {
                KeySelector = obj => ObjectIdUtilities.GetObjectId(((Note)obj).Channel),
            });

        [Test]
        public void SplitByObjects_Notes_Filter_1() => SplitByObjects(
            timedObjects: new[]
            {
                new Note((SevenBitNumber)50, 50, 0),
                new Note((SevenBitNumber)50, 70, 10),
                new Note((SevenBitNumber)50, 100, 20),
            },
            objectType: ObjectType.Note,
            expectedObjects: new[]
            {
                new[]
                {
                    new Note((SevenBitNumber)50, 50, 0),
                    new Note((SevenBitNumber)50, 100, 20),
                },
            },
            settings: new SplitByObjectsSettings
            {
                KeySelector = obj => ObjectIdUtilities.GetObjectId(((Note)obj).NoteNumber),
                Filter = obj => ((Note)obj).Length != 70
            },
            objectDetectionSettings: new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.FirstNoteOn
                }
            });

        [Test]
        public void SplitByObjects_Notes_Filter_2() => SplitByObjects(
            timedObjects: new[]
            {
                new Note((SevenBitNumber)50, 50, 0),
                new Note((SevenBitNumber)50, 70, 10),
                new Note((SevenBitNumber)50, 100, 20),
            },
            objectType: ObjectType.Note,
            expectedObjects: new[]
            {
                new[]
                {
                    new Note((SevenBitNumber)50, 120, 0),
                    new Note((SevenBitNumber)50, 30, 20),
                },
            },
            settings: new SplitByObjectsSettings
            {
                KeySelector = obj => ObjectIdUtilities.GetObjectId(((Note)obj).NoteNumber),
                Filter = obj => ((Note)obj).Length != 70
            },
            objectDetectionSettings: new ObjectDetectionSettings
            {
                NoteDetectionSettings = new NoteDetectionSettings
                {
                    NoteStartDetectionPolicy = NoteStartDetectionPolicy.LastNoteOn
                }
            });

        [Test]
        public void SplitByObjects_WriteToAllFiles_1() => SplitByObjects(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)100, 20, 0),
                new TimedEvent(new TextEvent("B"), 5),
                new Note((SevenBitNumber)50, 10, 10),
                new Note((SevenBitNumber)100, 30, 30),
                new Note((SevenBitNumber)50, 40, 100),
                new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
                new TimedEvent(new TextEvent("C"), 150),
            },
            objectType: ObjectType.Note | ObjectType.TimedEvent,
            expectedObjects: new[]
            {
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new Note((SevenBitNumber)100, 20, 0),
                    new TimedEvent(new TextEvent("B"), 5),
                    new Note((SevenBitNumber)100, 30, 30),
                    new TimedEvent(new TextEvent("C"), 150),
                },
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new TimedEvent(new TextEvent("B"), 5),
                    new Note((SevenBitNumber)50, 10, 10),
                    new Note((SevenBitNumber)50, 40, 100),
                    new TimedEvent(new TextEvent("C"), 150),
                    new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
                },
            },
            settings: new SplitByObjectsSettings
            {
                KeySelector = obj => ObjectIdUtilities.GetObjectId((obj as Note)?.NoteNumber),
                WriteToAllFilesPredicate = obj => obj is TimedEvent,
            });

        [Test]
        public void SplitByObjects_WriteToAllFiles_2() => Assert.Throws<AssertionException>(() => SplitByObjects(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)100, 20, 0),
                new TimedEvent(new TextEvent("B"), 5),
                new Note((SevenBitNumber)50, 10, 10),
                new Note((SevenBitNumber)100, 30, 30),
                new Note((SevenBitNumber)50, 40, 100),
                new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
                new TimedEvent(new TextEvent("C"), 150),
            },
            objectType: ObjectType.Note | ObjectType.TimedEvent,
            expectedObjects: new[]
            {
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new Note((SevenBitNumber)100, 20, 0),
                    new TimedEvent(new TextEvent("B"), 5),
                    new Note((SevenBitNumber)100, 30, 30),
                    new TimedEvent(new TextEvent("C"), 150),
                },
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new TimedEvent(new TextEvent("B"), 5),
                    new Note((SevenBitNumber)50, 10, 10),
                    new Note((SevenBitNumber)50, 40, 100),
                    new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
                },
            },
            settings: new SplitByObjectsSettings
            {
                KeySelector = obj => ObjectIdUtilities.GetObjectId((obj as Note)?.NoteNumber),
                WriteToAllFilesPredicate = obj => obj is TimedEvent,
            }));

        [Test]
        public void SplitByObjects_WriteToAllFiles_3() => SplitByObjects(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)100, 20, 0),
                new TimedEvent(new TextEvent("B"), 5),
                new Note((SevenBitNumber)50, 10, 10),
                new Note((SevenBitNumber)100, 30, 30),
                new Note((SevenBitNumber)50, 40, 100),
                new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
                new TimedEvent(new TextEvent("C"), 150),
            },
            objectType: ObjectType.Note | ObjectType.TimedEvent,
            expectedObjects: new[]
            {
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new Note((SevenBitNumber)100, 20, 0),
                    new Note((SevenBitNumber)100, 30, 30),
                    new TimedEvent(new TextEvent("C"), 150),
                },
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new Note((SevenBitNumber)50, 10, 10),
                    new Note((SevenBitNumber)50, 40, 100),
                    new TimedEvent(new TextEvent("C"), 150),
                    new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
                },
            },
            settings: new SplitByObjectsSettings
            {
                KeySelector = obj => ObjectIdUtilities.GetObjectId((obj as Note)?.NoteNumber),
                WriteToAllFilesPredicate = obj => obj is TimedEvent,
                AllFilesObjectsFilter = obj => ((TextEvent)((TimedEvent)obj).Event).Text != "B"
            });

        [Test]
        public void SplitByObjects_WriteToAllFiles_4() => SplitByObjects(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)100, 20, 0),
                new Note((SevenBitNumber)50, 10, 10),
                new Note((SevenBitNumber)100, 30, 30),
                new Note((SevenBitNumber)50, 40, 100),
                new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
                new TimedEvent(new TextEvent("C"), 150),
            },
            objectType: ObjectType.Note | ObjectType.TimedEvent,
            expectedObjects: new[]
            {
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new Note((SevenBitNumber)100, 20, 0),
                    new Note((SevenBitNumber)100, 30, 30),
                    new TimedEvent(new TextEvent("C"), 150),
                    new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
                },
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new Note((SevenBitNumber)50, 10, 10),
                    new Note((SevenBitNumber)50, 40, 100),
                    new TimedEvent(new TextEvent("C"), 150),
                    new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
                },
            },
            settings: new SplitByObjectsSettings
            {
                KeySelector = obj => ObjectIdUtilities.GetObjectId((obj as Note)?.NoteNumber),
                WriteToAllFilesPredicate = obj => obj is TimedEvent || ((Note)obj).Channel == 2,
            });

        [Test]
        public void SplitByObjects_WriteToAllFiles_5() => SplitByObjects(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)100, 20, 0),
                new Note((SevenBitNumber)50, 10, 10),
                new Note((SevenBitNumber)100, 30, 30),
                new Note((SevenBitNumber)50, 40, 100),
                new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
                new TimedEvent(new TextEvent("C"), 150),
            },
            objectType: ObjectType.Note | ObjectType.TimedEvent,
            expectedObjects: new[]
            {
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new TimedEvent(new TextEvent("C"), 150),
                },
                new ITimedObject[]
                {
                    new Note((SevenBitNumber)100, 20, 0),
                    new Note((SevenBitNumber)100, 30, 30),
                },
                new ITimedObject[]
                {
                    new Note((SevenBitNumber)50, 10, 10),
                    new Note((SevenBitNumber)50, 40, 100),
                    new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
                },
            },
            settings: new SplitByObjectsSettings
            {
                KeySelector = obj => ObjectIdUtilities.GetObjectId((obj as Note)?.NoteNumber),
            });

        [Test]
        public void SplitByObjects_TimeDivision() => SplitByObjects(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)100, 20, 0),
                new Note((SevenBitNumber)50, 10, 10),
                new Note((SevenBitNumber)100, 30, 30),
                new Note((SevenBitNumber)50, 40, 100),
                new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
                new TimedEvent(new TextEvent("C"), 150),
            },
            objectType: ObjectType.Note | ObjectType.TimedEvent,
            expectedObjects: new[]
            {
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new Note((SevenBitNumber)100, 20, 0),
                    new Note((SevenBitNumber)100, 30, 30),
                    new TimedEvent(new TextEvent("C"), 150),
                    new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
                },
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new Note((SevenBitNumber)50, 10, 10),
                    new Note((SevenBitNumber)50, 40, 100),
                    new TimedEvent(new TextEvent("C"), 150),
                    new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
                },
            },
            settings: new SplitByObjectsSettings
            {
                KeySelector = obj => ObjectIdUtilities.GetObjectId((obj as Note)?.NoteNumber),
                WriteToAllFilesPredicate = obj => obj is TimedEvent || ((Note)obj).Channel == 2,
            },
            ticksPerQuarterNote: 200);

        [Test]
        public void SplitByObjects_TempoMap_1() => SplitByObjects(
            timedObjects: new ITimedObject[]
            {
                new TimedEvent(new TextEvent("A"), 0),
                new Note((SevenBitNumber)100, 20, 0),
                new TimedEvent(new SetTempoEvent(100000), 5),
                new Note((SevenBitNumber)50, 10, 10),
                new Note((SevenBitNumber)100, 30, 30),
                new Note((SevenBitNumber)50, 40, 100),
                new TimedEvent(new TimeSignatureEvent(3, 8), 150),
                new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
                new TimedEvent(new TextEvent("C"), 150),
            },
            objectType: ObjectType.Note,
            expectedObjects: new[]
            {
                new ITimedObject[]
                {
                    new Note((SevenBitNumber)100, 20, 0),
                    new TimedEvent(new SetTempoEvent(100000), 5),
                    new Note((SevenBitNumber)100, 30, 30),
                    new TimedEvent(new TimeSignatureEvent(3, 8), 150),
                },
                new ITimedObject[]
                {
                    new TimedEvent(new SetTempoEvent(100000), 5),
                    new Note((SevenBitNumber)50, 10, 10),
                    new Note((SevenBitNumber)50, 40, 100),
                    new TimedEvent(new TimeSignatureEvent(3, 8), 150),
                    new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
                },
            },
            settings: new SplitByObjectsSettings
            {
                KeySelector = obj => ObjectIdUtilities.GetObjectId((obj as Note)?.NoteNumber),
            });

        [Test]
        public void SplitByObjects_TempoMap_2() => SplitByObjects(
            timedObjects: new ITimedObject[]
            {
                new Note((SevenBitNumber)100, 20, 0),
                new TimedEvent(new SetTempoEvent(100000), 5),
                new Note((SevenBitNumber)50, 10, 10),
                new Note((SevenBitNumber)100, 30, 30),
                new Note((SevenBitNumber)50, 40, 100),
                new TimedEvent(new TimeSignatureEvent(3, 8), 150),
                new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
            },
            objectType: ObjectType.Note | ObjectType.TimedEvent,
            expectedObjects: new[]
            {
                new ITimedObject[]
                {
                    new Note((SevenBitNumber)100, 20, 0),
                    new TimedEvent(new SetTempoEvent(100000), 5),
                    new Note((SevenBitNumber)100, 30, 30),
                    new TimedEvent(new TimeSignatureEvent(3, 8), 150),
                },
                new ITimedObject[]
                {
                    new TimedEvent(new SetTempoEvent(100000), 5),
                    new Note((SevenBitNumber)50, 10, 10),
                    new Note((SevenBitNumber)50, 40, 100),
                    new TimedEvent(new TimeSignatureEvent(3, 8), 150),
                    new Note((SevenBitNumber)50, 40, 200) { Channel = (FourBitNumber)2 },
                },
            },
            settings: new SplitByObjectsSettings
            {
                KeySelector = obj => ObjectIdUtilities.GetObjectId((obj as Note)?.NoteNumber),
                WriteToAllFilesPredicate = obj => obj is TimedEvent,
            });

        #endregion

        #region Private methods

        private void SplitByObjects(
            ICollection<ITimedObject> timedObjects,
            ObjectType objectType,
            ICollection<ICollection<ITimedObject>> expectedObjects,
            SplitByObjectsSettings settings = null,
            ObjectDetectionSettings objectDetectionSettings = null,
            short? ticksPerQuarterNote = null)
        {
            var midiFile = timedObjects.ToFile();
            if (ticksPerQuarterNote != null)
                midiFile.TimeDivision = new TicksPerQuarterNoteTimeDivision(ticksPerQuarterNote.Value);

            var actualFiles = midiFile.SplitByObjects(
                objectType,
                settings,
                objectDetectionSettings).ToArray();

            var expectedFiles = expectedObjects
                .Select(objects =>
                {
                    var file = objects.ToFile();
                    if (ticksPerQuarterNote != null)
                        file.TimeDivision = new TicksPerQuarterNoteTimeDivision(ticksPerQuarterNote.Value);

                    return file;
                })
                .ToArray();

            MidiAsserts.AreEqual(expectedFiles, actualFiles, false, "Invalid files.");
        }

        #endregion
    }
}
