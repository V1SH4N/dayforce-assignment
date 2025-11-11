using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.Interfaces.Confluence;
using Microsoft.Extensions.Azure;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Confluence
{
    //public class ConfluenceAttachmentsCleaner : IConfluenceAttachmentsCleaner
    //{
    //    public ConfluencePageAttachmentsDto CleanConfluenceAttachments(JsonElement payload)
    //    {
    //        var dto = new ConfluencePageAttachmentsDto();

    //        if (payload.ValueKind == JsonValueKind.Undefined || payload.ValueKind == JsonValueKind.Null)
    //            return dto;

    //        try
    //        {
    //            // Get base URL (if available)
    //            string? baseUrl = null;
    //            if (payload.TryGetProperty("_links", out var linksProp) &&
    //                linksProp.TryGetProperty("base", out var baseProp))
    //            {
    //                baseUrl = baseProp.GetString();
    //            }

    //            // Get attachments array
    //            if (!payload.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array)
    //                return dto;

    //            foreach (var item in results.EnumerateArray())
    //            {
    //                if (item.TryGetProperty("mediaType", out var mediaTypeProp) &&
    //                    mediaTypeProp.GetString() == "image/png")
    //                {
    //                    var title = item.TryGetProperty("title", out var titleProp)
    //                        ? titleProp.GetString()
    //                        : null;

    //                    var downloadLink = item.TryGetProperty("_links", out var linksNode) &&
    //                                       linksNode.TryGetProperty("download", out var downloadProp)
    //                        ? downloadProp.GetString()
    //                        : null;

    //                    if (!string.IsNullOrEmpty(baseUrl) && !string.IsNullOrEmpty(downloadLink))
    //                    {
    //                        dto.Attachments.Add(new Attachment
    //                        {
    //                            Title = title,
    //                            DownloadLink = baseUrl + downloadLink
    //                        });
    //                    }

    //                }
    //            }

    //            return dto;
    //        }
    //        catch
    //        {
    //            return new ConfluencePageAttachmentsDto();
    //        }
    //    }
    //}

    public class ConfluenceAttachmentsCleaner : IConfluenceAttachmentsCleaner
    {
        public ConfluencePageAttachmentsDto CleanConfluenceAttachments(JsonElement payload)
        {
            var dto = new ConfluencePageAttachmentsDto();

            if (payload.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
                return dto;

            // Get base URL
            string? baseUrl = GetStringProperty(payload, "_links", "base");

            // Get attachments array
            if (!payload.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array)
                return dto;

            foreach (var item in results.EnumerateArray())
            {
                if (GetStringProperty(item, "mediaType") != "image/png")
                    continue;

                var title = GetStringProperty(item, "title");
                var downloadLink = GetStringProperty(item, "_links", "download");

                if (!string.IsNullOrEmpty(baseUrl) && !string.IsNullOrEmpty(downloadLink))
                {
                    var fullUrl = new Uri(new Uri(baseUrl), downloadLink).ToString();
                    dto.Attachments.Add(new Attachment
                    {
                        Title = title,
                        DownloadLink = fullUrl
                    });
                }
            }

            return dto;
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
