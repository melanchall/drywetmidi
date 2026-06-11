using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Represents an input MIDI endpoint. More info in the
    /// <see href="xref:a_dev_overview">Devices</see> and
    /// <see href="xref:a_dev_input">Input endpoint</see> articles.
    /// </summary>
    /// <remarks>
    /// <os-specific-api/>
    /// </remarks>
    public sealed class InputEndpoint : MidiEndpoint, IInputEndpoint
    {
        #region Constants

        private const int DefaultSysExBufferSize = 2048;
        private const int MinSysExBufferSize = 32;

        private const int DefaultSysExBufferCount = 5;
        private const int MinSysExBufferCount = 2;

        private const int ChannelParametersBufferSize = 2;
        private static readonly int MidiTimeCodeComponentsCount = EnumHelper.GetValues<MidiTimeCodeComponent>().Length;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when a MIDI event is received.
        /// </summary>
        public event EventHandler<MidiEventReceivedEventArgs> EventReceived;

        /// <summary>
        /// Occurs when MIDI time code received, i.e. all MIDI events to complete MIDI time code are received.
        /// </summary>
        /// <remarks>
        /// This event will be raised only if <see cref="RaiseMidiTimeCodeReceived"/> is set to <c>true</c>.
        /// </remarks>
        public event EventHandler<MidiTimeCodeReceivedEventArgs> MidiTimeCodeReceived;

        #endregion

        #region Fields

        private static InputEndpointProperty[] _supportedProperties;

        private readonly BytesToMidiEventConverter _bytesToMidiEventConverter = new BytesToMidiEventConverter(ChannelParametersBufferSize) { BytesFormat = BytesFormat.Device };

        private InputEndpointApi.Callback_Win _callbackWin;
        private InputEndpointApi.Callback_Mac _callbackMac;

        private int _sysExBufferSize = DefaultSysExBufferSize;
        private int _sysExBuffersCount = DefaultSysExBufferCount;

        private readonly byte[] _channelParametersBuffer = new byte[ChannelParametersBufferSize];

        private readonly Dictionary<MidiTimeCodeComponent, FourBitNumber> _midiTimeCodeComponents = new Dictionary<MidiTimeCodeComponent, FourBitNumber>();
        private readonly List<byte[]> _sysExParts = new List<byte[]>();

        private readonly CommonApi.API_TYPE _apiType;

        private readonly object _handleLock = new object();
        private readonly object _eventProcessingLock = new object();
        private volatile bool _disposing;

        #endregion

        #region Constructor

        internal InputEndpoint(IntPtr info, CreationContext context)
            : base(context)
        {
            Info = new InputEndpointInfo(info);
            _apiType = CommonApi.Api_GetApiType();
            _bytesToMidiEventConverter.SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn;
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

                // TODO: cache the name and provide method to invalidate cache
                var result = InputEndpointApi.Api_GetEndpointName(Info.DangerousGetHandle(), out var name, out var errorCode);
                NativeApiUtilities.HandleEndpointNativeApiResult(result, errorCode);

                return name;
            }
        }

        public override string Id
        {
            get
            {
                var result = InputEndpointApi.Api_GetEndpointId(Info.DangerousGetHandle(), out var id, out var errorCode);
                NativeApiUtilities.HandleEndpointNativeApiResult(result, errorCode);

                return id;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if <see cref="MidiTimeCodeReceived"/> event should be raised or not.
        /// Default value is <c>true</c>.
        /// </summary>
        public bool RaiseMidiTimeCodeReceived { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether system exclusive event is treated as received only
        /// when it's completed or not. Default value is <c>true</c>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Some MIDI endpoints and MIDI subsystems of operating systems can split a system exclusive
        /// event into several parts on sending the event. If <see cref="WaitForCompleteSysExEvent"/> is
        /// set to <c>true</c> (default value), DryWetMIDI will wait until last part received, then the library
        /// will combine all event's parts into single MIDI event and fire the <see cref="EventReceived"/> event.
        /// </para>
        /// <para>
        /// For example, considering following separate events are received (here bytes in hex format):
        /// </para>
        /// <para>
        /// <c>F0 7F 60</c>
        /// </para>
        /// <para>
        /// <c>40 F7</c>
        /// </para>
        /// <para>
        /// With <see cref="WaitForCompleteSysExEvent"/> set to <c>true</c>, the <see cref="EventReceived"/>
        /// will be fired only once providing the single event:
        /// </para>
        /// <para>
        /// <c>F0 7F 60 40 F7</c>
        /// </para>
        /// <para>
        /// (<c>F7</c> is a marker of a completed system exclusive event).
        /// </para>
        /// <para>
        /// With the property set to <c>false</c> you'll be notified with <see cref="EventReceived"/> event
        /// two times, so every part will be considered as a MIDI event.
        /// </para>
        /// </remarks>
        public bool WaitForCompleteSysExEvent { get; set; } = true;

        /// <summary>
        /// Gets a value that indicates whether <see cref="InputEndpoint"/> is currently listening for
        /// incoming MIDI events.
        /// </summary>
        public bool IsListeningForEvents { get; private set; }

        /// <summary>
        /// Gets or sets reaction of the input endpoint on <c>Note On</c> events with velocity of zero.
        /// The default is <see cref="SilentNoteOnPolicy.NoteOn"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public SilentNoteOnPolicy SilentNoteOnPolicy
        {
            get
            {
                lock (_eventProcessingLock)
                {
                    return _bytesToMidiEventConverter.SilentNoteOnPolicy;
                }
            }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                lock (_eventProcessingLock)
                {
                    _bytesToMidiEventConverter.SilentNoteOnPolicy = value;
                }
            }
        }

        public int SysExBufferSize
        {
            get { return _sysExBufferSize; }
            set
            {
                ThrowIfArgument.IsLessThan(
                    nameof(value),
                    value,
                    MinSysExBufferSize,
                    $"System-exclusive event buffer size is less than {MinSysExBufferSize}.");

                lock (_handleLock)
                {
                    if (Handle != null && !Handle.IsClosed)
                        throw new InvalidOperationException("System-exclusive event buffer size cannot be changed since event listening has started.");

                    _sysExBufferSize = value;
                }
            }
        }

        public int SysExBuffersCount
        {
            get { return _sysExBuffersCount; }
            set
            {
                ThrowIfArgument.IsLessThan(
                    nameof(value),
                    value,
                    MinSysExBufferCount,
                    $"System-exclusive event buffers count is less than {MinSysExBufferCount}.");

                lock (_handleLock)
                {
                    if (Handle != null && !Handle.IsClosed)
                        throw new InvalidOperationException("System-exclusive event buffers count cannot be changed since event listening has started.");

                    _sysExBuffersCount = value;
                }
            }
        }

        internal ICollection<byte[]> SysExParts =>
            _sysExParts;

        #endregion

        #region Methods

        /// <summary>
        /// Starts listening for incoming MIDI events on the current input endpoint.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current <see cref="InputEndpoint"/> is disposed.</exception>
        /// <exception cref="NativeApiException">An error occurred on endpoint.</exception>
        /// <exception cref="InvalidOperationException">The current <see cref="InputEndpoint"/> instance is created by
        /// <see cref="EndpointsWatcher.EndpointRemoved"/> event and thus considered as removed so you cannot interact with it.</exception>
        public void StartEventsListening()
        {
            if (IsListeningForEvents)
                return;

            EnsureEndpointIsNotDisposed();
            EnsureEndpointIsNotRemoved();
            EnsureSessionIsCreated();
            EnsureHandleIsCreated();

            var result = InputEndpointApi.Api_Connect(Handle.DangerousGetHandle(), out var errorCode);
            NativeApiUtilities.HandleEndpointNativeApiResult(result, errorCode);

            IsListeningForEvents = true;
        }

        /// <summary>
        /// Stops listening for incoming MIDI events on the current input endpoint.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current <see cref="InputEndpoint"/> is disposed.</exception>
        /// <exception cref="NativeApiException">An error occurred on endpoint.</exception>
        /// <exception cref="InvalidOperationException">The current <see cref="InputEndpoint"/> instance is created by
        /// <see cref="EndpointsWatcher.EndpointRemoved"/> event and thus considered as removed so you cannot interact with it.</exception>
        public void StopEventsListening()
        {
            if (!IsListeningForEvents || Handle == null || Handle.IsClosed)
                return;

            EnsureEndpointIsNotDisposed();
            EnsureEndpointIsNotRemoved();
            EnsureSessionIsCreated();

            var result = StopEventsListeningSilently(out var errorCode);
            NativeApiUtilities.HandleEndpointNativeApiResult(result, errorCode);
        }

        /// <summary>
        /// Retrieves the number of input MIDI endpoints presented in the system.
        /// </summary>
        /// <returns>Number of input MIDI endpoints presented in the system.</returns>
        /// <exception cref="NativeApiException">An error occurred.</exception>
        /// <exception cref="PlatformNotSupportedException">This operation is not supported on the current operating system.</exception>
        public static int GetEndpointsCount()
        {
            NativeApiUtilities.EnsureOsIsSupported();
            EnsureSessionIsCreated();

            var result = InputEndpointApi.Api_GetEndpointsCount(out var count);
            NativeApiUtilities.HandleEndpointNativeApiResult(result, 0);
            
            return count;
        }

        /// <summary>
        /// Retrieves all input MIDI endpoints presented in the system.
        /// </summary>
        /// <returns>All input MIDI endpoints presented in the system.</returns>
        /// <exception cref="NativeApiException">An error occurred.</exception>
        /// <exception cref="PlatformNotSupportedException">This operation is not supported on the current operating system.</exception>
        public static ICollection<InputEndpoint> GetAll()
        {
            NativeApiUtilities.EnsureOsIsSupported();
            EnsureSessionIsCreated();

            return MidiEndpointsManager.Instance.GetAllInputEndpoints();
        }

        /// <summary>
        /// Retrieves a first input MIDI endpoint with the specified name.
        /// </summary>
        /// <param name="name">The name of an input MIDI endpoint to retrieve.</param>
        /// <returns>Input MIDI endpoint with the specified name.</returns>
        /// <exception cref="ArgumentException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="name"/> is <c>null</c> or contains white-spaces only.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="name"/> specifies an input MIDI endpoint which is not presented in the system.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="NativeApiException">An error occurred.</exception>
        /// <exception cref="PlatformNotSupportedException">This operation is not supported on the current operating system.</exception>
        public static InputEndpoint GetByName(string name)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(name), name, "Endpoint name");

            NativeApiUtilities.EnsureOsIsSupported();
            EnsureSessionIsCreated();

            var endpoint = MidiEndpointsManager.Instance.GetInputEndpointByName(name);
            if (endpoint == null)
                throw new ArgumentException($"There is no MIDI input endpoint '{name}'.", nameof(name));

            return endpoint;
        }

        private void OnEventReceived(MidiEvent midiEvent)
        {
            EventReceived?.Invoke(this, new MidiEventReceivedEventArgs(midiEvent));

            if (RaiseMidiTimeCodeReceived)
            {
                if (midiEvent is MidiTimeCodeEvent midiTimeCodeEvent)
                    TryRaiseMidiTimeCodeReceived(midiTimeCodeEvent);
            }
        }

        private void OnMidiTimeCodeReceived(MidiTimeCodeType timeCodeType, int hours, int minutes, int seconds, int frames)
        {
            MidiTimeCodeReceived?.Invoke(this, new MidiTimeCodeReceivedEventArgs(timeCodeType, hours, minutes, seconds, frames));
        }

        private void EnsureHandleIsCreated()
        {
            lock (_handleLock)
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
                            _callbackWin = OnMessage_Win;
                            var result = InputEndpointApi.Api_OpenEndpoint_Win(Info.DangerousGetHandle(), sessionHandle, _callbackWin, SysExBufferSize, SysExBuffersCount, out rawHandle, out errorCode);
                            NativeApiUtilities.HandleEndpointNativeApiResult(result, errorCode);
                        }
                        break;
                    case CommonApi.API_TYPE.API_TYPE_MAC:
                        {
                            _callbackMac = OnMessage_Mac;
                            var result = InputEndpointApi.Api_OpenEndpoint_Mac(Info.DangerousGetHandle(), sessionHandle, _callbackMac, out rawHandle, out errorCode);
                            NativeApiUtilities.HandleEndpointNativeApiResult(result, errorCode);
                        }
                        break;
                    default:
                        throw new NotSupportedException($"{_apiType} API is not supported.");
                }

                Handle = new InputEndpointHandle(rawHandle);

