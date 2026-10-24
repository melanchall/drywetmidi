using System.Threading.Tasks;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    public sealed partial class InteractionAsyncTests
    {
        [Test]
        public async Task Getters_MatchSynchronousResults()
        {
            var track = CreateTrack();
            var file = new MidiFile(track);
            var tracks = new[] { track };
            var events = track.Events;
            var type = ObjectType.Chord | ObjectType.Note | ObjectType.TimedEvent;

            MidiAsserts.AreEqual(events.GetNotes(), await events.GetNotesAsync(), "Events notes");
            MidiAsserts.AreEqual(track.GetNotes(), await track.GetNotesAsync(), "Track notes");
            MidiAsserts.AreEqual(tracks.GetNotes(), await tracks.GetNotesAsync(), "Tracks notes");
            MidiAsserts.AreEqual(file.GetNotes(), await file.GetNotesAsync(), "File notes");
            MidiAsserts.AreEqual(events.GetChords(), await events.GetChordsAsync(), "Events chords");
            MidiAsserts.AreEqual(track.GetChords(), await track.GetChordsAsync(), "Track chords");
            MidiAsserts.AreEqual(tracks.GetChords(), await tracks.GetChordsAsync(), "Tracks chords");
            MidiAsserts.AreEqual(file.GetChords(), await file.GetChordsAsync(), "File chords");
            MidiAsserts.AreEqual(file.GetNotes().GetChords(), await CollectAsync(file.GetNotes().GetChordsAsync()), "Notes chords");
            MidiAsserts.AreEqual(events.GetTimedEvents(), await events.GetTimedEventsAsync(), "Events timed events");
            MidiAsserts.AreEqual(track.GetTimedEvents(), await track.GetTimedEventsAsync(), "Track timed events");
            MidiAsserts.AreEqual(tracks.GetTimedEvents(), await tracks.GetTimedEventsAsync(), "Tracks timed events");
            MidiAsserts.AreEqual(file.GetTimedEvents(), await file.GetTimedEventsAsync(), "File timed events");
            MidiAsserts.AreEqual(events.GetObjects(type), await events.GetObjectsAsync(type), "Events objects");
            MidiAsserts.AreEqual(track.GetObjects(type), await track.GetObjectsAsync(type), "Track objects");
            MidiAsserts.AreEqual(tracks.GetObjects(type), await tracks.GetObjectsAsync(type), "Tracks objects");
            MidiAsserts.AreEqual(file.GetObjects(type), await file.GetObjectsAsync(type), "File objects");
            ClassicAssert.AreEqual(file.GetDuration<MidiTimeSpan>(), await file.GetDurationAsync<MidiTimeSpan>(), "Async file duration differs.");
            ClassicAssert.AreEqual(track.GetDuration<MidiTimeSpan>(TempoMap.Default), await track.GetDurationAsync(TimeSpanType.Midi, TempoMap.Default), "Async track duration differs.");
            ClassicAssert.AreEqual(tracks.GetDuration<MidiTimeSpan>(TempoMap.Default), await tracks.GetDurationAsync<MidiTimeSpan>(TempoMap.Default), "Async tracks duration differs.");
            MidiAsserts.AreEqual(file.GetNotes().GetRests(), await file.GetNotes().GetRestsAsync(), "Rests");
        }
    }
}
