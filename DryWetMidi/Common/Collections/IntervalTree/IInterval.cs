namespace Melanchall.DryWetMidi.Common
{
    internal interface IInterval<TEndpoint>
    {
        TEndpoint Start { get; }

        TEndpoint End { get; }
    }
}
