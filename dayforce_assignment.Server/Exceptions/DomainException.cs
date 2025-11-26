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
        public string JiraId { get; }
        public JiraIssueNotFoundException(string jiraId)
            : base($"Jira issue '{jiraId}' not found.") => JiraId = jiraId;
    }

    public class JiraUnauthorizedException : JiraException
    {
        public JiraUnauthorizedException() : base("Unauthorized access to Jira API.") { }
    }

    public class JiraForbiddenException : JiraException
    {
        public JiraForbiddenException() : base("Access forbidden to Jira API.") { }
    }

    public class JiraBadRequestException : JiraException
    {
        public JiraBadRequestException(string message) : base(message) { }
    }

    public class JiraPayloadTooLargeException : JiraException
    {
        public JiraPayloadTooLargeException() : base("Request payload is too large.") { }
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

    public class JiraIssueParsingException : JiraException
    {
        public JiraIssueParsingException(string jiraId, string details)
            : base($"Failed to parse Jira issue '{jiraId}': {details}") { }
    }
    public class JiraTriageSubtaskNotFoundException : JiraException
    {
        public JiraTriageSubtaskNotFoundException(string parentKey)
            : base($"No triage subtask found for Jira issue '{parentKey}'.") { }
    }

    public class JiraTriageSubtaskProcessingException : JiraException
    {
        public JiraTriageSubtaskProcessingException(string parentKey, string details)
            : base($"Failed to process triage subtasks for Jira issue '{parentKey}': {details}") { }
    }

    public class TriageSubtaskCleaningException : JiraException
    {
        public TriageSubtaskCleaningException(string key, string details)
            : base($"Failed to clean triage subtask '{key}': {details}") { }
    }

    public class JiraCustomFieldLookupException : JiraException
    {
        public JiraCustomFieldLookupException(string details)
            : base($"Failed to look up Jira custom field: {details}") { }
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
        public ConfluenceBadRequestException(string message) : base(message) { }
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

    public class ConfluenceSearchException : ConfluenceException
    {
        public ConfluenceSearchException(string message) : base(message) { }
    }

    public class ConfluenceSearchBadRequestException : ConfluenceSearchException
    {
        public ConfluenceSearchBadRequestException(string message) : base(message) { }
    }

    public class ConfluencePageParsingException : ConfluenceException
    {
        public ConfluencePageParsingException(string pageId, string details)
            : base($"Failed to parse Confluence page '{pageId}': {details}") { }
    }

    public class ConfluenceAttachmentsParsingException : ConfluenceException
    {
        public ConfluenceAttachmentsParsingException(string pageId, string details)
            : base($"Failed to parse attachments for Confluence page '{pageId}': {details}") { }
    }

    public class ConfluencePageReferenceExtractionException : ConfluenceException
    {
        public ConfluencePageReferenceExtractionException(string jiraId, string details)
            : base($"Failed to extract Confluence page references for Jira issue '{jiraId}': {details}") { }
    }

    public class ConfluenceSearchParameterExtractionException : ConfluenceException
    {
        public ConfluenceSearchParameterExtractionException(string jiraId, string details)
            : base($"Failed to extract Confluence search parameters for Jira issue '{jiraId}': {details}") { }
    }

    public class ConfluenceSearchFilterExtractionException : ConfluenceException
    {
        public ConfluenceSearchFilterExtractionException(string jiraId, string details)
            : base($"Failed to filter Confluence search results for Jira issue '{jiraId}': {details}") { }
    }

    public class ConfluencePageSummaryException : ConfluenceException
    {
        public ConfluencePageSummaryException(string pageId, string details)
            : base($"Failed to generate summary for Confluence page '{pageId}': {details}") { }
    }



    // Common Services Exceptions
    public class JsonFormattingException : DomainException
    {
        public JsonFormattingException(string message) : base(message) { }
    }

    public class AttachmentDownloadException : ConfluenceException
    {
        public AttachmentDownloadException(string url, string details)
            : base($"Failed to download Confluence attachment '{url}': {details}") { }
    }

    public class UnsupportedAttachmentMediaTypeException : ConfluenceException
    {
        public UnsupportedAttachmentMediaTypeException(string mediaType)
            : base($"Unsupported attachment media type: '{mediaType}'") { }
    }



    // Orchestrator Services Exceptions
    public class TestCaseGenerationException : DomainException
    {
        public string JiraId { get; }
        public TestCaseGenerationException(string jiraId, string details)
            : base($"Failed to generate test cases for Jira issue '{jiraId}': {details}") => JiraId = jiraId;
    }

    public class UserMessageBuilderException : DomainException
    {
        private string message1;

        public string JiraId { get; }
        public UserMessageBuilderException(string jiraId, string details, Exception ex)
            : base($"Failed to build user message for Jira issue '{jiraId}': {details}") => JiraId = jiraId;

        public UserMessageBuilderException(string message, string message1) : base(message)
        {
            this.message1 = message1;
        }
    }

    public class ConfluencePageSearchOrchestratorException : DomainException
    {
        public string JiraId { get; }
        public ConfluencePageSearchOrchestratorException(string jiraId, string details)
            : base($"Failed to orchestrate Confluence page search for Jira issue '{jiraId}': {details}") => JiraId = jiraId;
    }



    // Atlassian Exceptions 
    public class AtlassianConfigurationException : DomainException
    {
        public AtlassianConfigurationException(string message) : base(message) { }
    }

}
