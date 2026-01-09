using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Jira
{
    public interface IJiraHttpClientService
    {
        Task<JsonElement> GetIssueAsync(string jiraKey, CancellationToken cancellationToken);

        Task<JsonElement> GetIssueRemoteLinksAsync(string jiraKey, CancellationToken cancellationToken);

    }
}
