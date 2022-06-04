using Melanchall.DryWetMidi.Common;
using System;
using System.Runtime.Serialization;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// The exception that is thrown when count of track chunks in a MIDI file differs from
    /// the one declared in the header chunk of this file.
    /// </summary>
    /// <remarks>
    /// <para>Note that this exception will be thrown only if <see cref="ReadingSettings.UnexpectedTrackChunksCountPolicy"/>
    /// is set to <see cref="UnexpectedTrackChunksCountPolicy.Abort"/> for the <see cref="ReadingSettings"/>
    /// used for reading a MIDI file.</para>
    /// </remarks>
    [Serializable]
    public sealed class UnexpectedTrackChunksCountException : MidiException
    {
        #region Constructors

        internal UnexpectedTrackChunksCountException(int expectedCount, int actualCount)
            : base($"Count of track chunks is {actualCount} while {expectedCount} expected.")
        {
            ExpectedCount = expectedCount;
            ActualCount = actualCount;
        }

        private UnexpectedTrackChunksCountException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            ExpectedCount = info.GetInt32(nameof(ExpectedCount));
            ActualCount = info.GetInt32(nameof(ActualCount));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the expected count of track chunks read from the header chunk.
        /// </summary>
        public int ExpectedCount { get; }

        /// <summary>
        /// Gets the actual count of track chunks read from a MIDI file.
        /// </summary>
        public int ActualCount { get; }

        #endregion

        #region Overrides

        /// <summary>
        /// Sets the <see cref="SerializationInfo"/> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo"/> that holds the serialized object data
        /// about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext"/> that contains contextual information
        /// about the source or destination.</param>
        /// <exception cref="ArgumentNullException"><paramref name="info"/> is <c>null</c>.</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue(nameof(ExpectedCount), ExpectedCount);
            info.AddValue(nameof(ActualCount), ActualCount);
        }

        #endregion
    }
}
