using System.Collections.Generic;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Interaction
{
    internal sealed class MetricTempoMapValuesCache : ITempoMapValuesCache
    {
        #region Nested classes

        internal sealed class AccumulatedMicroseconds
        {
            #region Constructor

            public AccumulatedMicroseconds(long time, double microseconds, double microsecondsPerTick)
            {
                Time = time;
                Microseconds = microseconds;
                MicrosecondsPerTick = microsecondsPerTick;
                TicksPerMicrosecond = 1.0 / microsecondsPerTick;
            }

            #endregion

            #region Properties

            public long Time { get; }

            public double Microseconds { get; }

            public double MicrosecondsPerTick { get; }

            public double TicksPerMicrosecond { get; }

            #endregion
        }

        #endregion

        #region Properties

        public AccumulatedMicroseconds[] Microseconds { get; private set; }

        public double DefaultMicrosecondsPerTick { get; private set; }

        public double DefaultTicksPerMicrosecond { get; private set; }

        #endregion

        #region Methods

        private static double GetMicroseconds(long time, Tempo tempo, short ticksPerQuarterNote)
        {
            return time * tempo.MicrosecondsPerQuarterNote / (double)ticksPerQuarterNote;
        }

        #endregion

        #region ITempoMapValuesCache

        public IEnumerable<TempoMapLine> InvalidateOnLines { get; } = new[] { TempoMapLine.Tempo };

        public void Invalidate(TempoMap tempoMap)
        {
            var microseconds = new List<AccumulatedMicroseconds>();

            var ticksPerQuarterNote = ((TicksPerQuarterNoteTimeDivision)tempoMap.TimeDivision).TicksPerQuarterNote;

            var accumulatedMicroseconds = 0d;
            var lastTime = 0L;
            var lastTempo = Tempo.Default;

            foreach (var tempoChange in tempoMap.TempoLine)
            {
                var tempoChangeTime = tempoChange.Time;

                accumulatedMicroseconds += GetMicroseconds(tempoChangeTime - lastTime, lastTempo, ticksPerQuarterNote);

                lastTempo = tempoChange.Value;
                lastTime = tempoChangeTime;

                microseconds.Add(new AccumulatedMicroseconds(tempoChangeTime, accumulatedMicroseconds, lastTempo.MicrosecondsPerQuarterNote / (double)ticksPerQuarterNote));
            }

            Microseconds = microseconds.ToArray();
            DefaultMicrosecondsPerTick = Tempo.Default.MicrosecondsPerQuarterNote / (double)ticksPerQuarterNote;
            DefaultTicksPerMicrosecond = 1.0 / DefaultMicrosecondsPerTick;
        }

        #endregion
    }
}
