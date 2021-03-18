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
    public sealed partial class NotesManagingUtilitiesTests
    {
        #region Test methods

        [Test]
        public void RemoveNotes_EventsCollection_WithPredicate_EmptyCollection([Values] ContainerType containerType, [Values] bool predicateValue) => RemoveNotes_EventsCollection_WithPredicate(
            containerType: containerType,
            midiEvents: new MidiEvent[0],
            match: n => predicateValue,
            expectedMidiEvents: new MidiEvent[0],
            expectedRemovedCount: 0);

        [Test]
        public void RemoveNotes_EventsCollection_WithPredicate_OneNote_NotMatched([Values] ContainerType containerType) => RemoveNotes_EventsCollection_WithPredicate(
            containerType: containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new NoteOffEvent(),
            },
            match: n => false,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new NoteOffEvent(),
            },
            expectedRemovedCount: 0);

        [Test]
        public void RemoveNotes_EventsCollection_WithPredicate_OneNote_Matched_1([Values] ContainerType containerType) => RemoveNotes_EventsCollection_WithPredicate(
            containerType: containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 50 },
                new TextEvent("A") { DeltaTime = 100 },
                new NoteOffEvent(),
            },
            match: n => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A") { DeltaTime = 150 },
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_EventsCollection_WithPredicate_OneNote_Matched_2([Values] ContainerType containerType) => RemoveNotes_EventsCollection_WithPredicate(
            containerType: containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A") { DeltaTime = 100 },
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            match: n => n.NoteNumber == 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A") { DeltaTime = 100 },
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_EventsCollection_WithPredicate_OneNote_Matched_3([Values] ContainerType containerType) => RemoveNotes_EventsCollection_WithPredicate(
            containerType: containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent { DeltaTime = 350 },
                new TextEvent("A") { DeltaTime = 100 },
            },
            match: n => n.Length == 350,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A") { DeltaTime = 450 },
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_EventsCollection_WithPredicate_OneNote_Matched_4([Values] ContainerType containerType) => RemoveNotes_EventsCollection_WithPredicate(
            containerType: containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new ControlChangeEvent { DeltaTime = 40 },
                new NoteOffEvent { DeltaTime = 100 },
            },
            match: n => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent { DeltaTime = 40 },
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_EventsCollection_WithPredicate_OneNote_Matched_5([Values] ContainerType containerType) => RemoveNotes_EventsCollection_WithPredicate(
            containerType: containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent { DeltaTime = 100 },
                new TextEvent("A"),
                new ControlChangeEvent { DeltaTime = 40 },
            },
            match: n => n.Length == 100,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A") { DeltaTime = 100 },
                new ControlChangeEvent { DeltaTime = 40 },
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_EventsCollection_WithPredicate_MultipleNotes_NotMatched([Values] ContainerType containerType) => RemoveNotes_EventsCollection_WithPredicate(
            containerType: containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            match: n => false,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            expectedRemovedCount: 0);

        [Test]
        public void RemoveNotes_EventsCollection_WithPredicate_MultipleNotes_AllMatched_1([Values] ContainerType containerType) => RemoveNotes_EventsCollection_WithPredicate(
            containerType: containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A"),
                new NoteOnEvent { DeltaTime = 100 },
                new ControlChangeEvent(),
                new NoteOffEvent { DeltaTime = 50 },
                new TextEvent("B"),
            },
            match: n => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent { DeltaTime = 100 },
                new TextEvent("B") { DeltaTime = 50 },
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveNotes_EventsCollection_WithPredicate_MultipleNotes_AllMatched_2([Values] ContainerType containerType) => RemoveNotes_EventsCollection_WithPredicate(
            containerType: containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            match: n => true,
            expectedMidiEvents: new MidiEvent[0],
            expectedRemovedCount: 2);

        [Test]
        public void RemoveNotes_EventsCollection_WithPredicate_MultipleNotes_SomeMatched_1([Values] ContainerType containerType) => RemoveNotes_EventsCollection_WithPredicate(
            containerType: containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 100 },
                new ControlChangeEvent(),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 50 },
                new TextEvent("B"),
            },
            match: n => n.NoteNumber == 70,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A"),
                new ControlChangeEvent { DeltaTime = 100 },
                new TextEvent("B") { DeltaTime = 50 },
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_EventsCollection_WithPredicate_MultipleNotes_SomeMatched_2([Values] ContainerType containerType) => RemoveNotes_EventsCollection_WithPredicate(
            containerType: containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            match: n => n.NoteNumber == 70,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_EventsCollection_WithPredicate_MultipleNotes_SomeMatched_3([Values] ContainerType containerType) => RemoveNotes_EventsCollection_WithPredicate(
            containerType: containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent { DeltaTime = 100 },
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            match: n => n.NoteNumber == 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 100 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveNotes_EventsCollection_WithPredicate_MultipleNotes_SomeMatched_4([Values] ContainerType containerType) => RemoveNotes_EventsCollection_WithPredicate(
            containerType: containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new NoteOffEvent { DeltaTime = 100 },
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("B"),
            },
            match: n => n.NoteNumber == 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue) { DeltaTime = 100 },
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new TextEvent("B"),
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveNotes_EventsCollection_WithoutPredicate_EmptyCollection([Values] ContainerType containerType) => RemoveNotes_EventsCollection_WithoutPredicate(
            containerType: containerType,
            midiEvents: new MidiEvent[0],
            expectedMidiEvents: new MidiEvent[0]);

        [Test]
        public void RemoveNotes_EventsCollection_WithoutPredicate_OneNote_1([Values] ContainerType containerType) => RemoveNotes_EventsCollection_WithoutPredicate(
            containerType: containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new NoteOffEvent(),
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
            });

        [Test]
        public void RemoveNotes_EventsCollection_WithoutPredicate_OneNote_2([Values] ContainerType containerType) => RemoveNotes_EventsCollection_WithoutPredicate(
            containerType: containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 50 },
                new TextEvent("A") { DeltaTime = 100 },
                new NoteOffEvent(),
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A") { DeltaTime = 150 },
            });

        [Test]
        public void RemoveNotes_EventsCollection_WithoutPredicate_OneNote_3([Values] ContainerType containerType) => RemoveNotes_EventsCollection_WithoutPredicate(
            containerType: containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A") { DeltaTime = 100 },
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A") { DeltaTime = 100 },
            });

        [Test]
        public void RemoveNotes_EventsCollection_WithoutPredicate_OneNote_4([Values] ContainerType containerType) => RemoveNotes_EventsCollection_WithoutPredicate(
            containerType: containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent { DeltaTime = 350 },
                new TextEvent("A") { DeltaTime = 100 },
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A") { DeltaTime = 450 },
            });

        [Test]
        public void RemoveNotes_EventsCollection_WithoutPredicate_OneNote_5([Values] ContainerType containerType) => RemoveNotes_EventsCollection_WithoutPredicate(
            containerType: containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new TextEvent("A"),
                new ControlChangeEvent { DeltaTime = 40 },
                new NoteOffEvent { DeltaTime = 100 },
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent { DeltaTime = 40 },
            });

        [Test]
        public void RemoveNotes_EventsCollection_WithoutPredicate_OneNote_6([Values] ContainerType containerType) => RemoveNotes_EventsCollection_WithoutPredicate(
            containerType: containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent { DeltaTime = 100 },
                new TextEvent("A"),
                new ControlChangeEvent { DeltaTime = 40 },
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A") { DeltaTime = 100 },
                new ControlChangeEvent { DeltaTime = 40 },
            });

        [Test]
        public void RemoveNotes_EventsCollection_WithoutPredicate_MultipleNotes_1([Values] ContainerType containerType) => RemoveNotes_EventsCollection_WithoutPredicate(
            containerType: containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent(),
                new NoteOffEvent(),
            },
            expectedMidiEvents: new MidiEvent[0]);

        [Test]
        public void RemoveNotes_EventsCollection_WithoutPredicate_MultipleNotes_2([Values] ContainerType containerType) => RemoveNotes_EventsCollection_WithoutPredicate(
            containerType: containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new TextEvent("A"),
                new NoteOnEvent { DeltaTime = 100 },
                new ControlChangeEvent(),
                new NoteOffEvent { DeltaTime = 50 },
                new TextEvent("B"),
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent { DeltaTime = 100 },
                new TextEvent("B") { DeltaTime = 50 },
            });

        [Test]
        public void RemoveNotes_TrackChunks_WithPredicate_EmptyCollection([Values] bool wrapToFile, [Values] bool predicateValue) => RemoveNotes_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new MidiEvent[0][],
            n => predicateValue,
            expectedMidiEvents: new MidiEvent[0][],
            expectedRemovedCount: 0);

        [Test]
        public void RemoveNotes_TrackChunks_WithPredicate_EmptyTrackChunks([Values] bool wrapToFile, [Values] bool predicateValue) => RemoveNotes_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[0],
                new MidiEvent[0],
            },
            n => predicateValue,
            expectedMidiEvents: new[]
            {
                new MidiEvent[0],
                new MidiEvent[0],
            },
            expectedRemovedCount: 0);

        [Test]
        public void RemoveNotes_TrackChunks_WithPredicate_OneNote_NotMatched([Values] bool wrapToFile) => RemoveNotes_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent()
                },
                new MidiEvent[]
                {
                    new NoteOffEvent()
                },
            },
            n => false,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent()
                },
                new MidiEvent[]
                {
                    new NoteOffEvent()
                },
            },
            expectedRemovedCount: 0);

        [Test]
        public void RemoveNotes_TrackChunks_WithPredicate_OneNote_Matched([Values] bool wrapToFile) => RemoveNotes_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new TextEvent("A")
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 100 },
                    new ProgramChangeEvent()
                },
            },
            n => true,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A")
                },
                new MidiEvent[]
                {
                    new ProgramChangeEvent { DeltaTime = 100 }
                },
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_TrackChunks_WithPredicate_MultipleNotes_NotMatched([Values] bool wrapToFile) => RemoveNotes_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 },
                    new TextEvent("A"),
                    new NoteOnEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 200 },
                    new ProgramChangeEvent()
                },
            },
            n => false,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 },
                    new TextEvent("A"),
                    new NoteOnEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 200 },
                    new ProgramChangeEvent()
                },
            },
            expectedRemovedCount: 0);

        [Test]
        public void RemoveNotes_TrackChunks_WithPredicate_MultipleNotes_SomeMatched_1([Values] bool wrapToFile) => RemoveNotes_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 },
                    new TextEvent("A"),
                    new NoteOnEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 200 },
                    new ProgramChangeEvent()
                },
            },
            n => n.Time == 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A") { DeltaTime = 100 },
                    new NoteOnEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 200 },
                    new ProgramChangeEvent()
                },
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveNotes_TrackChunks_WithPredicate_MultipleNotes_SomeMatched_2([Values] bool wrapToFile) => RemoveNotes_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 100 },
                    new NoteOnEvent((SevenBitNumber)100, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)100, SevenBitNumber.MinValue) { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new ProgramChangeEvent()
                },
            },
            n => n.NoteNumber < 100,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent((SevenBitNumber)100, SevenBitNumber.MaxValue) { DeltaTime = 200 },
                    new NoteOffEvent((SevenBitNumber)100, SevenBitNumber.MinValue) { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new ProgramChangeEvent()
                },
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveNotes_TrackChunks_WithPredicate_MultipleNotes_SomeMatched_3([Values] bool wrapToFile) => RemoveNotes_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new ProgramChangeEvent()
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 100 },
                    new NoteOnEvent((SevenBitNumber)100, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)100, SevenBitNumber.MinValue) { DeltaTime = 100 },
                },
            },
            n => n.NoteNumber < 100,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new ProgramChangeEvent()
                },
                new MidiEvent[]
                {
                    new NoteOnEvent((SevenBitNumber)100, SevenBitNumber.MaxValue) { DeltaTime = 200 },
                    new NoteOffEvent((SevenBitNumber)100, SevenBitNumber.MinValue) { DeltaTime = 100 },
                },
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveNotes_TrackChunks_WithPredicate_MultipleNotes_SomeMatched_4([Values] bool wrapToFile) => RemoveNotes_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new ProgramChangeEvent()
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new NoteOnEvent((SevenBitNumber)100, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)100, SevenBitNumber.MinValue),
                },
            },
            n => n.NoteNumber < 100,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new ProgramChangeEvent()
                },
                new MidiEvent[]
                {
                    new NoteOnEvent((SevenBitNumber)100, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)100, SevenBitNumber.MinValue),
                },
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveNotes_TrackChunks_WithPredicate_MultipleNotes_AllMatched_1([Values] bool wrapToFile) => RemoveNotes_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 },
                    new TextEvent("A"),
                    new NoteOnEvent(),
                },
                new MidiEvent[]
                {
                    new NoteOffEvent { DeltaTime = 200 },
                    new ProgramChangeEvent()
                },
            },
            n => true,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A") { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new ProgramChangeEvent { DeltaTime = 200 }
                },
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveNotes_TrackChunks_WithPredicate_MultipleNotes_AllMatched_2([Values] bool wrapToFile) => RemoveNotes_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 100 },
                    new NoteOnEvent((SevenBitNumber)100, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)100, SevenBitNumber.MinValue) { DeltaTime = 100 },
                },
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new ProgramChangeEvent()
                },
            },
            n => true,
            expectedMidiEvents: new[]
            {
                new MidiEvent[0],
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new ProgramChangeEvent()
                },
            },
            expectedRemovedCount: 3);

        [Test]
        public void RemoveNotes_TrackChunks_WithPredicate_MultipleNotes_AllMatched_3([Values] bool wrapToFile) => RemoveNotes_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new ProgramChangeEvent()
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent { DeltaTime = 100 },
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue) { DeltaTime = 100 },
                    new NoteOnEvent((SevenBitNumber)100, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)100, SevenBitNumber.MinValue) { DeltaTime = 100 },
                },
            },
            n => n.NoteNumber < SevenBitNumber.MaxValue,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new ProgramChangeEvent()
                },
                new MidiEvent[0],
            },
            expectedRemovedCount: 3);

        [Test]
        public void RemoveNotes_TrackChunks_WithPredicate_MultipleNotes_AllMatched_4([Values] bool wrapToFile) => RemoveNotes_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new ProgramChangeEvent()
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new NoteOnEvent((SevenBitNumber)100, SevenBitNumber.MaxValue),
                    new NoteOffEvent((SevenBitNumber)100, SevenBitNumber.MinValue),
                },
            },
            n => true,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new ProgramChangeEvent()
                },
                new MidiEvent[0],
            },
            expectedRemovedCount: 3);

        #endregion

        #region Private methods

        private void RemoveNotes_EventsCollection_WithPredicate(
            ContainerType containerType,
            ICollection<MidiEvent> midiEvents,
            Predicate<Note> match,
            ICollection<MidiEvent> expectedMidiEvents,
            int expectedRemovedCount)
        {
            var eventsCollection = new EventsCollection();
            eventsCollection.AddRange(midiEvents);

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        Assert.AreEqual(
                            expectedRemovedCount,
                            eventsCollection.RemoveNotes(match),
                            "Invalid count of removed notes.");

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
                            expectedRemovedCount,
                            trackChunk.RemoveNotes(match),
                            "Invalid count of removed notes.");

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
                        RemoveNotes_TrackChunks_WithPredicate(
                            containerType == ContainerType.File,
                            new[] { midiEvents },
                            match,
                            new[] { expectedMidiEvents },
                            expectedRemovedCount);
                    }
                    break;
            }
        }

        private void RemoveNotes_EventsCollection_WithoutPredicate(
            ContainerType containerType,
            ICollection<MidiEvent> midiEvents,
            ICollection<MidiEvent> expectedMidiEvents)
        {
            var eventsCollection = new EventsCollection();
            eventsCollection.AddRange(midiEvents);

            var notesCount = eventsCollection.GetNotes().Count();

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        Assert.AreEqual(
                            notesCount,
                            eventsCollection.RemoveNotes(),
                            "Invalid count of removed notes.");

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
                            notesCount,
                            trackChunk.RemoveNotes(),
                            "Invalid count of removed notes.");

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
                        RemoveNotes_TrackChunks_WithoutPredicate(
                            containerType == ContainerType.File,
                            new[] { midiEvents },
                            new[] { expectedMidiEvents });
                    }
                    break;
            }
        }

        private void RemoveNotes_TrackChunks_WithPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Predicate<Note> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedRemovedCount)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    expectedRemovedCount,
                    midiFile.RemoveNotes(match),
                    "Invalid count of removed notes.");

                MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                Assert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                Assert.AreEqual(
                    expectedRemovedCount,
                    trackChunks.RemoveNotes(match),
                    "Invalid count of removed notes.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                Assert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        private void RemoveNotes_TrackChunks_WithoutPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();
            var notesCount = trackChunks.GetNotes().Count();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    notesCount,
                    midiFile.RemoveNotes(),
                    "Invalid count of removed notes.");

                MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                Assert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                Assert.AreEqual(
                    notesCount,
                    trackChunks.RemoveNotes(),
                    "Invalid count of removed notes.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                Assert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        #endregion
    }
}
