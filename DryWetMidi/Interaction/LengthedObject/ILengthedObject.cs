using System;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Represents an object that has length.
    /// </summary>
    public interface ILengthedObject : ITimedObject
    {
        #region Properties

        /// <summary>
        /// Gets the length of an object.
        /// </summary>
        /// <remarks>
        /// Note that the returned value will be in ticks (not seconds, not milliseconds and so on).
        /// Please read <see href="xref:a_time_length">Time and length</see> article to learn how you can
        /// get the length in different representations.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="value"/> is negative.</exception>
        long Length { get; set; }

        /// <summary>
        /// Gets the end time of an object.
        /// </summary>
        long EndTime { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Splits the current object by the specified time.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If <paramref name="time"/> is less than or equal to time of the object, the left part will
        /// be <c>null</c>. If <paramref name="time"/> is greater than or equal to end time of the object,
        /// the right part will be <c>null</c>.
        /// </para>
        /// <para>
        /// Let's see a simple example. Suppose we have an object with time of <c>10</c> and length of <c>50</c>.
        /// The table below shows what left and right parts will be in case of different values of
        /// <paramref name="time"/>:
        /// </para>
        /// <list type="table">
        /// <listheader>
        /// <term>Time to split at</term>
        /// <term>Left part</term>
        /// <term>Right part</term>
        /// </listheader>
        /// <item>
        /// <term><c>5</c></term>
        /// <term><c>null</c></term>
        /// <term>Copy of the object</term>
        /// </item>
        /// <item>
        /// <term><c>10</c></term>
        /// <term><c>null</c></term>
        /// <term>Copy of the object</term>
        /// </item>
        /// <item>
        /// <term><c>30</c></term>
        /// <term>Copy of the object with time of <c>10</c> and length of <c>20</c></term>
        /// <term>Copy of the object with time of <c>30</c> and length of <c>30</c></term>
        /// </item>
        /// <item>
        /// <term><c>60</c></term>
        /// <term>Copy of the object</term>
        /// <term><c>null</c></term>
        /// </item>
        /// <item>
        /// <term><c>70</c></term>
        /// <term>Copy of the object</term>
        /// <term><c>null</c></term>
        /// </item>
        /// </list>
        /// <para>
        /// To learn about other ways to split an object please read <see href="xref:a_obj_splitting">
        /// Objects splitting</see> article.
        /// </para>
        /// </remarks>
        /// <param name="time">Time to split the object at.</param>
        /// <returns>An object containing left and right parts of the split object.
        /// Both parts have the same type as the original object.</returns>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="time"/> is negative.</exception>
        SplitLengthedObject Split(long time);

        #endregion
    }
}
