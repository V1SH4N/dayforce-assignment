using Microsoft.SemanticKernel.ChatCompletion;

namespace dayforce_assignment.Server.Interfaces.Orchestrator
{
    public interface IUserMessageBuilder
    {
        Task<ChatMessageContentItemCollection> BuildUserMessageAsync(string jiraId);

    }
}
