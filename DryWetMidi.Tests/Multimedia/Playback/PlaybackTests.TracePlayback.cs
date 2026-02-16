using Melanchall.DryWetMidi.Multimedia;
using NUnit.Framework;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Melanchall.DryWetMidi.Tests.Multimedia
{
    [TestFixture]
    public sealed partial class PlaybackTests
    {
        #region Methods

        [Conditional("TRACE")]
        private static void SavePlaybackTraces(
            Playback playback,
            string label,
            ICollection<TimestampedEvent> expectedReceivedEvents,
            ICollection<TimestampedEvent> actualReceivedEvents)
        {
#if TRACE
            SavePlaybackActionsTrace(
                playback.ActionsTracer,
                label);
            SavePlaybackClockTrace(
                playback.ClockTracer,
                label,
                expectedReceivedEvents,
                actualReceivedEvents);
#endif
        }

        private static void SavePlaybackClockTrace(
            MidiClockTracer clockTracer,
            string label,
            ICollection<TimestampedEvent> expectedReceivedEvents,
            ICollection<TimestampedEvent> actualReceivedEvents)
        {
            var tracesDirectoryPath = GetPlaybackTracesDirectoryPath();

            //

            string GetTickDataLogFilePath(string subLabel)
            {
                var tickDataFileName = GetPlaybackTracesFileName($"{label}_{subLabel}");
                return Path.Combine(tracesDirectoryPath, $"{tickDataFileName}.log");
            }

            var tickTimesFilePath = GetTickDataLogFilePath("TickTimes");
            FileOperations.WriteAllLinesToFile(
                tickTimesFilePath,
                clockTracer.GetTickTimes()
                    .Select((tt, i) => $"{i}: {string.Join(",", tt)}"));

            void WriteTimesToFile(string subLabel, long[] times)
            {
                var timesFilePath = GetTickDataLogFilePath(subLabel);
                FileOperations.WriteAllLinesToFile(
                    timesFilePath,
                    new[] { string.Join(",", times) });
            }

            WriteTimesToFile("StartTimes", clockTracer.GetStartTimes());
            WriteTimesToFile("StopTimes", clockTracer.GetStopTimes());

            //

            var fileName = GetPlaybackTracesFileName(label);
            var filePath = Path.Combine(tracesDirectoryPath, $"{fileName}.png");

            const int graphWidth = 10000;
            const int graphHeight = 500;
            const int margin = 50;
            const int markerSize = 10;
            const int alternatingShift = markerSize * 2;

            var backgroundColor = new SKColor(30, 30, 30);

            var graphBorderColor = SKColors.DarkGray;
            var tickTimesColor = SKColors.Gray;
            var expectedEventsTimesColor = SKColors.Green;
            var actualEventsTimesColor = SKColors.OrangeRed;

            var imageInfo = new SKImageInfo(graphWidth + margin * 2, graphHeight + margin * 2);
            var surface = SKSurface.Create(imageInfo);
            var canvas = surface.Canvas;

            canvas.Clear(backgroundColor);
            canvas.DrawRect(
                margin,
                margin,
                graphWidth,
                graphHeight,
                new SKPaint
                {
                    Style = SKPaintStyle.Stroke,
                    Color = graphBorderColor,
                    StrokeWidth = 1
                });

            //

            void DrawStartStopTimes(long[] times, float max, bool start)
            {
                const int startStopMarkerSize = 20;

                foreach (var t in times)
                {
                    var x = margin + (t / (float)max) * graphWidth;
                    var color = start ? SKColors.Lime : SKColors.Red;

                    var markerPoints = start
                        ? new[]
                        {
                            new SKPoint(x - startStopMarkerSize / 2, (margin - startStopMarkerSize) / 2),
                            new SKPoint(x + startStopMarkerSize / 2, margin / 2),
                            new SKPoint(x - startStopMarkerSize / 2, (margin - startStopMarkerSize) / 2 + startStopMarkerSize)
                        }
                        : new[]
                        {
                            new SKPoint(x - startStopMarkerSize / 2, (margin - startStopMarkerSize) / 2),
                            new SKPoint(x + startStopMarkerSize / 2, (margin - startStopMarkerSize) / 2),
                            new SKPoint(x + startStopMarkerSize / 2, (margin - startStopMarkerSize) / 2 + startStopMarkerSize),
                            new SKPoint(x - startStopMarkerSize / 2, (margin - startStopMarkerSize) / 2 + startStopMarkerSize),
                        };

                    using (var path = new SKPath())
                    {
                        path.MoveTo(markerPoints[0]);

                        for (var i = 1; i < markerPoints.Length; i++)
                        {
                            path.LineTo(markerPoints[i]);
                        }

                        path.Close();

                        canvas.DrawPath(path, new SKPaint
                        {
                            Style = SKPaintStyle.Fill,
                            Color = color,
                            IsAntialias = true,
                        });
                    }

                    canvas.DrawLine(
                        x,
                        margin / 2,
                        x,
                        margin + 100,
                        new SKPaint
                        {
                            Style = SKPaintStyle.Stroke,
                            Color = color,
                            StrokeWidth = 5,
                            StrokeCap = SKStrokeCap.Round,
                            IsAntialias = true,
                        });
                }
            }

            var tickTimes = clockTracer.GetTickTimes().SelectMany(a => a).ToArray();
            var allTimes = tickTimes
                .Concat(clockTracer.GetStartTimes())
                .Concat(clockTracer.GetStopTimes())
                .Concat(expectedReceivedEvents.Select(e => (long)e.Time.TotalMilliseconds))
                .Concat(actualReceivedEvents.Select(e => (long)e.Time.TotalMilliseconds))
                .ToArray();

            var maxT = allTimes.Length == 0 ? 1 : allTimes.Max();
            if (maxT == 0)
                maxT = 1;

            //

            float GetTimeX(float time) =>
                margin + (time / (float)maxT) * graphWidth;

            //

            foreach (var time in tickTimes)
            {
                canvas.DrawLine(
                    GetTimeX(time),
                    margin,
                    GetTimeX(time),
                    margin + graphHeight,
                    new SKPaint
                    {
                        Style = SKPaintStyle.Stroke,
                        Color = tickTimesColor,
                        StrokeWidth = 1,
                        IsAntialias = true,
                    });
            }

            //

            DrawStartStopTimes(clockTracer.GetStartTimes(), maxT, true);
            DrawStartStopTimes(clockTracer.GetStopTimes(), maxT, false);

            //

            var expectedReceivedEventsEnumerator = expectedReceivedEvents.GetEnumerator();
            var actualReceivedEventsEnumerator = actualReceivedEvents.GetEnumerator();

            var i = 0;

            while (expectedReceivedEventsEnumerator.MoveNext() && actualReceivedEventsEnumerator.MoveNext())
            {
                var expected = expectedReceivedEventsEnumerator.Current;
                var actual = actualReceivedEventsEnumerator.Current;

                var yShift = alternatingShift * (i % 3);

                canvas.DrawLine(
                    GetTimeX((float)expected.Time.TotalMilliseconds),
                    margin + graphHeight / 3 + yShift,
                    GetTimeX((float)actual.Time.TotalMilliseconds),
                    margin + graphHeight - graphHeight / 3 + yShift,
                    new SKPaint
                    {
                        Style = SKPaintStyle.Stroke,
                        Color = SKColors.White,
                        StrokeWidth = 3,
                        IsAntialias = true,
                    });

                i++;
            }

            //

            void DrawEvents(ICollection<TimestampedEvent> events, int y, SKColor color)
            {
                var i = 0;

                foreach (var e in events)
                {
                    var yShift = alternatingShift * (i % 3);

                    canvas.DrawCircle(
                        GetTimeX((float)e.Time.TotalMilliseconds),
                        y + yShift,
                        markerSize,
                        new SKPaint
                        {
                            Style = SKPaintStyle.Fill,
                            Color = color,
                            IsAntialias = true,
                        });

                    canvas.DrawCircle(
                        GetTimeX((float)e.Time.TotalMilliseconds),
                        y + yShift,
                        markerSize,
                        new SKPaint
                        {
                            Style = SKPaintStyle.Stroke,
                            Color = SKColors.White,
                            StrokeWidth = 3,
                            IsAntialias = true,
                        });

                    i++;
                }
            }

            DrawEvents(expectedReceivedEvents, margin + graphHeight / 3, expectedEventsTimesColor);
            DrawEvents(actualReceivedEvents, margin + graphHeight - graphHeight / 3, actualEventsTimesColor);

            //

            using (var image = surface.Snapshot())
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            using (var stream = File.OpenWrite(filePath))
            {
                data.SaveTo(stream);
            }
        }

        private static void SavePlaybackActionsTrace(
            PlaybackActionsTracer actionsTracer,
            string label)
        {
            var tracesDirectoryPath = GetPlaybackTracesDirectoryPath();
            var fileName = GetPlaybackTracesFileName(label);
            var filePath = Path.Combine(tracesDirectoryPath, $"{fileName}.log");

            File.WriteAllLines(filePath, actionsTracer.GetTraces());
        }

        private static string GetPlaybackTracesFileName(string label)
        {
            var testName = GetTestName();
            var retryCount = TestContext.CurrentContext.CurrentRepeatCount;
            return $"{testName}{(string.IsNullOrWhiteSpace(label) ? string.Empty : $"_{label}")}_{retryCount}";
        }

        private static string GetPlaybackTracesRootDirectoryPath()
        {
            var artifactsStagingDirectory = Environment.GetEnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY");
            var buildId = Environment.GetEnvironmentVariable("BUILD_BUILDID");

            var tempPath = string.IsNullOrWhiteSpace(artifactsStagingDirectory)
                ? Path.GetTempPath()
                : Path.Combine(artifactsStagingDirectory, buildId);

            return Path.Combine(tempPath, "PlaybackTraces");
        }

        private static string GetPlaybackTracesDirectoryPath()
        {
            var rootPath = GetPlaybackTracesRootDirectoryPath();
            
            var directoryPath = Path.Combine(rootPath, GetTestName());
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);
            
            return directoryPath;
        }

        private static string GetTestName()
        {
            var testName = TestContext.CurrentContext.Test.Name;
            if (testName.StartsWith(nameof(CheckFilePlayback)))
            {
                var index = testName.IndexOf("MIDI files", StringComparison.OrdinalIgnoreCase);
                var testFileName = testName.Substring(index).Trim(')', '"');
                testName = $"{nameof(CheckFilePlayback)}({testFileName.Replace('/', '_').Replace('\\', '_')})";
            }

            return testName;
        }

        #endregion
    }
}
