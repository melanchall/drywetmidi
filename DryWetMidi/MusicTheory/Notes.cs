using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.MusicTheory
{
    /// <summary>
    /// Contains all available MIDI notes.
    /// </summary>
    public static class Notes
    {
        #region Constants

        /// <summary>
        /// C-1 note.
        /// </summary>
        public static readonly Note CMinus1      = Note.Get((SevenBitNumber)0);

        /// <summary>
        /// C#-1 note.
        /// </summary>
        public static readonly Note CSharpMinus1 = Note.Get((SevenBitNumber)1);

        /// <summary>
        /// D-1 note.
        /// </summary>
        public static readonly Note DMinus1      = Note.Get((SevenBitNumber)2);

        /// <summary>
        /// D#-1 note.
        /// </summary>
        public static readonly Note DSharpMinus1 = Note.Get((SevenBitNumber)3);

        /// <summary>
        /// E-1 note.
        /// </summary>
        public static readonly Note EMinus1      = Note.Get((SevenBitNumber)4);

        /// <summary>
        /// F-1 note.
        /// </summary>
        public static readonly Note FMinus1      = Note.Get((SevenBitNumber)5);

        /// <summary>
        /// F#-1 note.
        /// </summary>
        public static readonly Note FSharpMinus1 = Note.Get((SevenBitNumber)6);

        /// <summary>
        /// G-1 note.
        /// </summary>
        public static readonly Note GMinus1      = Note.Get((SevenBitNumber)7);

        /// <summary>
        /// G#-1 note.
        /// </summary>
        public static readonly Note GSharpMinus1 = Note.Get((SevenBitNumber)8);

        /// <summary>
        /// A-1 note.
        /// </summary>
        public static readonly Note AMinus1      = Note.Get((SevenBitNumber)9);

        /// <summary>
        /// A#-1 note.
        /// </summary>
        public static readonly Note ASharpMinus1 = Note.Get((SevenBitNumber)10);

        /// <summary>
        /// B-1 note.
        /// </summary>
        public static readonly Note BMinus1      = Note.Get((SevenBitNumber)11);

        /// <summary>
        /// C0 note.
        /// </summary>
        public static readonly Note C0           = Note.Get((SevenBitNumber)12);

        /// <summary>
        /// C#0 note.
        /// </summary>
        public static readonly Note CSharp0      = Note.Get((SevenBitNumber)13);

        /// <summary>
        /// D0 note.
        /// </summary>
        public static readonly Note D0           = Note.Get((SevenBitNumber)14);

        /// <summary>
        /// D#0 note.
        /// </summary>
        public static readonly Note DSharp0      = Note.Get((SevenBitNumber)15);

        /// <summary>
        /// E0 note.
        /// </summary>
        public static readonly Note E0           = Note.Get((SevenBitNumber)16);

        /// <summary>
        /// F0 note.
        /// </summary>
        public static readonly Note F0           = Note.Get((SevenBitNumber)17);

        /// <summary>
        /// F#0 note.
        /// </summary>
        public static readonly Note FSharp0      = Note.Get((SevenBitNumber)18);

        /// <summary>
        /// G0 note.
        /// </summary>
        public static readonly Note G0           = Note.Get((SevenBitNumber)19);

        /// <summary>
        /// G#0 note.
        /// </summary>
        public static readonly Note GSharp0      = Note.Get((SevenBitNumber)20);

        /// <summary>
        /// A0 note.
        /// </summary>
        public static readonly Note A0           = Note.Get((SevenBitNumber)21);

        /// <summary>
        /// A#0 note.
        /// </summary>
        public static readonly Note ASharp0      = Note.Get((SevenBitNumber)22);

        /// <summary>
        /// B0 note.
        /// </summary>
        public static readonly Note B0           = Note.Get((SevenBitNumber)23);

        /// <summary>
        /// C1 note.
        /// </summary>
        public static readonly Note C1           = Note.Get((SevenBitNumber)24);

        /// <summary>
        /// C#1 note.
        /// </summary>
        public static readonly Note CSharp1      = Note.Get((SevenBitNumber)25);

        /// <summary>
        /// D1 note.
        /// </summary>
        public static readonly Note D1           = Note.Get((SevenBitNumber)26);

        /// <summary>
        /// D#1 note.
        /// </summary>
        public static readonly Note DSharp1      = Note.Get((SevenBitNumber)27);

        /// <summary>
        /// E1 note.
        /// </summary>
        public static readonly Note E1           = Note.Get((SevenBitNumber)28);

        /// <summary>
        /// F1 note.
        /// </summary>
        public static readonly Note F1           = Note.Get((SevenBitNumber)29);

        /// <summary>
        /// F#1 note.
        /// </summary>
        public static readonly Note FSharp1      = Note.Get((SevenBitNumber)30);

        /// <summary>
        /// G1 note.
        /// </summary>
        public static readonly Note G1           = Note.Get((SevenBitNumber)31);

        /// <summary>
        /// G#1 note.
        /// </summary>
        public static readonly Note GSharp1      = Note.Get((SevenBitNumber)32);

        /// <summary>
        /// A1 note.
        /// </summary>
        public static readonly Note A1           = Note.Get((SevenBitNumber)33);

        /// <summary>
        /// A#1 note.
        /// </summary>
        public static readonly Note ASharp1      = Note.Get((SevenBitNumber)34);

        /// <summary>
        /// B1 note.
        /// </summary>
        public static readonly Note B1           = Note.Get((SevenBitNumber)35);

        /// <summary>
        /// C2 note.
        /// </summary>
        public static readonly Note C2           = Note.Get((SevenBitNumber)36);

        /// <summary>
        /// C#2 note.
        /// </summary>
        public static readonly Note CSharp2      = Note.Get((SevenBitNumber)37);

        /// <summary>
        /// D2 note.
        /// </summary>
        public static readonly Note D2           = Note.Get((SevenBitNumber)38);

        /// <summary>
        /// D#2 note.
        /// </summary>
        public static readonly Note DSharp2      = Note.Get((SevenBitNumber)39);

        /// <summary>
        /// E2 note.
        /// </summary>
        public static readonly Note E2           = Note.Get((SevenBitNumber)40);

        /// <summary>
        /// F2 note.
        /// </summary>
        public static readonly Note F2           = Note.Get((SevenBitNumber)41);

        /// <summary>
        /// F#2 note.
        /// </summary>
        public static readonly Note FSharp2      = Note.Get((SevenBitNumber)42);

        /// <summary>
        /// G2 note.
        /// </summary>
        public static readonly Note G2           = Note.Get((SevenBitNumber)43);

        /// <summary>
        /// G#2 note.
        /// </summary>
        public static readonly Note GSharp2      = Note.Get((SevenBitNumber)44);

        /// <summary>
        /// A2 note.
        /// </summary>
        public static readonly Note A2           = Note.Get((SevenBitNumber)45);

        /// <summary>
        /// A#2 note.
        /// </summary>
        public static readonly Note ASharp2      = Note.Get((SevenBitNumber)46);

        /// <summary>
        /// B2 note.
        /// </summary>
        public static readonly Note B2           = Note.Get((SevenBitNumber)47);

        /// <summary>
        /// C3 note.
        /// </summary>
        public static readonly Note C3           = Note.Get((SevenBitNumber)48);

        /// <summary>
        /// C#3 note.
        /// </summary>
        public static readonly Note CSharp3      = Note.Get((SevenBitNumber)49);

        /// <summary>
        /// D3 note.
        /// </summary>
        public static readonly Note D3           = Note.Get((SevenBitNumber)50);

        /// <summary>
        /// D#3 note.
        /// </summary>
        public static readonly Note DSharp3      = Note.Get((SevenBitNumber)51);

        /// <summary>
        /// E3 note.
        /// </summary>
        public static readonly Note E3           = Note.Get((SevenBitNumber)52);

        /// <summary>
        /// F3 note.
        /// </summary>
        public static readonly Note F3           = Note.Get((SevenBitNumber)53);

        /// <summary>
        /// F#3 note.
        /// </summary>
        public static readonly Note FSharp3      = Note.Get((SevenBitNumber)54);

        /// <summary>
        /// G3 note.
        /// </summary>
        public static readonly Note G3           = Note.Get((SevenBitNumber)55);

        /// <summary>
        /// G#3 note.
        /// </summary>
        public static readonly Note GSharp3      = Note.Get((SevenBitNumber)56);

        /// <summary>
        /// A3 note.
        /// </summary>
        public static readonly Note A3           = Note.Get((SevenBitNumber)57);

        /// <summary>
        /// A#3 note.
        /// </summary>
        public static readonly Note ASharp3      = Note.Get((SevenBitNumber)58);

        /// <summary>
        /// B3 note.
        /// </summary>
        public static readonly Note B3           = Note.Get((SevenBitNumber)59);

        /// <summary>
        /// C4 note.
        /// </summary>
        public static readonly Note C4           = Note.Get((SevenBitNumber)60);

        /// <summary>
        /// C#4 note.
        /// </summary>
        public static readonly Note CSharp4      = Note.Get((SevenBitNumber)61);

        /// <summary>
        /// D4 note.
        /// </summary>
        public static readonly Note D4           = Note.Get((SevenBitNumber)62);

        /// <summary>
        /// D#4 note.
        /// </summary>
        public static readonly Note DSharp4      = Note.Get((SevenBitNumber)63);

        /// <summary>
        /// E4 note.
        /// </summary>
        public static readonly Note E4           = Note.Get((SevenBitNumber)64);

        /// <summary>
        /// F4 note.
        /// </summary>
        public static readonly Note F4           = Note.Get((SevenBitNumber)65);

        /// <summary>
        /// F#4 note.
        /// </summary>
        public static readonly Note FSharp4      = Note.Get((SevenBitNumber)66);

        /// <summary>
        /// G4 note.
        /// </summary>
        public static readonly Note G4           = Note.Get((SevenBitNumber)67);

        /// <summary>
        /// G#4 note.
        /// </summary>
        public static readonly Note GSharp4      = Note.Get((SevenBitNumber)68);

        /// <summary>
        /// A4 note.
        /// </summary>
        public static readonly Note A4           = Note.Get((SevenBitNumber)69);

        /// <summary>
        /// A#4 note.
        /// </summary>
        public static readonly Note ASharp4      = Note.Get((SevenBitNumber)70);

        /// <summary>
        /// B4 note.
        /// </summary>
        public static readonly Note B4           = Note.Get((SevenBitNumber)71);

        /// <summary>
        /// C5 note.
        /// </summary>
        public static readonly Note C5           = Note.Get((SevenBitNumber)72);

        /// <summary>
        /// C#5 note.
        /// </summary>
        public static readonly Note CSharp5      = Note.Get((SevenBitNumber)73);

        /// <summary>
        /// D5 note.
        /// </summary>
        public static readonly Note D5           = Note.Get((SevenBitNumber)74);

        /// <summary>
        /// D#5 note.
        /// </summary>
        public static readonly Note DSharp5      = Note.Get((SevenBitNumber)75);

        /// <summary>
        /// E5 note.
        /// </summary>
        public static readonly Note E5           = Note.Get((SevenBitNumber)76);

        /// <summary>
        /// F5 note.
        /// </summary>
        public static readonly Note F5           = Note.Get((SevenBitNumber)77);

        /// <summary>
        /// F#5 note.
        /// </summary>
        public static readonly Note FSharp5      = Note.Get((SevenBitNumber)78);

        /// <summary>
        /// G5 note.
        /// </summary>
        public static readonly Note G5           = Note.Get((SevenBitNumber)79);

        /// <summary>
        /// G#5 note.
        /// </summary>
        public static readonly Note GSharp5      = Note.Get((SevenBitNumber)80);

        /// <summary>
        /// A5 note.
        /// </summary>
        public static readonly Note A5           = Note.Get((SevenBitNumber)81);

        /// <summary>
        /// A#5 note.
        /// </summary>
        public static readonly Note ASharp5      = Note.Get((SevenBitNumber)82);

        /// <summary>
        /// B5 note.
        /// </summary>
        public static readonly Note B5           = Note.Get((SevenBitNumber)83);

        /// <summary>
        /// C6 note.
        /// </summary>
        public static readonly Note C6           = Note.Get((SevenBitNumber)84);

        /// <summary>
        /// C#6 note.
        /// </summary>
        public static readonly Note CSharp6      = Note.Get((SevenBitNumber)85);

        /// <summary>
        /// D6 note.
        /// </summary>
        public static readonly Note D6           = Note.Get((SevenBitNumber)86);

        /// <summary>
        /// D#6 note.
        /// </summary>
        public static readonly Note DSharp6      = Note.Get((SevenBitNumber)87);

        /// <summary>
        /// E6 note.
        /// </summary>
        public static readonly Note E6           = Note.Get((SevenBitNumber)88);

        /// <summary>
        /// F6 note.
        /// </summary>
        public static readonly Note F6           = Note.Get((SevenBitNumber)89);

        /// <summary>
        /// F#6 note.
        /// </summary>
        public static readonly Note FSharp6      = Note.Get((SevenBitNumber)90);

        /// <summary>
        /// G6 note.
        /// </summary>
        public static readonly Note G6           = Note.Get((SevenBitNumber)91);

        /// <summary>
        /// G#6 note.
        /// </summary>
        public static readonly Note GSharp6      = Note.Get((SevenBitNumber)92);

        /// <summary>
        /// A6 note.
        /// </summary>
        public static readonly Note A6           = Note.Get((SevenBitNumber)93);

        /// <summary>
        /// A#6 note.
        /// </summary>
        public static readonly Note ASharp6      = Note.Get((SevenBitNumber)94);

        /// <summary>
        /// B6 note.
        /// </summary>
        public static readonly Note B6           = Note.Get((SevenBitNumber)95);

        /// <summary>
        /// C7 note.
        /// </summary>
        public static readonly Note C7           = Note.Get((SevenBitNumber)96);

        /// <summary>
        /// C#7 note.
        /// </summary>
        public static readonly Note CSharp7      = Note.Get((SevenBitNumber)97);

        /// <summary>
        /// D7 note.
        /// </summary>
        public static readonly Note D7           = Note.Get((SevenBitNumber)98);

        /// <summary>
        /// D#7 note.
        /// </summary>
        public static readonly Note DSharp7      = Note.Get((SevenBitNumber)99);

        /// <summary>
        /// E7 note.
        /// </summary>
        public static readonly Note E7           = Note.Get((SevenBitNumber)100);

        /// <summary>
        /// F7 note.
        /// </summary>
        public static readonly Note F7           = Note.Get((SevenBitNumber)101);

        /// <summary>
        /// F#7 note.
        /// </summary>
        public static readonly Note FSharp7      = Note.Get((SevenBitNumber)102);

        /// <summary>
        /// G7 note.
        /// </summary>
        public static readonly Note G7           = Note.Get((SevenBitNumber)103);

        /// <summary>
        /// G#7 note.
        /// </summary>
        public static readonly Note GSharp7      = Note.Get((SevenBitNumber)104);

        /// <summary>
        /// A7 note.
        /// </summary>
        public static readonly Note A7           = Note.Get((SevenBitNumber)105);

        /// <summary>
        /// A#7 note.
        /// </summary>
        public static readonly Note ASharp7      = Note.Get((SevenBitNumber)106);

        /// <summary>
        /// B7 note.
        /// </summary>
        public static readonly Note B7           = Note.Get((SevenBitNumber)107);

        /// <summary>
        /// C8 note.
        /// </summary>
        public static readonly Note C8           = Note.Get((SevenBitNumber)108);

        /// <summary>
        /// C#8 note.
        /// </summary>
        public static readonly Note CSharp8      = Note.Get((SevenBitNumber)109);

        /// <summary>
        /// D8 note.
        /// </summary>
        public static readonly Note D8           = Note.Get((SevenBitNumber)110);

        /// <summary>
        /// D#8 note.
        /// </summary>
        public static readonly Note DSharp8      = Note.Get((SevenBitNumber)111);

        /// <summary>
        /// E8 note.
        /// </summary>
        public static readonly Note E8           = Note.Get((SevenBitNumber)112);

        /// <summary>
        /// F8 note.
        /// </summary>
        public static readonly Note F8           = Note.Get((SevenBitNumber)113);

        /// <summary>
        /// F#8 note.
        /// </summary>
        public static readonly Note FSharp8      = Note.Get((SevenBitNumber)114);

        /// <summary>
        /// G8 note.
        /// </summary>
        public static readonly Note G8           = Note.Get((SevenBitNumber)115);

        /// <summary>
        /// G#8 note.
        /// </summary>
        public static readonly Note GSharp8      = Note.Get((SevenBitNumber)116);

        /// <summary>
        /// A8 note.
        /// </summary>
        public static readonly Note A8           = Note.Get((SevenBitNumber)117);

        /// <summary>
        /// A#8 note.
        /// </summary>
        public static readonly Note ASharp8      = Note.Get((SevenBitNumber)118);

        /// <summary>
        /// B8 note.
        /// </summary>
        public static readonly Note B8           = Note.Get((SevenBitNumber)119);

        /// <summary>
        /// C9 note.
        /// </summary>
        public static readonly Note C9           = Note.Get((SevenBitNumber)120);

        /// <summary>
        /// C#9 note.
        /// </summary>
        public static readonly Note CSharp9      = Note.Get((SevenBitNumber)121);

        /// <summary>
        /// D9 note.
        /// </summary>
        public static readonly Note D9           = Note.Get((SevenBitNumber)122);

        /// <summary>
        /// D#9 note.
        /// </summary>
        public static readonly Note DSharp9      = Note.Get((SevenBitNumber)123);

        /// <summary>
        /// E9 note.
        /// </summary>
        public static readonly Note E9           = Note.Get((SevenBitNumber)124);

        /// <summary>
        /// F9 note.
        /// </summary>
        public static readonly Note F9           = Note.Get((SevenBitNumber)125);

        /// <summary>
        /// F#9 note.
        /// </summary>
        public static readonly Note FSharp9      = Note.Get((SevenBitNumber)126);

        /// <summary>
        /// G9 note.
        /// </summary>
        public static readonly Note G9           = Note.Get((SevenBitNumber)127);

        #endregion
    }
}
