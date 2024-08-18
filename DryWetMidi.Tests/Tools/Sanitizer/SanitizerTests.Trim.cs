using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;
using System;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed partial class SanitizerTests
    {
        #region Test methods

        [Test]
        public void Sanitize_Trim_EmptyFile() => Sanitize(
            midiFile: new MidiFile(),
            settings: null,
            expectedMidiFile: new MidiFile());

        [Test]
        public void Sanitize_Trim_EmptyTrackChunks() => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(),
                new TrackChunk()),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(),
                new TrackChunk()));

        [Test]
        public void Sanitize_Trim_SingleTrackChunk_1([Values(0, 100, 1000)] long firstEventTime) => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = firstEventTime })),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false,
                Trim = true,
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A"))));

        [Test]
        public void Sanitize_Trim_SingleTrackChunk_2([Values(0, 100, 1000)] long firstEventTime) => Sanitize(
            midiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A") { DeltaTime = firstEventTime }),
                new TrackChunk()),
            settings: new SanitizingSettings
            {
                RemoveEmptyTrackChunks = false,
                Trim = true,
            },
            expectedMidiFile: new MidiFile(
                new TrackChunk(
                    new TextEvent("A")),
                new TrackChunk()));

        [Test]
        public void Sanitize_Trim_MultipleTrackChunks_1(
            [Values(0, 100, 1000)] long aFirstEventTime,
            [Values(0, 300, 3000)] long bFirstEventTime)
        {
            var minTime = Math.Min(aFirstEventTime, bFirstEventTime);
            Sanitize(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new TextEvent("A") { DeltaTime = aFirstEventTime }),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = bFirstEventTime })),
                settings: new SanitizingSettings
                {
                    RemoveEmptyTrackChunks = false,
                    Trim = true,
                },
                expectedMidiFile: new MidiFile(
                    new TrackChunk(
                        new TextEvent("A") { DeltaTime = aFirstEventTime - minTime }),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = bFirstEventTime - minTime })));
        }

        [Test]
        public void Sanitize_Trim_MultipleTrackChunks_2(
            [Values(0, 100, 1000)] long aFirstEventTime,
            [Values(0, 300, 3000)] long bFirstEventTime)
        {
            var minTime = Math.Min(aFirstEventTime, bFirstEventTime);
            Sanitize(
                midiFile: new MidiFile(
                    new TrackChunk(
                        new TextEvent("A") { DeltaTime = aFirstEventTime }),
                    new TrackChunk(),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = bFirstEventTime })),
                settings: new SanitizingSettings
                {
                    RemoveEmptyTrackChunks = false,
                    Trim = true,
                },
                expectedMidiFile: new MidiFile(
                    new TrackChunk(
                        new TextEvent("A") { DeltaTime = aFirstEventTime - minTime }),
                    new TrackChunk(),
                    new TrackChunk(
                        new TextEvent("B") { DeltaTime = bFirstEventTime - minTime })));
        }

        #endregion
    }
}
