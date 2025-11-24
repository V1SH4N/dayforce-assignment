using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Confluence
{
    public interface IConfluencePageService
    {
        Task<JsonElement> GetConfluencePageAsync(string baseUrl, string pageId);
    }
}
