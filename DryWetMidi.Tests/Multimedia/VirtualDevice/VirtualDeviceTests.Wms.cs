#if TEST
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Configuration;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Attributes;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Concurrent;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [NonParallelizable]
    public sealed partial class VirtualDeviceTests
    {
        #region Test methods

        [Test]
        [WinOnly]
        [VirtualDeviceApiRequired]
        public void WmsVirtualDeviceCreationFailureRollsBackActualLoopback(
            [Values(false, true)] bool failLoopbackRemoval)
        {
            using (var context = new WmsTestContext())
            {
                context.GetInputProperties();
                context.GetOutputProperties();
                context.AssertNoTemporaryInfos();
                var baselinePendingRemovals = context.GetCounter(WmsTestCounter.PendingLoopbackRemovals);
                var failedDeviceName = "WmsFailed-" + Guid.NewGuid();
                context.SetFailure(WmsTestFailure.AfterLoopbackCreation, true);
                context.SetFailure(WmsTestFailure.RemoveLoopback, failLoopbackRemoval);

                var result = context.TryCreateVirtualDevice(failedDeviceName, out var failedInfo, out var errorCode);

                ClassicAssert.AreEqual(
                    VirtualDeviceApi.VIRTUAL_OPENRESULT.VIRTUAL_OPENRESULT_WMSERROR,
                    result,
                    "Failure after real loopback creation did not return the WMS error.");
                ClassicAssert.AreNotEqual(0, errorCode, "Creation rollback lost the additional SDK error.");
                ClassicAssert.AreEqual(IntPtr.Zero, failedInfo, "Failed virtual creation exposed partially initialized device info.");

                if (failLoopbackRemoval)
                {
                    ClassicAssert.AreEqual(
                        baselinePendingRemovals + 1,
                        context.GetCounter(WmsTestCounter.PendingLoopbackRemovals),
                        "Rollback removal failure was not queued for retry.");
                    ClassicAssert.IsTrue(
                        WaitOperations.Wait(
                            () => context.GetInputProperties(true).Contains(failedDeviceName) &&
                                context.GetOutputProperties(true).Contains(failedDeviceName),
                            WmsTestContext.Timeout),
                        "The injected failure did not leave the actual SDK loopback awaiting removal.");
                }

                context.SetFailure(WmsTestFailure.AfterLoopbackCreation, false);
                context.SetFailure(WmsTestFailure.RemoveLoopback, false);

                using (var retryDevice = context.CreateVirtualDevice())
                {
                    ClassicAssert.AreEqual(
                        baselinePendingRemovals,
                        context.GetCounter(WmsTestCounter.PendingLoopbackRemovals),
                        "Creating a new device did not retry queued loopback removals.");
                    AssertWmsLoopbackAbsent(context, failedDeviceName);
                    context.AssertNoTemporaryInfos(2);
                }

                context.AssertNoTemporaryInfos();
            }
        }

        [Test]
        [WinOnly]
        [VirtualDeviceApiRequired]
        public void WmsVirtualDeviceCloseFailurePreservesInfoForRetry()
        {
            using (var context = new WmsTestContext())
            using (var virtualDevice = context.CreateVirtualDevice())
            {
                var originalInfo = virtualDevice.Info;
                var originalInputInfo = virtualDevice.InputInfo;
                var originalOutputInfo = virtualDevice.OutputInfo;
                context.SetFailure(WmsTestFailure.RemoveLoopback, true);

                var result = virtualDevice.Close(out var errorCode);

                ClassicAssert.AreEqual(
                    VirtualDeviceApi.VIRTUAL_CLOSERESULT.VIRTUAL_CLOSERESULT_FAILED,
                    result,
                    "Injected loopback-removal failure did not return the WMS close error.");
                ClassicAssert.AreNotEqual(0, errorCode, "Close failure lost the additional SDK error.");
                ClassicAssert.AreEqual(originalInfo, virtualDevice.Info, "Failed close discarded raw virtual device info.");
                ClassicAssert.AreEqual(originalInputInfo, VirtualDeviceApi.Api_GetInputEndpointInfo(originalInfo), "Failed close discarded the input info.");
                ClassicAssert.AreEqual(originalOutputInfo, VirtualDeviceApi.Api_GetOutputEndpointInfo(originalInfo), "Failed close discarded the output info.");

                var inputHandle = context.OpenInput(originalInputInfo);
                context.StartListening(inputHandle);
                var outputHandle = context.OpenOutput(originalOutputInfo);
                context.SendAndReceive(outputHandle);
                context.CloseInput(inputHandle);
                context.CloseOutput(outputHandle);
                context.SetFailure(WmsTestFailure.RemoveLoopback, false);

                ClassicAssert.AreEqual(
                    VirtualDeviceApi.VIRTUAL_CLOSERESULT.VIRTUAL_CLOSERESULT_OK,
                    virtualDevice.Close(out _),
                    "Retrying close after clearing the failure did not succeed.");
                ClassicAssert.AreEqual(IntPtr.Zero, virtualDevice.Info, "Successful close left raw device info owned by the wrapper.");
                ClassicAssert.AreEqual(originalInputInfo, virtualDevice.InputInfo, "Successful close consumed separately owned input info.");
                ClassicAssert.AreEqual(originalOutputInfo, virtualDevice.OutputInfo, "Successful close consumed separately owned output info.");
                AssertWmsLoopbackAbsent(context, virtualDevice.Name);
            }
        }

        [Test]
        [WinOnly]
        [VirtualDeviceApiRequired]
        public void WmsAbandonedVirtualDeviceQueuesFailedRemovalUntilNextCreate()
        {
            using (var context = new WmsTestContext())
            using (var virtualDevice = context.CreateVirtualDevice())
            {
                var baselinePendingRemovals = context.GetCounter(WmsTestCounter.PendingLoopbackRemovals);
                context.SetFailure(WmsTestFailure.RemoveLoopback, true);
                ClassicAssert.AreEqual(
                    VirtualDeviceApi.VIRTUAL_CLOSERESULT.VIRTUAL_CLOSERESULT_FAILED,
                    virtualDevice.Close(out _),
                    "Injected removal failure did not reach the failed-release path.");

                virtualDevice.Abandon();

                ClassicAssert.AreEqual(IntPtr.Zero, virtualDevice.Info, "Abandon did not relinquish raw virtual device info.");
                ClassicAssert.AreEqual(
                    baselinePendingRemovals + 1,
                    context.GetCounter(WmsTestCounter.PendingLoopbackRemovals),
                    "Abandon did not queue the failed loopback removal.");
                context.SetFailure(WmsTestFailure.RemoveLoopback, false);

                using (var retryDevice = context.CreateVirtualDevice())
                {
                    ClassicAssert.AreEqual(
                        baselinePendingRemovals,
                        context.GetCounter(WmsTestCounter.PendingLoopbackRemovals),
                        "Next create did not clean up the abandoned SDK loopback.");
                    AssertWmsLoopbackAbsent(context, virtualDevice.Name);
                }
            }
        }

        [Test]
        [WinOnly]
        [VirtualDeviceApiRequired]
        public void WmsVirtualDeviceSafeHandleFailedReleaseQueuesRemoval()
        {
            using (var context = new WmsTestContext())
            using (var virtualDevice = context.CreateVirtualDevice())
            using (var handle = virtualDevice.TransferHandle())
            {
                var checkpoints = new TestCheckpoints();
                handle.TestCheckpoints = checkpoints;
                var baselinePendingRemovals = context.GetCounter(WmsTestCounter.PendingLoopbackRemovals);
                context.SetFailure(WmsTestFailure.RemoveLoopback, true);

                handle.Dispose();

                ClassicAssert.AreEqual(
                    baselinePendingRemovals + 1,
                    context.GetCounter(WmsTestCounter.PendingLoopbackRemovals),
                    "Failed SafeHandle release did not retain a loopback removal retry.");
                checkpoints.CheckCheckpointsReached(
                    VirtualDeviceCheckpointsNames.ReleaseHandleEntered,
                    VirtualDeviceCheckpointsNames.CloseDeviceExecutedInReleaseHandle);
                checkpoints.CheckCheckpointsAreNotReached(
                    VirtualDeviceCheckpointsNames.CloseDeviceSuccessInReleaseHandle);
                context.SetFailure(WmsTestFailure.RemoveLoopback, false);

                using (var retryDevice = context.CreateVirtualDevice())
                {
                    ClassicAssert.AreEqual(
                        baselinePendingRemovals,
                        context.GetCounter(WmsTestCounter.PendingLoopbackRemovals),
                        "Next creation did not retry the failed SafeHandle removal.");
                    AssertWmsLoopbackAbsent(context, virtualDevice.Name);
                }
            }
        }

        [Test]
        [WinOnly]
        [VirtualDeviceApiRequired]
        public void WmsConfigurationCleanupRetriesAbandonedLoopbackRemoval()
        {
            using (var observerContext = new WmsTestContext())
            using (var context = new WmsTestContext())
            {
                var virtualDevice = context.CreateVirtualDevice();
                var name = virtualDevice.Name;

                ClassicAssert.IsTrue(
                    WaitOperations.Wait(
                        () => observerContext.GetInputProperties(true).Contains(name) &&
                            observerContext.GetOutputProperties(true).Contains(name),
                        WmsTestContext.Timeout),
                    "Observer did not discover the loopback before configuration cleanup.");

                virtualDevice.Abandon();

                ClassicAssert.AreEqual(
                    1,
                    context.GetCounter(WmsTestCounter.PendingLoopbackRemovals),
                    "Abandoned loopback was not queued before configuration cleanup.");
                context.Dispose();

                AssertWmsLoopbackAbsent(observerContext, name);
            }
        }

        [Test]
        [WinOnly]
        [VirtualDeviceApiRequired]
        public void WmsPublicVirtualDeviceDisposeFailureThrowsAndPreservesUsability()
        {
            var configuration = MidiConfiguration.GetConfigurationHandle();
            WmsTestContext.Enable(configuration);
            var virtualDevice = VirtualDevice.Create("WmsDispose-" + Guid.NewGuid());
            var inputEndpoint = virtualDevice.InputEndpoint;
            var outputEndpoint = virtualDevice.OutputEndpoint;
            var name = virtualDevice.Name;
            var checkpoints = new TestCheckpoints();
            virtualDevice.TestCheckpoints = checkpoints;
            var receivedEvents = new ConcurrentQueue<MidiEvent>();
            var callbackExceptions = new ConcurrentQueue<Exception>();
            EventHandler<MidiEventReceivedEventArgs> receivedHandler = (_, eventArgs) =>
            {
                try
                {
                    receivedEvents.Enqueue(eventArgs.Event);
                }
                catch (Exception exception)
                {
                    callbackExceptions.Enqueue(exception);
                }
            };

            try
            {
                WmsTestContext.SetFailure(configuration, WmsTestFailure.RemoveLoopback, true);

                ClassicAssert.Throws<NativeApiException>(
                    () => virtualDevice.Dispose(),
                    "Public Dispose suppressed the native loopback-removal error.");

                checkpoints.CheckCheckpointsReached(
                    VirtualDeviceCheckpointsNames.DisposeEntered,
                    VirtualDeviceCheckpointsNames.CloseDeviceExecutedInDispose);
                checkpoints.CheckCheckpointsAreNotReached(
                    VirtualDeviceCheckpointsNames.CloseDeviceSuccessInDispose);
                ClassicAssert.AreEqual(name, virtualDevice.Name, "Failed Dispose made the virtual device name inaccessible.");
                ClassicAssert.AreSame(inputEndpoint, virtualDevice.InputEndpoint, "Failed Dispose replaced or invalidated the input endpoint.");
                ClassicAssert.AreSame(outputEndpoint, virtualDevice.OutputEndpoint, "Failed Dispose replaced or invalidated the output endpoint.");

                inputEndpoint.EventReceived += receivedHandler;
                inputEndpoint.StartEventsListening();
                outputEndpoint.PrepareForEventsSending();
                var noteOnEvent = new NoteOnEvent((SevenBitNumber)60, (SevenBitNumber)64);
                outputEndpoint.SendEvent(noteOnEvent);

                ClassicAssert.IsTrue(
                    WaitOperations.Wait(() => !receivedEvents.IsEmpty || !callbackExceptions.IsEmpty, WmsTestContext.Timeout),
                    "Failed Dispose left the virtual device unable to receive MIDI.");
                ClassicAssert.IsTrue(callbackExceptions.IsEmpty, $"Public input callback failed: {string.Join("; ", callbackExceptions)}.");
                ClassicAssert.IsTrue(receivedEvents.TryDequeue(out var receivedEvent), "No MIDI event is available after failed Dispose.");
                MidiAsserts.AreEqual(noteOnEvent, receivedEvent, false, "Failed Dispose changed loopback MIDI.");
                WmsTestContext.SetFailure(configuration, WmsTestFailure.RemoveLoopback, false);
                virtualDevice.Dispose();

                ClassicAssert.AreEqual(
                    1,
                    checkpoints.GetCheckpointDataList(VirtualDeviceCheckpointsNames.CloseDeviceExecutedInDispose).Count,
                    "Successful public Dispose closed the native device more than once.");
                virtualDevice.Dispose();
                ClassicAssert.AreEqual(
                    1,
                    checkpoints.GetCheckpointDataList(VirtualDeviceCheckpointsNames.CloseDeviceExecutedInDispose).Count,
                    "Repeated public Dispose attempted another native close.");
                ClassicAssert.Throws<ObjectDisposedException>(
                    () => { var endpoint = virtualDevice.InputEndpoint; },
                    "Successful Dispose retry left the virtual device accessible.");
            }
            finally
            {
                inputEndpoint.EventReceived -= receivedHandler;

                foreach (WmsTestFailure failure in Enum.GetValues(typeof(WmsTestFailure)))
                {
                    WmsTestContext.SetFailure(configuration, failure, false);
                }

                virtualDevice.Dispose();
            }
        }

        #endregion

        #region Private methods

        private static void AssertWmsLoopbackAbsent(WmsTestContext context, string name)
        {
            ClassicAssert.IsTrue(
                WaitOperations.Wait(
                    () => !context.GetInputProperties(true).Contains(name) &&
                        !context.GetOutputProperties(true).Contains(name),
                    WmsTestContext.Timeout),
                $"SDK loopback '{name}' remained in input/output enumeration after removal.");
            context.AssertCallbacksSucceeded();
        }

        #endregion
    }
}
#endif
