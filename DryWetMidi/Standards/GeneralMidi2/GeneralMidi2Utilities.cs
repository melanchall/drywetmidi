using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;

namespace Melanchall.DryWetMidi.Standards
{
    /// <summary>
    /// Provides utilities for General MIDI Level 2.
    /// </summary>
    public static class GeneralMidi2Utilities
    {
        #region Nested classes

        private sealed class GeneralMidi2ProgramData
        {
            #region Constructor

            public GeneralMidi2ProgramData(GeneralMidi2Program generalMidi2Program, GeneralMidiProgram generalMidiProgram, SevenBitNumber bankMsb, SevenBitNumber bankLsb)
            {
                GeneralMidi2Program = generalMidi2Program;
                GeneralMidiProgram = generalMidiProgram;
                BankMsb = bankMsb;
                BankLsb = bankLsb;
            }

            #endregion

            #region Properties

            public GeneralMidi2Program GeneralMidi2Program { get; }

            public GeneralMidiProgram GeneralMidiProgram { get; }

            public SevenBitNumber BankMsb { get; }

            public SevenBitNumber BankLsb { get; }

            #endregion
        }

        #endregion

        #region Constants

        private const byte MelodyChannelBankMsb = 0x79;
        private const byte RhythmChannelBankMsb = 0x78;

