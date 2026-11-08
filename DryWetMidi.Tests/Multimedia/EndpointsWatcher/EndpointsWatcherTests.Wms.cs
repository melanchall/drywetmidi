#if TEST
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Attributes;
using Melanchall.DryWetMidi.Tests.Utilities;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [NonParallelizable]
    public sealed partial class EndpointsWatcherTests
    {
        #region Test methods

        [Test]
        [WinOnly]
        [VirtualDeviceApiRequired]
        public void WmsPublicWatcherCallbacksCanEnumerateOpenSendAndDisposeEndpoints()
        {
            WaitPreviousNotifications();

            var survivingDevice = VirtualDevice.Create("WmsSurvivor-" + Guid.NewGuid());
            var watchedName = "WmsWatcher-" + Guid.NewGuid();
            var addedIds = new ConcurrentDictionary<string, byte>();
            var removedIds = new ConcurrentDictionary<string, byte>();
            var callbackExceptions = new ConcurrentQueue<Exception>();
            var watcher = EndpointsWatcher.Instance;
            VirtualDevice watchedDevice = null;
            Task creationTask = null;
            Task removalTask = null;

            EventHandler<EndpointAddedRemovedEventArgs> addedHandler = (_, eventArgs) =>
            {
                try
                {
                    if (eventArgs.Endpoint.Name != watchedName)
                        return;

                    EnumerateWmsPublicEndpoints();

                    var inputEndpoint = eventArgs.Endpoint as InputEndpoint;
                    if (inputEndpoint != null)
                    {
                        inputEndpoint.StartEventsListening();
                    }
                    else
                    {
                        var outputEndpoint = (OutputEndpoint)eventArgs.Endpoint;
                        outputEndpoint.PrepareForEventsSending();
                        outputEndpoint.SendEvent(new NoteOnEvent());
                    }

                    var endpointId = eventArgs.Endpoint.Id;
                    eventArgs.Endpoint.Dispose();
                    addedIds.TryAdd(endpointId, 0);
                }
                catch (Exception exception)
                {
                    callbackExceptions.Enqueue(exception);
                }
                finally
                {
                    DisposeWmsCallbackEndpoint(eventArgs.Endpoint, callbackExceptions);
                }
            };

            EventHandler<EndpointAddedRemovedEventArgs> removedHandler = (_, eventArgs) =>
            {
                try
                {
                    var endpointId = eventArgs.Endpoint.Id;
                    if (!addedIds.ContainsKey(endpointId))
                        return;

                    EnumerateWmsPublicEndpoints();

                    using (var inputEndpoint = InputEndpoint.GetByName(survivingDevice.Name))
                    using (var outputEndpoint = OutputEndpoint.GetByName(survivingDevice.Name))
                    {
                        inputEndpoint.StartEventsListening();
                        outputEndpoint.PrepareForEventsSending();
                        outputEndpoint.SendEvent(new NoteOnEvent());
                    }

                    eventArgs.Endpoint.Dispose();
                    removedIds.TryAdd(endpointId, 0);
                }
                catch (Exception exception)
                {
                    callbackExceptions.Enqueue(exception);
                }
                finally
                {
                    DisposeWmsCallbackEndpoint(eventArgs.Endpoint, callbackExceptions);
                }
            };

            watcher.EndpointAdded += addedHandler;
            watcher.EndpointRemoved += removedHandler;

            try
            {
                creationTask = Task.Run(() => watchedDevice = VirtualDevice.Create(watchedName));

                ClassicAssert.IsTrue(
                    creationTask.Wait(WmsTestContext.Timeout),
                    "Virtual creation blocked while a public Added handler reentered the native API.");
                ClassicAssert.IsTrue(
                    WaitOperations.Wait(() => addedIds.Count == 2 || !callbackExceptions.IsEmpty, WmsTestContext.Timeout),
                    "Added handlers did not finish enumeration, open, send and disposal within the bounded wait.");
                ClassicAssert.IsTrue(callbackExceptions.IsEmpty, $"Added callback failed: {string.Join("; ", callbackExceptions)}.");
                ClassicAssert.AreEqual(2, addedIds.Count, "Added callback did not exercise both input and output endpoints.");

                removalTask = Task.Run(() => watchedDevice.Dispose());

                ClassicAssert.IsTrue(
                    removalTask.Wait(WmsTestContext.Timeout),
                    "Virtual removal blocked while a public Removed handler reentered the native API.");
                ClassicAssert.IsTrue(
                    WaitOperations.Wait(() => removedIds.Count == 2 || !callbackExceptions.IsEmpty, WmsTestContext.Timeout),
                    "Removed handlers did not finish enumeration and surviving-endpoint operations within the bounded wait.");
                ClassicAssert.IsTrue(callbackExceptions.IsEmpty, $"Removed callback failed: {string.Join("; ", callbackExceptions)}.");
                ClassicAssert.AreEqual(2, removedIds.Count, "Removed callback did not exercise both endpoint directions.");
            }
            finally
            {
                watcher.EndpointAdded -= addedHandler;
                watcher.EndpointRemoved -= removedHandler;

                // Native reentrancy failures must not turn teardown into an unbounded wait.
                if ((creationTask == null || creationTask.IsCompleted) &&
                    (removalTask == null || removalTask.IsCompleted))
                {
                    var cleanupTask = Task.Run(() =>
                    {
                        watchedDevice?.Dispose();
                        survivingDevice.Dispose();
                    });

                    ClassicAssert.IsTrue(
                        cleanupTask.Wait(WmsTestContext.Timeout),
                        "Watcher cleanup blocked after handlers were detached.");
                }
                else
                {
                    var outstandingTask = removalTask ?? creationTask;
                    outstandingTask.ContinueWith(_ =>
                    {
                        watchedDevice?.Dispose();
                        survivingDevice.Dispose();
                    });
                }
            }
        }

        #endregion

        #region Private methods

        private static void EnumerateWmsPublicEndpoints()
        {
            foreach (var inputEndpoint in InputEndpoint.GetAll())
            {
                inputEndpoint.Dispose();
            }

            foreach (var outputEndpoint in OutputEndpoint.GetAll())
            {
                outputEndpoint.Dispose();
            }
        }

        private static void DisposeWmsCallbackEndpoint(MidiEndpoint endpoint, ConcurrentQueue<Exception> callbackExceptions)
        {
            try
            {
                endpoint.Dispose();
            }
            catch (Exception exception)
            {
                callbackExceptions.Enqueue(exception);
            }
        }

        #endregion
    }
}
#endif
