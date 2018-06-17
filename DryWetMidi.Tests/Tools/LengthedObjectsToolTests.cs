using Melanchall.DryWetMidi.Smf.Interaction;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    public abstract class LengthedObjectsToolTests<TObject>
        where TObject : ILengthedObject
    {
        #region Constructor

        public LengthedObjectsToolTests(LengthedObjectMethods<TObject> objectMethods)
        {
            ObjectMethods = objectMethods;
        }

        #endregion

        #region Properties

        protected LengthedObjectMethods<TObject> ObjectMethods { get; }

        #endregion

        #region Methods

        [OneTimeSetUp]
        public void SetUp()
        {
            TestContext.AddFormatter<TObject>((object obj) =>
            {
                var tObj = (TObject)obj;
                return $"{tObj} (T = {tObj.Time}, L = {tObj.Length})";
            });
        }

        #endregion
    }
}
