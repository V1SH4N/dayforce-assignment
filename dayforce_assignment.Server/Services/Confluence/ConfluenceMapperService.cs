using dayforce_assignment.Server.DTOs.Common;
using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.Interfaces.Confluence;
using HtmlAgilityPack;
using System.Text;
using System.Text.Json;
using System.Web;

namespace dayforce_assignment.Server.Services.Confluence
{
    public class ConfluenceMapperService : IConfluenceMapperService
    {
        // Maps confluence page & comments to ConfluencePageDto
        public ConfluencePageDto MapPageToDto(JsonElement confluencePage, JsonElement confluenceComments)
        {
            // Extract page ID
            string pageId = confluencePage.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? string.Empty : string.Empty;

            // Extract title
            string title = confluencePage.TryGetProperty("title", out var titleProp) ? titleProp.GetString() ?? string.Empty : string.Empty;

            // Extract page body
            var pageBody = ExtractBodyValue(confluencePage);

            // Extract comments body into a list
            var commentsList = new List<string>();
            if (confluenceComments.ValueKind != JsonValueKind.Undefined &&
                confluenceComments.ValueKind != JsonValueKind.Null &&
                confluenceComments.TryGetProperty("results", out var commentsResults))
            {
                foreach (var comment in commentsResults.EnumerateArray())
                {
                    var commentBody = ExtractBodyValue(comment);
                    var cleaned = CleanHtml(commentBody);
                    if (!string.IsNullOrWhiteSpace(cleaned))
                        commentsList.Add(cleaned);
                }
            }

            return new ConfluencePageDto
            {
                Id = pageId,
                Title = title,
                Body = CleanHtml(pageBody),
                Comments = commentsList
            };
        }


        private static string ExtractBodyValue(JsonElement root)
        {
            if (root.TryGetProperty("body", out var bodyProp) &&
                bodyProp.TryGetProperty("storage", out var storageProp) &&
                storageProp.TryGetProperty("value", out var valueProp))
            {
                return valueProp.GetString() ?? string.Empty;
            }

            return string.Empty;
        }

        private static string CleanHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Remove macros, script, style elements
            RemoveNodes(doc.DocumentNode, "//*[local-name()='macro'] | //script | //style");

            var sb = new StringBuilder();
            var textNodes = doc.DocumentNode.SelectNodes("//text()");
            if (textNodes != null)
            {
                foreach (var textNode in textNodes)
                {
                    var text = textNode.InnerText;
                    if (!string.IsNullOrWhiteSpace(text))
                        sb.AppendLine(text.Trim());
                }
            }

            return HttpUtility.HtmlDecode(sb.ToString().Trim());
        }

        private static void RemoveNodes(HtmlNode root, string xPath)
        {
            var nodes = root.SelectNodes(xPath);
            if (nodes == null) return;

            foreach (var node in nodes)
                node.Remove();
        }



        // Maps confluence attachments to ConfluencePageAttachmentsDto
        public ConfluencePageAttachmentsDto MapAttachmentsToDto(JsonElement payload)
        {

                var dto = new ConfluencePageAttachmentsDto();

                if (payload.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
                    return dto;

                // Get base URL
                string baseUrl = GetStringProperty(payload, "_links", "base") ?? string.Empty;
                if (string.IsNullOrWhiteSpace(baseUrl))
                    return dto;
                // Get attachments array
                if (!payload.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array)
                    return dto;

                foreach (var item in results.EnumerateArray())
                {
                    string mediaType = GetStringProperty(item, "mediaType") ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(mediaType) ||
                        !(mediaType.StartsWith("image/") || mediaType.StartsWith("text/")))
                        continue; // ignore unsupported attachment types

                    string downloadLink = GetStringProperty(item, "_links", "download") ?? string.Empty;

                    if (string.IsNullOrWhiteSpace(downloadLink))
                        continue;
                    Console.WriteLine(baseUrl);

                    var fullUrl = $"{baseUrl.TrimEnd('/')}/{downloadLink.TrimStart('/')}";
                    Console.WriteLine(fullUrl);


                    dto.Attachments.Add(new Attachment
                    {
                        DownloadLink = fullUrl,
                        MediaType = mediaType
                    });
                }

                return dto;
        }

        private static string? GetStringProperty(JsonElement element, string propertyName)
        {
            return element.TryGetProperty(propertyName, out var prop) ? prop.GetString() : string.Empty;
        }

        private static string? GetStringProperty(JsonElement element, string parentProperty, string childProperty)
        {
            if (element.TryGetProperty(parentProperty, out var parentProp))
                return GetStringProperty(parentProp, childProperty);
            return string.Empty;
        }

    }
    
}
