using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf;

namespace Melanchall.DryWetMidi.Devices
{
    public sealed class OutputDevice : MidiDevice
    {
        #region Constants

        private const int ChannelEventBufferSize = 3;
        private static readonly byte[] ZeroBuffer = new byte[ChannelEventBufferSize];

        private const int SysExBufferLength = 2048;

        #endregion

        #region Events

        public event EventHandler<MidiEventSentEventArgs> EventSent;

        #endregion

        #region Fields

        private readonly MemoryStream _memoryStream = new MemoryStream(ChannelEventBufferSize);
        private readonly MidiWriter _midiWriter;
        private readonly WritingSettings _writingSettings = new WritingSettings();
        private MidiWinApi.MidiMessageCallback _callback;

        private readonly HashSet<IntPtr> _sysExHeadersPointers = new HashSet<IntPtr>();

        #endregion

        #region Constructor

        internal OutputDevice(uint id)
            : base(id)
        {
            _midiWriter = new MidiWriter(_memoryStream);

            SetDeviceInformation();
        }

        #endregion

        #region Finalizer

        ~OutputDevice()
        {
            Dispose(false);
        }

        #endregion

        #region Properties

        public OutputDeviceType DeviceType { get; private set; }

        public int VoicesNumber { get; private set; }

        public int NotesNumber { get; private set; }

        public IEnumerable<FourBitNumber> Channels { get; private set; }

        public bool SupportsPatchCaching { get; private set; }

        public bool SupportsLeftRightVolumeControl { get; private set; }

        public bool SupportsVolumeControl { get; private set; }

        public Volume Volume
        {
            get
            {
                var volume = default(uint);
                MidiWinApi.ProcessMmResult(() => MidiOutWinApi.midiOutGetVolume(_handle, ref volume), GetErrorText);

                var leftVolume = volume.GetTail();
                var rightVolume = volume.GetHead();

                return SupportsLeftRightVolumeControl
                    ? new Volume(leftVolume, rightVolume)
                    : new Volume(leftVolume, leftVolume);
            }
            set
            {
                var leftVolume = value.LeftVolume;
                var rightVolume = value.RightVolume;

                if (!SupportsLeftRightVolumeControl && leftVolume != rightVolume)
                    throw new ArgumentException("Device doesn't support separate volume control for each channel.", nameof(value));

                var volume = DataTypesUtilities.Combine(rightVolume, leftVolume);
                MidiWinApi.ProcessMmResult(() => MidiOutWinApi.midiOutSetVolume(_handle, volume), GetErrorText);
            }
        }

        #endregion

        #region Methods

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

        public void TurnAllNotesOff()
        {
            var allNotesOffEvents = from channel in FourBitNumber.Values
                                    from noteNumber in SevenBitNumber.Values
                                    select new NoteOffEvent(noteNumber, SevenBitNumber.MinValue) { Channel = channel };

            foreach (var noteOffEvent in allNotesOffEvents)
            {
                SendEvent(noteOffEvent);
            }
        }

        public static uint GetDevicesCount()
        {
            return MidiOutWinApi.midiOutGetNumDevs();
        }

        public static IEnumerable<OutputDevice> GetAll()
        {
            var devicesCount = GetDevicesCount();
            for (var deviceId = 0U; deviceId < devicesCount; deviceId++)
            {
                yield return new OutputDevice(deviceId);
            }
        }

        public static OutputDevice GetByName(string name)
        {
            ThrowIfArgument.IsNullOrEmptyString(nameof(name), name, "Device name");

            var device = GetAll().FirstOrDefault(d => d.Name == name);
            if (device == null)
                throw new MidiDeviceException($"There is no output device named '{name}'.");

            return device;
        }

        public static OutputDevice GetById(int id)
        {
            ThrowIfArgument.IsNegative(nameof(id), id, "Device ID is negative.");

            return new OutputDevice((uint)id);
        }

        internal void PrepareForEventsSending()
        {
            EnsureHandleIsCreated();
        }

        private void EnsureHandleIsCreated()
        {
            if (_handle != IntPtr.Zero)
                return;

            _callback = OnMessage;
            MidiWinApi.ProcessMmResult(() => MidiOutWinApi.midiOutOpen(out _handle, _id, _callback, IntPtr.Zero, MidiWinApi.CallbackFunction), GetErrorText);
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
            MidiWinApi.ProcessMmResult(() => MidiOutWinApi.midiOutGetDevCaps(new UIntPtr(_id), ref caps, (uint)Marshal.SizeOf(caps)), GetErrorText);

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
            MidiWinApi.ProcessMmResult(() => MidiOutWinApi.midiOutShortMsg(_handle, (uint)message), GetErrorText);
        }

        private void SendSysExEvent(SysExEvent sysExEvent)
        {
            var data = sysExEvent.Data;
            if (data == null || !data.Any())
                return;

            var headerPointer = PrepareSysExBuffer(data);
            _sysExHeadersPointers.Add(headerPointer);

            // TODO: catch exception
            MidiWinApi.ProcessMmResult(() => MidiOutWinApi.midiOutLongMsg(_handle, headerPointer, MidiWinApi.MidiHeaderSize), GetErrorText);
        }

        private int PackShortEvent(MidiEvent midiEvent)
        {
            WriteBytesToStream(_memoryStream, ZeroBuffer);

            var eventWriter = EventWriterFactory.GetWriter(midiEvent);
            eventWriter.Write(midiEvent, _midiWriter, _writingSettings, true);

            var bytes = _memoryStream.GetBuffer();
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
            var data = MidiWinApi.UnpackSysExBytes(sysExHeaderPointer);
            var midiEvent = new NormalSysExEvent(data);
            OnEventSent(midiEvent);

            UnprepareSysExBuffer(sysExHeaderPointer);
            _sysExHeadersPointers.Remove(sysExHeaderPointer);
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

            MidiWinApi.ProcessMmResult(() => MidiOutWinApi.midiOutPrepareHeader(_handle, headerPointer, MidiWinApi.MidiHeaderSize), GetErrorText);

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

        private static uint GetErrorText(uint mmrError, StringBuilder pszText, uint cchText)
        {
            return MidiOutWinApi.midiOutGetErrorText(mmrError, pszText, cchText);
        }

        #endregion

        #region Overrides

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _memoryStream.Dispose();
                _midiWriter.Dispose();
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
