namespace dayforce_assignment.Server.DTOs.Confluence
{
    public class ConfluencePageAttachmentsDto
    {
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
    }

    public class Attachment
    {
        public string Title { get; set; } = string.Empty;
        public string DownloadLink { get; set; } = string.Empty;
        public string mediaType { get; set; } = string.Empty;
    }
}
