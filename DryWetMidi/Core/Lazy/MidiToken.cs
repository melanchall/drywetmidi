namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Represents a single MIDI token from a MIDI file.
    /// </summary>
    /// <seealso cref="MidiTokensReader"/>
    public abstract class MidiToken
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiToken"/> with the
        /// specified token type.
        /// </summary>
        /// <param name="tokenType">The type of a MIDI token.</param>
        protected MidiToken(MidiTokenType tokenType)
        {
            TokenType = tokenType;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of a MIDI token.
        /// </summary>
        public MidiTokenType TokenType { get; }

        /// <summary>
        /// Gets the position of a MIDI token within an input data stream.
        /// </summary>
        public long Position { get; internal set; }

        /// <summary>
        /// Gets the length of a MIDI token in bytes within an input data stream.
        /// </summary>
        public long Length { get; internal set; }

        #endregion
    }
}
