using System;

namespace Melanchall.DryWetMidi
{
    public abstract class SysExEvent : MidiEvent
    {
        #region Properties

        public bool Completed { get; set; } = true;

        public byte[] Data { get; set; }

        #endregion

        #region Methods

        public bool Equals(SysExEvent sysExEvent)
        {
            return Equals(sysExEvent, true);
        }

        public bool Equals(SysExEvent sysExEvent, bool respectDeltaTime)
        {
            if (ReferenceEquals(null, sysExEvent))
                return false;

            if (ReferenceEquals(this, sysExEvent))
                return true;

            return base.Equals(sysExEvent, respectDeltaTime) &&
                   Completed == sysExEvent.Completed &&
                   ArrayUtilities.Equals(Data, sysExEvent.Data);
        }

        #endregion

        #region Overrides

        internal override void Write(MidiWriter writer, WritingSettings settings)
        {
            var data = Data;
            if (data != null)
                writer.WriteBytes(data);
        }

        internal override int GetSize()
        {
            return Data?.Length ?? 0;
        }

        protected sealed override MidiEvent CloneEvent()
        {
            var eventType = GetType();
            var sysExEvent = (SysExEvent)Activator.CreateInstance(eventType);

            sysExEvent.Completed = Completed;
            sysExEvent.Data = Data?.Clone() as byte[];

            return sysExEvent;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SysExEvent);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Completed.GetHashCode() ^
                                        (Data?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
