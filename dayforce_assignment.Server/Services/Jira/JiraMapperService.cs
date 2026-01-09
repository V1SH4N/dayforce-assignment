using dayforce_assignment.Server.DTOs.Common;
using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Exceptions;
using dayforce_assignment.Server.Interfaces.Jira;
using System.Text;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Jira
{
    public class JiraMapperService : IJiraMapperService
    {
        private readonly ICustomFieldService _customFieldService;

        public JiraMapperService(ICustomFieldService customFieldService)
        {
            _customFieldService = customFieldService;
        }




        // Maps json issue to JiraIssueDto
        public JiraIssueDto MapIssueToDto(JsonElement jsonIssue, JsonElement jsonRemoteLinks)
        {
            var dto = new JiraIssueDto();

            // Jira key
            string jiraKey = jsonIssue.TryGetProperty("key", out var jirakeyProp) ? jirakeyProp.GetString() ?? string.Empty : string.Empty;

            if (string.IsNullOrWhiteSpace(jiraKey))
                throw new JiraIssueMappingException("unknown", "Missing Jira key");
            else
                dto.Key = jiraKey;


            if (jsonIssue.TryGetProperty("fields", out var fieldsProp))
            {
                // Jira type
                if (fieldsProp.TryGetProperty("issuetype", out var issueTypeProp) &&
                    issueTypeProp.TryGetProperty("name", out var nameProp))
                {
                    string name = nameProp.GetString() ?? string.Empty;
                    dto.IssueType = name switch
                    {
                        "Story" => IssueType.Story,
                        "Bug" => IssueType.Bug,
                        _ => IssueType.Unknown
                    };
                }

                // Jira title
                string title = fieldsProp.TryGetProperty("summary", out var summaryProp) ? summaryProp.GetString() ?? string.Empty : string.Empty;
                if (string.IsNullOrWhiteSpace(title))
                    throw new JiraIssueMappingException(jiraKey, "Missing Jira title");
                else
                    dto.Title = title;
            }


            // Project info
            if (fieldsProp.TryGetProperty("project", out var projectProp))
            {
                string projectKey = projectProp.TryGetProperty("key", out var projKey) ? projKey.GetString() ?? string.Empty : string.Empty;
                string projectName = projectProp.TryGetProperty("name", out var projName) ? projName.GetString() ?? string.Empty : string.Empty;

                if (!string.IsNullOrWhiteSpace(projectKey))
                {
                    dto.Project = new ProjectInfo
                    {
                        Key = projectKey,
                        Name = projectName
                    };
                }
            }


            // Subtasks
            if (fieldsProp.TryGetProperty("subtasks", out var subtasksProp) && subtasksProp.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement st in subtasksProp.EnumerateArray())
                {
                    string subtaskKey = st.TryGetProperty("key", out var subtaskKeyProp) ? subtaskKeyProp.GetString() ?? string.Empty : string.Empty;
                    string summary = st.TryGetProperty("fields", out var f) && f.TryGetProperty("summary", out var summaryProp) ? summaryProp.GetString() ?? string.Empty : string.Empty;
                    if (!string.IsNullOrWhiteSpace(subtaskKey))
                    {
                        dto.Subtasks.Add(new IssueInfo
                        {
                            Key = subtaskKey,
                            Title = summary
                        });
                    }
                }
            }


            // Outward Issue Links
            if (fieldsProp.TryGetProperty("issuelinks", out JsonElement issueLinksProp))
            {
                foreach (JsonElement issueLink in issueLinksProp.EnumerateArray())
                {
                    if (issueLink.TryGetProperty("outwardIssue", out JsonElement outwardIssueProp) &&
                        outwardIssueProp.TryGetProperty("key", out var outwardIssuekeyProp)&&
                        outwardIssueProp.TryGetProperty("fields", out var outwardIssueFieldsProp) &&
                        outwardIssueFieldsProp.TryGetProperty("summary", out var outwardIssueSummaryProp))
                    {
                        string outwardIssueKey = outwardIssuekeyProp.GetString() ?? string.Empty;
                        string outwardIssueTitle = outwardIssueSummaryProp.GetString() ?? string.Empty;
                        
                        if (!string.IsNullOrEmpty(outwardIssueKey))
                        {
                            dto.OutwardIssueLinks.Add(new IssueInfo
                            {
                                Key = outwardIssueKey,
                                Title = outwardIssueTitle
                            });
                        }
                    }
                }
            }


            // Acceptance Criteria
            var acceptanceCriteriaFieldId = _customFieldService.GetCustomFieldId(jsonIssue, "Acceptance Criteria");

            if (!string.IsNullOrWhiteSpace(acceptanceCriteriaFieldId))
            {
                if (fieldsProp.TryGetProperty(acceptanceCriteriaFieldId, out var acFieldProp) &&
                acFieldProp.ValueKind == JsonValueKind.Object &&
                acFieldProp.TryGetProperty("type", out var acTypeProp) &&
                acTypeProp.GetString() == "doc")
                {
                    var sbAcceptanceCriteria = new StringBuilder();
                    ExtractTextFromDocType(acFieldProp, sbAcceptanceCriteria);
                    string acceptanceCriteria = sbAcceptanceCriteria.ToString();
                    if(!string.IsNullOrEmpty(acceptanceCriteria))
                        dto.AcceptanceCriteria = acceptanceCriteria;
                }
            }
                

            // Description
            var sbDescription = new StringBuilder();
            foreach (var prop in fieldsProp.EnumerateObject())
            {
                if (prop.Name == acceptanceCriteriaFieldId)
                    continue;

                if (prop.Value.ValueKind == JsonValueKind.Object &&
                    prop.Value.TryGetProperty("type", out var typePropDoc) &&
                    typePropDoc.GetString() == "doc")
                {
                    ExtractTextFromDocType(prop.Value, sbDescription);
                    sbDescription.AppendLine();
                }
            }
            string description = sbDescription.ToString();
            dto.Description = description;


            if (string.IsNullOrWhiteSpace(dto.AcceptanceCriteria) && string.IsNullOrWhiteSpace(dto.Description))
                throw new JiraIssueMappingException(jiraKey, "Missing description or acceptance criteria.");


            // Attachments
            if (fieldsProp.TryGetProperty("attachment", out var attachments) &&
                attachments.ValueKind == JsonValueKind.Array)
            {
                dto.Attachments = new List<Attachment>();
                foreach (var attachment in attachments.EnumerateArray())
                {
                    string content = attachment.TryGetProperty("content", out var c) ? c.GetString() ?? string.Empty : string.Empty;
                    string mimeType = attachment.TryGetProperty("mimeType", out var t) ? t.GetString() ?? string.Empty : string.Empty;
                    string fileName = attachment.TryGetProperty("filename", out var fn) ? fn.GetString() ?? string.Empty : string.Empty;

                    if (!string.IsNullOrWhiteSpace(content) && !string.IsNullOrEmpty(mimeType))
                    {
                        dto.Attachments.Add(new Attachment
                        {
                            DownloadLink = content,
                            MediaType = mimeType,
                            FileName = fileName
                        });
                    }
                }
            }


            // Remote Links (Confluence)
            if (jsonRemoteLinks.ValueKind == JsonValueKind.Array)
            {
                dto.RemoteLinks = new List<string>();
                foreach (var link in jsonRemoteLinks.EnumerateArray())
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




        // Maps json triage subtask to TriageSubtaskDto
        public TriageSubtaskDto MapTriageSubtaskToDto(JsonElement jsonTriageSubtask)
        {
            var dto = new TriageSubtaskDto();

            // Triage key
            string triageKey = jsonTriageSubtask.TryGetProperty("key", out var triagekeyProp) ? triagekeyProp.GetString() ?? string.Empty : string.Empty;

            if (string.IsNullOrWhiteSpace(triageKey))
                throw new TriageSubtaskMappingException("unknown", "Missing Jira key");
            else
                dto.Key = triageKey;



            if (jsonTriageSubtask.TryGetProperty("fields", out var fieldsProp))
            {
                // Triage title
                string title = fieldsProp.TryGetProperty("summary", out var summaryProp) ? summaryProp.GetString() ?? string.Empty : string.Empty;
                if (string.IsNullOrWhiteSpace(title))
                    throw new TriageSubtaskMappingException(triageKey, "Missing summary/title");
                else
                    dto.Title = title;
            }


            // Attachments
            if (fieldsProp.TryGetProperty("attachment", out var attachments) &&
                attachments.ValueKind == JsonValueKind.Array)
            {
                dto.Attachments = new List<Attachment>();
                foreach (var attachment in attachments.EnumerateArray())
                {
                    string content = attachment.TryGetProperty("content", out var c) ? c.GetString() ?? string.Empty : string.Empty;
                    string mimeType = attachment.TryGetProperty("mimeType", out var mt) ? mt.GetString() ?? string.Empty : string.Empty;
                    string fileName = attachment.TryGetProperty("filename", out var fn) ? fn.GetString() ?? string.Empty : string.Empty;

                    if (!string.IsNullOrWhiteSpace(content) && !string.IsNullOrEmpty(mimeType) && !string.IsNullOrEmpty(fileName))
                    {
                        dto.Attachments.Add(new Attachment
                        {
                            DownloadLink = content,
                            MediaType = mimeType,
                            FileName = fileName
                        });
                    }
                }
            }


            // Triage Comments
            var sbComments = new StringBuilder();

            if (fieldsProp.TryGetProperty("comment", out var commentSection) &&
                commentSection.TryGetProperty("comments", out var commentArray) &&
                commentArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var comment in commentArray.EnumerateArray())
                {
                    if (comment.TryGetProperty("body", out var body) &&
                        body.ValueKind == JsonValueKind.Object &&
                        body.TryGetProperty("type", out var bodyType) &&
                        bodyType.GetString() == "doc")
                    {
                        ExtractTextFromDocType(body, sbComments);
                        sbComments.AppendLine().AppendLine();
                    }
                }
            }

            string comments = sbComments.ToString();
            dto.Comments = comments;
            return dto;

        }




        // Extracts text content from json element (of type "doc")
        private static void ExtractTextFromDocType(JsonElement node, StringBuilder sb, int indentLevel = 0)
        {
            if (node.ValueKind != JsonValueKind.Object) return;

            if (node.TryGetProperty("text", out var textProp))
            {
                if (textProp.ValueKind == JsonValueKind.String)
                {
                    sb.Append(new string(' ', indentLevel * 2));
                    sb.Append(textProp.GetString());
                }   
            }

            if (node.TryGetProperty("type", out var typeProp))
            {
                string type = typeProp.GetString() ?? string.Empty;

                if (type == "inlineCard" &&
                    node.TryGetProperty("attrs", out var attrs) &&
                    attrs.TryGetProperty("url", out var urlProp))
                {
                    string url = urlProp.GetString() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(url))
                    {
                        sb.AppendLine();
                        sb.Append(url);
                        sb.AppendLine();
                    }
                }

                if (type == "paragraph" || type == "heading" || type == "listItem")
                {
                    sb.AppendLine(); 
                }
            }

            if (node.TryGetProperty("content", out var contentArray) && contentArray.ValueKind == JsonValueKind.Array)
            {
                foreach (var child in contentArray.EnumerateArray())
                {
                    ExtractTextFromDocType(child, sb, indentLevel + 1);
                }
            }
        }
        



    }
}
