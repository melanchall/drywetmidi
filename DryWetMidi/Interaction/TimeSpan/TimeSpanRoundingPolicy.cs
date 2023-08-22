using System;

namespace Melanchall.DryWetMidi.Interaction
{
    /// <summary>
    /// Specifies how a time span should be rounded.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value of the enum is <see cref="NoRounding"/>. Let's see how remaining options work.
    /// Please note that rounding step should be specified (see
    /// <see cref="TimeSpanUtilities.Round(ITimeSpan, TimeSpanRoundingPolicy, long, ITimeSpan, TempoMap)"/>).
    /// </para>
    /// <para>
    /// <see cref="RoundUp"/> used to round a time span to the smallest one that is greater than or equal
    /// to the time span. Following table shows how time span <see href="xref:a_time_length#metric">0:0:10</see> will be
    /// rounded using different steps:
    /// </para>
    /// <para>
    /// <list type="table">
    /// <listheader>
    /// <term>Step</term>
    /// <term>Result</term>
    /// </listheader>
    /// <item>
    /// <term>0:0:15</term>
    /// <term>0:0:15</term>
    /// </item>
    /// <item>
    /// <term>0:0:10</term>
    /// <term>0:0:10</term>
    /// </item>
    /// <item>
    /// <term>0:0:3</term>
    /// <term>0:0:12</term>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// <see cref="RoundDown"/> used to round a time span to the largest one that is less than or equal
    /// to the time span. Following table shows how time span <see href="xref:a_time_length#metric">0:0:10</see> will be
    /// rounded using different steps:
    /// </para>
    /// <para>
    /// <list type="table">
    /// <listheader>
    /// <term>Step</term>
    /// <term>Result</term>
    /// </listheader>
    /// <item>
    /// <term>0:0:3</term>
    /// <term>0:0:9</term>
    /// </item>
    /// <item>
    /// <term>0:0:10</term>
    /// <term>0:0:10</term>
    /// </item>
    /// <item>
    /// <term>0:0:6</term>
    /// <term>0:0:6</term>
    /// </item>
    /// <item>
    /// <term>0:0:15</term>
    /// <term>0:0:0</term>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    public enum TimeSpanRoundingPolicy
    {
        /// <summary>
        /// Don't round time span.
        /// </summary>
        NoRounding = 0,

        /// <summary>
        /// Round time span up (like, for example, <see cref="Math.Ceiling(double)"/>).
        /// </summary>
        RoundUp,

        /// <summary>
        /// Round time span down (like, for example, <see cref="Math.Floor(double)"/>).
        /// </summary>
        RoundDown
    }
}
