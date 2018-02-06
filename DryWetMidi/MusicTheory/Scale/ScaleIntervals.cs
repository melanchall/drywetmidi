using System.Linq;

namespace Melanchall.DryWetMidi.MusicTheory
{
    public static class ScaleIntervals
    {
        #region Constants

        // aeolian
        public static readonly Interval[] Aeolian = GetIntervals(2, 1, 2, 2, 1, 2, 2);

        // altered
        public static readonly Interval[] Altered = GetIntervals(1, 2, 1, 2, 2, 2, 2);

        // arabian
        public static readonly Interval[] Arabian = GetIntervals(2, 2, 1, 1, 2, 2, 2);

        // augmented
        public static readonly Interval[] Augmented = GetIntervals(3, 1, 3, 1, 3, 1);

        // augmented heptatonic
        public static readonly Interval[] AugmentedHeptatonic = GetIntervals(3, 1, 1, 2, 1, 3, 1);

        // balinese
        public static readonly Interval[] Balinese = GetIntervals(1, 2, 2, 2, 1, 3, 1);

        // bebop
        public static readonly Interval[] Bebop = GetIntervals(2, 2, 1, 2, 2, 1, 1, 1);

        // bebop dominant
        public static readonly Interval[] BebopDominant = GetIntervals(2, 2, 1, 2, 2, 1, 1, 1);

        // bebop locrian
        public static readonly Interval[] BebopLocrian = GetIntervals(1, 2, 2, 1, 1, 1, 2, 2);

        // bebop major
        public static readonly Interval[] BebopMajor = GetIntervals(2, 2, 1, 2, 1, 1, 2, 1);

        // bebop minor
        public static readonly Interval[] BebopMinor = GetIntervals(2, 1, 1, 1, 2, 2, 1, 2);

        // blues
        public static readonly Interval[] Blues = GetIntervals(3, 2, 1, 1, 3, 2);

        // chinese
        public static readonly Interval[] Chinese = GetIntervals(4, 2, 1, 4, 1);

        // chromatic
        public static readonly Interval[] Chromatic = GetIntervals(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);

        // composite blues
        public static readonly Interval[] CompositeBlues = GetIntervals(2, 1, 1, 1, 1, 1, 2, 1, 2);

        // diminished
        public static readonly Interval[] Diminished = GetIntervals(2, 1, 2, 1, 2, 1, 2, 1);

        // diminished whole tone
        public static readonly Interval[] DiminishedWholeTone = GetIntervals(1, 2, 1, 2, 2, 2, 2);

        // dominant
        public static readonly Interval[] Dominant = GetIntervals(2, 2, 1, 2, 2, 1, 2);

        // dorian
        public static readonly Interval[] Dorian = GetIntervals(2, 1, 2, 2, 2, 1, 2);

        // dorian #4
        public static readonly Interval[] Dorian4 = GetIntervals(2, 1, 3, 1, 2, 1, 2);

        // dorian b2
        public static readonly Interval[] DorianB2 = GetIntervals(1, 2, 2, 2, 2, 2, 1);

        // double harmonic lydian
        public static readonly Interval[] DoubleHarmonicLydian = GetIntervals(1, 3, 2, 1, 1, 3, 1);

        // double harmonic major
        public static readonly Interval[] DoubleHarmonicMajor = GetIntervals(1, 3, 1, 2, 1, 3, 1);

        // egyptian
        public static readonly Interval[] Egyptian = GetIntervals(2, 3, 2, 3, 2);

        // enigmatic
        public static readonly Interval[] Enigmatic = GetIntervals(1, 3, 2, 2, 2, 1, 1);

        // flamenco
        public static readonly Interval[] Flamenco = GetIntervals(1, 2, 1, 2, 1, 3, 2);

        // flat six pentatonic
        public static readonly Interval[] FlatSixPentatonic = GetIntervals(2, 2, 3, 1, 4);

        // flat three pentatonic
        public static readonly Interval[] FlatThreePentatonic = GetIntervals(2, 1, 4, 2, 3);

        // gypsy
        public static readonly Interval[] Gypsy = GetIntervals(1, 3, 1, 2, 1, 3, 1);

        // harmonic major
        public static readonly Interval[] HarmonicMajor = GetIntervals(2, 2, 1, 2, 1, 3, 1);

        // harmonic minor
        public static readonly Interval[] HarmonicMinor = GetIntervals(2, 1, 2, 2, 1, 3, 1);

        // hindu
        public static readonly Interval[] Hindu = GetIntervals(2, 2, 1, 2, 1, 2, 2);

        // hirajoshi
        public static readonly Interval[] Hirajoshi = GetIntervals(2, 1, 4, 1, 4);

        // hungarian major
        public static readonly Interval[] HungarianMajor = GetIntervals(3, 1, 2, 1, 2, 1, 2);

        // hungarian minor
        public static readonly Interval[] HungarianMinor = GetIntervals(2, 1, 3, 1, 1, 3, 1);

        // ichikosucho
        public static readonly Interval[] Ichikosucho = GetIntervals(2, 2, 1, 1, 1, 2, 2, 1);

