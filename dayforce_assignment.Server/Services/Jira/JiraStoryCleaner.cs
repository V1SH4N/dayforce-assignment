//using System.Text;
//using System.Text.Json;
//using System.Text.RegularExpressions;
//using dayforce_assignment.Server.Interfaces.Jira;
//using dayforce_assignment.Server.DTOs.Jira;

//namespace dayforce_assignment.Server.Services.Jira
//{
//    public class JiraStoryCleaner : IJiraStoryCleaner
//    {
//        private readonly ILogger<JiraStoryCleaner> _logger;

//        public JiraStoryCleaner(ILogger<JiraStoryCleaner> logger)
//        {
//            _logger = logger;
//        }

//        public JiraStoryDto CleanJiraStory(JsonElement jiraJson)
//        {
//            var dto = new JiraStoryDto();

//            try
//            {
//                var fields = jiraJson.GetProperty("fields");
//                dto.Key = jiraJson.GetProperty("key").GetString() ?? string.Empty;


//                // Parent
//                if (fields.TryGetProperty("parent", out var parent))
//                    dto.ParentKey = parent.GetProperty("key").GetString();


//                // Project
//                if (fields.TryGetProperty("project", out var project))
//                {
//                    dto.Project = new ProjectInfo
//                    {
//                        Key = project.GetProperty("key").GetString() ?? "",
//                        Name = project.GetProperty("name").GetString() ?? ""
//                    };
//                }


//                // Subtasks
//                if (fields.TryGetProperty("subtasks", out var subtasks) &&
//                    subtasks.ValueKind == JsonValueKind.Array)
//                {
//                    foreach (var st in subtasks.EnumerateArray())
//                    {
//                        dto.Subtasks.Add(new SubtaskInfo
//                        {
//                            Key = st.GetProperty("key").GetString() ?? "",
//                            Summary = st.GetProperty("fields").GetProperty("summary").GetString() ?? ""
//                        });
//                    }
//                }

//                // Extract all "doc" type fields and merge
//                var docContents = new List<string>();
//                foreach (var prop in fields.EnumerateObject())
//                {
//                    if (prop.Value.ValueKind == JsonValueKind.Object &&
//                        prop.Value.TryGetProperty("type", out var typeProp) &&
//                        typeProp.GetString() == "doc")
//                    {
//                        var text = ExtractPlainTextFromDoc(prop.Value);
//                        if (!string.IsNullOrWhiteSpace(text))
//                            docContents.Add(text.Trim());
//                    }
//                }

//                // Combine all doc fields with spacing
//                var combinedDoc = string.Join("\n\n", docContents);

//                // Normalize newlines & clean formatting
//                combinedDoc = NormalizeText(combinedDoc);
//                dto.DocContent = combinedDoc;

//                // Attachments (content URLs)
//                if (fields.TryGetProperty("attachment", out var attachments) &&
//                    attachments.ValueKind == JsonValueKind.Array)
//                {
//                    foreach (var attachment in attachments.EnumerateArray())
//                    {
//                        if (attachment.TryGetProperty("content", out var content))
//                            dto.Attachments.Add(content.GetString() ?? "");
//                    }
//                }

//                _logger.LogInformation("Successfully cleaned Jira issue {Key}", dto.Key);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error cleaning Jira JSON for {Key}", dto.Key);
//                throw;
//            }

//            return dto;
//        }


//        // Extracts plain text from Atlassian Document Format (ADF)
//        // Adds newlines where paragraph, heading, or list occurs
//        private static string ExtractPlainTextFromDoc(JsonElement doc)
//        {
//            if (doc.ValueKind != JsonValueKind.Object)
//                return string.Empty;

//            var sb = new StringBuilder();

//            if (doc.TryGetProperty("content", out var contentArray) &&
//                contentArray.ValueKind == JsonValueKind.Array)
//            {
//                foreach (var node in contentArray.EnumerateArray())
//                {
//                    if (node.TryGetProperty("text", out var textProp))
//                        sb.Append(textProp.GetString());

//                    if (node.TryGetProperty("type", out var typeProp))
//                    {
//                        var type = typeProp.GetString();

//                        if (type == "inlineCard" &&
//                            node.TryGetProperty("attrs", out var attrs) &&
//                            attrs.TryGetProperty("url", out var urlProp))
//                        {
//                            var url = urlProp.GetString();
//                            if (!string.IsNullOrWhiteSpace(url))
//                            {
//                                sb.AppendLine();
//                                sb.Append(url);
//                                sb.AppendLine();
//                            }
//                        }

//                        // Add newline for structure types
//                        if (type is "paragraph" or "heading" or "listItem")
//                            sb.AppendLine();
//                    }

