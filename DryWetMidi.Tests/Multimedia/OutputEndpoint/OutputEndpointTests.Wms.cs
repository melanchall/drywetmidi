#if TEST
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Attributes;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [NonParallelizable]
    public sealed partial class OutputEndpointTests
    {
        #region Test methods

        [Test]
        [WinOnly]
        [VirtualDeviceApiRequired]
        public void WmsOutputCloseReleasesHandleWhileSharedInputSurvives()
        {
            using (var context = new WmsTestContext())
            using (var virtualDevice = context.CreateVirtualDevice())
            {
                var baselineHandles = context.GetCounter(WmsTestCounter.LiveOutputHandles);
                var inputHandle = context.OpenInput(virtualDevice.InputInfo);
                context.StartListening(inputHandle);
                var baselineConnections = context.GetSessionCounter(WmsTestSessionCounter.CachedConnections);
                var baselinePlugins = context.GetSessionCounter(WmsTestSessionCounter.AttachedPlugins);
                var outputInfo = WmsTestContext.CloneOutputInfo(virtualDevice.OutputInfo);

                try
                {
                    for (var iteration = 0; iteration < 5; iteration++)
                    {
                        var outputHandle = context.OpenOutput(outputInfo);
                        context.SendAndReceive(outputHandle);
                        context.CloseOutput(outputHandle);

                        ClassicAssert.AreEqual(
                            baselineHandles,
                            context.GetCounter(WmsTestCounter.LiveOutputHandles),
                            $"Output close leaked a native handle at iteration {iteration}.");
                        ClassicAssert.AreEqual(
                            baselineConnections,
                            context.GetSessionCounter(WmsTestSessionCounter.CachedConnections),
                            $"Output close removed the shared input connection at iteration {iteration}.");
                        ClassicAssert.AreEqual(
                            baselinePlugins,
                            context.GetSessionCounter(WmsTestSessionCounter.AttachedPlugins),
                            $"Output close detached the surviving input listener at iteration {iteration}.");
                    }

                    var retryOutputHandle = context.OpenOutput(outputInfo);
                    context.SendAndReceive(retryOutputHandle);
                    context.CloseOutput(retryOutputHandle);
                }
                finally
                {
                    OutputEndpointApi.Api_DeleteEndpointInfo(outputInfo);
                }

                context.CloseInput(inputHandle);

                ClassicAssert.AreEqual(baselineHandles, context.GetCounter(WmsTestCounter.LiveOutputHandles), "Repeated output opens and closes leaked a handle.");
                ClassicAssert.AreEqual(0, context.GetSessionCounter(WmsTestSessionCounter.CachedConnections), "Last port close left a cached connection.");
                context.AssertCallbacksSucceeded();
            }
        }

        [Test]
        [WinOnly]
        [VirtualDeviceApiRequired]
        public void WmsOutputConnectionOpenFailureDoesNotCacheConnectionAndCanRetry()
        {
            using (var context = new WmsTestContext())
            using (var virtualDevice = context.CreateVirtualDevice())
            {
                var baselineHandles = context.GetCounter(WmsTestCounter.LiveOutputHandles);
                context.SetFailure(WmsTestFailure.OpenConnection, true);

                var result = context.OpenOutput(virtualDevice.OutputInfo, out var failedHandle, out var errorCode);

                ClassicAssert.AreEqual(
                    OutputEndpointApi.OUT_OPENRESULT.OUT_OPENRESULT_GETCONNECTION_OPENFAILED,
                    result,
                    $"Injected connection-open failure did not return the output open error; SDK error: {errorCode}.");
                ClassicAssert.AreEqual(IntPtr.Zero, failedHandle, "Failed output connection returned a live handle.");
                ClassicAssert.AreEqual(baselineHandles, context.GetCounter(WmsTestCounter.LiveOutputHandles), "Failed output connection leaked a handle.");
                ClassicAssert.AreEqual(0, context.GetSessionCounter(WmsTestSessionCounter.CachedConnections), "Failed output connection was cached.");

                context.SetFailure(WmsTestFailure.OpenConnection, false);
                var outputHandle = context.OpenOutput(virtualDevice.OutputInfo);
                var inputHandle = context.OpenInput(virtualDevice.InputInfo);
                context.StartListening(inputHandle);
                context.SendAndReceive(outputHandle);
                context.CloseOutput(outputHandle);
                context.CloseInput(inputHandle);

                ClassicAssert.AreEqual(baselineHandles, context.GetCounter(WmsTestCounter.LiveOutputHandles), "Retry leaked an output handle.");
                ClassicAssert.AreEqual(0, context.GetSessionCounter(WmsTestSessionCounter.CachedConnections), "Successful retry left a cached connection after close.");
            }
        }

        [Test]
        [WinOnly]
        [VirtualDeviceApiRequired]
        public void WmsOutputUnsupportedProtocolIsNotListedOrOpenedAndRejectsRetainedSends(
            [Values(WmsTestProtocol.UmpMidi2Only, WmsTestProtocol.UmpNegotiatedMidi2, WmsTestProtocol.UnknownUmp)] WmsTestProtocol protocol)
        {
            using (var context = new WmsTestContext())
            using (var receiverContext = new WmsTestContext())
            using (var virtualDevice = context.CreateVirtualDevice())
            {
                context.ProcessEndpoint(virtualDevice, true);
                OutputEndpointApi.Api_GetEndpointId(virtualDevice.OutputInfo, out var outputId, out _);
                InputEndpointApi.Api_GetEndpointId(virtualDevice.InputInfo, out var inputId, out _);
                ClassicAssert.Contains(outputId, context.GetOutputProperties(), "Supported loopback output is missing from enumeration.");
                ClassicAssert.IsTrue(
                    WaitOperations.Wait(() => receiverContext.GetInputProperties().Contains(inputId), WmsTestContext.Timeout),
                    "The unaffected receiver session did not discover the real SDK loopback.");
                var receiverInputInfo = receiverContext.GetInputInfo(inputId);

                try
                {
                    var outputHandle = context.OpenOutput(virtualDevice.OutputInfo);
                    var inputHandle = receiverContext.OpenInput(receiverInputInfo);
                    receiverContext.StartListening(inputHandle);
                    context.SendShort(outputHandle);
                    receiverContext.AssertReceivedMessage();

                    var baselineHandles = context.GetCounter(WmsTestCounter.LiveOutputHandles);
                    context.SetProtocol(protocol);
                    context.ProcessEndpoint(virtualDevice, true);

                    ClassicAssert.IsFalse(context.GetOutputProperties().Contains(outputId), "Unsupported output remained in the watcher cache.");
                    var openResult = context.OpenOutput(virtualDevice.OutputInfo, out var failedHandle, out var errorCode);

                    ClassicAssert.AreEqual(
                        OutputEndpointApi.OUT_OPENRESULT.OUT_OPENRESULT_UNSUPPORTEDPROTOCOL,
                        openResult,
                        $"Output open accepted unsupported protocol {protocol}; SDK error: {errorCode}.");
                    ClassicAssert.AreEqual(IntPtr.Zero, failedHandle, "Unsupported output open returned a native handle.");
                    ClassicAssert.AreEqual(baselineHandles, context.GetCounter(WmsTestCounter.LiveOutputHandles), "Unsupported output open leaked a handle.");

                    var shortResult = OutputEndpointApi.Api_SendShortEvent(outputHandle, context.Session, WmsTestContext.NoteOnMessage, out _);

                    ClassicAssert.AreEqual(
                        OutputEndpointApi.OUT_SENDSHORTRESULT.OUT_SENDSHORTRESULT_UNSUPPORTEDPROTOCOL,
                        shortResult,
                        "Retained output accepted a short message after transition to an unsupported protocol.");

                    var sysExData = new byte[] { 0xF0, 0x7D, 0x01, 0xF7 };
                    var sysExDataHandle = GCHandle.Alloc(sysExData, GCHandleType.Pinned);

                    try
                    {
                        var sysExResult = OutputEndpointApi.Api_SendSysExEvent_Win(
                            outputHandle,
                            context.Session,
                            sysExDataHandle.AddrOfPinnedObject(),
                            sysExData.Length,
                            out var dataOwnershipTransferred,
                            out _);

                        ClassicAssert.AreEqual(
                            OutputEndpointApi.OUT_SENDSYSEXRESULT.OUT_SENDSYSEXRESULT_UNSUPPORTEDPROTOCOL,
                            sysExResult,
                            "Retained output accepted SysEx after transition to an unsupported protocol.");
                        ClassicAssert.AreEqual(0, dataOwnershipTransferred, "Rejected SysEx transferred ownership of the caller's buffer.");
                    }
                    finally
                    {
                        sysExDataHandle.Free();
                    }

                    ClassicAssert.IsFalse(
                        WaitOperations.Wait(() => !receiverContext.ReceivedMessages.IsEmpty, TimeSpan.FromMilliseconds(250)),
                        "Unsupported short or SysEx send transferred MIDI bytes to an unaffected receiver.");
                    context.AssertCallbacksSucceeded();
                    receiverContext.AssertCallbacksSucceeded();

                    context.CloseOutput(outputHandle);
                    receiverContext.CloseInput(inputHandle);
                }
                finally
                {
                    receiverContext.ClosePorts();
                    InputEndpointApi.Api_DeleteEndpointInfo(receiverInputInfo);
                }
            }
        }

        [Test]
        [WinOnly]
        [VirtualDeviceApiRequired]
        public void WmsOutputProtocolTransitionAddsAndRemovesCachedEndpoint(
            [Values(WmsTestProtocol.NativeMidi1, WmsTestProtocol.UmpMidi1)] WmsTestProtocol supportedProtocol)
        {
            using (var context = new WmsTestContext())
            using (var virtualDevice = context.CreateVirtualDevice())
            {
                OutputEndpointApi.Api_GetEndpointId(virtualDevice.OutputInfo, out var outputId, out _);
                context.ProcessEndpoint(virtualDevice, false);
                context.SetProtocol(WmsTestProtocol.UnknownUmp);
                context.ProcessEndpoint(virtualDevice, true);
                ClassicAssert.IsFalse(context.GetOutputProperties().Contains(outputId), "Unsupported missing output was added.");

                context.SetProtocol(supportedProtocol);
                context.ProcessEndpoint(virtualDevice, true);

                ClassicAssert.Contains(outputId, context.GetOutputProperties(), "Transition to MIDI 1 did not add the output.");
                var outputHandle = context.OpenOutput(virtualDevice.OutputInfo);
                var inputHandle = context.OpenInput(virtualDevice.InputInfo);
                context.StartListening(inputHandle);
                context.SendAndReceive(outputHandle);
                context.CloseInput(inputHandle);
                context.CloseOutput(outputHandle);

                context.SetProtocol(WmsTestProtocol.UmpMidi2Only);
                context.ProcessEndpoint(virtualDevice, true);

                ClassicAssert.IsFalse(context.GetOutputProperties().Contains(outputId), "Transition away from MIDI 1 did not remove the output.");
                context.AssertCallbacksSucceeded();
            }
        }

        [Test]
        [WinOnly]
        [VirtualDeviceApiRequired]
        public void WmsOutputIdAndNameDependOnGroupNotBlockNumber(
            [Values(0, 15)] int groupIndex,
            [Values(1, 16)] int groupCount)
        {
            using (var context = new WmsTestContext())
            using (var virtualDevice = context.CreateVirtualDevice())
            {
                var firstGroup = groupCount == 1 ? groupIndex : 0;
                using (var outputEndpoint = new OutputEndpoint(
                    context.CreateEndpointInfo(virtualDevice, false, 1, firstGroup, groupCount, groupIndex),
                    MidiEndpoint.CreationContext.User))
                using (var renumberedOutputEndpoint = new OutputEndpoint(
                    context.CreateEndpointInfo(virtualDevice, false, 99, firstGroup, groupCount, groupIndex),
                    MidiEndpoint.CreationContext.User))
                using (var otherGroupOutputEndpoint = new OutputEndpoint(
                    context.CreateEndpointInfo(virtualDevice, false, 1, 0, 16, groupIndex == 0 ? 15 : 0),
                    MidiEndpoint.CreationContext.User))
                using (var inputEndpoint = new InputEndpoint(
                    context.CreateEndpointInfo(virtualDevice, true, 1, firstGroup, groupCount, groupIndex),
                    MidiEndpoint.CreationContext.User))
                {
                    var expectedName = groupCount == 1 ? "WMS test block" : $"WMS test block (Group {groupIndex + 1})";

                    ClassicAssert.AreEqual(outputEndpoint.Id, renumberedOutputEndpoint.Id, "Block renumbering changed the output ID.");
                    ClassicAssert.AreNotEqual(outputEndpoint.Id, otherGroupOutputEndpoint.Id, "Different groups have the same output ID.");
                    ClassicAssert.AreNotEqual(outputEndpoint.Id, inputEndpoint.Id, "Output and input IDs are not direction-qualified.");
                    ClassicAssert.IsTrue(
                        outputEndpoint.Id.EndsWith($"_out_{groupIndex}", StringComparison.Ordinal),
                        "Output ID does not use the zero-based group index.");
                    ClassicAssert.AreEqual(expectedName, outputEndpoint.Name, "Output name does not use one-based groups or single-group suppression.");

                    var outputHandle = context.OpenOutput(outputEndpoint.Handle.DangerousGetHandle());
                    context.CloseOutput(outputHandle);
                    var clonedInfo = WmsTestContext.CloneOutputInfo(outputEndpoint.Handle.DangerousGetHandle());

                    using (var clonedEndpoint = new OutputEndpoint(clonedInfo, MidiEndpoint.CreationContext.User))
                    {
                        ClassicAssert.AreEqual(outputEndpoint.Id, clonedEndpoint.Id, "Cloned output lost its group-qualified ID.");
                        ClassicAssert.AreEqual(outputEndpoint.Name, clonedEndpoint.Name, "Cloned output lost its group display name.");

                        var clonedHandle = context.OpenOutput(clonedInfo);
                        context.CloseOutput(clonedHandle);
                    }
                }

            }
        }

        #endregion
    }
}
#endif
