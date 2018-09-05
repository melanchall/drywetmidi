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

        private const int ParameterBufferSize = 2;
        private static readonly ReadingSettings ReadingSettings = new ReadingSettings();

        #endregion

        #region Fields

        private DateTime _startTime;

        private readonly MemoryStream _memoryStream = new MemoryStream(ParameterBufferSize);
        private readonly MidiReader _midiReader;
        private MidiWinApi.MidiMessageCallback _callback;

        #endregion

        #region Events

        public event EventHandler<MidiEventReceivedEventArgs> EventReceived;

        #endregion

        #region Constructor

        internal InputDevice(uint id)
            : base(id)
        {
            _midiReader = new MidiReader(_memoryStream);

            SetDeviceInformation();
        }

        #endregion

        #region Methods

        public void Start()
        {
            EnsureDeviceIsNotDisposed();
            EnsureHandleIsCreated();

            ProcessMmResult(() => MidiInWinApi.midiInStart(_handle));
            _startTime = DateTime.UtcNow;
        }

        public void Stop()
        {
            EnsureDeviceIsNotDisposed();

            if (_handle == IntPtr.Zero)
                return;

            ProcessMmResult(() => MidiInWinApi.midiInStop(_handle));
        }

        public void Reset()
        {
            EnsureDeviceIsNotDisposed();

            if (_handle == IntPtr.Zero)
                return;

            ProcessMmResult(() => MidiInWinApi.midiInReset(_handle));
            _startTime = DateTime.UtcNow;
        }

        public static int GetDevicesCount()
        {
            // TODO: process last error
            // TODO: uint instead of int
            return MidiInWinApi.midiInGetNumDevs();
        }

        public static IEnumerable<InputDevice> GetAll()
        {
            var devicesCount = GetDevicesCount();
            return Enumerable.Range(0, devicesCount).Select(i => new InputDevice((uint)i));
        }

        public static InputDevice GetByName(string name)
        {
            ThrowIfArgument.IsNullOrEmptyString(nameof(name), name, "Device name");

            return GetAll().FirstOrDefault(d => d.Name == name);
        }

        protected void OnEventReceived(MidiEvent midiEvent, int milliseconds)
        {
            EventReceived?.Invoke(this, new MidiEventReceivedEventArgs(midiEvent, _startTime.AddMilliseconds(milliseconds)));
        }

        private void EnsureHandleIsCreated()
        {
            if (_handle != IntPtr.Zero)
                return;

            _callback = OnMessage;
            ProcessMmResult(() => MidiInWinApi.midiInOpen(out _handle, _id, _callback, IntPtr.Zero, MidiWinApi.CallbackFunction));
        }

        private void DestroyHandle()
        {
            MidiInWinApi.midiInClose(_handle);
        }

        private void SetDeviceInformation()
        {
            var caps = default(MidiInWinApi.MIDIINCAPS);
            ProcessMmResult(() => MidiInWinApi.midiInGetDevCaps(new UIntPtr(_id), ref caps, (uint)Marshal.SizeOf(caps)));

            SetBasicDeviceInformation(caps.wMid, caps.wPid, caps.vDriverVersion, caps.szPname);
        }

        private void OnMessage(IntPtr hMidi, MidiMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2)
        {
            var parameter1 = dwParam1.ToInt32();
            var parameter2 = dwParam2.ToInt32();

            switch (wMsg)
            {
                case MidiMessage.MIM_DATA:
                    OnMessage(parameter1, parameter2);
                    break;

                case MidiMessage.MIM_ERROR:
                    break;
            }
        }

        private void OnMessage(int message, int milliseconds)
        {
            // TODO: move bit operations to DataTypesUtilities
            WriteBytesToStream(_memoryStream, (byte)((message & 0xFF00) >> 8), (byte)(message >> 16));

            var statusByte = (byte)(message & 0xFF);
            var eventReader = EventReaderFactory.GetReader(statusByte);
            var midiEvent = eventReader.Read(_midiReader, ReadingSettings, statusByte);

            OnEventReceived(midiEvent, milliseconds);
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
                _midiReader.Dispose();
            }

            _disposed = true;
        }

        internal override MMRESULT GetErrorText(MMRESULT mmrError, StringBuilder pszText, uint cchText)
        {
            return MidiInWinApi.midiInGetErrorText(mmrError, pszText, cchText);
        }

        #endregion
    }
}
