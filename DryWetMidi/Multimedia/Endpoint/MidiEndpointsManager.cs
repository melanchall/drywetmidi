using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Melanchall.DryWetMidi.Multimedia
{
    internal sealed class MidiEndpointsManager
    {
        #region Fields

        private static volatile MidiEndpointsManager _instance;
        private static readonly object _lockObject = new object();

        private bool _cachingRequired = false;

        private readonly HashSet<InputEndpoint> _inputEndpoints = new();
        private readonly object _inputEndpointsLock = new();

        private readonly HashSet<OutputEndpoint> _outputEndpoints = new();
        private readonly object _outputEndpointsLock = new();

        #endregion

        #region Constructor

        private MidiEndpointsManager()
        {
        }

        #endregion

        #region Properties

        public static MidiEndpointsManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lockObject)
                    {
                        if (_instance == null)
                        {
                            _instance = new MidiEndpointsManager();

                            var sessionHandle = MidiDevicesSession.GetSessionHandle();
                            _instance._cachingRequired = sessionHandle.IsDevicesCachingRequired;

                            if (_instance._cachingRequired)
                            {
                                lock (_instance._inputEndpointsLock)
                                {
                                    foreach (var endpoint in _instance.GetAllInputEndpointsInternal())
                                    {
                                        _instance._inputEndpoints.Add(endpoint);
                                    }
                                }

                                lock (_instance._outputEndpointsLock)
                                {
                                    foreach (var endpoint in _instance.GetAllOutputEndpointsInternal())
                                    {
                                        _instance._outputEndpoints.Add(endpoint);
                                    }
                                }

                                EndpointsWatcher.Instance.EndpointAdded += _instance.OnEndpointAdded;
                                EndpointsWatcher.Instance.EndpointRemoved += _instance.OnEndpointRemoved;
                            }
                        }
                    }
                }

                return _instance;
            }
        }

        #endregion

        #region Methods

        public ICollection<InputEndpoint> GetAllInputEndpoints()
        {
            if (_cachingRequired)
            {
                lock (_inputEndpointsLock)
                {
                    return _inputEndpoints.Select(CloneInputEndpoint).ToArray();
                }
            }

            return GetAllInputEndpointsInternal();
        }

        public ICollection<OutputEndpoint> GetAllOutputEndpoints()
        {
            if (_cachingRequired)
            {
                lock (_outputEndpointsLock)
                {
                    return _outputEndpoints.Select(CloneOutputEndpoint).ToArray();
                }
            }

            return GetAllOutputEndpointsInternal();
        }

        public InputEndpoint GetInputEndpointByName(string name)
        {
            if (_cachingRequired)
            {
                lock (_inputEndpointsLock)
                {
                    var result = _inputEndpoints.FirstOrDefault(d => d.Name == name);
                    if (result == null)
                        return null;

                    return CloneInputEndpoint(result);
                }
            }

            return GetAllInputEndpointsInternal().FirstOrDefault(d => d.Name == name);
        }

        public OutputEndpoint GetOutputEndpointByName(string name)
        {
            if (_cachingRequired)
            {
                lock (_outputEndpointsLock)
                {
                    var result = _outputEndpoints.FirstOrDefault(d => d.Name == name);
                    if (result == null)
                        return null;

                    return CloneOutputEndpoint(result);
                }
            }

            return GetAllOutputEndpointsInternal().FirstOrDefault(d => d.Name == name);
        }

        private InputEndpoint CloneInputEndpoint(InputEndpoint inputEndpoint)
        {
            InputEndpointApi.Api_CloneInputEndpointInfo(inputEndpoint.Info.DangerousGetHandle(), out var clonedInfo);
            return new InputEndpoint(clonedInfo, inputEndpoint.Context);
        }

        private OutputEndpoint CloneOutputEndpoint(OutputEndpoint outputEndpoint)
        {
            OutputEndpointApi.Api_CloneOutputEndpointInfo(outputEndpoint.Info.DangerousGetHandle(), out var clonedInfo);
            return new OutputEndpoint(clonedInfo, outputEndpoint.Context);
        }

        private ICollection<InputEndpoint> GetAllInputEndpointsInternal()
        {
            var result = InputEndpointApi.Api_GetEndpointsInfo(MidiConfiguration.GetConfigurationHandle(), MidiDevicesSession.GetSessionHandle(), out var endpointsInfo, out var endpointsCount, out var errorCode);
            NativeApiUtilities.HandleEndpointNativeApiResult(result, errorCode);

            var endpoints = new InputEndpoint[endpointsCount];

            for (int i = 0; i < endpointsCount; i++)
            {
                var info = Marshal.ReadIntPtr(endpointsInfo, i * IntPtr.Size);
                endpoints[i] = new InputEndpoint(info, MidiEndpoint.CreationContext.User);
            }

            InputEndpointApi.Api_FreeEndpointsInfo(endpointsInfo, endpointsCount);

            return endpoints;
        }

        private ICollection<OutputEndpoint> GetAllOutputEndpointsInternal()
        {
            var result = OutputEndpointApi.Api_GetEndpointsInfo(MidiConfiguration.GetConfigurationHandle(), MidiDevicesSession.GetSessionHandle(), out var endpointsInfo, out var endpointsCount, out var error);
            NativeApiUtilities.HandleEndpointNativeApiResult(result, error);

            var endpoints = new OutputEndpoint[endpointsCount];

            for (int i = 0; i < endpointsCount; i++)
            {
                var info = Marshal.ReadIntPtr(endpointsInfo, i * IntPtr.Size);
                endpoints[i] = new OutputEndpoint(info, MidiEndpoint.CreationContext.User);
            }

            OutputEndpointApi.Api_FreeEndpointsInfo(endpointsInfo, endpointsCount);

            return endpoints;
        }

        private void OnEndpointAdded(object sender, EndpointAddedRemovedEventArgs e)
        {
            lock (_inputEndpointsLock)
            {
                if (e.Endpoint is InputEndpoint inputEndpoint)
                    _inputEndpoints.Add(inputEndpoint);
            }

            lock (_outputEndpointsLock)
            {
                if (e.Endpoint is OutputEndpoint outputEndpoint)
                    _outputEndpoints.Add(outputEndpoint);
            }
        }

        private void OnEndpointRemoved(object sender, EndpointAddedRemovedEventArgs e)
        {
            lock (_inputEndpointsLock)
            {
                if (e.Endpoint is InputEndpoint inputEndpoint)
                    _inputEndpoints.Remove(inputEndpoint);
            }

            lock (_outputEndpointsLock)
            {
                if (e.Endpoint is OutputEndpoint outputEndpoint)
                    _outputEndpoints.Remove(outputEndpoint);
            }
        }

        #endregion
    }
}
