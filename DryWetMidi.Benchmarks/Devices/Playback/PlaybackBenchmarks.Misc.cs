using System.Linq;
using BenchmarkDotNet.Attributes;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Benchmarks.Devices
{
    [TestFixture]
    public sealed partial class PlaybackBenchmarks : BenchmarkTest
    {
        #region Nested classes

        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Monitoring, warmupCount: 2, targetCount: 3, launchCount: 4, invocationCount: 5)]
        public class Benchmarks_Playback
        {
            private Playback _playback;
            private Playback _playbackWithNoteCallback;

            [GlobalSetup]
            public void GlobalSetup()
            {
                var midiEvents = Enumerable.Range(0, 100)
                    .SelectMany(_ => new MidiEvent[] { new NoteOnEvent { DeltaTime = 1 }, new NoteOffEvent { DeltaTime = 1 } })
                    .ToList();

                _playback = new Playback(midiEvents, TempoMap.Default);

                _playbackWithNoteCallback = new Playback(midiEvents, TempoMap.Default);
                _playbackWithNoteCallback.NoteCallback = (d, rt, rl, t) => new NotePlaybackData(d.NoteNumber, d.Velocity, d.OffVelocity, d.Channel);
            }

            [GlobalCleanup]
            public void GlobalCleanup()
            {
                _playback.Dispose();
                _playbackWithNoteCallback.Dispose();
            }

            [IterationSetup]
            public void IterationSetup()
            {
                _playback.MoveToStart();
                _playbackWithNoteCallback.MoveToStart();
            }

            [Benchmark]
            public void Play()
            {
                _playback.Play();
            }

            [Benchmark]
            public void PlayWithNoteCallback()
            {
                _playbackWithNoteCallback.Play();
            }
        }

        #endregion

        #region Test methods

        [Test]
        public void Play()
        {
            RunBenchmarks<Benchmarks_Playback>();
        }

        #endregion

        #region Methods

        public static IEnumerable<ITimedObject> GetTimedObjects(int notesCount)
        {
            const long noteLength = 1000;
            return Enumerable
                .Range(0, notesCount)
                .SelectMany(i => SevenBitNumber.Values.Select(noteNumber => new Note(noteNumber, noteLength, i * noteLength)))
                .ToArray();
        }

        #endregion
    }
}
