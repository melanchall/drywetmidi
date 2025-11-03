using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TempoMapManagingUtilitiesTests
    {
        #region Test methods

        [Test]
        public void GetTempoMap_EmptyFile()
        {
            GetTempoMap_Default(new MidiFile());
        }

        [Test]
        public void GetTempoMap_NoTempoAndTimeSignatureChanges_OneTrackChunk()
        {
            GetTempoMap_Default(new MidiFile(new TrackChunk(Enumerable.Range(0, 1000).Select(i => new NoteOnEvent { DeltaTime = 10 }))));
        }

        [Test]
        public void GetTempoMap_NoTempoAndTimeSignatureChanges_MultipleTrackChunks()
        {
            GetTempoMap_Default(new MidiFile(Enumerable.Range(0, 10).Select(i => new TrackChunk(Enumerable.Range(0, 1000).Select(j => new NoteOnEvent { DeltaTime = 10 })))));
        }

        [Test]
        public void GetTempoMap_OnlySetTempo_OneTrackChunk()
        {
            var midiFile = new MidiFile(new TrackChunk(new SetTempoEvent(10) { DeltaTime = 2 }, new SetTempoEvent(20) { DeltaTime = 2 }, new SetTempoEvent(10) { DeltaTime = 2 }));
            var tempoMap = midiFile.GetTempoMap();
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<Tempo>(2, new Tempo(10)),
                    new ValueChange<Tempo>(4, new Tempo(20)),
                    new ValueChange<Tempo>(6, new Tempo(10)),
                },
                tempoMap.GetTempoChanges(),
                "Tempo changes are invalid.");
            CollectionAssert.IsEmpty(tempoMap.GetTimeSignatureChanges(), "There are time signature changes.");
            ClassicAssert.AreEqual(midiFile.TimeDivision, tempoMap.TimeDivision, "Time division is invalid.");
        }

        [Test]
        public void GetTempoMap_OnlySetTempo_MultipleTrackChunks()
        {
            var midiFile = new MidiFile(Enumerable
                .Range(0, 2)
                .Select(i => new TrackChunk(Enumerable
                    .Range(0, 3)
                    .Select(j => new SetTempoEvent((i + 1) * 10) { DeltaTime = (i * 2) + 2 }))));
            var tempoMap = midiFile.GetTempoMap();
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<Tempo>(2, new Tempo(10)),
                    new ValueChange<Tempo>(4, new Tempo(20)),
                    new ValueChange<Tempo>(6, new Tempo(10)),
                    new ValueChange<Tempo>(8, new Tempo(20)),
                },
                tempoMap.GetTempoChanges(),
                "Tempo changes are invalid.");
            CollectionAssert.IsEmpty(tempoMap.GetTimeSignatureChanges(), "There are time signature changes.");
            ClassicAssert.AreEqual(midiFile.TimeDivision, tempoMap.TimeDivision, "Time division is invalid.");
        }

        [Test]
        public void GetTempoMap_OnlyTimeSignature_OneTrackChunk()
        {
            var midiFile = new MidiFile(new TrackChunk(new TimeSignatureEvent(2, 8) { DeltaTime = 2 }, new TimeSignatureEvent(3, 16) { DeltaTime = 2 }, new TimeSignatureEvent(2, 8) { DeltaTime = 2 }));
            var tempoMap = midiFile.GetTempoMap();
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<TimeSignature>(2, new TimeSignature(2, 8)),
                    new ValueChange<TimeSignature>(4, new TimeSignature(3, 16)),
                    new ValueChange<TimeSignature>(6, new TimeSignature(2, 8)),
                },
                tempoMap.GetTimeSignatureChanges(),
                "Time signature changes are invalid.");
            CollectionAssert.IsEmpty(tempoMap.GetTempoChanges(), "There are tempo changes.");
            ClassicAssert.AreEqual(midiFile.TimeDivision, tempoMap.TimeDivision, "Time division is invalid.");
        }

        [Test]
        public void GetTempoMap_OnlyTimeSignature_MultipleTrackChunks()
        {
            var midiFile = new MidiFile(Enumerable.Range(0, 2).Select(i => new TrackChunk(Enumerable.Range(0, 3).Select(j => new TimeSignatureEvent((byte)((i + 1) * 10), 8) { DeltaTime = (i * 2) + 2 }))));
            var tempoMap = midiFile.GetTempoMap();
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<TimeSignature>(2, new TimeSignature(10, 8)),
                    new ValueChange<TimeSignature>(4, new TimeSignature(20, 8)),
                    new ValueChange<TimeSignature>(6, new TimeSignature(10, 8)),
                    new ValueChange<TimeSignature>(8, new TimeSignature(20, 8)),
                },
                tempoMap.GetTimeSignatureChanges(),
                "Time signature changes are invalid.");
            CollectionAssert.IsEmpty(tempoMap.GetTempoChanges(), "There are tempo changes.");
            ClassicAssert.AreEqual(midiFile.TimeDivision, tempoMap.TimeDivision, "Time division is invalid.");
        }

        [Test]
        public void GetTempoMap_OneTrackChunk()
        {
            var midiFile = new MidiFile(new TrackChunk(
                new TimeSignatureEvent(2, 8) { DeltaTime = 2 },
                new SetTempoEvent(10) { DeltaTime = 3 },
                new TimeSignatureEvent(3, 16) { DeltaTime = 2 },
                new SetTempoEvent(20) { DeltaTime = 3 },
                new TimeSignatureEvent(2, 8) { DeltaTime = 2 },
                new SetTempoEvent(10) { DeltaTime = 3 }));
            
            var tempoMap = midiFile.GetTempoMap();
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<TimeSignature>(2, new TimeSignature(2, 8)),
                    new ValueChange<TimeSignature>(7, new TimeSignature(3, 16)),
                    new ValueChange<TimeSignature>(12, new TimeSignature(2, 8)),
                },
                tempoMap.GetTimeSignatureChanges(),
                "Time signature changes are invalid.");
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<Tempo>(5, new Tempo(10)),
                    new ValueChange<Tempo>(10, new Tempo(20)),
                    new ValueChange<Tempo>(15, new Tempo(10)),
                },
                tempoMap.GetTempoChanges(),
                "Tempo changes are invalid.");
            ClassicAssert.AreEqual(midiFile.TimeDivision, tempoMap.TimeDivision, "Time division is invalid.");
        }

        [Test]
        public void GetTempoMap_MultipleTrackChunks_TempoAndTimeSignatureChangesOnSeparateTrackChunks()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new TimeSignatureEvent(2, 8) { DeltaTime = 2 },
                    new TimeSignatureEvent(3, 16) { DeltaTime = 2 },
                    new TimeSignatureEvent(2, 8) { DeltaTime = 2 }),
                new TrackChunk(
                    new SetTempoEvent(10) { DeltaTime = 3 },
                    new SetTempoEvent(20) { DeltaTime = 3 },
                    new SetTempoEvent(10) { DeltaTime = 3 }));

            var tempoMap = midiFile.GetTempoMap();
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<TimeSignature>(2, new TimeSignature(2, 8)),
                    new ValueChange<TimeSignature>(4, new TimeSignature(3, 16)),
                    new ValueChange<TimeSignature>(6, new TimeSignature(2, 8)),
                },
                tempoMap.GetTimeSignatureChanges(),
                "Time signature changes are invalid.");
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<Tempo>(3, new Tempo(10)),
                    new ValueChange<Tempo>(6, new Tempo(20)),
                    new ValueChange<Tempo>(9, new Tempo(10)),
                },
                tempoMap.GetTempoChanges(),
                "Tempo changes are invalid.");
            ClassicAssert.AreEqual(midiFile.TimeDivision, tempoMap.TimeDivision, "Time division is invalid.");
        }

        [Test]
        public void GetTempoMap_MultipleTrackChunks_TempoAndTimeSignatureChangesAreMixed()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new TimeSignatureEvent(2, 8) { DeltaTime = 2 },
                    new SetTempoEvent(10) { DeltaTime = 3 },
                    new TimeSignatureEvent(3, 16) { DeltaTime = 2 }),
                new TrackChunk(
                    new SetTempoEvent(20) { DeltaTime = 3 },
                    new TimeSignatureEvent(2, 8) { DeltaTime = 2 },
                    new SetTempoEvent(10) { DeltaTime = 3 }));

            var tempoMap = midiFile.GetTempoMap();
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<TimeSignature>(2, new TimeSignature(2, 8)),
                    new ValueChange<TimeSignature>(7, new TimeSignature(3, 16)),
                },
                tempoMap.GetTimeSignatureChanges(),
                "Time signature changes are invalid.");
            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<Tempo>(3, new Tempo(20)),
                    new ValueChange<Tempo>(5, new Tempo(10)),
                },
                tempoMap.GetTempoChanges(),
                "Tempo changes are invalid.");
            ClassicAssert.AreEqual(midiFile.TimeDivision, tempoMap.TimeDivision, "Time division is invalid.");
        }

        [Test]
        public void GetTempoMap_1()
        {
            var midiFile = new MidiFile();
            GetTempoMap(
                midiFile: midiFile,
                readingSettings: null,
                writingSettings: null,
                format: MidiFileFormat.MultiTrack,
                expectedTempoMap: TempoMap.Default,
                message: "Invalid tempo map.");
        }

        [Test]
        public void GetTempoMap_2()
        {
            var midiFile = new MidiFile(
                new TrackChunk(),
                new TrackChunk());

            GetTempoMap(
                midiFile: midiFile,
                readingSettings: null,
                writingSettings: null,
                format: MidiFileFormat.MultiTrack,
                expectedTempoMap: TempoMap.Default,
                message: "Invalid tempo map.");
        }

        [Test]
        public void GetTempoMap_3()
        {
            var midiFile = new MidiFile(
                new TrackChunk(new TextEvent("A")),
                new TrackChunk(new NoteOnEvent(), new NoteOffEvent()));

            GetTempoMap(
                midiFile: midiFile,
                readingSettings: null,
                writingSettings: null,
                format: MidiFileFormat.MultiTrack,
                expectedTempoMap: TempoMap.Default,
                message: "Invalid tempo map.");
        }

        [Test]
        public void GetTempoMap_4()
        {
            var trackChunk = new TrackChunk();
            var midiFile = new MidiFile(trackChunk);

            GetTempoMap(
                midiFile: midiFile,
                readingSettings: null,
                writingSettings: null,
                format: MidiFileFormat.MultiTrack,
                expectedTempoMap: TempoMap.Default,
                message: "Invalid initial tempo map.");

            var msPerQuarterNote = 100000;
            var setTempoEvent = new SetTempoEvent(msPerQuarterNote) { DeltaTime = 300 };
            trackChunk.Events.Add(setTempoEvent);

            GetTempoMap(
                midiFile: midiFile,
                readingSettings: null,
                writingSettings: null,
                format: MidiFileFormat.MultiTrack,
                expectedTempoMap: CreateTempoMap(
                    timeDivision: new TicksPerQuarterNoteTimeDivision(),
                    tempoChanges: new (long Time, Tempo Tempo)[]
                    {
                        (300, new Tempo(msPerQuarterNote))
                    },
                    timeSignatureChanges: new (long Time, TimeSignature TimeSignature)[]
                    {
                    }),
                message: "Invalid tempo map after Set Tempo event added.");

            trackChunk.Events.Remove(setTempoEvent);

            GetTempoMap(
                midiFile: midiFile,
                readingSettings: null,
                writingSettings: null,
                format: MidiFileFormat.MultiTrack,
                expectedTempoMap: TempoMap.Default,
                message: "Invalid tempo map after Set Tempo event removed.");
        }

        [Test]
        public void GetTempoMap_5()
        {
            var trackChunk1 = new TrackChunk();
            var trackChunk2 = new TrackChunk();
            var midiFile = new MidiFile(trackChunk1, trackChunk2);

            GetTempoMap(
                midiFile: midiFile,
                readingSettings: null,
                writingSettings: null,
                format: MidiFileFormat.MultiTrack,
                expectedTempoMap: TempoMap.Default,
                message: "Invalid initial tempo map.");

            var msPerQuarterNote = 100000;
            var setTempoEvent = new SetTempoEvent(msPerQuarterNote) { DeltaTime = 300 };
            trackChunk1.Events.Add(setTempoEvent);

            GetTempoMap(
                midiFile: midiFile,
                readingSettings: null,
                writingSettings: null,
                format: MidiFileFormat.MultiTrack,
                expectedTempoMap: CreateTempoMap(
                    timeDivision: new TicksPerQuarterNoteTimeDivision(),
                    tempoChanges: new (long Time, Tempo Tempo)[]
                    {
                        (300, new Tempo(msPerQuarterNote))
                    },
                    timeSignatureChanges: new (long Time, TimeSignature TimeSignature)[]
                    {
                    }),
                message: "Invalid tempo map after Set Tempo event added.");

            var numerator = 2;
            var denominator = 4;
            var timeSignatureEvent = new TimeSignatureEvent((byte)numerator, (byte)denominator) { DeltaTime = 500 };
            trackChunk2.Events.Add(timeSignatureEvent);

            GetTempoMap(
                midiFile: midiFile,
                readingSettings: null,
                writingSettings: null,
                format: MidiFileFormat.SingleTrack,
                expectedTempoMap: CreateTempoMap(
                    timeDivision: new TicksPerQuarterNoteTimeDivision(),
                    tempoChanges: new (long Time, Tempo Tempo)[]
                    {
                        (300, new Tempo(msPerQuarterNote))
                    },
                    timeSignatureChanges: new (long Time, TimeSignature TimeSignature)[]
                    {
                        (500, new TimeSignature((byte)numerator, (byte)denominator))
                    }),
                message: "Invalid tempo map after Time Signature event added.");

            trackChunk2.Events.RemoveAt(0);

            GetTempoMap(
                midiFile: midiFile,
                readingSettings: null,
                writingSettings: null,
                format: MidiFileFormat.MultiTrack,
                expectedTempoMap: CreateTempoMap(
                    timeDivision: new TicksPerQuarterNoteTimeDivision(),
                    tempoChanges: new (long Time, Tempo Tempo)[]
                    {
                        (300, new Tempo(msPerQuarterNote))
                    },
                    timeSignatureChanges: new (long Time, TimeSignature TimeSignature)[]
                    {
                    }),
                message: "Invalid tempo map after Time Signature event removed.");

            trackChunk1.Events.RemoveAll(e => e.EventType == MidiEventType.SetTempo);

            GetTempoMap(
                midiFile: midiFile,
                readingSettings: null,
                writingSettings: null,
                format: MidiFileFormat.MultiTrack,
                expectedTempoMap: TempoMap.Default,
                message: "Invalid tempo map after Set Tempo event removed.");
        }

        [Test]
        public void GetTempoMap_6()
        {
            var trackChunk1 = new TrackChunk();
            var trackChunk2 = new TrackChunk();
            var midiFile = new MidiFile(trackChunk1, trackChunk2);

            GetTempoMap(
                midiFile: midiFile,
                readingSettings: null,
                writingSettings: null,
                format: MidiFileFormat.MultiTrack,
                TempoMap.Default,
                message: "Invalid initial tempo map.");

            var msPerQuarterNote1 = 100000;
            var setTempoEvent1 = new SetTempoEvent(msPerQuarterNote1) { DeltaTime = 300 };
            trackChunk1.Events.Add(setTempoEvent1);

            GetTempoMap(
                midiFile: midiFile,
                readingSettings: null,
                writingSettings: null,
                format: MidiFileFormat.MultiTrack,
                expectedTempoMap: CreateTempoMap(
                    timeDivision: new TicksPerQuarterNoteTimeDivision(),
                    tempoChanges: new (long Time, Tempo Tempo)[]
                    {
                        (300, new Tempo(msPerQuarterNote1))
                    },
                    timeSignatureChanges: new (long Time, TimeSignature TimeSignature)[]
                    {
                    }),
                message: "Invalid tempo map after first Set Tempo event added.");

            var msPerQuarterNote2 = 50000;
            var setTempoEvent2 = new SetTempoEvent(msPerQuarterNote2) { DeltaTime = 200 };
            trackChunk1.Events.Add(setTempoEvent2);

            GetTempoMap(
                midiFile: midiFile,
                readingSettings: null,
                writingSettings: null,
                format: MidiFileFormat.MultiTrack,
                expectedTempoMap: CreateTempoMap(
                    timeDivision: new TicksPerQuarterNoteTimeDivision(),
                    tempoChanges: new (long Time, Tempo Tempo)[]
                    {
                        (300, new Tempo(msPerQuarterNote1)),
                        (500, new Tempo(msPerQuarterNote2)),
                    },
                    timeSignatureChanges: new (long Time, TimeSignature TimeSignature)[]
                    {
                    }),
                message: "Invalid tempo map after second Set Tempo event added.");

            var msPerQuarterNote3 = 10000;
            var setTempoEvent3 = new SetTempoEvent(msPerQuarterNote3) { DeltaTime = 200 };
            trackChunk2.Events.Add(setTempoEvent3);

            GetTempoMap(
                midiFile: midiFile,
                readingSettings: null,
                writingSettings: null,
                format: MidiFileFormat.MultiTrack,
                expectedTempoMap: CreateTempoMap(
                    timeDivision: new TicksPerQuarterNoteTimeDivision(),
                    tempoChanges: new (long Time, Tempo Tempo)[]
                    {
                        (200, new Tempo(msPerQuarterNote3)),
                        (300, new Tempo(msPerQuarterNote1)),
                        (500, new Tempo(msPerQuarterNote2)),
                    },
                    timeSignatureChanges: new (long Time, TimeSignature TimeSignature)[]
                    {
                    }),
                message: "Invalid tempo map after third Set Tempo event added.");
        }

        [Test]
        public void GetTempoMap_AfterTempoEventSetByIndex()
        {
            var trackChunks = new[]
            {
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B"))
            };

            var timeDivision = new TicksPerQuarterNoteTimeDivision(480);
            var tempoMap = trackChunks.GetTempoMap(timeDivision);

            CollectionAssert.IsEmpty(tempoMap.GetTimeSignatureChanges(), "There are time signature changes.");
            CollectionAssert.IsEmpty(tempoMap.GetTempoChanges(), "There are tempo changes.");
            ClassicAssert.AreEqual(timeDivision, tempoMap.TimeDivision, "Time division is invalid.");

            var setTempoEvent = new SetTempoEvent(600);
            trackChunks[0].Events[1] = setTempoEvent;

            tempoMap = trackChunks.GetTempoMap(timeDivision);

            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<Tempo>(0, new Tempo(600)),
                },
                tempoMap.GetTempoChanges(),
                "Invalid tempo changes.");
            CollectionAssert.IsEmpty(tempoMap.GetTimeSignatureChanges(), "There are time signature changes.");
        }

        [Test]
        public void GetTempoMap_AfterTimeSignatureEventSetByIndex()
        {
            var trackChunks = new[]
            {
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B"))
            };

            var timeDivision = new TicksPerQuarterNoteTimeDivision(480);
            var tempoMap = trackChunks.GetTempoMap(timeDivision);

            CollectionAssert.IsEmpty(tempoMap.GetTimeSignatureChanges(), "There are time signature changes.");
            CollectionAssert.IsEmpty(tempoMap.GetTempoChanges(), "There are tempo changes.");
            ClassicAssert.AreEqual(timeDivision, tempoMap.TimeDivision, "Time division is invalid.");

            var timeSignatureEvent = new TimeSignatureEvent(3, 8);
            trackChunks[0].Events[1] = timeSignatureEvent;

            tempoMap = trackChunks.GetTempoMap(timeDivision);

            CollectionAssert.AreEqual(
                new[]
                {
                    new ValueChange<TimeSignature>(0, new TimeSignature(3, 8)),
                },
                tempoMap.GetTimeSignatureChanges(),
                "Invalid time signature changes.");
            CollectionAssert.IsEmpty(tempoMap.GetTempoChanges(), "There are tempo changes.");
        }

        #endregion

        #region Private methods

        private void GetTempoMap_Default(MidiFile midiFile)
        {
            var tempoMap = midiFile.GetTempoMap();

            CollectionAssert.IsEmpty(tempoMap.GetTempoChanges(), "There are tempo changes.");
            ClassicAssert.AreEqual(Tempo.Default, tempoMap.GetTempoAtTime(new MidiTimeSpan()), "Tempo at the start is invalid.");
            ClassicAssert.AreEqual(Tempo.Default, tempoMap.GetTempoAtTime(new MidiTimeSpan(1000)), "Tempo at the middle is invalid.");

            CollectionAssert.IsEmpty(tempoMap.GetTimeSignatureChanges(), "There are time signature changes.");
            ClassicAssert.AreEqual(TimeSignature.Default, tempoMap.GetTimeSignatureAtTime(new MidiTimeSpan()), "Time signature at the start is invalid.");
            ClassicAssert.AreEqual(TimeSignature.Default, tempoMap.GetTimeSignatureAtTime(new MidiTimeSpan(1000)), "Time signature at the middle is invalid.");
        }

        private void GetTempoMap(
            MidiFile midiFile,
            ReadingSettings readingSettings,
            WritingSettings writingSettings,
            MidiFileFormat format,
            TempoMap expectedTempoMap,
            string message)
        {
            var tempoMap = midiFile.GetTempoMap();
            MidiAsserts.AreEqual(expectedTempoMap, tempoMap, $"{message} Invalid tempo map.");

            var readMidiFile = MidiFileTestUtilities.Read(
                midiFile,
                writingSettings,
                readingSettings,
                format);

            var tempoMapAfterReading = readMidiFile.GetTempoMap();
            MidiAsserts.AreEqual(expectedTempoMap, tempoMapAfterReading, $"{message} Invalid tempo map after reading.");
        }

        private static TempoMap CreateTempoMap(
            TimeDivision timeDivision,
            IEnumerable<(long Time, Tempo Tempo)> tempoChanges,
            IEnumerable<(long Time, TimeSignature TimeSignature)> timeSignatureChanges)
        {
            using (var tempoMapManager = new TempoMapManager(timeDivision))
            {
                foreach (var tempoChange in tempoChanges)
                {
                    tempoMapManager.SetTempo(tempoChange.Time, tempoChange.Tempo);
                }

                foreach (var timeSignatureChange in timeSignatureChanges)
                {
                    tempoMapManager.SetTimeSignature(timeSignatureChange.Time, timeSignatureChange.TimeSignature);
                }

                return tempoMapManager.TempoMap;
            }
        }

        #endregion
    }
}
