namespace Melanchall.DryWetMidi.Common
{
    /// <summary>
    /// MIDI controller names.
    /// </summary>
    public enum ControlName : byte
    {
        /// <summary>
        /// Bank Select.
        /// </summary>
        BankSelect = 0x00,

        /// <summary>
        /// Modulation Wheel or Lever.
        /// </summary>
        Modulation = 0x01,

        /// <summary>
        /// Breath Controller.
        /// </summary>
        BreathController = 0x02,

        /// <summary>
        /// Foot Controller.
        /// </summary>
        FootController = 0x04,

        /// <summary>
        /// Portamento Time.
        /// </summary>
        PortamentoTime = 0x05,

        /// <summary>
        /// Data Entry MSB.
        /// </summary>
        DataEntryMsb = 0x06,

        /// <summary>
        /// Channel Volume (formerly Main Volume).
        /// </summary>
        ChannelVolume = 0x07,

        /// <summary>
        /// Balance.
        /// </summary>
        Balance = 0x08,

        /// <summary>
        /// Pan.
        /// </summary>
        Pan = 0x0A,

        /// <summary>
        /// Expression Controller.
        /// </summary>
        ExpressionController = 0x0B,

        /// <summary>
        /// Effect Control 1.
        /// </summary>
        EffectControl1 = 0x0C,

        /// <summary>
        /// Effect Control 2.
        /// </summary>
        EffectControl2 = 0x0D,

        /// <summary>
        /// General Purpose Controller 1.
        /// </summary>
        GeneralPurposeController1 = 0x10,

        /// <summary>
        /// General Purpose Controller 2.
        /// </summary>
        GeneralPurposeController2 = 0x11,

        /// <summary>
        /// General Purpose Controller 3.
        /// </summary>
        GeneralPurposeController3 = 0x12,

        /// <summary>
        /// General Purpose Controller 4.
        /// </summary>
        GeneralPurposeController4 = 0x13,

        /// <summary>
        /// LSB for Bank Select.
        /// </summary>
        LsbForBankSelect = 0x20,

        /// <summary>
        /// LSB for Modulation.
        /// </summary>
        LsbForModulation = 0x21,

        /// <summary>
        /// LSB for Breath Controller.
        /// </summary>
        LsbForBreathController = 0x22,

        /// <summary>
        /// LSB for Control 3 (Undefined).
        /// </summary>
        LsbForController3 = 0x23,

        /// <summary>
        /// LSB for Foot Controller.
        /// </summary>
        LsbForFootController = 0x24,

        /// <summary>
        /// LSB for Portamento Time.
        /// </summary>
        LsbForPortamentoTime = 0x25,

        /// <summary>
        /// LSB for Data Entry.
        /// </summary>
        LsbForDataEntry = 0x26,

        /// <summary>
        /// LSB for Channel Volume (formerly Main Volume).
        /// </summary>
        LsbForChannelVolume = 0x27,

        /// <summary>
        /// LSB for Balance.
        /// </summary>
        LsbForBalance = 0x28,

        /// <summary>
        /// LSB for Control 9 (Undefined).
        /// </summary>
        LsbForController9 = 0x29,

        /// <summary>
        /// LSB for Pan.
        /// </summary>
        LsbForPan = 0x2A,

        /// <summary>
        /// LSB for Expression Controller.
        /// </summary>
        LsbForExpressionController = 0x2B,

        /// <summary>
        /// LSB for Effect Control 1.
        /// </summary>
        LsbForEffectControl1 = 0x2C,

        /// <summary>
        /// LSB for Effect Control 2.
        /// </summary>
        LsbForEffectControl2 = 0x2D,

        /// <summary>
        /// LSB for Control 14 (Undefined).
        /// </summary>
        LsbForController14 = 0x2E,

        /// <summary>
        /// LSB for Control 15 (Undefined).
        /// </summary>
        LsbForController15 = 0x2F,

        /// <summary>
        /// LSB for General Purpose Controller 1.
        /// </summary>
        LsbForGeneralPurposeController1 = 0x30,

