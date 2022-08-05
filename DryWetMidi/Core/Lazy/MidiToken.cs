namespace Melanchall.DryWetMidi.Core
{
    public abstract class MidiToken
    {
        #region Constructor

        protected MidiToken(MidiTokenType tokenType)
        {
            TokenType = tokenType;
        }

        #endregion

        #region Properties

        public MidiTokenType TokenType { get; }

        #endregion
    }
}
