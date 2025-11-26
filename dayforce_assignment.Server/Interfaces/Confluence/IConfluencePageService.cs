using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Confluence
{
    public interface IConfluencePageService
    {
        Task<JsonElement> GetPageAsync(string baseUrl, string pageId);
    }
}
