using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Confluence
{
    public interface IConfluencePageSearchService
    {
        Task<JsonElement> SearchConfluencePageAsync(string keywords);
    }
}
