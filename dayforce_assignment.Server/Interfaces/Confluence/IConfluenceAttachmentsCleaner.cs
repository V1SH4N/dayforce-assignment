using dayforce_assignment.Server.DTOs.Confluence;
using System.Text.Json;

namespace dayforce_assignment.Server.Interfaces.Confluence
{
    public interface IConfluenceAttachmentsCleaner
    {
        ConfluencePageAttachmentsDto CleanConfluenceAttachments(JsonElement attachment);
    }
}
