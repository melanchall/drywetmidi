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
            keySelector: obj => ((Note)obj).GetNoteId(),
            writeToAllFilesPredicate: obj => false,
            expectedObjects: Array.Empty<ICollection<ITimedObject>>());

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
            keySelector: obj => ((Note)obj).GetNoteId(),
            writeToAllFilesPredicate: obj => false,
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
            keySelector: obj => ((Note)obj).NoteNumber,
            writeToAllFilesPredicate: obj => false,
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
            keySelector: obj => ((Note)obj).Channel,
            writeToAllFilesPredicate: obj => false,
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
            keySelector: obj => ((Note)obj).NoteNumber,
            writeToAllFilesPredicate: obj => false,
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
            keySelector: obj => ((Note)obj).NoteNumber,
            writeToAllFilesPredicate: obj => false,
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
            keySelector: obj => (obj as Note)?.NoteNumber,
            writeToAllFilesPredicate: obj => obj is TimedEvent,
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
            keySelector: obj => (obj as Note)?.NoteNumber,
            writeToAllFilesPredicate: obj => obj is TimedEvent,
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
            keySelector: obj => (obj as Note)?.NoteNumber,
            writeToAllFilesPredicate: obj => obj is TimedEvent,
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
            keySelector: obj => (obj as Note)?.NoteNumber,
            writeToAllFilesPredicate: obj => obj is TimedEvent || ((Note)obj).Channel == 2,
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
            keySelector: obj => (obj as Note)?.NoteNumber,
            writeToAllFilesPredicate: obj => false,
            expectedObjects: new[]
            {
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
                new ITimedObject[]
                {
                    new TimedEvent(new TextEvent("A"), 0),
                    new TimedEvent(new TextEvent("C"), 150),
                },
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
            keySelector: obj => (obj as Note)?.NoteNumber,
            writeToAllFilesPredicate: obj => obj is TimedEvent || ((Note)obj).Channel == 2,
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
            keySelector: obj => (obj as Note)?.NoteNumber,
            writeToAllFilesPredicate: obj => false,
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
            keySelector: obj => (obj as Note)?.NoteNumber,
            writeToAllFilesPredicate: obj => obj is TimedEvent,
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
            });

        #endregion

        #region Private methods

        private void SplitByObjects<TKey>(
            ICollection<ITimedObject> timedObjects,
            ObjectType objectType,
            Func<ITimedObject, TKey> keySelector,
            Predicate<ITimedObject> writeToAllFilesPredicate,
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
                keySelector,
                writeToAllFilesPredicate,
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
