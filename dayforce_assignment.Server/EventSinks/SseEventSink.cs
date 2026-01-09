using dayforce_assignment.Server.DTOs.Error;
using dayforce_assignment.Server.Interfaces.EventSinks;
using System.Text.Json;

namespace dayforce_assignment.Server.EventSinks
{
    public class SseEventSink : ISseEventSink
    {
        private readonly HttpResponse _response;

        public SseEventSink(HttpResponse response)
        {
            _response = response;
        }

        private async Task WriteEventAsync(string name, object data, CancellationToken cancellationToken)
        {
            var json = JsonSerializer.Serialize(data);
            await _response.WriteAsync($"event: {name}\ndata: {json}\n\n", cancellationToken);
            await _response.Body.FlushAsync(cancellationToken);
        }


        public Task ErrorEventAsync(string title, string detail, int statusCode, CancellationToken cancellationToken)
        {
            var error = new ErrorDto
            {
                Title = title,
                Detail = detail,
                StatusCode = statusCode
            };
            return WriteEventAsync("ErrorEvent", error, cancellationToken);
        }

        public Task JiraFetchedAsync(string jiraKey, string jiraTitle, CancellationToken cancellationToken) =>
            WriteEventAsync("jiraFetched", new { jiraKey, jiraTitle }, cancellationToken);

        public Task ConfluencePageStartAsync(string pageId, string title, CancellationToken cancellationToken) =>
            WriteEventAsync("confluencePageStart", new { pageId, title }, cancellationToken);

        public Task ConfluencePageFinishedAsync(string pageId, CancellationToken cancellationToken) =>
            WriteEventAsync("confluencePageFinished", new { pageId }, cancellationToken);

        public Task ConfluencePageErrorAsync(string pageId, string error, CancellationToken cancellationToken) =>
            WriteEventAsync("confluencePageError", new { pageId, error }, cancellationToken);

        public Task SubtaskStartAsync(string subtaskId, string title, CancellationToken cancellationToken) =>
            WriteEventAsync("subtaskStart", new { subtaskId, title }, cancellationToken);

        public Task SubtaskFinishedAsync(string subtaskId, CancellationToken cancellationToken) =>
            WriteEventAsync("subtaskFinished", new { subtaskId }, cancellationToken);

        public Task SubtaskErrorAsync(string subtaskId, string error, CancellationToken cancellationToken) =>
            WriteEventAsync("subtaskError", new { subtaskId, error }, cancellationToken);

        public Task TestCaseGeneratedAsync(JsonElement testCases, CancellationToken cancellationToken) =>
            WriteEventAsync("testCaseGenerated", testCases, cancellationToken);

        public Task TestCasesFinishedAsync(CancellationToken cancellationToken) =>
            WriteEventAsync("testCasesFinished", "", cancellationToken);

        public Task RequestCompleteAsync(CancellationToken cancellationToken) =>
            WriteEventAsync("requestComplete", "", cancellationToken);

    }

}