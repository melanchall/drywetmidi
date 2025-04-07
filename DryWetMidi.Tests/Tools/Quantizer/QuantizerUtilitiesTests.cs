using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed class QuantizerUtilitiesTests
    {
        #region Test methods

        [Test]
        public void QuantizeObjects_SingleCollection_Default_1() => CheckQuantizeObjects_SingleCollection(
            events: new[]
            {
                new TimedEvent(new TextEvent("A"), 8),
                new TimedEvent(new NoteOnEvent(), 32),
                new TimedEvent(new NoteOffEvent(), 40)
            },
            objectType: ObjectType.TimedEvent | ObjectType.Note,
            grid: new SteppedGrid((MidiTimeSpan)15),
            tempoMap: TempoMap.Default,
            settings: null,
            objectDetectionSettings: null,
            expectedEvents: new[]
            {
                new TimedEvent(new TextEvent("A"), 15),
                new TimedEvent(new NoteOnEvent(), 30),
                new TimedEvent(new NoteOffEvent(), 38)
            });

        [Test]
        public void QuantizeObjects_SingleCollection_Default_2() => CheckQuantizeObjects_SingleCollection(
            events: new[]
            {
                new TimedEvent(new TextEvent("A"), 8),
                new TimedEvent(new NoteOnEvent(), 32),
                new TimedEvent(new NoteOffEvent(), 40)
            },
            objectType: ObjectType.Note,
            grid: new SteppedGrid((MidiTimeSpan)15),
            tempoMap: TempoMap.Default,
            settings: null,
            objectDetectionSettings: null,
            expectedEvents: new[]
            {
                new TimedEvent(new TextEvent("A"), 8),
                new TimedEvent(new NoteOnEvent(), 30),
                new TimedEvent(new NoteOffEvent(), 38)
            });

        [Test]
        public void QuantizeObjects_SingleCollection_Default_3() => CheckQuantizeObjects_SingleCollection(
            events: new[]
            {
                new TimedEvent(new TextEvent("A"), 8),
                new TimedEvent(new NoteOnEvent(), 32),
                new TimedEvent(new NoteOffEvent(), 40)
            },
            objectType: ObjectType.TimedEvent,
            grid: new SteppedGrid((MidiTimeSpan)15),
            tempoMap: TempoMap.Default,
            settings: null,
            objectDetectionSettings: null,
            expectedEvents: new[]
            {
                new TimedEvent(new TextEvent("A"), 15),
                new TimedEvent(new NoteOnEvent(), 30),
                new TimedEvent(new NoteOffEvent(), 45)
            });

        [Test]
        public void QuantizeObjects_SingleCollection_Settings_1() => CheckQuantizeObjects_SingleCollection(
            events: new[]
            {
                new TimedEvent(new TextEvent("A"), 8),
                new TimedEvent(new NoteOnEvent(), 32),
                new TimedEvent(new NoteOffEvent(), 40)
            },
            objectType: ObjectType.TimedEvent,
            grid: new SteppedGrid((MidiTimeSpan)15),
            tempoMap: TempoMap.Default,
            settings: new QuantizingSettings
            {
                Filter = obj => obj is TimedEvent timedEvent && !(timedEvent.Event is NoteEvent)
            },
            objectDetectionSettings: null,
            expectedEvents: new[]
            {
                new TimedEvent(new TextEvent("A"), 15),
                new TimedEvent(new NoteOnEvent(), 32),
                new TimedEvent(new NoteOffEvent(), 40)
            });

        [Test]
        public void QuantizeObjects_SingleCollection_Settings_2() => CheckQuantizeObjects_SingleCollection(
            events: new[]
            {
                new TimedEvent(new TextEvent("A"), 8),
                new TimedEvent(new NoteOnEvent(), 11),
                new TimedEvent(new NoteOffEvent(), 14),
                new TimedEvent(new NoteOnEvent(), 32),
                new TimedEvent(new TextEvent("A"), 35),
                new TimedEvent(new NoteOffEvent(), 40),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue), 34),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue), 45),
            },
            objectType: ObjectType.Note | ObjectType.Chord,
            grid: new SteppedGrid((MidiTimeSpan)15),
            tempoMap: TempoMap.Default,
            settings: null,
            objectDetectionSettings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesMinCount = 2,
                    NotesTolerance = 10
                }
            },
            expectedEvents: new[]
            {
                new TimedEvent(new TextEvent("A"), 8),
                new TimedEvent(new NoteOnEvent(), 15),
                new TimedEvent(new NoteOffEvent(), 18),
                new TimedEvent(new NoteOnEvent(), 30),
                new TimedEvent(new TextEvent("A"), 35),
                new TimedEvent(new NoteOffEvent(), 38),
                new TimedEvent(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue), 32),
                new TimedEvent(new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue), 43),
            });

        [Test]
        public void QuantizeObjects_MultipleCollections_Default_1() => CheckQuantizeObjects_MultipleCollections(
            events: new[]
            {
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 8),
                    new TimedEvent(new NoteOnEvent(), 32),
                    new TimedEvent(new NoteOffEvent(), 40)
                },
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 5),
                    new TimedEvent(new NoteOffEvent(), 10),
                    new TimedEvent(new NoteOffEvent(), 20),
                }
            },
            objectType: ObjectType.TimedEvent | ObjectType.Note,
            grid: new SteppedGrid((MidiTimeSpan)15),
            tempoMap: TempoMap.Default,
            settings: null,
            objectDetectionSettings: null,
            expectedEvents: new[]
            {
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 15),
                    new TimedEvent(new NoteOnEvent(), 30),
                    new TimedEvent(new NoteOffEvent(), 38)
                },
                new[]
                {
                    new TimedEvent(new NoteOnEvent(), 0),
                    new TimedEvent(new NoteOffEvent(), 5),
                    new TimedEvent(new NoteOffEvent(), 15),
                }
            });

        [Test]
        public void QuantizeObjects_MultipleCollections_Default_2() => CheckQuantizeObjects_MultipleCollections(
            events: new[]
            {
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 8),
                    new TimedEvent(new NoteOnEvent(), 32),
                    new TimedEvent(new NoteOffEvent(), 40)
                },
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 8),
                    new TimedEvent(new NoteOnEvent(), 32),
                    new TimedEvent(new NoteOffEvent(), 40),
                    new TimedEvent(new NoteOnEvent(), 32),
                }
            },
            objectType: ObjectType.Note,
            grid: new SteppedGrid((MidiTimeSpan)15),
            tempoMap: TempoMap.Default,
            settings: null,
            objectDetectionSettings: null,
            expectedEvents: new[]
            {
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 8),
                    new TimedEvent(new NoteOnEvent(), 30),
                    new TimedEvent(new NoteOffEvent(), 38)
                },
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 8),
                    new TimedEvent(new NoteOnEvent(), 30),
                    new TimedEvent(new NoteOffEvent(), 38),
                    new TimedEvent(new NoteOnEvent(), 32),
                }
            });

        [Test]
        public void QuantizeObjects_MultipleCollections_Default_3() => CheckQuantizeObjects_MultipleCollections(
            events: new[]
            {
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 8),
                    new TimedEvent(new NoteOnEvent(), 32),
                    new TimedEvent(new NoteOffEvent(), 40)
                },
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 15),
                    new TimedEvent(new NoteOnEvent(), 42),
                    new TimedEvent(new NoteOffEvent(), 50)
                }
            },
            objectType: ObjectType.TimedEvent,
            grid: new SteppedGrid((MidiTimeSpan)15),
            tempoMap: TempoMap.Default,
            settings: null,
            objectDetectionSettings: null,
            expectedEvents: new[]
            {
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 15),
                    new TimedEvent(new NoteOnEvent(), 30),
                    new TimedEvent(new NoteOffEvent(), 45)
                },
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 15),
                    new TimedEvent(new NoteOnEvent(), 45),
                    new TimedEvent(new NoteOffEvent(), 45)
                }
            });

        [Test]
        public void QuantizeObjects_MultipleCollections_Settings_1() => CheckQuantizeObjects_MultipleCollections(
            events: new[]
            {
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 8),
                    new TimedEvent(new NoteOnEvent(), 32),
                    new TimedEvent(new NoteOffEvent(), 40)
                },
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 8),
                    new TimedEvent(new NoteOnEvent(), 32),
                    new TimedEvent(new NoteOffEvent(), 40),
                    new TimedEvent(new ControlChangeEvent(), 48),
                }
            },
            objectType: ObjectType.TimedEvent,
            grid: new SteppedGrid((MidiTimeSpan)15),
            tempoMap: TempoMap.Default,
            settings: new QuantizingSettings
            {
                Filter = obj => obj is TimedEvent timedEvent && !(timedEvent.Event is NoteEvent)
            },
            objectDetectionSettings: null,
            expectedEvents: new[]
            {
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 15),
                    new TimedEvent(new NoteOnEvent(), 32),
                    new TimedEvent(new NoteOffEvent(), 40)
                },
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 15),
                    new TimedEvent(new NoteOnEvent(), 32),
                    new TimedEvent(new NoteOffEvent(), 40),
                    new TimedEvent(new ControlChangeEvent(), 45),
                }
            });

        [Test]
        public void QuantizeObjects_MultipleCollections_Settings_2() => CheckQuantizeObjects_MultipleCollections(
            events: new[]
            {
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 8),
                    new TimedEvent(new NoteOnEvent(), 11),
                    new TimedEvent(new NoteOffEvent(), 14),
                    new TimedEvent(new NoteOnEvent(), 32),
                    new TimedEvent(new TextEvent("A"), 35),
                    new TimedEvent(new NoteOffEvent(), 40),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue), 34),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue), 45),
                },
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 8),
                    new TimedEvent(new NoteOnEvent(), 11),
                    new TimedEvent(new NoteOffEvent(), 14),
                    new TimedEvent(new NoteOnEvent(), 32),
                    new TimedEvent(new TextEvent("A"), 35),
                    new TimedEvent(new NoteOffEvent(), 40),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue), 34),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue), 45),
                    new TimedEvent(new NoteOnEvent(), 62),
                    new TimedEvent(new NoteOffEvent(), 70),
                }
            },
            objectType: ObjectType.Chord,
            grid: new SteppedGrid((MidiTimeSpan)15),
            tempoMap: TempoMap.Default,
            settings: null,
            objectDetectionSettings: new ObjectDetectionSettings
            {
                ChordDetectionSettings = new ChordDetectionSettings
                {
                    NotesMinCount = 2,
                    NotesTolerance = 10
                }
            },
            expectedEvents: new[]
            {
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 8),
                    new TimedEvent(new NoteOnEvent(), 11),
                    new TimedEvent(new NoteOffEvent(), 14),
                    new TimedEvent(new NoteOnEvent(), 30),
                    new TimedEvent(new TextEvent("A"), 35),
                    new TimedEvent(new NoteOffEvent(), 38),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue), 32),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue), 43),
                },
                new[]
                {
                    new TimedEvent(new TextEvent("A"), 8),
                    new TimedEvent(new NoteOnEvent(), 11),
                    new TimedEvent(new NoteOffEvent(), 14),
                    new TimedEvent(new NoteOnEvent(), 30),
                    new TimedEvent(new TextEvent("A"), 35),
                    new TimedEvent(new NoteOffEvent(), 38),
                    new TimedEvent(new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue), 32),
                    new TimedEvent(new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue), 43),
                    new TimedEvent(new NoteOnEvent(), 62),
                    new TimedEvent(new NoteOffEvent(), 70),
                }
            });

        #endregion

        #region Private methods

        private void CheckQuantizeObjects_SingleCollection(
            ICollection<TimedEvent> events,
            ObjectType objectType,
            IGrid grid,
            TempoMap tempoMap,
            QuantizingSettings settings,
            ObjectDetectionSettings objectDetectionSettings,
            ICollection<TimedEvent> expectedEvents)
        {
            var trackChunk = events.ToTrackChunk();
            trackChunk.QuantizeObjects(objectType, grid, tempoMap, settings, objectDetectionSettings);
            MidiAsserts.AreEqual(expectedEvents.ToTrackChunk(), trackChunk, true, "Invalid quantized objects in track chunk.");

            //

            var trackChunks = new[] { events.ToTrackChunk() };
            trackChunks.QuantizeObjects(objectType, grid, tempoMap, settings, objectDetectionSettings);
            MidiAsserts.AreEqual(new[] { expectedEvents.ToTrackChunk() }, trackChunks, true, "Invalid quantized objects in track chunks.");

            //

            var midiFile = events.ToFile();
            midiFile.QuantizeObjects(objectType, grid, settings, objectDetectionSettings);
            MidiAsserts.AreEqual(expectedEvents.ToFile(), midiFile, true, "Invalid quantized objects in file.");
        }

        private void CheckQuantizeObjects_MultipleCollections(
            ICollection<ICollection<TimedEvent>> events,
            ObjectType objectType,
            IGrid grid,
            TempoMap tempoMap,
            QuantizingSettings settings,
            ObjectDetectionSettings objectDetectionSettings,
            ICollection<ICollection<TimedEvent>> expectedEvents)
        {
            ClassicAssert.Greater(events.Count, 1, "Invalid events collections count.");

            //

            var trackChunks = events.Select(e => e.ToTrackChunk()).ToArray();
            trackChunks.QuantizeObjects(objectType, grid, tempoMap, settings, objectDetectionSettings);
            MidiAsserts.AreEqual(expectedEvents.Select(e => e.ToTrackChunk()).ToArray(), trackChunks, true, "Invalid quantized objects in track chunks.");

            //

            var midiFile = new MidiFile(events.Select(e => e.ToTrackChunk()));
            midiFile.QuantizeObjects(objectType, grid, settings, objectDetectionSettings);
            MidiAsserts.AreEqual(new MidiFile(expectedEvents.Select(e => e.ToTrackChunk())), midiFile, true, "Invalid quantized objects in file.");
        }

        #endregion
    }
}
