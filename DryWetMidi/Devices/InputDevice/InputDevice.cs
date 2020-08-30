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
    /// Represents an input MIDI device.
    /// </summary>
    public sealed class InputDevice : MidiDevice, IInputDevice
    {
        #region Constants

        private const int SysExBufferLength = 2048;
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

        /// <summary>
        /// Occurs when invalid system exclusive event is received.
        /// </summary>
        public event EventHandler<InvalidSysExEventReceivedEventArgs> InvalidSysExEventReceived;

        /// <summary>
        /// Occurs when invalid channel, system common or system real-time event received.
        /// </summary>
        public event EventHandler<InvalidShortEventReceivedEventArgs> InvalidShortEventReceived;

        #endregion

        #region Fields

        private readonly BytesToMidiEventConverter _bytesToMidiEventConverter = new BytesToMidiEventConverter(ChannelParametersBufferSize);

        private IntPtr _sysExHeaderPointer = IntPtr.Zero;

        private MidiWinApi.MidiMessageCallback _callback;

        private readonly Dictionary<MidiTimeCodeComponent, FourBitNumber> _midiTimeCodeComponents = new Dictionary<MidiTimeCodeComponent, FourBitNumber>();

        #endregion

        #region Constructor

        private InputDevice(int id)
            : base(id)
        {
            _bytesToMidiEventConverter.ReadingSettings.SilentNoteOnPolicy = SilentNoteOnPolicy.NoteOn;
            SetDeviceInformation();
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
        /// Gets or sets a value indicating if <see cref="MidiTimeCodeReceived"/> event should be raised or not.
        /// </summary>
        public bool RaiseMidiTimeCodeReceived { get; set; } = true;

        /// <summary>
        /// Gets a value that indicates whether <see cref="InputDevice"/> is currently listening for
        /// incoming MIDI events.
        /// </summary>
        public bool IsListeningForEvents { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Starts listening for incoming MIDI events on the current input device.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current <see cref="InputDevice"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public void StartEventsListening()
        {
            EnsureDeviceIsNotDisposed();
            EnsureHandleIsCreated();

            PrepareSysExBuffer();
            ProcessMmResult(MidiInWinApi.midiInStart(_handle));

            IsListeningForEvents = true;
        }

        /// <summary>
        /// Stops listening for incoming MIDI events on the current input device.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current <see cref="InputDevice"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public void StopEventsListening()
        {
            EnsureDeviceIsNotDisposed();

            if (_handle == IntPtr.Zero)
                return;

            ProcessMmResult(StopEventsListeningSilently());
        }

        /// <summary>
        /// Stops listening for incoming MIDI events on the current <see cref="InputDevice"/> and flushes
        /// all pending data.
        /// </summary>
        /// <exception cref="ObjectDisposedException">The current <see cref="InputDevice"/> is disposed.</exception>
        /// <exception cref="MidiDeviceException">An error occurred on device.</exception>
        public void Reset()
        {
            EnsureDeviceIsNotDisposed();

            if (_handle == IntPtr.Zero)
                return;

            ProcessMmResult(MidiInWinApi.midiInReset(_handle));
        }

        /// <summary>
        /// Retrieves the number of input MIDI devices presented in the system.
        /// </summary>
        /// <returns>Number of input MIDI devices presented in the system.</returns>
        public static int GetDevicesCount()
        {
            return (int)MidiInWinApi.midiInGetNumDevs();
        }

        /// <summary>
        /// Retrieves all input MIDI devices presented in the system.
        /// </summary>
        /// <returns>All input MIDI devices presented in the system.</returns>
        public static IEnumerable<InputDevice> GetAll()
        {
            var devicesCount = GetDevicesCount();
            for (var deviceId = 0; deviceId < devicesCount; deviceId++)
            {
                yield return new InputDevice(deviceId);
            }
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
        public static InputDevice GetByName(string name)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(name), name, "Device name");

            var device = GetAll().FirstOrDefault(d => d.Name == name);
            if (device == null)
                throw new ArgumentException($"There is no MIDI input device '{name}'.", nameof(name));

            return device;
        }

        /// <summary>
        /// Retrieves input MIDI device with the specified ID.
        /// </summary>
        /// <param name="id">Device ID which is number from 0 to <see cref="GetDevicesCount"/> minus 1.</param>
        /// <returns>Input MIDI device with the specified ID.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="id"/> is out of valid range.</exception>
        public static InputDevice GetById(int id)
        {
            ThrowIfArgument.IsOutOfRange(nameof(id), id, 0, GetDevicesCount() - 1, "Device ID is out of range.");

            return new InputDevice(id);
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
            return MidiInWinApi.midiInGetErrorText(mmrError, pszText, cchText);
        }

        private void OnEventReceived(MidiEvent midiEvent)
        {
            EventReceived?.Invoke(this, new MidiEventReceivedEventArgs(midiEvent));
        }

        private void OnMidiTimeCodeReceived(MidiTimeCodeType timeCodeType, int hours, int minutes, int seconds, int frames)
        {
            MidiTimeCodeReceived?.Invoke(this, new MidiTimeCodeReceivedEventArgs(timeCodeType, hours, minutes, seconds, frames));
        }

        private void OnInvalidSysExEventReceived(byte[] data)
        {
            InvalidSysExEventReceived?.Invoke(this, new InvalidSysExEventReceivedEventArgs(data));
        }

        private void OnInvalidShortEventReceived(byte statusByte, byte firstDataByte, byte secondDataByte)
        {
            InvalidShortEventReceived?.Invoke(this, new InvalidShortEventReceivedEventArgs(statusByte, firstDataByte, secondDataByte));
        }

        private void PrepareSysExBuffer()
        {
            var header = new MidiWinApi.MIDIHDR
            {
                lpData = Marshal.AllocHGlobal(SysExBufferLength),
                dwBufferLength = SysExBufferLength,
                dwBytesRecorded = SysExBufferLength
            };

            _sysExHeaderPointer = Marshal.AllocHGlobal(MidiWinApi.MidiHeaderSize);
            Marshal.StructureToPtr(header, _sysExHeaderPointer, false);

            ProcessMmResult(MidiInWinApi.midiInPrepareHeader(_handle, _sysExHeaderPointer, MidiWinApi.MidiHeaderSize));
            ProcessMmResult(MidiInWinApi.midiInAddBuffer(_handle, _sysExHeaderPointer, MidiWinApi.MidiHeaderSize));
        }

        private void UnprepareSysExBuffer(IntPtr headerPointer)
        {
            if (headerPointer == IntPtr.Zero)
                return;

            MidiInWinApi.midiInUnprepareHeader(_handle, headerPointer, MidiWinApi.MidiHeaderSize);

            var header = (MidiWinApi.MIDIHDR)Marshal.PtrToStructure(headerPointer, typeof(MidiWinApi.MIDIHDR));
            Marshal.FreeHGlobal(header.lpData);
            Marshal.FreeHGlobal(headerPointer);
        }

        private void EnsureHandleIsCreated()
        {
            if (_handle != IntPtr.Zero)
                return;

            _callback = OnMessage;
            ProcessMmResult(MidiInWinApi.midiInOpen(out _handle, Id, _callback, IntPtr.Zero, MidiWinApi.CallbackFunction));
        }

        private void DestroyHandle()
        {
            if (_handle == IntPtr.Zero)
                return;

            MidiInWinApi.midiInReset(_handle);
            MidiInWinApi.midiInClose(_handle);
        }

        private void SetDeviceInformation()
        {
            var caps = default(MidiInWinApi.MIDIINCAPS);
            ProcessMmResult(MidiInWinApi.midiInGetDevCaps(new IntPtr(Id), ref caps, (uint)Marshal.SizeOf(caps)));

            SetBasicDeviceInformation(caps.wMid, caps.wPid, caps.vDriverVersion, caps.szPname);
        }

        private void OnMessage(IntPtr hMidi, MidiMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2)
        {
            if (!IsListeningForEvents)
                return;

            switch (wMsg)
            {
                case MidiMessage.MIM_DATA:
                case MidiMessage.MIM_MOREDATA:
                    OnShortMessage(dwParam1.ToInt32());
                    break;

                case MidiMessage.MIM_LONGDATA:
                    OnSysExMessage(dwParam1);
                    break;

                case MidiMessage.MIM_ERROR:
                    byte statusByte, firstDataByte, secondDataByte;
                    MidiWinApi.UnpackShortEventBytes(dwParam1.ToInt32(), out statusByte, out firstDataByte, out secondDataByte);
                    OnInvalidShortEventReceived(statusByte, firstDataByte, secondDataByte);
                    break;

                case MidiMessage.MIM_LONGERROR:
                    OnInvalidSysExEventReceived(MidiWinApi.UnpackSysExBytes(dwParam1));
                    break;
            }
        }

        private void OnShortMessage(int message)
        {
            try
            {
                byte statusByte, firstDataByte, secondDataByte;
                MidiWinApi.UnpackShortEventBytes(message, out statusByte, out firstDataByte, out secondDataByte);

                var midiEvent = _bytesToMidiEventConverter.Convert(statusByte, new[] { firstDataByte, secondDataByte });
                OnEventReceived(midiEvent);

                if (RaiseMidiTimeCodeReceived)
                {
                    var midiTimeCodeEvent = midiEvent as MidiTimeCodeEvent;
                    if (midiTimeCodeEvent != null)
                        TryRaiseMidiTimeCodeReceived(midiTimeCodeEvent);
                }
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
                data = MidiWinApi.UnpackSysExBytes(sysExHeaderPointer);
                var midiEvent = new NormalSysExEvent(data);
                OnEventReceived(midiEvent);

                UnprepareSysExBuffer(sysExHeaderPointer);
                PrepareSysExBuffer();
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

        private uint StopEventsListeningSilently()
        {
            IsListeningForEvents = false;
            return MidiInWinApi.midiInStop(_handle);
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
                _bytesToMidiEventConverter.Dispose();
            }

            //

            if (_handle != IntPtr.Zero)
            {
                StopEventsListeningSilently();
                DestroyHandle();
            }

            UnprepareSysExBuffer(_sysExHeaderPointer);

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