        // in-sen
        public static readonly Interval[] InSen = GetIntervals(1, 4, 2, 3, 2);

        // indian
        public static readonly Interval[] Indian = GetIntervals(4, 1, 2, 3, 2);

        // ionian
        public static readonly Interval[] Ionian = GetIntervals(2, 2, 1, 2, 2, 2, 1);

        // ionian augmented
        public static readonly Interval[] IonianAugmented = GetIntervals(2, 2, 1, 3, 1, 2, 1);

        // ionian pentatonic
        public static readonly Interval[] IonianPentatonic = GetIntervals(4, 1, 2, 4, 1);

        // iwato
        public static readonly Interval[] Iwato = GetIntervals(1, 4, 1, 4, 2);

        // kafi raga
        public static readonly Interval[] KafiRaga = GetIntervals(3, 1, 1, 2, 2, 1, 1, 1);

        // kumoi
        public static readonly Interval[] Kumoi = GetIntervals(2, 1, 4, 2, 3);

        // kumoijoshi
        public static readonly Interval[] Kumoijoshi = GetIntervals(1, 4, 2, 1, 4);

        // leading whole tone
        public static readonly Interval[] LeadingWholeTone = GetIntervals(2, 2, 2, 2, 2, 1, 1);

        // locrian
        public static readonly Interval[] Locrian = GetIntervals(1, 2, 2, 1, 2, 2, 2);

        // locrian #2
        public static readonly Interval[] Locrian2 = GetIntervals(2, 1, 2, 1, 2, 2, 2);

        // locrian major
        public static readonly Interval[] LocrianMajor = GetIntervals(2, 2, 1, 1, 2, 2, 2);

        // locrian pentatonic
        public static readonly Interval[] LocrianPentatonic = GetIntervals(3, 2, 1, 4, 2);

        // lydian
        public static readonly Interval[] Lydian = GetIntervals(2, 2, 2, 1, 2, 2, 1);

        // lydian #5P pentatonic
        public static readonly Interval[] Lydian5PPentatonic = GetIntervals(4, 2, 2, 3, 1);

        // lydian #9
        public static readonly Interval[] Lydian9 = GetIntervals(1, 3, 2, 1, 2, 2, 1);

        // lydian augmented
        public static readonly Interval[] LydianAugmented = GetIntervals(2, 2, 2, 2, 1, 2, 1);

        // lydian b7
        public static readonly Interval[] LydianB7 = GetIntervals(2, 2, 2, 1, 2, 1, 2);

        // lydian diminished
        public static readonly Interval[] LydianDiminished = GetIntervals(2, 1, 3, 1, 2, 2, 1);

        // lydian dominant
        public static readonly Interval[] LydianDominant = GetIntervals(2, 2, 2, 1, 2, 1, 2);

        // lydian dominant pentatonic
        public static readonly Interval[] LydianDominantPentatonic = GetIntervals(4, 2, 1, 3, 2);

        // lydian minor
        public static readonly Interval[] LydianMinor = GetIntervals(2, 2, 2, 1, 1, 2, 2);

        // lydian pentatonic
        public static readonly Interval[] LydianPentatonic = GetIntervals(4, 2, 1, 4, 1);

        // major
        public static readonly Interval[] Major = GetIntervals(2, 2, 1, 2, 2, 2, 1);

        // major blues
        public static readonly Interval[] MajorBlues = GetIntervals(2, 1, 1, 3, 2, 3);

        // major flat two pentatonic
        public static readonly Interval[] MajorFlatTwoPentatonic = GetIntervals(1, 3, 3, 2, 3);

        // major pentatonic
        public static readonly Interval[] MajorPentatonic = GetIntervals(2, 2, 3, 2, 3);

        // malkos raga
        public static readonly Interval[] MalkosRaga = GetIntervals(3, 2, 3, 2, 2);

        // melodic minor
        public static readonly Interval[] MelodicMinor = GetIntervals(2, 1, 2, 2, 2, 2, 1);

        // melodic minor fifth mode
        public static readonly Interval[] MelodicMinorFifthMode = GetIntervals(2, 2, 1, 2, 1, 2, 2);

        // melodic minor second mode
        public static readonly Interval[] MelodicMinorSecondMode = GetIntervals(1, 2, 2, 2, 2, 1, 2);

        // minor
        public static readonly Interval[] Minor = GetIntervals(2, 1, 2, 2, 1, 2, 2);

        // minor #7M pentatonic
        public static readonly Interval[] Minor7MPentatonic = GetIntervals(3, 2, 2, 4, 1);

        // minor bebop
        public static readonly Interval[] MinorBebop = GetIntervals(2, 1, 2, 2, 1, 2, 1, 1);

        // minor blues
        public static readonly Interval[] MinorBlues = GetIntervals(3, 2, 1, 1, 3, 2);

        // minor hexatonic
        public static readonly Interval[] MinorHexatonic = GetIntervals(2, 1, 2, 2, 4, 1);

        // minor pentatonic
        public static readonly Interval[] MinorPentatonic = GetIntervals(3, 2, 2, 3, 2);

