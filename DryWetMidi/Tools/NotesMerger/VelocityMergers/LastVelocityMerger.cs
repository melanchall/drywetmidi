using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Tools
{
    internal sealed class LastVelocityMerger : VelocityMerger
    {
        #region Overrides

        public override void Merge(SevenBitNumber velocity)
        {
            _velocity = velocity;
        }

        #endregion
    }
}
