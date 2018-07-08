using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Tools
{
    internal sealed class MinVelocityMerger : VelocityMerger
    {
        #region Overrides

        public override void Merge(SevenBitNumber velocity)
        {
            _velocity = (SevenBitNumber)Math.Min(_velocity, velocity);
        }

        #endregion
    }
}
