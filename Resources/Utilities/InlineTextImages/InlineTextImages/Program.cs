using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace InlineTextImages
{
    class Program
    {
        private record CellPoints(
            PointF TopLeft,
            PointF TopCenter,
            PointF TopRight,
            PointF CenterLeft,
            PointF Center,
            PointF CenterRight,
            PointF BottomLeft,
            PointF BottomCenter,
            PointF BottomRight);

        private static readonly Color BackAreaFillColor = ColorTranslator.FromHtml("#201F1E");
        private static readonly Color InnerBlockBorderColor = ColorTranslator.FromHtml("#5E5E5D");
        private static readonly Color SolidBlockBorderColor = ColorTranslator.FromHtml("#F8F8F8");
        private static readonly Color FontColor = ColorTranslator.FromHtml("#F8F8F8");
        private static readonly Color MergeAreaFillColor = ColorTranslator.FromHtml("#883B3A39");
        private static readonly Color BaseLineColor = ColorTranslator.FromHtml("#AA5E5E5D");

        private static readonly char[] CharsToIgnore = { '$', '[', ']' };

        static void Main(string[] args)
        {
            var filesPath = args[0];
            var symbolSize = int.Parse(args[1]);

            const string imagesDirectoryName = "images";
            const string generatedImagePrefix = "dwmgen_";

            Console.WriteLine("Cleaning up images folders...");
            Console.WriteLine("--------------------------------");

            var htmlFiles = Directory.GetFiles(filesPath, "*.html", SearchOption.AllDirectories);
            var directories = htmlFiles
                .Select(filePath => new FileInfo(filePath).Directory.FullName)
                .Distinct()
                .ToArray();

            foreach (var directoryPath in directories)
            {
                var directoryInfo = new DirectoryInfo(Path.Combine(directoryPath, imagesDirectoryName));
                Console.WriteLine($"{directoryInfo.FullName}...");

                if (directoryInfo.Exists)
                {
                    var generatedImagesFiles = directoryInfo.GetFiles($"{generatedImagePrefix}*.png", SearchOption.AllDirectories);

                    foreach (var fileInfo in generatedImagesFiles)
                    {
                        Console.Write($"{fileInfo.FullName}...");

                        try
                        {
                            fileInfo.Delete();
                            Console.WriteLine("DELETED");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"FAILED TO DELETE ({ex.Message})");
                        }
                    }
                }
                else
                    directoryInfo.Create();
            }

            Console.WriteLine("Processing files...");
            Console.WriteLine("--------------------------------");

            foreach (var filePath in htmlFiles)
            {
                Console.Write($"{filePath}...");

                var directoryInfo = new FileInfo(filePath).Directory;

                var text = File.ReadAllText(filePath);
                var processed = false;

                text = Regex.Replace(
                    text,
                    "<pre><code class=\"lang-image\">(.+?)<\\/code><\\/pre>",
                    m =>
                    {
                        var imageText = m.Groups[1].Value;
                        var image = CreateImage(imageText, symbolSize);

                        var imageFileName = $"{generatedImagePrefix}{Path.GetFileNameWithoutExtension(filePath)}-{m.Index}.png";
                        var path = Path.Combine(directoryInfo.FullName, imagesDirectoryName, imageFileName);
                        image.Save(path);

                        processed = true;

                        return $"<img src=\"{imagesDirectoryName}/{imageFileName}\" class=\"text-image\" />";
                    },
                    RegexOptions.Singleline);

                if (processed)
                    File.WriteAllText(filePath, text);

                Console.WriteLine(processed ? "OK" : "skipped");
            }
            
            Console.WriteLine("All done.");
        }

        private static Image CreateImage(string text, int symbolSize)
        {
            var lines = text
                .Split('\n', '\r')
                .Where(line => !string.IsNullOrEmpty(line))
                .ToArray();

            var width = lines.Max(line => line.Length);
            var height = lines.Length;
            
            var bitmap = new Bitmap(width * symbolSize, height * symbolSize, PixelFormat.Format32bppArgb);
            var realWidth = 0;
            
            using (var graphics = Graphics.FromImage(bitmap))
            {
                FillBackAreas(lines, graphics, symbolSize);
                FillMergeAreas(lines, graphics, symbolSize);

                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

                var fontSize = FindOptimalFontSize(graphics, symbolSize);
                var font = GetFont(fontSize);
                
                for (var y = 0; y < height; y++)
                {
                    var line = lines[y];
                    realWidth = Math.Max(realWidth, line.Length);
                    
                    for (var x = 0; x < width && x < line.Length; x++)
                    {
                        var s = line[x];

                        var cellPoints = GetCellPoints(x, y, symbolSize);

                        if (!DrawInnerBlock(s, graphics, symbolSize, cellPoints) &&
                            !DrawOuterBlock(s, graphics, symbolSize, x, y) &&
                            !DrawSolidBlock(s, graphics, symbolSize, x, y) &&
                            !DrawBaseLines(s, graphics, symbolSize, x, y) &&
                            !DrawSpecialSymbols(s, graphics, symbolSize, x, y) &&
                            !CharsToIgnore.Contains(s))
                        {
                            var symbolMeasure = graphics.MeasureString(s.ToString(), font);

                            graphics.DrawString(
                                s.ToString(),
                                font,
                                new SolidBrush(FontColor),
                                new RectangleF(
                                    x * symbolSize + (symbolSize - symbolMeasure.Width) / 2.0f,
                                    y * symbolSize + (symbolSize - symbolMeasure.Height) / 2.0f,
                                    symbolMeasure.Width,
                                    symbolMeasure.Height),
                                new StringFormat
                                {
                                    Alignment = StringAlignment.Center,
                                    LineAlignment = StringAlignment.Center,
                                });
                        }
                    }
                }
            }

            var result = new Bitmap(realWidth * symbolSize, height * symbolSize, PixelFormat.Format32bppArgb);

            using (var graphics = Graphics.FromImage(result))
            {
                graphics.DrawImageUnscaled(bitmap, new Point(0, 0));
            }

            return result;
        }

        private static CellPoints GetCellPoints(int x, int y, int symbolSize) => new CellPoints(
            new PointF(x * symbolSize, y * symbolSize),
            new PointF(x * symbolSize + symbolSize / 2.0f, y * symbolSize),
            new PointF(x * symbolSize + symbolSize, y * symbolSize),
            new PointF(x * symbolSize, y * symbolSize + symbolSize / 2.0f),
            new PointF(x * symbolSize + symbolSize / 2.0f, y * symbolSize + symbolSize / 2.0f),
            new PointF(x * symbolSize + symbolSize, y * symbolSize + symbolSize / 2.0f),
            new PointF(x * symbolSize, y * symbolSize + symbolSize),
            new PointF(x * symbolSize + symbolSize / 2.0f, y * symbolSize + symbolSize),
            new PointF(x * symbolSize + symbolSize, y * symbolSize + symbolSize));

        private static void FillMergeAreas(string[] lines, Graphics graphics, int symbolSize)
        {
            var brush = new SolidBrush(MergeAreaFillColor);

            for (var y = 0; y < lines.Length; y++)
            {
                var i = 0;
                var indicesToRemove = new List<int>();

                do
                {
                    var a = lines[y].IndexOf('[', i);
                    if (a < 0)
                        break;

                    var b = lines[y].IndexOf(']', a + 1);

                    graphics.FillRectangle(brush, new RectangleF(
                        a * symbolSize + symbolSize / 2.0f,
                        y * symbolSize + 1,
                        (b - a - 1) * symbolSize - symbolSize,
                        symbolSize - 2));

                    i = b + 1;

                    indicesToRemove.Add(a);
                    indicesToRemove.Add(b);
                }
                while (i >= 0 && i < lines[y].Length);

                var removedCharsCount = 0;
                foreach (var j in indicesToRemove)
                {
                    lines[y] = lines[y].Remove(j - removedCharsCount, 1);
                    removedCharsCount++;
                }
            }
        }

        private static void FillBackAreas(string[] lines, Graphics graphics, int symbolSize)
        {
            var regex = new Regex(@"\+[^ ]+?\+");
            var data = lines
                .Select((line, i) =>
                {
                    var mm = regex.Matches(line);
                    return mm.Select(m => new { LineIndex = i, Start = m.Index, Length = m.Length }).ToArray();
                })
                .ToArray();

            var rectangles = new List<(int X, int Y, int Width, int Height)>();

            for (var i = 0; i < data.Length; i++)
            {
                foreach (var d in data[i])
                {
                    var substring = lines[d.LineIndex].Substring(d.Start, d.Length);
                    if (substring.Contains('$'))
                        continue;

                    EscapeBackAreaBorder(lines, substring, d.LineIndex, d.Start, d.Length);

                    var bottomLineIndex = Enumerable.Range(d.LineIndex + 1, lines.Length)
                        .FirstOrDefault(j => data.Any(dd => dd.Any(ddd => ddd.LineIndex == j && ddd.Start == d.Start && ddd.Length == d.Length)));
                    if (bottomLineIndex == default)
                        continue;

                    substring = lines[bottomLineIndex].Substring(d.Start, d.Length);
                    EscapeBackAreaBorder(lines, substring, bottomLineIndex, d.Start, d.Length);

                    rectangles.Add((d.Start, d.LineIndex, d.Length, bottomLineIndex - d.LineIndex + 1));

                    for (var y = d.LineIndex; y <= bottomLineIndex; y++)
                    {
                        if (lines[y][d.Start] == '|')
                            lines[y] = lines[y].Remove(d.Start, 1).Insert(d.Start, "$");

                        if (lines[y][d.Start + d.Length - 1] == '|')
                            lines[y] = lines[y].Remove(d.Start + d.Length - 1, 1).Insert(d.Start + d.Length - 1, "$");
                    }
                }
            }

            var brush = new SolidBrush(BackAreaFillColor);

            foreach (var rect in rectangles)
            {
                graphics.FillRectangle(
                    brush,
                    new RectangleF(
                        rect.X * symbolSize + symbolSize / 2.0f,
                        rect.Y * symbolSize + symbolSize / 2.0f,
                        rect.Width * symbolSize - symbolSize,
                        rect.Height * symbolSize - symbolSize));
            }
        }

        private static void EscapeBackAreaBorder(string[] lines, string substring, int lineIndex, int start, int length)
        {
            var line = lines[lineIndex].Remove(start, length);
            line = line.Insert(start, substring.Replace('+', '$').Replace('-', '$'));
            lines[lineIndex] = line;
        }

        private static bool DrawBaseLines(char s, Graphics graphics, int symbolSize, int x, int y)
        {
            var pen = new Pen(new SolidBrush(BaseLineColor), 1)
            {
                DashPattern = new float[] { 2, 2 }
            };

            switch (s)
            {
                case '⁞':
                    graphics.DrawLine(
                        pen,
                        x * symbolSize + symbolSize / 2.0f,
                        y * symbolSize,
                        x * symbolSize + symbolSize / 2.0f,
                        y * symbolSize + symbolSize);
                    return true;
            }

            return false;
        }

        private static bool DrawOuterBlock(char s, Graphics graphics, int symbolSize, int x, int y)
        {
            switch (s)
            {
                case '·':
                    graphics.FillRectangle(
                        new HatchBrush(HatchStyle.DiagonalCross, Color.DarkGray, Color.Transparent),
                        x * symbolSize, y * symbolSize, symbolSize, symbolSize);
                    return true;
            }
            
            return false;
        }

        private static bool DrawSpecialSymbols(char s, Graphics graphics, int symbolSize, int x, int y)
        {
            var pen = new Pen(new SolidBrush(FontColor), 1);

            const int padding = 5;

            switch (s)
            {
                case '◊':
                    graphics.DrawPolygon(
                        pen,
                        new[]
                        {
                            new PointF(x * symbolSize + padding, y * symbolSize + symbolSize / 2.0f),
                            new PointF(x * symbolSize + symbolSize / 2.0f, y * symbolSize + padding),
                            new PointF(x * symbolSize + symbolSize - padding, y * symbolSize + symbolSize / 2.0f),
                            new PointF(x * symbolSize + symbolSize / 2.0f, y * symbolSize + symbolSize - padding),
                        });
                    return true;

                case '○':
                    graphics.DrawEllipse(
                        pen,
                        x * symbolSize + padding,
                        y * symbolSize + padding,
                        symbolSize - 2 * padding,
                        symbolSize - 2 * padding);
                    return true;

                case '□':
                    graphics.DrawRectangle(
                        pen,
                        x * symbolSize + padding,
                        y * symbolSize + padding,
                        symbolSize - 2 * padding,
                        symbolSize - 2 * padding);
                    return true;
            }

            return false;
        }

        private static bool DrawInnerBlock(char s, Graphics graphics, int symbolSize, CellPoints cellPoints)
        {
            var pen = new Pen(new SolidBrush(InnerBlockBorderColor), 1);
            
            switch (s)
            {
                case '┌':
                    graphics.DrawLines(pen, new[]
                    {
                        cellPoints.BottomCenter,
                        cellPoints.Center,
                        cellPoints.CenterRight
                    });
                    return true;
                
                case '┐':
                    graphics.DrawLines(pen, new []
                    {
                        cellPoints.BottomCenter,
                        cellPoints.Center,
                        cellPoints.CenterLeft
                    });
                    return true;
                
                case '└':
                    graphics.DrawLines(pen, new []
                    {
                        cellPoints.TopCenter,
                        cellPoints.Center,
                        cellPoints.CenterRight
                    });
                    return true;
                
                case '┘':
                    graphics.DrawLines(pen, new []
                    {
                        cellPoints.TopCenter,
                        cellPoints.Center,
                        cellPoints.CenterLeft
                    });
                    return true;
                
                case '─':
                    graphics.DrawLines(pen, new []
                    {
                        cellPoints.CenterLeft,
                        cellPoints.CenterRight,
                    });
                    return true;

                case '┬':
                    graphics.DrawLines(pen, new[]
                    {
                        cellPoints.CenterLeft,
                        cellPoints.CenterRight,
                    });
                    graphics.DrawLines(pen, new[]
                    {
                        cellPoints.Center,
                        cellPoints.BottomCenter,
                    });
                    return true;

                case '│':
                    graphics.DrawLines(pen, new []
                    {
                        cellPoints.TopCenter,
                        cellPoints.BottomCenter,
                    });
                    return true;

                case '├':
                    graphics.DrawLines(pen, new[]
                    {
                        cellPoints.TopCenter,
                        cellPoints.BottomCenter,
                    });
                    graphics.DrawLines(pen, new[]
                    {
                        cellPoints.Center,
                        cellPoints.CenterRight,
                    });
                    return true;
                    
                case '→':
                    graphics.DrawLines(pen, new[]
                    {
                        cellPoints.CenterLeft,
                        cellPoints.CenterRight,
                    });
                    graphics.DrawLines(pen, new[]
                    {
                        cellPoints.TopCenter,
                        cellPoints.CenterRight,
                        cellPoints.BottomCenter,
                    });
                    return true;

                case '←':
                    graphics.DrawLines(pen, new[]
                    {
                        cellPoints.CenterLeft,
                        cellPoints.CenterRight,
                    });
                    graphics.DrawLines(pen, new[]
                    {
                        cellPoints.TopCenter,
                        cellPoints.CenterLeft,
                        cellPoints.BottomCenter,
                    });
                    return true;

                case '↑':
                    graphics.DrawLines(pen, new[]
                    {
                        cellPoints.BottomCenter,
                        cellPoints.TopCenter,
                    });
                    graphics.DrawLines(pen, new[]
                    {
                        cellPoints.CenterLeft,
                        cellPoints.TopCenter,
                        cellPoints.CenterRight,
                    });
                    return true;

                case '┼':
                    graphics.DrawLines(pen, new[]
                    {
                        cellPoints.CenterLeft,
                        cellPoints.CenterRight,
                    });
                    graphics.DrawLines(pen, new[]
                    {
                        cellPoints.TopCenter,
                        cellPoints.BottomCenter,
                    });
                    return true;
            }
            
            return false;
        }

        private static bool DrawSolidBlock(char s, Graphics graphics, int symbolSize, int x, int y)
        {
            var pen = new Pen(new SolidBrush(SolidBlockBorderColor), 2);

            switch (s)
            {
                case '╔':
                    graphics.DrawLines(pen, new[]
                    {
                        new PointF(x * symbolSize + symbolSize / 2.0f, y * symbolSize + symbolSize),
                        new PointF(x * symbolSize + symbolSize / 2.0f, y * symbolSize + symbolSize / 2.0f),
                        new PointF(x * symbolSize + symbolSize, y * symbolSize + symbolSize / 2.0f),
                    });
                    return true;

                case '╗':
                    graphics.DrawLines(pen, new[]
                    {
                        new PointF(x * symbolSize + symbolSize / 2.0f, y * symbolSize + symbolSize),
                        new PointF(x * symbolSize + symbolSize / 2.0f, y * symbolSize + symbolSize / 2.0f),
                        new PointF(x * symbolSize, y * symbolSize + symbolSize / 2.0f),
                    });
                    return true;

                case '╚':
                    graphics.DrawLines(pen, new[]
                    {
                        new PointF(x * symbolSize + symbolSize / 2.0f, y * symbolSize),
                        new PointF(x * symbolSize + symbolSize / 2.0f, y * symbolSize + symbolSize / 2.0f),
                        new PointF(x * symbolSize + symbolSize, y * symbolSize + symbolSize / 2.0f),
                    });
                    return true;

                case '╝':
                    graphics.DrawLines(pen, new[]
                    {
                        new PointF(x * symbolSize + symbolSize / 2.0f, y * symbolSize),
                        new PointF(x * symbolSize + symbolSize / 2.0f, y * symbolSize + symbolSize / 2.0f),
                        new PointF(x * symbolSize, y * symbolSize + symbolSize / 2.0f),
                    });
                    return true;

                case '═':
                    graphics.DrawLines(pen, new[]
                    {
                        new PointF(x * symbolSize, y * symbolSize + symbolSize / 2.0f),
                        new PointF(x * symbolSize + symbolSize, y * symbolSize + symbolSize / 2.0f),
                    });
                    return true;

                case '║':
                    graphics.DrawLines(pen, new[]
                    {
                        new PointF(x * symbolSize + symbolSize / 2.0f, y * symbolSize),
                        new PointF(x * symbolSize + symbolSize / 2.0f, y * symbolSize + symbolSize),
                    });
                    return true;
            }

            return false;
        }

        private static float FindOptimalFontSize(Graphics graphics, int symbolSize) =>
            Enumerable.Range(1, int.MaxValue)
                .Select(size => new { Size = size, Measure = graphics.MeasureString("X", GetFont(size)) })
                .SkipWhile(size => size.Measure.Width < symbolSize && size.Measure.Height < symbolSize)
                .First()
                .Size - 1;

        private static Font GetFont(float emSize) => new Font(new FontFamily("Consolas"), emSize);
    }
}