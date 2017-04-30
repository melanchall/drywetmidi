namespace Melanchall.DryWetMidi
{
    public sealed class NoteAftertouchEvent : ChannelEvent
    {
        #region Constructor

        public NoteAftertouchEvent()
            : base(2)
        {
        }

        public NoteAftertouchEvent(SevenBitNumber noteNumber, SevenBitNumber aftertouchValue)
            : this()
        {
            NoteNumber = noteNumber;
            AftertouchValue = aftertouchValue;
        }

        #endregion

        #region Properties

        public SevenBitNumber NoteNumber
        {
            get { return _parameters[0]; }
            set { _parameters[0] = value; }
        }

        public SevenBitNumber AftertouchValue
        {
            get { return _parameters[1]; }
            set { _parameters[1] = value; }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"Note Aftertouch (channel = {Channel}, note number = {NoteNumber}, aftertouch value = {AftertouchValue})";
        }

        #endregion
    }
}
