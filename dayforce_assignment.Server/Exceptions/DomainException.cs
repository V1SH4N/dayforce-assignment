namespace dayforce_assignment.Server.Exceptions
{
    // Base domain exception
    public abstract class DomainException : Exception
    {
        protected DomainException(string message) : base(message) { }
    }



    // Jira Exceptions
    public class JiraException : DomainException
    {
        public JiraException(string message) : base(message) { }
    }

    public class JiraIssueNotFoundException : JiraException
    {
        public string JiraKey { get; }
        public JiraIssueNotFoundException(string jiraKey)
            : base($"Jira issue '{jiraKey}' not found.") => JiraKey = jiraKey;
    }

    public class JiraUnauthorizedException : JiraException
    {
        public JiraUnauthorizedException()
            : base("Unauthorized access to Jira API.") { }
    }

    public class JiraRemoteLinksForbiddenException : JiraException
    {
        public string JiraKey { get; }

        public JiraRemoteLinksForbiddenException(string jiraKey) : base("Issue linking is disabled for Jira issue '{jiraKey}'.") => JiraKey = jiraKey;
    }

    public class JiraBadRequestException : JiraException
    {
        public string JiraKey { get; }
        public JiraBadRequestException(string jiraKey) : base($"Invalid request to Jira API for issue '{jiraKey}'") => JiraKey = jiraKey;
    }

    public class JiraRemoteLinksPayloadTooLargeException : JiraException
    {
        public string JiraKey { get; }
        public JiraRemoteLinksPayloadTooLargeException(string jiraKey) : base("Per-issue remote link limit reached for Jira issue '{jiraKey}'.") => JiraKey = jiraKey;
    }

    public class JiraRemoteLinksNotFoundException : JiraException
    {
        public string JiraId { get; }
        public JiraRemoteLinksNotFoundException(string jiraId)
            : base($"No remote links found for Jira issue '{jiraId}'.") => JiraId = jiraId;
    }

    public class JiraApiException : JiraException
    {
        public int StatusCode { get; }
        public JiraApiException(int statusCode, string message)
            : base(message) => StatusCode = statusCode;
    }

    public class JiraIssueMappingException : JiraException
    {
        public JiraIssueMappingException(string jiraId, string details)
            : base($"Failed to map Jira issue '{jiraId} json': {details}") { }
    }

    public class TriageSubtaskMappingException : JiraException
    {
        public TriageSubtaskMappingException(string key, string details)
            : base($"Failed to map triage subtask '{key} json': {details}") { }
    }



    // Confluence Exceptions
    public class ConfluenceException : DomainException
    {
        public ConfluenceException(string message) : base(message) { }
    }

    public class ConfluencePageNotFoundException : ConfluenceException
    {
        public string PageId { get; }
        public ConfluencePageNotFoundException(string pageId)
            : base($"Confluence page '{pageId}' not found.") => PageId = pageId;
    }

    public class ConfluenceUnauthorizedException : ConfluenceException
    {
        public ConfluenceUnauthorizedException() : base("Unauthorized access to Confluence API.") { }
    }

    public class ConfluenceBadRequestException : ConfluenceException
    {
        public string PageId { get; }
        public ConfluenceBadRequestException(string pageId) : base($"Invalid request to Confluence API for page id '{pageId}'") => PageId = pageId;
    }

    public class ConfluenceApiException : ConfluenceException
    {
        public int StatusCode { get; }
        public ConfluenceApiException(int statusCode, string message)
            : base(message) => StatusCode = statusCode;
    }

    public class ConfluenceCommentsNotFoundException : ConfluenceException
    {
        public string PageId { get; }
        public ConfluenceCommentsNotFoundException(string pageId)
            : base($"No comments found for Confluence page '{pageId}'.") => PageId = pageId;
    }

    public class ConfluenceAttachmentsNotFoundException : ConfluenceException
    {
        public string PageId { get; }
        public ConfluenceAttachmentsNotFoundException(string pageId)
            : base($"No attachments found for Confluence page '{pageId}'.") => PageId = pageId;
    }

    public class ConfluenceSearchBadRequestException : ConfluenceException
    {
        public ConfluenceSearchBadRequestException() : base("Invalid request to Confluence API") { }
    }

    public class ConfluencePageReferenceExtractionException : ConfluenceException
    {
        public ConfluencePageReferenceExtractionException(string message): base(message) { }
    }

    public class ConfluenceSearchParameterException : ConfluenceException
    {
        public ConfluenceSearchParameterException(string message) : base(message) { }
    }

    public class ConfluenceSearchFilterException : ConfluenceException
    {
        public ConfluenceSearchFilterException(string message) : base(message) { }
    }

    public class ConfluencePageSummaryException : ConfluenceException
    {
        public string PageId { get; }
        public ConfluencePageSummaryException(string pageId, string details)
            : base($"Failed to generate summary for Confluence page '{pageId}': {details}") => PageId = pageId;
    }



    // Common Services Exceptions
    public class JsonFormattingException : DomainException
    {
        public JsonFormattingException(string message) : base(message) { }
    }

    public class AttachmentDownloadException : ConfluenceException
    {
        public AttachmentDownloadException(string url)
            : base($"Failed to download Confluence attachment '{url}'") { }
    }

    public class UnsupportedAttachmentMediaTypeException : ConfluenceException
    {
        public UnsupportedAttachmentMediaTypeException(string mediaType)
            : base($"Unsupported attachment media type: '{mediaType}'") { }
    }

    public class AttachmentSummaryException : ConfluenceException
    {
        public AttachmentSummaryException()
            : base($"Failed to summarize Image attachment.") { }
    }



    // Orchestrator Services Exceptions
    public class TestCaseGenerationException : DomainException
    {
        public string JiraId { get; }
        public TestCaseGenerationException(string jiraId, string details)
            : base($"Failed to generate test cases for Jira issue '{jiraId}': {details}") => JiraId = jiraId;
    }



    // Atlassian Exceptions 
    public class AtlassianConfigurationException : DomainException
    {
        public AtlassianConfigurationException(string message) : base(message) { }
    }

}
