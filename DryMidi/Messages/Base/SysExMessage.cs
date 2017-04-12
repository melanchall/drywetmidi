using System;

namespace Melanchall.DryMidi
{
    public abstract class SysExMessage : Message
    {
        #region Properties

        public bool Completed { get; set; } = true;

        public byte[] Data { get; set; }

        #endregion

        #region Methods

        public bool Equals(SysExMessage sysExMessage)
        {
            if (ReferenceEquals(null, sysExMessage))
                return false;

            if (ReferenceEquals(this, sysExMessage))
                return true;

            return base.Equals(sysExMessage) && Completed == sysExMessage.Completed &&
                                                ArrayUtilities.Equals(Data, sysExMessage.Data);
        }

        #endregion

        #region Overrides

        internal override void WriteContent(MidiWriter writer, WritingSettings settings)
        {
            var data = Data;
            if (data != null)
                writer.WriteBytes(data);
        }

        internal override int GetContentSize()
        {
            return Data?.Length ?? 0;
        }

        protected sealed override Message CloneMessage()
        {
            var messageType = GetType();
            var message = (SysExMessage)Activator.CreateInstance(messageType);

            message.Completed = Completed;
            message.Data = Data?.Clone() as byte[];

            return message;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SysExMessage);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode() ^ Completed.GetHashCode() ^
                                        (Data?.GetHashCode() ?? 0);
        }

        #endregion
    }
}
