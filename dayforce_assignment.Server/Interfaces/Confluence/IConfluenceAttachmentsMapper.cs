using dayforce_assignment.Server.DTOs.Confluence;
using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Confluence
{
    public interface IConfluenceAttachmentsMapper
    {
        ConfluencePageAttachmentsDto MapToDto(JsonElement attachment);
    }
}
