using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Devices
{
    /// <summary>
    /// Represents an output MIDI device.
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

        private readonly MidiEventToBytesConverter _midiEventToBytesConverter = new MidiEventToBytesConverter(ShortEventBufferSize);
        private readonly BytesToMidiEventConverter _bytesToMidiEventConverter = new BytesToMidiEventConverter();

        private OutputDeviceApi.Callback_Winmm _callback;

        private readonly OutputDeviceApi.API_TYPE _apiType;

        #endregion

        #region Constructor

        internal OutputDevice(IntPtr info)
            : this(info, DeviceOwner.User)
        {
        }

        internal OutputDevice(IntPtr info, DeviceOwner owner)
            : base(info, owner)
        {
            _apiType = OutputDeviceApiProvider.Api.Api_GetApiType();
        }

        #endregion

        #region Finalizer

        /// <summary>
        /// Finalizes the current instance of the <see cref="OutputDevice"/>.
        /// </summary>
        ~OutputDevice()
        {
            Dispose(false);
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
        public void SendEvent(MidiEvent midiEvent)
        {
            ThrowIfArgument.IsNull(nameof(midiEvent), midiEvent);

            if (!IsEnabled)
                return;

            EnsureDeviceIsNotDisposed();
            EnsureHandleIsCreated();

            if (midiEvent is ChannelEvent || midiEvent is SystemCommonEvent || midiEvent is SystemRealTimeEvent)
            {
                var message = PackShortEvent(midiEvent);
                NativeApi.HandleResult(
                    OutputDeviceApiProvider.Api.Api_SendShortEvent(_handle, message));
                OnEventSent(midiEvent);
            }
            else
            {
                var sysExEvent = midiEvent as SysExEvent;
                if (sysExEvent != null)
                    SendSysExEvent(sysExEvent);
            }
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
            EnsureHandleIsCreated();
        }

        /// <summary>
        /// Retrieves the number of output MIDI devices presented in the system.
        /// </summary>
        /// <returns>Number of output MIDI devices presented in the system.</returns>
        public static int GetDevicesCount()
        {
            return OutputDeviceApiProvider.Api.Api_GetDevicesCount();
        }

        /// <summary>
        /// Retrieves all output MIDI devices presented in the system.
        /// </summary>
        /// <returns>All output MIDI devices presented in the system.</returns>
        public static IEnumerable<OutputDevice> GetAll()
        {
            var devicesCount = GetDevicesCount();

            for (var i = 0; i < devicesCount; i++)
            {
                yield return GetByIndex(i);
            }
        }

        public static OutputDevice GetByIndex(int index)
        {
            var devicesCount = GetDevicesCount();
            ThrowIfArgument.IsOutOfRange(nameof(index), index, 0, devicesCount - 1, "Index is less than zero or greater than devices count minus 1.");

            IntPtr info;
            NativeApi.HandleResult(OutputDeviceApiProvider.Api.Api_GetDeviceInfo(index, out info));

            return new OutputDevice(info);
        }

        /// <summary>
        /// Retrieves a first output MIDI device with the specified name.
        /// </summary>
        /// <param name="name">The name of an output MIDI device to retrieve.</param>
        /// <returns>Output MIDI device with the specified name.</returns>
        /// <exception cref="ArgumentException">
        /// <para>One of the following errors occured:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="name"/> is <c>null</c> or contains white-spaces only.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="name"/> specifies an output MIDI device which is not presented in the system.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static OutputDevice GetByName(string name)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(name), name, "Device name");

            var device = GetAll().FirstOrDefault(d => d.Name == name);
            if (device == null)
                throw new ArgumentException($"There is no output MIDI device '{name}'.", nameof(name));

            return device;
        }

        protected override void SetBasicDeviceInformation()
        {
            Name = OutputDeviceApiProvider.Api.Api_GetDeviceName(_info);
            Manufacturer = OutputDeviceApiProvider.Api.Api_GetDeviceManufacturer(_info);
            Product = OutputDeviceApiProvider.Api.Api_GetDeviceProduct(_info);
            DriverVersion = OutputDeviceApiProvider.Api.Api_GetDeviceDriverVersion(_info);
        }

        private void EnsureHandleIsCreated()
        {
            if (_handle != IntPtr.Zero)
                return;

            var sessionHandle = MidiDevicesSession.GetSessionHandle();

            switch (_apiType)
            {
                case OutputDeviceApi.API_TYPE.API_TYPE_WINMM:
                    {
                        _callback = OnMessage;
                        NativeApi.HandleResult(
                            OutputDeviceApiProvider.Api.Api_OpenDevice_Winmm(_info, sessionHandle, _callback, out _handle));
                    }
                    break;
                case OutputDeviceApi.API_TYPE.API_TYPE_APPLE:
                    {
                        NativeApi.HandleResult(
                            OutputDeviceApiProvider.Api.Api_OpenDevice_Apple(_info, sessionHandle, out _handle));
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

            OutputDeviceApiProvider.Api.Api_CloseDevice(_handle);
            _handle = IntPtr.Zero;

            MidiDevicesSession.ExitSession();
        }

        private void SendSysExEvent(SysExEvent sysExEvent)
        {
            var data = sysExEvent.Data;
            if (data == null || !data.Any())
                return;

            switch (_apiType)
            {
                case OutputDeviceApi.API_TYPE.API_TYPE_WINMM:
                    SendSysExEventData_Winmm(data);
                    break;
                case OutputDeviceApi.API_TYPE.API_TYPE_APPLE:
                    SendSysExEventData_Apple(data);
                    OnEventSent(sysExEvent);
                    break;
                default:
                    throw new NotSupportedException($"{_apiType} API is not supported.");
            }
        }

        private void SendSysExEventData_Winmm(byte[] data)
        {
            var bufferLength = data.Length + 1;
            var bufferPointer = Marshal.AllocHGlobal(bufferLength);
            Marshal.WriteByte(bufferPointer, EventStatusBytes.Global.NormalSysEx);
            Marshal.Copy(data, 0, IntPtr.Add(bufferPointer, 1), data.Length);

            NativeApi.HandleResult(
                OutputDeviceApiProvider.Api.Api_SendSysExEvent_Winmm(_handle, bufferPointer, bufferLength));
        }

        private void SendSysExEventData_Apple(byte[] data)
        {
            var buffer = new byte[data.Length + 1];
            buffer[0] = EventStatusBytes.Global.NormalSysEx;
            Buffer.BlockCopy(data, 0, buffer, 1, data.Length);

            NativeApi.HandleResult(
                OutputDeviceApiProvider.Api.Api_SendSysExEvent_Apple(_handle, buffer, (ushort)buffer.Length));
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
                IntPtr dataPointer;
                int size;

                NativeApi.HandleResult(
                    OutputDeviceApiProvider.Api.Api_GetSysExBufferData(_handle, sysExHeaderPointer, out dataPointer,  out size));

                data = new byte[size - 1];
                Marshal.Copy(IntPtr.Add(dataPointer, 1), data, 0, data.Length);
                Marshal.FreeHGlobal(dataPointer);

                var midiEvent = new NormalSysExEvent(data);
                OnEventSent(midiEvent);
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

        #region Overrides

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
            }

            DestroyHandle();

            _disposed = true;
        }

        #endregion
    }
}
