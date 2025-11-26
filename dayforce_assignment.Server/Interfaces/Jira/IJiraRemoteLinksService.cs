using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Jira
{
    public interface IJiraRemoteLinksService
    {
        Task<JsonElement> GetRemoteLinksAsync(string jiraId);
    }
}
