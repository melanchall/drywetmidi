using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class TimedObjectUtilitiesTests
    {
        #region Test methods

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_EmptyCollection([Values] ContainerType containerType, [Values] bool predicateValue) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[0],
            action: c => { },
            match: c => predicateValue,
            expectedMidiEvents: new MidiEvent[0],
            expectedProcessedCount: 0);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_NoNotes_NoProcessing([Values] ContainerType containerType, [Values] bool predicateValue) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            action: c => { },
            match: c => predicateValue,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            expectedProcessedCount: 0);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_NoNotes_Processing([Values] ContainerType containerType, [Values] bool predicateValue) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            action: c => c.Time = 100,
            match: c => predicateValue,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            expectedProcessedCount: 0);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_NoChords_NoProcessing([Values] ContainerType containerType, [Values] bool predicateValue) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new ControlChangeEvent(),
            },
            action: c => { },
            match: c => predicateValue,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new ControlChangeEvent(),
            },
            expectedProcessedCount: 0);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_NoChords_Processing([Values] ContainerType containerType, [Values] bool predicateValue) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOffEvent(),
            },
            action: c => c.Time = 100,
            match: c => predicateValue,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOffEvent(),
            },
            expectedProcessedCount: 0);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_OneChord_NotMatched([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new ControlChangeEvent(),
                new NoteOffEvent(),
            },
            action: c => c.Time = 100,
            match: c => false,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new ControlChangeEvent(),
                new NoteOffEvent(),
            },
            expectedProcessedCount: 0);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_OneChord_Matched_NoProcessing([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new ControlChangeEvent(),
                new NoteOffEvent(),
            },
            action: c => { },
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new ControlChangeEvent(),
                new NoteOffEvent(),
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_OneChord_Matched_Processing_Channel([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new ControlChangeEvent(),
                new NoteOffEvent(),
            },
            action: c => ((Chord)c).Channel = (FourBitNumber)4,
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new ControlChangeEvent(),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_OneChord_Matched_Processing_Velocity([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            action: c => ((Chord)c).Velocity = (SevenBitNumber)40,
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOnEvent { Velocity = (SevenBitNumber)40 },
                new NoteOffEvent(),
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_OneChord_Matched_Processing_OffVelocity([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            action: c => ((Chord)c).OffVelocity = (SevenBitNumber)40,
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOnEvent(),
                new NoteOffEvent { Velocity = (SevenBitNumber)40 },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_OneChord_Matched_Processing_RemoveNote_WithHint([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            action: c => ((Chord)c).Notes.Remove(((Chord)c).Notes.First()),
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            expectedProcessedCount: 1,
            hint: ObjectProcessingHint.NotesCollectionCanBeChanged);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_OneChord_Matched_Processing_RemoveNote_WithoutHint([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            action: c => ((Chord)c).Notes.Remove(((Chord)c).Notes.First()),
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            expectedProcessedCount: 1,
            hint: ObjectProcessingHint.None);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_OneChord_Matched_Processing_AddNote_WithHint([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            action: c => ((Chord)c).Notes.Add(new Note((SevenBitNumber)70, 0, 0)),
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity),
                new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity),
            },
            expectedProcessedCount: 1,
            hint: ObjectProcessingHint.NotesCollectionCanBeChanged,
            newReferencesAllowed: true);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_OneChord_Matched_Processing_AddNote_WithoutHint([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            action: c => ((Chord)c).Notes.Add(new Note((SevenBitNumber)70, 0, 0)),
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            expectedProcessedCount: 1,
            hint: ObjectProcessingHint.None,
            newReferencesAllowed: true);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_OneChord_Matched_Processing_Time([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            action: c => c.Time = 100,
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_OneChord_Matched_Processing_NoteTime_1([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            action: c => ((Chord)c).Notes.First().Time = 100,
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
            },
            expectedProcessedCount: 1,
            hint: ObjectProcessingHint.NoteTimeOrLengthCanBeChanged);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_OneChord_Matched_Processing_NoteTime_2([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent { DeltaTime = 100 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            action: c => ((Chord)c).Notes.Last().Time = 50,
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 50 },
                new NoteOffEvent { DeltaTime = 50 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 50 },
            },
            expectedProcessedCount: 1,
            hint: ObjectProcessingHint.NoteTimeOrLengthCanBeChanged);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_OneChord_Matched_Processing_NoteTime_3([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue){ DeltaTime = 50 },
                new NoteOffEvent { DeltaTime = 50 },
            },
            action: c => ((Chord)c).Notes.Last().Time = 50,
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 50 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue){ DeltaTime = 50 },
                new NoteOffEvent(),
            },
            expectedProcessedCount: 1,
            hint: ObjectProcessingHint.NoteTimeOrLengthCanBeChanged);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_OneChord_Matched_Processing_Time_HintNone([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            action: c => c.Time = 100,
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            expectedProcessedCount: 1,
            hint: ObjectProcessingHint.None);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_OneChord_Matched_Processing_Length([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            action: c => ((Chord)c).Length = 100,
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOffEvent { DeltaTime = 100 },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_OneChord_Matched_Processing_Length_HintNone([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            action: c => ((Chord)c).Length = 100,
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            expectedProcessedCount: 1,
            hint: ObjectProcessingHint.None);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_OneChord_Matched_Processing_TimeAndLength([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A") { DeltaTime = 80 },
                new ControlChangeEvent { DeltaTime = 20 },
            },
            action: c =>
            {
                c.Time = 90;
                ((Chord)c).Length = 100;
            },
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A") { DeltaTime = 80 },
                new NoteOnEvent { DeltaTime = 10 },
                new ControlChangeEvent { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 90 },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_MultipleChords_NotMatched_NoProcessing([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
            },
            action: c => { },
            match: c => false,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
            },
            expectedProcessedCount: 0);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_MultipleChords_NotMatched_Processing([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
            },
            action: c => c.Time = 100,
            match: c => false,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
            },
            expectedProcessedCount: 0);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_MultipleChords_SomeMatched_NoProcessing([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
            },
            action: c => { },
            match: c => c.Time < 100,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_MultipleChords_SomeMatched_Processing_RemoveNote([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            action: c => ((Chord)c).Notes.Remove(((Chord)c).Notes.First()),
            match: c => ((Chord)c).Channel == 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            hint: ObjectProcessingHint.NotesCollectionCanBeChanged,
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_MultipleChords_SomeMatched_Processing_AddNote([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            action: c => ((Chord)c).Notes.Add(new Note((SevenBitNumber)70)),
            match: c => ((Chord)c).Channel == 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
                new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity),
                new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity),
            },
            expectedProcessedCount: 1,
            hint: ObjectProcessingHint.NotesCollectionCanBeChanged,
            newReferencesAllowed: true);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_MultipleChords_SomeMatched_Processing_1([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            action: c => ((Chord)c).Channel = (FourBitNumber)7,
            match: c => ((Chord)c).Channel == 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Channel = (FourBitNumber)7 },
                new NoteOffEvent { Channel = (FourBitNumber)7 },
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)7 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { Channel = (FourBitNumber)7 },
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_MultipleChords_SomeMatched_Processing_2([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new TextEvent("B"),
                new NoteOffEvent(),
                new TextEvent("C"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("D"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new TextEvent("E"),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
                new TextEvent("F"),
            },
            action: c => ((Chord)c).Channel = (FourBitNumber)7,
            match: c => ((Chord)c).Channel == 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Channel = (FourBitNumber)7 },
                new TextEvent("A"),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new TextEvent("B"),
                new NoteOffEvent { Channel = (FourBitNumber)7 },
                new TextEvent("C"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)7 },
                new TextEvent("D"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { Channel = (FourBitNumber)7 },
                new TextEvent("E"),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
                new TextEvent("F"),
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_MultipleChords_SomeMatched_Processing_3([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            action: c => c.Time = 100,
            match: c => ((Chord)c).Channel == 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_MultipleChords_SomeMatched_Processing_4([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            action: c =>
            {
                c.Time = 90;
                ((Chord)c).Length = 100;
            },
            match: c => ((Chord)c).Channel == 4,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { Channel = (FourBitNumber)4, DeltaTime = 90 },
                new NoteOnEvent { DeltaTime = 10 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOffEvent { Channel = (FourBitNumber)4, DeltaTime = 90 },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_MultipleChords_SomeMatched_Processing_5([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            action: c => ((Chord)c).Velocity = (SevenBitNumber)70,
            match: c => ((Chord)c).Channel == 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Velocity = (SevenBitNumber)70 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
                new NoteOnEvent { DeltaTime = 100, Velocity = (SevenBitNumber)70 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Velocity = (SevenBitNumber)70 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedProcessedCount: 2);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_MultipleChords_AllMatched_Processing_RemoveNote([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
                new NoteOnEvent((SevenBitNumber)80, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)4 },
                new NoteOffEvent((SevenBitNumber)80, SevenBitNumber.MinValue) { Channel = (FourBitNumber)4 },
            },
            action: c => ((Chord)c).Notes.Remove(((Chord)c).Notes.Last()),
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            expectedProcessedCount: 2,
            hint: ObjectProcessingHint.NotesCollectionCanBeChanged);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_MultipleChords_AllMatched_Processing_AddNote([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            action: c => ((Chord)c).Notes.Add(new Note((SevenBitNumber)70) { Channel = ((Chord)c).Channel }),
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
                new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity),
                new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity),
                new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity) { Channel = (FourBitNumber)4 },
                new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity) { Channel = (FourBitNumber)4 },
            },
            expectedProcessedCount: 2,
            hint: ObjectProcessingHint.NotesCollectionCanBeChanged,
            newReferencesAllowed: true);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_MultipleChords_AllMatched_Processing_1([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            action: c => ((Chord)c).Channel = (FourBitNumber)7,
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Channel = (FourBitNumber)7 },
                new NoteOffEvent { Channel = (FourBitNumber)7 },
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)7 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { Channel = (FourBitNumber)7 },
                new NoteOnEvent { Channel = (FourBitNumber)7 },
                new NoteOffEvent { Channel = (FourBitNumber)7 },
            },
            expectedProcessedCount: 2);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_MultipleChords_AllMatched_Processing_2([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new TextEvent("B"),
                new NoteOffEvent(),
                new TextEvent("C"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("D"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new TextEvent("E"),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
                new TextEvent("F"),
            },
            action: c => ((Chord)c).Channel = (FourBitNumber)7,
            match: c => ((Chord)c).Channel < 10,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Channel = (FourBitNumber)7 },
                new TextEvent("A"),
                new NoteOnEvent { Channel = (FourBitNumber)7 },
                new TextEvent("B"),
                new NoteOffEvent { Channel = (FourBitNumber)7 },
                new TextEvent("C"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)7 },
                new TextEvent("D"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { Channel = (FourBitNumber)7 },
                new TextEvent("E"),
                new NoteOffEvent { Channel = (FourBitNumber)7 },
                new TextEvent("F"),
            },
            expectedProcessedCount: 2);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_MultipleChords_AllMatched_Processing_3([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            action: c => c.Time = 100,
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            expectedProcessedCount: 2);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_MultipleChords_AllMatched_Processing_4([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent(),
                new TextEvent("A"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new TextEvent("B"),
            },
            action: c =>
            {
                c.Time = 90;
                ((Chord)c).Length = 100;
            },
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent { DeltaTime = 90 },
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOnEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("B") { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 90 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent(),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedProcessedCount: 3);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithPredicate_MultipleChords_AllMatched_Processing_5([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("A"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            action: c => ((Chord)c).Velocity = (SevenBitNumber)70,
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOnEvent { Channel = (FourBitNumber)4, Velocity = (SevenBitNumber)70 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Velocity = (SevenBitNumber)70 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
                new NoteOnEvent { DeltaTime = 100, Velocity = (SevenBitNumber)70 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Velocity = (SevenBitNumber)70 },
                new TextEvent("A"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedProcessedCount: 3);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithoutPredicate_EmptyCollection([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[0],
            action: c => { },
            expectedMidiEvents: new MidiEvent[0]);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithoutPredicate_NoNotes_NoProcessing([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            action: c => { },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
            });

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithoutPredicate_NoNotes_Processing([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            action: c => c.Time = 100,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
            });

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithoutPredicate_NoChords_NoProcessing([Values] ContainerType containerType, [Values] bool predicateValue) => ProcessObjects_Chords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new ControlChangeEvent(),
            },
            action: c => { },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new ControlChangeEvent(),
            });

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithoutPredicate_NoChords_Processing([Values] ContainerType containerType, [Values] bool predicateValue) => ProcessObjects_Chords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOffEvent(),
            },
            action: c => c.Time = 100,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOffEvent(),
            });

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithoutPredicate_OneChord_NoProcessing([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new ControlChangeEvent(),
                new NoteOffEvent(),
            },
            action: c => { },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new ControlChangeEvent(),
                new NoteOffEvent(),
            });

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithoutPredicate_OneChord_Processing_Channel([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new ControlChangeEvent(),
                new NoteOffEvent(),
            },
            action: c => ((Chord)c).Channel = (FourBitNumber)4,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new ControlChangeEvent(),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            });

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithoutPredicate_OneChord_Processing_Velocity([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            action: c => ((Chord)c).Velocity = (SevenBitNumber)40,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOnEvent { Velocity = (SevenBitNumber)40 },
                new NoteOffEvent(),
            });

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithoutPredicate_OneChord_Processing_OffVelocity([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            action: c => ((Chord)c).OffVelocity = (SevenBitNumber)40,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOnEvent(),
                new NoteOffEvent { Velocity = (SevenBitNumber)40 },
            });

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithoutPredicate_OneChord_Processing_Time([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            action: c => c.Time = 100,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
            });

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithoutPredicate_OneChord_Processing_Length([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            action: c => ((Chord)c).Length = 100,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOffEvent { DeltaTime = 100 },
            });

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithoutPredicate_OneChord_Processing_RemoveNote([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            action: c => ((Chord)c).Notes.Remove(((Chord)c).Notes.First()),
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            hint: ObjectProcessingHint.NotesCollectionCanBeChanged);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithoutPredicate_OneChord_Processing_AddNote([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            action: c => ((Chord)c).Notes.Add(new Note((SevenBitNumber)70, 0, 0)),
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity),
                new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity),
            },
            hint: ObjectProcessingHint.NotesCollectionCanBeChanged,
            newReferencesAllowed: true);

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithoutPredicate_OneChord_Processing_TimeAndLength([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A") { DeltaTime = 80 },
                new ControlChangeEvent { DeltaTime = 20 },
            },
            action: c =>
            {
                c.Time = 90;
                ((Chord)c).Length = 100;
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A") { DeltaTime = 80 },
                new NoteOnEvent { DeltaTime = 10 },
                new ControlChangeEvent { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 90 },
            });

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithoutPredicate_MultipleChords_Processing_1([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            action: c => ((Chord)c).Channel = (FourBitNumber)7,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Channel = (FourBitNumber)7 },
                new NoteOffEvent { Channel = (FourBitNumber)7 },
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)7 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { Channel = (FourBitNumber)7 },
                new NoteOnEvent { Channel = (FourBitNumber)7 },
                new NoteOffEvent { Channel = (FourBitNumber)7 },
            });

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithoutPredicate_MultipleChords_Processing_2([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new TextEvent("B"),
                new NoteOffEvent(),
                new TextEvent("C"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("D"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new TextEvent("E"),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
                new TextEvent("F"),
            },
            action: c => ((Chord)c).Channel = (FourBitNumber)7,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Channel = (FourBitNumber)7 },
                new TextEvent("A"),
                new NoteOnEvent { Channel = (FourBitNumber)7 },
                new TextEvent("B"),
                new NoteOffEvent { Channel = (FourBitNumber)7 },
                new TextEvent("C"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Channel = (FourBitNumber)7 },
                new TextEvent("D"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { Channel = (FourBitNumber)7 },
                new TextEvent("E"),
                new NoteOffEvent { Channel = (FourBitNumber)7 },
                new TextEvent("F"),
            });

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithoutPredicate_MultipleChords_Processing_3([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            action: c => c.Time = 100,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            });

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithoutPredicate_MultipleChords_Processing_4([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent(),
                new TextEvent("A"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new TextEvent("B"),
            },
            action: c =>
            {
                c.Time = 90;
                ((Chord)c).Length = 100;
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent { DeltaTime = 90 },
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOnEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("B") { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 90 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent(),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            });

        [Test]
        public void ProcessObjects_Chords_EventsCollection_WithoutPredicate_MultipleChords_Processing_5([Values] ContainerType containerType) => ProcessObjects_Chords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new TextEvent("A"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            action: c => ((Chord)c).Velocity = (SevenBitNumber)70,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                new NoteOnEvent { Channel = (FourBitNumber)4, Velocity = (SevenBitNumber)70 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Velocity = (SevenBitNumber)70 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
                new NoteOnEvent { DeltaTime = 100, Velocity = (SevenBitNumber)70 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Velocity = (SevenBitNumber)70 },
                new TextEvent("A"),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            });

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_EmptyCollection([Values] bool wrapToFile, [Values] bool predicateValue) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new MidiEvent[0][],
            action: c => { },
            match: c => predicateValue,
            expectedMidiEvents: new MidiEvent[0][],
            expectedProcessedCount: 0);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_EmptyTrackChunks([Values] bool wrapToFile, [Values] bool predicateValue) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[0],
                new MidiEvent[0],
            },
            action: c => { },
            match: c => predicateValue,
            expectedMidiEvents: new[]
            {
                new MidiEvent[0],
                new MidiEvent[0],
            },
            expectedProcessedCount: 0);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_NoNotes_NoProcessing([Values] bool wrapToFile, [Values] bool predicateValue) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                },
                new MidiEvent[]
                {
                    new TextEvent("B"),
                    new TextEvent("C"),
                },
            },
            action: c => { },
            match: c => predicateValue,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                },
                new MidiEvent[]
                {
                    new TextEvent("B"),
                    new TextEvent("C"),
                },
            },
            expectedProcessedCount: 0);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_NoNotes_Processing([Values] bool wrapToFile, [Values] bool predicateValue) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            },
            action: c => c.Time = 100,
            match: c => predicateValue,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            },
            expectedProcessedCount: 0);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_OneChord_NotMatched_NoProcessing([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                    new NoteOffEvent(),
                },
            },
            action: c => { },
            match: c => false,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                    new NoteOffEvent(),
                },
            },
            expectedProcessedCount: 0);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_OneChord_NotMatched_Processing([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                    new NoteOffEvent(),
                },
            },
            action: c => c.Time = 100,
            match: c => false,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                    new NoteOffEvent(),
                },
            },
            expectedProcessedCount: 0);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_OneChord_Matched_NoProcessing([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B"),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            },
            action: c => { },
            match: c => true,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B"),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_OneChord_Matched_Processing_AddNote_1([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("C"),
                    new NoteOffEvent(),
                },
            },
            action: c => ((Chord)c).Notes.Add(new Note((SevenBitNumber)70)),
            match: c => true,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity),
                    new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity),
                },
            },
            expectedProcessedCount: 1,
            hint: ObjectProcessingHint.NotesCollectionCanBeChanged,
            newReferencesAllowed: true);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_OneChord_Matched_Processing_AddNote_2([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("C"),
                    new NoteOffEvent(),
                },
            },
            action: c => ((Chord)c).Notes.Add(new Note((SevenBitNumber)70)),
            match: c => true,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity),
                    new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity),
                    new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity),
                },
            },
            expectedProcessedCount: 2,
            hint: ObjectProcessingHint.NotesCollectionCanBeChanged,
            newReferencesAllowed: true);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_OneChord_Matched_Processing_RemoveNote([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("C"),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, Note.DefaultVelocity),
                    new NoteOffEvent((SevenBitNumber)70, Note.DefaultOffVelocity),
                },
            },
            action: c => ((Chord)c).Notes.Remove(((Chord)c).Notes.Last()),
            match: c => true,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("C"),
                    new NoteOffEvent(),
                },
            },
            expectedProcessedCount: 1,
            hint: ObjectProcessingHint.NotesCollectionCanBeChanged);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_OneChord_Matched_Processing_Channel([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("C"),
                    new NoteOffEvent(),
                },
            },
            action: c => ((Chord)c).Channel = (FourBitNumber)7,
            match: c => true,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { Channel = (FourBitNumber)7 },
                    new TextEvent("C"),
                    new NoteOffEvent { Channel = (FourBitNumber)7 },
                },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_OneChord_Matched_Processing_Velocity([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            },
            action: c => ((Chord)c).Velocity = (SevenBitNumber)70,
            match: c => true,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_OneChord_Matched_Processing_OffVelocity([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent(),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            },
            action: c => ((Chord)c).OffVelocity = (SevenBitNumber)70,
            match: c => true,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent { Velocity = (SevenBitNumber)70 },
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_OneChord_Matched_Processing_Time([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent(),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            },
            action: c => c.Time = 100,
            match: c => true,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_OneChord_Matched_Processing_Length([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent(),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            },
            action: c => ((Chord)c).Length = 100,
            match: c => true,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new TextEvent("B"),
                    new NoteOffEvent { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_OneChord_Matched_Processing_TimeAndLength([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new TextEvent("A") { DeltaTime = 70 },
                    new TextEvent("B") { DeltaTime = 40 },
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            },
            action: c =>
            {
                c.Time = 100;
                ((Chord)c).Length = 50;
            },
            match: c => true,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A") { DeltaTime = 70 },
                    new NoteOnEvent { DeltaTime = 30 },
                    new TextEvent("B") { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 40 },
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_MultipleChords_NotMatched_NoProcessing([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent(),
                },
            },
            action: c => { },
            match: c => false,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent(),
                },
            },
            expectedProcessedCount: 0);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_MultipleChords_NotMatched_Processing([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent(),
                    new TextEvent("B"),
                },
            },
            action: c => ((Chord)c).Channel = (FourBitNumber)7,
            match: c => false,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent(),
                    new TextEvent("B"),
                },
            },
            expectedProcessedCount: 0);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_MultipleChords_SomeMatched_NoProcessing([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent(),
                    new TextEvent("B"),
                },
            },
            action: c => { },
            match: c => c.Time == 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent(),
                    new TextEvent("B"),
                },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_MultipleChords_SomeMatched_Processing_AddNote([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent(),
                    new NoteOnEvent { Channel = (FourBitNumber)4, DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 40 },
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent(),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
            },
            action: c => ((Chord)c).Notes.Add(new Note((SevenBitNumber)90)),
            match: c => c.Time == 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity),
                    new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity),
                    new NoteOnEvent { Channel = (FourBitNumber)4, DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 40 },
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent(),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)90, Note.DefaultVelocity),
                    new NoteOffEvent((SevenBitNumber)90, Note.DefaultOffVelocity),
                },
            },
            expectedProcessedCount: 2,
            hint: ObjectProcessingHint.NotesCollectionCanBeChanged,
            newReferencesAllowed: true);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_MultipleChords_SomeMatched_Processing_1([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent(),
                    new TextEvent("C"),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent(),
                    new TextEvent("B"),
                },
            },
            action: c => c.Time = 10,
            match: c => c.Time == 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("C"),
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent(),
                    new TextEvent("B"),
                },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_MultipleChords_SomeMatched_Processing_2([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent(),
                    new NoteOnEvent { Channel = (FourBitNumber)4, DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 40 },
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent(),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
            },
            action: c => ((Chord)c).Velocity = (SevenBitNumber)70,
            match: c => c.Time == 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                    new NoteOnEvent { Channel = (FourBitNumber)4, DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 40 },
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent(),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
            },
            expectedProcessedCount: 2);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_MultipleChords_SomeMatched_Processing_3([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent(),
                    new NoteOnEvent { Channel = (FourBitNumber)4, DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 40 },
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent(),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
            },
            action: c => c.Time = 20,
            match: c => c.Time == 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { Channel = (FourBitNumber)4, DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent(),
                    new NoteOnEvent { DeltaTime = 30 },
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent(),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 20 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 20 },
                    new NoteOffEvent(),
                },
            },
            expectedProcessedCount: 2);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_MultipleChords_SomeMatched_Processing_4([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                    new TextEvent("C"),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent(),
                    new TextEvent("D"),
                    new NoteOnEvent { Channel = (FourBitNumber)4, DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 40 },
                    new TextEvent("E"),
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent(),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new TextEvent("F"),
                },
            },
            action: c => c.Time = 20,
            match: c => c.Time == 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                    new TextEvent("C"),
                },
                new MidiEvent[]
                {
                    new TextEvent("D"),
                    new NoteOnEvent { Channel = (FourBitNumber)4, DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent(),
                    new NoteOnEvent { DeltaTime = 30 },
                    new TextEvent("E"),
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent(),
                    new TextEvent("F"),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 20 },
                },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_MultipleChords_SomeMatched_Processing_5([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new TextEvent("B"),
                    new NoteOffEvent(),
                    new TextEvent("C"),
                    new TextEvent("D"),
                    new NoteOnEvent { Channel = (FourBitNumber)4, DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 40 },
                    new TextEvent("E"),
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent(),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new TextEvent("F"),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
            },
            action: c => c.Time = 20,
            match: c => c.Time > 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new TextEvent("B"),
                    new NoteOffEvent(),
                    new TextEvent("C"),
                    new TextEvent("D"),
                    new NoteOnEvent { Channel = (FourBitNumber)4, DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new TextEvent("E") { DeltaTime = 30 },
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new TextEvent("F"),
                    new NoteOffEvent { Channel = (FourBitNumber)4, DeltaTime = 10 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
            },
            expectedProcessedCount: 2);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_MultipleChords_AllMatched_Processing_1([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent(),
                    new TextEvent("C"),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent(),
                    new TextEvent("B"),
                },
            },
            action: c => c.Time = 10,
            match: c => true,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("C"),
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent(),
                    new TextEvent("B") { DeltaTime = 40 },
                },
            },
            expectedProcessedCount: 2);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_MultipleChords_AllMatched_Processing_2([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent(),
                    new NoteOnEvent { Channel = (FourBitNumber)4, DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 40 },
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent(),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
            },
            action: c => ((Chord)c).Velocity = (SevenBitNumber)70,
            match: c => c.Time >= 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                    new NoteOnEvent { Channel = (FourBitNumber)4, DeltaTime = 10, Velocity = (SevenBitNumber)70 },
                    new NoteOnEvent { DeltaTime = 40, Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent(),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
            },
            expectedProcessedCount: 4);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_MultipleChords_AllMatched_Processing_3([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent(),
                    new NoteOnEvent { Channel = (FourBitNumber)4, DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 40 },
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent(),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
            },
            action: c => c.Time = 20,
            match: c => true,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 20 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent(),
                    new NoteOnEvent { Channel = (FourBitNumber)4 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOffEvent { Channel = (FourBitNumber)4, DeltaTime = 40 },
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 10 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 20 },
                    new NoteOffEvent(),
                },
            },
            expectedProcessedCount: 4);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithPredicate_MultipleChords_AllMatched_Processing_4([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("C"),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("B"),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent(),
                    new TextEvent("D"),
                    new NoteOnEvent { Channel = (FourBitNumber)4, DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 40 },
                    new TextEvent("E"),
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent(),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new TextEvent("F"),
                },
            },
            action: c => c.Time = 20,
            match: c => true,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("C"),
                },
                new MidiEvent[]
                {
                    new TextEvent("B"),
                    new TextEvent("D"),
                    new NoteOnEvent { DeltaTime = 20 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent(),
                    new NoteOnEvent { Channel = (FourBitNumber)4 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new TextEvent("E") { DeltaTime = 30 },
                    new TextEvent("F"),
                    new NoteOffEvent { Channel = (FourBitNumber)4, DeltaTime = 10 },
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 10 },
                },
            },
            expectedProcessedCount: 3);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithoutPredicate_EmptyCollection([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithoutPredicate(
            wrapToFile,
            midiEvents: new MidiEvent[0][],
            action: c => { },
            expectedMidiEvents: new MidiEvent[0][]);

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithoutPredicate_EmptyTrackChunks([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithoutPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[0],
                new MidiEvent[0],
            },
            action: c => { },
            expectedMidiEvents: new[]
            {
                new MidiEvent[0],
                new MidiEvent[0],
            });

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithoutPredicate_NoNotes_NoProcessing([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithoutPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                },
                new MidiEvent[]
                {
                    new TextEvent("B"),
                    new TextEvent("C"),
                },
            },
            action: c => { },
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                },
                new MidiEvent[]
                {
                    new TextEvent("B"),
                    new TextEvent("C"),
                },
            });

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithoutPredicate_NoNotes_Processing([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithoutPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            },
            action: c => c.Time = 100,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            });

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithoutPredicate_OneChord_NoProcessing([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithoutPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B"),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            },
            action: c => { },
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new TextEvent("B"),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            });

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithoutPredicate_OneChord_Processing_Channel([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithoutPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("C"),
                    new NoteOffEvent(),
                },
            },
            action: c => ((Chord)c).Channel = (FourBitNumber)7,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { Channel = (FourBitNumber)7 },
                    new TextEvent("C"),
                    new NoteOffEvent { Channel = (FourBitNumber)7 },
                },
            });

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithoutPredicate_OneChord_Processing_Velocity([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithoutPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            },
            action: c => ((Chord)c).Velocity = (SevenBitNumber)70,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            });

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithoutPredicate_OneChord_Processing_OffVelocity([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithoutPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent(),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            },
            action: c => ((Chord)c).OffVelocity = (SevenBitNumber)70,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent { Velocity = (SevenBitNumber)70 },
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            });

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithoutPredicate_OneChord_Processing_Time([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithoutPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent(),
                    new TextEvent("B"),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            },
            action: c => c.Time = 100,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                    new NoteOnEvent { DeltaTime = 100 },
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            });

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithoutPredicate_OneChord_Processing_Length([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithoutPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new TextEvent("B"),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            },
            action: c => ((Chord)c).Length = 100,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new TextEvent("B"),
                    new NoteOffEvent { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            });

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithoutPredicate_OneChord_Processing_TimeAndLength([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithoutPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new TextEvent("A") { DeltaTime = 70 },
                    new TextEvent("B") { DeltaTime = 40 },
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            },
            action: c =>
            {
                c.Time = 100;
                ((Chord)c).Length = 50;
            },
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A") { DeltaTime = 70 },
                    new NoteOnEvent { DeltaTime = 30 },
                    new TextEvent("B") { DeltaTime = 10 },
                    new NoteOffEvent { DeltaTime = 40 },
                },
                new MidiEvent[]
                {
                    new TextEvent("C"),
                },
            });

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithoutPredicate_MultipleChords_Processing_1([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithoutPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A"),
                    new NoteOffEvent(),
                    new TextEvent("C"),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent(),
                    new TextEvent("B"),
                },
            },
            action: c => c.Time = 10,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("C"),
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 10 },
                    new NoteOffEvent(),
                    new TextEvent("B") { DeltaTime = 40 },
                },
            });

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithoutPredicate_MultipleChords_Processing_2([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithoutPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent(),
                    new NoteOnEvent { Channel = (FourBitNumber)4, DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 40 },
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent(),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
            },
            action: c => ((Chord)c).Velocity = (SevenBitNumber)70,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { Velocity = (SevenBitNumber)70 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent(),
                    new NoteOnEvent { Channel = (FourBitNumber)4, DeltaTime = 10, Velocity = (SevenBitNumber)70 },
                    new NoteOnEvent { DeltaTime = 40, Velocity = (SevenBitNumber)70 },
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent(),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
            });

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithoutPredicate_MultipleChords_Processing_3([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithoutPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent(),
                    new NoteOnEvent { Channel = (FourBitNumber)4, DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 40 },
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent(),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                },
            },
            action: c => c.Time = 20,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 20 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent(),
                    new NoteOnEvent { Channel = (FourBitNumber)4 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOffEvent { Channel = (FourBitNumber)4, DeltaTime = 40 },
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 10 },
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 20 },
                    new NoteOffEvent(),
                },
            });

        [Test]
        public void ProcessObjects_Chords_TrackChunks_WithoutPredicate_MultipleChords_Processing_4([Values] bool wrapToFile) => ProcessObjects_Chords_TrackChunks_WithoutPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                    new TextEvent("C"),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent(),
                    new TextEvent("D"),
                    new NoteOnEvent { Channel = (FourBitNumber)4, DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 40 },
                    new TextEvent("E"),
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent(),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new TextEvent("F"),
                },
            },
            action: c => c.Time = 20,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                    new TextEvent("C"),
                },
                new MidiEvent[]
                {
                    new TextEvent("D"),
                    new NoteOnEvent { DeltaTime = 20 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent(),
                    new NoteOnEvent { Channel = (FourBitNumber)4 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new TextEvent("E") { DeltaTime = 30 },
                    new TextEvent("F"),
                    new NoteOffEvent { Channel = (FourBitNumber)4, DeltaTime = 10 },
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 10 },
                },
            });

        #endregion

        #region Private methods

        private void ProcessObjects_Chords_EventsCollection_WithPredicate(
            ContainerType containerType,
            ICollection<MidiEvent> midiEvents,
            Action<ITimedObject> action,
            Predicate<ITimedObject> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedProcessedCount,
            ObjectProcessingHint hint = ObjectProcessingHint.Default,
            bool newReferencesAllowed = false) => ProcessObjects_EventsCollection_WithPredicate(
                containerType,
                ObjectType.Chord,
                midiEvents,
                action,
                match,
                expectedMidiEvents,
                expectedProcessedCount,
                hint,
                newReferencesAllowed);

        private void ProcessObjects_Chords_EventsCollection_WithoutPredicate(
            ContainerType containerType,
            ICollection<MidiEvent> midiEvents,
            Action<ITimedObject> action,
            ICollection<MidiEvent> expectedMidiEvents,
            ObjectProcessingHint hint = ObjectProcessingHint.Default,
            bool newReferencesAllowed = false) => ProcessObjects_EventsCollection_WithoutPredicate(
                containerType,
                ObjectType.Chord,
                midiEvents,
                action,
                expectedMidiEvents,
                hint,
                newReferencesAllowed);

        private void ProcessObjects_Chords_TrackChunks_WithPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<ITimedObject> action,
            Predicate<ITimedObject> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedProcessedCount,
            ObjectProcessingHint hint = ObjectProcessingHint.Default,
            bool newReferencesAllowed = false) => ProcessObjects_TrackChunks_WithPredicate(
                ObjectType.Chord,
                wrapToFile,
                midiEvents,
                action,
                match,
                expectedMidiEvents,
                expectedProcessedCount,
                hint,
                newReferencesAllowed);

        private void ProcessObjects_Chords_TrackChunks_WithoutPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<ITimedObject> action,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            ObjectProcessingHint hint = ObjectProcessingHint.Default,
            bool newReferencesAllowed = false) => ProcessObjects_TrackChunks_WithoutPredicate(
                ObjectType.Chord,
                wrapToFile,
                midiEvents,
                action,
                expectedMidiEvents,
                hint,
                newReferencesAllowed);

        #endregion
    }
}
