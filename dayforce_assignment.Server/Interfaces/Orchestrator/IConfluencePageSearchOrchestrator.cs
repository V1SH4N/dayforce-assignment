using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;

namespace dayforce_assignment.Server.Interfaces.Orchestrator
{
    public interface IConfluencePageSearchOrchestrator
    {
        Task<ConfluencePageReferencesDto> GetConfluencePageReferencesAsync(JiraIssueDto jiraStory);

    }
}
