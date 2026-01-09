using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;
using System.Collections.Concurrent;
using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Confluence
{
    public interface IConfluenceSearchService
    {
        Task<JsonElement> SearchPageAsync(string keywords, CancellationToken cancellationToken);

        Task<ConfluencePageReferencesDto> FilterResultAsync(JiraIssueDto jiraStory, ConcurrentDictionary<string, ConfluencePage> searchResults, CancellationToken cancellationToken);

        Task<ConfluenceSearchParametersDto> GetParametersAsync(JiraIssueDto jiraStory, CancellationToken cancellationToken);

    }
}
