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
        public ConfluencePageDto CleanConfluencePage(JsonElement confluencePage, JsonElement confluenceComments)
        {
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
                Id = confluencePage.TryGetProperty("id", out var idProp) ? idProp.GetString() : string.Empty,
                Title = confluencePage.TryGetProperty("title", out var titleProp) ? titleProp.GetString() : null,
                BodyStorageValue = CleanHtml(pageBody),
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
    }
}
