using dayforce_assignment.Server.DTOs.Jira;
using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Jira
{
    public interface IJiraMapperService
    {
        JiraIssueDto MapIssueToDto(JsonElement jsonIssue, JsonElement jsonRemoteLinks);

        TriageSubtaskDto MapTriageSubtaskToDto(JsonElement TriageSubtask);
    }
}
