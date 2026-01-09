using dayforce_assignment.Server.DTOs.Common;

namespace dayforce_assignment.Server.DTOs.Jira
{
    public class JiraIssueDto
    {
        public string Key { get; set; } = string.Empty;
        public IssueType IssueType { get; set; } = IssueType.Unknown;
        public string Title { get; set; } = string.Empty;
        public ProjectInfo Project { get; set; } = new ProjectInfo();
        public List<IssueInfo> Subtasks { get; set; } = new List<IssueInfo>();
        public List<IssueInfo> OutwardIssueLinks { get; set; } = new List<IssueInfo>();
        public string Description { get; set; } = string.Empty;
        public string AcceptanceCriteria { get; set; } = string.Empty;
        public List<Attachment> Attachments { get; set; } = new List<Attachment>();
        public List<string> RemoteLinks { get; set; } = new List<string>();

    }

    public enum IssueType
    {
        Unknown = 0,
        Story = 1,
        Bug = 2
    }


    public class ProjectInfo
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class IssueInfo
    {
        public string Key { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
}
