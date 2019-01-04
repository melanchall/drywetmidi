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
    public sealed class InputDevice : MidiDevice
    {
        #region Constants

        private const int SysExBufferLength = 2048;
        private const int ChannelParametersBufferSize = 2;
        private static readonly ReadingSettings ReadingSettings = new ReadingSettings();
        private static readonly int MidiTimeCodeComponentsCount = Enum.GetValues(typeof(MidiTimeCodeComponent)).Length;

        #endregion

        #region Events

        public event EventHandler<MidiEventReceivedEventArgs> EventReceived;
        public event EventHandler<MidiTimeCodeReceivedEventArgs> MidiTimeCodeReceived;

        #endregion

        #region Fields

        private DateTime _startTime;

        private readonly MemoryStream _channelMessageMemoryStream = new MemoryStream(ChannelParametersBufferSize);
        private readonly MidiReader _channelEventReader;

        private readonly MemoryStream _sysExMessageMemoryStream = new MemoryStream(SysExBufferLength);
        private readonly MidiReader _sysExEventReader;
        private IntPtr _sysExHeaderPointer = IntPtr.Zero;

        private MidiWinApi.MidiMessageCallback _callback;

        private readonly Dictionary<MidiTimeCodeComponent, FourBitNumber> _midiTimeCodeComponents = new Dictionary<MidiTimeCodeComponent, FourBitNumber>();

        #endregion

        #region Constructor

        internal InputDevice(uint id)
            : base(id)
        {
            _channelEventReader = new MidiReader(_channelMessageMemoryStream);
            _sysExEventReader = new MidiReader(_sysExMessageMemoryStream);

            SetDeviceInformation();
        }

        #endregion

        #region Properties

        public bool RaiseMidiTimeCodeReceived { get; set; } = true;

        #endregion

        #region Methods

        public void StartEventsListening()
        {
            EnsureDeviceIsNotDisposed();
            EnsureHandleIsCreated();

            PrepareSysExBuffer();
            MidiWinApi.ProcessMmResult(() => MidiInWinApi.midiInStart(_handle), GetErrorText);
            _startTime = DateTime.UtcNow;
        }

        public void StopEventsListening()
        {
            EnsureDeviceIsNotDisposed();

            if (_handle == IntPtr.Zero)
                return;

            MidiWinApi.ProcessMmResult(() => MidiInWinApi.midiInStop(_handle), GetErrorText);
        }

        public void Reset()
        {
            EnsureDeviceIsNotDisposed();

            if (_handle == IntPtr.Zero)
                return;

            MidiWinApi.ProcessMmResult(() => MidiInWinApi.midiInReset(_handle), GetErrorText);
            _startTime = DateTime.UtcNow;
        }

        public static uint GetDevicesCount()
        {
            return MidiInWinApi.midiInGetNumDevs();
        }

        public static IEnumerable<InputDevice> GetAll()
        {
            var devicesCount = GetDevicesCount();
            for (var deviceId = 0U; deviceId < devicesCount; deviceId++)
            {
                yield return new InputDevice(deviceId);
            }
        }

        public static InputDevice GetByName(string name)
        {
            ThrowIfArgument.IsNullOrEmptyString(nameof(name), name, "Device name");

            var device = GetAll().FirstOrDefault(d => d.Name == name);
            if (device == null)
                throw new MidiDeviceException($"There is no input device named '{name}'.");

            return device;
        }

        public static InputDevice GetById(int id)
        {
            ThrowIfArgument.IsNegative(nameof(id), id, "Device ID is negative.");

            return new InputDevice((uint)id);
        }

        private void OnEventReceived(MidiEvent midiEvent, int milliseconds)
        {
            EventReceived?.Invoke(this, new MidiEventReceivedEventArgs(midiEvent, _startTime.AddMilliseconds(milliseconds)));
        }

        private void OnMidiTimeCodeReceived(MidiTimeCodeType timeCodeType, int hours, int minutes, int seconds, int frames)
        {
            MidiTimeCodeReceived?.Invoke(this, new MidiTimeCodeReceivedEventArgs(timeCodeType, hours, minutes, seconds, frames));
        }

        private void PrepareSysExBuffer()
        {
            var header = new MidiWinApi.MIDIHDR
            {
                // TODO: investigate reusing allocated memory
                lpData = Marshal.AllocHGlobal(SysExBufferLength),
                dwBufferLength = SysExBufferLength,
                dwBytesRecorded = SysExBufferLength
            };

            _sysExHeaderPointer = Marshal.AllocHGlobal(MidiWinApi.MidiHeaderSize);
            Marshal.StructureToPtr(header, _sysExHeaderPointer, false);

            MidiWinApi.ProcessMmResult(() => MidiInWinApi.midiInPrepareHeader(_handle, _sysExHeaderPointer, MidiWinApi.MidiHeaderSize), GetErrorText);
            MidiWinApi.ProcessMmResult(() => MidiInWinApi.midiInAddBuffer(_handle, _sysExHeaderPointer, MidiWinApi.MidiHeaderSize), GetErrorText);
        }

        private void UnprepareSysExBuffer(IntPtr headerPointer)
        {
            if (headerPointer == IntPtr.Zero)
                return;

            MidiInWinApi.midiInUnprepareHeader(_handle, headerPointer, MidiWinApi.MidiHeaderSize);

            try
            {
                var header = (MidiWinApi.MIDIHDR)Marshal.PtrToStructure(headerPointer, typeof(MidiWinApi.MIDIHDR));
                Marshal.FreeHGlobal(header.lpData);
            }
            finally
            {
                Marshal.FreeHGlobal(headerPointer);
            }
        }

        private void EnsureHandleIsCreated()
        {
            if (_handle != IntPtr.Zero)
                return;

            _callback = OnMessage;
            MidiWinApi.ProcessMmResult(() => MidiInWinApi.midiInOpen(out _handle, _id, _callback, IntPtr.Zero, MidiWinApi.CallbackFunction), GetErrorText);
        }

        private void DestroyHandle()
        {
            MidiInWinApi.midiInClose(_handle);
        }

        private void SetDeviceInformation()
        {
            var caps = default(MidiInWinApi.MIDIINCAPS);
            MidiWinApi.ProcessMmResult(() => MidiInWinApi.midiInGetDevCaps(new UIntPtr(_id), ref caps, (uint)Marshal.SizeOf(caps)), GetErrorText);

            SetBasicDeviceInformation(caps.wMid, caps.wPid, caps.vDriverVersion, caps.szPname);
        }

        private void OnMessage(IntPtr hMidi, MidiMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2)
        {
            switch (wMsg)
            {
                case MidiMessage.MIM_DATA:
                    OnShortMessage(dwParam1.ToInt32(), dwParam2.ToInt32());
                    break;

                case MidiMessage.MIM_LONGDATA:
                    OnSysExMessage(dwParam1, dwParam2.ToInt32());
                    break;

                case MidiMessage.MIM_ERROR:
                    break;
            }
        }

        private void OnShortMessage(int message, int milliseconds)
        {
            WriteBytesToStream(_channelMessageMemoryStream, message.GetThirdByte(), message.GetSecondByte());

            var statusByte = message.GetFourthByte();
            var eventReader = EventReaderFactory.GetReader(statusByte, smfOnly: false);
            var midiEvent = eventReader.Read(_channelEventReader, ReadingSettings, statusByte);

            OnEventReceived(midiEvent, milliseconds);

            if (RaiseMidiTimeCodeReceived)
            {
                var midiTimeCodeEvent = midiEvent as MidiTimeCodeEvent;
                if (midiTimeCodeEvent != null)
                    TryRaiseMidiTimeCodeReceived(midiTimeCodeEvent);
            }
        }

        private void OnSysExMessage(IntPtr sysExHeaderPointer, int milliseconds)
        {
            var header = (MidiWinApi.MIDIHDR)Marshal.PtrToStructure(sysExHeaderPointer, typeof(MidiWinApi.MIDIHDR));
            var data = new byte[header.dwBytesRecorded - 1];
            Marshal.Copy(IntPtr.Add(header.lpData, 1), data, 0, data.Length);

            var midiEvent = new NormalSysExEvent(data);
            OnEventReceived(midiEvent, milliseconds);

            UnprepareSysExBuffer(sysExHeaderPointer);
            PrepareSysExBuffer();
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

        private static MMRESULT GetErrorText(MMRESULT mmrError, StringBuilder pszText, uint cchText)
        {
            return MidiInWinApi.midiInGetErrorText(mmrError, pszText, cchText);
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

                _channelMessageMemoryStream.Dispose();
                _channelEventReader.Dispose();
            }

            UnprepareSysExBuffer(_sysExHeaderPointer);
            DestroyHandle();

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
