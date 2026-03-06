using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Multimedia
{
    public sealed class InputDeviceSettings
    {
        #region Fields

        private SilentNoteOnPolicy _silentNoteOnPolicy = SilentNoteOnPolicy.NoteOn;
        private int _sysExBufferSize = 2048;
        private int _sysExBuffersCount = 5;

        #endregion

        #region Properties

        public SilentNoteOnPolicy SilentNoteOnPolicy
        {
            get { return _silentNoteOnPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _silentNoteOnPolicy = value;
            }
        }

        public int SysExBufferSize
        {
            get { return _sysExBufferSize; }
            set
            {
                ThrowIfArgument.IsNonpositive(nameof(value), value, "System-exclusive event buffer size is zero or negative.");
                _sysExBufferSize = value;
            }
        }

        public int SysExBuffersCount
        {
            get { return _sysExBuffersCount; }
            set
            {
                ThrowIfArgument.IsNonpositive(nameof(value), value, "System-exclusive event buffers count is zero or negative.");
                _sysExBuffersCount = value;
            }
        }

        #endregion
    }
}
