namespace Melanchall.DryWetMidi.Smf.Interaction
{
    public interface ILengthedObject : ITimedObject
    {
        long Length { get; }
    }
}
