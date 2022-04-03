using Melanchall.DryWetMidi.Common;
using System;

namespace Melanchall.DryWetMidi.Composing
{
    public sealed partial class PatternBuilder
    {
        #region Methods

        /// <summary>
        /// Places the specified anchor at the current time.
        /// </summary>
        /// <param name="anchor">Anchor to place.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="anchor"/> is <c>null</c>.</exception>
        public PatternBuilder Anchor(object anchor)
        {
            ThrowIfArgument.IsNull(nameof(anchor), anchor);

            return AddAction(new AddAnchorAction(anchor));
        }

        /// <summary>
        /// Places an anchor at the current time.
        /// </summary>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        public PatternBuilder Anchor()
        {
            return AddAction(new AddAnchorAction());
        }

        /// <summary>
        /// Moves to the first specified anchor.
        /// </summary>
        /// <param name="anchor">Anchor to move to.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="anchor"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">There are no anchors with the <paramref name="anchor"/> key.</exception>
        public PatternBuilder MoveToFirstAnchor(object anchor)
        {
            ThrowIfArgument.IsNull(nameof(anchor), anchor);

            var counter = GetAnchorCounter(anchor);
            if (counter < 1)
                throw new ArgumentException($"There are no anchors with the '{anchor}' key.", nameof(anchor));

            return AddAction(new MoveToAnchorAction(anchor, AnchorPosition.First));
        }

        /// <summary>
        /// Move to the first anchor.
        /// </summary>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="InvalidOperationException">There are no anchors.</exception>
        public PatternBuilder MoveToFirstAnchor()
        {
            var counter = GetAnchorCounter(null);
            if (counter < 1)
                throw new InvalidOperationException("There are no anchors.");

            return AddAction(new MoveToAnchorAction(AnchorPosition.First));
        }

        /// <summary>
        /// Moves to the last specified anchor.
        /// </summary>
        /// <param name="anchor">Anchor to move to.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="anchor"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">There are no anchors with the <paramref name="anchor"/> key.</exception>
        public PatternBuilder MoveToLastAnchor(object anchor)
        {
            ThrowIfArgument.IsNull(nameof(anchor), anchor);

            var counter = GetAnchorCounter(anchor);
            if (counter < 1)
                throw new ArgumentException($"There are no anchors with the '{anchor}' key.", nameof(anchor));

            return AddAction(new MoveToAnchorAction(anchor, AnchorPosition.Last));
        }

        /// <summary>
        /// Moves to the last anchor.
        /// </summary>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="InvalidOperationException">The are no anchors.</exception>
        public PatternBuilder MoveToLastAnchor()
        {
            var counter = GetAnchorCounter(null);
            if (counter < 1)
                throw new InvalidOperationException("There are no anchors.");

            return AddAction(new MoveToAnchorAction(AnchorPosition.Last));
        }

        /// <summary>
        /// Moves to the nth specified anchor.
        /// </summary>
        /// <param name="anchor">Anchor to move to.</param>
        /// <param name="index">Index of an anchor to move to.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="anchor"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of range.</exception>
        public PatternBuilder MoveToNthAnchor(object anchor, int index)
        {
            ThrowIfArgument.IsNull(nameof(anchor), anchor);

            var counter = GetAnchorCounter(anchor);
            ThrowIfArgument.IsOutOfRange(nameof(index),
                                         index,
                                         0,
                                         counter - 1,
                                         "Index is out of range.");

            return AddAction(new MoveToAnchorAction(anchor, AnchorPosition.Nth, index));
        }

        /// <summary>
        /// Moves to the nth anchor.
        /// </summary>
        /// <param name="index">Index of an anchor to move to.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is out of range.</exception>
        public PatternBuilder MoveToNthAnchor(int index)
        {
            var counter = GetAnchorCounter(null);
            ThrowIfArgument.IsOutOfRange(nameof(index),
                                         index,
                                         0,
                                         counter - 1,
                                         "Index is out of range.");

            return AddAction(new MoveToAnchorAction(AnchorPosition.Nth, index));
        }

        #endregion
    }
}
