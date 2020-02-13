using System.Collections.Generic;

namespace Melanchall.DryWetMidi.Core
{
    public sealed class MidiFileEqualityComparer : IEqualityComparer<MidiFile>
    {
        #region Fields

        private readonly MidiFileEqualityCheckSettings _settings;

        #endregion

        #region Constructor

        public MidiFileEqualityComparer()
            : this(null)
        {
        }

        public MidiFileEqualityComparer(MidiFileEqualityCheckSettings settings)
        {
            _settings = settings ?? new MidiFileEqualityCheckSettings();
        }

        #endregion

        #region IEqualityComparer<MidiFile>

        public bool Equals(MidiFile x, MidiFile y)
        {
            string message;
            return MidiFile.Equals(x, y, _settings, out message);
        }

        public int GetHashCode(MidiFile obj)
        {
            return obj?.Chunks.Count.GetHashCode() ?? 0;
        }

        #endregion
    }
}
