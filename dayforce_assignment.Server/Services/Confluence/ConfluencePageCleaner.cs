using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.Interfaces.Confluence;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Web;

namespace dayforce_assignment.Server.Services.Confluence
{
    public class ConfluencePageCleaner : IConfluencePageCleaner
    {
        public ConfluencePageDto CleanConfluencePage(JsonElement payload)
        {
            if (payload.ValueKind == JsonValueKind.Undefined || payload.ValueKind == JsonValueKind.Null)
                return new ConfluencePageDto();

            try
            {
                var dto = new ConfluencePageDto
                {
                    Id = payload.TryGetProperty("id", out var idProp) ? idProp.GetString() : null,
                    Title = payload.TryGetProperty("title", out var titleProp) ? titleProp.GetString() : null,
                    BodyStorageValue = ExtractBodyValue(payload)
                };

                // Clean HTML content if found
                dto.BodyStorageValue = CleanHtml(dto.BodyStorageValue ?? string.Empty);

                return dto;
            }
            catch
            {
                return new ConfluencePageDto();
            }
        }

        private string? ExtractBodyValue(JsonElement root)
        {
            try
            {
                if (root.TryGetProperty("body", out var bodyProp) &&
                    bodyProp.TryGetProperty("storage", out var storageProp) &&
                    storageProp.TryGetProperty("value", out var valueProp))
                {
                    return valueProp.GetString();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Removes Confluence macros, HTML tags, and decodes entities.
        /// </summary>
        private string CleanHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            // Remove Confluence macros and embedded elements
            html = Regex.Replace(html, @"<ac:.*?>.*?</ac:.*?>", string.Empty, RegexOptions.Singleline);

            // Remove HTML tags
            html = Regex.Replace(html, "<.*?>", string.Empty);

            // Decode HTML entities (e.g., &amp; → &)
            html = HttpUtility.HtmlDecode(html);

            // Trim excess whitespace
            html = html.Trim();

            return html;
        }
    }
}
