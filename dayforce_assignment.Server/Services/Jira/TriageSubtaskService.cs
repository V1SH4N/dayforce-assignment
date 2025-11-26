//using dayforce_assignment.Server.DTOs.Jira;
//using dayforce_assignment.Server.Interfaces.Jira;
//using Microsoft.AspNetCore.SignalR;
//using System.Text.Json;
//using System.Threading.Tasks;

//namespace dayforce_assignment.Server.Services.Jira
//{
//    public class TriageSubtaskService: ITriageSubtaskService
//    {
//        private readonly IJiraIssueService _jiraIssueService;
//        private readonly ICustomFieldService _customFieldService;

//        public TriageSubtaskService(IJiraIssueService jiraIssueService, ICustomFieldService customFieldService)
//        {
//            _jiraIssueService = jiraIssueService;
//            _customFieldService = customFieldService;
//        }

//        public async Task<JsonElement> GetTriageSubTaskAsync(JiraIssueDto jiraIssue)
//        {
//            foreach (var subtask in jiraIssue.Subtasks)
//            {
//                JsonElement rawSubTaskIssue = await _jiraIssueService.GetJiraIssueAsync(subtask.Key);
//                string subtaskTypeFieldId = _customFieldService.GetCustomFieldId(rawSubTaskIssue, "Sub-Task Type");

//                if (!string.IsNullOrWhiteSpace(subtaskTypeFieldId)) 
//                {
//                    if(rawSubTaskIssue.TryGetProperty("fields", out var fields) &&
//                        fields.TryGetProperty(subtaskTypeFieldId, out var subtaskType) &&
//                        subtaskType.TryGetProperty("value", out var subtaskValue)) 
//                    {
//                        if (subtaskValue.GetString() == "Triage")
//                            return rawSubTaskIssue;     
//                    }
//                }
//            }
//            return new JsonElement();
//        }
//    }
//}

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

        public async Task<JsonElement> GetTriageSubTaskAsync(JiraIssueDto jiraIssue)
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
                    JsonElement rawSubTaskIssue = await _jiraIssueService.GetIssueAsync(subtask.Key);

                    string subtaskTypeFieldId = _customFieldService.GetCustomFieldId(rawSubTaskIssue, "Sub-Task Type");

                    if (string.IsNullOrWhiteSpace(subtaskTypeFieldId))
                        continue;

                    if (!rawSubTaskIssue.TryGetProperty("fields", out var fields))
                        continue;

                    if (!fields.TryGetProperty(subtaskTypeFieldId, out var subtaskType))
                        continue;

                    if (!subtaskType.TryGetProperty("value", out var subtaskValue))
                        continue;

                    if (subtaskValue.GetString() == "Triage")
                        return rawSubTaskIssue;
                }

                throw new JiraTriageSubtaskNotFoundException(jiraKey);
            }
            catch (JsonException ex)
            {
                throw new JiraTriageSubtaskProcessingException(jiraKey, $"Invalid JSON structure: {ex.Message}");
            }
            catch (Exception ex) when (!(ex is JiraException))
            {
                throw new JiraTriageSubtaskProcessingException(jiraKey, $"Unexpected error: {ex.Message}");
            }
        }
    }
}
