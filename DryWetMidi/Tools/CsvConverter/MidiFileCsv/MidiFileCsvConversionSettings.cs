using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Smf.Interaction;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class MidiFileCsvConversionSettings
    {
        #region Fields

        private MidiFileCsvLayout _csvLayout = MidiFileCsvLayout.DryWetMidi;
        private TimeSpanType _timeType = TimeSpanType.Midi;

        #endregion

        #region Properties

        public MidiFileCsvLayout CsvLayout
        {
            get { return _csvLayout; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _csvLayout = value;
            }
        }

        public TimeSpanType TimeType
        {
            get { return _timeType; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _timeType = value;
            }
        }

        #endregion
    }
}
