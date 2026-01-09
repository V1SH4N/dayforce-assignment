using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Confluence
{
    public interface IConfluenceHttpClientService
    {
        Task<JsonElement> GetPageAsync(string baseUrl, string pageId, CancellationToken cancellationToken);

        Task<JsonElement> GetAttachmentsAsync(string baseurl, string pageId, CancellationToken cancellationToken);

        Task<JsonElement> GetCommentsAsync(string baseurl, string pageId, CancellationToken cancellationToken);

    }
}
