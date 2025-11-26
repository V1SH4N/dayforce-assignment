using System.Text.Json;
using dayforce_assignment.Server.DTOs.Jira;

namespace dayforce_assignment.Server.Interfaces.Jira
{
    public interface IJiraIssueMapper
    {
        JiraIssueDto MapToDto(JsonElement jiraIssue, JsonElement jiraRemoteLinks);
    }
}
