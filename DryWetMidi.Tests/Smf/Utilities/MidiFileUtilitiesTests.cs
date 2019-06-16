using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Smf
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

        #region TrimEnd

        [Test]
        public void TrimEnd_Empty()
        {
            var midiFile = new MidiFile(new[] { new TrackChunk(), new TrackChunk() });

            midiFile.TrimEnd(_ => true);

            CollectionAssert.IsEmpty(midiFile.GetTrackChunks().SelectMany(c => c.Events), "Trimmed empty MIDI file is not empty.");
        }

        [Test]
        public void TrimEnd_AllMatched()
        {
            var trackChunk1 = new TrackChunk(new NoteOnEvent(), new NoteOffEvent(), new TextEvent());
            var trackChunk2 = new TrackChunk(new NoteOnEvent(), new NoteOffEvent(), new TextEvent());
            var midiFile = new MidiFile(trackChunk1, trackChunk2);

            midiFile.TrimEnd(_ => true);

            CollectionAssert.IsEmpty(midiFile.GetTrackChunks().First().Events, "Fully trimmed first track chunk is not empty.");
            CollectionAssert.IsEmpty(midiFile.GetTrackChunks().Last().Events, "Fully trimmed second track chunk is not empty.");
        }

        [Test]
        public void TrimEnd_NoneMatched()
        {
            var midiEvents1 = new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent(), new TextEvent() };
            var trackChunk1 = new TrackChunk(midiEvents1);
            var midiEvents2 = new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent(), new TextEvent(), new NoteOnEvent() };
            var trackChunk2 = new TrackChunk(midiEvents2);
            var midiFile = new MidiFile(trackChunk1, trackChunk2);

            midiFile.TrimEnd(_ => false);

            CollectionAssert.AreEqual(midiEvents1, midiFile.GetTrackChunks().First().Events, "Fully untrimmed first track chunk contains different events.");
            CollectionAssert.AreEqual(midiEvents2, midiFile.GetTrackChunks().Last().Events, "Fully untrimmed second track chunk contains different events.");
        }

        [Test]
        public void TrimEnd()
        {
            var midiEvents1 = new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent { DeltaTime = 15 }, new TextEvent() };
            var trackChunk1 = new TrackChunk(midiEvents1);
            var midiEvents2 = new MidiEvent[] { new NoteOnEvent(), new NoteOffEvent(), new TextEvent { DeltaTime = 10 } };
            var trackChunk2 = new TrackChunk(midiEvents2);
            var midiFile = new MidiFile(trackChunk1, trackChunk2);

            midiFile.TrimEnd(e => e is TextEvent);

            CollectionAssert.AreEqual(midiEvents1.Take(2).ToArray(), midiFile.GetTrackChunks().First().Events, "First track chunk is trimmed incorrectly.");
            CollectionAssert.AreEqual(midiEvents2, midiFile.GetTrackChunks().Last().Events, "Second track chunk is trimmed incorrectly.");
        }

        #endregion

        #endregion
    }
}
