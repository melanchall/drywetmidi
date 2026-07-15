using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Configuration;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Represents an output MIDI endpoint. More info in the
    /// <see href="xref:a_dev_overview">Devices</see> and
    /// <see href="xref:a_dev_output">Output endpoint</see> articles.
    /// </summary>
    /// <remarks>
    /// <os-specific-api/>
    /// </remarks>
    public sealed class OutputEndpoint : MidiEndpoint, IOutputEndpoint
    {
        #region Constants

        private const int ShortEventBufferSize = 3;

        private static readonly IEventWriter ChannelEventWriter = new ChannelEventWriter();
        private static readonly IEventWriter SystemRealTimeEventWriter = new SystemRealTimeEventWriter();

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a MIDI event is sent.
        /// </summary>
        public event EventHandler<MidiEventSentEventArgs> EventSent;

        #endregion

        #region Fields

        private readonly MidiEventToBytesConverter _midiEventToBytesConverter = new MidiEventToBytesConverter(ShortEventBufferSize) { BytesFormat = BytesFormat.Device };
        private readonly BytesToMidiEventConverter _bytesToMidiEventConverter = new BytesToMidiEventConverter { BytesFormat = BytesFormat.Device };

        private OutputEndpointApi.Callback_Win _callback;

        private readonly CommonApi.API_TYPE _apiType;

        private string _id;

        #endregion

        #region Constructor

        internal OutputEndpoint(IntPtr info, CreationContext context)
            : base(context)
        {
            Info = new OutputEndpointInfo(info);
            _apiType = CommonApi.Api_GetApiType();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the current MIDI endpoint.
        /// </summary>
        public override string Name
        {
            get
            {
                EnsureSessionIsCreated();
                EnsureEndpointIsNotRemoved();

                var result = OutputEndpointApi.Api_GetEndpointName(Info.DangerousGetHandle(), out var name, out var errorCode);
                NativeApiUtilities.HandleEndpointNativeApiResult(result, errorCode);

                return name;
            }
        }

        public override string Id
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_id))
                    return _id;

                var result = OutputEndpointApi.Api_GetEndpointId(Info.DangerousGetHandle(), out _id, out var errorCode);
                NativeApiUtilities.HandleEndpointNativeApiResult(result, errorCode);
                
                return _id;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sends a MIDI event to the current output endpoint.
        /// </summary>
        /// <param name="midiEvent">MIDI event to send.</param>
        /// <exception cref="ObjectDisposedException">The current <see cref="OutputEndpoint"/> is disposed.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvent"/> is <c>null</c>.</exception>
        /// <exception cref="NativeApiException">An error occurred on endpoint.</exception>
        /// <exception cref="ArgumentException"><c>EscapeSysExEvent</c> is prohibited. Use <c>NormalSysExEvent</c> instead.</exception>
        public void SendEvent(MidiEvent midiEvent)
        {
            ThrowIfArgument.IsNull(nameof(midiEvent), midiEvent);
            ThrowIfArgument.IsOfType<EscapeSysExEvent>(
                nameof(midiEvent),
                midiEvent,
                "EscapeSysExEvent is prohibited. Use NormalSysExEvent instead.");

            if (!IsEnabled)
                return;

            EnsureEndpointIsNotDisposed();
            EnsureEndpointIsNotRemoved();
            EnsureSessionIsCreated();
            EnsureHandleIsCreated();

            if (midiEvent is ChannelEvent || midiEvent is SystemCommonEvent || midiEvent is SystemRealTimeEvent)
            {
                var message = PackShortEvent(midiEvent);
                var result = OutputEndpointApi.Api_SendShortEvent(Handle.DangerousGetHandle(), message, out var errorCode);
                NativeApiUtilities.HandleEndpointNativeApiResult(result, errorCode);
            }
            else
            {
                var sysExEvent = midiEvent as SysExEvent;
                if (sysExEvent != null)
                    SendSysExEvent(sysExEvent);
            }

            OnEventSent(midiEvent);
        }

        /// <summary>
        /// Turns off all notes that were turned on by sending Note On events, and which haven't
        /// yet been turned off by respective Note Off events.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current <see cref="OutputEndpoint"/> is disposed.</exception>
        /// <exception cref="NativeApiException">An error occurred on endpoint.</exception>
        public void TurnAllNotesOff()
        {
            EnsureEndpointIsNotDisposed();
            EnsureEndpointIsNotRemoved();
            EnsureSessionIsCreated();
            EnsureHandleIsCreated();

            var allNotesOffEvents = from channel in FourBitNumber.Values
                                    from noteNumber in SevenBitNumber.Values
                                    select new NoteOffEvent(noteNumber, SevenBitNumber.MinValue) { Channel = channel };

            foreach (var noteOffEvent in allNotesOffEvents)
            {
                SendEvent(noteOffEvent);
            }
        }

        /// <summary>
        /// Prepares output MIDI endpoint for sending events to it allocating necessary
        /// resources.
        /// </summary>
        /// <remarks>It is not needed to call this method before actual MIDI data
        /// sending since first call of <see cref="SendEvent(MidiEvent)"/> will prepare
        /// the endpoint automatically. But it can take some time so you may decide
        /// to call <see cref="PrepareForEventsSending"/> before working with endpoint.</remarks>
        /// <exception cref="NativeApiException">An error occurred on endpoint.</exception>
        public void PrepareForEventsSending()
        {
            EnsureSessionIsCreated();
            EnsureHandleIsCreated();
        }

        /// <summary>
        /// Retrieves the number of output MIDI endpoints presented in the system.
        /// </summary>
        /// <returns>Number of output MIDI endpoints presented in the system.</returns>
        /// <exception cref="NativeApiException">An error occurred.</exception>
        public static int GetEndpointsCount()
        {
            NativeApiUtilities.EnsureOsIsSupported();
            EnsureSessionIsCreated();

            var result = OutputEndpointApi.Api_GetEndpointsCount(out var count);
            NativeApiUtilities.HandleEndpointNativeApiResult(result, 0);

            return count;
        }

        /// <summary>
        /// Retrieves all output MIDI endpoints presented in the system.
        /// </summary>
        /// <returns>All output MIDI endpoints presented in the system.</returns>
        /// <exception cref="NativeApiException">An error occurred.</exception>
        public static ICollection<OutputEndpoint> GetAll()
        {
            NativeApiUtilities.EnsureOsIsSupported();
            EnsureSessionIsCreated();

            var result = OutputEndpointApi.Api_GetEndpointsInfo(MidiConfiguration.GetConfigurationHandle(), MidiDevicesSession.GetSessionHandle(), out var endpointsInfo, out var endpointsCount, out var error);
            NativeApiUtilities.HandleEndpointNativeApiResult(result, error);

            var endpoints = new OutputEndpoint[endpointsCount];

            for (int i = 0; i < endpointsCount; i++)
            {
                var info = Marshal.ReadIntPtr(endpointsInfo, i * IntPtr.Size);
                endpoints[i] = new OutputEndpoint(info, MidiEndpoint.CreationContext.User);
            }

            OutputEndpointApi.Api_FreeEndpointsInfo(endpointsInfo, endpointsCount);

            return endpoints;
        }

        /// <summary>
        /// Retrieves a first output MIDI endpoint with the specified name.
        /// </summary>
        /// <param name="name">The name of an output MIDI endpoint to retrieve.</param>
        /// <returns>Output MIDI endpoint with the specified name.</returns>
        /// <exception cref="ArgumentException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="name"/> is <c>null</c> or contains white-spaces only.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="name"/> specifies an output MIDI endpoint which is not presented in the system.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="NativeApiException">An error occurred.</exception>
        public static OutputEndpoint GetByName(string name)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(name), name, "Endpoint name");

            NativeApiUtilities.EnsureOsIsSupported();
            EnsureSessionIsCreated();

            var endpoint = GetAll().FirstOrDefault(d => d.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (endpoint == null)
                throw new ArgumentException($"There is no MIDI output endpoint '{name}'.", nameof(name));

            return endpoint;
        }

        internal void SendData_Win(byte[] data)
        {
            EnsureEndpointIsNotDisposed();
            EnsureEndpointIsNotRemoved();
            EnsureSessionIsCreated();
            EnsureHandleIsCreated();

            var bufferLength = data.Length;
            var bufferPointer = Marshal.AllocHGlobal(bufferLength);
            Marshal.Copy(data, 0, bufferPointer, data.Length);

            var result = OutputEndpointApi.Api_SendSysExEvent_Win(Handle.DangerousGetHandle(), bufferPointer, bufferLength, out var errorCode);
            if (result != OutputEndpointApi.OUT_SENDSYSEXRESULT.OUT_SENDSYSEXRESULT_OK)
                Marshal.FreeHGlobal(bufferPointer);
            
            NativeApiUtilities.HandleEndpointNativeApiResult(result, errorCode);
        }

        private void EnsureHandleIsCreated()
        {
            if (Handle != null)
                return;

            var sessionHandle = MidiDevicesSession.GetSessionHandle();
            var rawHandle = IntPtr.Zero;

            int errorCode;

            switch (_apiType)
            {
                case CommonApi.API_TYPE.API_TYPE_WIN:
                    {
                        _callback = OnMessage;
                        var result = OutputEndpointApi.Api_OpenEndpoint_Win(Info.DangerousGetHandle(), sessionHandle, _callback, out rawHandle, out errorCode);
                        NativeApiUtilities.HandleEndpointNativeApiResult(result, errorCode);
                    }
                    break;
                case CommonApi.API_TYPE.API_TYPE_MAC:
                    {
                        var result = OutputEndpointApi.Api_OpenEndpoint_Mac(Info.DangerousGetHandle(), sessionHandle, out rawHandle, out errorCode);
                        NativeApiUtilities.HandleEndpointNativeApiResult(result, errorCode);
                    }
                    break;
                default:
                    throw new NotSupportedException($"{_apiType} API is not supported.");
            }

            Handle = new OutputEndpointHandle(rawHandle);

#if TEST
            Handle.TestCheckpoints = TestCheckpoints;
#endif
        }

        private void SendSysExEvent(SysExEvent sysExEvent)
        {
            var data = sysExEvent.Data;
            if (data == null || !data.Any())
                return;

            switch (_apiType)
            {
                case CommonApi.API_TYPE.API_TYPE_WIN:
                    SendSysExEventData_Win(data);
                    break;
                case CommonApi.API_TYPE.API_TYPE_MAC:
                    SendSysExEventData_Mac(data);
                    break;
                default:
                    throw new NotSupportedException($"{_apiType} API is not supported.");
            }
        }

        private void SendSysExEventData_Win(byte[] data)
        {
            if (data == null || data.Length == 0)
                return;

            var hasStartByte = data[0] == EventStatusBytes.Global.NormalSysEx;

            var bufferLength = hasStartByte
                ? data.Length
                : data.Length + 1;

            var bufferPointer = Marshal.AllocHGlobal(bufferLength);
            if (!hasStartByte)
                Marshal.WriteByte(bufferPointer, EventStatusBytes.Global.NormalSysEx);
            
            Marshal.Copy(
                data,
                0,
                hasStartByte ? bufferPointer : IntPtr.Add(bufferPointer, 1),
                data.Length);

            var result = OutputEndpointApi.Api_SendSysExEvent_Win(Handle.DangerousGetHandle(), bufferPointer, bufferLength, out var errorCode);
            if (result != OutputEndpointApi.OUT_SENDSYSEXRESULT.OUT_SENDSYSEXRESULT_OK)
                Marshal.FreeHGlobal(bufferPointer);
            
            NativeApiUtilities.HandleEndpointNativeApiResult(result, errorCode);
        }

        private void SendSysExEventData_Mac(byte[] data)
        {
            if (data == null || data.Length == 0)
                return;

            var hasStartByte = data[0] == EventStatusBytes.Global.NormalSysEx;

            var buffer = new byte[hasStartByte ? data.Length : data.Length + 1];
            if (!hasStartByte)
                buffer[0] = EventStatusBytes.Global.NormalSysEx;
            
            Buffer.BlockCopy(
                data,
                0,
                buffer,
                hasStartByte ? 0 : 1,
                data.Length);

            var result = OutputEndpointApi.Api_SendSysExEvent_Mac(Handle.DangerousGetHandle(), buffer, (ushort)buffer.Length, out var errorCode);
            NativeApiUtilities.HandleEndpointNativeApiResult(result, errorCode);
        }

        private int PackShortEvent(MidiEvent midiEvent)
        {
            var channelEvent = midiEvent as ChannelEvent;
            if (channelEvent != null)
                return ChannelEventWriter.GetStatusByte(channelEvent) | (channelEvent._dataByte1 << 8) | (channelEvent._dataByte2 << 16);

            var systemRealTimeEvent = midiEvent as SystemRealTimeEvent;
            if (systemRealTimeEvent != null)
                return SystemRealTimeEventWriter.GetStatusByte(systemRealTimeEvent);

            var bytes = _midiEventToBytesConverter.Convert(midiEvent, ShortEventBufferSize);
            return bytes[0] + (bytes[1] << 8) + (bytes[2] << 16);
        }

        private void OnMessage(IntPtr hMidi, NativeApi.MidiMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2)
        {
            switch (wMsg)
            {
                case NativeApi.MidiMessage.MOM_DONE:
                    OnSysExEventSent(dwParam1);
                    break;
            }
        }

        private void OnSysExEventSent(IntPtr sysExHeaderPointer)
        {
            byte[] data = null;
            IntPtr dataPointer = IntPtr.Zero;

            try
            {
                var result = OutputEndpointApi.Api_GetSysExBufferData(Handle.DangerousGetHandle(), sysExHeaderPointer, out dataPointer, out var size, out var errorCode);
                NativeApiUtilities.HandleEndpointNativeApiResult(result, errorCode);
            }
            catch (Exception ex)
            {
                var exception = new NativeApiException("Failed to parse sent system exclusive event.", ex);
                exception.Data.Add("Data", data);
                OnError(exception);
            }
            finally
            {
                if (dataPointer != IntPtr.Zero)
                    Marshal.FreeHGlobal(dataPointer);
            }
        }

        private void OnEventSent(MidiEvent midiEvent)
        {
            EventSent?.Invoke(this, new MidiEventSentEventArgs(midiEvent));
        }

#endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="OutputEndpoint"/> objects are equal.
        /// </summary>
        /// <remarks>
        /// On Windows the operator will just compare objects references. "True" equality check available
        /// on macOS only.
        /// </remarks>
        /// <param name="outputEndpoint1">The first <see cref="OutputEndpoint"/> to compare.</param>
        /// <param name="outputEndpoint2">The second <see cref="OutputEndpoint"/> to compare.</param>
        /// <returns><c>true</c> if the endpoints are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(OutputEndpoint outputEndpoint1, OutputEndpoint outputEndpoint2)
        {
            if (ReferenceEquals(outputEndpoint1, outputEndpoint2))
                return true;

            if (ReferenceEquals(null, outputEndpoint1) || ReferenceEquals(null, outputEndpoint2))
                return false;

            return outputEndpoint1.Equals(outputEndpoint2);
        }

        /// <summary>
        /// Determines if two <see cref="OutputEndpoint"/> objects are not equal.
        /// </summary>
        /// <remarks>
        /// On Windows the operator will just compare objects references. "True" inequality check available
        /// on macOS only.
        /// </remarks>
        /// <param name="outputEndpoint1">The first <see cref="OutputEndpoint"/> to compare.</param>
        /// <param name="outputEndpoint2">The second <see cref="OutputEndpoint"/> to compare.</param>
        /// <returns><c>false</c> if the endpoints are equal, <c>true</c> otherwise.</returns>
        public static bool operator !=(OutputEndpoint outputEndpoint1, OutputEndpoint outputEndpoint2)
        {
            return !(outputEndpoint1 == outputEndpoint2);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <remarks>
        /// On Windows the method will just compare objects references. "True" equality check available
        /// on macOS only.
        /// </remarks>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            var outputEndpoint = obj as OutputEndpoint;
            if (outputEndpoint == null)
                return false;

            return Id == outputEndpoint.Id;
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var baseDescription = base.ToString();
            return $"Output endpoint{(string.IsNullOrWhiteSpace(baseDescription) ? string.Empty : $" ({baseDescription})")}";
        }

        /// <summary>
        /// Releases the unmanaged resources used by the MIDI endpoint class and optionally releases
        /// the managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to
        /// release only unmanaged resources.</param>
        internal override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _midiEventToBytesConverter.Dispose();
                _bytesToMidiEventConverter.Dispose();

                Handle?.Dispose();
                Handle = null;

                Info?.Dispose();
                Info = null;
            }

            _disposed = true;
        }

        #endregion
    }
}
