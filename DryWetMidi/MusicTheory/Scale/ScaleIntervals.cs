using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    public static class ScaleIntervals
    {
        #region Constants

        [DisplayName("aeolian")]
        public static readonly IEnumerable<Interval> Aeolian = GetIntervals(2, 1, 2, 2, 1, 2, 2);

        [DisplayName("altered")]
        public static readonly IEnumerable<Interval> Altered = GetIntervals(1, 2, 1, 2, 2, 2, 2);

        [DisplayName("arabian")]
        public static readonly IEnumerable<Interval> Arabian = GetIntervals(2, 2, 1, 1, 2, 2, 2);

        [DisplayName("augmented")]
        public static readonly IEnumerable<Interval> Augmented = GetIntervals(3, 1, 3, 1, 3, 1);

        [DisplayName("augmented heptatonic")]
        public static readonly IEnumerable<Interval> AugmentedHeptatonic = GetIntervals(3, 1, 1, 2, 1, 3, 1);

        [DisplayName("balinese")]
        public static readonly IEnumerable<Interval> Balinese = GetIntervals(1, 2, 2, 2, 1, 3, 1);

        [DisplayName("bebop")]
        public static readonly IEnumerable<Interval> Bebop = GetIntervals(2, 2, 1, 2, 2, 1, 1, 1);

        [DisplayName("bebop dominant")]
        public static readonly IEnumerable<Interval> BebopDominant = GetIntervals(2, 2, 1, 2, 2, 1, 1, 1);

        [DisplayName("bebop locrian")]
        public static readonly IEnumerable<Interval> BebopLocrian = GetIntervals(1, 2, 2, 1, 1, 1, 2, 2);

        [DisplayName("bebop major")]
        public static readonly IEnumerable<Interval> BebopMajor = GetIntervals(2, 2, 1, 2, 1, 1, 2, 1);

        [DisplayName("bebop minor")]
        public static readonly IEnumerable<Interval> BebopMinor = GetIntervals(2, 1, 1, 1, 2, 2, 1, 2);

        [DisplayName("blues")]
        public static readonly IEnumerable<Interval> Blues = GetIntervals(3, 2, 1, 1, 3, 2);

        [DisplayName("chinese")]
        public static readonly IEnumerable<Interval> Chinese = GetIntervals(4, 2, 1, 4, 1);

        [DisplayName("chromatic")]
        public static readonly IEnumerable<Interval> Chromatic = GetIntervals(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);

        [DisplayName("composite blues")]
        public static readonly IEnumerable<Interval> CompositeBlues = GetIntervals(2, 1, 1, 1, 1, 1, 2, 1, 2);

        [DisplayName("diminished")]
        public static readonly IEnumerable<Interval> Diminished = GetIntervals(2, 1, 2, 1, 2, 1, 2, 1);

        [DisplayName("diminished whole tone")]
        public static readonly IEnumerable<Interval> DiminishedWholeTone = GetIntervals(1, 2, 1, 2, 2, 2, 2);

        [DisplayName("dominant")]
        public static readonly IEnumerable<Interval> Dominant = GetIntervals(2, 2, 1, 2, 2, 1, 2);

        [DisplayName("dorian")]
        public static readonly IEnumerable<Interval> Dorian = GetIntervals(2, 1, 2, 2, 2, 1, 2);

        [DisplayName("dorian #4")]
        public static readonly IEnumerable<Interval> Dorian4 = GetIntervals(2, 1, 3, 1, 2, 1, 2);

        [DisplayName("dorian b2")]
        public static readonly IEnumerable<Interval> DorianB2 = GetIntervals(1, 2, 2, 2, 2, 2, 1);

        [DisplayName("double harmonic lydian")]
        public static readonly IEnumerable<Interval> DoubleHarmonicLydian = GetIntervals(1, 3, 2, 1, 1, 3, 1);

        [DisplayName("double harmonic major")]
        public static readonly IEnumerable<Interval> DoubleHarmonicMajor = GetIntervals(1, 3, 1, 2, 1, 3, 1);

        [DisplayName("egyptian")]
        public static readonly IEnumerable<Interval> Egyptian = GetIntervals(2, 3, 2, 3, 2);

        [DisplayName("enigmatic")]
        public static readonly IEnumerable<Interval> Enigmatic = GetIntervals(1, 3, 2, 2, 2, 1, 1);

        [DisplayName("flamenco")]
        public static readonly IEnumerable<Interval> Flamenco = GetIntervals(1, 2, 1, 2, 1, 3, 2);

        [DisplayName("flat six pentatonic")]
        public static readonly IEnumerable<Interval> FlatSixPentatonic = GetIntervals(2, 2, 3, 1, 4);

        [DisplayName("flat three pentatonic")]
        public static readonly IEnumerable<Interval> FlatThreePentatonic = GetIntervals(2, 1, 4, 2, 3);

        [DisplayName("gypsy")]
        public static readonly IEnumerable<Interval> Gypsy = GetIntervals(1, 3, 1, 2, 1, 3, 1);

        [DisplayName("harmonic major")]
        public static readonly IEnumerable<Interval> HarmonicMajor = GetIntervals(2, 2, 1, 2, 1, 3, 1);

        [DisplayName("harmonic minor")]
        public static readonly IEnumerable<Interval> HarmonicMinor = GetIntervals(2, 1, 2, 2, 1, 3, 1);

        [DisplayName("hindu")]
        public static readonly IEnumerable<Interval> Hindu = GetIntervals(2, 2, 1, 2, 1, 2, 2);

        [DisplayName("hirajoshi")]
        public static readonly IEnumerable<Interval> Hirajoshi = GetIntervals(2, 1, 4, 1, 4);

        [DisplayName("hungarian major")]
        public static readonly IEnumerable<Interval> HungarianMajor = GetIntervals(3, 1, 2, 1, 2, 1, 2);

        [DisplayName("hungarian minor")]
        public static readonly IEnumerable<Interval> HungarianMinor = GetIntervals(2, 1, 3, 1, 1, 3, 1);

        [DisplayName("ichikosucho")]
        public static readonly IEnumerable<Interval> Ichikosucho = GetIntervals(2, 2, 1, 1, 1, 2, 2, 1);

        [DisplayName("in-sen")]
        public static readonly IEnumerable<Interval> InSen = GetIntervals(1, 4, 2, 3, 2);

        [DisplayName("indian")]
        public static readonly IEnumerable<Interval> Indian = GetIntervals(4, 1, 2, 3, 2);

        [DisplayName("ionian")]
        public static readonly IEnumerable<Interval> Ionian = GetIntervals(2, 2, 1, 2, 2, 2, 1);

        [DisplayName("ionian augmented")]
        public static readonly IEnumerable<Interval> IonianAugmented = GetIntervals(2, 2, 1, 3, 1, 2, 1);

        [DisplayName("ionian pentatonic")]
        public static readonly IEnumerable<Interval> IonianPentatonic = GetIntervals(4, 1, 2, 4, 1);

        [DisplayName("iwato")]
        public static readonly IEnumerable<Interval> Iwato = GetIntervals(1, 4, 1, 4, 2);

        [DisplayName("kafi raga")]
        public static readonly IEnumerable<Interval> KafiRaga = GetIntervals(3, 1, 1, 2, 2, 1, 1, 1);

        [DisplayName("kumoi")]
        public static readonly IEnumerable<Interval> Kumoi = GetIntervals(2, 1, 4, 2, 3);

        [DisplayName("kumoijoshi")]
        public static readonly IEnumerable<Interval> Kumoijoshi = GetIntervals(1, 4, 2, 1, 4);

        [DisplayName("leading whole tone")]
        public static readonly IEnumerable<Interval> LeadingWholeTone = GetIntervals(2, 2, 2, 2, 2, 1, 1);

        [DisplayName("locrian")]
        public static readonly IEnumerable<Interval> Locrian = GetIntervals(1, 2, 2, 1, 2, 2, 2);

        [DisplayName("locrian #2")]
        public static readonly IEnumerable<Interval> Locrian2 = GetIntervals(2, 1, 2, 1, 2, 2, 2);

        [DisplayName("locrian major")]
        public static readonly IEnumerable<Interval> LocrianMajor = GetIntervals(2, 2, 1, 1, 2, 2, 2);

        [DisplayName("locrian pentatonic")]
        public static readonly IEnumerable<Interval> LocrianPentatonic = GetIntervals(3, 2, 1, 4, 2);

        [DisplayName("lydian")]
        public static readonly IEnumerable<Interval> Lydian = GetIntervals(2, 2, 2, 1, 2, 2, 1);

        [DisplayName("lydian #5P pentatonic")]
        public static readonly IEnumerable<Interval> Lydian5PPentatonic = GetIntervals(4, 2, 2, 3, 1);

        [DisplayName("lydian #9")]
        public static readonly IEnumerable<Interval> Lydian9 = GetIntervals(1, 3, 2, 1, 2, 2, 1);

        [DisplayName("lydian augmented")]
        public static readonly IEnumerable<Interval> LydianAugmented = GetIntervals(2, 2, 2, 2, 1, 2, 1);

        [DisplayName("lydian b7")]
        public static readonly IEnumerable<Interval> LydianB7 = GetIntervals(2, 2, 2, 1, 2, 1, 2);

        [DisplayName("lydian diminished")]
        public static readonly IEnumerable<Interval> LydianDiminished = GetIntervals(2, 1, 3, 1, 2, 2, 1);

        [DisplayName("lydian dominant")]
        public static readonly IEnumerable<Interval> LydianDominant = GetIntervals(2, 2, 2, 1, 2, 1, 2);

        [DisplayName("lydian dominant pentatonic")]
        public static readonly IEnumerable<Interval> LydianDominantPentatonic = GetIntervals(4, 2, 1, 3, 2);

        [DisplayName("lydian minor")]
        public static readonly IEnumerable<Interval> LydianMinor = GetIntervals(2, 2, 2, 1, 1, 2, 2);

        [DisplayName("lydian pentatonic")]
        public static readonly IEnumerable<Interval> LydianPentatonic = GetIntervals(4, 2, 1, 4, 1);

        [DisplayName("major")]
        public static readonly IEnumerable<Interval> Major = GetIntervals(2, 2, 1, 2, 2, 2, 1);

        [DisplayName("major blues")]
        public static readonly IEnumerable<Interval> MajorBlues = GetIntervals(2, 1, 1, 3, 2, 3);

        [DisplayName("major flat two pentatonic")]
        public static readonly IEnumerable<Interval> MajorFlatTwoPentatonic = GetIntervals(1, 3, 3, 2, 3);

        [DisplayName("major pentatonic")]
        public static readonly IEnumerable<Interval> MajorPentatonic = GetIntervals(2, 2, 3, 2, 3);

        [DisplayName("malkos raga")]
        public static readonly IEnumerable<Interval> MalkosRaga = GetIntervals(3, 2, 3, 2, 2);

        [DisplayName("melodic minor")]
        public static readonly IEnumerable<Interval> MelodicMinor = GetIntervals(2, 1, 2, 2, 2, 2, 1);

        [DisplayName("melodic minor fifth mode")]
        public static readonly IEnumerable<Interval> MelodicMinorFifthMode = GetIntervals(2, 2, 1, 2, 1, 2, 2);

        [DisplayName("melodic minor second mode")]
        public static readonly IEnumerable<Interval> MelodicMinorSecondMode = GetIntervals(1, 2, 2, 2, 2, 1, 2);

        [DisplayName("minor")]
        public static readonly IEnumerable<Interval> Minor = GetIntervals(2, 1, 2, 2, 1, 2, 2);

        [DisplayName("minor #7M pentatonic")]
        public static readonly IEnumerable<Interval> Minor7MPentatonic = GetIntervals(3, 2, 2, 4, 1);

        [DisplayName("minor bebop")]
        public static readonly IEnumerable<Interval> MinorBebop = GetIntervals(2, 1, 2, 2, 1, 2, 1, 1);

        [DisplayName("minor blues")]
        public static readonly IEnumerable<Interval> MinorBlues = GetIntervals(3, 2, 1, 1, 3, 2);

        [DisplayName("minor hexatonic")]
        public static readonly IEnumerable<Interval> MinorHexatonic = GetIntervals(2, 1, 2, 2, 4, 1);

        [DisplayName("minor pentatonic")]
        public static readonly IEnumerable<Interval> MinorPentatonic = GetIntervals(3, 2, 2, 3, 2);

        [DisplayName("minor seven flat five pentatonic")]
        public static readonly IEnumerable<Interval> MinorSevenFlatFivePentatonic = GetIntervals(3, 2, 1, 4, 2);

        [DisplayName("minor six diminished")]
        public static readonly IEnumerable<Interval> MinorSixDiminished = GetIntervals(2, 1, 2, 2, 1, 1, 2, 1);

        [DisplayName("minor six pentatonic")]
        public static readonly IEnumerable<Interval> MinorSixPentatonic = GetIntervals(3, 2, 2, 2, 3);

        [DisplayName("mixolydian")]
        public static readonly IEnumerable<Interval> Mixolydian = GetIntervals(2, 2, 1, 2, 2, 1, 2);

        [DisplayName("mixolydian b6M")]
        public static readonly IEnumerable<Interval> MixolydianB6M = GetIntervals(2, 2, 1, 2, 1, 2, 2);

        [DisplayName("mixolydian pentatonic")]
        public static readonly IEnumerable<Interval> MixolydianPentatonic = GetIntervals(4, 1, 2, 3, 2);

        [DisplayName("mystery #1")]
        public static readonly IEnumerable<Interval> Mystery1 = GetIntervals(1, 3, 2, 2, 2, 2);

        [DisplayName("neopolitan")]
        public static readonly IEnumerable<Interval> Neopolitan = GetIntervals(1, 2, 2, 2, 1, 3, 1);

        [DisplayName("neopolitan major")]
        public static readonly IEnumerable<Interval> NeopolitanMajor = GetIntervals(1, 2, 2, 2, 2, 2, 1);

        [DisplayName("neopolitan major pentatonic")]
        public static readonly IEnumerable<Interval> NeopolitanMajorPentatonic = GetIntervals(4, 1, 1, 4, 2);

        [DisplayName("neopolitan minor")]
        public static readonly IEnumerable<Interval> NeopolitanMinor = GetIntervals(1, 2, 2, 2, 1, 3, 1);

        [DisplayName("oriental")]
        public static readonly IEnumerable<Interval> Oriental = GetIntervals(1, 3, 1, 1, 3, 1, 2);

        [DisplayName("pelog")]
        public static readonly IEnumerable<Interval> Pelog = GetIntervals(1, 2, 4, 1, 4);

        [DisplayName("pentatonic")]
        public static readonly IEnumerable<Interval> Pentatonic = GetIntervals(2, 2, 3, 2, 3);

        [DisplayName("persian")]
        public static readonly IEnumerable<Interval> Persian = GetIntervals(1, 3, 1, 1, 2, 3, 1);

        [DisplayName("phrygian")]
        public static readonly IEnumerable<Interval> Phrygian = GetIntervals(1, 2, 2, 2, 1, 2, 2);

        [DisplayName("phrygian major")]
        public static readonly IEnumerable<Interval> PhrygianMajor = GetIntervals(1, 3, 1, 2, 1, 2, 2);

        [DisplayName("piongio")]
        public static readonly IEnumerable<Interval> Piongio = GetIntervals(2, 3, 2, 2, 1, 2);

        [DisplayName("pomeroy")]
        public static readonly IEnumerable<Interval> Pomeroy = GetIntervals(1, 2, 1, 2, 2, 2, 2);

        [DisplayName("prometheus")]
        public static readonly IEnumerable<Interval> Prometheus = GetIntervals(2, 2, 2, 3, 1, 2);

        [DisplayName("prometheus neopolitan")]
        public static readonly IEnumerable<Interval> PrometheusNeopolitan = GetIntervals(1, 3, 2, 3, 1, 2);

        [DisplayName("purvi raga")]
        public static readonly IEnumerable<Interval> PurviRaga = GetIntervals(1, 3, 1, 1, 1, 1, 3, 1);

        [DisplayName("ritusen")]
        public static readonly IEnumerable<Interval> Ritusen = GetIntervals(2, 3, 2, 2, 3);

        [DisplayName("romanian minor")]
        public static readonly IEnumerable<Interval> RomanianMinor = GetIntervals(2, 1, 3, 1, 2, 1, 2);

        [DisplayName("scriabin")]
        public static readonly IEnumerable<Interval> Scriabin = GetIntervals(1, 3, 3, 2, 3);

        [DisplayName("six tone symmetric")]
        public static readonly IEnumerable<Interval> SixToneSymmetric = GetIntervals(1, 3, 1, 3, 1, 3);

        [DisplayName("spanish")]
        public static readonly IEnumerable<Interval> Spanish = GetIntervals(1, 3, 1, 2, 1, 2, 2);

        [DisplayName("spanish heptatonic")]
        public static readonly IEnumerable<Interval> SpanishHeptatonic = GetIntervals(1, 2, 1, 1, 2, 1, 2, 2);

        [DisplayName("super locrian")]
        public static readonly IEnumerable<Interval> SuperLocrian = GetIntervals(1, 2, 1, 2, 2, 2, 2);

        [DisplayName("super locrian pentatonic")]
        public static readonly IEnumerable<Interval> SuperLocrianPentatonic = GetIntervals(3, 1, 2, 4, 2);

        [DisplayName("todi raga")]
        public static readonly IEnumerable<Interval> TodiRaga = GetIntervals(1, 2, 3, 1, 1, 3, 1);

        [DisplayName("vietnamese 1")]
        public static readonly IEnumerable<Interval> Vietnamese1 = GetIntervals(3, 2, 2, 1, 4);

        [DisplayName("vietnamese 2")]
        public static readonly IEnumerable<Interval> Vietnamese2 = GetIntervals(3, 2, 2, 3, 2);

        [DisplayName("whole tone")]
        public static readonly IEnumerable<Interval> WholeTone = GetIntervals(2, 2, 2, 2, 2, 2);

        [DisplayName("whole tone pentatonic")]
        public static readonly IEnumerable<Interval> WholeTonePentatonic = GetIntervals(4, 2, 2, 2, 2);

        #endregion

        #region Methods

        public static IEnumerable<Interval> GetByName(string name)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(name), name, "Scale's name");

            foreach (var fieldInfo in typeof(ScaleIntervals).GetFields(BindingFlags.Static | BindingFlags.Public))
            {
                var displayName = (Attribute.GetCustomAttribute(fieldInfo, typeof(DisplayNameAttribute)) as DisplayNameAttribute)?.Name;
                if (string.IsNullOrWhiteSpace(displayName) || !displayName.Equals(name, StringComparison.InvariantCultureIgnoreCase))
                    continue;

                var intervals = fieldInfo.GetValue(null) as IEnumerable<Interval>;
                if (intervals == null)
                    continue;

                return intervals;
            }

            return null;
        }

        private static IEnumerable<Interval> GetIntervals(params int[] intervalsInHalfSteps)
        {
            return intervalsInHalfSteps.Select(i => Interval.FromHalfSteps(i))
                                       .ToArray();
        }

        #endregion
    }
}
