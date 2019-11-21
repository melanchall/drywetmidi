using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Musical objects that can be played.
    /// </summary>
    public interface IMusicalObject
    {
        #region Properties

        /// <summary>
        /// Gets the channel which should be used to play an object.
        /// </summary>
        FourBitNumber Channel { get; }

        #endregion
    }
}
