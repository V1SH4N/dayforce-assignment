using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.Interfaces.Confluence;
using HtmlAgilityPack;
using System.Text;
using System.Text.Json;
using System.Web;

namespace dayforce_assignment.Server.Services.Confluence
{
    public class ConfluencePageCleaner : IConfluencePageCleaner
    {
        public ConfluencePageDto CleanConfluencePage(JsonElement payload)
        {
            if (payload.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
                return new ConfluencePageDto();

            var dto = new ConfluencePageDto
            {
                Id = payload.TryGetProperty("id", out var idProp) ? idProp.GetString() : null,
                Title = payload.TryGetProperty("title", out var titleProp) ? titleProp.GetString() : null,
                BodyStorageValue = CleanHtml(ExtractBodyValue(payload))
            };

            return dto;
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

        /// <summary>
        /// Cleans HTML using HtmlAgilityPack: removes Confluence macros, scripts, and style elements, then extracts visible text.
        /// </summary>
        private static string CleanHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            // Remove all Confluence macros (ac:macro) and other unwanted elements
            RemoveNodes(doc.DocumentNode, "//*[local-name()='macro'] | //script | //style");

            // Extract all visible text
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

            // Decode HTML entities and trim
            return HttpUtility.HtmlDecode(sb.ToString().Trim());
        }

        /// <summary>
        /// Helper to remove nodes matching XPath
        /// </summary>
        private static void RemoveNodes(HtmlNode root, string xPath)
        {
            var nodes = root.SelectNodes(xPath);
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    node.Remove();
                }
            }
        }
    }
}




