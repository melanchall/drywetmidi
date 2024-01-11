namespace ConvertReSharperReportToHtml
{
    internal sealed class RuleDto
    {
        public string Id { get; set; }

        public TextDto FullDescription { get; set; }

        public TextDto ShortDescription { get; set; }

        public RelationshipDto[] Relationships { get; set; }
    }
}
