namespace ConvertReSharperReportToHtml
{
    internal sealed class RegionDto
    {
        public int StartLine { get; set; }

        public int StartColumn { get; set; }

        public int EndLine { get; set; }

        public int EndColumn { get; set; }

        public int CharOffset { get; set; }

        public int CharLength { get; set; }
    }
}
