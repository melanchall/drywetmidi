namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Provides a way to convert the time of an object between <see cref="long"/> and custom time type
    /// that implements <see cref="ITime"/> and is supported by this converter.
    /// </summary>
    public interface ITimeConverter
    {
        /// <summary>
        /// Converts time from <see cref="long"/> to the time type supported by the converter.
        /// </summary>
        /// <param name="time">Time to convert.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="time"/>.</param>
        /// <returns>Time as an instance of time type supported by this converter.</returns>
        ITime ConvertTo(long time, TempoMap tempoMap);

        /// <summary>
        /// Converts time from the time type supported by the converter to <see cref="long"/>.
        /// </summary>
        /// <param name="time">Time to convert.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="time"/>.</param>
        /// <returns>Time as <see cref="long"/>.</returns>
        long ConvertFrom(ITime time, TempoMap tempoMap);
    }
}
