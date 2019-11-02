using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Core
{
    [TestFixture]
    public sealed class MidiFileUtilitiesTests
    {
        #region Test methods

        #region GetChannels

        [Test]
        public void GetChannels_NoEvents()
        {
            var midiFile = new MidiFile();
            CollectionAssert.IsEmpty(midiFile.GetChannels(), "Channels collection is not empty for empty MIDI file.");
        }

        [Test]
        public void GetChannels_EmptyTrackChunks()
        {
            var trackChunks = new[] { new TrackChunk(), new TrackChunk() };
            var midiFile = new MidiFile(trackChunks);
            CollectionAssert.IsEmpty(midiFile.GetChannels(), "Channels collection is not empty for MIDI file with empty track chunks.");
        }

        [Test]
        public void GetChannels_NoChannelEvents()
        {
            var trackChunks = new[]
            {
                new TrackChunk(),
                new TrackChunk(new TextEvent("Text"))
            };
            var midiFile = new MidiFile(trackChunks);
            CollectionAssert.IsEmpty(midiFile.GetChannels(), "Channels collection is not empty for MIDI file with track chunks without channel events.");
        }

        [Test]
        public void GetChannels_SingleChannel()
        {
            var trackChunks = new[]
            {
                new TrackChunk(new TextEvent("Text"), new NoteOnEvent { Channel = (FourBitNumber)10 }),
                new TrackChunk(new NoteOffEvent { Channel = (FourBitNumber)10 })
            };
            var midiFile = new MidiFile(trackChunks);
            CollectionAssert.AreEquivalent(
                new[] { (FourBitNumber)10 },
                midiFile.GetChannels(),
                "Channels collection is invalid.");
        }

        [Test]
        public void GetChannels_MultipleChannels()
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
            var midiFile = new MidiFile(trackChunks);
            CollectionAssert.AreEquivalent(
                new[] { (FourBitNumber)10, (FourBitNumber)0, (FourBitNumber)1, (FourBitNumber)2 },
                midiFile.GetChannels(),
                "Channels collection is invalid.");
        }

        #endregion

        #endregion
    }
}