#if TEST
                Handle.TestCheckpoints = TestCheckpoints;
#endif
            }
        }

        private void OnMessage_Win(IntPtr hMidi, NativeApi.MidiMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2)
        {
            if (_disposing || !IsListeningForEvents || !IsEnabled)
                return;

            lock (_eventProcessingLock)
            {
                switch (wMsg)
                {
                    case NativeApi.MidiMessage.MIM_DATA:
                    case NativeApi.MidiMessage.MIM_MOREDATA:
                        OnShortMessage(dwParam1.ToInt32());
                        break;

                    case NativeApi.MidiMessage.MIM_LONGDATA:
                        OnSysExMessage(dwParam1);
                        break;

                    case NativeApi.MidiMessage.MIM_ERROR:
                        OnInvalidShortEvent(dwParam1.ToInt32());
                        break;

                    case NativeApi.MidiMessage.MIM_LONGERROR:
                        OnInvalidSysExEvent(dwParam1);
                        break;
                }
            }
        }

        private void OnMessage_Mac(IntPtr pktlist, IntPtr readProcRefCon, IntPtr srcConnRefCon)
        {
            if (_disposing || !IsListeningForEvents || !IsEnabled)
                return;

            lock (_eventProcessingLock)
            {
#if TEST
                TestCheckpoints?.SetCheckpointReached(InputEndpointCheckpointsNames.MessageDataReceived, null);
#endif

                int packetsCount = 1;

                for (var i = 0; i < packetsCount; i++)
                {
                    OnPacket_Mac(pktlist, i, out packetsCount);
                }
            }
        }

        private void OnPacket_Mac(IntPtr pktlist, int packetIndex, out int packetsCount)
        {
            packetsCount = 0;
            byte[] data = null;

            try
            {
                NativeApiUtilities.HandleEndpointNativeApiResult(
                    InputEndpointApi.Api_GetEventData(pktlist, packetIndex, out var dataPtr, out var length, out packetsCount), 0);

                data = new byte[length];
                Marshal.Copy(dataPtr, data, 0, length);

#if TEST
                TestCheckpoints?.SetCheckpointReached(InputEndpointCheckpointsNames.MessageDataReceived, data);
#endif

                if (data[0] == EventStatusBytes.Global.NormalSysEx)
                {
                    HandleSysExStartPart(data);
                    return;
                }
                else if (_sysExParts.Any())
                {
                    HandleSysExSubsequentPart(data);
                    return;
                }

                HandleEvents(data);
            }
            catch (Exception ex)
            {
                var exception = new NativeApiException("Failed to parse message.", ex);
                exception.Data.Add("Data", data);
                OnError(exception);
            }
        }

        private void HandleSysExStartPart(byte[] data)
        {
            var sysExData = new byte[data.Length - 1];
            Buffer.BlockCopy(data, 1, sysExData, 0, sysExData.Length);

            if (data[data.Length - 1] == SysExEvent.EndOfEventByte || !WaitForCompleteSysExEvent)
            {
                var midiEvent = new NormalSysExEvent(sysExData);
                OnEventReceived(midiEvent);
            }
            else
                _sysExParts.Add(sysExData);
        }

        private void HandleSysExSubsequentPart(byte[] data)
        {
            _sysExParts.Add(data);

            if (data[data.Length - 1] == SysExEvent.EndOfEventByte)
            {
                var sysExData = new byte[_sysExParts.Sum(p => p.Length)];
                var i = 0;

                foreach (var p in _sysExParts)
                {
                    Buffer.BlockCopy(p, 0, sysExData, i, p.Length);
                    i += p.Length;
                }

                _sysExParts.Clear();

                var midiEvent = new NormalSysExEvent(sysExData);
                OnEventReceived(midiEvent);
            }
        }

        private void HandleEvents(byte[] data)
        {
            byte? runningStatusByte = null;
            var length = data.Length;

            using (var stream = new MemoryStream(data))
            using (var midiReader = new MidiReader(stream, new ReaderSettings()))
            {
                midiReader.Position = 0;

                while (midiReader.Position < length)
                {
                    var statusByte = midiReader.ReadByte();
                    if (statusByte <= SevenBitNumber.MaxValue)
                    {
                        if (runningStatusByte == null)
                            throw new UnexpectedRunningStatusException();

                        statusByte = runningStatusByte.Value;
                        midiReader.Position--;
                    }

                    runningStatusByte = statusByte;

                    var eventReader = EventReaderFactory.GetReader(statusByte, smfOnly: false);
                    var midiEvent = eventReader.Read(midiReader, _bytesToMidiEventConverter.ReadingSettings, statusByte);
                    
                    if (statusByte == EventStatusBytes.Global.NormalSysEx)
                    {
                        var sysExEvent = (SysExEvent)midiEvent;
                        if (sysExEvent.Completed || !WaitForCompleteSysExEvent)
                            OnEventReceived(midiEvent);
                        else
                        {
                            var buffer = new byte[sysExEvent.Data.Length + 1];
                            buffer[0] = statusByte;
                            Buffer.BlockCopy(sysExEvent.Data, 0, buffer, 1, sysExEvent.Data.Length);
                            _sysExParts.Add(buffer);
                        }
                    }
                    else
                        OnEventReceived(midiEvent);
                }
            }
        }

        private void OnInvalidShortEvent(int message)
        {
            var exception = new NativeApiException("Invalid short event received.");
            exception.Data["StatusByte"] = message.GetFourthByte();
            exception.Data["FirstDataByte"] = message.GetThirdByte();
            exception.Data["SecondDataByte"] = message.GetSecondByte();

            OnError(exception);
        }

        private void OnInvalidSysExEvent(IntPtr headerPointer)
        {
            NativeApiUtilities.HandleEndpointNativeApiResult(
                InputEndpointApi.Api_GetSysExBufferData(headerPointer, out var dataPointer, out var size), 0);

            var data = new byte[size];
            Marshal.Copy(dataPointer, data, 0, size);

            var exception = new NativeApiException("Invalid system exclusive event received.");
            exception.Data.Add("Data", data);
            OnError(exception);
        }

        private void OnShortMessage(int message)
        {
            try
            {
                var statusByte = (byte)(message & 0xFF);

                _channelParametersBuffer[0] = (byte)((message >> 8) & 0xFF);
                _channelParametersBuffer[1] = (byte)((message >> 16) & 0xFF);

                var midiEvent = _bytesToMidiEventConverter.Convert(statusByte, _channelParametersBuffer);
                OnEventReceived(midiEvent);
            }
            catch (Exception ex)
            {
                var exception = new NativeApiException("Failed to parse short message.", ex);
                exception.Data.Add("Message", message);
                OnError(exception);
            }
        }

        private void OnSysExMessage(IntPtr sysExHeaderPointer)
        {
            byte[] data = null;

            try
            {
                NativeApiUtilities.HandleEndpointNativeApiResult(
                    InputEndpointApi.Api_GetSysExBufferData(sysExHeaderPointer, out var dataPointer, out var size), 0);

                if (size <= 0)
                    return;

                data = new byte[size];
                Marshal.Copy(dataPointer, data, 0, data.Length);

#if TEST
                TestCheckpoints?.SetCheckpointReached(InputEndpointCheckpointsNames.MessageDataReceived, data);
#endif

                if (data[0] == EventStatusBytes.Global.NormalSysEx)
                    HandleSysExStartPart(data);
                else if (_sysExParts.Any())
                    HandleSysExSubsequentPart(data);

                if (_disposing || Handle == null || Handle.IsClosed)
                    return;

                lock (Handle.Lock)
                {
                    if (_disposing || Handle == null || Handle.IsClosed)
                        return;

                    var result = InputEndpointApi.Api_RenewInputEndpointSysExBuffer(Handle.DangerousGetHandle(), sysExHeaderPointer, out var errorCode);
                    if (result == InputEndpointApi.IN_RENEWSYSEXBUFFERRESULT.IN_RENEWSYSEXBUFFERRESULT_CLOSING)
                        return;

                    NativeApiUtilities.HandleEndpointNativeApiResult(result, errorCode);
                }
            }
            catch (Exception ex)
            {
                var exception = new NativeApiException("Failed to parse system exclusive message.", ex);
                exception.Data.Add("Data", data);
                OnError(exception);
            }
        }

        private void TryRaiseMidiTimeCodeReceived(MidiTimeCodeEvent midiTimeCodeEvent)
        {
            var component = midiTimeCodeEvent.Component;
            var componentValue = midiTimeCodeEvent.ComponentValue;

            _midiTimeCodeComponents[component] = componentValue;
            if (_midiTimeCodeComponents.Count != MidiTimeCodeComponentsCount)
                return;

            var frames = DataTypesUtilities.Combine(_midiTimeCodeComponents[MidiTimeCodeComponent.FramesMsb],
                                                    _midiTimeCodeComponents[MidiTimeCodeComponent.FramesLsb]);

            var minutes = DataTypesUtilities.Combine(_midiTimeCodeComponents[MidiTimeCodeComponent.MinutesMsb],
                                                     _midiTimeCodeComponents[MidiTimeCodeComponent.MinutesLsb]);

            var seconds = DataTypesUtilities.Combine(_midiTimeCodeComponents[MidiTimeCodeComponent.SecondsMsb],
                                                     _midiTimeCodeComponents[MidiTimeCodeComponent.SecondsLsb]);

            var hoursAndTimeCodeType = DataTypesUtilities.Combine(_midiTimeCodeComponents[MidiTimeCodeComponent.HoursMsbAndTimeCodeType],
                                                                  _midiTimeCodeComponents[MidiTimeCodeComponent.HoursLsb]);
            var hours = hoursAndTimeCodeType & 0x1F;
            var timeCodeType = (MidiTimeCodeType)((hoursAndTimeCodeType >> 5) & 0x3);

            OnMidiTimeCodeReceived(timeCodeType, hours, minutes, seconds, frames);
            _midiTimeCodeComponents.Clear();
        }

        private InputEndpointApi.IN_DISCONNECTRESULT StopEventsListeningSilently(out int errorCode)
        {
            errorCode = 0;

            IsListeningForEvents = false;

            if (Handle == null || Handle.IsClosed)
                return InputEndpointApi.IN_DISCONNECTRESULT.IN_DISCONNECTRESULT_OK;

            return InputEndpointApi.Api_Disconnect(Handle.DangerousGetHandle(), out errorCode);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="InputEndpoint"/> objects are equal.
        /// </summary>
        /// <remarks>
        /// On Windows the operator will just compare objects references. "True" equality check available
        /// on macOS only.
        /// </remarks>
        /// <param name="inputEndpoint1">The first <see cref="InputEndpoint"/> to compare.</param>
        /// <param name="inputEndpoint2">The second <see cref="InputEndpoint"/> to compare.</param>
        /// <returns><c>true</c> if the endpoints are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(InputEndpoint inputEndpoint1, InputEndpoint inputEndpoint2)
        {
            if (ReferenceEquals(inputEndpoint1, inputEndpoint2))
                return true;

            if (ReferenceEquals(null, inputEndpoint1) || ReferenceEquals(null, inputEndpoint2))
                return false;

            return inputEndpoint1.Equals(inputEndpoint2);
        }

        /// <summary>
        /// Determines if two <see cref="InputEndpoint"/> objects are not equal.
        /// </summary>
        /// <remarks>
        /// On Windows the operator will just compare objects references. "True" inequality check available
        /// on macOS only.
        /// </remarks>
        /// <param name="inputEndpoint1">The first <see cref="InputEndpoint"/> to compare.</param>
        /// <param name="inputEndpoint2">The second <see cref="InputEndpoint"/> to compare.</param>
        /// <returns><c>false</c> if the endpoints are equal, <c>true</c> otherwise.</returns>
        public static bool operator !=(InputEndpoint inputEndpoint1, InputEndpoint inputEndpoint2)
        {
            return !(inputEndpoint1 == inputEndpoint2);
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
            var inputEndpoint = obj as InputEndpoint;
            if (inputEndpoint == null)
                return false;

            return Id == inputEndpoint.Id;
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
            return $"Input endpoint{(string.IsNullOrWhiteSpace(baseDescription) ? string.Empty : $" ({baseDescription})")}";
        }

        internal override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            _disposing = true;

            if (disposing)
            {
                if (Handle != null)
                {
                    lock (Handle.Lock)
                    {
                        _bytesToMidiEventConverter.Dispose();
                        Handle?.Dispose();
                        Handle = null;
                    }
                }

                Info?.Dispose();
                Info = null;
            }
            else
            {
                Handle?.Dispose();
                Handle = null;
            }

            _disposed = true;
        }

        #endregion
    }
}
