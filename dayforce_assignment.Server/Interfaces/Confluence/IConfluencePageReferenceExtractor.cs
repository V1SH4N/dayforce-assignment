using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;
using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Confluence
{
    public interface IConfluencePageReferenceExtractor
    {
        Task<ConfluencePageReferencesDto> GetConfluencePageReferencesAsync(JiraIssueDto jiraStory);
    }
}
