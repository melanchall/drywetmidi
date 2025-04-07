using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Composing;
using Melanchall.DryWetMidi.MusicTheory;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Standards;
using NUnit.Framework;
using System.Collections.Generic;
using System;
using System.Linq;

using Random = Melanchall.DryWetMidi.Common.Random;
using NUnit.Framework.Legacy;

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

                if (type == typeof(AddChordAction))
                    patternAction = new AddChordAction(new ChordDescriptor(GetRandomNotesCollection(), GetRandomSevenBitNumber(), GetRandomMusicalTimeSpan()));
                else if (type == typeof(AddNoteAction))
                    patternAction = new AddNoteAction(new NoteDescriptor(GetRandomNote(), GetRandomSevenBitNumber(), GetRandomMusicalTimeSpan()));
                else if (type == typeof(AddPatternAction))
                    patternAction = new AddPatternAction(new PatternBuilder().Build());
                else if (type == typeof(AddTextEventAction<>))
                    patternAction = new AddTextEventAction<TextEvent>(GetRandomString());
                else if (type == typeof(AddControlChangeEventAction))
                    patternAction = new AddControlChangeEventAction(GetRandomSevenBitNumber(), GetRandomSevenBitNumber());
                else if (type == typeof(AddPitchBendEventAction))
                    patternAction = new AddPitchBendEventAction((ushort)Random.Instance.Next(ushort.MaxValue + 1));
                else if (type == typeof(MoveToAnchorAction))
                    patternAction = new MoveToAnchorAction(GetRandomEnumValue<AnchorPosition>());
                else if (type == typeof(SetGeneralMidi2ProgramAction))
                    patternAction = new SetGeneralMidi2ProgramAction(GetRandomEnumValue<GeneralMidi2Program>());
                else if (type == typeof(SetGeneralMidiProgramAction))
                    patternAction = new SetGeneralMidiProgramAction(GetRandomEnumValue<GeneralMidiProgram>());
                else if (type == typeof(SetProgramNumberAction))
                    patternAction = new SetProgramNumberAction(GetRandomSevenBitNumber());
                else if (type == typeof(StepBackAction))
                    patternAction = new StepBackAction(GetRandomMusicalTimeSpan());
                else if (type == typeof(StepForwardAction))
                    patternAction = new StepForwardAction(GetRandomMusicalTimeSpan());
                else
                    patternAction = (PatternAction)Activator.CreateInstance(type);

                var patternActionClone = patternAction.Clone();
                ClassicAssert.AreEqual(patternAction.GetType(), patternActionClone.GetType(), $"Clone of {type} is of invalid type.");
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

        private static TValue GetRandomEnumValue<TValue>()
        {
            var values = Enum.GetValues(typeof(TValue)).OfType<TValue>().ToArray();
            return values[Random.Instance.Next(values.Length)];
        }

        private static SevenBitNumber GetRandomSevenBitNumber() =>
            (SevenBitNumber)Random.Instance.Next(SevenBitNumber.MaxValue + 1);

        private static DryWetMidi.MusicTheory.Note GetRandomNote() =>
            DryWetMidi.MusicTheory.Note.Get(GetRandomSevenBitNumber());

        private static IEnumerable<DryWetMidi.MusicTheory.Note> GetRandomNotesCollection() =>
            Enumerable
                .Range(0, Random.Instance.Next(11))
                .Select(i => GetRandomNote());

        private static MusicalTimeSpan GetRandomMusicalTimeSpan() =>
            new MusicalTimeSpan(1, (long)Math.Pow(2, Random.Instance.Next(6)));

        private static string GetRandomString() =>
            new string(Enumerable.Range(0, Random.Instance.Next(11)).Select(_ => (char)Random.Instance.Next(32, 127)).ToArray());

        #endregion
    }
}
