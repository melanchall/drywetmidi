namespace Melanchall.DryWetMidi.Tools
{
    // TODO: use next/previous grid point
    public enum QuantizingBeyondFixedEndPolicy
    {
        CollapseAndFix = 0,
        CollapseAndMove,
        SwapEnds,
        Skip,
        Abort
    }
}
