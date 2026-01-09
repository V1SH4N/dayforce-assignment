using dayforce_assignment.Server.DTOs.Jira;
using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Jira
{
    public interface ITriageSubtaskService
    {
        Task<JsonElement> GetSubtaskAsync(JiraIssueDto jiraIssue, CancellationToken cancellationToken);
    }
}
