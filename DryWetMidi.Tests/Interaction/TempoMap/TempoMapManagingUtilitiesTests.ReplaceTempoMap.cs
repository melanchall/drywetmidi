using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TempoMapManagingUtilitiesTests
    {
        #region Nested classes

        private sealed class CustomChunk : MidiChunk
        {
            public CustomChunk()
                : base("Cstm")
            {
            }

            public override MidiChunk Clone()
            {
                throw new NotImplementedException();
            }

            protected override uint GetContentSize(WritingSettings settings)
            {
                throw new NotImplementedException();
            }

            protected override void ReadContent(MidiReader reader, ReadingSettings settings, uint size)
            {
                throw new NotImplementedException();
            }

            protected override void WriteContent(MidiWriter writer, WritingSettings settings)
            {
                throw new NotImplementedException();
            }

            public override bool Equals(object obj)
            {
                return obj is CustomChunk;
            }
        }

        #endregion

        #region Test methods

        [Test]
        public void ReplaceTempoMap_EmptyFile()
        {
            var midiFile = new MidiFile();
            midiFile.ReplaceTempoMap(TempoMap.Default);
            MidiAsserts.AreEqual(new MidiFile(), midiFile, false, "Invalid file.");
        }

        [Test]
        public void ReplaceTempoMap_EventsCollections_Empty() => ReplaceTempoMap(
            events: Array.Empty<ICollection<MidiEvent>>(),
            tempoMap: GetNewTempoMap(null, null),
            expectedEvents: Array.Empty<ICollection<MidiEvent>>());

        [Test]
        public void ReplaceTempoMap_EventsCollections_SingleCollection_Empty_DefaultTempoMap() => ReplaceTempoMap(
            events: new[]
            {
                Array.Empty<MidiEvent>(),
            },
            tempoMap: TempoMap.Default,
            expectedEvents: new[]
            {
                Array.Empty<MidiEvent>(),
            });

        [Test]
        public void ReplaceTempoMap_EventsCollections_SingleCollection_Empty_CustomTempoMap() => ReplaceTempoMap(
            events: new[]
            {
                Array.Empty<MidiEvent>(),
            },
            tempoMap: GetNewTempoMap(
                new[] { (50L, new Tempo(100000)) },
                new[] { (70L, new TimeSignature(3, 8)) }),
            expectedEvents: new[]
            {
                new MidiEvent[]
                {
                    new SetTempoEvent(100000) { DeltaTime = 50 },
                    new TimeSignatureEvent(3, 8) { DeltaTime = 20 },
                }
            });

        [Test]
        public void ReplaceTempoMap_EventsCollections_SingleCollection_NoTempoMapEvents_DefaultTempoMap() => ReplaceTempoMap(
            events: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 100 },
                }
            },
            tempoMap: TempoMap.Default,
            expectedEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 100 },
                }
            });

        [Test]
        public void ReplaceTempoMap_EventsCollections_SingleCollection_NoTempoMapEvents_CustomTempoMap() => ReplaceTempoMap(
            events: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 100 },
                }
            },
            tempoMap: GetNewTempoMap(
                new[] { (50L, new Tempo(100000)) },
                new[] { (70L, new TimeSignature(3, 8)) }),
            expectedEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new SetTempoEvent(100000) { DeltaTime = 40 },
                    new TimeSignatureEvent(3, 8) { DeltaTime = 20 },
                    new NoteOffEvent { DeltaTime = 40 },
                }
            });

        [Test]
        public void ReplaceTempoMap_EventsCollections_SingleCollection_TempoMapEvents_DefaultTempoMap() => ReplaceTempoMap(
            events: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new SetTempoEvent(100000) { DeltaTime = 30 },
                    new NoteOffEvent { DeltaTime = 100 },
                    new TimeSignatureEvent(5, 16) { DeltaTime = 1000 }
                }
            },
            tempoMap: TempoMap.Default,
            expectedEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 130 },
                }
            });

        [Test]
        public void ReplaceTempoMap_EventsCollections_SingleCollection_TempoMapEvents_CustomTempoMap_New() => ReplaceTempoMap(
            events: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new SetTempoEvent(100000) { DeltaTime = 30 },
                    new NoteOffEvent { DeltaTime = 100 },
                    new TimeSignatureEvent(5, 16) { DeltaTime = 1000 }
                }
            },
            tempoMap: GetNewTempoMap(
                new[] { (50L, new Tempo(200000)) },
                null),
            expectedEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new SetTempoEvent(200000) { DeltaTime = 40 },
                    new NoteOffEvent { DeltaTime = 90 },
                }
            });

        [Test]
        public void ReplaceTempoMap_EventsCollections_SingleCollection_TempoMapEvents_CustomTempoMap_Replace() => ReplaceTempoMap(
            events: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new SetTempoEvent(100000) { DeltaTime = 30 },
                    new NoteOffEvent { DeltaTime = 100 },
                    new TimeSignatureEvent(5, 16) { DeltaTime = 1000 }
                }
            },
            tempoMap: GetNewTempoMap(
                new[] { (40L, new Tempo(300000)), (50L, new Tempo(200000)) },
                new[] { (1140L, new TimeSignature(3, 4)) }),
            expectedEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new SetTempoEvent(300000) { DeltaTime = 30 },
                    new SetTempoEvent(200000) { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 90 },
                    new TimeSignatureEvent(3, 4) { DeltaTime = 1000 }
                }
            });

        [Test]
        public void ReplaceTempoMap_EventsCollections_MultipleCollections_Empty_DefaultTempoMap() => ReplaceTempoMap(
            events: new[]
            {
                Array.Empty<MidiEvent>(),
                Array.Empty<MidiEvent>(),
            },
            tempoMap: TempoMap.Default,
            expectedEvents: new[]
            {
                Array.Empty<MidiEvent>(),
                Array.Empty<MidiEvent>(),
            });

        [Test]
        public void ReplaceTempoMap_EventsCollections_MultipleCollections_Empty_CustomTempoMap() => ReplaceTempoMap(
            events: new[]
            {
                Array.Empty<MidiEvent>(),
                Array.Empty<MidiEvent>(),
            },
            tempoMap: GetNewTempoMap(
                new[] { (50L, new Tempo(100000)) },
                new[] { (70L, new TimeSignature(3, 8)) }),
            expectedEvents: new[]
            {
                new MidiEvent[]
                {
                    new SetTempoEvent(100000) { DeltaTime = 50 },
                    new TimeSignatureEvent(3, 8) { DeltaTime = 20 },
                },
                Array.Empty<MidiEvent>(),
            });

        [Test]
        public void ReplaceTempoMap_EventsCollections_MultipleCollections_NoTempoMapEvents_DefaultTempoMap() => ReplaceTempoMap(
            events: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                }
            },
            tempoMap: TempoMap.Default,
            expectedEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                }
            });

        [Test]
        public void ReplaceTempoMap_EventsCollections_MultipleCollections_NoTempoMapEvents_CustomTempoMap() => ReplaceTempoMap(
            events: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                }
            },
            tempoMap: GetNewTempoMap(
                new[] { (50L, new Tempo(100000)) },
                new[] { (70L, new TimeSignature(3, 8)) }),
            expectedEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new SetTempoEvent(100000) { DeltaTime = 40 },
                    new TimeSignatureEvent(3, 8) { DeltaTime = 20 },
                    new NoteOffEvent { DeltaTime = 40 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                }
            });

        [Test]
        public void ReplaceTempoMap_EventsCollections_MultipleCollections_TempoMapEvents_DefaultTempoMap() => ReplaceTempoMap(
            events: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new SetTempoEvent(100000) { DeltaTime = 30 },
                    new NoteOffEvent { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TimeSignatureEvent(3, 8) { DeltaTime = 30 },
                }
            },
            tempoMap: TempoMap.Default,
            expectedEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 130 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                }
            });

        [Test]
        public void ReplaceTempoMap_EventsCollections_MultipleCollections_TempoMapEvents_CustomTempoMap_New() => ReplaceTempoMap(
            events: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new SetTempoEvent(100000) { DeltaTime = 30 },
                    new NoteOffEvent { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TimeSignatureEvent(3, 8) { DeltaTime = 30 },
                }
            },
            tempoMap: GetNewTempoMap(
                new[] { (50L, new Tempo(200000)) },
                new[] { (20L, new TimeSignature(7, 32)) }),
            expectedEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new TimeSignatureEvent(7, 32) { DeltaTime = 10 },
                    new SetTempoEvent(200000) { DeltaTime = 30 },
                    new NoteOffEvent { DeltaTime = 90 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                }
            });

        [Test]
        public void ReplaceTempoMap_EventsCollections_MultipleCollections_TempoMapEvents_CustomTempoMap_Replace() => ReplaceTempoMap(
            events: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new SetTempoEvent(100000) { DeltaTime = 40 },
                    new NoteOffEvent { DeltaTime = 90 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TimeSignatureEvent(3, 8) { DeltaTime = 30 },
                }
            },
            tempoMap: GetNewTempoMap(
                new[] { (50L, new Tempo(200000)) },
                new[] { (30L, new TimeSignature(7, 32)) }),
            expectedEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new TimeSignatureEvent(7, 32) { DeltaTime = 20 },
                    new SetTempoEvent(200000) { DeltaTime = 20 },
                    new NoteOffEvent { DeltaTime = 90 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                }
            });

        [Test]
        public void ReplaceTempoMap_File_Empty_DefaultTempoMap() => ReplaceTempoMap(
            midiFile: new MidiFile(),
            tempoMap: TempoMap.Default,
            expectedFile: new MidiFile());

        [Test]
        public void ReplaceTempoMap_File_CustomChunk_DefaultTempoMap() => ReplaceTempoMap(
            midiFile: new MidiFile(new CustomChunk()),
            tempoMap: TempoMap.Default,
            expectedFile: new MidiFile(new CustomChunk()));

        [Test]
        public void ReplaceTempoMap_File_Empty_CustomTempoMap() => ReplaceTempoMap(
            midiFile: new MidiFile(),
            tempoMap: GetNewTempoMap(
                new[] { (20L, new Tempo(100000)) },
                null,
                500),
            expectedFile: new MidiFile(
                new TrackChunk(
                    new SetTempoEvent(100000) { DeltaTime = 20 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(500)
            });

        [Test]
        public void ReplaceTempoMap_File_CustomChunk_CustomTempoMap() => ReplaceTempoMap(
            midiFile: new MidiFile(new CustomChunk()),
            tempoMap: GetNewTempoMap(
                new[] { (20L, new Tempo(100000)) },
                null,
                500),
            expectedFile: new MidiFile(
                new CustomChunk(),
                new TrackChunk(
                    new SetTempoEvent(100000) { DeltaTime = 20 }))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(500)
            });

        #endregion

        #region Private methods

        private TempoMap GetNewTempoMap(
            (long Time, Tempo Tempo)[] tempoChanges,
            (long Time, TimeSignature TimeSignature)[] timeSignatureChanges,
            short ticksPerQuarterNote = TicksPerQuarterNoteTimeDivision.DefaultTicksPerQuarterNote)
        {
            using (var tempoMapManager = new TempoMapManager(new TicksPerQuarterNoteTimeDivision(ticksPerQuarterNote)))
            {
                foreach (var tempoChange in tempoChanges ?? Array.Empty<(long, Tempo)>())
                {
                    tempoMapManager.SetTempo(tempoChange.Time, tempoChange.Tempo);
                }

                foreach (var timeSignatureChange in timeSignatureChanges ?? Array.Empty<(long, TimeSignature)>())
                {
                    tempoMapManager.SetTimeSignature(timeSignatureChange.Time, timeSignatureChange.TimeSignature);
                }

                return tempoMapManager.TempoMap;
            }
        }

        private void ReplaceTempoMap(
            ICollection<ICollection<MidiEvent>> events,
            TempoMap tempoMap,
            ICollection<ICollection<MidiEvent>> expectedEvents)
        {
            var expectedTrackChunks = expectedEvents
                .Select(e => new TrackChunk(e))
                .ToArray();

            //

            var eventsCollections = events
                .Select(e =>
                {
                    var result = new EventsCollection();
                    result.AddRange(e.Select(ee => ee.Clone()));
                    return result;
                })
                .ToArray();
            eventsCollections.ReplaceTempoMap(tempoMap);
            MidiAsserts.AreEqual(
                expectedTrackChunks,
                eventsCollections.Select(e => new TrackChunk(e)),
                true,
                "Invalid events collections.");

            //

            var trackChunks = events
                .Select(e => new TrackChunk(e.Select(ee => ee.Clone())))
                .ToArray();
            trackChunks.ReplaceTempoMap(tempoMap);
            MidiAsserts.AreEqual(
                expectedTrackChunks,
                trackChunks,
                true,
                "Invalid track chunks.");

            //

            var midiFile = new MidiFile(events
                .Select(e => new TrackChunk(e.Select(ee => ee.Clone()))));
            midiFile.ReplaceTempoMap(tempoMap);
            MidiAsserts.AreEqual(
                new MidiFile(expectedTrackChunks),
                midiFile,
                false,
                "Invalid file.");
        }

        private void ReplaceTempoMap(
            MidiFile midiFile,
            TempoMap tempoMap,
            MidiFile expectedFile)
        {
            midiFile.ReplaceTempoMap(tempoMap);
            MidiAsserts.AreEqual(expectedFile, midiFile, false, "Invalid file.");
        }

        #endregion
    }
}
