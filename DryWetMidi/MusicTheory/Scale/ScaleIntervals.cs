using System;
using System.Collections.Generic;
using System.Linq;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    /// <summary>
    /// Provides intervals sequences for known musical scales.
    /// </summary>
    public static class ScaleIntervals
    {
        #region Constants

        /// <summary>
        /// 'Aeolian' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Aeolian = GetIntervals(2, 1, 2, 2, 1, 2, 2);

        /// <summary>
        /// 'Altered' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Altered = GetIntervals(1, 2, 1, 2, 2, 2, 2);

        /// <summary>
        /// 'Arabian' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Arabian = GetIntervals(2, 2, 1, 1, 2, 2, 2);

        /// <summary>
        /// 'Augmented' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Augmented = GetIntervals(3, 1, 3, 1, 3, 1);

        /// <summary>
        /// 'Augmented Heptatonic' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> AugmentedHeptatonic = GetIntervals(3, 1, 1, 2, 1, 3, 1);

        /// <summary>
        /// 'Balinese' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Balinese = GetIntervals(1, 2, 2, 2, 1, 3, 1);

        /// <summary>
        /// 'Bebop' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Bebop = GetIntervals(2, 2, 1, 2, 2, 1, 1, 1);

        /// <summary>
        /// 'Bebop Dominant' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> BebopDominant = GetIntervals(2, 2, 1, 2, 2, 1, 1, 1);

        /// <summary>
        /// 'Bebop Locrian' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> BebopLocrian = GetIntervals(1, 2, 2, 1, 1, 1, 2, 2);

        /// <summary>
        /// 'Bebop Major' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> BebopMajor = GetIntervals(2, 2, 1, 2, 1, 1, 2, 1);

        /// <summary>
        /// 'Bebop Minor' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> BebopMinor = GetIntervals(2, 1, 1, 1, 2, 2, 1, 2);

        /// <summary>
        /// 'Blues' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Blues = GetIntervals(3, 2, 1, 1, 3, 2);

        /// <summary>
        /// 'Chinese' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Chinese = GetIntervals(4, 2, 1, 4, 1);

        /// <summary>
        /// 'Chromatic' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Chromatic = GetIntervals(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1);

        /// <summary>
        /// 'Composite Blues' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> CompositeBlues = GetIntervals(2, 1, 1, 1, 1, 1, 2, 1, 2);

        /// <summary>
        /// 'Diminished' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Diminished = GetIntervals(2, 1, 2, 1, 2, 1, 2, 1);

        /// <summary>
        /// 'Diminished Whole Tone' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> DiminishedWholeTone = GetIntervals(1, 2, 1, 2, 2, 2, 2);

        /// <summary>
        /// 'Dominant' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Dominant = GetIntervals(2, 2, 1, 2, 2, 1, 2);

        /// <summary>
        /// 'Dorian' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Dorian = GetIntervals(2, 1, 2, 2, 2, 1, 2);

        /// <summary>
        /// 'Dorian #4' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Dorian4 = GetIntervals(2, 1, 3, 1, 2, 1, 2);

        /// <summary>
        /// 'Dorian b2' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> DorianB2 = GetIntervals(1, 2, 2, 2, 2, 2, 1);

        /// <summary>
        /// 'Double Harmonic Lydian' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> DoubleHarmonicLydian = GetIntervals(1, 3, 2, 1, 1, 3, 1);

        /// <summary>
        /// 'Double Harmonic Major' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> DoubleHarmonicMajor = GetIntervals(1, 3, 1, 2, 1, 3, 1);

        /// <summary>
        /// 'Egyptian' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Egyptian = GetIntervals(2, 3, 2, 3, 2);

        /// <summary>
        /// 'Enigmatic' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Enigmatic = GetIntervals(1, 3, 2, 2, 2, 1, 1);

        /// <summary>
        /// 'Flamenco' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Flamenco = GetIntervals(1, 2, 1, 2, 1, 3, 2);

        /// <summary>
        /// 'Flat Six Pentatonic' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> FlatSixPentatonic = GetIntervals(2, 2, 3, 1, 4);

        /// <summary>
        /// 'Flat Three Pentatonic' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> FlatThreePentatonic = GetIntervals(2, 1, 4, 2, 3);

        /// <summary>
        /// 'Gypsy' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Gypsy = GetIntervals(1, 3, 1, 2, 1, 3, 1);

        /// <summary>
        /// 'Harmonic Major' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> HarmonicMajor = GetIntervals(2, 2, 1, 2, 1, 3, 1);

        /// <summary>
        /// 'Harmonic Minor' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> HarmonicMinor = GetIntervals(2, 1, 2, 2, 1, 3, 1);

        /// <summary>
        /// 'Hindu' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Hindu = GetIntervals(2, 2, 1, 2, 1, 2, 2);

        /// <summary>
        /// 'Hirajoshi' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Hirajoshi = GetIntervals(2, 1, 4, 1, 4);

        /// <summary>
        /// 'Hungarian Major' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> HungarianMajor = GetIntervals(3, 1, 2, 1, 2, 1, 2);

        /// <summary>
        /// 'Hungarian Minor' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> HungarianMinor = GetIntervals(2, 1, 3, 1, 1, 3, 1);

        /// <summary>
        /// 'Ichikosucho' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Ichikosucho = GetIntervals(2, 2, 1, 1, 1, 2, 2, 1);

        /// <summary>
        /// 'In-Sen' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> InSen = GetIntervals(1, 4, 2, 3, 2);

        /// <summary>
        /// 'Indian' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Indian = GetIntervals(4, 1, 2, 3, 2);

        /// <summary>
        /// 'Ionian' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Ionian = GetIntervals(2, 2, 1, 2, 2, 2, 1);

        /// <summary>
        /// 'Ionian Augmented' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> IonianAugmented = GetIntervals(2, 2, 1, 3, 1, 2, 1);

        /// <summary>
        /// 'Ionian Pentatonic' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> IonianPentatonic = GetIntervals(4, 1, 2, 4, 1);

        /// <summary>
        /// 'Iwato' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Iwato = GetIntervals(1, 4, 1, 4, 2);

        /// <summary>
        /// 'Kafi Raga' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> KafiRaga = GetIntervals(3, 1, 1, 2, 2, 1, 1, 1);

        /// <summary>
        /// 'Kumoi' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Kumoi = GetIntervals(2, 1, 4, 2, 3);

        /// <summary>
        /// 'Kumoijoshi' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Kumoijoshi = GetIntervals(1, 4, 2, 1, 4);

        /// <summary>
        /// 'Leading Whole Tone' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> LeadingWholeTone = GetIntervals(2, 2, 2, 2, 2, 1, 1);

        /// <summary>
        /// 'Locrian' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Locrian = GetIntervals(1, 2, 2, 1, 2, 2, 2);

        /// <summary>
        /// 'Locrian #2' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Locrian2 = GetIntervals(2, 1, 2, 1, 2, 2, 2);

        /// <summary>
        /// 'Locrian Major' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> LocrianMajor = GetIntervals(2, 2, 1, 1, 2, 2, 2);

        /// <summary>
        /// 'Locrian Pentatonic' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> LocrianPentatonic = GetIntervals(3, 2, 1, 4, 2);

        /// <summary>
        /// 'Lydian' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Lydian = GetIntervals(2, 2, 2, 1, 2, 2, 1);

        /// <summary>
        /// 'Lydian #5P Pentatonic' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Lydian5PPentatonic = GetIntervals(4, 2, 2, 3, 1);

        /// <summary>
        /// 'Lydian #9' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Lydian9 = GetIntervals(1, 3, 2, 1, 2, 2, 1);

        /// <summary>
        /// 'Lydian Augmented' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> LydianAugmented = GetIntervals(2, 2, 2, 2, 1, 2, 1);

        /// <summary>
        /// 'Lydian b7' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> LydianB7 = GetIntervals(2, 2, 2, 1, 2, 1, 2);

        /// <summary>
        /// 'Lydian Diminished' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> LydianDiminished = GetIntervals(2, 1, 3, 1, 2, 2, 1);

        /// <summary>
        /// 'Lydian Dominant' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> LydianDominant = GetIntervals(2, 2, 2, 1, 2, 1, 2);

        /// <summary>
        /// 'Lydian Dominant Pentatonic' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> LydianDominantPentatonic = GetIntervals(4, 2, 1, 3, 2);

        /// <summary>
        /// 'Lydian Minor' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> LydianMinor = GetIntervals(2, 2, 2, 1, 1, 2, 2);

        /// <summary>
        /// 'Lydian Pentatonic' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> LydianPentatonic = GetIntervals(4, 2, 1, 4, 1);

        /// <summary>
        /// 'Major' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Major = GetIntervals(2, 2, 1, 2, 2, 2, 1);

        /// <summary>
        /// 'Major Blues' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> MajorBlues = GetIntervals(2, 1, 1, 3, 2, 3);

        /// <summary>
        /// 'Major Flat Two Pentatonic' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> MajorFlatTwoPentatonic = GetIntervals(1, 3, 3, 2, 3);

        /// <summary>
        /// 'Major Pentatonic' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> MajorPentatonic = GetIntervals(2, 2, 3, 2, 3);

        /// <summary>
        /// 'Malkos Raga' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> MalkosRaga = GetIntervals(3, 2, 3, 2, 2);

        /// <summary>
        /// 'Melodic Minor' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> MelodicMinor = GetIntervals(2, 1, 2, 2, 2, 2, 1);

        /// <summary>
        /// 'Melodic Minor Fifth Mode' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> MelodicMinorFifthMode = GetIntervals(2, 2, 1, 2, 1, 2, 2);

        /// <summary>
        /// 'Melodic Minor Second Mode' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> MelodicMinorSecondMode = GetIntervals(1, 2, 2, 2, 2, 1, 2);

        /// <summary>
        /// 'Minor' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Minor = GetIntervals(2, 1, 2, 2, 1, 2, 2);

        /// <summary>
        /// 'Minor #7M Pentatonic' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Minor7MPentatonic = GetIntervals(3, 2, 2, 4, 1);

        /// <summary>
        /// 'Minor Bebop' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> MinorBebop = GetIntervals(2, 1, 2, 2, 1, 2, 1, 1);

        /// <summary>
        /// 'Minor Blues' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> MinorBlues = GetIntervals(3, 2, 1, 1, 3, 2);

        /// <summary>
        /// 'Minor Hexatonic' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> MinorHexatonic = GetIntervals(2, 1, 2, 2, 4, 1);

        /// <summary>
        /// 'Minor Pentatonic' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> MinorPentatonic = GetIntervals(3, 2, 2, 3, 2);

        /// <summary>
        /// 'Minor Seven Flat Five Pentatonic' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> MinorSevenFlatFivePentatonic = GetIntervals(3, 2, 1, 4, 2);

        /// <summary>
        /// 'Minor Six Diminished' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> MinorSixDiminished = GetIntervals(2, 1, 2, 2, 1, 1, 2, 1);

        /// <summary>
        /// 'Minor Six Pentatonic' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> MinorSixPentatonic = GetIntervals(3, 2, 2, 2, 3);

        /// <summary>
        /// 'Mixolydian' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Mixolydian = GetIntervals(2, 2, 1, 2, 2, 1, 2);

        /// <summary>
        /// 'Mixolydian b6M' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> MixolydianB6M = GetIntervals(2, 2, 1, 2, 1, 2, 2);

        /// <summary>
        /// 'Mixolydian Pentatonic' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> MixolydianPentatonic = GetIntervals(4, 1, 2, 3, 2);

        /// <summary>
        /// 'Mystery #1' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Mystery1 = GetIntervals(1, 3, 2, 2, 2, 2);

        /// <summary>
        /// 'Neopolitan' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Neopolitan = GetIntervals(1, 2, 2, 2, 1, 3, 1);

        /// <summary>
        /// 'Neopolitan Major' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> NeopolitanMajor = GetIntervals(1, 2, 2, 2, 2, 2, 1);

        /// <summary>
        /// 'Neopolitan Major Pentatonic' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> NeopolitanMajorPentatonic = GetIntervals(4, 1, 1, 4, 2);

        /// <summary>
        /// 'Neopolitan Minor' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> NeopolitanMinor = GetIntervals(1, 2, 2, 2, 1, 3, 1);

        /// <summary>
        /// 'Oriental' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Oriental = GetIntervals(1, 3, 1, 1, 3, 1, 2);

        /// <summary>
        /// 'Pelog' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Pelog = GetIntervals(1, 2, 4, 1, 4);

        /// <summary>
        /// 'Pentatonic' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Pentatonic = GetIntervals(2, 2, 3, 2, 3);

        /// <summary>
        /// 'Persian' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Persian = GetIntervals(1, 3, 1, 1, 2, 3, 1);

        /// <summary>
        /// 'Phrygian' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Phrygian = GetIntervals(1, 2, 2, 2, 1, 2, 2);

        /// <summary>
        /// 'Phrygian Major' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> PhrygianMajor = GetIntervals(1, 3, 1, 2, 1, 2, 2);

        /// <summary>
        /// 'Piongio' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Piongio = GetIntervals(2, 3, 2, 2, 1, 2);

        /// <summary>
        /// 'Pomeroy' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Pomeroy = GetIntervals(1, 2, 1, 2, 2, 2, 2);

        /// <summary>
        /// 'Prometheus' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Prometheus = GetIntervals(2, 2, 2, 3, 1, 2);

        /// <summary>
        /// 'Prometheus Neopolitan' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> PrometheusNeopolitan = GetIntervals(1, 3, 2, 3, 1, 2);

        /// <summary>
        /// 'Purvi Raga' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> PurviRaga = GetIntervals(1, 3, 1, 1, 1, 1, 3, 1);

        /// <summary>
        /// 'Ritusen' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Ritusen = GetIntervals(2, 3, 2, 2, 3);

        /// <summary>
        /// 'Romanian Minor' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> RomanianMinor = GetIntervals(2, 1, 3, 1, 2, 1, 2);

        /// <summary>
        /// 'Scriabin' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Scriabin = GetIntervals(1, 3, 3, 2, 3);

        /// <summary>
        /// 'Six Tone Symmetric' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> SixToneSymmetric = GetIntervals(1, 3, 1, 3, 1, 3);

        /// <summary>
        /// 'Spanish' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Spanish = GetIntervals(1, 3, 1, 2, 1, 2, 2);

        /// <summary>
        /// 'Spanish Heptatonic' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> SpanishHeptatonic = GetIntervals(1, 2, 1, 1, 2, 1, 2, 2);

        /// <summary>
        /// 'Super Locrian' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> SuperLocrian = GetIntervals(1, 2, 1, 2, 2, 2, 2);

        /// <summary>
        /// 'Super Locrian Pentatonic' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> SuperLocrianPentatonic = GetIntervals(3, 1, 2, 4, 2);

        /// <summary>
        /// 'Todi Raga' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> TodiRaga = GetIntervals(1, 2, 3, 1, 1, 3, 1);

        /// <summary>
        /// 'Vietnamese 1' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Vietnamese1 = GetIntervals(3, 2, 2, 1, 4);

        /// <summary>
        /// 'Vietnamese 2' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> Vietnamese2 = GetIntervals(3, 2, 2, 3, 2);

        /// <summary>
        /// 'Whole Tone' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> WholeTone = GetIntervals(2, 2, 2, 2, 2, 2);

        /// <summary>
        /// 'Whole Tone Pentatonic' scale's intervals sequence.
        /// </summary>
        public static readonly ICollection<Interval> WholeTonePentatonic = GetIntervals(4, 2, 2, 2, 2);

        internal static readonly Dictionary<string, ICollection<Interval>> ScalesByName = new(StringComparer.InvariantCultureIgnoreCase)
        {
            { "aeolian", Aeolian },
            { "altered", Altered },
            { "arabian", Arabian },
            { "augmented", Augmented },
            { "augmented heptatonic", AugmentedHeptatonic },
            { "balinese", Balinese },
            { "bebop", Bebop },
            { "bebop dominant", BebopDominant },
            { "bebop locrian", BebopLocrian },
            { "bebop major", BebopMajor },
            { "bebop minor", BebopMinor },
            { "blues", Blues },
            { "chinese", Chinese },
            { "chromatic", Chromatic },
            { "composite blues", CompositeBlues },
            { "diminished", Diminished },
            { "diminished whole tone", DiminishedWholeTone },
            { "dominant", Dominant },
            { "dorian", Dorian },
            { "dorian #4", Dorian4 },
            { "dorian b2", DorianB2 },
            { "double harmonic lydian", DoubleHarmonicLydian },
            { "double harmonic major", DoubleHarmonicMajor },
            { "egyptian", Egyptian },
            { "enigmatic", Enigmatic },
            { "flamenco", Flamenco },
            { "flat six pentatonic", FlatSixPentatonic },
            { "flat three pentatonic", FlatThreePentatonic },
            { "gypsy", Gypsy },
            { "harmonic major", HarmonicMajor },
            { "harmonic minor", HarmonicMinor },
            { "hindu", Hindu },
            { "hirajoshi", Hirajoshi },
            { "hungarian major", HungarianMajor },
            { "hungarian minor", HungarianMinor },
            { "ichikosucho", Ichikosucho },
            { "in-sen", InSen },
            { "indian", Indian },
            { "ionian", Ionian },
            { "ionian augmented", IonianAugmented },
            { "ionian pentatonic", IonianPentatonic },
            { "iwato", Iwato },
            { "kafi raga", KafiRaga },
            { "kumoi", Kumoi },
            { "kumoijoshi", Kumoijoshi },
            { "leading whole tone", LeadingWholeTone },
            { "locrian", Locrian },
            { "locrian #2", Locrian2 },
            { "locrian major", LocrianMajor },
            { "locrian pentatonic", LocrianPentatonic },
            { "lydian", Lydian },
            { "lydian #5p pentatonic", Lydian5PPentatonic },
            { "lydian #9", Lydian9 },
            { "lydian augmented", LydianAugmented },
            { "lydian b7", LydianB7 },
            { "lydian diminished", LydianDiminished },
            { "lydian dominant", LydianDominant },
            { "lydian dominant pentatonic", LydianDominantPentatonic },
            { "lydian minor", LydianMinor },
            { "lydian pentatonic", LydianPentatonic },
            { "major", Major },
            { "major blues", MajorBlues },
            { "major flat two pentatonic", MajorFlatTwoPentatonic },
            { "major pentatonic", MajorPentatonic },
            { "malkos raga", MalkosRaga },
            { "melodic minor", MelodicMinor },
            { "melodic minor fifth mode", MelodicMinorFifthMode },
            { "melodic minor second mode", MelodicMinorSecondMode },
            { "minor", Minor },
            { "minor #7m pentatonic", Minor7MPentatonic },
            { "minor bebop", MinorBebop },
            { "minor blues", MinorBlues },
            { "minor hexatonic", MinorHexatonic },
            { "minor pentatonic", MinorPentatonic },
            { "minor seven flat five pentatonic", MinorSevenFlatFivePentatonic },
            { "minor six diminished", MinorSixDiminished },
            { "minor six pentatonic", MinorSixPentatonic },
            { "mixolydian", Mixolydian },
            { "mixolydian b6m", MixolydianB6M },
            { "mixolydian pentatonic", MixolydianPentatonic },
            { "mystery #1", Mystery1 },
            { "neopolitan", Neopolitan },
            { "neopolitan major", NeopolitanMajor },
            { "neopolitan major pentatonic", NeopolitanMajorPentatonic },
            { "neopolitan minor", NeopolitanMinor },
            { "oriental", Oriental },
            { "pelog", Pelog },
            { "pentatonic", Pentatonic },
            { "persian", Persian },
            { "phrygian", Phrygian },
            { "phrygian major", PhrygianMajor },
            { "piongio", Piongio },
            { "pomeroy", Pomeroy },
            { "prometheus", Prometheus },
            { "prometheus neopolitan", PrometheusNeopolitan },
            { "purvi raga", PurviRaga },
            { "ritusen", Ritusen },
            { "romanian minor", RomanianMinor },
            { "scriabin", Scriabin },
            { "six tone symmetric", SixToneSymmetric },
            { "spanish", Spanish },
            { "spanish heptatonic", SpanishHeptatonic },
            { "super locrian", SuperLocrian },
            { "super locrian pentatonic", SuperLocrianPentatonic },
            { "todi raga", TodiRaga },
            { "vietnamese 1", Vietnamese1 },
            { "vietnamese 2", Vietnamese2 },
            { "whole tone", WholeTone },
            { "whole tone pentatonic", WholeTonePentatonic }
        };

        internal static readonly string[] BNames = ScalesByName
            .Keys
            .Where(n => n.StartsWith("b", StringComparison.InvariantCultureIgnoreCase))
            .ToArray();

        internal static readonly string[] FlatNames = ScalesByName
            .Keys
            .Where(n => n.StartsWith("flat", StringComparison.InvariantCultureIgnoreCase))
            .ToArray();

        #endregion

        #region Methods

        /// <summary>
        /// Gets musical scale's intervals sequence by the scale's name.
        /// </summary>
        /// <param name="name">The name of a scale.</param>
        /// <returns>Intervals sequence for the scale with the name <paramref name="name"/>; or <c>null</c> if
        /// there is no a scale with this name.</returns>
        /// <exception cref="ArgumentException"><paramref name="name"/> is <c>null</c> or contains white-spaces only.</exception>
        public static ICollection<Interval>? GetByName(string name)
        {
            ThrowIfArgument.IsNullOrWhiteSpaceString(nameof(name), name, "Scale's name");

            return ScalesByName.TryGetValue(name, out var intervals)
                ? intervals
                : null;
        }

        private static ICollection<Interval> GetIntervals(params int[] intervalsInHalfSteps)
        {
            return intervalsInHalfSteps.Select(Interval.FromHalfSteps).ToArray();
        }

        #endregion
    }
}
