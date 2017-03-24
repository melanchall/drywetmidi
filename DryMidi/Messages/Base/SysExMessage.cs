using System;

namespace Melanchall.DryMidi
{
    public abstract class SysExMessage : Message
    {
        #region Properties

        public bool Completed { get; set; } = true;

        public byte[] Data { get; set; }

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

        #endregion
    }
}
