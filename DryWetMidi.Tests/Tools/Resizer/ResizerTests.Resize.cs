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
    public sealed partial class ResizerTests
    {
        #region Test methods

        [Test]
        public void Resize_ToLength_EventsCollection_Empty() => Resize_ToLength_EventsCollection(
            midiEvents: Array.Empty<MidiEvent>(),
            length: (MidiTimeSpan)100,
            tempoMap: TempoMap.Default,
            expectedEvents: Array.Empty<MidiEvent>());

        [Test]
        public void Resize_ToLength_EventsCollection_OneEvent_Midi([Values(0, 50, 100)] long deltaTime, [Values(10, 200)] int toTicks) => Resize_ToLength_EventsCollection(
            midiEvents: new[]
            {
                new TextEvent("A") { DeltaTime = deltaTime }
            },
            length: (MidiTimeSpan)toTicks,
            tempoMap: TempoMap.Default,
            expectedEvents: new[]
            {
                new TextEvent("A") { DeltaTime = deltaTime == 0 ? 0 : toTicks }
            });

        [Test]
        public void Resize_ToLength_EventsCollection_OneEvent_Metric([Values(0, 50, 100)] int seconds, [Values(10, 200)] int toSeconds) => Resize_ToLength_EventsCollection(
            midiEvents: new[]
            {
                new TextEvent("A") { DeltaTime = TimeConverter.ConvertFrom(new MetricTimeSpan(0, 0, seconds), TempoMap.Default) }
            },
            length: new MetricTimeSpan(0, 0, toSeconds),
            tempoMap: TempoMap.Default,
            expectedEvents: new[]
            {
                new TextEvent("A") { DeltaTime = seconds == 0 ? 0 : TimeConverter.ConvertFrom(new MetricTimeSpan(0, 0, toSeconds), TempoMap.Default) }
            });

        [Test]
        public void Resize_ToLength_EventsCollection_MultipleEvents_1() => Resize_ToLength_EventsCollection(
            midiEvents: new[]
            {
                new TextEvent("A") { DeltaTime = 0 },
                new TextEvent("B") { DeltaTime = 0 },
            },
            length: (MidiTimeSpan)100,
            tempoMap: TempoMap.Default,
            expectedEvents: new[]
            {
                new TextEvent("A") { DeltaTime = 0 },
                new TextEvent("B") { DeltaTime = 0 },
            });

        [Test]
        public void Resize_ToLength_EventsCollection_MultipleEvents_2() => Resize_ToLength_EventsCollection(
            midiEvents: new[]
            {
                new TextEvent("A") { DeltaTime = 50 },
                new TextEvent("B") { DeltaTime = 100 },
            },
            length: (MidiTimeSpan)1500,
            tempoMap: TempoMap.Default,
            expectedEvents: new[]
            {
                new TextEvent("A") { DeltaTime = 500 },
                new TextEvent("B") { DeltaTime = 1000 },
            });

        [Test]
        public void Resize_ToLength_EventsCollection_MultipleEvents_3() => Resize_ToLength_EventsCollection(
            midiEvents: new[]
            {
                new TextEvent("A") { DeltaTime = 50 },
                new TextEvent("B") { DeltaTime = 100 },
            },
            length: (MidiTimeSpan)15,
            tempoMap: TempoMap.Default,
            expectedEvents: new[]
            {
                new TextEvent("A") { DeltaTime = 5 },
                new TextEvent("B") { DeltaTime = 10 },
            });

        [Test]
        public void Resize_ToLength_EventsCollection_MultipleEvents_4() => Resize_ToLength_EventsCollection(
            midiEvents: new[]
            {
                new TextEvent("A") { DeltaTime = 0 },
                new TextEvent("B") { DeltaTime = 100 },
            },
            length: (MidiTimeSpan)10,
            tempoMap: TempoMap.Default,
            expectedEvents: new[]
            {
                new TextEvent("A") { DeltaTime = 0 },
                new TextEvent("B") { DeltaTime = 10 },
            });

        [Test]
        public void Resize_ToLength_EventsCollection_MultipleEvents_5() => Resize_ToLength_EventsCollection(
            midiEvents: new[]
            {
                new TextEvent("A") { DeltaTime = 50 },
                new TextEvent("B") { DeltaTime = 0 },
            },
            length: (MidiTimeSpan)1000,
            tempoMap: TempoMap.Default,
            expectedEvents: new[]
            {
                new TextEvent("A") { DeltaTime = 1000 },
                new TextEvent("B") { DeltaTime = 0 },
            });

        [Test]
        public void Resize_ByRatio_EventsCollection_Empty() => Resize_ByRatio_EventsCollection(
            midiEvents: Array.Empty<MidiEvent>(),
            ratio: 2.0,
            expectedEvents: Array.Empty<MidiEvent>());

        [Test]
        public void Resize_ByRatio_EventsCollection_OneEvent_Midi([Values(0, 50, 100)] long deltaTime, [Values(0.5, 2.0)] double ratio) => Resize_ByRatio_EventsCollection(
            midiEvents: new[]
            {
                new TextEvent("A") { DeltaTime = deltaTime }
            },
            ratio: ratio,
            expectedEvents: new[]
            {
                new TextEvent("A") { DeltaTime = (long)Math.Round(deltaTime * ratio) }
            });

        [Test]
        public void Resize_ByRatio_EventsCollection_OneEvent_Metric([Values(0, 50, 100)] int seconds, [Values(0.5, 2.0)] double ratio) => Resize_ByRatio_EventsCollection(
            midiEvents: new[]
            {
                new TextEvent("A") { DeltaTime = TimeConverter.ConvertFrom(new MetricTimeSpan(0, 0, seconds), TempoMap.Default) }
            },
            ratio: ratio,
            expectedEvents: new[]
            {
                new TextEvent("A") { DeltaTime = TimeConverter.ConvertFrom(new MetricTimeSpan(0, 0, (int)Math.Round(seconds * ratio)), TempoMap.Default) }
            });

        [Test]
        public void Resize_ByRatio_EventsCollection_MultipleEvents_1() => Resize_ByRatio_EventsCollection(
            midiEvents: new[]
            {
                new TextEvent("A") { DeltaTime = 0 },
                new TextEvent("B") { DeltaTime = 0 },
            },
            ratio: 2.0,
            expectedEvents: new[]
            {
                new TextEvent("A") { DeltaTime = 0 },
                new TextEvent("B") { DeltaTime = 0 },
            });

        [Test]
        public void Resize_ByRatio_EventsCollection_MultipleEvents_2() => Resize_ByRatio_EventsCollection(
            midiEvents: new[]
            {
                new TextEvent("A") { DeltaTime = 50 },
                new TextEvent("B") { DeltaTime = 100 },
            },
            ratio: 10.0,
            expectedEvents: new[]
            {
                new TextEvent("A") { DeltaTime = 500 },
                new TextEvent("B") { DeltaTime = 1000 },
            });

        [Test]
        public void Resize_ByRatio_EventsCollection_MultipleEvents_3() => Resize_ByRatio_EventsCollection(
            midiEvents: new[]
            {
                new TextEvent("A") { DeltaTime = 50 },
                new TextEvent("B") { DeltaTime = 100 },
            },
            ratio: 0.1,
            expectedEvents: new[]
            {
                new TextEvent("A") { DeltaTime = 5 },
                new TextEvent("B") { DeltaTime = 10 },
            });

        [Test]
        public void Resize_ByRatio_EventsCollection_MultipleEvents_4() => Resize_ByRatio_EventsCollection(
            midiEvents: new[]
            {
                new TextEvent("A") { DeltaTime = 0 },
                new TextEvent("B") { DeltaTime = 100 },
            },
            ratio: 0.1,
            expectedEvents: new[]
            {
                new TextEvent("A") { DeltaTime = 0 },
                new TextEvent("B") { DeltaTime = 10 },
            });

        [Test]
        public void Resize_ByRatio_EventsCollection_MultipleEvents_5() => Resize_ByRatio_EventsCollection(
            midiEvents: new[]
            {
                new TextEvent("A") { DeltaTime = 50 },
                new TextEvent("B") { DeltaTime = 0 },
            },
            ratio: 20.0,
            expectedEvents: new[]
            {
                new TextEvent("A") { DeltaTime = 1000 },
                new TextEvent("B") { DeltaTime = 0 },
            });

        [Test]
        public void Resize_ToLength_EventsCollections_EmptyCollection() => Resize_ToLength_EventsCollections(
            midiEvents: new[] { Array.Empty<MidiEvent>() },
            length: (MidiTimeSpan)100,
            tempoMap: TempoMap.Default,
            expectedEvents: new[] { Array.Empty<MidiEvent>() });

        [Test]
        public void Resize_ToLength_EventsCollections_EmptyChunks() => Resize_ToLength_EventsCollections(
            midiEvents: new[]
            {
                Array.Empty<MidiEvent>(),
                Array.Empty<MidiEvent>()
            },
            length: (MidiTimeSpan)100,
            tempoMap: TempoMap.Default,
            expectedEvents: new[]
            {
                Array.Empty<MidiEvent>(),
                Array.Empty<MidiEvent>()
            });

        [Test]
        public void Resize_ToLength_EventsCollections_OneEvent_1([Values(0, 50, 100)] long deltaTime, [Values(10, 200)] int toTicks) => Resize_ToLength_EventsCollections(
            midiEvents: new[]
            {
                new[]
                {
                    new TextEvent("A") { DeltaTime = deltaTime }
                },
                Array.Empty<MidiEvent>()
            },
            length: (MidiTimeSpan)toTicks,
            tempoMap: TempoMap.Default,
            expectedEvents: new[]
            {
                new[]
                {
                    new TextEvent("A") { DeltaTime = deltaTime == 0 ? 0 : toTicks }
                },
                Array.Empty<MidiEvent>()
            });

        [Test]
        public void Resize_ToLength_EventsCollections_OneEvent_2([Values(0, 50, 100)] long deltaTime, [Values(10, 200)] int toTicks) => Resize_ToLength_EventsCollections(
            midiEvents: new[]
            {
                Array.Empty<MidiEvent>(),
                new[]
                {
                    new TextEvent("A") { DeltaTime = deltaTime }
                },
            },
            length: (MidiTimeSpan)toTicks,
            tempoMap: TempoMap.Default,
            expectedEvents: new[]
            {
                Array.Empty<MidiEvent>(),
                new[]
                {
                    new TextEvent("A") { DeltaTime = deltaTime == 0 ? 0 : toTicks }
                },
            });

        [Test]
        public void Resize_ByRatio_EventsCollections_OneEvent_1([Values(0, 50, 100)] long deltaTime, [Values(0.5, 2.0)] double ratio) => Resize_ByRatio_EventsCollections(
            midiEvents: new[]
            {
                new[]
                {
                    new TextEvent("A") { DeltaTime = deltaTime }
                },
                Array.Empty<MidiEvent>(),
            },
            ratio: ratio,
            expectedEvents: new[]
            {
                new[]
                {
                    new TextEvent("A") { DeltaTime = (long)Math.Round(deltaTime * ratio) }
                },
                Array.Empty<MidiEvent>(),
            });

        [Test]
        public void Resize_ByRatio_EventsCollections_OneEvent_2([Values(0, 50, 100)] long deltaTime, [Values(0.5, 2.0)] double ratio) => Resize_ByRatio_EventsCollections(
            midiEvents: new[]
            {
                Array.Empty<MidiEvent>(),
                new[]
                {
                    new TextEvent("A") { DeltaTime = deltaTime }
                },
            },
            ratio: ratio,
            expectedEvents: new[]
            {
                Array.Empty<MidiEvent>(),
                new[]
                {
                    new TextEvent("A") { DeltaTime = (long)Math.Round(deltaTime * ratio) }
                },
            });

        #endregion

        #region Private methods

        private void Resize_ToLength_EventsCollection(
            ICollection<MidiEvent> midiEvents,
            ITimeSpan length,
            TempoMap tempoMap,
            ICollection<MidiEvent> expectedEvents)
        {
            var expectedTrackChunk = new TrackChunk(expectedEvents);

            //

            var trackChunk = new TrackChunk(midiEvents.Select(e => e.Clone()));
            trackChunk.Resize(length, tempoMap);
            MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Invalid track chunk.");

            //

            var trackChunks = new[] { new TrackChunk(midiEvents.Select(e => e.Clone())) };
            trackChunks.Resize(length, tempoMap);
            MidiAsserts.AreEqual(new[] { expectedTrackChunk }, trackChunks, true, "Invalid track chunks.");

            //

            var midiFile = new MidiFile(new TrackChunk(midiEvents.Select(e => e.Clone())));
            midiFile.Resize(length);
            MidiAsserts.AreEqual(new MidiFile(expectedTrackChunk), midiFile, false, "Invalid MIDI file.");
        }

        private void Resize_ByRatio_EventsCollection(
            ICollection<MidiEvent> midiEvents,
            double ratio,
            ICollection<MidiEvent> expectedEvents)
        {
            var expectedTrackChunk = new TrackChunk(expectedEvents);

            //

            var trackChunk = new TrackChunk(midiEvents.Select(e => e.Clone()));
            trackChunk.Resize(ratio);
            MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Invalid track chunk.");

            //

            var trackChunks = new[] { new TrackChunk(midiEvents.Select(e => e.Clone())) };
            trackChunks.Resize(ratio);
            MidiAsserts.AreEqual(new[] { expectedTrackChunk }, trackChunks, true, "Invalid track chunks.");

            //

            var midiFile = new MidiFile(new TrackChunk(midiEvents.Select(e => e.Clone())));
            midiFile.Resize(ratio);
            MidiAsserts.AreEqual(new MidiFile(expectedTrackChunk), midiFile, false, "Invalid MIDI file.");
        }

        private void Resize_ToLength_EventsCollections(
            ICollection<ICollection<MidiEvent>> midiEvents,
            ITimeSpan length,
            TempoMap tempoMap,
            ICollection<ICollection<MidiEvent>> expectedEvents)
        {
            var expectedTrackChunks = expectedEvents.Select(e => new TrackChunk(e)).ToArray();

            //

            var trackChunks = midiEvents.Select(e => new TrackChunk(e.Select(ee => ee.Clone()))).ToArray();
            trackChunks.Resize(length, tempoMap);
            MidiAsserts.AreEqual(expectedTrackChunks, trackChunks, true, "Invalid track chunks.");

            //

            var midiFile = new MidiFile(midiEvents.Select(e => new TrackChunk(e.Select(ee => ee.Clone()))).ToArray());
            midiFile.Resize(length);
            MidiAsserts.AreEqual(new MidiFile(expectedTrackChunks), midiFile, false, "Invalid MIDI file.");
        }

        private void Resize_ByRatio_EventsCollections(
            ICollection<ICollection<MidiEvent>> midiEvents,
            double ratio,
            ICollection<ICollection<MidiEvent>> expectedEvents)
        {
            var expectedTrackChunks = expectedEvents.Select(e => new TrackChunk(e)).ToArray();

            //

            var trackChunks = midiEvents.Select(e => new TrackChunk(e.Select(ee => ee.Clone()))).ToArray();
            trackChunks.Resize(ratio);
            MidiAsserts.AreEqual(expectedTrackChunks, trackChunks, true, "Invalid track chunks.");

            //

            var midiFile = new MidiFile(midiEvents.Select(e => new TrackChunk(e.Select(ee => ee.Clone()))).ToArray());
            midiFile.Resize(ratio);
            MidiAsserts.AreEqual(new MidiFile(expectedTrackChunks), midiFile, false, "Invalid MIDI file.");
        }

        #endregion
    }
}
