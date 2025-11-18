using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.Exceptions.ApiExceptions;
using dayforce_assignment.Server.Interfaces.Confluence;
using HtmlAgilityPack;
using System.Text;
using System.Text.Json;
using System.Web;

namespace dayforce_assignment.Server.Services.Confluence
{
    public class ConfluencePageCleaner : IConfluencePageCleaner
    {
        private readonly ILogger<ConfluencePageCleaner> _logger;

        public ConfluencePageCleaner(ILogger<ConfluencePageCleaner> logger)
        {
            _logger = logger;
        }

        public ConfluencePageDto CleanConfluencePage(JsonElement confluencePage, JsonElement confluenceComments)
        {
            try
            {
                if (confluencePage.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
                    return new ConfluencePageDto();

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
                    Id = confluencePage.TryGetProperty("id", out var idProp) ? idProp.GetString() : null,
                    Title = confluencePage.TryGetProperty("title", out var titleProp) ? titleProp.GetString() : null,
                    BodyStorageValue = CleanHtml(pageBody),
                    Comments = commentsList
                };
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "JSON parsing error while cleaning Confluence page.");
                throw new ApiException(StatusCodes.Status400BadRequest, "Invalid JSON format", internalMessage: jsonEx.Message);
            }
            catch (HtmlWebException htmlEx)
            {
                _logger.LogError(htmlEx, "HTML parsing error while cleaning Confluence page.");
                throw new ApiException(StatusCodes.Status422UnprocessableEntity, "Error parsing HTML content", internalMessage: htmlEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while cleaning Confluence page.");
                throw new ApiException(StatusCodes.Status500InternalServerError, "An unexpected error occurred", internalMessage: ex.Message);
            }
        }

        private static string ExtractBodyValue(JsonElement root)
        {
            try
            {
                if (root.TryGetProperty("body", out var bodyProp) &&
                    bodyProp.TryGetProperty("storage", out var storageProp) &&
                    storageProp.TryGetProperty("value", out var valueProp))
                {
                    return valueProp.GetString() ?? string.Empty;
                }

                return string.Empty;
            }
            catch (Exception ex)
            {
                throw new HtmlWebException("Failed to extract body value from JSON.", ex);
            }
        }

        private static string CleanHtml(string html)
        {
            if (string.IsNullOrWhiteSpace(html))
                return string.Empty;

            try
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
            catch (Exception ex)
            {
                throw new HtmlWebException("Failed to clean HTML content.", ex);
            }
        }

        private static void RemoveNodes(HtmlNode root, string xPath)
        {
            var nodes = root.SelectNodes(xPath);
            if (nodes == null) return;

            foreach (var node in nodes)
                node.Remove();
        }
    }

    // Custom exception for HTML parsing errors
    public class HtmlWebException : Exception
    {
        public HtmlWebException(string message, Exception? innerException = null)
            : base(message, innerException) { }
    }
}
