using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;

namespace Melanchall.DryWetMidi.Composing
{
    public sealed partial class PatternBuilder
    {
        #region Methods

        /// <summary>
        /// Adds lyrics.
        /// </summary>
        /// <param name="text">Text of lyrics.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is <c>null</c>.</exception>
        public PatternBuilder Lyrics(string text)
        {
            ThrowIfArgument.IsNull(nameof(text), text);

            return AddAction(new AddTextEventAction<LyricEvent>(text));
        }

        /// <summary>
        /// Adds a marker.
        /// </summary>
        /// <param name="marker">The text of marker.</param>
        /// <returns>The current <see cref="PatternBuilder"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="marker"/> is <c>null</c>.</exception>
        public PatternBuilder Marker(string marker)
        {
            ThrowIfArgument.IsNull(nameof(marker), marker);

            return AddAction(new AddTextEventAction<MarkerEvent>(marker));
        }

        #endregion
    }
}
