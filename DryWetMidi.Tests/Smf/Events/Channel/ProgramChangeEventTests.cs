using Melanchall.DryWetMidi.Smf;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf
{
    [TestClass]
    public sealed class ProgramChangeEventTests : BaseEventTests<ProgramChangeEvent>
    {
        protected override ProgramChangeEvent CreateEvent1()
        {
            return new ProgramChangeEvent((SevenBitNumber)100);
        }

        protected override ProgramChangeEvent CreateEvent2()
        {
            return new ProgramChangeEvent((SevenBitNumber)100);
        }
    }
}
