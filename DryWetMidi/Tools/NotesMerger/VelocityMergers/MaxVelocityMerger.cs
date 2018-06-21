using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Tools
{
    internal sealed class MaxVelocityMerger : VelocityMerger
    {
        #region Overrides

        public override void Merge(SevenBitNumber velocity)
        {
            _velocity = (SevenBitNumber)Math.Max(_velocity, velocity);
        }

        #endregion
    }
}
