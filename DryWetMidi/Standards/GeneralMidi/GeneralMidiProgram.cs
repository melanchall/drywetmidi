namespace Melanchall.DryWetMidi.Standards
{
    /// <summary>
    /// General MIDI Level 1 program (patch).
    /// </summary>
    public enum GeneralMidiProgram : byte
    {
        /// <summary>
        /// 'Acoustic Grand Piano' General MIDI program.
        /// </summary>
        AcousticGrandPiano = 0,

        /// <summary>
        /// 'Bright Acoustic Piano' General MIDI program.
        /// </summary>
        BrightAcousticPiano = 1,

        /// <summary>
        /// 'Electric Grand Piano' General MIDI program.
        /// </summary>
        ElectricGrandPiano = 2,

        /// <summary>
        /// 'Honky-tonk Piano' General MIDI program.
        /// </summary>
        HonkyTonkPiano = 3,

        /// <summary>
        /// 'Electric Piano 1' General MIDI program.
        /// </summary>
        ElectricPiano1 = 4,

        /// <summary>
        /// 'Electric Piano 2' General MIDI program.
        /// </summary>
        ElectricPiano2 = 5,

        /// <summary>
        /// 'Harpsichord' General MIDI program.
        /// </summary>
        Harpsichord = 6,

        /// <summary>
        /// 'Clavi' General MIDI program.
        /// </summary>
        Clavi = 7,

        /// <summary>
        /// 'Celesta' General MIDI program.
        /// </summary>
        Celesta = 8,

        /// <summary>
        /// 'Glockenspiel' General MIDI program.
        /// </summary>
        Glockenspiel = 9,

        /// <summary>
        /// 'Music Box' General MIDI program.
        /// </summary>
        MusicBox = 10,

        /// <summary>
        /// 'Vibraphone' General MIDI program.
        /// </summary>
        Vibraphone = 11,

        /// <summary>
        /// 'Marimba' General MIDI program.
        /// </summary>
        Marimba = 12,

        /// <summary>
        /// 'Xylophone' General MIDI program.
        /// </summary>
        Xylophone = 13,

        /// <summary>
        /// 'Tubular Bells' General MIDI program.
        /// </summary>
        TubularBells = 14,

        /// <summary>
        /// 'Dulcimer' General MIDI program.
        /// </summary>
        Dulcimer = 15,

        /// <summary>
        /// 'Drawbar Organ' General MIDI program.
        /// </summary>
        DrawbarOrgan = 16,

        /// <summary>
        /// 'Percussive Organ' General MIDI program.
        /// </summary>
        PercussiveOrgan = 17,

        /// <summary>
        /// 'Rock Organ' General MIDI program.
        /// </summary>
        RockOrgan = 18,

        /// <summary>
        /// 'Church Organ' General MIDI program.
        /// </summary>
        ChurchOrgan = 19,

        /// <summary>
        /// 'Reed Organ' General MIDI program.
        /// </summary>
        ReedOrgan = 20,

        /// <summary>
        /// 'Accordion' General MIDI program.
        /// </summary>
        Accordion = 21,

        /// <summary>
        /// 'Harmonica' General MIDI program.
        /// </summary>
        Harmonica = 22,

        /// <summary>
        /// 'Tango Accordion' General MIDI program.
        /// </summary>
        TangoAccordion = 23,

        /// <summary>
        /// 'Acoustic Guitar (nylon)' General MIDI program.
        /// </summary>
        AcousticGuitar1 = 24,

        /// <summary>
        /// 'Acoustic Guitar (steel)' General MIDI program.
        /// </summary>
        AcousticGuitar2 = 25,

        /// <summary>
        /// 'Electric Guitar (jazz)' General MIDI program.
        /// </summary>
        ElectricGuitar1 = 26,

        /// <summary>
        /// 'Electric Guitar (clean)' General MIDI program.
        /// </summary>
        ElectricGuitar2 = 27,

        /// <summary>
        /// 'Electric Guitar (muted)' General MIDI program.
        /// </summary>
        ElectricGuitar3 = 28,

        /// <summary>
        /// 'Overdriven Guitar' General MIDI program.
        /// </summary>
        OverdrivenGuitar = 29,

        /// <summary>
        /// 'Distortion Guitar' General MIDI program.
        /// </summary>
        DistortionGuitar = 30,

        /// <summary>
        /// 'Guitar harmonics' General MIDI program.
        /// </summary>
        GuitarHarmonics = 31,

        /// <summary>
        /// 'Acoustic Bass' General MIDI program.
        /// </summary>
        AcousticBass = 32,

        /// <summary>
        /// 'Electric Bass (finger)' General MIDI program.
        /// </summary>
        ElectricBass1 = 33,

        /// <summary>
        /// 'Electric Bass (pick)' General MIDI program.
        /// </summary>
        ElectricBass2 = 34,

        /// <summary>
        /// 'Fretless Bass' General MIDI program.
        /// </summary>
        FretlessBass = 35,

        /// <summary>
        /// 'Slap Bass 1' General MIDI program.
        /// </summary>
        SlapBass1 = 36,

        /// <summary>
        /// 'Slap Bass 2' General MIDI program.
        /// </summary>
        SlapBass2 = 37,

        /// <summary>
        /// 'Synth Bass 1' General MIDI program.
        /// </summary>
        SynthBass1 = 38,

        /// <summary>
        /// 'Synth Bass 2' General MIDI program.
        /// </summary>
        SynthBass2 = 39,

        /// <summary>
        /// 'Violin' General MIDI program.
        /// </summary>
        Violin = 40,

        /// <summary>
        /// 'Viola' General MIDI program.
        /// </summary>
        Viola = 41,

        /// <summary>
        /// 'Cello' General MIDI program.
        /// </summary>
        Cello = 42,

        /// <summary>
        /// 'Contrabass' General MIDI program.
        /// </summary>
        Contrabass = 43,

        /// <summary>
        /// 'Tremolo Strings' General MIDI program.
        /// </summary>
        TremoloStrings = 44,

        /// <summary>
        /// 'Pizzicato Strings' General MIDI program.
        /// </summary>
        PizzicatoStrings = 45,

        /// <summary>
        /// 'Orchestral Harp' General MIDI program.
        /// </summary>
        OrchestralHarp = 46,

        /// <summary>
        /// 'Timpani' General MIDI program.
        /// </summary>
        Timpani = 47,

        /// <summary>
        /// 'String Ensemble 1' General MIDI program.
        /// </summary>
        StringEnsemble1 = 48,

        /// <summary>
        /// 'String Ensemble 2' General MIDI program.
        /// </summary>
        StringEnsemble2 = 49,

        /// <summary>
        /// 'SynthStrings 1' General MIDI program.
        /// </summary>
        SynthStrings1 = 50,

        /// <summary>
        /// 'SynthStrings 2' General MIDI program.
        /// </summary>
        SynthStrings2 = 51,

        /// <summary>
        /// 'Choir Aahs' General MIDI program.
        /// </summary>
        ChoirAahs = 52,

        /// <summary>
        /// 'Voice Oohs' General MIDI program.
        /// </summary>
        VoiceOohs = 53,

        /// <summary>
        /// 'Synth Voice' General MIDI program.
        /// </summary>
        SynthVoice = 54,

        /// <summary>
        /// 'Orchestra Hit' General MIDI program.
        /// </summary>
        OrchestraHit = 55,

        /// <summary>
        /// 'Trumpet' General MIDI program.
        /// </summary>
        Trumpet = 56,

        /// <summary>
        /// 'Trombone' General MIDI program.
        /// </summary>
        Trombone = 57,

        /// <summary>
        /// 'Tuba' General MIDI program.
        /// </summary>
        Tuba = 58,

        /// <summary>
        /// 'Muted Trumpet' General MIDI program.
        /// </summary>
        MutedTrumpet = 59,

        /// <summary>
        /// 'French Horn' General MIDI program.
        /// </summary>
        FrenchHorn = 60,

        /// <summary>
        /// 'Brass Section' General MIDI program.
        /// </summary>
        BrassSection = 61,

        /// <summary>
        /// 'SynthBrass 1' General MIDI program.
        /// </summary>
        SynthBrass1 = 62,

        /// <summary>
        /// 'SynthBrass 2' General MIDI program.
        /// </summary>
        SynthBrass2 = 63,

        /// <summary>
        /// 'Soprano Sax' General MIDI program.
        /// </summary>
        SopranoSax = 64,

        /// <summary>
        /// 'Alto Sax' General MIDI program.
        /// </summary>
        AltoSax = 65,

        /// <summary>
        /// 'Tenor Sax' General MIDI program.
        /// </summary>
        TenorSax = 66,

        /// <summary>
        /// 'Baritone Sax' General MIDI program.
        /// </summary>
        BaritoneSax = 67,

        /// <summary>
        /// 'Oboe' General MIDI program.
        /// </summary>
        Oboe = 68,

        /// <summary>
        /// 'English Horn' General MIDI program.
        /// </summary>
        EnglishHorn = 69,

        /// <summary>
        /// 'Bassoon' General MIDI program.
        /// </summary>
        Bassoon = 70,

        /// <summary>
        /// 'Clarinet' General MIDI program.
        /// </summary>
        Clarinet = 71,

        /// <summary>
        /// 'Piccolo' General MIDI program.
        /// </summary>
        Piccolo = 72,

        /// <summary>
        /// 'Flute' General MIDI program.
        /// </summary>
        Flute = 73,

        /// <summary>
        /// 'Recorder' General MIDI program.
        /// </summary>
        Recorder = 74,

        /// <summary>
        /// 'Pan Flute' General MIDI program.
        /// </summary>
        PanFlute = 75,

        /// <summary>
        /// 'Blown Bottle' General MIDI program.
        /// </summary>
        BlownBottle = 76,

        /// <summary>
        /// 'Shakuhachi' General MIDI program.
        /// </summary>
        Shakuhachi = 77,

        /// <summary>
        /// 'Whistle' General MIDI program.
        /// </summary>
        Whistle = 78,

        /// <summary>
        /// 'Ocarina' General MIDI program.
        /// </summary>
        Ocarina = 79,

        /// <summary>
        /// 'Lead 1 (square)' General MIDI program.
        /// </summary>
        Lead1 = 80,

        /// <summary>
        /// 'Lead 2 (sawtooth)' General MIDI program.
        /// </summary>
        Lead2 = 81,

        /// <summary>
        /// 'Lead 3 (calliope)' General MIDI program.
        /// </summary>
        Lead3 = 82,

        /// <summary>
        /// 'Lead 4 (chiff)' General MIDI program.
        /// </summary>
        Lead4 = 83,

        /// <summary>
        /// 'Lead 5 (charang)' General MIDI program.
        /// </summary>
        Lead5 = 84,

        /// <summary>
        /// 'Lead 6 (voice)' General MIDI program.
        /// </summary>
        Lead6 = 85,

        /// <summary>
        /// 'Lead 7 (fifths)' General MIDI program.
        /// </summary>
        Lead7 = 86,

        /// <summary>
        /// 'Lead 8 (bass + lead)' General MIDI program.
        /// </summary>
        Lead8 = 87,

        /// <summary>
        /// 'Pad 1 (new age)' General MIDI program.
        /// </summary>
        Pad1 = 88,

        /// <summary>
        /// 'Pad 2 (warm)' General MIDI program.
        /// </summary>
        Pad2 = 89,

        /// <summary>
        /// 'Pad 3 (polysynth)' General MIDI program.
        /// </summary>
        Pad3 = 90,

        /// <summary>
        /// 'Pad 4 (choir)' General MIDI program.
        /// </summary>
        Pad4 = 91,

        /// <summary>
        /// 'Pad 5 (bowed)' General MIDI program.
        /// </summary>
        Pad5 = 92,

        /// <summary>
        /// 'Pad 6 (metallic)' General MIDI program.
        /// </summary>
        Pad6 = 93,

        /// <summary>
        /// 'Pad 7 (halo)' General MIDI program.
        /// </summary>
        Pad7 = 94,

        /// <summary>
        /// 'Pad 8 (sweep)' General MIDI program.
        /// </summary>
        Pad8 = 95,

        /// <summary>
        /// 'FX 1 (rain)' General MIDI program.
        /// </summary>
        Fx1 = 96,

        /// <summary>
        /// 'FX 2 (soundtrack)' General MIDI program.
        /// </summary>
        Fx2 = 97,

        /// <summary>
        /// 'FX 3 (crystal)' General MIDI program.
        /// </summary>
        Fx3 = 98,

        /// <summary>
        /// 'FX 4 (atmosphere)' General MIDI program.
        /// </summary>
        Fx4 = 99,

        /// <summary>
        /// 'FX 5 (brightness)' General MIDI program.
        /// </summary>
        Fx5 = 100,

        /// <summary>
        /// 'FX 6 (goblins)' General MIDI program.
        /// </summary>
        Fx6 = 101,

        /// <summary>
        /// 'FX 7 (echoes)' General MIDI program.
        /// </summary>
        Fx7 = 102,

        /// <summary>
        /// 'FX 8 (sci-fi)' General MIDI program.
        /// </summary>
        Fx8 = 103,

        /// <summary>
        /// 'Sitar' General MIDI program.
        /// </summary>
        Sitar = 104,

        /// <summary>
        /// 'Banjo' General MIDI program.
        /// </summary>
        Banjo = 105,

        /// <summary>
        /// 'Shamisen' General MIDI program.
        /// </summary>
        Shamisen = 106,

        /// <summary>
        /// 'Koto' General MIDI program.
        /// </summary>
        Koto = 107,

        /// <summary>
        /// 'Kalimba' General MIDI program.
        /// </summary>
        Kalimba = 108,

        /// <summary>
        /// 'Bag pipe' General MIDI program.
        /// </summary>
        BagPipe = 109,

        /// <summary>
        /// 'Fiddle' General MIDI program.
        /// </summary>
        Fiddle = 110,

        /// <summary>
        /// 'Shanai' General MIDI program.
        /// </summary>
        Shanai = 111,

        /// <summary>
        /// 'Tinkle Bell' General MIDI program.
        /// </summary>
        TinkleBell = 112,

        /// <summary>
        /// 'Agogo' General MIDI program.
        /// </summary>
        Agogo = 113,

        /// <summary>
        /// 'Steel Drums' General MIDI program.
        /// </summary>
        SteelDrums = 114,

        /// <summary>
        /// 'Woodblock' General MIDI program.
        /// </summary>
        Woodblock = 115,

        /// <summary>
        /// 'Taiko Drum' General MIDI program.
        /// </summary>
        TaikoDrum = 116,

        /// <summary>
        /// 'Melodic Tom' General MIDI program.
        /// </summary>
        MelodicTom = 117,

        /// <summary>
        /// 'Synth Drum' General MIDI program.
        /// </summary>
        SynthDrum = 118,

        /// <summary>
        /// 'Reverse Cymbal' General MIDI program.
        /// </summary>
        ReverseCymbal = 119,

        /// <summary>
        /// 'Guitar Fret Noise' General MIDI program.
        /// </summary>
        GuitarFretNoise = 120,

        /// <summary>
        /// 'Breath Noise' General MIDI program.
        /// </summary>
        BreathNoise = 121,

        /// <summary>
        /// 'Seashore' General MIDI program.
        /// </summary>
        Seashore = 122,

        /// <summary>
        /// 'Bird Tweet' General MIDI program.
        /// </summary>
        BirdTweet = 123,

        /// <summary>
        /// 'Telephone Ring' General MIDI program.
        /// </summary>
        TelephoneRing = 124,

        /// <summary>
        /// 'Helicopter' General MIDI program.
        /// </summary>
        Helicopter = 125,

        /// <summary>
        /// 'Applause' General MIDI program.
        /// </summary>
        Applause = 126,

        /// <summary>
        /// 'Gunshot' General MIDI program.
        /// </summary>
        Gunshot = 127
    }
}