        private static readonly Dictionary<GeneralMidi2Program, GeneralMidi2ProgramData> ProgramsData = new[]
        {
            // Piano

            GetProgramsData(GeneralMidiProgram.AcousticGrandPiano,
                GeneralMidi2Program.AcousticGrandPiano,
                GeneralMidi2Program.AcousticGrandPianoWide,
                GeneralMidi2Program.AcousticGrandPianoDark),
            GetProgramsData(GeneralMidiProgram.BrightAcousticPiano,
                GeneralMidi2Program.BrightAcousticPiano,
                GeneralMidi2Program.BrightAcousticPianoWide),
            GetProgramsData(GeneralMidiProgram.ElectricGrandPiano,
                GeneralMidi2Program.ElectricGrandPiano,
                GeneralMidi2Program.ElectricGrandPianoWide),
            GetProgramsData(GeneralMidiProgram.HonkyTonkPiano,
                GeneralMidi2Program.HonkyTonkPiano,
                GeneralMidi2Program.HonkyTonkPianoWide),
            GetProgramsData(GeneralMidiProgram.ElectricPiano1,
                GeneralMidi2Program.ElectricPiano1,
                GeneralMidi2Program.DetunedElectricPiano1,
                GeneralMidi2Program.ElectricPiano1VelocityMix,
                GeneralMidi2Program.SixtiesElectricPiano),
            GetProgramsData(GeneralMidiProgram.ElectricPiano2,
                GeneralMidi2Program.ElectricPiano2,
                GeneralMidi2Program.DetunedElectricPiano2,
                GeneralMidi2Program.ElectricPiano2VelocityMix,
                GeneralMidi2Program.EpLegend,
                GeneralMidi2Program.EpPhase),
            GetProgramsData(GeneralMidiProgram.Harpsichord,
                GeneralMidi2Program.Harpsichord,
                GeneralMidi2Program.HarpsichordOctaveMix,
                GeneralMidi2Program.HarpsichordWide,
                GeneralMidi2Program.HarpsichordWithKeyOff),
            GetProgramsData(GeneralMidiProgram.Clavi,
                GeneralMidi2Program.Clavi,
                GeneralMidi2Program.PulseClavi),

            // Chromatic Percussion

            GetProgramsData(GeneralMidiProgram.Celesta,
                GeneralMidi2Program.Celesta),
            GetProgramsData(GeneralMidiProgram.Glockenspiel,
                GeneralMidi2Program.Glockenspiel),
            GetProgramsData(GeneralMidiProgram.MusicBox,
                GeneralMidi2Program.MusicBox),
            GetProgramsData(GeneralMidiProgram.Vibraphone,
                GeneralMidi2Program.Vibraphone,
                GeneralMidi2Program.VibraphoneWide),
            GetProgramsData(GeneralMidiProgram.Marimba,
                GeneralMidi2Program.Marimba,
                GeneralMidi2Program.MarimbaWide),
            GetProgramsData(GeneralMidiProgram.Xylophone,
                GeneralMidi2Program.Xylophone),
            GetProgramsData(GeneralMidiProgram.TubularBells,
                GeneralMidi2Program.TubularBells,
                GeneralMidi2Program.ChurchBell,
                GeneralMidi2Program.Carillon),
            GetProgramsData(GeneralMidiProgram.Dulcimer,
                GeneralMidi2Program.Dulcimer),

            // Organ

            GetProgramsData(GeneralMidiProgram.DrawbarOrgan,
                GeneralMidi2Program.DrawbarOrgan,
                GeneralMidi2Program.DetunedDrawbarOrgan,
                GeneralMidi2Program.ItalianSixtiesOrgan,
                GeneralMidi2Program.DrawbarOrgan2),
            GetProgramsData(GeneralMidiProgram.PercussiveOrgan,
                GeneralMidi2Program.PercussiveOrgan,
                GeneralMidi2Program.DetunedPercussiveOrgan,
                GeneralMidi2Program.PercussiveOrgan2),
            GetProgramsData(GeneralMidiProgram.RockOrgan,
                GeneralMidi2Program.RockOrgan),
            GetProgramsData(GeneralMidiProgram.ChurchOrgan,
                GeneralMidi2Program.ChurchOrgan,
                GeneralMidi2Program.ChurchOrganOctaveMix,
                GeneralMidi2Program.DetunedChurchOrgan),
            GetProgramsData(GeneralMidiProgram.ReedOrgan,
                GeneralMidi2Program.ReedOrgan,
                GeneralMidi2Program.PuffOrgan),
            GetProgramsData(GeneralMidiProgram.Accordion,
                GeneralMidi2Program.Accordion,
                GeneralMidi2Program.Accordion2),
            GetProgramsData(GeneralMidiProgram.Harmonica,
                GeneralMidi2Program.Harmonica),
            GetProgramsData(GeneralMidiProgram.TangoAccordion,
                GeneralMidi2Program.TangoAccordion),

            // Guitar

            GetProgramsData(GeneralMidiProgram.AcousticGuitar1,
                GeneralMidi2Program.AcousticGuitarNylon,
                GeneralMidi2Program.Ukulele,
                GeneralMidi2Program.AcousticGuitarNylonKeyOff,
                GeneralMidi2Program.AcousticGuitarNylon2),
            GetProgramsData(GeneralMidiProgram.AcousticGuitar2,
                GeneralMidi2Program.AcousticGuitarSteel,
                GeneralMidi2Program.TwelveStringsGuitar,
                GeneralMidi2Program.Mandolin,
                GeneralMidi2Program.SteelGuitarWithBodySound),
            GetProgramsData(GeneralMidiProgram.ElectricGuitar1,
                GeneralMidi2Program.ElectricGuitarJazz,
                GeneralMidi2Program.ElectricGuitarPedalSteel),
            GetProgramsData(GeneralMidiProgram.ElectricGuitar2,
                GeneralMidi2Program.ElectricGuitarClean,
                GeneralMidi2Program.ElectricGuitarDetunedClean,
                GeneralMidi2Program.MidToneGuitar),
            GetProgramsData(GeneralMidiProgram.ElectricGuitar3,
                GeneralMidi2Program.ElectricGuitarMuted,
                GeneralMidi2Program.ElectricGuitarFunkyCutting,
                GeneralMidi2Program.ElectricGuitarMutedVeloSw,
                GeneralMidi2Program.JazzMan),
            GetProgramsData(GeneralMidiProgram.OverdrivenGuitar,
                GeneralMidi2Program.OverdrivenGuitar,
                GeneralMidi2Program.GuitarPinch),
            GetProgramsData(GeneralMidiProgram.DistortionGuitar,
                GeneralMidi2Program.DistortionGuitar,
                GeneralMidi2Program.DistortionGuitarWithFeedback,
                GeneralMidi2Program.DistortedRhythmGuitar),
            GetProgramsData(GeneralMidiProgram.GuitarHarmonics,
                GeneralMidi2Program.GuitarHarmonics,
                GeneralMidi2Program.GuitarFeedback),

            // Bass

            GetProgramsData(GeneralMidiProgram.AcousticBass,
                GeneralMidi2Program.AcousticBass),
            GetProgramsData(GeneralMidiProgram.ElectricBass1,
                GeneralMidi2Program.ElectricBassFinger,
                GeneralMidi2Program.FingerSlapBass),
            GetProgramsData(GeneralMidiProgram.ElectricBass2,
                GeneralMidi2Program.ElectricBassPick),
            GetProgramsData(GeneralMidiProgram.FretlessBass,
                GeneralMidi2Program.FretlessBass),
            GetProgramsData(GeneralMidiProgram.SlapBass1,
                GeneralMidi2Program.SlapBass1),
            GetProgramsData(GeneralMidiProgram.SlapBass2,
                GeneralMidi2Program.SlapBass2),
            GetProgramsData(GeneralMidiProgram.SynthBass1,
                GeneralMidi2Program.SynthBass1,
                GeneralMidi2Program.SynthBassWarm,
                GeneralMidi2Program.SynthBass3Resonance,
                GeneralMidi2Program.ClaviBass,
                GeneralMidi2Program.Hammer),
            GetProgramsData(GeneralMidiProgram.SynthBass2,
                GeneralMidi2Program.SynthBass2,
                GeneralMidi2Program.SynthBass4Attack,
                GeneralMidi2Program.SynthBassRubber,
                GeneralMidi2Program.AttackPulse),

            // Strings & Orchestral instruments

            GetProgramsData(GeneralMidiProgram.Violin,
                GeneralMidi2Program.Violin,
                GeneralMidi2Program.ViolinSlowAttack),
            GetProgramsData(GeneralMidiProgram.Viola,
                GeneralMidi2Program.Viola),
            GetProgramsData(GeneralMidiProgram.Cello,
                GeneralMidi2Program.Cello),
            GetProgramsData(GeneralMidiProgram.Contrabass,
                GeneralMidi2Program.Contrabass),
            GetProgramsData(GeneralMidiProgram.TremoloStrings,
                GeneralMidi2Program.TremoloStrings),
            GetProgramsData(GeneralMidiProgram.PizzicatoStrings,
                GeneralMidi2Program.PizzicatoStrings),
            GetProgramsData(GeneralMidiProgram.OrchestralHarp,
                GeneralMidi2Program.OrchestralHarp,
                GeneralMidi2Program.YangChin),
            GetProgramsData(GeneralMidiProgram.Timpani,
                GeneralMidi2Program.Timpani),

            // Ensemble

            GetProgramsData(GeneralMidiProgram.StringEnsemble1,
                GeneralMidi2Program.StringEnsembles1,
                GeneralMidi2Program.StringsAndBrass,
                GeneralMidi2Program.SixtiesStrings),
            GetProgramsData(GeneralMidiProgram.StringEnsemble2,
                GeneralMidi2Program.StringEnsembles2),
            GetProgramsData(GeneralMidiProgram.SynthStrings1,
                GeneralMidi2Program.SynthStrings1,
                GeneralMidi2Program.SynthStrings3),
            GetProgramsData(GeneralMidiProgram.SynthStrings2,
                GeneralMidi2Program.SynthStrings2),
            GetProgramsData(GeneralMidiProgram.ChoirAahs,
                GeneralMidi2Program.ChoirAahs,
                GeneralMidi2Program.ChoirAahs2),
            GetProgramsData(GeneralMidiProgram.VoiceOohs,
                GeneralMidi2Program.VoiceOohs,
                GeneralMidi2Program.Humming),
            GetProgramsData(GeneralMidiProgram.SynthVoice,
                GeneralMidi2Program.SynthVoice,
                GeneralMidi2Program.AnalogVoice),
            GetProgramsData(GeneralMidiProgram.OrchestraHit,
                GeneralMidi2Program.OrchestraHit,
                GeneralMidi2Program.BassHitPlus,
                GeneralMidi2Program.SixthHit,
                GeneralMidi2Program.EuroHit),

            // Brass

            GetProgramsData(GeneralMidiProgram.Trumpet,
                GeneralMidi2Program.Trumpet,
                GeneralMidi2Program.DarkTrumpetSoft),
            GetProgramsData(GeneralMidiProgram.Trombone,
                GeneralMidi2Program.Trombone,
                GeneralMidi2Program.Trombone2,
                GeneralMidi2Program.BrightTrombone),
            GetProgramsData(GeneralMidiProgram.Tuba,
                GeneralMidi2Program.Tuba),
            GetProgramsData(GeneralMidiProgram.MutedTrumpet,
                GeneralMidi2Program.MutedTrumpet,
                GeneralMidi2Program.MutedTrumpet2),
            GetProgramsData(GeneralMidiProgram.FrenchHorn,
                GeneralMidi2Program.FrenchHorn,
                GeneralMidi2Program.FrenchHorn2Warm),
            GetProgramsData(GeneralMidiProgram.BrassSection,
                GeneralMidi2Program.BrassSection,
                GeneralMidi2Program.BrassSection2OctaveMix),
            GetProgramsData(GeneralMidiProgram.SynthBrass1,
                GeneralMidi2Program.SynthBrass1,
                GeneralMidi2Program.SynthBrass3,
                GeneralMidi2Program.AnalogSynthBrass1,
                GeneralMidi2Program.JumpBrass),
            GetProgramsData(GeneralMidiProgram.SynthBrass2,
                GeneralMidi2Program.SynthBrass2,
                GeneralMidi2Program.SynthBrass4,
                GeneralMidi2Program.AnalogSynthBrass2),

            // Reed

            GetProgramsData(GeneralMidiProgram.SopranoSax,
                GeneralMidi2Program.SopranoSax),
            GetProgramsData(GeneralMidiProgram.AltoSax,
                GeneralMidi2Program.AltoSax),
            GetProgramsData(GeneralMidiProgram.TenorSax,
                GeneralMidi2Program.TenorSax),
            GetProgramsData(GeneralMidiProgram.BaritoneSax,
                GeneralMidi2Program.BaritoneSax),
            GetProgramsData(GeneralMidiProgram.Oboe,
                GeneralMidi2Program.Oboe),
            GetProgramsData(GeneralMidiProgram.EnglishHorn,
                GeneralMidi2Program.EnglishHorn),
            GetProgramsData(GeneralMidiProgram.Bassoon,
                GeneralMidi2Program.Bassoon),
            GetProgramsData(GeneralMidiProgram.Clarinet,
                GeneralMidi2Program.Clarinet),

            // Pipe

            GetProgramsData(GeneralMidiProgram.Piccolo,
                GeneralMidi2Program.Piccolo),
            GetProgramsData(GeneralMidiProgram.Flute,
                GeneralMidi2Program.Flute),
            GetProgramsData(GeneralMidiProgram.Recorder,
                GeneralMidi2Program.Recorder),
            GetProgramsData(GeneralMidiProgram.PanFlute,
                GeneralMidi2Program.PanFlute),
            GetProgramsData(GeneralMidiProgram.BlownBottle,
                GeneralMidi2Program.BlownBottle),
            GetProgramsData(GeneralMidiProgram.Shakuhachi,
                GeneralMidi2Program.Shakuhachi),
            GetProgramsData(GeneralMidiProgram.Whistle,
                GeneralMidi2Program.Whistle),
            GetProgramsData(GeneralMidiProgram.Ocarina,
                GeneralMidi2Program.Ocarina),

            // Synth Lead

            GetProgramsData(GeneralMidiProgram.Lead1,
                GeneralMidi2Program.Lead1Square,
                GeneralMidi2Program.Lead1ASquare2,
                GeneralMidi2Program.Lead1BSine),
            GetProgramsData(GeneralMidiProgram.Lead2,
                GeneralMidi2Program.Lead2Sawtooth,
                GeneralMidi2Program.Lead2ASawtooth2,
                GeneralMidi2Program.Lead2BSawPulse,
                GeneralMidi2Program.Lead2CDoubleSawtooth,
                GeneralMidi2Program.Lead2DSequencedAnalog),
            GetProgramsData(GeneralMidiProgram.Lead3,
                GeneralMidi2Program.Lead3Calliope),
            GetProgramsData(GeneralMidiProgram.Lead4,
                GeneralMidi2Program.Lead4Chiff),
            GetProgramsData(GeneralMidiProgram.Lead5,
                GeneralMidi2Program.Lead5Charang,
                GeneralMidi2Program.Lead5AWireLead),
            GetProgramsData(GeneralMidiProgram.Lead6,
                GeneralMidi2Program.Lead6Voice),
            GetProgramsData(GeneralMidiProgram.Lead7,
                GeneralMidi2Program.Lead7Fifths),
            GetProgramsData(GeneralMidiProgram.Lead8,
                GeneralMidi2Program.Lead8BassLead,
                GeneralMidi2Program.Lead8ASoftWrl),

            // Synth Pad

            GetProgramsData(GeneralMidiProgram.Pad1,
                GeneralMidi2Program.Pad1NewAge),
            GetProgramsData(GeneralMidiProgram.Pad2,
                GeneralMidi2Program.Pad2Warm,
                GeneralMidi2Program.Pad2ASinePad),
            GetProgramsData(GeneralMidiProgram.Pad3,
                GeneralMidi2Program.Pad3Polysynth),
            GetProgramsData(GeneralMidiProgram.Pad4,
                GeneralMidi2Program.Pad4Choir,
                GeneralMidi2Program.Pad4AItopia),
            GetProgramsData(GeneralMidiProgram.Pad5,
                GeneralMidi2Program.Pad5Bowed),
            GetProgramsData(GeneralMidiProgram.Pad6,
                GeneralMidi2Program.Pad6Metallic),
            GetProgramsData(GeneralMidiProgram.Pad7,
                GeneralMidi2Program.Pad7Halo),
            GetProgramsData(GeneralMidiProgram.Pad8,
                GeneralMidi2Program.Pad8Sweep),

            // Synth SFX

            GetProgramsData(GeneralMidiProgram.Fx1,
                GeneralMidi2Program.Fx1Rain),
            GetProgramsData(GeneralMidiProgram.Fx2,
                GeneralMidi2Program.Fx2Soundtrack),
            GetProgramsData(GeneralMidiProgram.Fx3,
                GeneralMidi2Program.Fx3Crystal,
                GeneralMidi2Program.Fx3ASynthMallet),
            GetProgramsData(GeneralMidiProgram.Fx4,
                GeneralMidi2Program.Fx4Atmosphere),
            GetProgramsData(GeneralMidiProgram.Fx5,
                GeneralMidi2Program.Fx5Brightness),
            GetProgramsData(GeneralMidiProgram.Fx6,
                GeneralMidi2Program.Fx6Goblins),
            GetProgramsData(GeneralMidiProgram.Fx7,
                GeneralMidi2Program.Fx7Echoes,
                GeneralMidi2Program.Fx7AEchoBell,
                GeneralMidi2Program.Fx7BEchoPan),
            GetProgramsData(GeneralMidiProgram.Fx8,
                GeneralMidi2Program.Fx8SciFi),

            // Ethnic Misc.

            GetProgramsData(GeneralMidiProgram.Sitar,
                GeneralMidi2Program.Sitar,
                GeneralMidi2Program.Sitar2Bend),
            GetProgramsData(GeneralMidiProgram.Banjo,
                GeneralMidi2Program.Banjo),
            GetProgramsData(GeneralMidiProgram.Shamisen,
                GeneralMidi2Program.Shamisen),
            GetProgramsData(GeneralMidiProgram.Koto,
                GeneralMidi2Program.Koto,
                GeneralMidi2Program.TaishoKoto),
            GetProgramsData(GeneralMidiProgram.Kalimba,
                GeneralMidi2Program.Kalimba),
            GetProgramsData(GeneralMidiProgram.BagPipe,
                GeneralMidi2Program.BagPipe),
            GetProgramsData(GeneralMidiProgram.Fiddle,
                GeneralMidi2Program.Fiddle),
            GetProgramsData(GeneralMidiProgram.Shanai,
                GeneralMidi2Program.Shanai),

            // Percussive

            GetProgramsData(GeneralMidiProgram.TinkleBell,
                GeneralMidi2Program.TinkleBell),
            GetProgramsData(GeneralMidiProgram.Agogo,
                GeneralMidi2Program.Agogo),
            GetProgramsData(GeneralMidiProgram.SteelDrums,
                GeneralMidi2Program.SteelDrums),
            GetProgramsData(GeneralMidiProgram.Woodblock,
                GeneralMidi2Program.Woodblock,
                GeneralMidi2Program.Castanets),
            GetProgramsData(GeneralMidiProgram.TaikoDrum,
                GeneralMidi2Program.TaikoDrum,
                GeneralMidi2Program.ConcertBassDrum),
            GetProgramsData(GeneralMidiProgram.MelodicTom,
                GeneralMidi2Program.MelodicTom,
                GeneralMidi2Program.MelodicTom2Power),
            GetProgramsData(GeneralMidiProgram.SynthDrum,
                GeneralMidi2Program.SynthDrum,
                GeneralMidi2Program.RhythmBoxTom,
                GeneralMidi2Program.ElectricDrum),
            GetProgramsData(GeneralMidiProgram.ReverseCymbal,
                GeneralMidi2Program.ReverseCymbal),

            // SFX

            GetProgramsData(GeneralMidiProgram.GuitarFretNoise,
                GeneralMidi2Program.GuitarFretNoise,
                GeneralMidi2Program.GuitarCuttingNoise,
                GeneralMidi2Program.AcousticBassStringSlap),
            GetProgramsData(GeneralMidiProgram.BreathNoise,
                GeneralMidi2Program.BreathNoise,
                GeneralMidi2Program.FluteKeyClick),
            GetProgramsData(GeneralMidiProgram.Seashore,
                GeneralMidi2Program.Seashore,
                GeneralMidi2Program.Rain,
                GeneralMidi2Program.Thunder,
                GeneralMidi2Program.Wind,
                GeneralMidi2Program.Stream,
                GeneralMidi2Program.Bubble),
            GetProgramsData(GeneralMidiProgram.BirdTweet,
                GeneralMidi2Program.BirdTweet,
                GeneralMidi2Program.Dog,
                GeneralMidi2Program.HorseGallop,
                GeneralMidi2Program.BirdTweet2),
            GetProgramsData(GeneralMidiProgram.TelephoneRing,
                GeneralMidi2Program.TelephoneRing,
                GeneralMidi2Program.TelephoneRing2,
                GeneralMidi2Program.DoorCreaking,
                GeneralMidi2Program.Door,
                GeneralMidi2Program.Scratch,
                GeneralMidi2Program.WindChime),
            GetProgramsData(GeneralMidiProgram.Helicopter,
                GeneralMidi2Program.Helicopter,
                GeneralMidi2Program.CarEngine,
                GeneralMidi2Program.CarStop,
                GeneralMidi2Program.CarPass,
                GeneralMidi2Program.CarCrash,
                GeneralMidi2Program.Siren,
                GeneralMidi2Program.Train,
                GeneralMidi2Program.Jetplane,
                GeneralMidi2Program.Starship,
                GeneralMidi2Program.BurstNoise),
            GetProgramsData(GeneralMidiProgram.Applause,
                GeneralMidi2Program.Applause,
                GeneralMidi2Program.Laughing,
                GeneralMidi2Program.Screaming,
                GeneralMidi2Program.Punch,
                GeneralMidi2Program.HeartBeat,
                GeneralMidi2Program.Footsteps),
            GetProgramsData(GeneralMidiProgram.Gunshot,
                GeneralMidi2Program.Gunshot,
                GeneralMidi2Program.MachineGun,
                GeneralMidi2Program.Lasergun,
                GeneralMidi2Program.Explosion)
        }
        .SelectMany(d => d)
        .ToDictionary(d => d.GeneralMidi2Program,
                      d => d);

