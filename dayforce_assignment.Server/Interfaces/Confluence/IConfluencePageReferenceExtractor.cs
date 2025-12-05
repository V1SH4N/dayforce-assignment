using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;

namespace dayforce_assignment.Server.Interfaces.Confluence
{
    public interface IConfluencePageReferenceExtractor
    {
        Task<ConfluencePageReferencesDto> GetReferencesAsync(JiraIssueDto jiraStory);
    }
}
