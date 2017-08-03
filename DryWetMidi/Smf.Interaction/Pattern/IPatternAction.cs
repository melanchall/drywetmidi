namespace Melanchall.DryWetMidi.Smf.Interaction
{
    internal interface IPatternAction
    {
        PatternActionResult Invoke(long time, PatternContext context);
    }
}
