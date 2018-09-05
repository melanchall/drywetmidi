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

        #endregion

        #region Fields

        private readonly MemoryStream _memoryStream = new MemoryStream(ChannelEventBufferSize);
        private readonly MidiWriter _midiWriter;
        private readonly WritingSettings _writingSettings = new WritingSettings();
        private MidiWinApi.MidiMessageCallback _callback;
        private IntPtr _streamHandle;

        #endregion

        #region Constructor

        internal OutputDevice(uint id)
            : base(id)
        {
            _midiWriter = new MidiWriter(_memoryStream);

            SetDeviceInformation();
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

        #endregion

        #region Methods

        public void SendEvent(MidiEvent midiEvent)
        {
            ThrowIfArgument.IsNull(nameof(midiEvent), midiEvent);

            EnsureDeviceIsNotDisposed();
            EnsureHandleIsCreated();

            var channelEvent = midiEvent as ChannelEvent;
            if (channelEvent != null)
            {
                SendChannelEvent(channelEvent);
                return;
            }

            var sysExEvent = midiEvent as SysExEvent;
            if (sysExEvent != null)
            {
                // TODO: implement sending SysEx events
            }
        }

        public static int GetDevicesCount()
        {
            // TODO: process last error
            // TODO: uint instead of int
            return MidiOutWinApi.midiOutGetNumDevs();
        }

        public static IEnumerable<OutputDevice> GetAll()
        {
            var devicesCount = GetDevicesCount();
            return Enumerable.Range(0, devicesCount).Select(i => new OutputDevice((uint)i));
        }

        public static OutputDevice GetByName(string name)
        {
            ThrowIfArgument.IsNullOrEmptyString(nameof(name), name, "Device name");

            return GetAll().FirstOrDefault(d => d.Name == name);
        }

        public static OutputDevice GetDefault()
        {
            return new OutputDevice(unchecked((uint)-1));
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
            ProcessMmResult(() => MidiOutWinApi.midiOutOpen(out _handle, _id, _callback, IntPtr.Zero, MidiWinApi.CallbackFunction));
        }

        private void DestroyHandle()
        {
            MidiOutWinApi.midiOutClose(_handle);
        }

        private void SetDeviceInformation()
        {
            var caps = default(MidiOutWinApi.MIDIOUTCAPS);
            ProcessMmResult(() => MidiOutWinApi.midiOutGetDevCaps(new UIntPtr(_id), ref caps, (uint)Marshal.SizeOf(caps)));

            SetBasicDeviceInformation(caps.wMid, caps.wPid, caps.vDriverVersion, caps.szPname);

            DeviceType = (OutputDeviceType)caps.wTechnology;
            VoicesNumber = caps.wVoices;
            NotesNumber = caps.wNotes;
            Channels = (from i in Enumerable.Range(0, FourBitNumber.MaxValue + 1)
                        let channel = (FourBitNumber)i
                        let isChannelSupported = (caps.wChannelMask >> i) & 1
                        where isChannelSupported == 1
                        select channel).ToArray();

            var support = (MidiOutWinApi.MIDICAPS)caps.dwSupport;
            SupportsPatchCaching = support.HasFlag(MidiOutWinApi.MIDICAPS.MIDICAPS_CACHE);
            SupportsVolumeControl = support.HasFlag(MidiOutWinApi.MIDICAPS.MIDICAPS_VOLUME);
            SupportsLeftRightVolumeControl = support.HasFlag(MidiOutWinApi.MIDICAPS.MIDICAPS_LRVOLUME);
        }

        private void SendChannelEvent(ChannelEvent channelEvent)
        {
            var message = PackChannelEvent(channelEvent);
            ProcessMmResult(() => MidiOutWinApi.midiOutShortMsg(_handle, (uint)message));
        }

        private int PackChannelEvent(ChannelEvent channelEvent)
        {
            var eventWriter = EventWriterFactory.GetWriter(channelEvent);

            var statusByte = eventWriter.GetStatusByte(channelEvent);

            WriteBytesToStream(_memoryStream, ZeroBuffer);
            eventWriter.Write(channelEvent, _midiWriter, _writingSettings, true);

            var bytes = _memoryStream.GetBuffer();
            return bytes[0] + (bytes[1] << 8) + (bytes[2] << 16);
        }

        private void OnMessage(IntPtr hMidi, MidiMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2)
        {
            // TODO: process MOM_DONE
        }

        #endregion

        #region Overrides

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_handle == IntPtr.Zero)
                    return;

                DestroyHandle();

                _memoryStream.Dispose();
                _midiWriter.Dispose();
            }

            _disposed = true;
        }

        internal override MMRESULT GetErrorText(MMRESULT mmrError, StringBuilder pszText, uint cchText)
        {
            return MidiOutWinApi.midiOutGetErrorText(mmrError, pszText, cchText);
        }

        #endregion
    }
}
