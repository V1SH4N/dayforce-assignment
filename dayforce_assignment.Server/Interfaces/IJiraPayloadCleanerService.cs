using System.Text.Json;
using dayforce_assignment.Server.DTOs;

namespace dayforce_assignment.Server.Interfaces
{
    public interface IJiraPayloadCleanerService
    {
        JiraStoryDto CleanJiraJson(JsonElement jiraJson);
    }
}
