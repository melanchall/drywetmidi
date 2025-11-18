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
        /// <remarks>
        /// Channel is a zero-based number in DryWetMIDI, valid values are from <c>0</c> to <c>15</c>.
        /// Other libraries and software can use one-based channel numbers (i.e.from <c>1</c>
        /// to <c>16</c>) so be aware about that: channel <c>10</c> in such software will be <c>9</c>
        /// in DryWetMIDI.
        /// </remarks>
        FourBitNumber Channel { get; }

        #endregion
    }
}
