namespace Melanchall.DryMidi
{
    public sealed class NoteOffEvent : ChannelEvent
    {
        #region Constructor

        public NoteOffEvent()
            : base(2)
        {
        }

        public NoteOffEvent(SevenBitNumber noteNumber, SevenBitNumber velocity)
            : this()
        {
            NoteNumber = noteNumber;
            Velocity = velocity;
        }

        #endregion

        #region Properties

        public SevenBitNumber NoteNumber
        {
            get { return _parameters[0]; }
            set { _parameters[0] = value; }
        }

        public SevenBitNumber Velocity
        {
            get { return _parameters[1]; }
            set { _parameters[1] = value; }
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return $"Note Off (channel = {Channel}, note number = {NoteNumber}, velocity = {Velocity})";
        }

        #endregion
    }
}
