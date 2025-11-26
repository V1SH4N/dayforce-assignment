using dayforce_assignment.Server.DTOs.Common;
using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.Exceptions;
using dayforce_assignment.Server.Interfaces.Confluence;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Confluence
{
    public class ConfluenceAttachmentsMapper : IConfluenceAttachmentsMapper
    {
        public ConfluencePageAttachmentsDto MapToDto(JsonElement payload)
        {
            try
            {
                var dto = new ConfluencePageAttachmentsDto();

                if (payload.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
                    return dto;

                // Get base URL
                string? baseUrl = GetStringProperty(payload, "_links", "base");
                if (string.IsNullOrWhiteSpace(baseUrl))
                    return dto;
                // Get attachments array
                if (!payload.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array)
                    return dto;

                foreach (var item in results.EnumerateArray())
                {
                    var mediaType = GetStringProperty(item, "mediaType");

                    if (string.IsNullOrWhiteSpace(mediaType) ||
                        !(mediaType.StartsWith("image/") || mediaType.StartsWith("text/")))
                        continue; // ignore unsupported attachment types

                    //var title = GetStringProperty(item, "title") ?? "Unnamed Attachment";
                    var downloadLink = GetStringProperty(item, "_links", "download");

                    if (string.IsNullOrWhiteSpace(downloadLink))
                        continue;
                    Console.WriteLine(baseUrl);

                    var fullUrl = $"{baseUrl.TrimEnd('/')}/{downloadLink.TrimStart('/')}";
                    Console.WriteLine(fullUrl);


                    dto.Attachments.Add(new Attachment
                    {
                        //Title = title,
                        DownloadLink = fullUrl,
                        MediaType = mediaType
                    });
                }

                return dto;
            }
            catch (Exception ex) when (!(ex is DomainException))
            {
                // Optionally include page ID if available
                string pageId = payload.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "unknown" : "unknown";
                throw new ConfluenceAttachmentsParsingException(pageId, ex.Message);
            }
        }

        private static string? GetStringProperty(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) ? prop.GetString() : null;
        }

        private static string? GetStringProperty(JsonElement element, string parentProperty, string childProperty)
        {
            if (element.TryGetProperty(parentProperty, out var parentProp))
                return GetStringProperty(parentProp, childProperty);
            return null;
        }
    }
}
