using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
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

        private readonly HashSet<IntPtr> _sysExHeadersPointers = new HashSet<IntPtr>();

        #endregion

        #region Constructor

        internal OutputDevice(IntPtr info)
            : base(info)
        {
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
                SendShortEvent(midiEvent);
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
                // TODO: handle result
                IntPtr info;
                var result = OutputDeviceApiProvider.Api.Api_GetDeviceInfo(i, out info);
                yield return new OutputDevice(info);
            }
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
                throw new ArgumentException($"There is no output MIDI device '{name}' among {GetDevicesCount()} device(s) ({string.Join(", ", GetAll().Select(d => d.Name))}).", nameof(name));

            return device;
        }

        /// <summary>
        /// Gets error description for the specified MMRESULT which is return value of winmm function.
        /// </summary>
        /// <param name="mmrError">MMRESULT which is return value of winmm function.</param>
        /// <param name="pszText"><see cref="StringBuilder"/> to write error description to.</param>
        /// <param name="cchText">Size of <paramref name="pszText"/> buffer.</param>
        /// <returns>Return value of winmm function which gets error description.</returns>
        protected override uint GetErrorText(uint mmrError, StringBuilder pszText, uint cchText)
        {
            return MidiOutWinApi.midiOutGetErrorText(mmrError, pszText, cchText);
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
            if (_windowsHandle != IntPtr.Zero)
                return;

            _callback = OnMessage;

            var sessionHandle = MidiDevicesSession.GetSessionHandle();

            // TODO: handle result
            var result = OutputDeviceApiProvider.Api.Api_OpenDevice_Winmm(_info, sessionHandle, _callback, out _handle);
            if (result != OutputDeviceApi.OUT_OPENRESULT.OUT_OPENRESULT_OK)
            {
                switch (result)
                {
                    case OutputDeviceApi.OUT_OPENRESULT.OUT_OPENRESULT_ALLOCATED:
                        throw new MidiDeviceException("The device is already in use.");
                    case OutputDeviceApi.OUT_OPENRESULT.OUT_OPENRESULT_NOMEMORY:
                        throw new MidiDeviceException("There is no memory to allocate the requested resources.");
                }

                throw new MidiDeviceException($"Unknown error ({result}).");
            }

            _windowsHandle = OutputDeviceApiProvider.Api.Api_GetHandle(_handle);
        }

        private void DestroyHandle()
        {
            if (_handle == IntPtr.Zero)
                return;

            OutputDeviceApiProvider.Api.Api_CloseDevice(_handle);

            _handle = IntPtr.Zero;
            _windowsHandle = IntPtr.Zero;
        }

        private void SendShortEvent(MidiEvent midiEvent)
        {
            var message = PackShortEvent(midiEvent);
            ProcessMmResult(MidiOutWinApi.midiOutShortMsg(_windowsHandle, (uint)message));
        }

        private void SendSysExEvent(SysExEvent sysExEvent)
        {
            var data = sysExEvent.Data;
            if (data == null || !data.Any())
                return;

            var headerPointer = PrepareSysExBuffer(data);
            _sysExHeadersPointers.Add(headerPointer);

            ProcessMmResult(MidiOutWinApi.midiOutLongMsg(_windowsHandle, headerPointer, MidiWinApi.MidiHeaderSize));
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

        private void OnMessage(IntPtr hMidi, MidiMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2)
        {
            switch (wMsg)
            {
                case MidiMessage.MOM_DONE:
                    OnSysExEventSent(dwParam1);
                    break;
            }
        }

        private void OnSysExEventSent(IntPtr sysExHeaderPointer)
        {
            byte[] data = null;

            try
            {
                data = MidiWinApi.UnpackSysExBytes(sysExHeaderPointer);
                var midiEvent = new NormalSysExEvent(data);
                OnEventSent(midiEvent);

                UnprepareSysExBuffer(sysExHeaderPointer);
                _sysExHeadersPointers.Remove(sysExHeaderPointer);
            }
            catch (Exception ex)
            {
                var exception = new MidiDeviceException("Failed to parse sent system exclusive event.", ex);
                exception.Data.Add("Data", data);
                OnError(exception);
            }
        }

        private IntPtr PrepareSysExBuffer(byte[] data)
        {
            var bufferLength = data.Length + 1;
            var dataPointer = Marshal.AllocHGlobal(bufferLength);
            Marshal.WriteByte(dataPointer, EventStatusBytes.Global.NormalSysEx);
            Marshal.Copy(data, 0, IntPtr.Add(dataPointer, 1), data.Length);

            var header = new MidiWinApi.MIDIHDR
            {
                lpData = dataPointer,
                dwBufferLength = bufferLength,
                dwBytesRecorded = bufferLength
            };

            var headerPointer = Marshal.AllocHGlobal(MidiWinApi.MidiHeaderSize);
            Marshal.StructureToPtr(header, headerPointer, false);

            ProcessMmResult(MidiOutWinApi.midiOutPrepareHeader(_windowsHandle, headerPointer, MidiWinApi.MidiHeaderSize));

            return headerPointer;
        }

        private void UnprepareSysExBuffer(IntPtr headerPointer)
        {
            if (headerPointer == IntPtr.Zero)
                return;

            MidiOutWinApi.midiOutUnprepareHeader(_windowsHandle, headerPointer, MidiWinApi.MidiHeaderSize);

            var header = (MidiWinApi.MIDIHDR)Marshal.PtrToStructure(headerPointer, typeof(MidiWinApi.MIDIHDR));
            Marshal.FreeHGlobal(header.lpData);
            Marshal.FreeHGlobal(headerPointer);
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
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _midiEventToBytesConverter.Dispose();
                _bytesToMidiEventConverter.Dispose();
            }

            DestroyHandle();

            foreach (var sysExHeaderPointer in _sysExHeadersPointers)
            {
                UnprepareSysExBuffer(sysExHeaderPointer);
            }

            _disposed = true;
        }

        #endregion
    }
}
