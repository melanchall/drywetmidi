#if TEST
using Melanchall.DryWetMidi.Tests.Attributes;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [NonParallelizable]
    public sealed partial class MidiDevicesSessionTests
    {
        #region Test methods

        [Test]
        [WinOnly]
        [VirtualDeviceApiRequired]
        public void WmsInitialEnumerationOwnsOnlyCachedInfosAndFreesListingCopies()
        {
            using (var context = new WmsTestContext())
            {
                var baselineCachedInfos = context.GetSessionCounter(WmsTestSessionCounter.CachedEndpointInfos);

                context.GetInputProperties();
                context.GetOutputProperties();

                ClassicAssert.AreEqual(
                    baselineCachedInfos,
                    context.GetSessionCounter(WmsTestSessionCounter.CachedEndpointInfos),
                    "Listing endpoints changed the initial enumeration cache.");
                context.AssertNoTemporaryInfos();
            }
        }

        [Test]
        [WinOnly]
        [VirtualDeviceApiRequired]
        public void WmsInitialEnumerationAddAndRemoveRetainNoTemporaryOrRemovedInfos()
        {
            using (var context = new WmsTestContext())
            using (var virtualDevice = context.CreateVirtualDevice())
            {
                ClassicAssert.IsTrue(
                    WaitOperations.Wait(
                        () => context.GetInputProperties(true).Contains(virtualDevice.Name) &&
                            context.GetOutputProperties(true).Contains(virtualDevice.Name),
                        WmsTestContext.Timeout),
                    "Initial SDK enumeration did not discover both real loopback directions.");
                context.AssertNoTemporaryInfos(2);
                var cachedInfosWithLoopback = context.GetSessionCounter(WmsTestSessionCounter.CachedEndpointInfos);
                context.ProcessEndpoint(virtualDevice, false);
                var baselineCachedInfos = context.GetSessionCounter(WmsTestSessionCounter.CachedEndpointInfos);
                var baselineLiveInfos = context.GetCounter(WmsTestCounter.LiveEndpointInfos);

                ClassicAssert.Greater(cachedInfosWithLoopback, baselineCachedInfos, "Deterministic remove did not remove any cached loopback infos.");
                context.SetEnumerationCompleted(false);

                try
                {
                    context.ProcessEndpoint(virtualDevice, true);

                    ClassicAssert.AreEqual(
                        cachedInfosWithLoopback,
                        context.GetSessionCounter(WmsTestSessionCounter.CachedEndpointInfos),
                        "Initial-enumeration add did not retain the expected persistent infos.");
                    context.AssertNoTemporaryInfos(2);

                    context.ProcessEndpoint(virtualDevice, false);

                    ClassicAssert.AreEqual(
                        baselineCachedInfos,
                        context.GetSessionCounter(WmsTestSessionCounter.CachedEndpointInfos),
                        "Initial-enumeration remove retained cached endpoint infos.");
                    ClassicAssert.AreEqual(
                        baselineLiveInfos,
                        context.GetCounter(WmsTestCounter.LiveEndpointInfos),
                        "Initial-enumeration add/remove leaked temporary or persistent endpoint infos.");
                }
                finally
                {
                    context.SetEnumerationCompleted(true);
                }

                context.GetInputProperties();
                context.GetOutputProperties();

                ClassicAssert.AreEqual(
                    baselineLiveInfos,
                    context.GetCounter(WmsTestCounter.LiveEndpointInfos),
                    "Freeing input/output enumeration copies did not restore the live-info baseline.");
                context.AssertNoTemporaryInfos(2);
            }
        }

        #endregion
    }
}
#endif
