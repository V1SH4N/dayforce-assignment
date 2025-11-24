namespace dayforce_assignment.Server.DTOs.Jira
{
    public class JiraIssueDto
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string ParentKey { get; set; } = string.Empty;
        public ProjectInfo Project { get; set; } = new ProjectInfo();
        public List<SubtaskInfo> Subtasks { get; set; } = new List<SubtaskInfo>();
        public string DocContent { get; set; } = string.Empty;
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
        public List<string>? RemoteLinks { get; set; }

    }

    public class ProjectInfo
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class SubtaskInfo
    {
        public string Key { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
    }

    public class Attachment
    {
        public string DownloadLink { get; set; } = string.Empty;        
        public string MediaType { get; set; } = string.Empty;
    }
}
