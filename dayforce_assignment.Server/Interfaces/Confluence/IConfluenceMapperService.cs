using dayforce_assignment.Server.DTOs.Confluence;
using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Confluence
{
    public interface IConfluenceMapperService
    {
        ConfluencePageDto MapPageToDto(JsonElement confluencePage, JsonElement confluenceComments);

        ConfluencePageAttachmentsDto MapAttachmentsToDto(JsonElement attachment);

    }
}
