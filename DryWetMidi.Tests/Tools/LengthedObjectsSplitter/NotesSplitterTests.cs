using System.Collections.Generic;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [TestFixture]
    public sealed class NotesSplitterTests : LengthedObjectsSplitterTests<Note>
    {
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

        protected override SplittedLengthedObject<Note> SplitObject(Note obj, long time)
        {
            return obj.Split(time);
        }

        #endregion
    }
}
