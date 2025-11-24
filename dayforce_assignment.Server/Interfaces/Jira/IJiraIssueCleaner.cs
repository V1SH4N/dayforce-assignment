using System.Text.Json;
using dayforce_assignment.Server.DTOs.Jira;

namespace dayforce_assignment.Server.Interfaces.Jira
{
    public interface IJiraIssueCleaner
    {
        JiraIssueDto CleanJiraIssue(JsonElement jiraStory, JsonElement jiraRemoteLinks);
    }
}
