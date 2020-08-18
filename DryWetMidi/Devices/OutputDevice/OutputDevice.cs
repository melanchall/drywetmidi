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

        private MidiWinApi.MidiMessageCallback _callback;

        private readonly HashSet<IntPtr> _sysExHeadersPointers = new HashSet<IntPtr>();

        #endregion

        #region Constructor

        internal OutputDevice(int id)
            : base(id)
        {
            SetDeviceInformation();
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

        #region Properties

        /// <summary>
        /// Gets the type of the current <see cref="OutputDevice"/>.
        /// </summary>
        public OutputDeviceType DeviceType { get; private set; }

        /// <summary>
        /// Gets the number of voices supported by an internal synthesizer device. If the device is a port,
        /// this member is not meaningful and will be 0.
        /// </summary>
        public int VoicesNumber { get; private set; }

        /// <summary>
        /// Gets the maximum number of simultaneous notes that can be played by an internal synthesizer device.
        /// If the device is a port, this member is not meaningful and will be 0.
        /// </summary>
        public int NotesNumber { get; private set; }

        /// <summary>
        /// Gets the channels that an internal synthesizer device responds to.
        /// </summary>
        public IEnumerable<FourBitNumber> Channels { get; private set; }

        /// <summary>
        /// Gets a value indicating whether device supports patch caching.
        /// </summary>
        public bool SupportsPatchCaching { get; private set; }

        /// <summary>
        /// Gets a value indicating whether device supports separate left and right volume control or not.
        /// </summary>
        public bool SupportsLeftRightVolumeControl { get; private set; }

        /// <summary>
        /// Gets a value indicating whether device supports volume control.
        /// </summary>
        public bool SupportsVolumeControl { get; private set; }

        /// <summary>
        /// Gets or sets the volume of the output MIDI device.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current <see cref="OutputDevice"/> is disposed.</exception>
        /// <exception cref="InvalidOperationException">Device doesn't support volume control.</exception>
        /// <exception cref="ArgumentException">Device doesn't support separate volume control for each channel.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public Volume Volume
        {
            get
            {
                EnsureDeviceIsNotDisposed();
                EnsureHandleIsCreated();

                if (!SupportsVolumeControl)
                    throw new InvalidOperationException("Device doesn't support volume control.");

                var volume = default(uint);
                ProcessMmResult(MidiOutWinApi.midiOutGetVolume(_handle, ref volume));

                var leftVolume = volume.GetTail();
                var rightVolume = volume.GetHead();

                return SupportsLeftRightVolumeControl
                    ? new Volume(leftVolume, rightVolume)
                    : new Volume(leftVolume, leftVolume);
            }
            set
            {
                EnsureDeviceIsNotDisposed();
                EnsureHandleIsCreated();

                if (!SupportsVolumeControl)
                    throw new InvalidOperationException("Device doesn't support volume control.");

                var leftVolume = value.LeftVolume;
                var rightVolume = value.RightVolume;

                if (!SupportsLeftRightVolumeControl && leftVolume != rightVolume)
                    throw new ArgumentException("Device doesn't support separate volume control for each channel.", nameof(value));

                var volume = DataTypesUtilities.Combine(rightVolume, leftVolume);
                ProcessMmResult(MidiOutWinApi.midiOutSetVolume(_handle, volume));
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
        public void SendEvent(MidiEvent midiEvent)
        {
            ThrowIfArgument.IsNull(nameof(midiEvent), midiEvent);

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
            return (int)MidiOutWinApi.midiOutGetNumDevs();
        }

        /// <summary>
        /// Retrieves all output MIDI devices presented in the system.
        /// </summary>
        /// <returns>All output MIDI devices presented in the system.</returns>
        public static IEnumerable<OutputDevice> GetAll()
        {
            var devicesCount = GetDevicesCount();
            for (var deviceId = 0; deviceId < devicesCount; deviceId++)
            {
                yield return new OutputDevice(deviceId);
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
                throw new ArgumentException($"There is no output MIDI device '{name}'.", nameof(name));

            return device;
        }

        /// <summary>
        /// Retrieves output MIDI device with the specified ID.
        /// </summary>
        /// <param name="id">Device ID which is number from 0 to <see cref="GetDevicesCount"/> minus 1.</param>
        /// <returns>Output MIDI device with the specified ID.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is out of valid range.</exception>
        public static OutputDevice GetById(int id)
        {
            ThrowIfArgument.IsOutOfRange(nameof(id), id, 0, GetDevicesCount() - 1, "Device ID is out of range.");

            return new OutputDevice(id);
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

        private void EnsureHandleIsCreated()
        {
            if (_handle != IntPtr.Zero)
                return;

            _callback = OnMessage;
            ProcessMmResult(MidiOutWinApi.midiOutOpen(out _handle, Id, _callback, IntPtr.Zero, MidiWinApi.CallbackFunction));
        }

        private void DestroyHandle()
        {
            if (_handle == IntPtr.Zero)
                return;

            MidiOutWinApi.midiOutClose(_handle);
        }

        private void SetDeviceInformation()
        {
            var caps = default(MidiOutWinApi.MIDIOUTCAPS);
            ProcessMmResult(MidiOutWinApi.midiOutGetDevCaps(new IntPtr(Id), ref caps, (uint)Marshal.SizeOf(caps)));

            SetBasicDeviceInformation(caps.wMid, caps.wPid, caps.vDriverVersion, caps.szPname);

            DeviceType = (OutputDeviceType)caps.wTechnology;
            VoicesNumber = caps.wVoices;
            NotesNumber = caps.wNotes;
            Channels = (from channel in FourBitNumber.Values
                        let isChannelSupported = (caps.wChannelMask >> channel) & 1
                        where isChannelSupported == 1
                        select channel).ToArray();

            var support = (MidiOutWinApi.MIDICAPS)caps.dwSupport;
            SupportsPatchCaching = support.HasFlag(MidiOutWinApi.MIDICAPS.MIDICAPS_CACHE);
            SupportsVolumeControl = support.HasFlag(MidiOutWinApi.MIDICAPS.MIDICAPS_VOLUME);
            SupportsLeftRightVolumeControl = support.HasFlag(MidiOutWinApi.MIDICAPS.MIDICAPS_LRVOLUME);
        }

        private void SendShortEvent(MidiEvent midiEvent)
        {
            var message = PackShortEvent(midiEvent);
            ProcessMmResult(MidiOutWinApi.midiOutShortMsg(_handle, (uint)message));
        }

        private void SendSysExEvent(SysExEvent sysExEvent)
        {
            var data = sysExEvent.Data;
            if (data == null || !data.Any())
                return;

            var headerPointer = PrepareSysExBuffer(data);
            _sysExHeadersPointers.Add(headerPointer);

            ProcessMmResult(MidiOutWinApi.midiOutLongMsg(_handle, headerPointer, MidiWinApi.MidiHeaderSize));
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

            ProcessMmResult(MidiOutWinApi.midiOutPrepareHeader(_handle, headerPointer, MidiWinApi.MidiHeaderSize));

            return headerPointer;
        }

        private void UnprepareSysExBuffer(IntPtr headerPointer)
        {
            if (headerPointer == IntPtr.Zero)
                return;

            MidiOutWinApi.midiOutUnprepareHeader(_handle, headerPointer, MidiWinApi.MidiHeaderSize);

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

        internal override IntPtr GetHandle()
        {
            EnsureHandleIsCreated();
            return _handle;
        }

        #endregion
    }
}
