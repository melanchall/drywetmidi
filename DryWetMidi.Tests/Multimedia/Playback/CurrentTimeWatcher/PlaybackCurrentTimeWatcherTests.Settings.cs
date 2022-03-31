using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tests.Common;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackCurrentTimeWatcherTests
    {
        #region Test methods

        [Test]
        public void WatchOnlyRunningPlaybacks_SinglePlayback_NotRunning()
        {
            var eventFired = false;

            using (var watcher = GetWatcherForOnlyRunningPlaybacks())
            using (var playback = GetLongPlayback())
            {
                watcher.CurrentTimeChanged += (_, __) => eventFired = true;
                watcher.AddPlayback(playback);


                watcher.Start();
                WaitOperations.Wait(TimeSpan.FromSeconds(1));
                watcher.Stop();
            }

            Assert.IsFalse(eventFired, "Event is fired.");
        }

        [Test]
        public void WatchOnlyRunningPlaybacks_SinglePlayback_ChangedFromRunningToNotRunning()
        {
            var objects = new List<object>();
            var objectsCount = 0;

            using (var watcher = GetWatcherForOnlyRunningPlaybacks())
            using (var playback = GetLongPlayback())
            {
                watcher.CurrentTimeChanged += (_, __) => objects.Add(new object());
                watcher.AddPlayback(playback);

                playback.Start();
                watcher.Start();

                WaitOperations.Wait(TimeSpan.FromSeconds(1));
                playback.Stop();
                objectsCount = objects.Count;

                WaitOperations.Wait(TimeSpan.FromSeconds(1));
                watcher.Stop();
            }

            Assert.Greater(objectsCount, 0, "Event was not fired at all.");
            Assert.AreEqual(objectsCount, objects.Count, "Event is fired after playback stopped.");
        }

        [Test]
        public void WatchOnlyRunningPlaybacks_MultiplePlaybacks_NotRunning()
        {
            var eventFired = false;

            using (var watcher = GetWatcherForOnlyRunningPlaybacks())
            using (var playback1 = GetLongPlayback())
            using (var playback2 = GetLongPlayback())
            {
                watcher.CurrentTimeChanged += (_, __) => eventFired = true;
                watcher.AddPlayback(playback1);
                watcher.AddPlayback(playback2);

                watcher.Start();
                WaitOperations.Wait(TimeSpan.FromSeconds(1));
                watcher.Stop();
            }

            Assert.IsFalse(eventFired, "Event is fired.");
        }

        [Test]
        public void WatchOnlyRunningPlaybacks_MultiplePlaybacks_OneRunning()
        {
            var firstPlayback = true;

            using (var watcher = GetWatcherForOnlyRunningPlaybacks())
            using (var playback1 = GetLongPlayback())
            using (var playback2 = GetLongPlayback())
            {
                watcher.CurrentTimeChanged += (_, e) => firstPlayback &= e.Times.All(t => t.Playback != playback2);
                watcher.AddPlayback(playback1);
                watcher.AddPlayback(playback2);

                playback1.Start();
                watcher.Start();
                WaitOperations.Wait(TimeSpan.FromSeconds(1));
                watcher.Stop();
            }

            Assert.IsTrue(firstPlayback, "Second playback time reported.");
        }

        #endregion

        #region Private methods

        private static PlaybackCurrentTimeWatcher GetWatcherForOnlyRunningPlaybacks() =>
            new PlaybackCurrentTimeWatcher(new PlaybackCurrentTimeWatcherSettings
            {
                WatchOnlyRunningPlaybacks = true
            });

        #endregion
    }
}
