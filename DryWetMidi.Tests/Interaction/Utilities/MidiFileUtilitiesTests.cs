using System;
using System.Linq;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed class MidiFileUtilitiesTests
    {
        #region Test methods

        #region GetDuration

        [Test]
        public void GetDuration_EmptyFile()
        {
            var midiFile = new MidiFile();
            var duration = midiFile.GetDuration<MetricTimeSpan>();
            Assert.AreEqual(new MetricTimeSpan(), duration, "Duration of empty MIDI file is invalid.");
        }

        [Test]
        public void GetDuration_Metric()
        {
            var midiFile = new PatternBuilder()
                .SetNoteLength(new MetricTimeSpan(0, 0, 1))
                .Note(DryWetMidi.MusicTheory.NoteName.CSharp)
                .Repeat(9)
                .Build()
                .ToFile(TempoMap.Default);

            var duration = midiFile.GetDuration(TimeSpanType.Metric);
            TimeSpanTestUtilities.AreEqual(new MetricTimeSpan(0, 0, 10), duration, "Duration of MIDI file is invalid.");
        }

        [Test]
        public void GetDuration_Midi()
        {
            var midiFile = new MidiFile(
                new TrackChunk(
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 1000 }));

            var duration = midiFile.GetDuration(TimeSpanType.Midi);
            Assert.AreEqual(new MidiTimeSpan(1000), duration, "Duration of MIDI file is invalid.");
        }

        [Test]
        public void GetDuration_TrackChunk_Empty()
        {
            var trackChunk = new TrackChunk();
            var duration = trackChunk.GetDuration<MetricTimeSpan>(TempoMap.Default);
            Assert.AreEqual(new MetricTimeSpan(), duration, "Invalid duration.");
        }

        [Test]
        public void GetDuration_TrackChunk_OneEvent_Midi_1([Values(0, 100)] long deltaTime)
        {
            var trackChunk = new TrackChunk(new TextEvent("A") { DeltaTime = deltaTime });
            var duration = trackChunk.GetDuration<MidiTimeSpan>(TempoMap.Default);
            Assert.AreEqual(new MidiTimeSpan(deltaTime), duration, "Invalid duration.");
        }

        [Test]
        public void GetDuration_TrackChunk_OneEvent_Midi_2([Values(0, 100)] long deltaTime)
        {
            var trackChunk = new TrackChunk(new TextEvent("A") { DeltaTime = deltaTime });
            var duration = trackChunk.GetDuration(TimeSpanType.Midi, TempoMap.Default);
            Assert.AreEqual(new MidiTimeSpan(deltaTime), duration, "Invalid duration.");
        }

        [Test]
        public void GetDuration_TrackChunk_OneEvent_Metric_1([Values(0, 100)] long deltaTime)
        {
            var trackChunk = new TrackChunk(new TextEvent("A") { DeltaTime = deltaTime });
            var duration = trackChunk.GetDuration<MetricTimeSpan>(TempoMap.Default);
            Assert.AreEqual(TimeConverter.ConvertTo<MetricTimeSpan>(deltaTime, TempoMap.Default), duration, "Invalid duration.");
        }

        [Test]
        public void GetDuration_TrackChunk_OneEvent_Metric_2([Values(0, 100)] long deltaTime)
        {
            var trackChunk = new TrackChunk(new TextEvent("A") { DeltaTime = deltaTime });
            var duration = trackChunk.GetDuration(TimeSpanType.Metric, TempoMap.Default);
            Assert.AreEqual(TimeConverter.ConvertTo<MetricTimeSpan>(deltaTime, TempoMap.Default), duration, "Invalid duration.");
        }

        [Test]
        public void GetDuration_TrackChunk_MultipleEvents_Midi([Values(0, 100)] long deltaTime1, [Values(0, 50)] long deltaTime2)
        {
            var trackChunk = new TrackChunk(
                new TextEvent("A") { DeltaTime = deltaTime1 },
                new TextEvent("B") { DeltaTime = deltaTime2 });
            var duration = trackChunk.GetDuration<MidiTimeSpan>(TempoMap.Default);
            Assert.AreEqual(new MidiTimeSpan(deltaTime1 + deltaTime2), duration, "Invalid duration.");
        }

        [Test]
        public void GetDuration_TrackChunks_EmptyCollection()
        {
            var trackChunks = Array.Empty<TrackChunk>();
            var duration = trackChunks.GetDuration<MetricTimeSpan>(TempoMap.Default);
            Assert.AreEqual(new MetricTimeSpan(), duration, "Invalid duration.");
        }

        [Test]
        public void GetDuration_TrackChunks_Empty()
        {
            var trackChunks = new[] { new TrackChunk(), new TrackChunk() };
            var duration = trackChunks.GetDuration<MetricTimeSpan>(TempoMap.Default);
            Assert.AreEqual(new MetricTimeSpan(), duration, "Invalid duration.");
        }

        [Test]
        public void GetDuration_TrackChunks_OneEvent_1([Values(0, 100)] long deltaTime)
        {
            var trackChunks = new[]
            {
                new TrackChunk(new TextEvent("A") { DeltaTime = deltaTime }),
                new TrackChunk()
            };
            var duration = trackChunks.GetDuration<MidiTimeSpan>(TempoMap.Default);
            Assert.AreEqual(new MidiTimeSpan(deltaTime), duration, "Invalid duration.");
        }

        [Test]
        public void GetDuration_TrackChunks_OneEvent_2([Values(0, 100)] long deltaTime)
        {
            var trackChunks = new[]
            {
                new TrackChunk(),
                new TrackChunk(new TextEvent("A") { DeltaTime = deltaTime }),
            };
            var duration = trackChunks.GetDuration<MidiTimeSpan>(TempoMap.Default);
            Assert.AreEqual(new MidiTimeSpan(deltaTime), duration, "Invalid duration.");
        }

        [Test]
        public void GetDuration_TrackChunks_OneEvent_3([Values(0, 100)] long deltaTime)
        {
            var trackChunks = new[]
            {
                new TrackChunk(),
                new TrackChunk(new TextEvent("A") { DeltaTime = deltaTime }),
            };
            var duration = trackChunks.GetDuration<MetricTimeSpan>(TempoMap.Default);
            Assert.AreEqual(TimeConverter.ConvertTo<MetricTimeSpan>(deltaTime, TempoMap.Default), duration, "Invalid duration.");
        }

        [Test]
        public void GetDuration_TrackChunks_OneEventInEachChunk_1([Values(0, 50, 100)] long deltaTime1, [Values(0, 50, 100)] long deltaTime2)
        {
            var trackChunks = new[]
            {
                new TrackChunk(new TextEvent("A") { DeltaTime = deltaTime1 }),
                new TrackChunk(new TextEvent("B") { DeltaTime = deltaTime2 })
            };
            var duration = trackChunks.GetDuration<MidiTimeSpan>(TempoMap.Default);
            Assert.AreEqual(new MidiTimeSpan(Math.Max(deltaTime1, deltaTime2)), duration, "Invalid duration.");
        }

        #endregion

        #region IsEmpty

        [Test]
        public void IsEmpty_True()
        {
            Assert.IsTrue(new MidiFile().IsEmpty());
        }

        [Test]
        public void IsEmpty_False_SingeTrackChunk()
        {
            Assert.IsFalse(new MidiFile(new TrackChunk(new TextEvent())).IsEmpty());
        }

        [Test]
        public void IsEmpty_False_MultipleTrackChunks()
        {
            Assert.IsFalse(new MidiFile(new TrackChunk(new TextEvent()), new TrackChunk(new NoteOnEvent(), new NoteOffEvent())).IsEmpty());
        }

        #endregion

        #region ShiftEvents

        [Test]
        public void ShiftEvents_ValidFiles_Midi()
        {
            var distance = 10000;

            foreach (var midiFile in TestFilesProvider.GetValidFiles())
            {
                var originalTimes = midiFile.GetTimedEvents().Select(e => e.Time).ToList();

                midiFile.ShiftEvents((MidiTimeSpan)distance);
                var newTimes = midiFile.GetTimedEvents().Select(e => e.Time).ToList();

                Assert.IsTrue(midiFile.GetTimedEvents().All(e => e.Time >= distance), "Some events are not shifted.");
                CollectionAssert.AreEqual(originalTimes, newTimes.Select(t => t - distance));
            }
        }

        [Test]
        public void ShiftEvents_ValidFiles_Metric()
        {
            var distance = new MetricTimeSpan(0, 1, 0);

            foreach (var midiFile in TestFilesProvider.GetValidFiles())
            {
                midiFile.ShiftEvents(distance);

                var tempoMap = midiFile.GetTempoMap();

                Assert.IsTrue(midiFile.GetTimedEvents()
                                      .Select(e => e.TimeAs<MetricTimeSpan>(tempoMap).CompareTo(distance))
                                      .All(t => t >= 0),
                              "Some events are not shifted.");
            }
        }

        #endregion

        #endregion
    }
}
