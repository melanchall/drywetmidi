﻿using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Standards;
using NUnit.Framework;
using System.Collections.Generic;
using System;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Composing
{
    [TestFixture]
    public sealed partial class PatternBuilderTests
    {
        #region Test methods

        #region ReplayPattern

        [Test]
        public void ReplayPattern_Empty()
        {
            var pattern1 = new PatternBuilder().Build();

            var pattern2 = new PatternBuilder()
                .ReplayPattern(pattern1)
                .Build();

            CollectionAssert.IsEmpty(pattern2.Actions, "Pattern is not empty.");
        }

        [Test]
        public void ReplayPattern_Notes()
        {
            var pattern1 = new PatternBuilder()
                .Note(Notes.A4)
                .Note(Notes.ASharp4)
                .Build();

            var pattern2 = new PatternBuilder()
                .ReplayPattern(pattern1)
                .Build();

            PatternTestUtilities.TestNotes(pattern2, new[]
            {
                new NoteInfo(NoteName.A, 4, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.ASharp, 4, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void ReplayPattern_Anchor()
        {
            var pattern1 = new PatternBuilder()
                .Note(Notes.A4)
                .Anchor("X")
                .Note(Notes.ASharp4)
                .MoveToFirstAnchor("X")
                .Note(Notes.DSharp3)
                .Build();

            var pattern2 = new PatternBuilder()
                .ReplayPattern(pattern1)
                .Build();

            PatternTestUtilities.TestNotes(pattern2, new[]
            {
                new NoteInfo(NoteName.A, 4, null, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.ASharp, 4, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.DSharp, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength)
            });
        }

        #endregion

        #region Build from pattern

        [Test]
        public void BuildFromPattern_Empty()
        {
            var pattern1 = new PatternBuilder().Build();
            var pattern2 = new PatternBuilder(pattern1).Build();

            CollectionAssert.IsEmpty(pattern2.Actions, "Pattern is not empty.");
        }

        [Test]
        public void BuildFromPattern_Notes()
        {
            var pattern1 = new PatternBuilder()
                .Note(Notes.A4)
                .Note(Notes.ASharp4)
                .Build();

            var pattern2 = new PatternBuilder(pattern1).Build();

            PatternTestUtilities.TestNotes(pattern2, new[]
            {
                new NoteInfo(NoteName.A, 4, null, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity),
                new NoteInfo(NoteName.ASharp, 4, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultVelocity)
            });
        }

        [Test]
        public void BuildFromPattern_Anchor()
        {
            var pattern1 = new PatternBuilder()
                .Note(Notes.A4)
                .Anchor("X")
                .Note(Notes.ASharp4)
                .MoveToFirstAnchor("X")
                .Note(Notes.DSharp3)
                .Build();

            var pattern2 = new PatternBuilder(pattern1).Build();

            PatternTestUtilities.TestNotes(pattern2, new[]
            {
                new NoteInfo(NoteName.A, 4, null, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.ASharp, 4, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength),
                new NoteInfo(NoteName.DSharp, 3, PatternBuilder.DefaultNoteLength, PatternBuilder.DefaultNoteLength)
            });
        }

        #endregion

        #region Internal

        [Test]
        public void ClonePatternAction_TypeIsCorrect()
        {
            foreach (var type in GetAllActionTypes())
            {
                PatternAction patternAction = null;

                // TODO: randomize values
                if (type == typeof(AddChordAction))
                    patternAction = new AddChordAction(new ChordDescriptor(Enumerable.Empty<DryWetMidi.MusicTheory.Note>(), SevenBitNumber.MinValue, MusicalTimeSpan.Eighth));
                else if (type == typeof(AddNoteAction))
                    patternAction = new AddNoteAction(new NoteDescriptor(Notes.A1, SevenBitNumber.MinValue, MusicalTimeSpan.Eighth));
                else if (type == typeof(AddPatternAction))
                    patternAction = new AddPatternAction(new PatternBuilder().Build());
                else if (type == typeof(AddTextEventAction<>))
                    patternAction = new AddTextEventAction<TextEvent>(string.Empty);
                else if (type == typeof(AddControlChangeEventAction))
                    patternAction = new AddControlChangeEventAction((SevenBitNumber)70, (SevenBitNumber)30);
                else if (type == typeof(AddPitchBendEventAction))
                    patternAction = new AddPitchBendEventAction(600);
                else if (type == typeof(MoveToAnchorAction))
                    patternAction = new MoveToAnchorAction(AnchorPosition.First);
                else if (type == typeof(SetGeneralMidi2ProgramAction))
                    patternAction = new SetGeneralMidi2ProgramAction(GeneralMidi2Program.AcousticBassStringSlap);
                else if (type == typeof(SetGeneralMidiProgramAction))
                    patternAction = new SetGeneralMidiProgramAction(GeneralMidiProgram.AcousticBass);
                else if (type == typeof(SetProgramNumberAction))
                    patternAction = new SetProgramNumberAction(SevenBitNumber.MinValue);
                else if (type == typeof(StepBackAction))
                    patternAction = new StepBackAction(MusicalTimeSpan.Quarter);
                else if (type == typeof(StepForwardAction))
                    patternAction = new StepForwardAction(MusicalTimeSpan.Quarter);
                else
                    patternAction = (PatternAction)Activator.CreateInstance(type);

                var patternActionClone = patternAction.Clone();
                Assert.AreEqual(patternAction.GetType(), patternActionClone.GetType(), $"Clone of {type} is of invalid type.");
            }
        }

        #endregion

        #endregion

        #region Private methods

        private static IEnumerable<Type> GetAllActionTypes()
        {
            var patternActionType = typeof(PatternAction);
            return patternActionType
                .Assembly
                .GetTypes()
                .Where(t => !t.IsAbstract && t.IsSubclassOf(patternActionType))
                .ToList();
        }

        #endregion
    }
}
