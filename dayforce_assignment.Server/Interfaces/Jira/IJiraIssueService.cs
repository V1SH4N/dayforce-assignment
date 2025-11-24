using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Jira
{
    public interface IJiraIssueService
    {
        Task<JsonElement> GetJiraIssueAsync (string jiraId);
    }
}
