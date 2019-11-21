using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;

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

        #endregion
    }
}
