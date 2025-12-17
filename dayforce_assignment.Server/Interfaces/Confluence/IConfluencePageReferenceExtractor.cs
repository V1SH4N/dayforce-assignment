using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;

namespace dayforce_assignment.Server.Interfaces.Confluence
{
    public interface IConfluencePageReferenceExtractor
    {
        public ConfluencePageReferencesDto GetReferences(JiraIssueDto jiraIssue);
    }
}
