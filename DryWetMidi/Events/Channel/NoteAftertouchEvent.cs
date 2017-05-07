namespace Melanchall.DryWetMidi
{
    public sealed class NoteAftertouchEvent : ChannelEvent
    {
        #region Constants

        private const int ParametersCount = 2;
        private const int NoteNumberParameterIndex = 0;
        private const int AftertouchValueParameterIndex = 1;

        #endregion

        #region Constructor

        public NoteAftertouchEvent()
            : base(ParametersCount)
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
            get { return _parameters[NoteNumberParameterIndex]; }
            set { _parameters[NoteNumberParameterIndex] = value; }
        }

        public SevenBitNumber AftertouchValue
        {
            get { return _parameters[AftertouchValueParameterIndex]; }
            set { _parameters[AftertouchValueParameterIndex] = value; }
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