        /// <summary>
        /// LSB for General Purpose Controller 2.
        /// </summary>
        LsbForGeneralPurposeController2 = 0x31,

        /// <summary>
        /// LSB for General Purpose Controller 3.
        /// </summary>
        LsbForGeneralPurposeController3 = 0x32,

        /// <summary>
        /// LSB for General Purpose Controller 4.
        /// </summary>
        LsbForGeneralPurposeController4 = 0x33,

        /// <summary>
        /// LSB for Control 20 (Undefined).
        /// </summary>
        LsbForController20 = 0x34,

        /// <summary>
        /// LSB for Control 21 (Undefined).
        /// </summary>
        LsbForController21 = 0x35,

        /// <summary>
        /// LSB for Control 22 (Undefined).
        /// </summary>
        LsbForController22 = 0x36,

        /// <summary>
        /// LSB for Control 23 (Undefined).
        /// </summary>
        LsbForController23 = 0x37,

        /// <summary>
        /// LSB for Control 24 (Undefined).
        /// </summary>
        LsbForController24 = 0x38,

        /// <summary>
        /// LSB for Control 25 (Undefined).
        /// </summary>
        LsbForController25 = 0x39,

        /// <summary>
        /// LSB for Control 26 (Undefined).
        /// </summary>
        LsbForController26 = 0x3A,

        /// <summary>
        /// LSB for Control 27 (Undefined).
        /// </summary>
        LsbForController27 = 0x3B,

        /// <summary>
        /// LSB for Control 28 (Undefined).
        /// </summary>
        LsbForController28 = 0x3C,

        /// <summary>
        /// LSB for Control 29 (Undefined).
        /// </summary>
        LsbForController29 = 0x3D,

        /// <summary>
        /// LSB for Control 30 (Undefined).
        /// </summary>
        LsbForController30 = 0x3E,

        /// <summary>
        /// LSB for Control 31 (Undefined).
        /// </summary>
        LsbForController31 = 0x3F,

        /// <summary>
        /// Damper Pedal On/Off (Sustain).
        /// </summary>
        DamperPedal = 0x40,

        /// <summary>
        /// Portamento On/Off.
        /// </summary>
        Portamento = 0x41,

        /// <summary>
        /// Sostenuto On/Off.
        /// </summary>
        Sostenuto = 0x42,

        /// <summary>
        /// Soft Pedal On/Off.
        /// </summary>
        SoftPedal = 0x43,

        /// <summary>
        /// Legato Footswitch.
        /// </summary>
        LegatoFootswitch = 0x44,

        /// <summary>
        /// Hold 2.
        /// </summary>
        Hold2 = 0x45,

        /// <summary>
        /// Sound Controller 1 (default: Sound Variation).
        /// </summary>
        SoundController1 = 0x46,

        /// <summary>
        /// Sound Controller 2 (default: Timbre/Harmonic Intens.).
        /// </summary>
        SoundController2 = 0x47,

        /// <summary>
        /// Sound Controller 3 (default: Release Time).
        /// </summary>
        SoundController3 = 0x48,

        /// <summary>
        /// Sound Controller 4 (default: Attack Time).
        /// </summary>
        SoundController4 = 0x49,

        /// <summary>
        /// Sound Controller 5 (default: Brightness).
        /// </summary>
        SoundController5 = 0x4A,

        /// <summary>
        /// Sound Controller 6 (default: Decay Time - see MMA RP-021).
        /// </summary>
        SoundController6 = 0x4B,

        /// <summary>
        /// Sound Controller 7 (default: Vibrato Rate - see MMA RP-021).
        /// </summary>
        SoundController7 = 0x4C,

        /// <summary>
        /// Sound Controller 8 (default: Vibrato Depth - see MMA RP-021).
        /// </summary>
        SoundController8 = 0x4D,

        /// <summary>
        /// Sound Controller 9 (default: Vibrato Delay - see MMA RP-021).
        /// </summary>
        SoundController9 = 0x4E,

