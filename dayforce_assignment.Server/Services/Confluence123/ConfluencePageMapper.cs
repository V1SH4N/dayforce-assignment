//using dayforce_assignment.Server.DTOs.Confluence;
//using dayforce_assignment.Server.Exceptions;
//using dayforce_assignment.Server.Interfaces.Confluence;
//using HtmlAgilityPack;
//using System.Text;
//using System.Text.Json;
//using System.Web;

//namespace dayforce_assignment.Server.Services.Confluence
//{
//    public class ConfluencePageMapper : IConfluencePageMapper
//    {
//        public ConfluencePageDto MapToDto(JsonElement confluencePage, JsonElement confluenceComments)
//        {
//            try
//            {
//                var pageId = confluencePage.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "unknown" : "unknown";

//                // Extract page body
//                var pageBody = ExtractBodyValue(confluencePage);

//                // Extract comments body into a list
//                var commentsList = new List<string>();
//                if (confluenceComments.ValueKind != JsonValueKind.Undefined &&
//                    confluenceComments.ValueKind != JsonValueKind.Null &&
//                    confluenceComments.TryGetProperty("results", out var commentsResults))
//                {
//                    foreach (var comment in commentsResults.EnumerateArray())
//                    {
//                        var commentBody = ExtractBodyValue(comment);
//                        var cleaned = CleanHtml(commentBody);
//                        if (!string.IsNullOrWhiteSpace(cleaned))
//                            commentsList.Add(cleaned);
//                    }
//                }

//                return new ConfluencePageDto
//                {
//                    Id = pageId,
//                    Title = confluencePage.TryGetProperty("title", out var titleProp) ? titleProp.GetString() ?? string.Empty : string.Empty,
//                    Body = CleanHtml(pageBody),
//                    Comments = commentsList
//                };
//            }
//            catch (JsonException ex)
//            {
//                var pageId = confluencePage.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "unknown" : "unknown";
//                throw new ConfluencePageParsingException(pageId, $"JSON parsing error: {ex.Message}");
//            }
//            catch (Exception ex) when (!(ex is DomainException))
//            {
//                var pageId = confluencePage.TryGetProperty("id", out var idProp) ? idProp.GetString() ?? "unknown" : "unknown";
//                throw new ConfluencePageParsingException(pageId, $"Unexpected error: {ex.Message}");
//            }
//        }

//        private static string ExtractBodyValue(JsonElement root)
//        {
//            if (root.TryGetProperty("body", out var bodyProp) &&
//                bodyProp.TryGetProperty("storage", out var storageProp) &&
//                storageProp.TryGetProperty("value", out var valueProp))
//            {
//                return valueProp.GetString() ?? string.Empty;
//            }

//            return string.Empty;
//        }

//        private static string CleanHtml(string html)
//        {
//            var doc = new HtmlDocument();
//            doc.LoadHtml(html);

//            // Remove macros, script, style elements
//            RemoveNodes(doc.DocumentNode, "//*[local-name()='macro'] | //script | //style");

//            var sb = new StringBuilder();
//            var textNodes = doc.DocumentNode.SelectNodes("//text()");
//            if (textNodes != null)
//            {
//                foreach (var textNode in textNodes)
//                {
//                    var text = textNode.InnerText;
//                    if (!string.IsNullOrWhiteSpace(text))
//                        sb.AppendLine(text.Trim());
//                }
//            }

//            return HttpUtility.HtmlDecode(sb.ToString().Trim());
//        }

//        private static void RemoveNodes(HtmlNode root, string xPath)
//        {
//            var nodes = root.SelectNodes(xPath);
//            if (nodes == null) return;

//            foreach (var node in nodes)
//                node.Remove();
//        }
//    }
//}
