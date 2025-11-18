using dayforce_assignment.Server.Interfaces.Jira;
using dayforce_assignment.Server.DTOs.Jira;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using dayforce_assignment.Server.Exceptions.ApiExceptions;

public class JiraStoryCleaner : IJiraStoryCleaner
{
    private const string DocType = "doc";
    private const string Paragraph = "paragraph";
    private const string Heading = "heading";
    private const string ListItem = "listItem";
    private const string InlineCard = "inlineCard";

    public JiraStoryDto CleanJiraStory(JsonElement jiraJson, JsonElement jiraRemoteLinks)
    {
        try
        {
            var dto = new JiraStoryDto();

            if (!jiraJson.TryGetProperty("fields", out var fields))
                throw new ApiException(
                    StatusCodes.Status500InternalServerError,
                    "Jira story data is invalid",
                    internalMessage: "Missing 'fields' property in Jira JSON.");

            dto.Key = jiraJson.TryGetProperty("key", out var keyProp)
                ? keyProp.GetString() ?? string.Empty
                : throw new ApiException(
                    StatusCodes.Status500InternalServerError,
                    "Jira story data is invalid",
                    internalMessage: "Missing 'key' property in Jira JSON.");

            // Parent
            if (fields.TryGetProperty("parent", out var parent))
                dto.ParentKey = parent.TryGetProperty("key", out var parentKey) ? parentKey.GetString() : null;

            // Project
            if (fields.TryGetProperty("project", out var project))
            {
                dto.Project = new ProjectInfo
                {
                    Key = project.TryGetProperty("key", out var projKey) ? projKey.GetString() ?? string.Empty : string.Empty,
                    Name = project.TryGetProperty("name", out var projName) ? projName.GetString() ?? string.Empty : string.Empty
                };
            }

            // Subtasks
            if (fields.TryGetProperty("subtasks", out var subtasks) && subtasks.ValueKind == JsonValueKind.Array)
            {
                foreach (var st in subtasks.EnumerateArray())
                {
                    var subKey = st.TryGetProperty("key", out var k) ? k.GetString() ?? string.Empty : string.Empty;
                    var summary = st.TryGetProperty("fields", out var f) &&
                                  f.TryGetProperty("summary", out var s) ? s.GetString() ?? string.Empty : string.Empty;

                    dto.Subtasks.Add(new SubtaskInfo { Key = subKey, Summary = summary });
                }
            }

            // doc fields
            var sb = new StringBuilder();
            foreach (var prop in fields.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.Object &&
                    prop.Value.TryGetProperty("type", out var typeProp) &&
                    typeProp.GetString() == DocType)
                {
                    ExtractPlainTextFromDoc(prop.Value, sb);
                    sb.AppendLine().AppendLine();
                }
            }
            dto.DocContent = NormalizeText(sb.ToString());

            // Attachments
            if (fields.TryGetProperty("attachment", out var attachments) && attachments.ValueKind == JsonValueKind.Array)
            {
                foreach (var attachment in attachments.EnumerateArray())
                {
                    var content = attachment.TryGetProperty("content", out var c) ? c.GetString() ?? string.Empty : string.Empty;
                    var type = attachment.TryGetProperty("mimeType", out var t) ? t.GetString() ?? string.Empty : string.Empty;

                    if (!string.IsNullOrWhiteSpace(content))
                    {
                        dto.Attachments.Add(new Attachment
                        {
                            DownloadLink = content,
                            MediaType = type
                        });
                    }
                }
            }

            // RemoteLinks
            if (jiraRemoteLinks.ValueKind == JsonValueKind.Array)
            {
                foreach (var link in jiraRemoteLinks.EnumerateArray())
                {
                    if (link.TryGetProperty("application", out var app) &&
                        app.TryGetProperty("type", out var typeProp) &&
                        typeProp.GetString() == "com.atlassian.confluence" &&
                        link.TryGetProperty("object", out var obj) &&
                        link.TryGetProperty("relationship", out var rel) &&
                        rel.GetString() == "Wiki Page" &&
                        obj.TryGetProperty("url", out var urlProp))
                    {
                        var url = urlProp.GetString() ?? string.Empty;
                        if (!string.IsNullOrWhiteSpace(url))
                            dto.RemoteLinks.Add(url);
                    }
                }
            }

            return dto;
        }
        catch (Exception ex) when (!(ex is ApiException))
        {
            throw new ApiException(
                StatusCodes.Status500InternalServerError,
                "An error occurred while cleaning Jira story data.",
                internalMessage: ex.ToString());
        }
    }

    private static void ExtractPlainTextFromDoc(JsonElement node, StringBuilder sb)
    {
        if (node.ValueKind != JsonValueKind.Object) return;

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

            if (type is Paragraph or Heading or ListItem)
                sb.AppendLine();
        }

        if (node.TryGetProperty("content", out var contentArray) && contentArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var child in contentArray.EnumerateArray())
                ExtractPlainTextFromDoc(child, sb);
        }
    }

    private static string NormalizeText(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        input = input.Replace("\r\n", "\n").Replace("\r", "\n");
        input = Regex.Replace(input, @"(\n\s*){2,}", "\n\n");
        return input.Trim();
    }
}
