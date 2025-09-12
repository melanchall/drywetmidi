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
            ICollection<SentReceivedEvent> expectedReceivedEvents,
            ICollection<SentReceivedEvent> actualReceivedEvents)
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
            ICollection<SentReceivedEvent> expectedReceivedEvents,
            ICollection<SentReceivedEvent> actualReceivedEvents)
        {
            var tracesDirectoryPath = GetPlaybackTracesDirectoryPath();
            var fileName = GetPlaybackTracesFileName(label);
            var filePath = Path.Combine(tracesDirectoryPath, $"{fileName}.png");

            const int graphWidth = 5000;
            const int graphHeight = 500;
            const int margin = 50;
            const int markerSize = 20;

            var backgroundColor = new SKColor(30, 30, 30);

            var graphBorderColor = SKColors.DarkGray;
            var tickTimesColor = SKColors.Gray;
            var expectedEventsTimesColor = SKColors.Lime;
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

            T[] Flatten<T>(IEnumerable<IEnumerable<T>> input) =>
                input.SelectMany(a => a).ToArray();

            //

            void DrawStartStopTimes(long[] times, long max, bool start)
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
                            StrokeWidth = 3,
                            IsAntialias = true,
                        });
                }
            }

            var tickTimes = clockTracer.GetTickTimes();
            var flattenTickTimes = Flatten(tickTimes);
            var maxT = flattenTickTimes.Length == 0 ? 1 : flattenTickTimes.Max();
            if (maxT == 0)
                maxT = 1;

            DrawStartStopTimes(clockTracer.GetStartTimes(), maxT, true);
            DrawStartStopTimes(clockTracer.GetStopTimes(), maxT, false);

            //

            if (expectedReceivedEvents.Count == actualReceivedEvents.Count)
            {
                foreach (var ab in expectedReceivedEvents.Zip(actualReceivedEvents, (a, b) => new { Expected = a, Actual = b }))
                {
                    canvas.DrawLine(
                        margin + (float)ab.Actual.Time.TotalMilliseconds / maxT * graphWidth,
                        margin + graphHeight / 3,
                        margin + (float)ab.Expected.Time.TotalMilliseconds / maxT * graphWidth,
                        margin + graphHeight - graphHeight / 3,
                        new SKPaint
                        {
                            Style = SKPaintStyle.Stroke,
                            Color = SKColors.Lime,
                            StrokeWidth = 3,
                            IsAntialias = true,
                        });
                }
            }

            //

            void DrawGraph(long[][] ms, SKColor color, bool drawMarker, float legendXOffset, int explicitY)
            {
                var flattenMs = Flatten(ms);
                if (!flattenMs.Any())
                    return;

                var minMs = flattenMs.Min();
                var maxMs = flattenMs.Max();

                var deltas = Flatten(ms.Select((tt, j) => tt.Select((t, i) => i > 0 ? t - ms[j][i - 1] : 0)));
                var minDelta = deltas.Min();
                var maxDelta = deltas.Max();

                for (var i = 0; i < ms.Length; i++)
                {
                    var current = ms[i].First();
                    var points = ms[i].Select(t =>
                    {
                        var divisor = maxDelta - minDelta;
                        if (divisor == 0)
                            divisor = 1;

                        var result = new SKPoint(
                            margin + (t / (float)maxMs) * graphWidth,
                            margin + graphHeight - (t - current) / divisor * graphHeight);

                        current = t;
                        return result;
                    })
                    .ToArray();

                    var linePaint = new SKPaint
                    {
                        Style = SKPaintStyle.Stroke,
                        Color = color,
                        StrokeWidth = 3,
                        StrokeCap = SKStrokeCap.Round,
                        IsAntialias = true
                    };

                    if (drawMarker)
                    {
                        canvas.DrawPoints(
                            SKPointMode.Points,
                            points.Select(p => new SKPoint(
                                p.X,
                                explicitY)).ToArray(),
                            new SKPaint
                            {
                                Color = color,
                                StrokeWidth = markerSize,
                                IsAntialias = true,
                                StrokeJoin = SKStrokeJoin.Round,
                                StrokeCap = SKStrokeCap.Round
                            });
                    }
                    else
                    {
                        foreach (var point in points)
                        {
                            canvas.DrawLine(
                                point.X,
                                point.Y,
                                point.X,
                                margin + graphHeight - (Math.Abs(point.Y - (margin + graphHeight)) <= 1 ? 10 : 0),
                                linePaint);
                        }
                    }
                }

                var font = new SKFont(SKTypeface.FromFamilyName("Consolas"), 30);
                var legendMargin = 10;

                font.MeasureText("0", out var rect);
                var height = rect.Height;

                void DrawLegend(string text, float x, float y)
                {
                    font.MeasureText(text, out rect);

                    var backMargin = legendMargin / 2;
                    canvas.DrawRect(
                        x + legendXOffset - backMargin,
                        y - rect.Height - backMargin,
                        rect.Width + backMargin * 2,
                        rect.Height + backMargin * 2,
                        new SKPaint
                        {
                            Style = SKPaintStyle.Fill,
                            Color = backgroundColor
                        });

                    canvas.DrawText(
                        text,
                        x + legendXOffset,
                        y,
                        font,
                        new SKPaint
                        {
                            Color = color,
                            IsAntialias = true,
                        });
                }

                DrawLegend(minDelta.ToString(), margin + legendMargin, margin + graphHeight - legendMargin);
                DrawLegend(maxDelta.ToString(), margin + legendMargin, margin + height + legendMargin);
            }

            //

            DrawGraph(
                tickTimes,
                tickTimesColor,
                false,
                0,
                0);

            DrawGraph(
                new[] { expectedReceivedEvents.Select(e => (long)e.Time.TotalMilliseconds).ToArray() },
                expectedEventsTimesColor,
                true,
                100,
                margin + graphHeight / 3);

            DrawGraph(
                new[] { actualReceivedEvents.Select(e => (long)e.Time.TotalMilliseconds).ToArray() },
                actualEventsTimesColor,
                true,
                200,
                margin + graphHeight - graphHeight / 3);

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
            var testName = TestContext.CurrentContext.Test.Name;
            var retryCount = TestContext.CurrentContext.CurrentRepeatCount;

            var fileName = testName;
            if (fileName.StartsWith(nameof(CheckFilePlayback)))
            {
                var index = fileName.IndexOf("MIDI files", StringComparison.OrdinalIgnoreCase);
                var testFileName = fileName.Substring(index);
                fileName = $"{nameof(CheckFilePlayback)}({testFileName.Replace('/', '_').Replace('\\', '_')})";
            }

            return $"{fileName}{(string.IsNullOrWhiteSpace(label) ? string.Empty : $"_{label}")}_{retryCount}";
        }

        private static string GetPlaybackTracesDirectoryPath()
        {
            var artifactsStagingDirectory = Environment.GetEnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY");
            var buildId = Environment.GetEnvironmentVariable("BUILD_BUILDID");

            var tempPath = string.IsNullOrWhiteSpace(artifactsStagingDirectory)
                ? Path.GetTempPath()
                : Path.Combine(artifactsStagingDirectory, buildId);

            return Path.Combine(tempPath, "PlaybackTraces");
        }

        #endregion
    }
}
