using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Represents an output MIDI device. More info in the
    /// <see href="xref:a_dev_overview">Devices</see> and
    /// <see href="xref:a_dev_output">Output device</see> articles.
    /// </summary>
    public sealed class OutputDevice : MidiDevice, IOutputDevice
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

        private static OutputDeviceProperty[] _supportedProperties;

        private readonly MidiEventToBytesConverter _midiEventToBytesConverter = new MidiEventToBytesConverter(ShortEventBufferSize) { BytesFormat = BytesFormat.Device };
        private readonly BytesToMidiEventConverter _bytesToMidiEventConverter = new BytesToMidiEventConverter { BytesFormat = BytesFormat.Device };

        private OutputDeviceApi.Callback_Win _callback;

        private readonly CommonApi.API_TYPE _apiType;
        private readonly int _hashCode;

        private readonly IntPtr _info = IntPtr.Zero;

        #endregion

        #region Constructor

        internal OutputDevice(IntPtr info, CreationContext context)
            : base(context)
        {
            _info = info;
            _apiType = CommonApi.Api_GetApiType();
            _hashCode = OutputDeviceApi.Api_GetDeviceHashCode(info);
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

                var result = OutputDeviceApi.Api_GetDeviceName(_info, out var name, out var errorCode);
                NativeApiUtilities.HandleDevicesNativeApiResult(result, errorCode);

                return name;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sends a MIDI event to the current output device.
        /// </summary>
        /// <param name="midiEvent">MIDI event to send.</param>
        /// <exception cref="ObjectDisposedException">The current <see cref="OutputDevice"/> is disposed.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="midiEvent"/> is <c>null</c>.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        /// <exception cref="ArgumentException">EscapeSysExEvent is prohibited. Use NormalSysExEvent instead.</exception>
        public void SendEvent(MidiEvent midiEvent)
        {
            ThrowIfArgument.IsNull(nameof(midiEvent), midiEvent);
            ThrowIfArgument.IsOfType<EscapeSysExEvent>(
                nameof(midiEvent),
                midiEvent,
                "EscapeSysExEvent is prohibited. Use NormalSysExEvent instead.");

            if (!IsEnabled)
                return;

            EnsureDeviceIsNotDisposed();
            EnsureDeviceIsNotRemoved();
            EnsureSessionIsCreated();
            EnsureHandleIsCreated();

            if (midiEvent is ChannelEvent || midiEvent is SystemCommonEvent || midiEvent is SystemRealTimeEvent)
            {
                var message = PackShortEvent(midiEvent);
                var result = OutputDeviceApi.Api_SendShortEvent(Handle.DangerousGetHandle(), message, out var errorCode);
                NativeApiUtilities.HandleDevicesNativeApiResult(result, errorCode);
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
        /// <exception cref="ObjectDisposedException">The current <see cref="OutputDevice"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public void TurnAllNotesOff()
        {
            EnsureDeviceIsNotDisposed();
            EnsureDeviceIsNotRemoved();
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
        /// Prepares output MIDI device for sending events to it allocating necessary
        /// resources.
        /// </summary>
        /// <remarks>It is not needed to call this method before actual MIDI data
        /// sending since first call of <see cref="SendEvent(MidiEvent)"/> will prepare
        /// the device automatically. But it can take some time so you may decide
        /// to call <see cref="PrepareForEventsSending"/> before working with device.</remarks>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public void PrepareForEventsSending()
        {
            EnsureSessionIsCreated();
            EnsureHandleIsCreated();
        }

        /// <summary>
        /// Returns current value of the specified property attached to the current output device.
        /// </summary>
        /// <remarks>
        /// <para>
        /// To get the list of properties applicable to output devices on the current operating system use
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
        /// <term><see cref="OutputDeviceProperty.Product"/></term>
        /// <term><see cref="string"/></term>
        /// </item>
        /// <item>
        /// <term><see cref="OutputDeviceProperty.Manufacturer"/></term>
        /// <term><see cref="string"/></term>
        /// </item>
        /// <item>
        /// <term><see cref="OutputDeviceProperty.DriverVersion"/></term>
        /// <term><see cref="int"/></term>
        /// </item>
        /// <item>
        /// <term><see cref="OutputDeviceProperty.UniqueId"/></term>
        /// <term><see cref="int"/></term>
        /// </item>
        /// <item>
        /// <term><see cref="OutputDeviceProperty.DriverOwner"/></term>
        /// <term><see cref="string"/></term>
        /// </item>
        /// <item>
        /// <term><see cref="OutputDeviceProperty.Technology"/></term>
        /// <term><see cref="OutputDeviceTechnology"/></term>
        /// </item>
        /// <item>
        /// <term><see cref="OutputDeviceProperty.VoicesNumber"/></term>
        /// <term><see cref="int"/></term>
        /// </item>
        /// <item>
        /// <term><see cref="OutputDeviceProperty.NotesNumber"/></term>
        /// <term><see cref="int"/></term>
        /// </item>
        /// <item>
        /// <term><see cref="OutputDeviceProperty.Channels"/></term>
        /// <term><see cref="FourBitNumber"/>[]</term>
        /// </item>
        /// <item>
        /// <term><see cref="OutputDeviceProperty.Options"/></term>
        /// <term><see cref="OutputDeviceOption"/></term>
        /// </item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <param name="property">The property to get value of.</param>
        /// <returns>The current value of the <paramref name="property"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="property"/> specified an invalid value.</exception>
        /// <exception cref="ArgumentException"><paramref name="property"/> is not in the list of the properties
        /// supported for the current operating system.</exception>
        /// <exception cref="ObjectDisposedException">The current <see cref="OutputDevice"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on the device. One of the cases when this exception can be thrown
        /// is device is not in the system anymore (for example, unplugged).</exception>
        /// <exception cref="InvalidOperationException">The current <see cref="OutputDevice"/> instance is created by
        /// <see cref="DevicesWatcher.DeviceRemoved"/> event and thus considered as removed so you cannot interact with it.</exception>
        public object GetProperty(OutputDeviceProperty property)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(property), property);

            EnsureDeviceIsNotDisposed();
            EnsureDeviceIsNotRemoved();
            EnsureSessionIsCreated();

            if (!GetSupportedProperties().Contains(property))
                throw new ArgumentException("Property is not supported.", nameof(property));

            int errorCode;

            switch (property)
            {
                case OutputDeviceProperty.Product:
                    {
                        var result = OutputDeviceApi.Api_GetDeviceProduct(_info, out var product, out errorCode);
                        NativeApiUtilities.HandleDevicesNativeApiResult(result, errorCode);
                        return product;
                    }
                case OutputDeviceProperty.Manufacturer:
                    {
                        var result = OutputDeviceApi.Api_GetDeviceManufacturer(_info, out var manufacturer, out errorCode);
                        NativeApiUtilities.HandleDevicesNativeApiResult(result, errorCode);
                        return manufacturer;
                    }
                case OutputDeviceProperty.DriverVersion:
                    {
                        var result = OutputDeviceApi.Api_GetDeviceDriverVersion(_info, out var driverVersion, out errorCode);
                        NativeApiUtilities.HandleDevicesNativeApiResult(result, errorCode);
                        return driverVersion;
                    }
                case OutputDeviceProperty.Technology:
                    {
                        OutputDeviceTechnology technology;
                        var result = OutputDeviceApi.Api_GetDeviceTechnology(_info, out technology, out errorCode);
                        NativeApiUtilities.HandleDevicesNativeApiResult(result, errorCode);
                        return technology;
                    }
                case OutputDeviceProperty.UniqueId:
                    {
                        var result = OutputDeviceApi.Api_GetDeviceUniqueId(_info, out var uniqueId, out errorCode);
                        NativeApiUtilities.HandleDevicesNativeApiResult(result, errorCode);
                        return uniqueId;
                    }
                case OutputDeviceProperty.VoicesNumber:
                    {
                        var result = OutputDeviceApi.Api_GetDeviceVoicesNumber(_info, out var voicesNumber, out errorCode);
                        NativeApiUtilities.HandleDevicesNativeApiResult(result, errorCode);
                        return voicesNumber;
                    }
                case OutputDeviceProperty.NotesNumber:
                    {
                        var result = OutputDeviceApi.Api_GetDeviceNotesNumber(_info, out var notesNumber, out errorCode);
                        NativeApiUtilities.HandleDevicesNativeApiResult(result, errorCode);
                        return notesNumber;
                    }
                case OutputDeviceProperty.Channels:
                    {
                        var result = OutputDeviceApi.Api_GetDeviceChannelsMask(_info, out var channelsMask, out errorCode);
                        NativeApiUtilities.HandleDevicesNativeApiResult(result, errorCode);
                        return (from channel in FourBitNumber.Values
                                let isChannelSupported = (channelsMask >> channel) & 1
                                where isChannelSupported == 1
                                select channel).ToArray();
                    }
                case OutputDeviceProperty.Options:
                    {
                        var result = OutputDeviceApi.Api_GetDeviceOptions(_info, out var option, out errorCode);
                        NativeApiUtilities.HandleDevicesNativeApiResult(result, errorCode);
                        return option;
                    }
                case OutputDeviceProperty.DriverOwner:
                    {
                        var result = OutputDeviceApi.Api_GetDeviceDriverOwner(_info, out var driverOwner, out errorCode);
                        NativeApiUtilities.HandleDevicesNativeApiResult(result, errorCode);
                        return driverOwner;
                    }
                default:
                    throw new NotSupportedException("Property is not supported.");
            }
        }

        /// <summary>
        /// Retrieves the number of output MIDI devices presented in the system.
        /// </summary>
        /// <returns>Number of output MIDI devices presented in the system.</returns>
        /// <exception cref="MidiDeviceException">An error occurred.</exception>
        public static int GetDevicesCount()
        {
            EnsureSessionIsCreated();

            var result = OutputDeviceApi.Api_GetDevicesCount(out var count);
            NativeApiUtilities.HandleDevicesNativeApiResult(result, 0);

            return count;
        }

        /// <summary>
        /// Returns the list of the properties supported by output devices on the current
        /// operating system.
        /// </summary>
        /// <returns>The list of the properties supported by output devices on the current
        /// operating system.</returns>
        public static OutputDeviceProperty[] GetSupportedProperties()
        {
            if (_supportedProperties != null)
                return _supportedProperties;

            return _supportedProperties = Enum.GetValues(typeof(OutputDeviceProperty))
                .OfType<OutputDeviceProperty>()
                .Where(p => OutputDeviceApi.Api_IsPropertySupported(p))
                .ToArray();
        }

        /// <summary>
        /// Retrieves all output MIDI devices presented in the system.
        /// </summary>
        /// <returns>All output MIDI devices presented in the system.</returns>
        /// <exception cref="MidiDeviceException">An error occurred.</exception>
        public static ICollection<OutputDevice> GetAll()
        {
            EnsureSessionIsCreated();

            return GetAllLazy().ToArray();
        }

        /// <summary>
        /// Retrieves an output MIDI device by the specified index.
        /// </summary>
        /// <param name="index">Index of an output device to retrieve.</param>
        /// <returns>Output MIDI device at the specified index.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Index is less than zero or greater than devices count minus 1.</exception>
        /// <exception cref="MidiDeviceException">An error occurred.</exception>
        public static OutputDevice GetByIndex(int index)
        {
            var devicesCount = GetDevicesCount();
            ThrowIfArgument.IsOutOfRange(nameof(index), index, 0, devicesCount - 1, "Index is less than zero or greater than devices count minus 1.");

            EnsureSessionIsCreated();

            var result = OutputDeviceApi.Api_GetDeviceInfo(index, out var info, out var errorCode);
            NativeApiUtilities.HandleDevicesNativeApiResult(result, errorCode);

            return new OutputDevice(info, CreationContext.User);
        }

        /// <summary>
        /// Retrieves a first output MIDI device with the specified name.
        /// </summary>
        /// <param name="name">The name of an output MIDI device to retrieve.</param>
        /// <returns>Output MIDI device with the specified name.</returns>
        /// <exception cref="ArgumentException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="name"/> is <c>null</c> or contains white-spaces only.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="name"/> specifies an output MIDI device which is not presented in the system.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="MidiDeviceException">An error occurred.</exception>
        public static OutputDevice GetByName(string name)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(name), name, "Device name");

            EnsureSessionIsCreated();

            var device = GetAllLazy().FirstOrDefault(d => d.Name == name);
            if (device == null)
                throw new ArgumentException($"There is no output MIDI device '{name}'.", nameof(name));

            return device;
        }

        internal void SendData_Win(byte[] data)
        {
            EnsureDeviceIsNotDisposed();
            EnsureDeviceIsNotRemoved();
            EnsureSessionIsCreated();
            EnsureHandleIsCreated();

            var bufferLength = data.Length;
            var bufferPointer = Marshal.AllocHGlobal(bufferLength);
            Marshal.Copy(data, 0, bufferPointer, data.Length);

            var result = OutputDeviceApi.Api_SendSysExEvent_Win(Handle.DangerousGetHandle(), bufferPointer, bufferLength, out var errorCode);
            NativeApiUtilities.HandleDevicesNativeApiResult(result, errorCode);
        }

        private static IEnumerable<OutputDevice> GetAllLazy()
        {
            var devicesCount = GetDevicesCount();

            for (var i = 0; i < devicesCount; i++)
            {
                yield return GetByIndex(i);
            }
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
                        var result = OutputDeviceApi.Api_OpenDevice_Win(_info, sessionHandle, _callback, out rawHandle, out errorCode);
                        NativeApiUtilities.HandleDevicesNativeApiResult(result, errorCode);
                    }
                    break;
                case CommonApi.API_TYPE.API_TYPE_MAC:
                    {
                        var result = OutputDeviceApi.Api_OpenDevice_Mac(_info, sessionHandle, out rawHandle, out errorCode);
                        NativeApiUtilities.HandleDevicesNativeApiResult(result, errorCode);
                    }
                    break;
                default:
                    throw new NotSupportedException($"{_apiType} API is not supported.");
            }

            Handle = new OutputDeviceHandle(rawHandle);

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

            var result = OutputDeviceApi.Api_SendSysExEvent_Win(Handle.DangerousGetHandle(), bufferPointer, bufferLength, out var errorCode);
            NativeApiUtilities.HandleDevicesNativeApiResult(result, errorCode);
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

            var result = OutputDeviceApi.Api_SendSysExEvent_Mac(Handle.DangerousGetHandle(), buffer, (ushort)buffer.Length, out var errorCode);
            NativeApiUtilities.HandleDevicesNativeApiResult(result, errorCode);
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

            try
            {
                var result = OutputDeviceApi.Api_GetSysExBufferData(Handle.DangerousGetHandle(), sysExHeaderPointer, out var dataPointer, out var size, out var errorCode);
                NativeApiUtilities.HandleDevicesNativeApiResult(result, errorCode);

                data = new byte[size - 1];
                Marshal.Copy(IntPtr.Add(dataPointer, 1), data, 0, data.Length);
                Marshal.FreeHGlobal(dataPointer);
            }
            catch (Exception ex)
            {
                var exception = new MidiDeviceException("Failed to parse sent system exclusive event.", ex);
                exception.Data.Add("Data", data);
                OnError(exception);
            }
        }

        private void OnEventSent(MidiEvent midiEvent)
        {
            EventSent?.Invoke(this, new MidiEventSentEventArgs(midiEvent));
        }

