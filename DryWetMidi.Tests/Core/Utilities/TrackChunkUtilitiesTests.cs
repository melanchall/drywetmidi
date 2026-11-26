using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed class TrackChunkUtilitiesTests
    {
        #region Test methods

        #region GetChannels

        [Test]
        public void GetChannels_SingleTrackChunk_NoEvents()
        {
            var trackChunk = new TrackChunk();
            CollectionAssert.IsEmpty(trackChunk.GetChannels(), "Channels collection is not empty for empty track chunk.");
        }

        [Test]
        public void GetChannels_SingleTrackChunk_NoChannelEvents()
        {
            var trackChunk = new TrackChunk(new TextEvent("Text"));
            CollectionAssert.IsEmpty(trackChunk.GetChannels(), "Channels collection is not empty for track chunk without channel events.");
        }

        [Test]
        public void GetChannels_SingleTrackChunk_SingleChannel()
        {
            var trackChunk = new TrackChunk(
                new TextEvent("Text"),
                new NoteOnEvent { Channel = (FourBitNumber)10 });
            CollectionAssert.AreEquivalent(
                new[] { (FourBitNumber)10 },
                trackChunk.GetChannels(),
                "Channels collection is invalid.");
        }

        [Test]
        public void GetChannels_SingleTrackChunk_MultipleChannels()
        {
            var trackChunk = new TrackChunk(
                new TextEvent("Text"),
                new NoteOnEvent { Channel = (FourBitNumber)10 },
                new NoteOffEvent { Channel = (FourBitNumber)10 },
                new NoteOnEvent(),
                new NoteOnEvent { Channel = (FourBitNumber)1 });
            CollectionAssert.AreEquivalent(
                new[] { (FourBitNumber)10, (FourBitNumber)0, (FourBitNumber)1 },
                trackChunk.GetChannels(),
                "Channels collection is invalid.");
        }

        [Test]
        public void GetChannels_MultipleTrackChunks_NoTrackChunks()
        {
            var trackChunks = Enumerable.Empty<TrackChunk>();
            CollectionAssert.IsEmpty(trackChunks.GetChannels(), "Channels collection is not empty for empty track chunks collection.");
        }

        [Test]
        public void GetChannels_MultipleTrackChunks_NoEvents()
        {
            var trackChunks = new[] { new TrackChunk() };
            CollectionAssert.IsEmpty(trackChunks.GetChannels(), "Channels collection is not empty for empty track chunks.");
        }

        [Test]
        public void GetChannels_MultipleTrackChunks_NoChannelEvents()
        {
            var trackChunks = new[]
            {
                new TrackChunk(),
                new TrackChunk(new TextEvent("Text"))
            };
            CollectionAssert.IsEmpty(trackChunks.GetChannels(), "Channels collection is not empty for track chunks without channel events.");
        }

        [Test]
        public void GetChannels_MultipleTrackChunks_SingleChannel()
        {
            var trackChunks = new[]
            {
                new TrackChunk(new TextEvent("Text"), new NoteOnEvent { Channel = (FourBitNumber)10 }),
                new TrackChunk(new NoteOffEvent { Channel = (FourBitNumber)10 })
            };
            CollectionAssert.AreEquivalent(
                new[] { (FourBitNumber)10 },
                trackChunks.GetChannels(),
                "Channels collection is invalid.");
        }

        [Test]
        public void GetChannels_MultipleTrackChunks_MultipleChannels()
        {
            var trackChunks = new[]
            {
                new TrackChunk(
                    new TextEvent("Text"),
                    new NoteOnEvent { Channel = (FourBitNumber)10 },
                    new NoteOffEvent { Channel = (FourBitNumber)10 },
                    new NoteOnEvent(),
                    new NoteOnEvent { Channel = (FourBitNumber)1 }),
                new TrackChunk(
                    new TextEvent("Text 2"),
                    new NoteOnEvent { Channel = (FourBitNumber)1 },
                    new NoteOffEvent { Channel = (FourBitNumber)2 })
            };
            CollectionAssert.AreEquivalent(
                new[] { (FourBitNumber)10, (FourBitNumber)0, (FourBitNumber)1, (FourBitNumber)2 },
                trackChunks.GetChannels(),
                "Channels collection is invalid.");
        }

        #endregion

        #region Merge

        [Test]
        public void Merge_NullTrackChunks()
        {
            IEnumerable<TrackChunk> trackChunks = null;

            ClassicAssert.Throws<ArgumentNullException>(
                () => trackChunks.Merge(),
                "Exception not thrown for null track chunks collection.");
        }

        [Test]
        public void Merge_EmptyTrackChunks()
        {
            var trackChunks = Enumerable.Empty<TrackChunk>();

            ClassicAssert.Throws<ArgumentException>(
                () => trackChunks.Merge(),
                "Exception not thrown for empty track chunks collection.");
        }

        [Test]
        public void Merge_SingleTrackChunk_NoEvents()
        {
            var trackChunk = new TrackChunk();

            var result = new[] { trackChunk }.Merge();

            CollectionAssert.IsEmpty(result.Events, "Result track chunk contains events.");
        }

        [Test]
        public void Merge_SingleTrackChunk()
        {
            var trackChunk = new TrackChunk(
                new TextEvent("A"),
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 20 });

            var result = new[] { trackChunk }.Merge();

            MidiAsserts.AreEqual(
                new TrackChunk(
                    new TextEvent("A"),
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 20 }),
                result,
                true,
                "Result track chunk is invalid.");
        }

        [Test]
        public void Merge_MultipleTrackChunks_NoEvents()
        {
            var trackChunks = new[]
            {
                new TrackChunk(),
                new TrackChunk()
            };

            var result = trackChunks.Merge();

            CollectionAssert.IsEmpty(result.Events, "Result track chunk contains events.");
        }

        [Test]
        public void Merge_MultipleTrackChunks_OneEmpty()
        {
            var trackChunks = new[]
            {
                new TrackChunk(),
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = 5 },
                    new NoteOnEvent { DeltaTime = 10 })
            };

            var result = trackChunks.Merge();

            MidiAsserts.AreEqual(
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = 5 },
                    new NoteOnEvent { DeltaTime = 10 }),
                result,
                true,
                "Result track chunk is invalid.");
        }

        [Test]
        public void Merge_MultipleTrackChunks_DifferentTimes()
        {
            var trackChunks = new[]
            {
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)1, (SevenBitNumber)100) { DeltaTime = 10 },
                    new NoteOffEvent((SevenBitNumber)1, (SevenBitNumber)0) { DeltaTime = 30 }),
                new TrackChunk(
                    new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)100) { DeltaTime = 20 },
                    new NoteOffEvent((SevenBitNumber)2, (SevenBitNumber)0) { DeltaTime = 5 }),
                new TrackChunk(
                    new TextEvent("A"),
                    new TextEvent("B") { DeltaTime = 100 })
            };

            var result = trackChunks.Merge();

            MidiAsserts.AreEqual(
                new TrackChunk(
                    new TextEvent("A"),
                    new NoteOnEvent((SevenBitNumber)1, (SevenBitNumber)100) { DeltaTime = 10 },
                    new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)100) { DeltaTime = 10 },
                    new NoteOffEvent((SevenBitNumber)2, (SevenBitNumber)0) { DeltaTime = 5 },
                    new NoteOffEvent((SevenBitNumber)1, (SevenBitNumber)0) { DeltaTime = 15 },
                    new TextEvent("B") { DeltaTime = 60 }),
                result,
                true,
                "Result track chunk is invalid.");
        }

        [Test]
        public void Merge_MultipleTrackChunks_SameTime_MetaEventsFirst()
        {
            var trackChunks = new[]
            {
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 10 },
                    new ControlChangeEvent { DeltaTime = 10 }),
                new TrackChunk(
                    new SetTempoEvent(400000) { DeltaTime = 10 },
                    new TextEvent("A") { DeltaTime = 10 })
            };

            var result = trackChunks.Merge();

            MidiAsserts.AreEqual(
                new TrackChunk(
                    new SetTempoEvent(400000) { DeltaTime = 10 },
                    new NoteOnEvent(),
                    new TextEvent("A") { DeltaTime = 10 },
                    new ControlChangeEvent()),
                result,
                true,
                "Result track chunk is invalid.");
        }

        [Test]
        public void Merge_MultipleTrackChunks_SameTime_OriginalOrderPreserved()
        {
            var trackChunks = new[]
            {
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = 10 },
                    new NoteOnEvent((SevenBitNumber)1, (SevenBitNumber)100) { DeltaTime = 10 },
                    new NormalSysExEvent(new byte[] { 0xF7 })),
                new TrackChunk(
                    new TextEvent("B") { DeltaTime = 10 },
                    new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)100) { DeltaTime = 10 }),
                new TrackChunk(
                    new TextEvent("C") { DeltaTime = 10 },
                    new NoteOnEvent((SevenBitNumber)3, (SevenBitNumber)100) { DeltaTime = 10 })
            };

            var result = trackChunks.Merge();

            MidiAsserts.AreEqual(
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = 10 },
                    new TextEvent("B"),
                    new TextEvent("C"),
                    new NoteOnEvent((SevenBitNumber)1, (SevenBitNumber)100) { DeltaTime = 10 },
                    new NormalSysExEvent(new byte[] { 0xF7 }),
                    new NoteOnEvent((SevenBitNumber)2, (SevenBitNumber)100),
                    new NoteOnEvent((SevenBitNumber)3, (SevenBitNumber)100)),
                result,
                true,
                "Result track chunk is invalid.");
        }

        [Test]
        public void Merge_MultipleTrackChunks_EventsCloned()
        {
            var noteOnEvent = new NoteOnEvent { DeltaTime = 10 };
            var textEvent = new TextEvent("A") { DeltaTime = 20 };
            var trackChunks = new[]
            {
                new TrackChunk(noteOnEvent),
                new TrackChunk(textEvent)
            };

            var result = trackChunks.Merge();

            CollectionAssert.DoesNotContain(trackChunks, result, "Result track chunk is one of the input track chunks.");
            ClassicAssert.IsFalse(
                result.Events.Any(e => ReferenceEquals(e, noteOnEvent) || ReferenceEquals(e, textEvent)),
                "Result track chunk contains original events.");
        }

        [Test]
        public void Merge_MultipleTrackChunks_InputNotChanged()
        {
            var trackChunks = new[]
            {
                new TrackChunk(
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 30 }),
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = 20 },
                    new TextEvent("B") { DeltaTime = 5 })
            };

            trackChunks.Merge();

            MidiAsserts.AreEqual(
                new MidiChunk[]
                {
                    new TrackChunk(
                        new NoteOnEvent { DeltaTime = 10 },
                        new NoteOffEvent { DeltaTime = 30 }),
                    new TrackChunk(
                        new TextEvent("A") { DeltaTime = 20 },
                        new TextEvent("B") { DeltaTime = 5 })
                },
                trackChunks,
                true,
                "Input track chunks are changed.");
        }

        [Test]
        public void Merge_SingleTrackChunk_EventsCloned()
        {
            var noteOnEvent = new NoteOnEvent { DeltaTime = 10 };
            var trackChunk = new TrackChunk(noteOnEvent);

            var result = new[] { trackChunk }.Merge();

            ClassicAssert.AreNotSame(trackChunk, result, "Result track chunk is the input track chunk.");
            ClassicAssert.AreNotSame(noteOnEvent, result.Events.Single(), "Result track chunk contains original event.");
        }

        [Test]
        public void Merge_NullTrackChunksOnly()
        {
            var trackChunks = new TrackChunk[] { null, null };

            var result = trackChunks.Merge();

            CollectionAssert.IsEmpty(result.Events, "Result track chunk contains events.");
        }

        [Test]
        public void Merge_TrackChunksWithNulls()
        {
            var trackChunks = new[]
            {
                null,
                new TrackChunk(new NoteOnEvent { DeltaTime = 10 }),
                null,
                new TrackChunk(new TextEvent("A") { DeltaTime = 5 })
            };

            var result = trackChunks.Merge();

            MidiAsserts.AreEqual(
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = 5 },
                    new NoteOnEvent { DeltaTime = 5 }),
                result,
                true,
                "Result track chunk is invalid.");
        }

        [Test]
        public void Merge_SingleTrackChunkWithNulls()
        {
            var trackChunk = new TrackChunk(new NoteOnEvent { DeltaTime = 10 });
            var trackChunks = new[] { null, trackChunk, null };

            var result = trackChunks.Merge();

            ClassicAssert.AreNotSame(trackChunk, result, "Result track chunk is the input track chunk.");
            MidiAsserts.AreEqual(
                new TrackChunk(new NoteOnEvent { DeltaTime = 10 }),
                result,
                true,
                "Result track chunk is invalid.");
        }

        #endregion

        #endregion
    }
}
