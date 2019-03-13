using System.Linq;

namespace Melanchall.DryWetMidi.Smf
{
    public abstract class MtcWithInfoSysExData : MtcSysExData
    {
        #region Properties

        public byte[] Info { get; set; }

        #endregion

        #region Overrides

        internal sealed override void Read(MidiReader reader, SysExDataReadingSettings settings)
        {
            base.Read(reader, settings);

            var info = reader.ReadAllBytes();
            if (info != null && info.Last() == SysExEvent.EndOfEventByte)
                info = info.Take(info.Length - 1).ToArray();

            Info = info;
        }

        #endregion
    }
}
