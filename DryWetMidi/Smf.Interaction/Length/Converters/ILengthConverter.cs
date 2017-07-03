namespace Melanchall.DryWetMidi.Smf.Interaction
{
    /// <summary>
    /// Represents converter that can be used to convert length of an object from <see cref="long"/>
    /// to specific length type and vice versa.
    /// </summary>
    public interface ILengthConverter
    {
        /// <summary>
        /// Converts length from <see cref="long"/> to the length type the converter is for.
        /// </summary>
        /// <param name="length">Length to convert.</param>
        /// <param name="time">Start time of an object to convert length of.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="length"/>.</param>
        /// <returns>Length as an instance of type a converter is for.</returns>
        ILength ConvertTo(long length, long time, TempoMap tempoMap);

        /// <summary>
        /// Converts length from <see cref="long"/> to the length type the converter is for.
        /// </summary>
        /// <param name="length">Length to convert.</param>
        /// <param name="time">Start time of an object to convert length of.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="length"/>.</param>
        /// <returns>Length as an instance of type a converter is for.</returns>
        ILength ConvertTo(long length, ITime time, TempoMap tempoMap);

        /// <summary>
        /// Converts length from the length type the converter is for to <see cref="long"/>.
        /// </summary>
        /// <param name="length">Length to convert.</param>
        /// <param name="time">Start time of an object to convert length of.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="length"/>.</param>
        /// <returns>Length as <see cref="long"/>.</returns>
        long ConvertFrom(ILength length, long time, TempoMap tempoMap);

        /// <summary>
        /// Converts length from the length type the converter is for to <see cref="long"/>.
        /// </summary>
        /// <param name="length">Length to convert.</param>
        /// <param name="time">Start time of an object to convert length of.</param>
        /// <param name="tempoMap">Tempo map used to convert <paramref name="length"/>.</param>
        /// <returns>Length as <see cref="long"/>.</returns>
        long ConvertFrom(ILength length, ITime time, TempoMap tempoMap);
    }
}
