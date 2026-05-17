using System;
using Melanchall.DryWetMidi.Common;

namespace Melanchall.DryWetMidi.Multimedia
{
    /// <summary>
    /// Provides methods to connect MIDI devices. More info in the
    /// <see href="xref:a_dev_connector">Devices connector</see> article.
    /// </summary>
    public static class EndpointsConnectorUtilities
    {
        #region Methods

        /// <summary>
        /// Connects an input endpoint to the specified output endpoints.
        /// </summary>
        /// <param name="inputEndpoint">Input MIDI endpoint to connect to <paramref name="outputEndpoints"/>.</param>
        /// <param name="outputEndpoints">Output MIDI endpoints to connect <paramref name="inputEndpoint"/> to.</param>
        /// <exception cref="ArgumentNullException">
        /// <para>One of the following errors occurred:</para>
        /// <list type="bullet">
        /// <item>
        /// <description><paramref name="inputEndpoint"/> is <c>null</c>.</description>
        /// </item>
        /// <item>
        /// <description><paramref name="outputEndpoints"/> is <c>null</c>.</description>
        /// </item>
        /// </list>
        /// </exception>
        /// <exception cref="ArgumentException"><paramref name="outputEndpoints"/> contains <c>null</c>.</exception>
        public static EndpointsConnector Connect(this IInputEndpoint inputEndpoint, params IOutputEndpoint[] outputEndpoints)
        {
            ThrowIfArgument.IsNull(nameof(inputEndpoint), inputEndpoint);
            ThrowIfArgument.IsNull(nameof(outputEndpoints), outputEndpoints);
            ThrowIfArgument.ContainsNull(nameof(outputEndpoints), outputEndpoints);

            var devicesConnector = new EndpointsConnector(inputEndpoint, outputEndpoints);
            devicesConnector.Connect();
            return devicesConnector;
        }

        #endregion
    }
}
