using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Represents a tempo map of a MIDI file. More info in the <see href="xref:a_tempo_map">Tempo map</see> article.
    /// </summary>
    public sealed class TempoMap
    {
        #region Constants

        /// <summary>
        /// The default tempo map which uses 4/4 time signature and tempo of 500,000 microseconds per quarter note.
        /// </summary>
        public static readonly TempoMap Default = new TempoMap(new TicksPerQuarterNoteTimeDivision());

        #endregion

        #region Fields

        private ValueLine<TimeSignature> _timeSignatureLine;
        private ValueLine<Tempo> _tempoLine;

        private readonly List<ITempoMapValuesCache> _valuesCaches = new List<ITempoMapValuesCache>();

        private bool _isTempoMapReady = true;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TempoMap"/> with the specified time division
        /// of a MIDI file.
        /// </summary>
        /// <param name="timeDivision">MIDI file time division which specifies the meaning of the time
        /// used by events of the file.</param>
        /// <exception cref="ArgumentNullException"><paramref name="timeDivision"/> is <c>null</c>.</exception>
        internal TempoMap(TimeDivision timeDivision)
        {
            ThrowIfArgument.IsNull(nameof(timeDivision), timeDivision);

            TimeDivision = timeDivision;
            TempoLine = new ValueLine<Tempo>(Tempo.Default);
            TimeSignatureLine = new ValueLine<TimeSignature>(TimeSignature.Default);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the time division used by a tempo map.
        /// </summary>
        public TimeDivision TimeDivision { get; internal set; }

        internal ValueLine<TimeSignature> TimeSignatureLine
        {
            get { return _timeSignatureLine; }
            set
            {
                if (_timeSignatureLine != null)
                    _timeSignatureLine.ValuesChanged -= OnTimeSignatureChanged;

                _timeSignatureLine = value;
                _timeSignatureLine.ValuesChanged += OnTimeSignatureChanged;
            }
        }

        internal ValueLine<Tempo> TempoLine
        {
            get { return _tempoLine; }
            set
            {
                if (_tempoLine != null)
                    _tempoLine.ValuesChanged -= OnTempoChanged;

                _tempoLine = value;
                _tempoLine.ValuesChanged += OnTempoChanged;
            }
        }

        internal bool IsTempoMapReady
        {
            get { return _isTempoMapReady; }
            set
            {
                if (_isTempoMapReady == value)
                    return;

                _isTempoMapReady = value;
                
                if (_isTempoMapReady)
                {
                    InvalidateCaches(TempoMapLine.Tempo);
                    InvalidateCaches(TempoMapLine.TimeSignature);
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Returns the collection of tempo changes.
        /// </summary>
        /// <returns>Collection of tempo changes.</returns>
        public IEnumerable<ValueChange<Tempo>> GetTempoChanges()
        {
            return _tempoLine;
        }

        /// <summary>
        /// Returns tempo at the specified time.
        /// </summary>
        /// <param name="time">Time to get tempo at.</param>
        /// <returns>Tempo at the time of <paramref name="time"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time"/> is <c>null</c>.</exception>
        public Tempo GetTempoAtTime(ITimeSpan time)
        {
            ThrowIfArgument.IsNull(nameof(time), time);

            var convertedTime = TimeConverter.ConvertFrom(time, this);
            return TempoLine.GetValueAtTime(convertedTime);
        }

        /// <summary>
        /// Returns the collection of time signature changes.
        /// </summary>
        /// <returns>Collection of time signature changes.</returns>
        public IEnumerable<ValueChange<TimeSignature>> GetTimeSignatureChanges()
        {
            return _timeSignatureLine;
        }

        /// <summary>
        /// Returns time signature at the specified time.
        /// </summary>
        /// <param name="time">Time signature to get tempo at.</param>
        /// <returns>Time signature at the time of <paramref name="time"/>.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="time"/> is <c>null</c>.</exception>
        public TimeSignature GetTimeSignatureAtTime(ITimeSpan time)
        {
            ThrowIfArgument.IsNull(nameof(time), time);

            var convertedTime = TimeConverter.ConvertFrom(time, this);
            return TimeSignatureLine.GetValueAtTime(convertedTime);
        }

        /// <summary>
        /// Clones the current <see cref="TempoMap"/>.
        /// </summary>
        /// <returns>An instance of the <see cref="TempoMap"/> which is a clone of the current one.</returns>
        public TempoMap Clone()
        {
            var tempoMap = new TempoMap(TimeDivision.Clone());

            tempoMap.TempoLine.ReplaceValues(TempoLine);
            tempoMap.TimeSignatureLine.ReplaceValues(TimeSignatureLine);

            return tempoMap;
        }

        /// <summary>
        /// Creates an instance of the <see cref="TempoMap"/> with the specified tempo and
        /// time signature using default time division (96 ticks per quarter note).
        /// </summary>
        /// <param name="tempo">Tempo of the tempo map.</param>
        /// <param name="timeSignature">Time signature of the tempo map.</param>
        /// <returns><see cref="TempoMap"/> with the specified tempo and time signature.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="tempo"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="timeSignature"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static TempoMap Create(Tempo tempo, TimeSignature timeSignature)
        {
            ThrowIfArgument.IsNull(nameof(tempo), tempo);
            ThrowIfArgument.IsNull(nameof(timeSignature), timeSignature);

            var tempoMap = Default.Clone();
            SetGlobalTempo(tempoMap, tempo);
            SetGlobalTimeSignature(tempoMap, timeSignature);

            return tempoMap;
        }

        /// <summary>
        /// Creates an instance of the <see cref="TempoMap"/> with the specified tempo using
        /// default time division (96 ticks per quarter note).
        /// </summary>
        /// <param name="tempo">Tempo of the tempo map.</param>
        /// <returns><see cref="TempoMap"/> with the specified tempo.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="tempo"/> is <c>null</c>.</exception>
        public static TempoMap Create(Tempo tempo)
        {
            ThrowIfArgument.IsNull(nameof(tempo), tempo);

            var tempoMap = Default.Clone();
            SetGlobalTempo(tempoMap, tempo);

            return tempoMap;
        }

        /// <summary>
        /// Creates an instance of the <see cref="TempoMap"/> with the specified time signature using
        /// default time division (96 ticks per quarter note).
        /// </summary>
        /// <param name="timeSignature">Time signature of the tempo map.</param>
        /// <returns><see cref="TempoMap"/> with the specified time signature.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeSignature"/> is <c>null</c>.</exception>
        public static TempoMap Create(TimeSignature timeSignature)
        {
            ThrowIfArgument.IsNull(nameof(timeSignature), timeSignature);

            var tempoMap = Default.Clone();
            SetGlobalTimeSignature(tempoMap, timeSignature);

            return tempoMap;
        }

        /// <summary>
        /// Creates an instance of the <see cref="TempoMap"/> with the specified time division using
        /// default tempo (120 BPM) and default time signature (4/4).
        /// </summary>
        /// <param name="timeDivision">Time division of the tempo map.</param>
        /// <returns><see cref="TempoMap"/> with the specified time division.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="timeDivision"/> is <c>null</c>.</exception>
        public static TempoMap Create(TimeDivision timeDivision)
        {
            ThrowIfArgument.IsNull(nameof(timeDivision), timeDivision);

            return new TempoMap(timeDivision);
        }

        /// <summary>
        /// Creates an instance of the <see cref="TempoMap"/> with the specified time division and
        /// tempo using default time signature (4/4).
        /// </summary>
        /// <param name="timeDivision">Time division of the tempo map.</param>
        /// <param name="tempo">Tempo of the tempo map.</param>
        /// <returns><see cref="TempoMap"/> with the specified time division and tempo.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="timeDivision"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempo"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static TempoMap Create(TimeDivision timeDivision, Tempo tempo)
        {
            ThrowIfArgument.IsNull(nameof(timeDivision), timeDivision);
            ThrowIfArgument.IsNull(nameof(tempo), tempo);

            var tempoMap = new TempoMap(timeDivision);
            SetGlobalTempo(tempoMap, tempo);

            return tempoMap;
        }

        /// <summary>
        /// Creates an instance of the <see cref="TempoMap"/> with the specified time division and
        /// time signature using default tempo (120 BPM).
        /// </summary>
        /// <param name="timeDivision">Time division of the tempo map.</param>
        /// <param name="timeSignature">Time signature of the tempo map.</param>
        /// <returns><see cref="TempoMap"/> with the specified time division and time signature.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="timeDivision"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="timeSignature"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static TempoMap Create(TimeDivision timeDivision, TimeSignature timeSignature)
        {
            ThrowIfArgument.IsNull(nameof(timeDivision), timeDivision);
            ThrowIfArgument.IsNull(nameof(timeSignature), timeSignature);

            var tempoMap = new TempoMap(timeDivision);
            SetGlobalTimeSignature(tempoMap, timeSignature);

            return tempoMap;
        }

        /// <summary>
        /// Creates an instance of the <see cref="TempoMap"/> with the specified time division,
        /// tempo and time signature.
        /// </summary>
        /// <param name="timeDivision">Time division of the tempo map.</param>
        /// <param name="tempo">Tempo of the tempo map.</param>
        /// <param name="timeSignature">Time signature of the tempo map.</param>
        /// <returns><see cref="TempoMap"/> with the specified time division, tempo and time signature.</returns>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="timeDivision"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="tempo"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="timeSignature"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        public static TempoMap Create(TimeDivision timeDivision, Tempo tempo, TimeSignature timeSignature)
        {
            ThrowIfArgument.IsNull(nameof(timeDivision), timeDivision);
            ThrowIfArgument.IsNull(nameof(tempo), tempo);
            ThrowIfArgument.IsNull(nameof(timeSignature), timeSignature);

            var tempoMap = new TempoMap(timeDivision);
            SetGlobalTempo(tempoMap, tempo);
            SetGlobalTimeSignature(tempoMap, timeSignature);

            return tempoMap;
        }

        internal TempoMap Flip(long centerTime)
        {
            return new TempoMap(TimeDivision)
            {
                TempoLine = TempoLine.Reverse(centerTime),
                TimeSignatureLine = TimeSignatureLine.Reverse(centerTime)
            };
        }

        internal TCache GetValuesCache<TCache>() where TCache : ITempoMapValuesCache, new()
        {
            var result = _valuesCaches.OfType<TCache>().FirstOrDefault();
            if (result == null)
            {
                _valuesCaches.Add(result = new TCache());
                result.Invalidate(this);
            }

            return result;
        }

        private static void SetGlobalTempo(TempoMap tempoMap, Tempo tempo)
        {
            tempoMap.TempoLine.SetValue(0, tempo);
        }

        private static void SetGlobalTimeSignature(TempoMap tempoMap, TimeSignature timeSignature)
        {
            tempoMap.TimeSignatureLine.SetValue(0, timeSignature);
        }

        private void InvalidateCaches(TempoMapLine tempoMapLine)
        {
            if (!IsTempoMapReady)
                return;

            foreach (var valuesCache in _valuesCaches.Where(c => c.InvalidateOnLines?.Contains(tempoMapLine) == true))
            {
                valuesCache.Invalidate(this);
            }
        }

        private void OnTimeSignatureChanged(object sender, EventArgs args)
        {
            InvalidateCaches(TempoMapLine.TimeSignature);
        }

        private void OnTempoChanged(object sender, EventArgs args)
        {
            InvalidateCaches(TempoMapLine.Tempo);
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            var tempoMap = obj as TempoMap;
            if (ReferenceEquals(tempoMap, null))
                return false;

            return
                TimeDivision.Equals(tempoMap.TimeDivision) &&
                GetTempoChanges().SequenceEqual(tempoMap.GetTempoChanges()) &&
                GetTimeSignatureChanges().SequenceEqual(tempoMap.GetTimeSignatureChanges());
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = TimeDivision.GetHashCode();
                result = result * 23 + _tempoLine.ValueChangesCount.GetHashCode();
                result = result * 23 + _timeSignatureLine.ValueChangesCount.GetHashCode();
                return result;
            }
        }

        #endregion
    }
}
