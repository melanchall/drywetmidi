using Melanchall.DryWetMidi.Smf.Interaction;
using NUnit.Framework;

namespace Melanchall.DryWetMidi.Tests.Tools
{
    public abstract class LengthedObjectsToolTests<TObject>
        where TObject : ILengthedObject
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            TestContext.AddFormatter<TObject>((object obj) =>
            {
                var tObj = (TObject)obj;
                return $"{tObj} (T = {tObj.Time}, L = {tObj.Length})";
            });
        }
    }
}
