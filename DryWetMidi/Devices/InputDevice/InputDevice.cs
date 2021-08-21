using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Represents an input MIDI device.
    /// </summary>
    public sealed class InputDevice : MidiDevice, IInputDevice
    {
        #region Constants

        private const int SysExBufferSize = 2048;
        private const int ChannelParametersBufferSize = 2;
        private static readonly int MidiTimeCodeComponentsCount = Enum.GetValues(typeof(MidiTimeCodeComponent)).Length;

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

        private static InputDeviceProperty[] _supportedProperties;

        private readonly BytesToMidiEventConverter _bytesToMidiEventConverter = new BytesToMidiEventConverter(ChannelParametersBufferSize);

        private InputDeviceApi.Callback_Win _callback_Win;
        private InputDeviceApi.Callback_Mac _callback_Mac;

        private readonly byte[] _channelParametersBuffer = new byte[ChannelParametersBufferSize];

        private readonly Dictionary<MidiTimeCodeComponent, FourBitNumber> _midiTimeCodeComponents = new Dictionary<MidiTimeCodeComponent, FourBitNumber>();

        private readonly CommonApi.API_TYPE _apiType;

        #endregion

        #region Constructor

        internal InputDevice(IntPtr info, CreationContext context)
            : base(info, context)
        {
            _apiType = CommonApiProvider.Api.Api_GetApiType();
            _bytesToMidiEventConverter.ReadingSettings.SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn;
        }

        #endregion

        #region Finalizer

        /// <summary>
        /// Finalizes the current instance of the <see cref="InputDevice"/>.
        /// </summary>
        ~InputDevice()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the current MIDI device.
        /// </summary>
        public override string Name
        {
            get
            {
                EnsureSessionIsCreated();
                EnsureDeviceIsNotRemoved();

                string name;
                NativeApi.HandleResult(InputDeviceApiProvider.Api.Api_GetDeviceName(_info, out name));

                return name;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if <see cref="MidiTimeCodeReceived"/> event should be raised or not.
        /// </summary>
        public bool RaiseMidiTimeCodeReceived { get; set; } = true;

        /// <summary>
        /// Gets a value that indicates whether <see cref="InputDevice"/> is currently listening for
        /// incoming MIDI events.
        /// </summary>
        public bool IsListeningForEvents { get; private set; }

        /// <summary>
        /// Gets or sets reaction of the input device on <c>Note On</c> events with velocity of zero.
        /// The default is <see cref="SilentNoteOnPolicy.NoteOn"/>.
        /// </summary>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="value"/> specified an invalid value.</exception>
        public SilentNoteOnPolicy SilentNoteOnPolicy
        {
            get { return _bytesToMidiEventConverter.ReadingSettings.SilentNoteOnPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _bytesToMidiEventConverter.ReadingSettings.SilentNoteOnPolicy = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Starts listening for incoming MIDI events on the current input device.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current <see cref="InputDevice"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        /// <exception cref="InvalidOperationException">The current <see cref="InputDevice"/> instance is created by
        /// <see cref="DevicesWatcher.DeviceRemoved"/> event and thus considered as removed so you cannot interact with it.</exception>
        public void StartEventsListening()
        {
            if (IsListeningForEvents)
                return;

            EnsureDeviceIsNotDisposed();
            EnsureDeviceIsNotRemoved();
            EnsureSessionIsCreated();
            EnsureHandleIsCreated();

            NativeApi.HandleResult(
                InputDeviceApiProvider.Api.Api_Connect(_handle));
            IsListeningForEvents = true;
        }

        /// <summary>
        /// Stops listening for incoming MIDI events on the current input device.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current <see cref="InputDevice"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        /// <exception cref="InvalidOperationException">The current <see cref="InputDevice"/> instance is created by
        /// <see cref="DevicesWatcher.DeviceRemoved"/> event and thus considered as removed so you cannot interact with it.</exception>
        public void StopEventsListening()
        {
            if (!IsListeningForEvents || _handle == IntPtr.Zero)
                return;

            EnsureDeviceIsNotDisposed();
            EnsureDeviceIsNotRemoved();
            EnsureSessionIsCreated();

            NativeApi.HandleResult(
                StopEventsListeningSilently());
        }

        /// <summary>
        /// Returns current value of the specified property attached to the current input device.
        /// </summary>
        /// <remarks>
        /// <para>
        /// To get the list of properties applicable to input devices on the current operating system use
        /// <see cref="GetSupportedProperties"/> method.
        /// </para>
        /// <para>
        /// Following table shows the type of value returned by the method for each property:
        /// </para>
        /// <para>
        /// <list type="table">
        /// <listheader>
        /// <term>Property</term>
        /// <term>Type</term>
        /// </listheader>
        /// <item>
        /// <term><see cref="InputDeviceProperty.Product"/></term>
        /// <term><see cref="string"/></term>
        /// </item>
        /// <item>
        /// <term><see cref="InputDeviceProperty.Manufacturer"/></term>
        /// <term><see cref="string"/></term>
        /// </item>
        /// <item>
        /// <term><see cref="InputDeviceProperty.DriverVersion"/></term>
        /// <term><see cref="int"/></term>
        /// </item>
        /// <item>
        /// <term><see cref="InputDeviceProperty.UniqueId"/></term>
        /// <term><see cref="int"/></term>
        /// </item>
        /// <item>
        /// <term><see cref="InputDeviceProperty.DriverOwner"/></term>
        /// <term><see cref="string"/></term>
        /// </item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <param name="property">The property to get value of.</param>
        /// <returns>The current value of the <paramref name="property"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="property"/> specified an invalid value.</exception>
        /// <exception cref="ArgumentException"><paramref name="property"/> is not in the list of the properties
        /// supported for the current operating system.</exception>
        /// <exception cref="ObjectDisposedException">The current <see cref="InputDevice"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on the device. One of the cases when this exception can be thrown
        /// is device is not in the system anymore (for example, unplugged).</exception>
        /// <exception cref="InvalidOperationException">The current <see cref="InputDevice"/> instance is created by
        /// <see cref="DevicesWatcher.DeviceRemoved"/> event and thus considered as removed so you cannot interact with it.</exception>
        public object GetProperty(InputDeviceProperty property)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(property), property);

            EnsureDeviceIsNotDisposed();
            EnsureDeviceIsNotRemoved();
            EnsureSessionIsCreated();

            if (!GetSupportedProperties().Contains(property))
                throw new ArgumentException("Property is not supported.", nameof(property));

            var api = InputDeviceApiProvider.Api;

            switch (property)
            {
                case InputDeviceProperty.Product:
                    {
                        string product;
                        NativeApi.HandleResult(api.Api_GetDeviceProduct(_info, out product));
                        return product;
                    }
                case InputDeviceProperty.Manufacturer:
                    {
                        string manufacturer;
                        NativeApi.HandleResult(api.Api_GetDeviceManufacturer(_info, out manufacturer));
                        return manufacturer;
                    }
                case InputDeviceProperty.DriverVersion:
                    {
                        int driverVersion;
                        NativeApi.HandleResult(api.Api_GetDeviceDriverVersion(_info, out driverVersion));
                        return driverVersion;
                    }
                case InputDeviceProperty.UniqueId:
                    {
                        int uniqueId;
                        NativeApi.HandleResult(api.Api_GetDeviceUniqueId(_info, out uniqueId));
                        return uniqueId;
                    }
                case InputDeviceProperty.DriverOwner:
                    {
                        string driverOwner;
                        NativeApi.HandleResult(api.Api_GetDeviceDriverOwner(_info, out driverOwner));
                        return driverOwner;
                    }
                default:
                    throw new NotSupportedException("Property is not supported.");
            }
        }

        /// <summary>
        /// Returns the list of the properties supported by input devices on the current
        /// operating system.
        /// </summary>
        /// <returns>The list of the properties supported by input devices on the current
        /// operating system.</returns>
        public static InputDeviceProperty[] GetSupportedProperties()
        {
            if (_supportedProperties != null)
                return _supportedProperties;

            return _supportedProperties = Enum.GetValues(typeof(InputDeviceProperty))
                .OfType<InputDeviceProperty>()
                .Where(p => InputDeviceApiProvider.Api.Api_IsPropertySupported(p))
                .ToArray();
        }

        /// <summary>
        /// Retrieves the number of input MIDI devices presented in the system.
        /// </summary>
        /// <returns>Number of input MIDI devices presented in the system.</returns>
        public static int GetDevicesCount()
        {
            EnsureSessionIsCreated();

            return InputDeviceApiProvider.Api.Api_GetDevicesCount();
        }

        /// <summary>
        /// Retrieves all input MIDI devices presented in the system.
        /// </summary>
        /// <returns>All input MIDI devices presented in the system.</returns>
        public static IEnumerable<InputDevice> GetAll()
        {
            EnsureSessionIsCreated();

            var devicesCount = GetDevicesCount();

            for (var i = 0; i < devicesCount; i++)
            {
                yield return GetByIndex(i);
            }
        }

        /// <summary>
        /// Retrieves an input MIDI device by the specified index.
        /// </summary>
        /// <param name="index">Index of an input device to retrieve.</param>
        /// <returns>Input MIDI device at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Index is less than zero or greater than devices count minus 1.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on the device.</exception>
        public static InputDevice GetByIndex(int index)
        {
            var devicesCount = GetDevicesCount();
            ThrowIfArgument.IsOutOfRange(nameof(index), index, 0, devicesCount - 1, "Index is less than zero or greater than devices count minus 1.");

            EnsureSessionIsCreated();

            IntPtr info;
            NativeApi.HandleResult(InputDeviceApiProvider.Api.Api_GetDeviceInfo(index, out info));

            return new InputDevice(info, CreationContext.User);
        }

        /// <summary>
        /// Retrieves a first input MIDI device with the specified name.
        /// </summary>
        /// <param name="name">The name of an input MIDI device to retrieve.</param>
        /// <returns>Input MIDI device with the specified name.</returns>
        /// <exception cref="ArgumentException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="name"/> is <c>null</c> or contains white-spaces only.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="name"/> specifies an input MIDI device which is not presented in the system.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="MidiDeviceException">An error occurred on the device.</exception>
        public static InputDevice GetByName(string name)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(name), name, "Device name");

            EnsureSessionIsCreated();

            var device = GetAll().FirstOrDefault(d => d.Name == name);
            if (device == null)
                throw new ArgumentException($"There is no MIDI input device '{name}'.", nameof(name));

            return device;
        }

        private void OnEventReceived(MidiEvent midiEvent)
        {
            EventReceived?.Invoke(this, new MidiEventReceivedEventArgs(midiEvent));

            if (RaiseMidiTimeCodeReceived)
            {
                var midiTimeCodeEvent = midiEvent as MidiTimeCodeEvent;
                if (midiTimeCodeEvent != null)
                    TryRaiseMidiTimeCodeReceived(midiTimeCodeEvent);
            }
        }

        private void OnMidiTimeCodeReceived(MidiTimeCodeType timeCodeType, int hours, int minutes, int seconds, int frames)
        {
            MidiTimeCodeReceived?.Invoke(this, new MidiTimeCodeReceivedEventArgs(timeCodeType, hours, minutes, seconds, frames));
        }

        private void EnsureHandleIsCreated()
        {
            if (_handle != IntPtr.Zero)
                return;

            var sessionHandle = MidiDevicesSession.GetSessionHandle();

            switch (_apiType)
            {
                case CommonApi.API_TYPE.API_TYPE_WIN:
                    {
                        _callback_Win = OnMessage_Win;
                        NativeApi.HandleResult(
                            InputDeviceApiProvider.Api.Api_OpenDevice_Win(_info, sessionHandle, _callback_Win, SysExBufferSize, out _handle));
                    }
                    break;
                case CommonApi.API_TYPE.API_TYPE_MAC:
                    {
                        _callback_Mac = OnMessage_Mac;
                        NativeApi.HandleResult(
                            InputDeviceApiProvider.Api.Api_OpenDevice_Mac(_info, sessionHandle, _callback_Mac, out _handle));
                    }
                    break;
                default:
                    throw new NotSupportedException($"{_apiType} API is not supported.");
            }
        }

        private void DestroyHandle()
        {
            if (_handle == IntPtr.Zero)
                return;

            InputDeviceApiProvider.Api.Api_CloseDevice(_handle);
            _handle = IntPtr.Zero;
        }

        private void OnMessage_Win(IntPtr hMidi, NativeApi.MidiMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2)
        {
            if (!IsListeningForEvents || !IsEnabled)
                return;

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

        private void OnMessage_Mac(IntPtr pktlist, IntPtr readProcRefCon, IntPtr srcConnRefCon)
        {
            if (!IsListeningForEvents || !IsEnabled)
                return;

            byte[] data = null;

            try
            {
                IntPtr dataPtr;
                int length;

                NativeApi.HandleResult(
                    InputDeviceApiProvider.Api.Api_GetEventData(pktlist, 0, out dataPtr, out length));

                data = new byte[length];
                Marshal.Copy(dataPtr, data, 0, length);

                // TODO: handle escape sysex
                if (data[0] == EventStatusBytes.Global.NormalSysEx)
                {
                    var sysExData = new byte[length - 1];
                    Buffer.BlockCopy(data, 1, sysExData, 0, sysExData.Length);

                    var midiEvent = new NormalSysExEvent(sysExData);
                    OnEventReceived(midiEvent);
                    return;
                }

                byte? runningStatusByte = null;

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

                        OnEventReceived(midiEvent);
                    }
                }
            }
            catch (Exception ex)
            {
                var exception = new MidiDeviceException($"Failed to parse message.", ex);
                exception.Data.Add("Data", data);
                OnError(exception);
            }
        }

        private void OnInvalidShortEvent(int message)
        {
            var exception = new MidiDeviceException("Invalid short event received.");
            exception.Data["StatusByte"] = message.GetFourthByte();
            exception.Data["FirstDataByte"] = message.GetThirdByte();
            exception.Data["SecondDataByte"] = message.GetSecondByte();

            OnError(exception);
        }

        private void OnInvalidSysExEvent(IntPtr headerPointer)
        {
            IntPtr dataPointer;
            int size;

            NativeApi.HandleResult(
                InputDeviceApiProvider.Api.Api_GetSysExBufferData(headerPointer, out dataPointer, out size));

            var data = new byte[size];
            Marshal.Copy(dataPointer, data, 0, size);

            var exception = new MidiDeviceException("Invalid system exclusive event received.");
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
                var exception = new MidiDeviceException($"Failed to parse short message.", ex);
                exception.Data.Add("Message", message);
                OnError(exception);
            }
        }

        private void OnSysExMessage(IntPtr sysExHeaderPointer)
        {
            byte[] data = null;

            try
            {
                IntPtr dataPointer;
                int size;

                NativeApi.HandleResult(
                    InputDeviceApiProvider.Api.Api_GetSysExBufferData(sysExHeaderPointer, out dataPointer, out size));

                data = new byte[size - 1];
                Marshal.Copy(IntPtr.Add(dataPointer, 1), data, 0, data.Length);

                var midiEvent = new NormalSysExEvent(data);
                OnEventReceived(midiEvent);

                NativeApi.HandleResult(
                    InputDeviceApiProvider.Api.Api_RenewSysExBuffer(_handle, SysExBufferSize));
            }
            catch (Exception ex)
            {
                var exception = new MidiDeviceException($"Failed to parse system exclusive message.", ex);
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

        private InputDeviceApi.IN_DISCONNECTRESULT StopEventsListeningSilently()
        {
            IsListeningForEvents = false;
            return InputDeviceApiProvider.Api.Api_Disconnect(_handle);
        }

        #endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="InputDevice"/> objects are equal.
        /// </summary>
        /// <param name="inputDevice1">The first <see cref="InputDevice"/> to compare.</param>
        /// <param name="inputDevice2">The second <see cref="InputDevice"/> to compare.</param>
        /// <returns><c>true</c> if the devices are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(InputDevice inputDevice1, InputDevice inputDevice2)
        {
            if (ReferenceEquals(inputDevice1, inputDevice2))
                return true;

            if (ReferenceEquals(null, inputDevice1) || ReferenceEquals(null, inputDevice2))
                return false;

            return inputDevice1.Equals(inputDevice2);
        }

        /// <summary>
        /// Determines if two <see cref="InputDevice"/> objects are not equal.
        /// </summary>
        /// <param name="inputDevice1">The first <see cref="InputDevice"/> to compare.</param>
        /// <param name="inputDevice2">The second <see cref="InputDevice"/> to compare.</param>
        /// <returns><c>false</c> if the devices are equal, <c>true</c> otherwise.</returns>
        public static bool operator !=(InputDevice inputDevice1, InputDevice inputDevice2)
        {
            return !(inputDevice1 == inputDevice2);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            var inputDevice = obj as InputDevice;
            if (inputDevice == null)
                return false;

            var canCompare = CommonApiProvider.Api.Api_CanCompareDevices();
            return canCompare
                ? InputDeviceApiProvider.Api.Api_AreDevicesEqual(_info, inputDevice._info)
                : _info.Equals(inputDevice._info);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return _info.ToInt32();
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var baseDescription = base.ToString();
            return $"Input device{(string.IsNullOrWhiteSpace(baseDescription) ? string.Empty : $" ({baseDescription})")}";
        }

        /// <summary>
        /// Releases the unmanaged resources used by the MIDI device class and optionally releases
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
                _bytesToMidiEventConverter.Dispose();
            }

            if (_handle != IntPtr.Zero)
            {
                StopEventsListeningSilently();
                DestroyHandle();
            }

            _disposed = true;
        }

        #endregion
    }
}
