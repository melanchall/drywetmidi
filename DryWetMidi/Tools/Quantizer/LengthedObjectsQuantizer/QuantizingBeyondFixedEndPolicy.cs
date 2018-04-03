namespace Melanchall.DryWetMidi.Tools
{
    // TODO: use next/previous grid point
    public enum QuantizingBeyondFixedEndPolicy
    {
        CollapseAndFix = 0,
        CollapseAndMove,
        // TODO: better name
        ReverseEnds,
        Skip,
        Abort
    }
}
