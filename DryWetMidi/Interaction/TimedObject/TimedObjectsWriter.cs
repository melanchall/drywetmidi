using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Provides a wrapper around <see cref="MidiTokensWriter"/> to write objects to it keeping
    /// low memory consumption. See
    /// <see href="xref:a_file_lazy_reading_writing">Lazy reading/writing</see> article to learn more.
    /// </summary>
    public sealed class TimedObjectsWriter : IDisposable
    {
        #region Fields

        private readonly MidiTokensWriter _tokensWriter;
        private readonly LinkedList<TimedEvent> _timedEvents = new LinkedList<TimedEvent>();

        private bool _disposed;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TimedObjectsWriter"/> with the specified
        /// instance of <see cref="MidiTokensWriter"/>.
        /// </summary>
        /// <param name="tokensWriter"><see cref="MidiTokensWriter"/> that will be used to write objects.</param>
        /// <exception cref="ArgumentNullException"><paramref name="tokensWriter"/> is <c>null</c>.</exception>
        public TimedObjectsWriter(MidiTokensWriter tokensWriter)
        {
            ThrowIfArgument.IsNull(nameof(tokensWriter), tokensWriter);

            _tokensWriter = tokensWriter;
            _tokensWriter.BeforeEndTrackChunk = Flush;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Writes the specified object with the underlying <see cref="MidiTokensWriter"/>.
        /// </summary>
        /// <remarks>
        /// Objects passed to each next call of the method must be ordered in non-descending
        /// order by their times.
        /// </remarks>
        /// <param name="timedObject">Object to write.</param>
        /// <exception cref="InvalidOperationException">Object's time is less than the current
        /// time of the underlying writer.</exception>
        /// <exception cref="IOException">An I/O error occurred while writing the object.</exception>
        /// <exception cref="InvalidOperationException">A track chunk is not started
        /// (see <see cref="MidiTokensWriter.StartTrackChunk"/>).</exception>
        public void WriteObject(ITimedObject timedObject)
        {
            ThrowIfArgument.IsNull(nameof(timedObject), timedObject);

            var time = timedObject.Time;
            if (time < _tokensWriter.CurrentTime)
                throw new InvalidOperationException("Object's time is less than the current time of the underlying writer.");

            WritePreviousTimedEvents(time);

            var objectTimedEvents = GetObjectTimedEvents(timedObject);

            int remainingEventsStartIndex;
            WriteTimedEventsAtTime(objectTimedEvents, time, out remainingEventsStartIndex);
            if (remainingEventsStartIndex >= objectTimedEvents.Length)
                return;

            SaveTimedEvents(objectTimedEvents, remainingEventsStartIndex);
        }

        /// <summary>
        /// Writes the specified objects with the underlying <see cref="MidiTokensWriter"/>.
        /// </summary>
        /// <remarks>
        /// Objects passed to each next call of the method must be ordered in non-descending
        /// order by their times.
        /// </remarks>
        /// <param name="timedObjects">Objects to write.</param>
        /// <exception cref="InvalidOperationException">An object's time is less than the current
        /// time of the underlying writer.</exception>
        /// <exception cref="IOException">An I/O error occurred while writing the object.</exception>
        /// <exception cref="InvalidOperationException">A track chunk is not started
        /// (see <see cref="MidiTokensWriter.StartTrackChunk"/>).</exception>
        public void WriteObjects(IEnumerable<ITimedObject> timedObjects)
        {
            ThrowIfArgument.IsNull(nameof(timedObjects), timedObjects);

            foreach (var obj in timedObjects)
            {
                WriteObject(obj);
            }
        }

        private void WritePreviousTimedEvents(long time)
        {
            var node = _timedEvents.First;
            while (node != null && node.Value.Time <= time)
            {
                WriteTimedEvent(node.Value);

                var next = node.Next;
                _timedEvents.Remove(node);
                node = next;
            }
        }

        private void WriteTimedEventsAtTime(TimedEvent[] timedEvents, long time, out int remainingEventsStartIndex)
        {
            var i = 0;

            for (; i < timedEvents.Length; i++)
            {
                var timedEvent = timedEvents[i];
                if (timedEvent.Time > time)
                    break;

                WriteTimedEvent(timedEvent);
            }

            remainingEventsStartIndex = i;
        }

        private void SaveTimedEvents(TimedEvent[] timedEvents, int index)
        {
            if (!_timedEvents.Any())
            {
                SaveTimedEventsLast(timedEvents, ref index);
                return;
            }

            //

            for (var node = _timedEvents.First; node != null && index < timedEvents.Length; node = node.Next)
            {
                while (index < timedEvents.Length && timedEvents[index].Time < node.Value.Time)
                {
                    _timedEvents.AddBefore(node, timedEvents[index++]);
                }
            }

            //

            SaveTimedEventsLast(timedEvents, ref index);
        }

        private void SaveTimedEventsLast(TimedEvent[] timedEvents, ref int index)
        {
            for (; index < timedEvents.Length; index++)
            {
                _timedEvents.AddLast(timedEvents[index]);
            }
        }

        private void WriteTimedEvent(TimedEvent timedEvent)
        {
            var midiEvent = timedEvent.Event;
            midiEvent.DeltaTime = timedEvent.Time - _tokensWriter.CurrentTime;
            _tokensWriter.WriteEvent(midiEvent);
        }

        private TimedEvent[] GetObjectTimedEvents(ITimedObject timedObject)
        {
            var timedEvent = timedObject as TimedEvent;
            if (timedEvent != null)
                return new[] { timedEvent };

            var note = timedObject as Note;
            if (note != null)
                return new[]
                {
                    note.GetTimedNoteOnEvent(),
                    note.GetTimedNoteOffEvent()
                };

            var chord = timedObject as Chord;
            if (chord != null)
                return chord.Notes.SelectMany(GetObjectTimedEvents).OrderBy(obj => obj.Time).ToArray();

            var registeredParameter = timedObject as RegisteredParameter;
            if (registeredParameter != null)
                return registeredParameter.GetTimedEvents().ToArray();

            return new TimedEvent[0];
        }

        private void Flush()
        {
            foreach (var timedEvent in _timedEvents)
            {
                WriteTimedEvent(timedEvent);
            }

            _timedEvents.Clear();
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Releases all resources used by the current <see cref="TimedObjectsWriter"/>
        /// and also flushes all remaining data to the underlying stream.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Flush();
            }

            _disposed = true;
        }

        #endregion
    }
}
