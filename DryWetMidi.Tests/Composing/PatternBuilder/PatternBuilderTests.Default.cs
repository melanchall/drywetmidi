﻿using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Interaction;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    [TestFixture]
    public sealed partial class PatternBuilderTests
    {
        #region Test methods

        [Test]
        public void SetNoteLength()
        {
            ClassicAssert.AreEqual(MusicalTimeSpan.Quarter, PatternBuilder.DefaultNoteLength, "Default note length is invalid.");

            var patternBuilder = new PatternBuilder();
            ClassicAssert.AreEqual(PatternBuilder.DefaultNoteLength, patternBuilder.NoteLength, "Invalid initial note length.");

            var noteLength = new MetricTimeSpan(0, 0, 20);
            patternBuilder.SetNoteLength(noteLength);
            ClassicAssert.AreEqual(noteLength, patternBuilder.NoteLength, "Invalid note length after change.");
        }

        [Test]
        public void SetRootNote()
        {
            ClassicAssert.AreEqual(Octave.Middle.C, PatternBuilder.DefaultRootNote, "Default root note is invalid.");

            var patternBuilder = new PatternBuilder();
            ClassicAssert.AreEqual(PatternBuilder.DefaultRootNote, patternBuilder.RootNote, "Invalid initial root note.");

            var rootNote = Octave.Get(2).ASharp;
            patternBuilder.SetRootNote(rootNote);
            ClassicAssert.AreEqual(rootNote, patternBuilder.RootNote, "Invalid root note after change.");
        }

        [Test]
        public void SetVelocity()
        {
            ClassicAssert.AreEqual(DryWetMidi.Interaction.Note.DefaultVelocity, PatternBuilder.DefaultVelocity, "Default velocity is invalid.");

            var patternBuilder = new PatternBuilder();
            ClassicAssert.AreEqual(PatternBuilder.DefaultVelocity, patternBuilder.Velocity, "Invalid initial velocity.");

            var velocity = (SevenBitNumber)75;
            patternBuilder.SetVelocity(velocity);
            ClassicAssert.AreEqual(velocity, patternBuilder.Velocity, "Invalid velocity after change.");
        }

        [Test]
        public void SetStep()
        {
            ClassicAssert.AreEqual(MusicalTimeSpan.Quarter, PatternBuilder.DefaultStep, "Default step is invalid.");

            var patternBuilder = new PatternBuilder();
            ClassicAssert.AreEqual(PatternBuilder.DefaultStep, patternBuilder.Step, "Invalid initial step.");

            var step = new BarBeatFractionTimeSpan(0, 2.5);
            patternBuilder.SetStep(step);
            ClassicAssert.AreEqual(step, patternBuilder.Step, "Invalid step after change.");
        }

        [Test]
        public void SetOctave()
        {
            ClassicAssert.AreEqual(Octave.Middle, PatternBuilder.DefaultOctave, "Default octave is invalid.");

            var patternBuilder = new PatternBuilder();
            ClassicAssert.AreEqual(PatternBuilder.DefaultOctave, patternBuilder.Octave, "Invalid initial octave.");

            var octave = Octave.Get(6);
            patternBuilder.SetOctave(octave);
            ClassicAssert.AreEqual(octave, patternBuilder.Octave, "Invalid octave after change.");
        }

        #endregion
    }
}
