using System;

namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public sealed class TempoMap
    {
        #region Constructor

        public TempoMap(TimeDivision timeDivision)
        {
            if (timeDivision == null)
                throw new ArgumentNullException(nameof(timeDivision));

            TimeDivision = timeDivision;
            TimeSignatureLine = new ValueLine<TimeSignature>(TimeSignature.Default);
            TempoLine = new ValueLine<Tempo>(Tempo.Default);
        }

        #endregion

        #region Properties

        public TimeDivision TimeDivision { get; }

        public ValueLine<TimeSignature> TimeSignatureLine { get; }

        public ValueLine<Tempo> TempoLine { get; }

        #endregion
    }
}
