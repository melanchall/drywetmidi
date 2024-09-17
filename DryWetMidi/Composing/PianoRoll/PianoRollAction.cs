using System;

namespace Melanchall.DryWetMidi.Composing
{
    public sealed class PianoRollAction
    {
        #region Constructors

        private PianoRollAction(
            char startSymbol,
            char? endSymbol,
            Action<PatternBuilder, PianoRollActionContext> action)
        {
            StartSymbol = startSymbol;
            EndSymbol = endSymbol;
            Action = action;
        }

        #endregion

        #region Properties

        internal char StartSymbol { get; }

        internal char? EndSymbol { get; }

        internal Action<PatternBuilder, PianoRollActionContext> Action { get; }

        #endregion

        #region Methods

        public static PianoRollAction CreateSingleCell(
            char symbol,
            Action<PatternBuilder, PianoRollActionContext> action)
        {
            return new PianoRollAction(symbol, null, action);
        }

        public static PianoRollAction CreateMultiCell(
            char startSymbol,
            char endSymbol,
            Action<PatternBuilder, PianoRollActionContext> action)
        {
            return new PianoRollAction(startSymbol, endSymbol, action);
        }

        #endregion
    }
}
