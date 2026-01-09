using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.EventSinks
{
    public interface ISseEventSink
    {
        Task ErrorEventAsync(string title, string detail, int statusCode, CancellationToken cancellationToken);
        Task JiraFetchedAsync(string jiraKey, string jiraTitle, CancellationToken cancellationToken);
        Task ConfluencePageStartAsync(string pageId, string title, CancellationToken cancellationToken);
        Task ConfluencePageFinishedAsync(string pageId, CancellationToken cancellationToken);
        Task ConfluencePageErrorAsync(string pageId, string error, CancellationToken cancellationToken);
        Task SubtaskStartAsync(string pageId, string title, CancellationToken cancellationToken);
        Task SubtaskFinishedAsync(string pageId, CancellationToken cancellationToken);
        Task SubtaskErrorAsync(string subtaskId, string error, CancellationToken cancellationToken);
        Task TestCaseGeneratedAsync(JsonElement testCases, CancellationToken cancellationToken);
        Task TestCasesFinishedAsync(CancellationToken cancellationToken);
        Task RequestCompleteAsync(CancellationToken cancellationToken);

    }

}