#endregion

        #region Operators

        /// <summary>
        /// Determines if two <see cref="OutputDevice"/> objects are equal.
        /// </summary>
        /// <remarks>
        /// On Windows the operator will just compare objects references. "True" equality check available
        /// on macOS only.
        /// </remarks>
        /// <param name="outputDevice1">The first <see cref="OutputDevice"/> to compare.</param>
        /// <param name="outputDevice2">The second <see cref="OutputDevice"/> to compare.</param>
        /// <returns><c>true</c> if the devices are equal, <c>false</c> otherwise.</returns>
        public static bool operator ==(OutputDevice outputDevice1, OutputDevice outputDevice2)
        {
            if (ReferenceEquals(outputDevice1, outputDevice2))
                return true;

            if (ReferenceEquals(null, outputDevice1) || ReferenceEquals(null, outputDevice2))
                return false;

            return outputDevice1.Equals(outputDevice2);
        }

        /// <summary>
        /// Determines if two <see cref="OutputDevice"/> objects are not equal.
        /// </summary>
        /// <remarks>
        /// On Windows the operator will just compare objects references. "True" inequality check available
        /// on macOS only.
        /// </remarks>
        /// <param name="outputDevice1">The first <see cref="OutputDevice"/> to compare.</param>
        /// <param name="outputDevice2">The second <see cref="OutputDevice"/> to compare.</param>
        /// <returns><c>false</c> if the devices are equal, <c>true</c> otherwise.</returns>
        public static bool operator !=(OutputDevice outputDevice1, OutputDevice outputDevice2)
        {
            return !(outputDevice1 == outputDevice2);
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
            var outputDevice = obj as OutputDevice;
            if (outputDevice == null)
                return false;

            var canCompare = CommonApi.Api_CanCompareDevices();
            return canCompare
                ? OutputDeviceApi.Api_AreDevicesEqual(_info, outputDevice._info)
                : _info.Equals(outputDevice._info);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return _hashCode;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var baseDescription = base.ToString();
            return $"Output device{(string.IsNullOrWhiteSpace(baseDescription) ? string.Empty : $" ({baseDescription})")}";
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
                _midiEventToBytesConverter.Dispose();
                _bytesToMidiEventConverter.Dispose();
                Handle?.Dispose();
                Handle = null;
            }

            _disposed = true;
        }

        #endregion
    }
}
