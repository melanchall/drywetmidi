using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using System;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    [Obsolete("OBS13")]
    public abstract partial class LengthedObjectsQuantizerTests<TObject> : LengthedObjectsToolTests<TObject>
        where TObject : ILengthedObject
    {
        #region Constructor

        public LengthedObjectsQuantizerTests(
            LengthedObjectMethods<TObject> methods,
            Quantizer skipQuantizer,
            Func<long, Quantizer> fixedTimeQuantizerGetter)
            : base(methods)
        {
            SkipQuantizer = skipQuantizer;
            FixedTimeQuantizerGetter = fixedTimeQuantizerGetter;
        }

        #endregion

        #region Properties

        protected Quantizer Quantizer { get; } = new Quantizer();

        protected Quantizer SkipQuantizer { get; }

        protected Func<long, Quantizer> FixedTimeQuantizerGetter { get; }

        #endregion
    }
}
