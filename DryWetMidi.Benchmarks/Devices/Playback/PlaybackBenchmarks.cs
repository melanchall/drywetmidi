using System.Linq;
using BenchmarkDotNet.Attributes;
using Melanchall.DryWetMidi.Devices;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Benchmarks.Devices
{
    [TestFixture]
    public sealed class PlaybackBenchmarks : BenchmarkTest
    {
        [InProcessSimpleJob(BenchmarkDotNet.Engines.RunStrategy.Monitoring, warmupCount: 5, targetCount: 5, launchCount: 5, invocationCount: 5)]
        public class Benchmarks_Playback
        {
            private Playback _playback;
            private Playback _playbackWithNoteCallback;

            [GlobalSetup]
            public void GlobalSetup()
            {
                var midiEvents = Enumerable.Range(0, 10)
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

            [Benchmark]
            public void Play()
            {
                _playback.MoveToStart();
                _playback.Play();
            }

            [Benchmark]
            public void PlayWithNoteCallback()
            {
                _playbackWithNoteCallback.MoveToStart();
                _playbackWithNoteCallback.Play();
            }
        }

        [Test]
        public void Play()
        {
            RunBenchmarks<Benchmarks_Playback>();
        }
    }
}
