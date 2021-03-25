using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Interaction
{
    [TestFixture]
    public sealed partial class ChordsManagingUtilitiesTests
    {
        #region Test methods

        [Test]
        public void ProcessChords_EventsCollection_WithPredicate_EmptyCollection([Values] ContainerType containerType, [Values] bool predicateValue) => ProcessChords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[0],
            action: c => { },
            match: c => predicateValue,
            expectedMidiEvents: new MidiEvent[0],
            expectedProcessedCount: 0);

        [Test]
        public void ProcessChords_EventsCollection_WithPredicate_NoNotes_NoProcessing([Values] ContainerType containerType, [Values] bool predicateValue) => ProcessChords_EventsCollection_WithPredicate(
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
        public void ProcessChords_EventsCollection_WithPredicate_NoNotes_Processing([Values] ContainerType containerType, [Values] bool predicateValue) => ProcessChords_EventsCollection_WithPredicate(
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
        public void ProcessChords_EventsCollection_WithPredicate_NoChords_NoProcessing([Values] ContainerType containerType, [Values] bool predicateValue) => ProcessChords_EventsCollection_WithPredicate(
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
        public void ProcessChords_EventsCollection_WithPredicate_NoChords_Processing([Values] ContainerType containerType, [Values] bool predicateValue) => ProcessChords_EventsCollection_WithPredicate(
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
        public void ProcessChords_EventsCollection_WithPredicate_OneChord_NotMatched([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithPredicate(
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
        public void ProcessChords_EventsCollection_WithPredicate_OneChord_Matched_NoProcessing([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithPredicate(
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
        public void ProcessChords_EventsCollection_WithPredicate_OneChord_Matched_Processing_Channel([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new ControlChangeEvent(),
                new NoteOffEvent(),
            },
            action: c => c.Channel = (FourBitNumber)4,
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
        public void ProcessChords_EventsCollection_WithPredicate_OneChord_Matched_Processing_Velocity([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            action: c => c.Velocity = (SevenBitNumber)40,
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
        public void ProcessChords_EventsCollection_WithPredicate_OneChord_Matched_Processing_OffVelocity([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            action: c => c.OffVelocity = (SevenBitNumber)40,
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
        public void ProcessChords_EventsCollection_WithPredicate_OneChord_Matched_Processing_Time([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithPredicate(
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
        public void ProcessChords_EventsCollection_WithPredicate_OneChord_Matched_Processing_Length([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            action: c => c.Length = 100,
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
        public void ProcessChords_EventsCollection_WithPredicate_OneChord_Matched_Processing_TimeAndLength([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithPredicate(
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
                c.Length = 100;
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
        public void ProcessChords_EventsCollection_WithPredicate_MultipleChords_NotMatched_NoProcessing([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithPredicate(
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
        public void ProcessChords_EventsCollection_WithPredicate_MultipleChords_NotMatched_Processing([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithPredicate(
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
        public void ProcessChords_EventsCollection_WithPredicate_MultipleChords_SomeMatched_NoProcessing([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithPredicate(
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
        public void ProcessChords_EventsCollection_WithPredicate_MultipleChords_SomeMatched_Processing_1([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithPredicate(
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
            action: c => c.Channel = (FourBitNumber)7,
            match: c => c.Channel == 0,
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
        public void ProcessChords_EventsCollection_WithPredicate_MultipleChords_SomeMatched_Processing_2([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithPredicate(
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
            action: c => c.Channel = (FourBitNumber)7,
            match: c => c.Channel == 0,
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
        public void ProcessChords_EventsCollection_WithPredicate_MultipleChords_SomeMatched_Processing_3([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithPredicate(
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
            match: c => c.Channel == 0,
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
        public void ProcessChords_EventsCollection_WithPredicate_MultipleChords_SomeMatched_Processing_4([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithPredicate(
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
                c.Length = 100;
            },
            match: c => c.Channel == 4,
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
        public void ProcessChords_EventsCollection_WithPredicate_MultipleChords_SomeMatched_Processing_5([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithPredicate(
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
            action: c => c.Velocity = (SevenBitNumber)70,
            match: c => c.Channel == 0,
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
        public void ProcessChords_EventsCollection_WithPredicate_MultipleChords_AllMatched_Processing_1([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithPredicate(
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
            action: c => c.Channel = (FourBitNumber)7,
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
        public void ProcessChords_EventsCollection_WithPredicate_MultipleChords_AllMatched_Processing_2([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithPredicate(
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
            action: c => c.Channel = (FourBitNumber)7,
            match: c => c.Channel < 10,
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
        public void ProcessChords_EventsCollection_WithPredicate_MultipleChords_AllMatched_Processing_3([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithPredicate(
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
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            expectedProcessedCount: 2);

        [Test]
        public void ProcessChords_EventsCollection_WithPredicate_MultipleChords_AllMatched_Processing_4([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithPredicate(
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
                c.Length = 100;
            },
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent { DeltaTime = 90 },
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
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
        public void ProcessChords_EventsCollection_WithPredicate_MultipleChords_AllMatched_Processing_5([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithPredicate(
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
            action: c => c.Velocity = (SevenBitNumber)70,
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
        public void ProcessChords_EventsCollection_WithoutPredicate_EmptyCollection([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[0],
            action: c => { },
            expectedMidiEvents: new MidiEvent[0]);

        [Test]
        public void ProcessChords_EventsCollection_WithoutPredicate_NoNotes_NoProcessing([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithoutPredicate(
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
        public void ProcessChords_EventsCollection_WithoutPredicate_NoNotes_Processing([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithoutPredicate(
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
        public void ProcessChords_EventsCollection_WithoutPredicate_NoChords_NoProcessing([Values] ContainerType containerType, [Values] bool predicateValue) => ProcessChords_EventsCollection_WithoutPredicate(
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
        public void ProcessChords_EventsCollection_WithoutPredicate_NoChords_Processing([Values] ContainerType containerType, [Values] bool predicateValue) => ProcessChords_EventsCollection_WithoutPredicate(
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
        public void ProcessChords_EventsCollection_WithoutPredicate_OneChord_NoProcessing([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithoutPredicate(
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
        public void ProcessChords_EventsCollection_WithoutPredicate_OneChord_Processing_Channel([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new ControlChangeEvent(),
                new NoteOffEvent(),
            },
            action: c => c.Channel = (FourBitNumber)4,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new ControlChangeEvent(),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            });

        [Test]
        public void ProcessChords_EventsCollection_WithoutPredicate_OneChord_Processing_Velocity([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            action: c => c.Velocity = (SevenBitNumber)40,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOnEvent { Velocity = (SevenBitNumber)40 },
                new NoteOffEvent(),
            });

        [Test]
        public void ProcessChords_EventsCollection_WithoutPredicate_OneChord_Processing_OffVelocity([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            action: c => c.OffVelocity = (SevenBitNumber)40,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOnEvent(),
                new NoteOffEvent { Velocity = (SevenBitNumber)40 },
            });

        [Test]
        public void ProcessChords_EventsCollection_WithoutPredicate_OneChord_Processing_Time([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithoutPredicate(
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
        public void ProcessChords_EventsCollection_WithoutPredicate_OneChord_Processing_Length([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            action: c => c.Length = 100,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOffEvent { DeltaTime = 100 },
            });

        [Test]
        public void ProcessChords_EventsCollection_WithoutPredicate_OneChord_Processing_TimeAndLength([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithoutPredicate(
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
                c.Length = 100;
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A") { DeltaTime = 80 },
                new NoteOnEvent { DeltaTime = 10 },
                new ControlChangeEvent { DeltaTime = 10 },
                new NoteOffEvent { DeltaTime = 90 },
            });

        [Test]
        public void ProcessChords_EventsCollection_WithoutPredicate_MultipleChords_Processing_1([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithoutPredicate(
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
            action: c => c.Channel = (FourBitNumber)7,
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
        public void ProcessChords_EventsCollection_WithoutPredicate_MultipleChords_Processing_2([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithoutPredicate(
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
            action: c => c.Channel = (FourBitNumber)7,
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
        public void ProcessChords_EventsCollection_WithoutPredicate_MultipleChords_Processing_3([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithoutPredicate(
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
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            });

        [Test]
        public void ProcessChords_EventsCollection_WithoutPredicate_MultipleChords_Processing_4([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithoutPredicate(
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
                c.Length = 100;
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent { DeltaTime = 90 },
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
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
        public void ProcessChords_EventsCollection_WithoutPredicate_MultipleChords_Processing_5([Values] ContainerType containerType) => ProcessChords_EventsCollection_WithoutPredicate(
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
            action: c => c.Velocity = (SevenBitNumber)70,
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
        public void ProcessChords_TrackChunks_WithPredicate_EmptyCollection([Values] bool wrapToFile, [Values] bool predicateValue) => ProcessChords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new MidiEvent[0][],
            action: c => { },
            match: c => predicateValue,
            expectedMidiEvents: new MidiEvent[0][],
            expectedProcessedCount: 0);

        [Test]
        public void ProcessChords_TrackChunks_WithPredicate_EmptyTrackChunks([Values] bool wrapToFile, [Values] bool predicateValue) => ProcessChords_TrackChunks_WithPredicate(
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
        public void ProcessChords_TrackChunks_WithPredicate_NoNotes_NoProcessing([Values] bool wrapToFile, [Values] bool predicateValue) => ProcessChords_TrackChunks_WithPredicate(
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
        public void ProcessChords_TrackChunks_WithPredicate_NoNotes_Processing([Values] bool wrapToFile, [Values] bool predicateValue) => ProcessChords_TrackChunks_WithPredicate(
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
        public void ProcessChords_TrackChunks_WithPredicate_OneChord_NotMatched_NoProcessing([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithPredicate(
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
        public void ProcessChords_TrackChunks_WithPredicate_OneChord_NotMatched_Processing([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithPredicate(
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
        public void ProcessChords_TrackChunks_WithPredicate_OneChord_Matched_NoProcessing([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithPredicate(
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
        public void ProcessChords_TrackChunks_WithPredicate_OneChord_Matched_Processing_Channel([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithPredicate(
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
            action: c => c.Channel = (FourBitNumber)7,
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
        public void ProcessChords_TrackChunks_WithPredicate_OneChord_Matched_Processing_Velocity([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithPredicate(
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
            action: c => c.Velocity = (SevenBitNumber)70,
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
        public void ProcessChords_TrackChunks_WithPredicate_OneChord_Matched_Processing_OffVelocity([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithPredicate(
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
            action: c => c.OffVelocity = (SevenBitNumber)70,
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
        public void ProcessChords_TrackChunks_WithPredicate_OneChord_Matched_Processing_Time([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithPredicate(
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
        public void ProcessChords_TrackChunks_WithPredicate_OneChord_Matched_Processing_Length([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithPredicate(
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
            action: c => c.Length = 100,
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
        public void ProcessChords_TrackChunks_WithPredicate_OneChord_Matched_Processing_TimeAndLength([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithPredicate(
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
                c.Length = 50;
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
        public void ProcessChords_TrackChunks_WithPredicate_MultipleChords_NotMatched_NoProcessing([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithPredicate(
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
        public void ProcessChords_TrackChunks_WithPredicate_MultipleChords_NotMatched_Processing([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithPredicate(
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
            action: c => c.Channel = (FourBitNumber)7,
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
        public void ProcessChords_TrackChunks_WithPredicate_MultipleChords_SomeMatched_NoProcessing([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithPredicate(
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
        public void ProcessChords_TrackChunks_WithPredicate_MultipleChords_SomeMatched_Processing_1([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithPredicate(
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
        public void ProcessChords_TrackChunks_WithPredicate_MultipleChords_SomeMatched_Processing_2([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithPredicate(
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
            action: c => c.Velocity = (SevenBitNumber)70,
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
        public void ProcessChords_TrackChunks_WithPredicate_MultipleChords_SomeMatched_Processing_3([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithPredicate(
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
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent { Channel = (FourBitNumber)4, DeltaTime = 30 },
                    new NoteOnEvent(),
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
        public void ProcessChords_TrackChunks_WithPredicate_MultipleChords_SomeMatched_Processing_4([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithPredicate(
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
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent { Channel = (FourBitNumber)4, DeltaTime = 30 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new TextEvent("E"),
                    new TextEvent("F"),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 20 },
                },
            },
            expectedProcessedCount: 1);

        [Test]
        public void ProcessChords_TrackChunks_WithPredicate_MultipleChords_SomeMatched_Processing_5([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithPredicate(
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
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new TextEvent("B"),
                    new TextEvent("C"),
                    new TextEvent("D"),
                    new NoteOnEvent { Channel = (FourBitNumber)4, DeltaTime = 20 },
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 30 },
                    new TextEvent("E"),
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
        public void ProcessChords_TrackChunks_WithPredicate_MultipleChords_AllMatched_Processing_1([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithPredicate(
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
        public void ProcessChords_TrackChunks_WithPredicate_MultipleChords_AllMatched_Processing_2([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithPredicate(
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
            action: c => c.Velocity = (SevenBitNumber)70,
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
        public void ProcessChords_TrackChunks_WithPredicate_MultipleChords_AllMatched_Processing_3([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithPredicate(
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
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
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
        public void ProcessChords_TrackChunks_WithPredicate_MultipleChords_AllMatched_Processing_4([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithPredicate(
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
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
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
        public void ProcessChords_TrackChunks_WithoutPredicate_EmptyCollection([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithoutPredicate(
            wrapToFile,
            midiEvents: new MidiEvent[0][],
            action: c => { },
            expectedMidiEvents: new MidiEvent[0][]);

        [Test]
        public void ProcessChords_TrackChunks_WithoutPredicate_EmptyTrackChunks([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithoutPredicate(
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
        public void ProcessChords_TrackChunks_WithoutPredicate_NoNotes_NoProcessing([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithoutPredicate(
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
        public void ProcessChords_TrackChunks_WithoutPredicate_NoNotes_Processing([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithoutPredicate(
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
        public void ProcessChords_TrackChunks_WithoutPredicate_OneChord_NoProcessing([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithoutPredicate(
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
        public void ProcessChords_TrackChunks_WithoutPredicate_OneChord_Processing_Channel([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithoutPredicate(
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
            action: c => c.Channel = (FourBitNumber)7,
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
        public void ProcessChords_TrackChunks_WithoutPredicate_OneChord_Processing_Velocity([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithoutPredicate(
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
            action: c => c.Velocity = (SevenBitNumber)70,
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
        public void ProcessChords_TrackChunks_WithoutPredicate_OneChord_Processing_OffVelocity([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithoutPredicate(
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
            action: c => c.OffVelocity = (SevenBitNumber)70,
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
        public void ProcessChords_TrackChunks_WithoutPredicate_OneChord_Processing_Time([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithoutPredicate(
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
        public void ProcessChords_TrackChunks_WithoutPredicate_OneChord_Processing_Length([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithoutPredicate(
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
            action: c => c.Length = 100,
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
        public void ProcessChords_TrackChunks_WithoutPredicate_OneChord_Processing_TimeAndLength([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithoutPredicate(
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
                c.Length = 50;
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
        public void ProcessChords_TrackChunks_WithoutPredicate_MultipleChords_Processing_1([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithoutPredicate(
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
        public void ProcessChords_TrackChunks_WithoutPredicate_MultipleChords_Processing_2([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithoutPredicate(
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
            action: c => c.Velocity = (SevenBitNumber)70,
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
        public void ProcessChords_TrackChunks_WithoutPredicate_MultipleChords_Processing_3([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithoutPredicate(
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
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
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
        public void ProcessChords_TrackChunks_WithoutPredicate_MultipleChords_Processing_4([Values] bool wrapToFile) => ProcessChords_TrackChunks_WithoutPredicate(
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
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
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

        private void ProcessChords_EventsCollection_WithPredicate(
            ContainerType containerType,
            ICollection<MidiEvent> midiEvents,
            Action<Chord> action,
            Predicate<Chord> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedProcessedCount)
        {
            var eventsCollection = new EventsCollection();
            eventsCollection.AddRange(midiEvents);

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        Assert.AreEqual(
                            expectedProcessedCount,
                            eventsCollection.ProcessChords(action, match),
                            "Invalid count of processed chords.");

                        var expectedEventsCollection = new EventsCollection();
                        expectedEventsCollection.AddRange(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
                        Assert.IsTrue(
                            eventsCollection.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunk:
                    {
                        var trackChunk = new TrackChunk(eventsCollection);

                        Assert.AreEqual(
                            expectedProcessedCount,
                            trackChunk.ProcessChords(action, match),
                            "Invalid count of processed chords.");

                        var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
                        Assert.IsTrue(
                            trackChunk.Events.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunks:
                case ContainerType.File:
                    {
                        ProcessChords_TrackChunks_WithPredicate(
                            containerType == ContainerType.File,
                            new[] { midiEvents },
                            action,
                            match,
                            new[] { expectedMidiEvents },
                            expectedProcessedCount);
                    }
                    break;
            }
        }

        private void ProcessChords_EventsCollection_WithoutPredicate(
            ContainerType containerType,
            ICollection<MidiEvent> midiEvents,
            Action<Chord> action,
            ICollection<MidiEvent> expectedMidiEvents)
        {
            var chordsCount = midiEvents.GetChords().Count;

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        var eventsCollection = new EventsCollection();
                        eventsCollection.AddRange(midiEvents);

                        Assert.AreEqual(
                            chordsCount,
                            eventsCollection.ProcessChords(action),
                            "Invalid count of processed chords.");

                        var expectedEventsCollection = new EventsCollection();
                        expectedEventsCollection.AddRange(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedEventsCollection, eventsCollection, true, "Events are invalid.");
                        Assert.IsTrue(
                            eventsCollection.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunk:
                    {
                        var trackChunk = new TrackChunk(midiEvents);

                        Assert.AreEqual(
                            chordsCount,
                            trackChunk.ProcessChords(action),
                            "Invalid count of processed chords.");

                        var expectedTrackChunk = new TrackChunk(expectedMidiEvents);
                        MidiAsserts.AreEqual(expectedTrackChunk, trackChunk, true, "Events are invalid.");
                        Assert.IsTrue(
                            trackChunk.Events.All(e => midiEvents.Any(ee => object.ReferenceEquals(e, ee))),
                            "There are new events references.");
                    }
                    break;
                case ContainerType.TrackChunks:
                    {
                        ProcessChords_TrackChunks_WithoutPredicate(
                            false,
                            new[] { midiEvents },
                            action,
                            new[] { expectedMidiEvents });
                    }
                    break;
                case ContainerType.File:
                    {
                        ProcessChords_TrackChunks_WithoutPredicate(
                            true,
                            new[] { midiEvents },
                            action,
                            new[] { expectedMidiEvents });
                    }
                    break;
            }
        }

        private void ProcessChords_TrackChunks_WithPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<Chord> action,
            Predicate<Chord> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedProcessedCount)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    expectedProcessedCount,
                    midiFile.ProcessChords(action, match),
                    "Invalid count of processed chords.");

                MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                Assert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                Assert.AreEqual(
                    expectedProcessedCount,
                    trackChunks.ProcessChords(action, match),
                    "Invalid count of processed chords.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                Assert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        private void ProcessChords_TrackChunks_WithoutPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Action<Chord> action,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();
            var chordsCount = trackChunks.GetChords().Count;

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    chordsCount,
                    midiFile.ProcessChords(action),
                    "Invalid count of processed chords.");

                MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                Assert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                Assert.AreEqual(
                    chordsCount,
                    trackChunks.ProcessChords(action),
                    "Invalid count of processed chords.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                Assert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        #endregion
    }
}
