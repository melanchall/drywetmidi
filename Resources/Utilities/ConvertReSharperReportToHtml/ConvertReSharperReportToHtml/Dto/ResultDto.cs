namespace ConvertReSharperReportToHtml
{
    internal sealed class ResultDto
    {
        public string RuleId { get; set; }

        public TextDto Message { get; set; }

        public LocationDto[] Locations { get; set; }
    }
}
