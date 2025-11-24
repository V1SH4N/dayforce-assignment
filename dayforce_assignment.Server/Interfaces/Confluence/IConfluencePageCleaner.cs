using dayforce_assignment.Server.DTOs.Confluence;
using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Confluence
{
    public interface IConfluencePageCleaner
    {
        ConfluencePageDto CleanConfluencePage(JsonElement confluencePage, JsonElement confluenceComments);
    }
}
