using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;

namespace dayforce_assignment.Server.Interfaces.Confluence
{
    public interface IConfluencePageSearchFilterService
    {
        Task<ConfluenceSearchResultsDto> FilterSearchResultAsync (JiraIssueDto jiraStory, ConfluenceSearchResultsDto searchresult);

    }
}
