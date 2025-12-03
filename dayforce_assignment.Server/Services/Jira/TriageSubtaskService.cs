using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Exceptions;
using dayforce_assignment.Server.Interfaces.Jira;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Jira
{
    public class TriageSubtaskService : ITriageSubtaskService
    {
        private readonly IJiraHttpClientService _jiraHttpClientService;
        private readonly ICustomFieldService _customFieldService;
        private readonly ILogger<TriageSubtaskService> _logger;

        public TriageSubtaskService(
            IJiraHttpClientService jiraHttpClientService,
            ICustomFieldService customFieldService,
            ILogger<TriageSubtaskService> logger)
        {
            _jiraHttpClientService = jiraHttpClientService;
            _customFieldService = customFieldService;
            _logger = logger;
        }

        // Searches for Triage subtask and return the json subtask issue.
        public async Task<JsonElement> GetSubtaskAsync(JiraIssueDto jiraIssue)
        {
            if (jiraIssue.Subtasks.Any())
            {
                JsonElement jsonSubTaskIssue = new JsonElement();

                foreach (SubtaskInfo subtask in jiraIssue.Subtasks)
                {
                    try
                    {
                        jsonSubTaskIssue = await _jiraHttpClientService.GetIssueAsync(subtask.Key);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to fetch Jira subtask {SubtaskKey}", subtask.Key);
                        continue;
                    }

                    string subtaskTypeFieldId = _customFieldService.GetCustomFieldId(jsonSubTaskIssue, "Sub-Task Type");

                    if (string.IsNullOrWhiteSpace(subtaskTypeFieldId))
                        continue;

                    if (!jsonSubTaskIssue.TryGetProperty("fields", out var fields))
                        continue;

                    if (!fields.TryGetProperty(subtaskTypeFieldId, out var subtaskType))
                        continue;

                    if (!subtaskType.TryGetProperty("value", out var subtaskValue))
                        continue;

                    if (subtaskValue.GetString() == "Triage")
                        return jsonSubTaskIssue;
                }
            }
            return new JsonElement();
        }
    }
}
