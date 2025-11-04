using dayforce_assignment.Server.Interfaces;
using dayforce_assignment.Server.DTOs;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace dayforce_assignment.Server.Services
{
    public class JiraPayloadCleanerService : IJiraPayloadCleanerService
    {
        private readonly ILogger<JiraPayloadCleanerService> _logger;

        public JiraPayloadCleanerService(ILogger<JiraPayloadCleanerService> logger)
        {
            _logger = logger;
        }

        public JiraStoryDto CleanJiraJson(JsonElement jiraJson)
        {
            var dto = new JiraStoryDto();

            try
            {
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
                        Key = project.GetProperty("key").GetString() ?? "",
                        Name = project.GetProperty("name").GetString() ?? ""
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
                            Key = st.GetProperty("key").GetString() ?? "",
                            Summary = st.GetProperty("fields").GetProperty("summary").GetString() ?? ""
                        });
                    }
                }

                // Extract all "doc" type fields and merge
                var docContents = new List<string>();
                foreach (var prop in fields.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Object &&
                        prop.Value.TryGetProperty("type", out var typeProp) &&
                        typeProp.GetString() == "doc")
                    {
                        var text = ExtractPlainTextFromDoc(prop.Value);
                        if (!string.IsNullOrWhiteSpace(text))
                            docContents.Add(text.Trim());
                    }
                }

                // Combine all doc fields with spacing
                var combinedDoc = string.Join("\n\n", docContents);

                // Normalize newlines & clean formatting
                combinedDoc = NormalizeText(combinedDoc);
                dto.DocContent = combinedDoc;

                // Attachments (content URLs)
                if (fields.TryGetProperty("attachment", out var attachments) &&
                    attachments.ValueKind == JsonValueKind.Array)
                {
                    foreach (var attachment in attachments.EnumerateArray())
                    {
                        if (attachment.TryGetProperty("content", out var content))
                            dto.Attachments.Add(content.GetString() ?? "");
                    }
                }

                _logger.LogInformation("Successfully cleaned Jira issue {Key}", dto.Key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning Jira JSON for {Key}", dto.Key);
                throw;
            }

            return dto;
        }


        // Extracts plain text from Atlassian Document Format (ADF)
        // Adds newlines where paragraph, heading, or list occurs
        private static string ExtractPlainTextFromDoc(JsonElement doc)
        {
            if (doc.ValueKind != JsonValueKind.Object)
                return string.Empty;

            var sb = new StringBuilder();

            if (doc.TryGetProperty("content", out var contentArray) &&
                contentArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var node in contentArray.EnumerateArray())
                {
                    if (node.TryGetProperty("text", out var textProp))
                        sb.Append(textProp.GetString());

                    if (node.TryGetProperty("type", out var typeProp))
                    {
                        var type = typeProp.GetString();

                        if (type == "inlineCard" &&
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
                        if (type is "paragraph" or "heading" or "listItem")
                            sb.AppendLine();
                    }

                    // Recursively process nested nodes
                    sb.Append(ExtractPlainTextFromDoc(node));
                }
            }

            return sb.ToString();
        }

        // Cleans up redundant whitespace, normalizes newlines to \n,
        // removes extra blank lines, and trims text
        private static string NormalizeText(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            // Normalize Windows/Mac newlines to \n
            input = input.Replace("\r\n", "\n").Replace("\r", "\n");

            // Remove excessive blank lines
            input = Regex.Replace(input, @"(\n\s*){2,}", "\n\n");

            // Trim start and end
            input = input.Trim();

            return input;
        }
    }
}
