using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Exceptions;
using dayforce_assignment.Server.Interfaces.Jira;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Jira
{
    public class TriageSubtaskService : ITriageSubtaskService
    {
        private readonly IJiraIssueService _jiraIssueService;
        private readonly ICustomFieldService _customFieldService;

        public TriageSubtaskService(
            IJiraIssueService jiraIssueService,
            ICustomFieldService customFieldService)
        {
            _jiraIssueService = jiraIssueService;
            _customFieldService = customFieldService;
        }

        public async Task<JsonElement> GetSubTaskAsync(JiraIssueDto jiraIssue)
        {
            var jiraKey = jiraIssue?.Key ?? "unknown";

            try
            {
                if (jiraIssue == null)
                    throw new JiraTriageSubtaskProcessingException(jiraKey, "JiraIssueDto is null.");

                if (jiraIssue.Subtasks == null || !jiraIssue.Subtasks.Any())
                    throw new JiraTriageSubtaskNotFoundException(jiraKey);

                foreach (var subtask in jiraIssue.Subtasks)
                {
                    JsonElement jsonSubTaskIssue = await _jiraIssueService.GetIssueAsync(subtask.Key);

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

                throw new JiraTriageSubtaskNotFoundException(jiraKey);
            }
            catch (JsonException ex)
            {
                throw new JiraTriageSubtaskProcessingException(jiraKey, $"Invalid JSON structure: {ex.Message}");
            }
            catch (Exception ex) when (!(ex is JiraException))
            {
                throw new JiraTriageSubtaskProcessingException(jiraKey, $"An unexpected error has occured");
            }
        }
    }
}
