using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Attributes;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Test methods

        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Clear_WithoutPlaying_1() => CheckPlaybackDataChangesOnTheFly_Clear_WithoutPlaying(
            initialObjects: new[]
            {
                new TimedEvent(new SetTempoEvent(), 0),
                new TimedEvent(new TextEvent("AAA"), 100),
            },
            tempoMap: TempoMap.Default,
            check: playback => { });

        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Clear_WithoutPlaying_2() => CheckPlaybackDataChangesOnTheFly_Clear_WithoutPlaying(
            initialObjects: new[]
            {
                new TimedEvent(new SetTempoEvent(400000), 50),
                new TimedEvent(new NoteOnEvent(), 70),
                new TimedEvent(new TextEvent("AAA"), 100),
                new TimedEvent(new NoteOffEvent(), 70),
            },
            tempoMap: TempoMap.Default,
            check: playback => { });

        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Clear_WithoutPlaying_3() => CheckPlaybackDataChangesOnTheFly_Clear_WithoutPlaying(
            initialObjects: new ITimedObject[]
            {
                new TimedEvent(new SetTempoEvent(400000), 50),
                new Note((SevenBitNumber)70, 200),
                new TimedEvent(new TextEvent("AAA"), 100),
            },
            tempoMap: TempoMap.Create(new TicksPerQuarterNoteTimeDivision(480)),
            check: playback => { });

        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Clear_WithoutPlaying_4() => CheckPlaybackDataChangesOnTheFly_Clear_WithoutPlaying(
            initialObjects: new ITimedObject[]
            {
                new TimedEvent(new SetTempoEvent(400000), 50),
                new TimedEvent(new TextEvent("AAA"), 100),
                new Note((SevenBitNumber)70, 200),
            },
            tempoMap: TempoMap.Create(new TicksPerQuarterNoteTimeDivision(480), Tempo.FromBeatsPerMinute(240)),
            check: playback => { });

        [TimingCritical]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Clear_WithPlaying_1()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
                new TimedEvent(new ProgramChangeEvent((SevenBitNumber)10)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Clear();
                        CheckDuration(TimeSpan.Zero, playback);

                        WaitOperations.Wait(TimeSpan.FromMilliseconds(20));

                        collection.Add(new TimedEvent(new MarkerEvent("END")).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap));

                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 500));
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(0)),
                    new TimestampedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                });
        }

        [TimingCritical]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Clear_WithPlaying_2()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
                new TimedEvent(new ProgramChangeEvent((SevenBitNumber)10)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.ChangeCollection(() =>
                        {
                            collection.Clear();
                            CheckDuration(TimeSpan.FromMilliseconds(1000), playback);

                            collection.Add(new TimedEvent(new MarkerEvent("END")).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap));
                        });

                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 500));
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(0)),
                    new TimestampedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                    new TimestampedEvent(new MarkerEvent("END"), TimeSpan.FromMilliseconds(300)),
                });
        }

        [TimingCritical]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Clear_WithPlaying_3()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
                new TimedEvent(new ProgramChangeEvent((SevenBitNumber)10)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Clear();
                        CheckDuration(TimeSpan.FromMilliseconds(0), playback);
                    }),
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        playback.Start();
                        collection.Add(new TimedEvent(new MarkerEvent("END")).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap));
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 500));
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(0)),
                    new TimestampedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                    new TimestampedEvent(new MarkerEvent("END"), TimeSpan.FromMilliseconds(350)),
                });
        }

        [TimingCritical]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Clear_WithPlaying_4()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
                new TimedEvent(new ControlChangeEvent((SevenBitNumber)10, (SevenBitNumber)20)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Clear();
                        CheckDuration(TimeSpan.FromMilliseconds(0), playback);
                    }),
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        playback.Start();
                        collection.Add(new TimedEvent(new MarkerEvent("END")).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap));
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 500));
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(0)),
                    new TimestampedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                    new TimestampedEvent(new MarkerEvent("END"), TimeSpan.FromMilliseconds(350)),
                });
        }

        [TimingCritical]
        [Test]
        public void CheckPlaybackDataChangesOnTheFly_Clear_WithPlaying_5()
        {
            var initialObjects = new ITimedObject[]
            {
                new Note((SevenBitNumber)70).SetLength(new MetricTimeSpan(0, 0, 1), TempoMap),
                new TimedEvent(new PitchBendEvent((SevenBitNumber)10)).SetTime(new MetricTimeSpan(0, 0, 0, 300), TempoMap),
            };

            CheckPlaybackDataChangesOnTheFly(
                initialObjects: initialObjects,
                actions: new[]
                {
                    new DynamicPlaybackAction(100, (playback, collection) =>
                    {
                        collection.Clear();
                        CheckDuration(TimeSpan.FromMilliseconds(0), playback);
                    }),
                    new DynamicPlaybackAction(50, (playback, collection) =>
                    {
                        playback.Start();
                        collection.Add(new TimedEvent(new MarkerEvent("END")).SetTime(new MetricTimeSpan(0, 0, 0, 700), TempoMap));
                        playback.MoveToTime(new MetricTimeSpan(0, 0, 0, 500));
                    }),
                },
                expectedReceivedEvents: new[]
                {
                    new TimestampedEvent(new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity), TimeSpan.FromMilliseconds(0)),
                    new TimestampedEvent(new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity), TimeSpan.FromMilliseconds(100)),
                    new TimestampedEvent(new MarkerEvent("END"), TimeSpan.FromMilliseconds(350)),
                });
        }

        #endregion

        #region Private methods

        private void CheckPlaybackDataChangesOnTheFly_Clear_WithoutPlaying(
            ICollection<ITimedObject> initialObjects,
            TempoMap tempoMap,
            Action<Playback> check)
        {
            var initialTempoMap = tempoMap.Clone();

            var collection = new ObservableTimedObjectsCollection(initialObjects);
            var playback = new Playback(collection, tempoMap, new PlaybackSettings
            {
                CalculateTempoMap = true,
            });

            collection.Clear();

            ClassicAssert.AreEqual(
                initialTempoMap,
                playback.TempoMap,
                "Invalid tempo map.");

            ClassicAssert.AreEqual(
                (MidiTimeSpan)0,
                playback.GetDuration<MidiTimeSpan>(),
                "Invalid duration.");
            ClassicAssert.AreEqual(
                (MidiTimeSpan)0,
                playback.GetCurrentTime<MidiTimeSpan>(),
                "Invalid duration.");

            var playedEvents = new List<MidiEvent>();
            playback.EventPlayed += (_, e) => playedEvents.Add(e.Event);
            playback.Start();
            WaitOperations.Wait(() => !playback.IsRunning, TimeSpan.FromSeconds(1));
            CollectionAssert.IsEmpty(playedEvents, "Some events were played.");

            playback.MoveToTime((MidiTimeSpan)100);
            ClassicAssert.AreEqual(
                (MidiTimeSpan)0,
                playback.GetCurrentTime<MidiTimeSpan>(),
                "Invalid duration.");

            check(playback);
        }

        #endregion
    }
}
