using dayforce_assignment.Server.DTOs.Common;

namespace dayforce_assignment.Server.DTOs.Jira
{
    public class TriageSubtaskDto
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
        public string Comments { get; set; } = string.Empty;
    }
}

