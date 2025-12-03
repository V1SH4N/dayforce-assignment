//using dayforce_assignment.Server.DTOs.Common;
//using dayforce_assignment.Server.DTOs.Jira;
//using dayforce_assignment.Server.Exceptions;
//using dayforce_assignment.Server.Interfaces.Jira;
//using System.Text;
//using System.Text.Json;
//using System.Text.RegularExpressions;

//namespace dayforce_assignment.Server.Services.Jira
//{
//    public class TriageSubtaskMapper : ITriageSubtaskMapper
//    {
//        private const string DocType = "doc";
//        private const string Paragraph = "paragraph";
//        private const string Heading = "heading";
//        private const string ListItem = "listItem";
//        private const string InlineCard = "inlineCard";

//        public TriageSubtaskDto MapToDto(JsonElement triageSubtask)
//        {
//            string triageKey = triageSubtask.TryGetProperty("key", out var keyProp)
//                ? keyProp.GetString() ?? "unknown"
//                : "unknown";

//            try
//            {
//                if (triageSubtask.ValueKind == JsonValueKind.Undefined ||
//                    triageSubtask.ValueKind == JsonValueKind.Null)
//                {
//                    throw new TriageSubtaskCleaningException(triageKey, "Input JSON payload is empty.");
//                }

//                var dto = new TriageSubtaskDto { Key = triageKey };

//                // Fields
//                if (!triageSubtask.TryGetProperty("fields", out var fields))
//                    throw new TriageSubtaskCleaningException(triageKey, "Missing 'fields' section.");

//                // Title
//                dto.Title = fields.TryGetProperty("summary", out var summary)
//                    ? summary.GetString() ?? string.Empty
//                    : string.Empty;

//                // Attachments
//                dto.Attachments = new List<Attachment>();

//                if (fields.TryGetProperty("attachment", out var attachments) &&
//                    attachments.ValueKind == JsonValueKind.Array)
//                {
//                    foreach (var attachment in attachments.EnumerateArray())
//                    {
//                        var content = attachment.TryGetProperty("content", out var c)
//                            ? c.GetString()
//                            : null;

//                        if (string.IsNullOrWhiteSpace(content))
//                            continue;

//                        var mimeType = attachment.TryGetProperty("mimeType", out var m)
//                            ? m.GetString() ?? string.Empty
//                            : string.Empty;

//                        dto.Attachments.Add(new Attachment
//                        {
//                            DownloadLink = content,
//                            MediaType = mimeType
//                        });
//                    }
//                }

//                // Comments extraction
//                var sbComments = new StringBuilder();

//                if (fields.TryGetProperty("comment", out var commentSection) &&
//                    commentSection.TryGetProperty("comments", out var commentArray) &&
//                    commentArray.ValueKind == JsonValueKind.Array)
//                {
//                    foreach (var comment in commentArray.EnumerateArray())
//                    {
//                        if (comment.TryGetProperty("body", out var body) &&
//                            body.ValueKind == JsonValueKind.Object &&
//                            body.TryGetProperty("type", out var bodyType) &&
//                            bodyType.GetString() == DocType)
//                        {
//                            ExtractPlainTextFromDoc(body, sbComments);
//                            sbComments.AppendLine().AppendLine();
//                        }
//                    }
//                }

//                dto.Comments = NormalizeText(sbComments.ToString());

//                return dto;
//            }
//            catch (JsonException ex)
//            {
//                throw new TriageSubtaskCleaningException(triageKey, $"Invalid JSON structure: {ex.Message}");
//            }
//            catch (Exception ex) when (ex is not JiraException)
//            {
//                throw new TriageSubtaskCleaningException(triageKey, $"Unexpected error: {ex.Message}");
//            }
//        }

//        // Helpers
//        private static void ExtractPlainTextFromDoc(JsonElement node, StringBuilder sb)
//        {
//            if (node.ValueKind != JsonValueKind.Object)
//                return;

//            if (node.TryGetProperty("text", out var textProp))
//                sb.Append(textProp.GetString());

//            if (node.TryGetProperty("type", out var typeProp))
//            {
//                var type = typeProp.GetString();

//                if (type == InlineCard &&
//                    node.TryGetProperty("attrs", out var attrs) &&
//                    attrs.TryGetProperty("url", out var urlProp))
//                {
//                    var url = urlProp.GetString();
//                    if (!string.IsNullOrWhiteSpace(url))
//                    {
//                        sb.AppendLine();
//                        sb.Append(url);
//                        sb.AppendLine();
//                    }
//                }

//                if (type is Paragraph or Heading or ListItem)
//                    sb.AppendLine();
//            }

//            if (node.TryGetProperty("content", out var contentArray) &&
//                contentArray.ValueKind == JsonValueKind.Array)
//            {
//                foreach (var child in contentArray.EnumerateArray())
//                    ExtractPlainTextFromDoc(child, sb);
//            }
//        }

//        private static string NormalizeText(string input)
//        {
//            if (string.IsNullOrWhiteSpace(input))
//                return string.Empty;

//            input = input.Replace("\r\n", "\n").Replace("\r", "\n");
//            input = Regex.Replace(input, @"(\n\s*){2,}", "\n\n");
//            return input.Trim();
//        }
//    }
//}
