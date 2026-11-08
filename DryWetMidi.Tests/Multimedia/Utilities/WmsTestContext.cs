#if TEST
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Configuration;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    public enum WmsTestProtocol
    {
        Actual = 0,
        NativeMidi1 = 1,
        UmpMidi1 = 2,
        UmpMidi2Only = 3,
        UmpNegotiatedMidi2 = 4,
        UnknownUmp = 5
    }

    internal enum WmsTestFailure
    {
        OpenConnection = 0,
        AddPlugin = 1,
        AfterLoopbackCreation = 2,
        RemoveLoopback = 3
    }

    internal enum WmsTestCounter
    {
        LiveInputHandles = 0,
        LiveOutputHandles = 1,
        LiveEndpointInfos = 2,
        PluginsPresentBeforeOpen = 3,
        PendingLoopbackRemovals = 4
    }

    internal enum WmsTestSessionCounter
    {
        CachedConnections = 0,
        CachedEndpointInfos = 1,
        AttachedPlugins = 2
    }

    internal sealed class WmsTestContext : IDisposable
    {
        #region Constants

        public const int NoteOnMessage = 0x00403C90;

        public const int PluginInitializationFailureErrorCode = 4;

        #endregion

        #region Fields

        private readonly MidiConfigurationApi.NativeApiActivityCallback _activityCallback;
        private readonly MidiDevicesSessionApi.InputEndpointCallback _inputEndpointCallback;
        private readonly MidiDevicesSessionApi.OutputEndpointCallback _outputEndpointCallback;
        private readonly InputEndpointApi.Callback_Win _inputCallback;
        private readonly OutputEndpointApi.Callback_Win _outputCallback;
        private readonly InputEndpointApi.BytesReceivedCallback _bytesReceivedCallback;
        private readonly List<WmsTestVirtualDevice> _virtualDevices = new List<WmsTestVirtualDevice>();
        private readonly HashSet<IntPtr> _inputHandles = new HashSet<IntPtr>();
        private readonly HashSet<IntPtr> _outputHandles = new HashSet<IntPtr>();
        private bool _isDisposed;

        #endregion

        #region Constructor

        public WmsTestContext()
        {
            _activityCallback = _ => { };
            _inputEndpointCallback = (info, _) => CaptureCallback(() => InputEndpointApi.Api_DeleteEndpointInfo(info));
            _outputEndpointCallback = (info, _) => CaptureCallback(() => OutputEndpointApi.Api_DeleteEndpointInfo(info));
            _inputCallback = (handle, message, instance, parameter1, parameter2) => { };
            _outputCallback = (handle, message, instance, parameter1, parameter2) => { };
            _bytesReceivedCallback = (bytes, size, _) => CaptureCallback(() =>
            {
                var data = new byte[size];
                Marshal.Copy(bytes, data, 0, size);
                ReceivedMessages.Enqueue(data);
            });

            var configurationResult = MidiConfigurationApi.Api_GetConfiguration(
                true,
                false,
                _activityCallback,
                out var configuration,
                out var configurationError);

            ClassicAssert.AreEqual(
                MidiConfigurationApi.CONFIGURATION_GETRESULT.CONFIGURATION_GETRESULT_OK,
                configurationResult,
                $"Unable to create isolated WMS configuration; SDK error: {configurationError}.");

            Configuration = new MidiConfigurationHandle(configuration);

            try
            {
                Enable(Configuration);
                SetProtocol(WmsTestProtocol.UmpMidi1);

                var sessionResult = MidiDevicesSessionApi.Api_OpenSession(
                    Guid.NewGuid().ToString(),
                    Configuration,
                    _inputEndpointCallback,
                    _outputEndpointCallback,
                    out var session,
                    out var sessionError);

                ClassicAssert.AreEqual(
                    MidiDevicesSessionApi.SESSION_OPENRESULT.SESSION_OPENRESULT_OK,
                    sessionResult,
                    $"Unable to create isolated WMS session; SDK error: {sessionError}.");

                Session = new MidiDevicesSessionHandle(session);

                GetInputProperties();
                GetOutputProperties();
                AssertNoTemporaryInfos();
            }
            catch
            {
                Session?.Dispose();
                Configuration.Dispose();
                throw;
            }
        }

        #endregion

        #region Properties

        public static TimeSpan Timeout { get; } = TimeSpan.FromSeconds(10);

        public MidiConfigurationHandle Configuration { get; }

        public MidiDevicesSessionHandle Session { get; }

        public ConcurrentQueue<Exception> CallbackExceptions { get; } = new ConcurrentQueue<Exception>();

        public ConcurrentQueue<byte[]> ReceivedMessages { get; } = new ConcurrentQueue<byte[]>();

        #endregion

        #region Methods

        public static void Enable(MidiConfigurationHandle configuration)
        {
            MidiOperationsExecutor.Instance.ExecuteOperation(() => EnableWmsTests_Win(configuration.DangerousGetHandle()));
        }

        public static void SetFailure(MidiConfigurationHandle configuration, WmsTestFailure failure, bool isEnabled)
        {
            MidiOperationsExecutor.Instance.ExecuteOperation(() =>
                SetWmsTestFailure_Win(configuration.DangerousGetHandle(), (int)failure, isEnabled ? 1 : 0));
        }

        public void SetFailure(WmsTestFailure failure, bool isEnabled)
        {
            SetFailure(Configuration, failure, isEnabled);
        }

        public void SetProtocol(WmsTestProtocol protocol)
        {
            MidiOperationsExecutor.Instance.ExecuteOperation(() =>
                SetWmsTestProtocol_Win(Configuration.DangerousGetHandle(), (int)protocol));
        }

        public int GetCounter(WmsTestCounter counter)
        {
            return MidiOperationsExecutor.Instance.ExecuteOperation(() =>
                GetWmsTestCounter_Win(Configuration.DangerousGetHandle(), (int)counter));
        }

        public int GetSessionCounter(WmsTestSessionCounter counter)
        {
            return MidiOperationsExecutor.Instance.ExecuteOperation(() =>
                GetWmsTestSessionCounter_Win(Session.DangerousGetHandle(), (int)counter));
        }

        public void ProcessEndpoint(WmsTestVirtualDevice virtualDevice, bool isAdded)
        {
            MidiOperationsExecutor.Instance.ExecuteOperation(() =>
                ProcessWmsTestEndpoint_Win(Session.DangerousGetHandle(), virtualDevice.Info, isAdded ? 1 : 0));
        }

        public void SetEnumerationCompleted(bool isCompleted)
        {
            MidiOperationsExecutor.Instance.ExecuteOperation(() =>
                SetWmsTestEnumerationCompleted_Win(Session.DangerousGetHandle(), isCompleted ? 1 : 0));
        }

        public IntPtr CreateEndpointInfo(
            WmsTestVirtualDevice virtualDevice,
            bool isInput,
            int blockNumber,
            int firstGroup,
            int groupCount,
            int groupIndex)
        {
            var info = MidiOperationsExecutor.Instance.ExecuteOperation(() =>
                CreateWmsTestEndpointInfo_Win(
                    Configuration.DangerousGetHandle(),
                    virtualDevice.Info,
                    isInput ? 1 : 0,
                    blockNumber,
                    firstGroup,
                    groupCount,
                    groupIndex));

            ClassicAssert.AreNotEqual(IntPtr.Zero, info, "Fake group-terminal-block endpoint info was not created.");

            return info;
        }

        public static IntPtr CloneOutputInfo(IntPtr source)
        {
            var info = IntPtr.Zero;
            MidiOperationsExecutor.Instance.ExecuteOperation(() => CloneOutputEndpointInfo(source, out info));

            ClassicAssert.AreNotEqual(IntPtr.Zero, info, "Output endpoint info was not cloned.");

            return info;
        }

        public VirtualDeviceApi.VIRTUAL_OPENRESULT TryCreateVirtualDevice(string name, out IntPtr info, out int errorCode)
        {
            return VirtualDeviceApi.Api_OpenDevice_Win(name, Configuration, Session, out info, out errorCode);
        }

        public WmsTestVirtualDevice CreateVirtualDevice()
        {
            var name = "WmsTest-" + Guid.NewGuid();
            var result = TryCreateVirtualDevice(name, out var info, out var errorCode);

            ClassicAssert.AreEqual(
                VirtualDeviceApi.VIRTUAL_OPENRESULT.VIRTUAL_OPENRESULT_OK,
                result,
                $"Unable to create isolated WMS loopback; SDK error: {errorCode}.");

            var virtualDevice = new WmsTestVirtualDevice(this, info, name);
            _virtualDevices.Add(virtualDevice);

            return virtualDevice;
        }

        public InputEndpointApi.IN_OPENRESULT OpenInput(IntPtr info, out IntPtr handle, out int errorCode)
        {
            var result = InputEndpointApi.Api_OpenEndpoint_Win(
                info,
                Session,
                _inputCallback,
                _bytesReceivedCallback,
                1024,
                2,
                out handle,
                out errorCode);

            if (handle != IntPtr.Zero)
                _inputHandles.Add(handle);

            return result;
        }

        public OutputEndpointApi.OUT_OPENRESULT OpenOutput(IntPtr info, out IntPtr handle, out int errorCode)
        {
            var result = OutputEndpointApi.Api_OpenEndpoint_Win(info, Session, _outputCallback, out handle, out errorCode);

            if (handle != IntPtr.Zero)
                _outputHandles.Add(handle);

            return result;
        }

        public IntPtr OpenInput(IntPtr info)
        {
            var result = OpenInput(info, out var handle, out var errorCode);

            ClassicAssert.AreEqual(
                InputEndpointApi.IN_OPENRESULT.IN_OPENRESULT_OK,
                result,
                $"Input endpoint did not open; SDK error: {errorCode}.");
            ClassicAssert.AreNotEqual(IntPtr.Zero, handle, "Successful input open returned a null handle.");

            return handle;
        }

        public IntPtr OpenOutput(IntPtr info)
        {
            var result = OpenOutput(info, out var handle, out var errorCode);

            ClassicAssert.AreEqual(
                OutputEndpointApi.OUT_OPENRESULT.OUT_OPENRESULT_OK,
                result,
                $"Output endpoint did not open; SDK error: {errorCode}.");
            ClassicAssert.AreNotEqual(IntPtr.Zero, handle, "Successful output open returned a null handle.");

            return handle;
        }

        public void CloseInput(IntPtr handle)
        {
            var result = InputEndpointApi.Api_CloseEndpoint(handle, out var errorCode);

            ClassicAssert.AreEqual(
                InputEndpointApi.IN_CLOSERESULT.IN_CLOSERESULT_OK,
                result,
                $"Input endpoint did not close; SDK error: {errorCode}.");

            _inputHandles.Remove(handle);
        }

        public void CloseOutput(IntPtr handle)
        {
            var result = OutputEndpointApi.Api_CloseEndpoint(handle, out var errorCode);

            ClassicAssert.AreEqual(
                OutputEndpointApi.OUT_CLOSERESULT.OUT_CLOSERESULT_OK,
                result,
                $"Output endpoint did not close; SDK error: {errorCode}.");

            _outputHandles.Remove(handle);
        }

        public void StartListening(IntPtr handle)
        {
            var result = InputEndpointApi.Api_Connect(handle, Session, out var errorCode);

            ClassicAssert.AreEqual(
                InputEndpointApi.IN_CONNECTRESULT.IN_CONNECTRESULT_OK,
                result,
                $"Input endpoint did not start listening; SDK error: {errorCode}.");
        }

        public void SendAndReceive(IntPtr outputHandle)
        {
            while (ReceivedMessages.TryDequeue(out _))
            {
            }

            SendShort(outputHandle);
            AssertReceivedMessage();
        }

        public void SendShort(IntPtr outputHandle)
        {
            var result = OutputEndpointApi.Api_SendShortEvent(outputHandle, Session, NoteOnMessage, out var errorCode);

            ClassicAssert.AreEqual(
                OutputEndpointApi.OUT_SENDSHORTRESULT.OUT_SENDSHORTRESULT_OK,
                result,
                $"Output endpoint did not send a short event; SDK error: {errorCode}.");
        }

        public void AssertReceivedMessage()
        {
            ClassicAssert.IsTrue(
                WaitOperations.Wait(() => !ReceivedMessages.IsEmpty || !CallbackExceptions.IsEmpty, Timeout),
                "Input endpoint did not receive a loopback event within the bounded wait.");

            AssertCallbacksSucceeded();
            ClassicAssert.IsTrue(ReceivedMessages.TryDequeue(out var receivedMessage), "No received MIDI message is available.");
            ClassicAssert.AreEqual(new byte[] { 0x90, 0x3C, 0x40 }, receivedMessage, "Loopback MIDI bytes differ.");
        }

        public IntPtr GetOutputInfo(string endpointId)
        {
            var result = OutputEndpointApi.Api_GetEndpointsInfo(
                Configuration,
                Session,
                false,
                out var infos,
                out var count,
                out var errorCode);

            ClassicAssert.AreEqual(
                OutputEndpointApi.OUT_GETALLINFORESULT.OUT_GETALLINFORESULT_OK,
                result,
                $"Output enumeration failed; SDK error: {errorCode}.");

            try
            {
                for (var index = 0; index < count; index++)
                {
                    var info = Marshal.ReadIntPtr(infos, index * IntPtr.Size);
                    var propertyResult = OutputEndpointApi.Api_GetEndpointId(info, out var id, out errorCode);

                    ClassicAssert.AreEqual(
                        OutputEndpointApi.OUT_GETPROPERTYRESULT.OUT_GETPROPERTYRESULT_OK,
                        propertyResult,
                        $"Output ID lookup failed; SDK error: {errorCode}.");

                    if (id == endpointId)
                        return CloneOutputInfo(info);
                }
            }
            finally
            {
                for (var index = 0; index < count; index++)
                {
                    OutputEndpointApi.Api_DeleteEndpointInfo(Marshal.ReadIntPtr(infos, index * IntPtr.Size));
                }

                OutputEndpointApi.Api_FreeEndpointsInfo(infos, count);
            }

            ClassicAssert.Fail($"Output endpoint '{endpointId}' was not found in the unaffected session.");

            return IntPtr.Zero;
        }

        public IntPtr GetInputInfo(string endpointId)
        {
            var result = InputEndpointApi.Api_GetEndpointsInfo(
                Configuration,
                Session,
                out var infos,
                out var count,
                out var errorCode);

            ClassicAssert.AreEqual(
                InputEndpointApi.IN_GETALLINFORESULT.IN_GETALLINFORESULT_OK,
                result,
                $"Input enumeration failed; SDK error: {errorCode}.");

            try
            {
                for (var index = 0; index < count; index++)
                {
                    var info = Marshal.ReadIntPtr(infos, index * IntPtr.Size);
                    var propertyResult = InputEndpointApi.Api_GetEndpointId(info, out var id, out errorCode);

                    ClassicAssert.AreEqual(
                        InputEndpointApi.IN_GETPROPERTYRESULT.IN_GETPROPERTYRESULT_OK,
                        propertyResult,
                        $"Input ID lookup failed; SDK error: {errorCode}.");

                    if (id == endpointId)
                    {
                        InputEndpointApi.Api_CloneInputEndpointInfo(info, out var clonedInfo);

                        ClassicAssert.AreNotEqual(IntPtr.Zero, clonedInfo, "Input endpoint info was not cloned for the unaffected session.");

                        return clonedInfo;
                    }
                }
            }
            finally
            {
                for (var index = 0; index < count; index++)
                {
                    InputEndpointApi.Api_DeleteEndpointInfo(Marshal.ReadIntPtr(infos, index * IntPtr.Size));
                }

                InputEndpointApi.Api_FreeEndpointsInfo(infos, count);
            }

            ClassicAssert.Fail($"Input endpoint '{endpointId}' was not found in the unaffected session.");

            return IntPtr.Zero;
        }

        public List<string> GetInputProperties(bool getNames = false)
        {
            var result = InputEndpointApi.Api_GetEndpointsInfo(
                Configuration,
                Session,
                out var infos,
                out var count,
                out var errorCode);

            ClassicAssert.AreEqual(
                InputEndpointApi.IN_GETALLINFORESULT.IN_GETALLINFORESULT_OK,
                result,
                $"Input enumeration failed; SDK error: {errorCode}.");

            var properties = new List<string>();

            try
            {
                for (var index = 0; index < count; index++)
                {
                    var info = Marshal.ReadIntPtr(infos, index * IntPtr.Size);
                    var propertyResult = getNames
                        ? InputEndpointApi.Api_GetEndpointName(info, out var property, out errorCode)
                        : InputEndpointApi.Api_GetEndpointId(info, out property, out errorCode);

                    ClassicAssert.AreEqual(
                        InputEndpointApi.IN_GETPROPERTYRESULT.IN_GETPROPERTYRESULT_OK,
                        propertyResult,
                        $"Input property lookup failed; SDK error: {errorCode}.");
                    properties.Add(property);
                }
            }
            finally
            {
                for (var index = 0; index < count; index++)
                {
                    InputEndpointApi.Api_DeleteEndpointInfo(Marshal.ReadIntPtr(infos, index * IntPtr.Size));
                }

                InputEndpointApi.Api_FreeEndpointsInfo(infos, count);
            }

            return properties;
        }

        public List<string> GetOutputProperties(bool getNames = false)
        {
            var result = OutputEndpointApi.Api_GetEndpointsInfo(
                Configuration,
                Session,
                false,
                out var infos,
                out var count,
                out var errorCode);

            ClassicAssert.AreEqual(
                OutputEndpointApi.OUT_GETALLINFORESULT.OUT_GETALLINFORESULT_OK,
                result,
                $"Output enumeration failed; SDK error: {errorCode}.");

            var properties = new List<string>();

            try
            {
                for (var index = 0; index < count; index++)
                {
                    var info = Marshal.ReadIntPtr(infos, index * IntPtr.Size);
                    var propertyResult = getNames
                        ? OutputEndpointApi.Api_GetEndpointName(info, out var property, out errorCode)
                        : OutputEndpointApi.Api_GetEndpointId(info, out property, out errorCode);

                    ClassicAssert.AreEqual(
                        OutputEndpointApi.OUT_GETPROPERTYRESULT.OUT_GETPROPERTYRESULT_OK,
                        propertyResult,
                        $"Output property lookup failed; SDK error: {errorCode}.");
                    properties.Add(property);
                }
            }
            finally
            {
                for (var index = 0; index < count; index++)
                {
                    OutputEndpointApi.Api_DeleteEndpointInfo(Marshal.ReadIntPtr(infos, index * IntPtr.Size));
                }

                OutputEndpointApi.Api_FreeEndpointsInfo(infos, count);
            }

            return properties;
        }

        public void AssertCallbacksSucceeded()
        {
            ClassicAssert.IsTrue(CallbackExceptions.IsEmpty, $"Native callback failed: {string.Join("; ", CallbackExceptions)}.");
        }

        public void AssertNoTemporaryInfos(int additionalOwnedInfos = 0)
        {
            ClassicAssert.IsTrue(
                WaitOperations.Wait(
                    () => GetCounter(WmsTestCounter.LiveEndpointInfos) ==
                        GetSessionCounter(WmsTestSessionCounter.CachedEndpointInfos) + additionalOwnedInfos,
                    Timeout),
                $"Endpoint info ownership did not settle; live: {GetCounter(WmsTestCounter.LiveEndpointInfos)}, " +
                $"cached: {GetSessionCounter(WmsTestSessionCounter.CachedEndpointInfos)}, owned: {additionalOwnedInfos}.");

            AssertCallbacksSucceeded();
        }

        public void ClosePorts()
        {
            foreach (var inputHandle in _inputHandles)
            {
                InputEndpointApi.Api_CloseEndpoint(inputHandle, out _);
            }

            foreach (var outputHandle in _outputHandles)
            {
                OutputEndpointApi.Api_CloseEndpoint(outputHandle, out _);
            }

            _inputHandles.Clear();
            _outputHandles.Clear();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            foreach (WmsTestFailure failure in Enum.GetValues(typeof(WmsTestFailure)))
            {
                SetFailure(failure, false);
            }

            ClosePorts();

            foreach (var virtualDevice in _virtualDevices)
            {
                virtualDevice.Dispose();
            }

            Session.Dispose();
            Configuration.Dispose();
            _isDisposed = true;

            GC.KeepAlive(_inputEndpointCallback);
            GC.KeepAlive(_outputEndpointCallback);
            GC.KeepAlive(_inputCallback);
            GC.KeepAlive(_outputCallback);
            GC.KeepAlive(_bytesReceivedCallback);
            GC.KeepAlive(_activityCallback);
        }

        private void CaptureCallback(Action callback)
        {
            try
            {
                callback();
            }
            catch (Exception exception)
            {
                CallbackExceptions.Enqueue(exception);
            }
        }

        #endregion

        #region Extern functions

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void EnableWmsTests_Win(IntPtr configuration);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CloneOutputEndpointInfo(IntPtr source, out IntPtr info);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetWmsTestFailure_Win(IntPtr configuration, int failure, int enabled);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetWmsTestProtocol_Win(IntPtr configuration, int protocol);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetWmsTestCounter_Win(IntPtr configuration, int counter);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetWmsTestSessionCounter_Win(IntPtr session, int counter);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void ProcessWmsTestEndpoint_Win(IntPtr session, IntPtr virtualDeviceInfo, int isAdded);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern void SetWmsTestEnumerationCompleted_Win(IntPtr session, int completed);

        [DllImport(NativeApi.LibraryName, ExactSpelling = true, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CreateWmsTestEndpointInfo_Win(
            IntPtr configuration,
            IntPtr virtualDeviceInfo,
            int isInput,
            int blockNumber,
            int firstGroup,
            int groupCount,
            int groupIndex);

        #endregion
    }

    internal sealed class WmsTestVirtualDevice : IDisposable
    {
        #region Fields

        private readonly WmsTestContext _context;

        #endregion

        #region Constructor

        public WmsTestVirtualDevice(WmsTestContext context, IntPtr info, string name)
        {
            _context = context;
            Info = info;
            Name = name;
            InputInfo = VirtualDeviceApi.Api_GetInputEndpointInfo(info);
            OutputInfo = VirtualDeviceApi.Api_GetOutputEndpointInfo(info);
        }

        #endregion

        #region Properties

        public IntPtr Info { get; private set; }

        public string Name { get; }

        public IntPtr InputInfo { get; private set; }

        public IntPtr OutputInfo { get; private set; }

        #endregion

        #region Methods

        public VirtualDeviceApi.VIRTUAL_CLOSERESULT Close(out int errorCode)
        {
            var result = VirtualDeviceApi.Api_CloseDevice(Info, out errorCode);

            if (result == VirtualDeviceApi.VIRTUAL_CLOSERESULT.VIRTUAL_CLOSERESULT_OK)
                Info = IntPtr.Zero;

            return result;
        }

        public void Abandon()
        {
            VirtualDeviceApi.Api_AbandonDevice_Win(Info);
            Info = IntPtr.Zero;
        }

        public VirtualDeviceHandle TransferHandle()
        {
            var handle = new VirtualDeviceHandle(Info);
            Info = IntPtr.Zero;

            return handle;
        }

        public void Dispose()
        {
            if (Info == IntPtr.Zero && InputInfo == IntPtr.Zero && OutputInfo == IntPtr.Zero)
                return;

            _context.ClosePorts();
            _context.SetFailure(WmsTestFailure.RemoveLoopback, false);

            if (Info != IntPtr.Zero)
            {
                var result = Close(out _);
                if (result != VirtualDeviceApi.VIRTUAL_CLOSERESULT.VIRTUAL_CLOSERESULT_OK)
                    Abandon();
            }

            if (InputInfo != IntPtr.Zero)
            {
                InputEndpointApi.Api_DeleteEndpointInfo(InputInfo);
                InputInfo = IntPtr.Zero;
            }

            if (OutputInfo != IntPtr.Zero)
            {
                OutputEndpointApi.Api_DeleteEndpointInfo(OutputInfo);
                OutputInfo = IntPtr.Zero;
            }
        }

        #endregion
    }
}
#endif
