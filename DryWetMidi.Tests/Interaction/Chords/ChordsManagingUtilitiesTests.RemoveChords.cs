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
        public void RemoveChords_EventsCollection_WithPredicate_EmptyCollection([Values] ContainerType containerType, [Values] bool predicateValue) => RemoveChords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[0],
            match: c => predicateValue,
            expectedMidiEvents: new MidiEvent[0],
            expectedRemovedCount: 0);

        [Test]
        public void RemoveChords_EventsCollection_WithPredicate_NoNotes([Values] ContainerType containerType, [Values] bool predicateValue) => RemoveChords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            match: c => predicateValue,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            expectedRemovedCount: 0);

        [Test]
        public void RemoveChords_EventsCollection_WithPredicate_NoChords([Values] ContainerType containerType, [Values] bool predicateValue) => RemoveChords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOffEvent(),
            },
            match: c => predicateValue,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
                new NoteOffEvent(),
            },
            expectedRemovedCount: 0);

        [Test]
        public void RemoveChords_EventsCollection_WithPredicate_OneChord_NotMatched([Values] ContainerType containerType) => RemoveChords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new ControlChangeEvent(),
                new NoteOffEvent(),
            },
            match: c => false,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new ControlChangeEvent(),
                new NoteOffEvent(),
            },
            expectedRemovedCount: 0);

        [Test]
        public void RemoveChords_EventsCollection_WithPredicate_OneChord_Matched([Values] ContainerType containerType) => RemoveChords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new ControlChangeEvent(),
                new NoteOffEvent(),
            },
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveChords_EventsCollection_WithPredicate_MultipleChords_NotMatched([Values] ContainerType containerType) => RemoveChords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
            },
            match: c => false,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
            },
            expectedRemovedCount: 0);

        [Test]
        public void RemoveChords_EventsCollection_WithPredicate_MultipleChords_SomeMatched_1([Values] ContainerType containerType) => RemoveChords_EventsCollection_WithPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
            },
            match: c => c.Time < 100,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveChords_EventsCollection_WithPredicate_MultipleChords_SomeMatched_2([Values] ContainerType containerType) => RemoveChords_EventsCollection_WithPredicate(
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
            match: c => c.Channel == 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveChords_EventsCollection_WithPredicate_MultipleChords_SomeMatched_3([Values] ContainerType containerType) => RemoveChords_EventsCollection_WithPredicate(
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
            match: c => c.Channel == 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new TextEvent("B"),
                new TextEvent("C"),
                new TextEvent("D"),
                new TextEvent("E"),
                new NoteOffEvent { Channel = (FourBitNumber)4 },
                new TextEvent("F"),
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveChords_EventsCollection_WithPredicate_MultipleChords_SomeMatched_4([Values] ContainerType containerType) => RemoveChords_EventsCollection_WithPredicate(
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
            match: c => c.Channel == 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveChords_EventsCollection_WithPredicate_MultipleChords_SomeMatched_5([Values] ContainerType containerType) => RemoveChords_EventsCollection_WithPredicate(
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
            match: c => c.Channel == 4,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent(),
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                new NoteOnEvent { DeltaTime = 100 },
                new NoteOffEvent(),
                new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveChords_EventsCollection_WithPredicate_MultipleChords_SomeMatched_6([Values] ContainerType containerType) => RemoveChords_EventsCollection_WithPredicate(
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
            match: c => c.Channel == 0,
            expectedMidiEvents: new MidiEvent[]
            {
                new NoteOnEvent { Channel = (FourBitNumber)4 },
                new NoteOffEvent { Channel = (FourBitNumber)4 },
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveChords_EventsCollection_WithPredicate_MultipleChords_AllMatched_1([Values] ContainerType containerType) => RemoveChords_EventsCollection_WithPredicate(
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
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveChords_EventsCollection_WithPredicate_MultipleChords_AllMatched_2([Values] ContainerType containerType) => RemoveChords_EventsCollection_WithPredicate(
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
            match: c => c.Channel < 10,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B"),
                new TextEvent("C"),
                new TextEvent("D"),
                new TextEvent("E"),
                new TextEvent("F"),
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveChords_EventsCollection_WithPredicate_MultipleChords_AllMatched_3([Values] ContainerType containerType) => RemoveChords_EventsCollection_WithPredicate(
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
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveChords_EventsCollection_WithPredicate_MultipleChords_AllMatched_4([Values] ContainerType containerType) => RemoveChords_EventsCollection_WithPredicate(
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
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B") { DeltaTime = 100 },
            },
            expectedRemovedCount: 3);

        [Test]
        public void RemoveChords_EventsCollection_WithPredicate_MultipleChords_AllMatched_5([Values] ContainerType containerType) => RemoveChords_EventsCollection_WithPredicate(
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
            match: c => true,
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A") { DeltaTime = 100 },
            },
            expectedRemovedCount: 3);

        [Test]
        public void RemoveChords_EventsCollection_WithoutPredicate_EmptyCollection([Values] ContainerType containerType) => RemoveChords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[0],
            expectedMidiEvents: new MidiEvent[0]);

        [Test]
        public void RemoveChords_EventsCollection_WithoutPredicate_NoNotes([Values] ContainerType containerType) => RemoveChords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
            });

        [Test]
        public void RemoveChords_EventsCollection_WithoutPredicate_NoChords([Values] ContainerType containerType, [Values] bool predicateValue) => RemoveChords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new ControlChangeEvent(),
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new ControlChangeEvent(),
            });

        [Test]
        public void RemoveChords_EventsCollection_WithoutPredicate_OneChord([Values] ContainerType containerType) => RemoveChords_EventsCollection_WithoutPredicate(
            containerType,
            midiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new NoteOnEvent(),
                new ControlChangeEvent(),
                new NoteOffEvent(),
            },
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new ControlChangeEvent(),
            });

        [Test]
        public void RemoveChords_EventsCollection_WithoutPredicate_MultipleChords_1([Values] ContainerType containerType) => RemoveChords_EventsCollection_WithoutPredicate(
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
            expectedMidiEvents: new MidiEvent[]
            {
            });

        [Test]
        public void RemoveChords_EventsCollection_WithoutPredicate_MultipleChords_2([Values] ContainerType containerType) => RemoveChords_EventsCollection_WithoutPredicate(
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
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B"),
                new TextEvent("C"),
                new TextEvent("D"),
                new TextEvent("E"),
                new TextEvent("F"),
            });

        [Test]
        public void RemoveChords_EventsCollection_WithoutPredicate_MultipleChords_3([Values] ContainerType containerType) => RemoveChords_EventsCollection_WithoutPredicate(
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
            expectedMidiEvents: new MidiEvent[]
            {
            });

        [Test]
        public void RemoveChords_EventsCollection_WithoutPredicate_MultipleChords_4([Values] ContainerType containerType) => RemoveChords_EventsCollection_WithoutPredicate(
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
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A"),
                new TextEvent("B") { DeltaTime = 100 },
            });

        [Test]
        public void RemoveChords_EventsCollection_WithoutPredicate_MultipleChords_5([Values] ContainerType containerType) => RemoveChords_EventsCollection_WithoutPredicate(
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
            expectedMidiEvents: new MidiEvent[]
            {
                new TextEvent("A") { DeltaTime = 100 },
            });

        [Test]
        public void RemoveChords_TrackChunks_WithPredicate_EmptyCollection([Values] bool wrapToFile, [Values] bool predicateValue) => RemoveChords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new MidiEvent[0][],
            match: c => predicateValue,
            expectedMidiEvents: new MidiEvent[0][],
            expectedRemovedCount: 0);

        [Test]
        public void RemoveChords_TrackChunks_WithPredicate_EmptyTrackChunks([Values] bool wrapToFile, [Values] bool predicateValue) => RemoveChords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[0],
                new MidiEvent[0],
            },
            match: c => predicateValue,
            expectedMidiEvents: new[]
            {
                new MidiEvent[0],
                new MidiEvent[0],
            },
            expectedRemovedCount: 0);

        [Test]
        public void RemoveChords_TrackChunks_WithPredicate_NoNotes([Values] bool wrapToFile, [Values] bool predicateValue) => RemoveChords_TrackChunks_WithPredicate(
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
            expectedRemovedCount: 0);

        [Test]
        public void RemoveChords_TrackChunks_WithPredicate_OneChord_NotMatched([Values] bool wrapToFile) => RemoveChords_TrackChunks_WithPredicate(
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
            expectedRemovedCount: 0);

        [Test]
        public void RemoveChords_TrackChunks_WithPredicate_OneChord_Matched([Values] bool wrapToFile) => RemoveChords_TrackChunks_WithPredicate(
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
                    new TextEvent("C"),
                },
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveChords_TrackChunks_WithPredicate_MultipleChords_NotMatched([Values] bool wrapToFile) => RemoveChords_TrackChunks_WithPredicate(
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
            expectedRemovedCount: 0);

        [Test]
        public void RemoveChords_TrackChunks_WithPredicate_MultipleChords_SomeMatched_1([Values] bool wrapToFile) => RemoveChords_TrackChunks_WithPredicate(
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
            match: c => c.Time == 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent(),
                    new TextEvent("B"),
                },
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveChords_TrackChunks_WithPredicate_MultipleChords_SomeMatched_2([Values] bool wrapToFile) => RemoveChords_TrackChunks_WithPredicate(
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
            match: c => c.Time == 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("C"),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent { DeltaTime = 50 },
                    new NoteOffEvent(),
                    new TextEvent("B"),
                },
            },
            expectedRemovedCount: 1);

        [Test]
        public void RemoveChords_TrackChunks_WithPredicate_MultipleChords_SomeMatched_3([Values] bool wrapToFile) => RemoveChords_TrackChunks_WithPredicate(
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
            match: c => c.Time == 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new NoteOnEvent { Channel = (FourBitNumber)4, DeltaTime = 10 },
                    new NoteOnEvent { DeltaTime = 40 },
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent(),
                },
                new MidiEvent[]
                {
                },
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveChords_TrackChunks_WithPredicate_MultipleChords_SomeMatched_4([Values] bool wrapToFile) => RemoveChords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
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
                    new NoteOnEvent { DeltaTime = 40 },
                    new TextEvent("E"),
                    new NoteOffEvent { Channel = (FourBitNumber)4 },
                    new NoteOffEvent(),
                    new TextEvent("F"),
                },
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveChords_TrackChunks_WithPredicate_MultipleChords_SomeMatched_5([Values] bool wrapToFile) => RemoveChords_TrackChunks_WithPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
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
            match: c => c.Time > 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("B"),
                    new NoteOnEvent(),
                    new NoteOffEvent(),
                    new TextEvent("C"),
                },
                new MidiEvent[]
                {
                    new NoteOnEvent(),
                    new NoteOnEvent((SevenBitNumber)70, SevenBitNumber.MaxValue),
                    new NoteOffEvent(),
                    new TextEvent("D"),
                    new TextEvent("E") { DeltaTime = 50 },
                    new NoteOffEvent((SevenBitNumber)70, SevenBitNumber.MinValue),
                    new TextEvent("F"),
                },
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveChords_TrackChunks_WithPredicate_MultipleChords_AllMatched_1([Values] bool wrapToFile) => RemoveChords_TrackChunks_WithPredicate(
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
                    new TextEvent("B") { DeltaTime = 50 },
                },
            },
            expectedRemovedCount: 2);

        [Test]
        public void RemoveChords_TrackChunks_WithPredicate_MultipleChords_AllMatched_2([Values] bool wrapToFile) => RemoveChords_TrackChunks_WithPredicate(
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
            match: c => c.Time >= 0,
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                },
            },
            expectedRemovedCount: 4);

        [Test]
        public void RemoveChords_TrackChunks_WithPredicate_MultipleChords_AllMatched_3([Values] bool wrapToFile) => RemoveChords_TrackChunks_WithPredicate(
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
            match: c => true,
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
                    new TextEvent("E") { DeltaTime = 50 },
                    new TextEvent("F"),
                },
            },
            expectedRemovedCount: 3);

        [Test]
        public void RemoveChords_TrackChunks_WithoutPredicate_EmptyCollection([Values] bool wrapToFile) => RemoveChords_TrackChunks_WithoutPredicate(
            wrapToFile,
            midiEvents: new MidiEvent[0][],
            expectedMidiEvents: new MidiEvent[0][]);

        [Test]
        public void RemoveChords_TrackChunks_WithoutPredicate_EmptyTrackChunks([Values] bool wrapToFile) => RemoveChords_TrackChunks_WithoutPredicate(
            wrapToFile,
            midiEvents: new[]
            {
                new MidiEvent[0],
                new MidiEvent[0],
            },
            expectedMidiEvents: new[]
            {
                new MidiEvent[0],
                new MidiEvent[0],
            });

        [Test]
        public void RemoveChords_TrackChunks_WithoutPredicate_NoNotes([Values] bool wrapToFile) => RemoveChords_TrackChunks_WithoutPredicate(
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
        public void RemoveChords_TrackChunks_WithoutPredicate_OneChord([Values] bool wrapToFile) => RemoveChords_TrackChunks_WithoutPredicate(
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
        public void RemoveChords_TrackChunks_WithoutPredicate_MultipleChords_1([Values] bool wrapToFile) => RemoveChords_TrackChunks_WithoutPredicate(
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
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                    new TextEvent("A"),
                    new TextEvent("C"),
                },
                new MidiEvent[]
                {
                    new TextEvent("B") { DeltaTime = 50 },
                },
            });

        [Test]
        public void RemoveChords_TrackChunks_WithoutPredicate_MultipleChords_2([Values] bool wrapToFile) => RemoveChords_TrackChunks_WithoutPredicate(
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
            expectedMidiEvents: new[]
            {
                new MidiEvent[]
                {
                },
                new MidiEvent[]
                {
                },
            });

        [Test]
        public void RemoveChords_TrackChunks_WithoutPredicate_MultipleChords_3([Values] bool wrapToFile) => RemoveChords_TrackChunks_WithoutPredicate(
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
                    new TextEvent("E") { DeltaTime = 50 },
                    new TextEvent("F"),
                },
            });

        #endregion

        #region Private methods

        private void RemoveChords_EventsCollection_WithPredicate(
            ContainerType containerType,
            ICollection<MidiEvent> midiEvents,
            Predicate<Chord> match,
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
                            eventsCollection.RemoveChords(match),
                            "Invalid count of removed chords.");

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
                            trackChunk.RemoveChords(match),
                            "Invalid count of removed chords.");

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
                        RemoveChords_TrackChunks_WithPredicate(
                            containerType == ContainerType.File,
                            new[] { midiEvents },
                            match,
                            new[] { expectedMidiEvents },
                            expectedRemovedCount);
                    }
                    break;
            }
        }

        private void RemoveChords_EventsCollection_WithoutPredicate(
            ContainerType containerType,
            ICollection<MidiEvent> midiEvents,
            ICollection<MidiEvent> expectedMidiEvents)
        {
            var eventsCollection = new EventsCollection();
            eventsCollection.AddRange(midiEvents);

            var chordsCount = eventsCollection.GetChords().Count;

            switch (containerType)
            {
                case ContainerType.EventsCollection:
                    {
                        Assert.AreEqual(
                            chordsCount,
                            eventsCollection.RemoveChords(),
                            "Invalid count of removed chords.");

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
                            chordsCount,
                            trackChunk.RemoveChords(),
                            "Invalid count of removed chords.");

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
                        RemoveChords_TrackChunks_WithoutPredicate(
                            containerType == ContainerType.File,
                            new[] { midiEvents },
                            new[] { expectedMidiEvents });
                    }
                    break;
            }
        }

        private void RemoveChords_TrackChunks_WithPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            Predicate<Chord> match,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents,
            int expectedRemovedCount)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    expectedRemovedCount,
                    midiFile.RemoveChords(match),
                    "Invalid count of removed chords.");

                MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                Assert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                Assert.AreEqual(
                    expectedRemovedCount,
                    trackChunks.RemoveChords(match),
                    "Invalid count of removed chords.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                Assert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        private void RemoveChords_TrackChunks_WithoutPredicate(
            bool wrapToFile,
            ICollection<ICollection<MidiEvent>> midiEvents,
            ICollection<ICollection<MidiEvent>> expectedMidiEvents)
        {
            var trackChunks = midiEvents.Select(e => new TrackChunk(e)).ToList();
            var chordsCount = trackChunks.GetChords().Count;

            if (wrapToFile)
            {
                var midiFile = new MidiFile(trackChunks);

                Assert.AreEqual(
                    chordsCount,
                    midiFile.RemoveChords(),
                    "Invalid count of removed chords.");

                MidiAsserts.AreEqual(new MidiFile(expectedMidiEvents.Select(e => new TrackChunk(e))), midiFile, false, "Events are invalid.");
                Assert.IsTrue(
                    midiFile.GetTrackChunks().SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
            else
            {
                Assert.AreEqual(
                    chordsCount,
                    trackChunks.RemoveChords(),
                    "Invalid count of removed chords.");

                MidiAsserts.AreEqual(expectedMidiEvents.Select(e => new TrackChunk(e)), trackChunks, true, "Events are invalid.");
                Assert.IsTrue(
                    trackChunks.SelectMany(c => c.Events).All(e => midiEvents.SelectMany(ee => ee).Any(ee => object.ReferenceEquals(e, ee))),
                    "There are new events references.");
            }
        }

        #endregion
    }
}