        /// <summary>
        /// Sound Controller 10 (default undefined - see MMA RP-021).
        /// </summary>
        SoundController10 = 0x4F,

        /// <summary>
        /// General Purpose Controller 5.
        /// </summary>
        GeneralPurposeController5 = 0x50,

        /// <summary>
        /// General Purpose Controller 6.
        /// </summary>
        GeneralPurposeController6 = 0x51,

        /// <summary>
        /// General Purpose Controller 7.
        /// </summary>
        GeneralPurposeController7 = 0x52,

        /// <summary>
        /// General Purpose Controller 8.
        /// </summary>
        GeneralPurposeController8 = 0x53,

        /// <summary>
        /// Portamento Control.
        /// </summary>
        PortamentoControl = 0x54,

        /// <summary>
        /// High Resolution Velocity Prefix.
        /// </summary>
        HighResolutionVelocityPrefix = 0x58,

        /// <summary>
        /// Effects 1 Depth (default: Reverb Send Level - see MMA RP-023; formerly External Effects Depth).
        /// </summary>
        Effects1Depth = 0x5B,

        /// <summary>
        /// Effects 2 Depth (formerly Tremolo Depth).
        /// </summary>
        Effects2Depth = 0x5C,

        /// <summary>
        /// Effects 3 Depth (default: Chorus Send Level - see MMA RP-023; formerly Chorus Depth).
        /// </summary>
        Effects3Depth = 0x5D,

        /// <summary>
        /// Effects 4 Depth (formerly Celeste [Detune] Depth).
        /// </summary>
        Effects4Depth = 0x5E,

        /// <summary>
        /// Effects 5 Depth (formerly Phaser Depth).
        /// </summary>
        Effects5Depth = 0x5F,

        /// <summary>
        /// Data Increment (Data Entry +1; see MMA RP-018).
        /// </summary>
        DataIncrement = 0x60,

        /// <summary>
        /// Data Decrement (Data Entry -1; see MMA RP-018).
        /// </summary>
        DataDecrement = 0x61,

        /// <summary>
        /// Non-Registered Parameter Number (NRPN) - LSB.
        /// </summary>
        NonRegisteredParameterNumberLsb = 0x62,

        /// <summary>
        /// Non-Registered Parameter Number (NRPN) - MSB.
        /// </summary>
        NonRegisteredParameterNumberMsb = 0x63,

        /// <summary>
        /// Registered Parameter Number (RPN) - LSB.
        /// </summary>
        RegisteredParameterNumberLsb = 0x64,

        /// <summary>
        /// Registered Parameter Number (RPN) - MSB.
        /// </summary>
        RegisteredParameterNumberMsb = 0x65,

        /// <summary>
        /// Channel Mode Message: All Sound Off.
        /// </summary>
        AllSoundOff = 0x78,

        /// <summary>
        /// Channel Mode Message: Reset All Controllers (See MMA RP-015).
        /// </summary>
        ResetAllControllers = 0x79,

        /// <summary>
        /// Channel Mode Message: Local Control On/Off.
        /// </summary>
        LocalControl = 0x7A,

        /// <summary>
        /// Channel Mode Message: All Notes Off.
        /// </summary>
        AllNotesOff = 0x7B,

        /// <summary>
        /// Channel Mode Message: Omni Mode Off (+ all notes off).
        /// </summary>
        OmniModeOff = 0x7C,

        /// <summary>
        /// Channel Mode Message: Omni Mode On (+ all notes off).
        /// </summary>
        OmniModeOn = 0x7D,

        /// <summary>
        /// Channel Mode Message: Mono Mode On (+ poly off, + all notes off).
        /// </summary>
        MonoModeOn = 0x7E,

        /// <summary>
        /// Channel Mode Message: Poly Mode On (+ mono off, +all notes off).
        /// </summary>
        PolyModeOn = 0x7F,

        /// <summary>
        /// Undefined Controller.
        /// </summary>
        Undefined = 0xFF
    }
}
