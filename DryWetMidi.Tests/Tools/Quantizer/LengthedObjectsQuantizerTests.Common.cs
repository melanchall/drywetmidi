using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tests.Utilities;
using Melanchall.DryWetMidi.Tools;
using System;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    public abstract partial class LengthedObjectsQuantizerTests<TObject, TSettings> : LengthedObjectsToolTests<TObject>
        where TObject : ILengthedObject
        where TSettings : LengthedObjectsQuantizingSettings<TObject>, new()
    {
        #region Constructor

        public LengthedObjectsQuantizerTests(
            LengthedObjectMethods<TObject> methods,
            LengthedObjectsQuantizer<TObject, TSettings> quantizer,
            LengthedObjectsQuantizer<TObject, TSettings> skipQuantizer,
            Func<long, LengthedObjectsQuantizer<TObject, TSettings>> fixedTimeQuantizerGetter)
            : base(methods)
        {
            Quantizer = quantizer;
            SkipQuantizer = skipQuantizer;
            FixedTimeQuantizerGetter = fixedTimeQuantizerGetter;
        }

        #endregion

        #region Properties

        protected LengthedObjectsQuantizer<TObject, TSettings> Quantizer { get; }

        protected LengthedObjectsQuantizer<TObject, TSettings> SkipQuantizer { get; }

        protected Func<long, LengthedObjectsQuantizer<TObject, TSettings>> FixedTimeQuantizerGetter { get; }

        #endregion
    }
}
