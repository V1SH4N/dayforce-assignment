using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Interfaces.EventSinks;
using Microsoft.SemanticKernel.ChatCompletion;

namespace dayforce_assignment.Server.Interfaces.Orchestrator
{
    public interface IUserPromptBuilder
    {
        Task<ChatMessageContentItemCollection> BuildAsync(JiraIssueDto jiraIssue, bool IsBugIssue, bool summarizeAttachment, ISseEventSink events, CancellationToken cancellationToken);
    }
}
