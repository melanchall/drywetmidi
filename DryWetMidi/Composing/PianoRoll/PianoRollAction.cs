using System;

namespace Melanchall.DryWetMidi.Composing
{
    /// <summary>
    /// Provides a custom action for piano roll. More info in the
    /// <see href="xref:a_composing_pattern#customization">Pattern: Piano roll: Customization</see> article.
    /// </summary>
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

        /// <summary>
        /// Creates a single-cell action.
        /// </summary>
        /// <param name="symbol">Symbol to trigger the action.</param>
        /// <param name="action">Action to execute on the <paramref name="symbol"/>.</param>
        /// <returns>An instance of the <see cref="PianoRollAction"/> that holds information
        /// of what to do when the <paramref name="symbol"/> is encountered.</returns>
        public static PianoRollAction CreateSingleCell(
            char symbol,
            Action<PatternBuilder, PianoRollActionContext> action)
        {
            return new PianoRollAction(symbol, null, action);
        }

        /// <summary>
        /// Creates a multi-cell action.
        /// </summary>
        /// <param name="startSymbol">Symbol to trigger the action. Cell time of the symbol
        /// defines the start time of the action.</param>
        /// <param name="endSymbol">Symbol to finish the action, so cell time of the symbol
        /// defines the end time of the action.</param>
        /// <param name="action">Action to execute.</param>
        /// <returns>An instance of the <see cref="PianoRollAction"/> that holds information
        /// of what to do when cells span between the <paramref name="startSymbol"/> and
        /// <paramref name="endSymbol"/> is encountered.</returns>
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
