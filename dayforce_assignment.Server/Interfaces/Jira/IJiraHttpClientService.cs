using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Jira
{
    public interface IJiraHttpClientService
    {
        Task<JsonElement> GetIssueAsync(string jiraKey);

        Task<JsonElement> GetIssueRemoteLinksAsync(string jiraKey);

    }
}
