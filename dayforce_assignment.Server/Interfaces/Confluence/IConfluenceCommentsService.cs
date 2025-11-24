using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Confluence
{
    public interface IConfluenceCommentsService
    {
        Task<JsonElement> GetConfluenceCommentsAsync(string baseurl, string pageId);
    }
}
