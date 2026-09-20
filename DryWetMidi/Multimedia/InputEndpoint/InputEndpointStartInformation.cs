namespace Melanchall.DryWetMidi.Multimedia
{
    public sealed class InputEndpointStartInformation
    {
        public InputEndpointStartInformation(
            long timestamp)
        {
            Timestamp = timestamp;
        }

        public long Timestamp { get; }
    }
}
