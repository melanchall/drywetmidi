using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Tools
{
    public sealed class NotesMergingSettings
    {
        #region Fields

        private VelocityMergingPolicy _velocityMergingPolicy = VelocityMergingPolicy.First;
        private VelocityMergingPolicy _offVelocityMergingPolicy = VelocityMergingPolicy.Last;

        #endregion

        #region Properties

        public VelocityMergingPolicy VelocityMergingPolicy
        {
            get { return _velocityMergingPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _velocityMergingPolicy = value;
            }
        }

        public VelocityMergingPolicy OffVelocityMergingPolicy
        {
            get { return _offVelocityMergingPolicy; }
            set
            {
                ThrowIfArgument.IsInvalidEnumValue(nameof(value), value);

                _offVelocityMergingPolicy = value;
            }
        }

        #endregion
    }
}
