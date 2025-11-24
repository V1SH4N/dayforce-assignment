using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Confluence
{
    public interface IConfluenceAttachmentsService
    {
        Task<JsonElement> GetConfluenceAttachmentsAsync(string baseurl, string pageId);
    }
}
