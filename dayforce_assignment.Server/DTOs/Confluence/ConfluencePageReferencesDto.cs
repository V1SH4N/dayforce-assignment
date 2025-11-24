namespace dayforce_assignment.Server.DTOs.Confluence
{
    public class ConfluencePageReferencesDto
    {
        public List<ConfluencePage> ConfluencePages { get; set; } = new();

    }

    public class ConfluencePage
    {
        public string baseUrl { get; set; } = string.Empty;
        public string pageId { get; set; } = string.Empty;
    }

}
