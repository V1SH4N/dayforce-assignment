using System.Net;

namespace dayforce_assignment.Server.Exceptions
{


    // Base domain exception
    public abstract class DomainException : Exception
    {
        public virtual HttpStatusCode StatusCode { get; }

        protected DomainException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
            : base(message)
        {
            StatusCode = statusCode;
        }
    }


    // Jira Exceptions
    public class JiraException : DomainException
    {
        protected JiraException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
            : base(message, statusCode) { }
    }

    public class JiraUnauthorizedException : JiraException
    {
        public JiraUnauthorizedException()
            : base("Unauthorized access to Jira API.", HttpStatusCode.Unauthorized) { }
    }

    public class JiraIssueNotFoundException : JiraException
    {
        public string JiraKey { get; }

        public JiraIssueNotFoundException(string jiraKey)
            : base($"Jira issue '{jiraKey}' not found.", HttpStatusCode.NotFound)
        {
            JiraKey = jiraKey;
        }
    }

    public class JiraRemoteLinksForbiddenException : JiraException
    {
        public string JiraKey { get; }

        public JiraRemoteLinksForbiddenException(string jiraKey)
            : base($"Issue linking is disabled for Jira issue '{jiraKey}'.", HttpStatusCode.Forbidden)
        {
            JiraKey = jiraKey;
        }
    }

    public class JiraBadRequestException : JiraException
    {
        public string JiraKey { get; }

        public JiraBadRequestException(string jiraKey)
            : base($"Invalid request to Jira API for issue '{jiraKey}'.", HttpStatusCode.BadRequest)
        {
            JiraKey = jiraKey;
        }
    }

    public class JiraRemoteLinksPayloadTooLargeException : JiraException
    {
        public string JiraKey { get; }

        public JiraRemoteLinksPayloadTooLargeException(string jiraKey)
            : base($"Per-issue remote link limit reached for Jira issue '{jiraKey}'.", HttpStatusCode.RequestEntityTooLarge)
        {
            JiraKey = jiraKey;
        }
    }

    public class JiraRemoteLinksNotFoundException : JiraException
    {
        public string JiraId { get; }

        public JiraRemoteLinksNotFoundException(string jiraId)
            : base($"No remote links found for Jira issue '{jiraId}'.", HttpStatusCode.NotFound)
        {
            JiraId = jiraId;
        }
    }

    public class JiraApiException : JiraException
    {
        public JiraApiException(string message)
            : base(message, HttpStatusCode.BadGateway) { }
    }

    public class JiraIssueMappingException : JiraException
    {
        public JiraIssueMappingException(string jiraId, string details)
            : base($"Failed to map Jira issue '{jiraId}' JSON: {details}", HttpStatusCode.InternalServerError) { }
    }

    public class TriageSubtaskMappingException : JiraException
    {
        public TriageSubtaskMappingException(string key, string details)
            : base($"Failed to map triage subtask '{key}' JSON: {details}", HttpStatusCode.InternalServerError) { }
    }



    // Confluence Exceptions
    public abstract class ConfluenceException : DomainException
    {
        protected ConfluenceException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError)
            : base(message, statusCode)
        {
        }
    }

    public class ConfluencePageNotFoundException : ConfluenceException
    {
        public string PageId { get; }

        public ConfluencePageNotFoundException(string pageId)
            : base($"Confluence page '{pageId}' not found.", HttpStatusCode.NotFound)
        {
            PageId = pageId;
        }
    }

    public class ConfluenceUnauthorizedException : ConfluenceException
    {
        public ConfluenceUnauthorizedException()
            : base("Unauthorized access to Confluence API.", HttpStatusCode.Unauthorized)
        {
        }
    }

    public class ConfluenceBadRequestException : ConfluenceException
    {
        public string PageId { get; }

        public ConfluenceBadRequestException(string pageId)
            : base($"Invalid request to Confluence API for page id '{pageId}'.", HttpStatusCode.BadRequest)
        {
            PageId = pageId;
        }
    }

    public class ConfluenceApiException : ConfluenceException
    {
        public int ApiStatusCode { get; }

        public ConfluenceApiException(string message)
            : base(message, HttpStatusCode.BadGateway) { }
    }

    public class ConfluenceCommentsNotFoundException : ConfluenceException
    {
        public string PageId { get; }

        public ConfluenceCommentsNotFoundException(string pageId)
            : base($"No comments found for Confluence page '{pageId}'.", HttpStatusCode.NotFound)
        {
            PageId = pageId;
        }
    }

    public class ConfluenceAttachmentsNotFoundException : ConfluenceException
    {
        public string PageId { get; }

        public ConfluenceAttachmentsNotFoundException(string pageId)
            : base($"No attachments found for Confluence page '{pageId}'.", HttpStatusCode.NotFound)
        {
            PageId = pageId;
        }
    }

    public class ConfluenceSearchBadRequestException : ConfluenceException
    {
        public ConfluenceSearchBadRequestException()
            : base("Invalid request to Confluence API.", HttpStatusCode.BadRequest)
        {
        }
    }

    public class ConfluenceSearchParameterException : ConfluenceException
    {
        public ConfluenceSearchParameterException(string message)
            : base(message, HttpStatusCode.InternalServerError)
        {
        }
    }

    public class ConfluenceSearchFilterException : ConfluenceException
    {
        public ConfluenceSearchFilterException(string message)
            : base(message, HttpStatusCode.InternalServerError)
        {
        }
    }

    public class ConfluencePageSummaryException : ConfluenceException
    {
        public string PageId { get; }

        public ConfluencePageSummaryException(string pageId, string details)
            : base($"Failed to generate summary for Confluence page '{pageId}': {details}", HttpStatusCode.InternalServerError)
        {
            PageId = pageId;
        }
    }



    // Common Services Exceptions

    // JSON formatting exception
    public class JsonFormattingException : DomainException
    {
        public JsonFormattingException(string message)
            : base(message, HttpStatusCode.InternalServerError) 
        {
        }
    }

    // Attachment exceptions

    public class AttachmentDownloadException : ConfluenceException
    {
        public string FileName { get; }

        public AttachmentDownloadException(string fileName)
            : base($"Failed to download attachment: '{fileName}'", HttpStatusCode.BadGateway)
        {
            FileName = fileName;
        }
    }

    public class UnsupportedAttachmentMediaTypeException : ConfluenceException
    {
        public string FileName { get; }

        public UnsupportedAttachmentMediaTypeException(string fileName)
            : base($"Unsupported attachment media type for attachment: {fileName}.", HttpStatusCode.UnsupportedMediaType)
        {
            FileName = fileName;
        }
    }

    public class AttachmentSummaryException : ConfluenceException
    {
        public string FileName { get; }

        public AttachmentSummaryException(string fileName)
            : base($"Failed to summarize attachment: {fileName}.", HttpStatusCode.InternalServerError)
        {
            FileName = fileName;
        }
    }



    // Orchestrator Services Exceptions

    // Test case generation exception
    public class TestCaseGenerationException : DomainException
    {
        public string JiraId { get; }

        public TestCaseGenerationException(string jiraId, string details)
            : base($"Failed to generate test cases for Jira issue '{jiraId}': {details}", HttpStatusCode.InternalServerError)
        {
            JiraId = jiraId;
        }
    }

    // Atlassian configuration exception
    public class AtlassianConfigurationException : DomainException
    {
        public AtlassianConfigurationException(string message)
            : base(message, HttpStatusCode.InternalServerError) { }
    }
}
