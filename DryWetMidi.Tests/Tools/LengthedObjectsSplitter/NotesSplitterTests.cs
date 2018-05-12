using System;
using System.Collections;
using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed class NotesSplitterTests : LengthedObjectsSplitterTests<Note>
    {
        #region Nestd classes

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

        #region Constructor

        public NotesSplitterTests()
            : base(new NoteMethods(), new NotesSplitter())
        {
        }

        #endregion

        #region Overrides

        protected override IEnumerable<Note> CreateInputObjects(long length)
        {
            return new[]
            {
                new Note((SevenBitNumber)100, length),
                new Note((SevenBitNumber)110, length, 200),
            };
        }

        protected override Tuple<Note, Note> SplitObject(Note obj, long time)
        {
            return obj.Split(time);
        }

        #endregion
    }
}
