using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Confluence
{
    public interface IConfluenceCommentsService
    {
        Task<JsonElement> GetCommentsAsync(string baseurl, string pageId);
    }
}