//                    // Recursively process nested nodes
//                    sb.Append(ExtractPlainTextFromDoc(node));
//                }
//            }

//            return sb.ToString();
//        }

//        // Cleans up redundant whitespace, normalizes newlines to \n,
//        // removes extra blank lines, and trims text
//        private static string NormalizeText(string input)
//        {
//            if (string.IsNullOrWhiteSpace(input))
//                return string.Empty;

//            // Normalize Windows/Mac newlines to \n
//            input = input.Replace("\r\n", "\n").Replace("\r", "\n");

//            // Remove excessive blank lines
//            input = Regex.Replace(input, @"(\n\s*){2,}", "\n\n");

//            // Trim start and end
//            input = input.Trim();

//            return input;
//        }
//    }
//}

using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using dayforce_assignment.Server.Interfaces.Jira;
using dayforce_assignment.Server.DTOs.Jira;

namespace dayforce_assignment.Server.Services.Jira
{
    public class JiraStoryCleaner : IJiraStoryCleaner
    {
        // Constants for ADF types
        private const string DocType = "doc";
        private const string Paragraph = "paragraph";
        private const string Heading = "heading";
        private const string ListItem = "listItem";
        private const string InlineCard = "inlineCard";

        public JiraStoryDto CleanJiraStory(JsonElement jiraJson)
        {
            var dto = new JiraStoryDto();

            var fields = jiraJson.GetProperty("fields");
            dto.Key = jiraJson.GetProperty("key").GetString() ?? string.Empty;

            // Parent
            if (fields.TryGetProperty("parent", out var parent))
                dto.ParentKey = parent.GetProperty("key").GetString();

            // Project
            if (fields.TryGetProperty("project", out var project))
            {
                dto.Project = new ProjectInfo
                {
                    Key = project.GetProperty("key").GetString() ?? string.Empty,
                    Name = project.GetProperty("name").GetString() ?? string.Empty
                };
            }

            // Subtasks
            if (fields.TryGetProperty("subtasks", out var subtasks) &&
                subtasks.ValueKind == JsonValueKind.Array)
            {
                foreach (var st in subtasks.EnumerateArray())
                {
                    dto.Subtasks.Add(new SubtaskInfo
                    {
                        Key = st.GetProperty("key").GetString() ?? string.Empty,
                        Summary = st.GetProperty("fields").GetProperty("summary").GetString() ?? string.Empty
                    });
                }
            }

            // Extract all doc fields
            var sb = new StringBuilder();
            foreach (var prop in fields.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.Object &&
                    prop.Value.TryGetProperty("type", out var typeProp) &&
                    typeProp.GetString() == DocType)
                {
                    ExtractPlainTextFromDoc(prop.Value, sb);
                    sb.AppendLine().AppendLine(); // Separate doc fields
                }
            }

            dto.DocContent = NormalizeText(sb.ToString());

            // Attachments
            if (fields.TryGetProperty("attachment", out var attachments) &&
                attachments.ValueKind == JsonValueKind.Array)
            {
                foreach (var attachment in attachments.EnumerateArray())
                {
                    if (attachment.TryGetProperty("content", out var content))
                        dto.Attachments.Add(content.GetString() ?? string.Empty);
                }
            }

            return dto;
        }

        // Recursively extract text from ADF nodes using a single StringBuilder
        private static void ExtractPlainTextFromDoc(JsonElement node, StringBuilder sb)
        {
            if (node.ValueKind != JsonValueKind.Object)
                return;

            if (node.TryGetProperty("text", out var textProp))
                sb.Append(textProp.GetString());

            if (node.TryGetProperty("type", out var typeProp))
            {
                var type = typeProp.GetString();

                if (type == InlineCard &&
                    node.TryGetProperty("attrs", out var attrs) &&
                    attrs.TryGetProperty("url", out var urlProp))
                {
                    var url = urlProp.GetString();
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        sb.AppendLine();
                        sb.Append(url);
                        sb.AppendLine();
                    }
                }

                // Add newline for structure types
                if (type is Paragraph or Heading or ListItem)
                    sb.AppendLine();
            }

            // Process child nodes
            if (node.TryGetProperty("content", out var contentArray) &&
                contentArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var child in contentArray.EnumerateArray())
                    ExtractPlainTextFromDoc(child, sb);
            }
        }

        // Normalize whitespace, newlines, and remove extra blank lines
        private static string NormalizeText(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            input = input.Replace("\r\n", "\n").Replace("\r", "\n"); // normalize newlines
            input = Regex.Replace(input, @"(\n\s*){2,}", "\n\n");      // remove excess blank lines
            return input.Trim();
        }
    }
}
