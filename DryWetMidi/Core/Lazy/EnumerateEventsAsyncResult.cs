using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Melanchall.DryWetMidi.Core
{
    /// <summary>
    /// Contains the asynchronous sequence returned by
    /// <see cref="MidiTokensReaderUtilities.EnumerateEventsAsync(MidiTokensReader)"/>.
    /// </summary>
    public sealed class EnumerateEventsAsyncResult
    {
        #region Nested classes

        private sealed class EventsSequence : IAsyncEnumerable<MidiEvent>
        {
            private readonly MidiTokensReader _reader;
            private readonly EnumerateEventsAsyncResult _result;
            private int _enumerationStarted;

            internal EventsSequence(
                MidiTokensReader reader,
                EnumerateEventsAsyncResult result)
            {
                _reader = reader;
                _result = result;
            }

            public IAsyncEnumerator<MidiEvent> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                if (Interlocked.Exchange(ref _enumerationStarted, 1) != 0)
                    throw new InvalidOperationException("The events sequence can only be enumerated once.");

                return ReadEventsAsync(cancellationToken).GetAsyncEnumerator();
            }

            private async IAsyncEnumerable<MidiEvent> ReadEventsAsync(
                [EnumeratorCancellation] CancellationToken cancellationToken)
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var token = await _reader.ReadTokenAsync(cancellationToken).ConfigureAwait(false);
                    if (token is MidiEventToken eventToken)
                    {
                        yield return eventToken.Event;
                        continue;
                    }

                    _result.NextToken = token;
                    _result.IsCompleted = true;
                    yield break;
                }
            }
        }

        #endregion

        #region Constructor

        internal EnumerateEventsAsyncResult(MidiTokensReader reader)
        {
            Events = new EventsSequence(reader, this);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a single-use asynchronous sequence of consecutive MIDI events.
        /// Disposing its enumerator does not dispose the caller-owned reader.
        /// </summary>
        /// <exception cref="InvalidOperationException">The sequence is enumerated more than once.</exception>
        public IAsyncEnumerable<MidiEvent> Events { get; }

        /// <summary>
        /// Gets the non-event token consumed after the events, or <c>null</c> if
        /// enumeration has not completed or completed at the end of the input.
        /// Process this token before requesting another token from the reader.
        /// </summary>
        public MidiToken? NextToken { get; internal set; }

        /// <summary>
        /// Gets a value indicating whether enumeration has reached the next non-event
        /// token or the end of the input. Early disposal, cancellation, and failure
        /// do not complete enumeration.
        /// </summary>
        public bool IsCompleted { get; internal set; }

        #endregion
    }
}
