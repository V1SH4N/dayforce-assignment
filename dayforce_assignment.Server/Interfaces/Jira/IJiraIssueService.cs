using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Jira
{
    public interface IJiraIssueService
    {
        Task<JsonElement> GetIssueAsync (string jiraId);
    }
}
