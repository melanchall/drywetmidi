namespace Melanchall.DryWetMidi.Composing
{
    internal interface IPatternAction
    {
        PatternActionResult Invoke(long time, PatternContext context);
    }
}
