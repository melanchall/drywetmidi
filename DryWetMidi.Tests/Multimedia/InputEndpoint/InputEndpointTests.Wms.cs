#if TEST
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Attributes;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [NonParallelizable]
    public sealed partial class InputEndpointTests
    {
        #region Test methods

        [Test]
        [WinOnly]
        [VirtualDeviceApiRequired]
        public void InputCloseReleasesHandleAndListenerWhileSharedOutputSurvives()
        {
            using (var context = new WmsTestContext())
            using (var virtualDevice = context.CreateVirtualDevice())
            {
                var baselineHandles = context.GetCounter(WmsTestCounter.LiveInputHandles);
                var outputHandle = context.OpenOutput(virtualDevice.OutputInfo);
                var baselinePlugins = context.GetSessionCounter(WmsTestSessionCounter.AttachedPlugins);
                var baselineConnections = context.GetSessionCounter(WmsTestSessionCounter.CachedConnections);

                InputEndpointApi.Api_CloneInputEndpointInfo(virtualDevice.InputInfo, out var inputInfo);

                try
                {
                    for (var iteration = 0; iteration < 5; iteration++)
                    {
                        var inputHandle = context.OpenInput(inputInfo);
                        context.StartListening(inputHandle);
                        context.SendAndReceive(outputHandle);
                        context.CloseInput(inputHandle);

                        ClassicAssert.AreEqual(
                            baselineHandles,
                            context.GetCounter(WmsTestCounter.LiveInputHandles),
                            $"Input close leaked a native handle at iteration {iteration}.");
                        ClassicAssert.AreEqual(
                            baselinePlugins,
                            context.GetSessionCounter(WmsTestSessionCounter.AttachedPlugins),
                            $"Input close left a stale listener at iteration {iteration}.");
                        ClassicAssert.AreEqual(
                            baselineConnections,
                            context.GetSessionCounter(WmsTestSessionCounter.CachedConnections),
                            $"Input close removed the shared output connection at iteration {iteration}.");
                        ClassicAssert.AreEqual(
                            OutputEndpointApi.OUT_SENDSHORTRESULT.OUT_SENDSHORTRESULT_OK,
                            OutputEndpointApi.Api_SendShortEvent(outputHandle, context.Session, WmsTestContext.NoteOnMessage, out _),
                            $"Surviving output could not send after input close at iteration {iteration}.");
                    }
                }
                finally
                {
                    context.ClosePorts();
                    InputEndpointApi.Api_DeleteEndpointInfo(inputInfo);
                }

                ClassicAssert.AreEqual(0, context.GetSessionCounter(WmsTestSessionCounter.CachedConnections), "Last port close left a cached connection.");
                context.AssertCallbacksSucceeded();
            }
        }

        [Test]
        [WinOnly]
        [VirtualDeviceApiRequired]
        public void WmsInputListenerIsAttachedBeforeConnectionOpens()
        {
            using (var context = new WmsTestContext())
            using (var virtualDevice = context.CreateVirtualDevice())
            {
                var baselinePluginsBeforeOpen = context.GetCounter(WmsTestCounter.PluginsPresentBeforeOpen);

                var inputHandle = context.OpenInput(virtualDevice.InputInfo);

                ClassicAssert.Greater(
                    context.GetCounter(WmsTestCounter.PluginsPresentBeforeOpen),
                    baselinePluginsBeforeOpen,
                    "The first input connection opened before its listener was attached.");
                context.StartListening(inputHandle);
                var outputHandle = context.OpenOutput(virtualDevice.OutputInfo);
                context.SendAndReceive(outputHandle);
            }
        }

        [Test]
        [WinOnly]
        [VirtualDeviceApiRequired]
        public void WmsInputPluginAddFailureRollsBackAndCanRetry([Values(false, true)] bool reuseConnection)
        {
            using (var context = new WmsTestContext())
            using (var virtualDevice = context.CreateVirtualDevice())
            {
                var outputHandle = reuseConnection ? context.OpenOutput(virtualDevice.OutputInfo) : IntPtr.Zero;
                var baselineHandles = context.GetCounter(WmsTestCounter.LiveInputHandles);
                var baselineConnections = context.GetSessionCounter(WmsTestSessionCounter.CachedConnections);
                var baselinePlugins = context.GetSessionCounter(WmsTestSessionCounter.AttachedPlugins);
                context.SetFailure(WmsTestFailure.AddPlugin, true);

                var result = context.OpenInput(virtualDevice.InputInfo, out var failedHandle, out var errorCode);

                ClassicAssert.AreEqual(
                    InputEndpointApi.IN_OPENRESULT.IN_OPENRESULT_PLUGINADDFAILED,
                    result,
                    "Injected plugin-add failure did not return its dedicated error.");
                ClassicAssert.AreEqual(
                    WmsTestContext.PluginInitializationFailureErrorCode,
                    errorCode,
                    "Plugin-add failure lost the SDK initialization failure code.");
                ClassicAssert.AreEqual(IntPtr.Zero, failedHandle, "Plugin-add failure returned a live input handle.");
                ClassicAssert.AreEqual(baselineHandles, context.GetCounter(WmsTestCounter.LiveInputHandles), "Plugin-add failure leaked an input handle.");
                ClassicAssert.AreEqual(baselineConnections, context.GetSessionCounter(WmsTestSessionCounter.CachedConnections), "Plugin-add failure changed the connection cache.");
                ClassicAssert.AreEqual(baselinePlugins, context.GetSessionCounter(WmsTestSessionCounter.AttachedPlugins), "Plugin-add failure leaked a listener.");

                context.SetFailure(WmsTestFailure.AddPlugin, false);
                var inputHandle = context.OpenInput(virtualDevice.InputInfo);
                context.StartListening(inputHandle);
                if (!reuseConnection)
                    outputHandle = context.OpenOutput(virtualDevice.OutputInfo);

                context.SendAndReceive(outputHandle);
                context.CloseInput(inputHandle);
                context.CloseOutput(outputHandle);

                ClassicAssert.AreEqual(baselineHandles, context.GetCounter(WmsTestCounter.LiveInputHandles), "Retry leaked an input handle.");
                ClassicAssert.AreEqual(0, context.GetSessionCounter(WmsTestSessionCounter.CachedConnections), "Retry left a cached connection after both ports closed.");
                ClassicAssert.AreEqual(0, context.GetSessionCounter(WmsTestSessionCounter.AttachedPlugins), "Retry left a stale input listener.");
            }
        }

        [Test]
        [WinOnly]
        [VirtualDeviceApiRequired]
        public void WmsInputConnectionOpenFailureDoesNotCacheConnectionAndCanRetry()
        {
            using (var context = new WmsTestContext())
            using (var virtualDevice = context.CreateVirtualDevice())
            {
                var baselineHandles = context.GetCounter(WmsTestCounter.LiveInputHandles);
                context.SetFailure(WmsTestFailure.OpenConnection, true);

                var result = context.OpenInput(virtualDevice.InputInfo, out var failedHandle, out var errorCode);

                ClassicAssert.AreEqual(
                    InputEndpointApi.IN_OPENRESULT.IN_OPENRESULT_GETCONNECTION_OPENFAILED,
                    result,
                    "Injected connection-open failure did not return the input open error.");
                ClassicAssert.AreEqual(IntPtr.Zero, failedHandle, "Failed input connection returned a live handle.");
                ClassicAssert.AreEqual(baselineHandles, context.GetCounter(WmsTestCounter.LiveInputHandles), "Failed input connection leaked a handle.");
                ClassicAssert.AreEqual(0, context.GetSessionCounter(WmsTestSessionCounter.CachedConnections), "Failed input connection was cached.");
                ClassicAssert.AreEqual(0, context.GetSessionCounter(WmsTestSessionCounter.AttachedPlugins), "Failed input connection retained a listener.");

                context.SetFailure(WmsTestFailure.OpenConnection, false);
                var inputHandle = context.OpenInput(virtualDevice.InputInfo);
                context.StartListening(inputHandle);
                var outputHandle = context.OpenOutput(virtualDevice.OutputInfo);
                context.SendAndReceive(outputHandle);
                context.CloseInput(inputHandle);
                context.CloseOutput(outputHandle);

                ClassicAssert.AreEqual(baselineHandles, context.GetCounter(WmsTestCounter.LiveInputHandles), $"Retry leaked a handle; original SDK error: {errorCode}.");
                ClassicAssert.AreEqual(0, context.GetSessionCounter(WmsTestSessionCounter.CachedConnections), "Successful retry left a cached connection after close.");
            }
        }

        [Test]
        [WinOnly]
        [VirtualDeviceApiRequired]
        public void WmsInputUnsupportedProtocolIsNotListedOrOpened(
            [Values(WmsTestProtocol.UmpMidi2Only, WmsTestProtocol.UmpNegotiatedMidi2, WmsTestProtocol.UnknownUmp)] WmsTestProtocol protocol)
        {
            using (var context = new WmsTestContext())
            using (var virtualDevice = context.CreateVirtualDevice())
            {
                context.ProcessEndpoint(virtualDevice, true);
                InputEndpointApi.Api_GetEndpointId(virtualDevice.InputInfo, out var inputId, out _);
                ClassicAssert.Contains(inputId, context.GetInputProperties(), "Supported loopback input is missing from enumeration.");
                var baselineHandles = context.GetCounter(WmsTestCounter.LiveInputHandles);

                context.SetProtocol(protocol);
                context.ProcessEndpoint(virtualDevice, true);
                var result = context.OpenInput(virtualDevice.InputInfo, out var handle, out var errorCode);

                ClassicAssert.IsFalse(context.GetInputProperties().Contains(inputId), "Unsupported input remained in the watcher cache.");
                ClassicAssert.AreEqual(
                    InputEndpointApi.IN_OPENRESULT.IN_OPENRESULT_UNSUPPORTEDPROTOCOL,
                    result,
                    $"Input open accepted unsupported protocol {protocol}; SDK error: {errorCode}.");
                ClassicAssert.AreEqual(IntPtr.Zero, handle, "Unsupported input open returned a native handle.");
                ClassicAssert.AreEqual(baselineHandles, context.GetCounter(WmsTestCounter.LiveInputHandles), "Unsupported input open leaked a handle.");
                context.AssertCallbacksSucceeded();
            }
        }

        [Test]
        [WinOnly]
        [VirtualDeviceApiRequired]
        public void WmsInputProtocolTransitionAddsAndRemovesCachedEndpoint(
            [Values(WmsTestProtocol.NativeMidi1, WmsTestProtocol.UmpMidi1)] WmsTestProtocol supportedProtocol)
        {
            using (var context = new WmsTestContext())
            using (var virtualDevice = context.CreateVirtualDevice())
            {
                InputEndpointApi.Api_GetEndpointId(virtualDevice.InputInfo, out var inputId, out _);
                context.ProcessEndpoint(virtualDevice, false);
                context.SetProtocol(WmsTestProtocol.UmpMidi2Only);
                context.ProcessEndpoint(virtualDevice, true);
                ClassicAssert.IsFalse(context.GetInputProperties().Contains(inputId), "Unsupported missing input was added.");

                context.SetProtocol(supportedProtocol);
                context.ProcessEndpoint(virtualDevice, true);

                ClassicAssert.Contains(inputId, context.GetInputProperties(), "Transition to MIDI 1 did not add the input.");
                var inputHandle = context.OpenInput(virtualDevice.InputInfo);
                context.CloseInput(inputHandle);

                context.SetProtocol(WmsTestProtocol.UmpNegotiatedMidi2);
                context.ProcessEndpoint(virtualDevice, true);

                ClassicAssert.IsFalse(context.GetInputProperties().Contains(inputId), "Transition away from MIDI 1 did not remove the input.");
                context.AssertCallbacksSucceeded();
            }
        }

        [Test]
        [WinOnly]
        [VirtualDeviceApiRequired]
        public void WmsInputProtocolTransitionSuppressesAndResumesAlreadyOpenListener(
            [Values(WmsTestProtocol.UmpMidi2Only, WmsTestProtocol.UmpNegotiatedMidi2, WmsTestProtocol.UnknownUmp)] WmsTestProtocol unsupportedProtocol)
        {
            using (var context = new WmsTestContext())
            using (var senderContext = new WmsTestContext())
            using (var virtualDevice = context.CreateVirtualDevice())
            {
                context.ProcessEndpoint(virtualDevice, true);
                OutputEndpointApi.Api_GetEndpointId(virtualDevice.OutputInfo, out var outputId, out _);
                ClassicAssert.IsTrue(
                    WaitOperations.Wait(() => senderContext.GetOutputProperties().Contains(outputId), WmsTestContext.Timeout),
                    "The unaffected sender session did not discover the real SDK loopback.");
                var senderOutputInfo = senderContext.GetOutputInfo(outputId);

                try
                {
                    var inputHandle = context.OpenInput(virtualDevice.InputInfo);
                    context.StartListening(inputHandle);
                    var outputHandle = senderContext.OpenOutput(senderOutputInfo);
                    senderContext.SendShort(outputHandle);
                    context.AssertReceivedMessage();

                    context.SetProtocol(unsupportedProtocol);
                    context.ProcessEndpoint(virtualDevice, true);
                    senderContext.SendShort(outputHandle);

                    ClassicAssert.IsFalse(
                        WaitOperations.Wait(() => !context.ReceivedMessages.IsEmpty, TimeSpan.FromMilliseconds(250)),
                        "Already-open input delivered MIDI after its protocol became unsupported.");
                    context.AssertCallbacksSucceeded();

                    context.SetProtocol(WmsTestProtocol.UmpMidi1);
                    context.ProcessEndpoint(virtualDevice, true);
                    senderContext.SendShort(outputHandle);

                    context.AssertReceivedMessage();
                    context.CloseInput(inputHandle);
                    senderContext.CloseOutput(outputHandle);
                    ClassicAssert.AreEqual(0, context.GetCounter(WmsTestCounter.LiveInputHandles), "Protocol transition leaked the retained input handle.");
                }
                finally
                {
                    senderContext.ClosePorts();
                    OutputEndpointApi.Api_DeleteEndpointInfo(senderOutputInfo);
                }
            }
        }

        [Test]
        [WinOnly]
        [VirtualDeviceApiRequired]
        public void WmsInputIdAndNameDependOnGroupNotBlockNumber(
            [Values(0, 15)] int groupIndex,
            [Values(1, 16)] int groupCount)
        {
            using (var context = new WmsTestContext())
            using (var virtualDevice = context.CreateVirtualDevice())
            {
                var firstGroup = groupCount == 1 ? groupIndex : 0;
                using (var inputEndpoint = new InputEndpoint(
                    context.CreateEndpointInfo(virtualDevice, true, 1, firstGroup, groupCount, groupIndex),
                    MidiEndpoint.CreationContext.User))
                using (var renumberedInputEndpoint = new InputEndpoint(
                    context.CreateEndpointInfo(virtualDevice, true, 99, firstGroup, groupCount, groupIndex),
                    MidiEndpoint.CreationContext.User))
                using (var otherGroupInputEndpoint = new InputEndpoint(
                    context.CreateEndpointInfo(virtualDevice, true, 1, 0, 16, groupIndex == 0 ? 15 : 0),
                    MidiEndpoint.CreationContext.User))
                using (var outputEndpoint = new OutputEndpoint(
                    context.CreateEndpointInfo(virtualDevice, false, 1, firstGroup, groupCount, groupIndex),
                    MidiEndpoint.CreationContext.User))
                {
                    var expectedName = groupCount == 1 ? "WMS test block" : $"WMS test block (Group {groupIndex + 1})";

                    ClassicAssert.AreEqual(inputEndpoint.Id, renumberedInputEndpoint.Id, "Block renumbering changed the input ID.");
                    ClassicAssert.AreNotEqual(inputEndpoint.Id, otherGroupInputEndpoint.Id, "Different groups have the same input ID.");
                    ClassicAssert.AreNotEqual(inputEndpoint.Id, outputEndpoint.Id, "Input and output IDs are not direction-qualified.");
                    ClassicAssert.IsTrue(
                        inputEndpoint.Id.EndsWith($"_in_{groupIndex}", StringComparison.Ordinal),
                        "Input ID does not use the zero-based group index.");
                    ClassicAssert.AreEqual(expectedName, inputEndpoint.Name, "Input name does not use one-based groups or single-group suppression.");

                    var inputHandle = context.OpenInput(inputEndpoint.Handle.DangerousGetHandle());
                    context.CloseInput(inputHandle);
                    InputEndpointApi.Api_CloneInputEndpointInfo(inputEndpoint.Handle.DangerousGetHandle(), out var clonedInfo);

                    using (var clonedEndpoint = new InputEndpoint(clonedInfo, MidiEndpoint.CreationContext.User))
                    {
                        ClassicAssert.AreEqual(inputEndpoint.Id, clonedEndpoint.Id, "Cloned input lost its group-qualified ID.");
                        ClassicAssert.AreEqual(inputEndpoint.Name, clonedEndpoint.Name, "Cloned input lost its group display name.");

                        var clonedHandle = context.OpenInput(clonedInfo);
                        context.CloseInput(clonedHandle);
                    }
                }

            }
        }

        #endregion
    }
}
#endif
