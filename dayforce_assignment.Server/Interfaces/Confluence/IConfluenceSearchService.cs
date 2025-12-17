using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;
using System.Collections.Concurrent;
using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Confluence
{
    public interface IConfluenceSearchService
    {
        Task<JsonElement> SearchPageAsync(string keywords);

        Task<ConfluencePageReferencesDto> FilterResultAsync(JiraIssueDto jiraStory, ConcurrentDictionary<string, ConfluencePage> searchResults);

        Task<ConfluenceSearchParametersDto> GetParametersAsync(JiraIssueDto jiraStory);

    }
}
