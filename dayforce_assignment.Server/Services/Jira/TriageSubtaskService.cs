using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Interfaces.Jira;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Jira
{
    public class TriageSubtaskService : ITriageSubtaskService
    {
        private readonly IJiraHttpClientService _jiraHttpClientService;
        private readonly ICustomFieldService _customFieldService;

        public TriageSubtaskService(
            IJiraHttpClientService jiraHttpClientService,
            ICustomFieldService customFieldService)
        {
            _jiraHttpClientService = jiraHttpClientService;
            _customFieldService = customFieldService;
        }

        // Searches for Triage subtask in Jira subtasks & Jira outward issue links. Returns undefined jsonElement if no triage subtask found.
        public async Task<JsonElement> GetSubtaskAsync(JiraIssueDto jiraIssue)
        {
            if (jiraIssue.Subtasks.Any())
            {
                var triageSubtask = await FindTriageIssueAsync(jiraIssue.Subtasks);
                if (triageSubtask.ValueKind != JsonValueKind.Undefined)
                    return triageSubtask;
            }

            if (jiraIssue.OutwardIssueLinks.Any())
            {
                var triageLinkedIssue = await FindTriageIssueAsync(jiraIssue.OutwardIssueLinks);
                if (triageLinkedIssue.ValueKind != JsonValueKind.Undefined)
                    return triageLinkedIssue;
            }

             return new JsonElement();
        }


        // Finds custom field mapping for subtask type & checks if type is traige. Returns undefined jsonelement if subtask type != triage.
        private async Task<JsonElement> FindTriageIssueAsync(IEnumerable<IssueInfo> relatedIssues)
        {
            foreach (IssueInfo issueInfo in relatedIssues)
            {
                JsonElement issue = await _jiraHttpClientService.GetIssueAsync(issueInfo.Key);

                string subTaskTypeFieldId = _customFieldService.GetCustomFieldId(issue, "Sub-Task Type");
                if (string.IsNullOrWhiteSpace(subTaskTypeFieldId))
                    continue;

                if(issue.TryGetProperty("fields", out var fields) &&
                    fields.TryGetProperty(subTaskTypeFieldId, out var subTaskType) &&
                    subTaskType.ValueKind != JsonValueKind.Null &&
                    subTaskType.TryGetProperty("value", out var subTaskTypeValue)&&
                    subTaskTypeValue.GetString() == "Triage")
                {
                    return issue;
                }
            }
            return new JsonElement();
        }
    }
}
