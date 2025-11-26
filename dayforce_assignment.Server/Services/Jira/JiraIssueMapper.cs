using dayforce_assignment.Server.DTOs.Common;
using dayforce_assignment.Server.DTOs.Jira;
    using dayforce_assignment.Server.Exceptions;
    using dayforce_assignment.Server.Interfaces.Jira;
    using System.Net.Http;
    using System.Text;
    using System.Text.Json;
    using System.Text.RegularExpressions;

    public class JiraIssueMapper : IJiraIssueMapper
    {   
        private readonly ICustomFieldService _customFieldService;
        private const string DocType = "doc";
        private const string Paragraph = "paragraph";
        private const string Heading = "heading";
        private const string ListItem = "listItem";
        private const string InlineCard = "inlineCard";

        public JiraIssueMapper(ICustomFieldService customFieldService)
        {
         _customFieldService = customFieldService;   
        }
        public JiraIssueDto MapToDto(JsonElement jiraJson, JsonElement jiraRemoteLinks)
        {
            try
            {
                // Jira key
                var jiraKey = jiraJson.TryGetProperty("key", out var keyProp) ? keyProp.GetString() : null;
                if (string.IsNullOrWhiteSpace(jiraKey))
                    throw new JiraIssueParsingException("unknown", "Missing Jira key");

                var dto = new JiraIssueDto { Key = jiraKey };


                if (jiraJson.TryGetProperty("fields", out var fields))
                {
                    // Jira type
                    if (fields.TryGetProperty("issuetype", out var issueType) &&
                        issueType.TryGetProperty("name", out var nameProp))
                    {
                        var name = nameProp.GetString();
                        dto.IssueType = name switch
                        {
                            "Story" => IssueType.Story,
                            "Bug" => IssueType.Bug,
                            _ => IssueType.Unknown
                        };
                    }

                    // Jira title
                    dto.Title = fields.TryGetProperty("summary", out var summary) ? summary.GetString() ?? string.Empty : string.Empty;
                }

                if (string.IsNullOrWhiteSpace(dto.Title))
                    throw new JiraIssueParsingException(jiraKey, "Missing summary/title");

                // Parent Issue
                if (fields.TryGetProperty("parent", out var parent))
                    dto.ParentKey = parent.TryGetProperty("key", out var parentKey) ? parentKey.GetString() ?? string.Empty : string.Empty;

                // Project info
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
                    dto.Subtasks = new List<SubtaskInfo>();
                    foreach (var st in subtasks.EnumerateArray())
                    {
                        var subKey = st.TryGetProperty("key", out var k) ? k.GetString() ?? string.Empty : string.Empty;
                        var summaryText =
                            st.TryGetProperty("fields", out var f) && f.TryGetProperty("summary", out var s) ? s.GetString() ?? string.Empty : string.Empty;
                        dto.Subtasks.Add(new SubtaskInfo { Key = subKey, Summary = summaryText });
                    }
                }


                // Acceptance Criteria
                var acceptanceCriteriaFieldId = _customFieldService.GetCustomFieldId(jiraJson, "Acceptance Criteria");

                if (!string.IsNullOrWhiteSpace(acceptanceCriteriaFieldId) &&
                    fields.TryGetProperty(acceptanceCriteriaFieldId, out var acField) &&
                    acField.ValueKind == JsonValueKind.Object &&
                    acField.TryGetProperty("type", out var acTypeProp) &&
                    acTypeProp.GetString() == DocType)
                {
                    var sbAcceptanceCriteria = new StringBuilder();
                    ExtractPlainTextFromDoc(acField, sbAcceptanceCriteria);
                    dto.AcceptanceCriteria = NormalizeText(sbAcceptanceCriteria.ToString());
                }




                // Description
                var sbDocContent = new StringBuilder();
                foreach (var prop in fields.EnumerateObject())
                {
                    // Skip the Acceptance Criteria field
                    if (prop.Name == acceptanceCriteriaFieldId)
                        continue;

                    if (prop.Value.ValueKind == JsonValueKind.Object &&
                        prop.Value.TryGetProperty("type", out var typePropDoc) &&
                        typePropDoc.GetString() == DocType)
                    {
                        ExtractPlainTextFromDoc(prop.Value, sbDocContent);
                        sbDocContent.AppendLine().AppendLine();
                    }
                }
                dto.Description = NormalizeText(sbDocContent.ToString());


                // Attachments
                if (fields.TryGetProperty("attachment", out var attachments) &&
                    attachments.ValueKind == JsonValueKind.Array)
                {
                    dto.Attachments = new List<Attachment>();
                    foreach (var attachment in attachments.EnumerateArray())
                    {
                        var content = attachment.TryGetProperty("content", out var c) ? c.GetString() ?? string.Empty : string.Empty;
                        var mimeType = attachment.TryGetProperty("mimeType", out var t) ? t.GetString() ?? string.Empty : string.Empty;

                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            dto.Attachments.Add(new Attachment
                            {
                                DownloadLink = content,
                                MediaType = mimeType
                            });
                        }
                    }
                }

                //Remote Links(Confluence)
                if (jiraRemoteLinks.ValueKind == JsonValueKind.Array)
                {
                    dto.RemoteLinks = new List<string>();
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
                            var url = urlProp.GetString() ?? null;
                            if (!string.IsNullOrWhiteSpace(url))
                                dto.RemoteLinks.Add(url);
                        }
                    }
                }


                return dto;
            }
            catch (JsonException ex)
            {
                throw new JiraIssueParsingException(
                    jiraJson.TryGetProperty("key", out var keyProp) ? keyProp.GetString() ?? "unknown" : "unknown",
                    $"JSON parsing error: {ex.Message}");
            }
            catch (Exception ex) when (!(ex is DomainException))
            {
                throw new JiraIssueParsingException(
                    jiraJson.TryGetProperty("key", out var keyProp) ? keyProp.GetString() ?? "unknown" : "unknown",
                    $"Unexpected error: {ex.Message}");
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
            input = Regex.Replace(input, @"(\n\s*){2,}", "\n\n"); // Reduce excessive blank lines
            return input.Trim();
        }
    }