        // minor seven flat five pentatonic
        public static readonly Interval[] MinorSevenFlatFivePentatonic = GetIntervals(3, 2, 1, 4, 2);

        // minor six diminished
        public static readonly Interval[] MinorSixDiminished = GetIntervals(2, 1, 2, 2, 1, 1, 2, 1);

        // minor six pentatonic
        public static readonly Interval[] MinorSixPentatonic = GetIntervals(3, 2, 2, 2, 3);

        // mixolydian
        public static readonly Interval[] Mixolydian = GetIntervals(2, 2, 1, 2, 2, 1, 2);

        // mixolydian b6M
        public static readonly Interval[] MixolydianB6M = GetIntervals(2, 2, 1, 2, 1, 2, 2);

        // mixolydian pentatonic
        public static readonly Interval[] MixolydianPentatonic = GetIntervals(4, 1, 2, 3, 2);

        // mystery #1
        public static readonly Interval[] Mystery1 = GetIntervals(1, 3, 2, 2, 2, 2);

        // neopolitan
        public static readonly Interval[] Neopolitan = GetIntervals(1, 2, 2, 2, 1, 3, 1);

        // neopolitan major
        public static readonly Interval[] NeopolitanMajor = GetIntervals(1, 2, 2, 2, 2, 2, 1);

        // neopolitan major pentatonic
        public static readonly Interval[] NeopolitanMajorPentatonic = GetIntervals(4, 1, 1, 4, 2);

        // neopolitan minor
        public static readonly Interval[] NeopolitanMinor = GetIntervals(1, 2, 2, 2, 1, 3, 1);

        // oriental
        public static readonly Interval[] Oriental = GetIntervals(1, 3, 1, 1, 3, 1, 2);

        // pelog
        public static readonly Interval[] Pelog = GetIntervals(1, 2, 4, 1, 4);

        // pentatonic
        public static readonly Interval[] Pentatonic = GetIntervals(2, 2, 3, 2, 3);

        // persian
        public static readonly Interval[] Persian = GetIntervals(1, 3, 1, 1, 2, 3, 1);

        // phrygian
        public static readonly Interval[] Phrygian = GetIntervals(1, 2, 2, 2, 1, 2, 2);

        // phrygian major
        public static readonly Interval[] PhrygianMajor = GetIntervals(1, 3, 1, 2, 1, 2, 2);

        // piongio
        public static readonly Interval[] Piongio = GetIntervals(2, 3, 2, 2, 1, 2);

        // pomeroy
        public static readonly Interval[] Pomeroy = GetIntervals(1, 2, 1, 2, 2, 2, 2);

        // prometheus
        public static readonly Interval[] Prometheus = GetIntervals(2, 2, 2, 3, 1, 2);

        // prometheus neopolitan
        public static readonly Interval[] PrometheusNeopolitan = GetIntervals(1, 3, 2, 3, 1, 2);

        // purvi raga
        public static readonly Interval[] PurviRaga = GetIntervals(1, 3, 1, 1, 1, 1, 3, 1);

        // ritusen
        public static readonly Interval[] Ritusen = GetIntervals(2, 3, 2, 2, 3);

        // romanian minor
        public static readonly Interval[] RomanianMinor = GetIntervals(2, 1, 3, 1, 2, 1, 2);

        // scriabin
        public static readonly Interval[] Scriabin = GetIntervals(1, 3, 3, 2, 3);

        // six tone symmetric
        public static readonly Interval[] SixToneSymmetric = GetIntervals(1, 3, 1, 3, 1, 3);

        // spanish
        public static readonly Interval[] Spanish = GetIntervals(1, 3, 1, 2, 1, 2, 2);

        // spanish heptatonic
        public static readonly Interval[] SpanishHeptatonic = GetIntervals(1, 2, 1, 1, 2, 1, 2, 2);

        // super locrian
        public static readonly Interval[] SuperLocrian = GetIntervals(1, 2, 1, 2, 2, 2, 2);

        // super locrian pentatonic
        public static readonly Interval[] SuperLocrianPentatonic = GetIntervals(3, 1, 2, 4, 2);

        // todi raga
        public static readonly Interval[] TodiRaga = GetIntervals(1, 2, 3, 1, 1, 3, 1);

        // vietnamese 1
        public static readonly Interval[] Vietnamese1 = GetIntervals(3, 2, 2, 1, 4);

        // vietnamese 2
        public static readonly Interval[] Vietnamese2 = GetIntervals(3, 2, 2, 3, 2);

        // whole tone
        public static readonly Interval[] WholeTone = GetIntervals(2, 2, 2, 2, 2, 2);

        // whole tone pentatonic
        public static readonly Interval[] WholeTonePentatonic = GetIntervals(4, 2, 2, 2, 2);

        #endregion

        #region Methods

        private static Interval[] GetIntervals(params int[] intervalsInHalfSteps)
        {
            return intervalsInHalfSteps.Select(i => Interval.FromHalfSteps(i))
                                       .ToArray();
        }

        #endregion
    }
}
