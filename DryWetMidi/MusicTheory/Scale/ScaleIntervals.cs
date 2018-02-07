using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.MusicTheory
{
    public static class ScaleIntervals
    {
        #region Constants

        // aeolian
        public static readonly IEnumerable<Interval> Aeolian = GetIntervals(2, 1, 2, 2, 1, 2, 2);

        // altered
        public static readonly IEnumerable<Interval> Altered = GetIntervals(1, 2, 1, 2, 2, 2, 2);

        // arabian
        public static readonly IEnumerable<Interval> Arabian = GetIntervals(2, 2, 1, 1, 2, 2, 2);

        // augmented
        public static readonly IEnumerable<Interval> Augmented = GetIntervals(3, 1, 3, 1, 3, 1);

        // augmented heptatonic
        public static readonly IEnumerable<Interval> AugmentedHeptatonic = GetIntervals(3, 1, 1, 2, 1, 3, 1);

        // balinese
        public static readonly IEnumerable<Interval> Balinese = GetIntervals(1, 2, 2, 2, 1, 3, 1);

        // bebop
        public static readonly IEnumerable<Interval> Bebop = GetIntervals(2, 2, 1, 2, 2, 1, 1, 1);

        // bebop dominant
        public static readonly IEnumerable<Interval> BebopDominant = GetIntervals(2, 2, 1, 2, 2, 1, 1, 1);

        // bebop locrian
        public static readonly IEnumerable<Interval> BebopLocrian = GetIntervals(1, 2, 2, 1, 1, 1, 2, 2);

        // bebop major
        public static readonly IEnumerable<Interval> BebopMajor = GetIntervals(2, 2, 1, 2, 1, 1, 2, 1);

        // bebop minor
        public static readonly IEnumerable<Interval> BebopMinor = GetIntervals(2, 1, 1, 1, 2, 2, 1, 2);

        // blues
        public static readonly IEnumerable<Interval> Blues = GetIntervals(3, 2, 1, 1, 3, 2);

        // chinese
        public static readonly IEnumerable<Interval> Chinese = GetIntervals(4, 2, 1, 4, 1);

        // chromatic
        public static readonly IEnumerable<Interval> Chromatic = GetIntervals(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);

        // composite blues
        public static readonly IEnumerable<Interval> CompositeBlues = GetIntervals(2, 1, 1, 1, 1, 1, 2, 1, 2);

        // diminished
        public static readonly IEnumerable<Interval> Diminished = GetIntervals(2, 1, 2, 1, 2, 1, 2, 1);

        // diminished whole tone
        public static readonly IEnumerable<Interval> DiminishedWholeTone = GetIntervals(1, 2, 1, 2, 2, 2, 2);

        // dominant
        public static readonly IEnumerable<Interval> Dominant = GetIntervals(2, 2, 1, 2, 2, 1, 2);

        // dorian
        public static readonly IEnumerable<Interval> Dorian = GetIntervals(2, 1, 2, 2, 2, 1, 2);

        // dorian #4
        public static readonly IEnumerable<Interval> Dorian4 = GetIntervals(2, 1, 3, 1, 2, 1, 2);

        // dorian b2
        public static readonly IEnumerable<Interval> DorianB2 = GetIntervals(1, 2, 2, 2, 2, 2, 1);

        // double harmonic lydian
        public static readonly IEnumerable<Interval> DoubleHarmonicLydian = GetIntervals(1, 3, 2, 1, 1, 3, 1);

        // double harmonic major
        public static readonly IEnumerable<Interval> DoubleHarmonicMajor = GetIntervals(1, 3, 1, 2, 1, 3, 1);

        // egyptian
        public static readonly IEnumerable<Interval> Egyptian = GetIntervals(2, 3, 2, 3, 2);

        // enigmatic
        public static readonly IEnumerable<Interval> Enigmatic = GetIntervals(1, 3, 2, 2, 2, 1, 1);

        // flamenco
        public static readonly IEnumerable<Interval> Flamenco = GetIntervals(1, 2, 1, 2, 1, 3, 2);

        // flat six pentatonic
        public static readonly IEnumerable<Interval> FlatSixPentatonic = GetIntervals(2, 2, 3, 1, 4);

        // flat three pentatonic
        public static readonly IEnumerable<Interval> FlatThreePentatonic = GetIntervals(2, 1, 4, 2, 3);

        // gypsy
        public static readonly IEnumerable<Interval> Gypsy = GetIntervals(1, 3, 1, 2, 1, 3, 1);

        // harmonic major
        public static readonly IEnumerable<Interval> HarmonicMajor = GetIntervals(2, 2, 1, 2, 1, 3, 1);

        // harmonic minor
        public static readonly IEnumerable<Interval> HarmonicMinor = GetIntervals(2, 1, 2, 2, 1, 3, 1);

        // hindu
        public static readonly IEnumerable<Interval> Hindu = GetIntervals(2, 2, 1, 2, 1, 2, 2);

        // hirajoshi
        public static readonly IEnumerable<Interval> Hirajoshi = GetIntervals(2, 1, 4, 1, 4);

        // hungarian major
        public static readonly IEnumerable<Interval> HungarianMajor = GetIntervals(3, 1, 2, 1, 2, 1, 2);

        // hungarian minor
        public static readonly IEnumerable<Interval> HungarianMinor = GetIntervals(2, 1, 3, 1, 1, 3, 1);

        // ichikosucho
        public static readonly IEnumerable<Interval> Ichikosucho = GetIntervals(2, 2, 1, 1, 1, 2, 2, 1);

        // in-sen
        public static readonly IEnumerable<Interval> InSen = GetIntervals(1, 4, 2, 3, 2);

        // indian
        public static readonly IEnumerable<Interval> Indian = GetIntervals(4, 1, 2, 3, 2);

        // ionian
        public static readonly IEnumerable<Interval> Ionian = GetIntervals(2, 2, 1, 2, 2, 2, 1);

        // ionian augmented
        public static readonly IEnumerable<Interval> IonianAugmented = GetIntervals(2, 2, 1, 3, 1, 2, 1);

        // ionian pentatonic
        public static readonly IEnumerable<Interval> IonianPentatonic = GetIntervals(4, 1, 2, 4, 1);

        // iwato
        public static readonly IEnumerable<Interval> Iwato = GetIntervals(1, 4, 1, 4, 2);

        // kafi raga
        public static readonly IEnumerable<Interval> KafiRaga = GetIntervals(3, 1, 1, 2, 2, 1, 1, 1);

        // kumoi
        public static readonly IEnumerable<Interval> Kumoi = GetIntervals(2, 1, 4, 2, 3);

        // kumoijoshi
        public static readonly IEnumerable<Interval> Kumoijoshi = GetIntervals(1, 4, 2, 1, 4);

        // leading whole tone
        public static readonly IEnumerable<Interval> LeadingWholeTone = GetIntervals(2, 2, 2, 2, 2, 1, 1);

        // locrian
        public static readonly IEnumerable<Interval> Locrian = GetIntervals(1, 2, 2, 1, 2, 2, 2);

        // locrian #2
        public static readonly IEnumerable<Interval> Locrian2 = GetIntervals(2, 1, 2, 1, 2, 2, 2);

        // locrian major
        public static readonly IEnumerable<Interval> LocrianMajor = GetIntervals(2, 2, 1, 1, 2, 2, 2);

        // locrian pentatonic
        public static readonly IEnumerable<Interval> LocrianPentatonic = GetIntervals(3, 2, 1, 4, 2);

        // lydian
        public static readonly IEnumerable<Interval> Lydian = GetIntervals(2, 2, 2, 1, 2, 2, 1);

        // lydian #5P pentatonic
        public static readonly IEnumerable<Interval> Lydian5PPentatonic = GetIntervals(4, 2, 2, 3, 1);

        // lydian #9
        public static readonly IEnumerable<Interval> Lydian9 = GetIntervals(1, 3, 2, 1, 2, 2, 1);

        // lydian augmented
        public static readonly IEnumerable<Interval> LydianAugmented = GetIntervals(2, 2, 2, 2, 1, 2, 1);

        // lydian b7
        public static readonly IEnumerable<Interval> LydianB7 = GetIntervals(2, 2, 2, 1, 2, 1, 2);

        // lydian diminished
        public static readonly IEnumerable<Interval> LydianDiminished = GetIntervals(2, 1, 3, 1, 2, 2, 1);

        // lydian dominant
        public static readonly IEnumerable<Interval> LydianDominant = GetIntervals(2, 2, 2, 1, 2, 1, 2);

        // lydian dominant pentatonic
        public static readonly IEnumerable<Interval> LydianDominantPentatonic = GetIntervals(4, 2, 1, 3, 2);

        // lydian minor
        public static readonly IEnumerable<Interval> LydianMinor = GetIntervals(2, 2, 2, 1, 1, 2, 2);

        // lydian pentatonic
        public static readonly IEnumerable<Interval> LydianPentatonic = GetIntervals(4, 2, 1, 4, 1);

        // major
        public static readonly IEnumerable<Interval> Major = GetIntervals(2, 2, 1, 2, 2, 2, 1);

        // major blues
        public static readonly IEnumerable<Interval> MajorBlues = GetIntervals(2, 1, 1, 3, 2, 3);

        // major flat two pentatonic
        public static readonly IEnumerable<Interval> MajorFlatTwoPentatonic = GetIntervals(1, 3, 3, 2, 3);

        // major pentatonic
        public static readonly IEnumerable<Interval> MajorPentatonic = GetIntervals(2, 2, 3, 2, 3);

        // malkos raga
        public static readonly IEnumerable<Interval> MalkosRaga = GetIntervals(3, 2, 3, 2, 2);

        // melodic minor
        public static readonly IEnumerable<Interval> MelodicMinor = GetIntervals(2, 1, 2, 2, 2, 2, 1);

        // melodic minor fifth mode
        public static readonly IEnumerable<Interval> MelodicMinorFifthMode = GetIntervals(2, 2, 1, 2, 1, 2, 2);

        // melodic minor second mode
        public static readonly IEnumerable<Interval> MelodicMinorSecondMode = GetIntervals(1, 2, 2, 2, 2, 1, 2);

        // minor
        public static readonly IEnumerable<Interval> Minor = GetIntervals(2, 1, 2, 2, 1, 2, 2);

        // minor #7M pentatonic
        public static readonly IEnumerable<Interval> Minor7MPentatonic = GetIntervals(3, 2, 2, 4, 1);

        // minor bebop
        public static readonly IEnumerable<Interval> MinorBebop = GetIntervals(2, 1, 2, 2, 1, 2, 1, 1);

        // minor blues
        public static readonly IEnumerable<Interval> MinorBlues = GetIntervals(3, 2, 1, 1, 3, 2);

        // minor hexatonic
        public static readonly IEnumerable<Interval> MinorHexatonic = GetIntervals(2, 1, 2, 2, 4, 1);

        // minor pentatonic
        public static readonly IEnumerable<Interval> MinorPentatonic = GetIntervals(3, 2, 2, 3, 2);

        // minor seven flat five pentatonic
        public static readonly IEnumerable<Interval> MinorSevenFlatFivePentatonic = GetIntervals(3, 2, 1, 4, 2);

        // minor six diminished
        public static readonly IEnumerable<Interval> MinorSixDiminished = GetIntervals(2, 1, 2, 2, 1, 1, 2, 1);

        // minor six pentatonic
        public static readonly IEnumerable<Interval> MinorSixPentatonic = GetIntervals(3, 2, 2, 2, 3);

        // mixolydian
        public static readonly IEnumerable<Interval> Mixolydian = GetIntervals(2, 2, 1, 2, 2, 1, 2);

        // mixolydian b6M
        public static readonly IEnumerable<Interval> MixolydianB6M = GetIntervals(2, 2, 1, 2, 1, 2, 2);

        // mixolydian pentatonic
        public static readonly IEnumerable<Interval> MixolydianPentatonic = GetIntervals(4, 1, 2, 3, 2);

        // mystery #1
        public static readonly IEnumerable<Interval> Mystery1 = GetIntervals(1, 3, 2, 2, 2, 2);

        // neopolitan
        public static readonly IEnumerable<Interval> Neopolitan = GetIntervals(1, 2, 2, 2, 1, 3, 1);

        // neopolitan major
        public static readonly IEnumerable<Interval> NeopolitanMajor = GetIntervals(1, 2, 2, 2, 2, 2, 1);

        // neopolitan major pentatonic
        public static readonly IEnumerable<Interval> NeopolitanMajorPentatonic = GetIntervals(4, 1, 1, 4, 2);

        // neopolitan minor
        public static readonly IEnumerable<Interval> NeopolitanMinor = GetIntervals(1, 2, 2, 2, 1, 3, 1);

        // oriental
        public static readonly IEnumerable<Interval> Oriental = GetIntervals(1, 3, 1, 1, 3, 1, 2);

        // pelog
        public static readonly IEnumerable<Interval> Pelog = GetIntervals(1, 2, 4, 1, 4);

        // pentatonic
        public static readonly IEnumerable<Interval> Pentatonic = GetIntervals(2, 2, 3, 2, 3);

        // persian
        public static readonly IEnumerable<Interval> Persian = GetIntervals(1, 3, 1, 1, 2, 3, 1);

        // phrygian
        public static readonly IEnumerable<Interval> Phrygian = GetIntervals(1, 2, 2, 2, 1, 2, 2);

        // phrygian major
        public static readonly IEnumerable<Interval> PhrygianMajor = GetIntervals(1, 3, 1, 2, 1, 2, 2);

        // piongio
        public static readonly IEnumerable<Interval> Piongio = GetIntervals(2, 3, 2, 2, 1, 2);

        // pomeroy
        public static readonly IEnumerable<Interval> Pomeroy = GetIntervals(1, 2, 1, 2, 2, 2, 2);

        // prometheus
        public static readonly IEnumerable<Interval> Prometheus = GetIntervals(2, 2, 2, 3, 1, 2);

        // prometheus neopolitan
        public static readonly IEnumerable<Interval> PrometheusNeopolitan = GetIntervals(1, 3, 2, 3, 1, 2);

        // purvi raga
        public static readonly IEnumerable<Interval> PurviRaga = GetIntervals(1, 3, 1, 1, 1, 1, 3, 1);

        // ritusen
        public static readonly IEnumerable<Interval> Ritusen = GetIntervals(2, 3, 2, 2, 3);

        // romanian minor
        public static readonly IEnumerable<Interval> RomanianMinor = GetIntervals(2, 1, 3, 1, 2, 1, 2);

        // scriabin
        public static readonly IEnumerable<Interval> Scriabin = GetIntervals(1, 3, 3, 2, 3);

        // six tone symmetric
        public static readonly IEnumerable<Interval> SixToneSymmetric = GetIntervals(1, 3, 1, 3, 1, 3);

        // spanish
        public static readonly IEnumerable<Interval> Spanish = GetIntervals(1, 3, 1, 2, 1, 2, 2);

        // spanish heptatonic
        public static readonly IEnumerable<Interval> SpanishHeptatonic = GetIntervals(1, 2, 1, 1, 2, 1, 2, 2);

        // super locrian
        public static readonly IEnumerable<Interval> SuperLocrian = GetIntervals(1, 2, 1, 2, 2, 2, 2);

        // super locrian pentatonic
        public static readonly IEnumerable<Interval> SuperLocrianPentatonic = GetIntervals(3, 1, 2, 4, 2);

        // todi raga
        public static readonly IEnumerable<Interval> TodiRaga = GetIntervals(1, 2, 3, 1, 1, 3, 1);

        // vietnamese 1
        public static readonly IEnumerable<Interval> Vietnamese1 = GetIntervals(3, 2, 2, 1, 4);

        // vietnamese 2
        public static readonly IEnumerable<Interval> Vietnamese2 = GetIntervals(3, 2, 2, 3, 2);

        // whole tone
        public static readonly IEnumerable<Interval> WholeTone = GetIntervals(2, 2, 2, 2, 2, 2);

        // whole tone pentatonic
        public static readonly IEnumerable<Interval> WholeTonePentatonic = GetIntervals(4, 2, 2, 2, 2);

        #endregion

        #region Methods

        private static IEnumerable<Interval> GetIntervals(params int[] intervalsInHalfSteps)
        {
            return intervalsInHalfSteps.Select(i => Interval.FromHalfSteps(i))
                                       .ToArray();
        }

        #endregion
    }
}
