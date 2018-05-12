using System;
using System.Collections;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;

namespace Melanchall.DryWetMidi.Tests.Common
{
    public sealed class NoteMethods : LengthedObjectMethods<Note>
    {
        #region Nested classes

        private sealed class NoteComparer : IComparer
        {
            #region IComparer

            public int Compare(object x, object y)
            {
                var xNote = (Note)x;
                var yNote = (Note)y;

                return NoteEquality.AreEqual(xNote, yNote) ? 0 : -1;
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly Random _random = new Random();

        #endregion

        #region Overrides

        protected override IComparer Comparer { get; } = new NoteComparer();

        public override Note Create(long time, long length)
        {
            return new Note((SevenBitNumber)_random.Next(SevenBitNumber.MaxValue), length, time);
        }

        public override Note Clone(Note obj)
        {
            return obj.Clone();
        }

        #endregion
    }
}
