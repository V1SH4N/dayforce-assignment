using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;
using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Confluence
{
    public interface IConfluenceSearchService
    {
        Task<JsonElement> SearchPageAsync(string keywords);

        Task<ConfluenceSearchResultsDto> FilterResultAsync(JiraIssueDto jiraStory, ConfluenceSearchResultsDto searchresult);

        Task<ConfluenceSearchParametersDto> GetParametersAsync(JiraIssueDto jiraStory);

    }
}
