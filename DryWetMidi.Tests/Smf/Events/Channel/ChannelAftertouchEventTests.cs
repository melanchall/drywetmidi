using Melanchall.DryWetMidi.Smf;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Melanchall.DryWetMidi.Tests.Smf
{
    [TestClass]
    public sealed class ChannelAftertouchEventTests : BaseEventTests<ChannelAftertouchEvent>
    {
        protected override ChannelAftertouchEvent CreateEvent1()
        {
            return new ChannelAftertouchEvent((SevenBitNumber)100);
        }

        protected override ChannelAftertouchEvent CreateEvent2()
        {
            return new ChannelAftertouchEvent((SevenBitNumber)100);
        }
    }
}