        #endregion

        #region Methods

        /// <summary>
        /// Gets MIDI events sequence to switch to the specified General MIDI Level 2 program.
        /// </summary>
        /// <param name="program"><see cref="GeneralMidi2Program"/> to get events for.</param>
        /// <param name="channel">Channel events should be created for.</param>
        /// <returns>MIDI events sequence to switch to the <paramref name="program"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="program"/> specified an invalid value.</exception>
        public static IEnumerable<MidiEvent> GetProgramEvents(this GeneralMidi2Program program, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(program), program);

            var programData = ProgramsData[program];

            return new[]
            {
                ControlName.BankSelect.GetControlChangeEvent(programData.BankMsb, channel),
                ControlName.LsbForBankSelect.GetControlChangeEvent(programData.BankLsb, channel),
                programData.GeneralMidiProgram.GetProgramEvent(channel)
            };
        }

        /// <summary>
        /// Gets MIDI events sequence to switch to the specified General MIDI Level 2 percussion set.
        /// </summary>
        /// <param name="percussionSet"><see cref="GeneralMidi2PercussionSet"/> to get events for.</param>
        /// <param name="channel">Channel events should be created for.</param>
        /// <returns>MIDI events sequence to switch to the <paramref name="percussionSet"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussionSet"/> specified an invalid value.</exception>
        public static IEnumerable<MidiEvent> GetPercussionSetEvents(this GeneralMidi2PercussionSet percussionSet, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussionSet), percussionSet);

            return new[]
            {
                ControlName.BankSelect.GetControlChangeEvent((SevenBitNumber)RhythmChannelBankMsb, channel),
                ControlName.LsbForBankSelect.GetControlChangeEvent((SevenBitNumber)0, channel),
                percussionSet.GetProgramEvent(channel)
            };
        }

        /// <summary>
        /// Gets Program Change event corresponding to the specified General MIDI Level 2 percussion set.
        /// </summary>
        /// <param name="percussionSet"><see cref="GeneralMidi2PercussionSet"/> to get event for.</param>
        /// <param name="channel">Channel event should be created for.</param>
        /// <returns>Program Change event corresponding to the <paramref name="percussionSet"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussionSet"/> specified an invalid value.</exception>
        public static MidiEvent GetProgramEvent(this GeneralMidi2PercussionSet percussionSet, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussionSet), percussionSet);

            return new ProgramChangeEvent(percussionSet.AsSevenBitNumber()) { Channel = channel };
        }

        /// <summary>
        /// Converts <see cref="GeneralMidi2PercussionSet"/> to the corresponding value of the
        /// <see cref="SevenBitNumber"/> type.
        /// </summary>
        /// <param name="percussionSet"><see cref="GeneralMidi2PercussionSet"/> to convert to <see cref="SevenBitNumber"/>.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the <paramref name="percussionSet"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussionSet"/> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this GeneralMidi2PercussionSet percussionSet)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussionSet), percussionSet);

            return (SevenBitNumber)(byte)percussionSet;
        }

        /// <summary>
        /// Converts <see cref="GeneralMidi2AnalogPercussion"/> to the corresponding value of the
        /// <see cref="SevenBitNumber"/> type.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2AnalogPercussion"/> to convert to <see cref="SevenBitNumber"/>.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this GeneralMidi2AnalogPercussion percussion)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return (SevenBitNumber)(byte)percussion;
        }

        /// <summary>
        /// Converts <see cref="GeneralMidi2BrushPercussion"/> to the corresponding value of the
        /// <see cref="SevenBitNumber"/> type.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2BrushPercussion"/> to convert to <see cref="SevenBitNumber"/>.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this GeneralMidi2BrushPercussion percussion)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return (SevenBitNumber)(byte)percussion;
        }

        /// <summary>
        /// Converts <see cref="GeneralMidi2ElectronicPercussion"/> to the corresponding value of the
        /// <see cref="SevenBitNumber"/> type.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2ElectronicPercussion"/> to convert to <see cref="SevenBitNumber"/>.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this GeneralMidi2ElectronicPercussion percussion)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return (SevenBitNumber)(byte)percussion;
        }

        /// <summary>
        /// Converts <see cref="GeneralMidi2JazzPercussion"/> to the corresponding value of the
        /// <see cref="SevenBitNumber"/> type.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2JazzPercussion"/> to convert to <see cref="SevenBitNumber"/>.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this GeneralMidi2JazzPercussion percussion)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return (SevenBitNumber)(byte)percussion;
        }

        /// <summary>
        /// Converts <see cref="GeneralMidi2OrchestraPercussion"/> to the corresponding value of the
        /// <see cref="SevenBitNumber"/> type.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2OrchestraPercussion"/> to convert to <see cref="SevenBitNumber"/>.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this GeneralMidi2OrchestraPercussion percussion)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return (SevenBitNumber)(byte)percussion;
        }

        /// <summary>
        /// Converts <see cref="GeneralMidi2PowerPercussion"/> to the corresponding value of the
        /// <see cref="SevenBitNumber"/> type.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2PowerPercussion"/> to convert to <see cref="SevenBitNumber"/>.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this GeneralMidi2PowerPercussion percussion)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return (SevenBitNumber)(byte)percussion;
        }

        /// <summary>
        /// Converts <see cref="GeneralMidi2RoomPercussion"/> to the corresponding value of the
        /// <see cref="SevenBitNumber"/> type.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2RoomPercussion"/> to convert to <see cref="SevenBitNumber"/>.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this GeneralMidi2RoomPercussion percussion)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return (SevenBitNumber)(byte)percussion;
        }

        /// <summary>
        /// Converts <see cref="GeneralMidi2SfxPercussion"/> to the corresponding value of the
        /// <see cref="SevenBitNumber"/> type.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2SfxPercussion"/> to convert to <see cref="SevenBitNumber"/>.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this GeneralMidi2SfxPercussion percussion)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return (SevenBitNumber)(byte)percussion;
        }

        /// <summary>
        /// Converts <see cref="GeneralMidi2StandardPercussion"/> to the corresponding value of the
        /// <see cref="SevenBitNumber"/> type.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2StandardPercussion"/> to convert to <see cref="SevenBitNumber"/>.</param>
        /// <returns><see cref="SevenBitNumber"/> representing the <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static SevenBitNumber AsSevenBitNumber(this GeneralMidi2StandardPercussion percussion)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return (SevenBitNumber)(byte)percussion;
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// General MIDI Level 2 'Analog' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2AnalogPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOnEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOnEvent GetNoteOnEvent(this GeneralMidi2AnalogPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOnEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// General MIDI Level 2 'Brush' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2BrushPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOnEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOnEvent GetNoteOnEvent(this GeneralMidi2BrushPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOnEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// General MIDI Level 2 'Electronic' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2ElectronicPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOnEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOnEvent GetNoteOnEvent(this GeneralMidi2ElectronicPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOnEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// General MIDI Level 2 'Jazz' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2JazzPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOnEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOnEvent GetNoteOnEvent(this GeneralMidi2JazzPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOnEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// General MIDI Level 2 'Orchestra' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2OrchestraPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOnEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOnEvent GetNoteOnEvent(this GeneralMidi2OrchestraPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOnEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// General MIDI Level 2 'Power' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2PowerPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOnEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOnEvent GetNoteOnEvent(this GeneralMidi2PowerPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOnEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// General MIDI Level 2 'Room' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2RoomPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOnEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOnEvent GetNoteOnEvent(this GeneralMidi2RoomPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOnEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// General MIDI Level 2 'SFX' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2SfxPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOnEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOnEvent GetNoteOnEvent(this GeneralMidi2SfxPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOnEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// General MIDI Level 2 'Standard' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2StandardPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOnEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOnEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOnEvent GetNoteOnEvent(this GeneralMidi2StandardPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOnEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// General MIDI Level 2 'Analog' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2AnalogPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOffEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOffEvent GetNoteOffEvent(this GeneralMidi2AnalogPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOffEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// General MIDI Level 2 'Brush' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2BrushPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOffEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOffEvent GetNoteOffEvent(this GeneralMidi2BrushPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOffEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// General MIDI Level 2 'Electronic' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2ElectronicPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOffEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOffEvent GetNoteOffEvent(this GeneralMidi2ElectronicPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOffEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// General MIDI Level 2 'Jazz' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2JazzPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOffEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOffEvent GetNoteOffEvent(this GeneralMidi2JazzPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOffEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// General MIDI Level 2 'Orchestra' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2OrchestraPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOffEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOffEvent GetNoteOffEvent(this GeneralMidi2OrchestraPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOffEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// General MIDI Level 2 'Power' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2PowerPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOffEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOffEvent GetNoteOffEvent(this GeneralMidi2PowerPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOffEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// General MIDI Level 2 'Room' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2RoomPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOffEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOffEvent GetNoteOffEvent(this GeneralMidi2RoomPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOffEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// General MIDI Level 2 'SFX' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2SfxPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOffEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOffEvent GetNoteOffEvent(this GeneralMidi2SfxPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOffEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        /// <summary>
        /// Gets an instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// General MIDI Level 2 'Standard' percussion.
        /// </summary>
        /// <param name="percussion"><see cref="GeneralMidi2StandardPercussion"/> to get an event for.</param>
        /// <param name="velocity">Velocity of the <see cref="NoteOffEvent"/>.</param>
        /// <param name="channel">Channel an event should be created for.</param>
        /// <returns>An instance of the <see cref="NoteOffEvent"/> corresponding to the specified
        /// <paramref name="percussion"/>.</returns>
        /// <exception cref="InvalidEnumArgumentException"><paramref name="percussion"/> specified an invalid value.</exception>
        public static NoteOffEvent GetNoteOffEvent(this GeneralMidi2StandardPercussion percussion, SevenBitNumber velocity, FourBitNumber channel)
        {
            ThrowIfArgument.IsInvalidEnumValue(nameof(percussion), percussion);

            return new NoteOffEvent(percussion.AsSevenBitNumber(), velocity) { Channel = channel };
        }

        private static IEnumerable<GeneralMidi2ProgramData> GetProgramsData(GeneralMidiProgram generalMidiProgram, params GeneralMidi2Program[] programs)
        {
            return programs.Select((p, i) => GetProgramData(p, generalMidiProgram, MelodyChannelBankMsb, (byte)i));
        }

        private static GeneralMidi2ProgramData GetProgramData(GeneralMidi2Program generalMidi2Program, GeneralMidiProgram generalMidiProgram, byte bankMsb, byte bankLsb)
        {
            return new GeneralMidi2ProgramData(generalMidi2Program, generalMidiProgram, (SevenBitNumber)bankMsb, (SevenBitNumber)bankLsb);
        }

        #endregion
    }
}
