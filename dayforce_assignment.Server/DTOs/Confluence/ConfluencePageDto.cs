namespace dayforce_assignment.Server.DTOs.Confluence
{
    public class ConfluencePageDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string BodyStorageValue { get; set; } = string.Empty;            
        public List<string> Comments { get; set; } = new List<string>();
    }
}
