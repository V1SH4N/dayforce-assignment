using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Confluence
{
    public interface IConfluenceAttachmentsService
    {
        Task<JsonElement> GetAttachmentsAsync(string baseurl, string pageId);
    }
}
