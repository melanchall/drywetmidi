using System.IO;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Smf
{
    public static class SysExDataUtilities
    {
        #region Methods

        public static SysExData ReadSysExData(this SysExEvent sysExEvent, SysExDataReadingSettings settings = null)
        {
            ThrowIfArgument.IsNull(nameof(sysExEvent), sysExEvent);

            settings = settings ?? new SysExDataReadingSettings();

            var data = sysExEvent.Data;
            var sysExDataReader = SysExDataReaderFactory.GetReader(data);
            if (sysExDataReader == null)
                return new UnknownSysExData(data);

            using (var memoryStream = new MemoryStream(data))
            using (var midiReader = new MidiReader(memoryStream))
            {
                return sysExDataReader.Read(midiReader, settings);
            }
        }

        #endregion
    }
}
