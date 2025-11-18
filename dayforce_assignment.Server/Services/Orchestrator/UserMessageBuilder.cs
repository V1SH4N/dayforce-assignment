using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Interfaces.Common;
using dayforce_assignment.Server.Interfaces.Confluence;
using dayforce_assignment.Server.Interfaces.Jira;
using dayforce_assignment.Server.Interfaces.Orchestrator;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;




namespace dayforce_assignment.Server.Services.Orchestrator
{
    public class UserMessageBuilder : IUserMessageBuilder
    {
        private readonly IJiraStoryService _jiraStoryService;
        private readonly IJiraRemoteLinksService _jiraRemoteLinksService;
        private readonly IJiraStoryCleaner _jiraStoryCleaner;
        private readonly IConfluencePageReferenceExtractor _confluencePageReferenceExtractor;
        private readonly IConfluencePageService _confluencePageService;
        private readonly IConfluencePageCleaner _confluencePageCleaner;
        private readonly IAttachmentDownloadService _attachmentDownloadService;
        private readonly IConfluencePageSearchOrchestrator _confluencePageSearchOrchestrator;
        private readonly IConfluenceCommentsService _confluenceCommentsService;
        private readonly IConfluencePageSummaryService _confluencePageSummary;

        public UserMessageBuilder(
            IJiraStoryService jiraStoryService,
            IJiraRemoteLinksService jiraRemoteLinksService,
            IJiraStoryCleaner jiraStoryCleaner,
            IConfluencePageReferenceExtractor confluencePageReferenceExtractor,
            IConfluencePageService confluencePageService,
            IConfluencePageCleaner confluencePageCleaner,
            IConfluenceAttachmentsService confluenceAttachmentsService,
            IConfluenceAttachmentsCleaner confluenceAttachmentsCleaner,
            IAttachmentDownloadService attachmentDownloadService,
            IConfluencePageSearchOrchestrator confluencePageSearchOrchestrator,
            IConfluenceCommentsService confluenceCommentsService,
            IConfluencePageSummaryService confluencePageSummary
            )
        {
            _jiraStoryService = jiraStoryService;
            _jiraRemoteLinksService = jiraRemoteLinksService;
            _jiraStoryCleaner = jiraStoryCleaner;
            _confluencePageReferenceExtractor = confluencePageReferenceExtractor;
            _confluencePageService = confluencePageService;
            _confluencePageCleaner = confluencePageCleaner;
            _attachmentDownloadService = attachmentDownloadService;
            _confluencePageSearchOrchestrator = confluencePageSearchOrchestrator;
            _confluenceCommentsService = confluenceCommentsService;
            _confluencePageSummary = confluencePageSummary;
        }

        public async Task<ChatMessageContentItemCollection> BuildUserMessageAsync(string jiraId)
        {
            
            var userMessage = new ChatMessageContentItemCollection();

            // Jira Story Section

            JsonElement rawJiraStory = await _jiraStoryService.GetJiraStoryAsync(jiraId);
            JsonElement rawJiraRemoteLinks = await _jiraRemoteLinksService.GetJiraRemoteLinksAsync(jiraId);

            JiraStoryDto cleanedJiraStory = _jiraStoryCleaner.CleanJiraStory(rawJiraStory, rawJiraRemoteLinks);

            // Add Jira story to user message
            userMessage.Add(new TextContent("Jira story: "));
            userMessage.Add(new TextContent(JsonSerializer.Serialize(cleanedJiraStory)));

            if (cleanedJiraStory.Attachments?.Count > 0)
            {
                foreach (DTOs.Jira.Attachment attachment in cleanedJiraStory.Attachments)
                {
                    var jiraAttachment = await _attachmentDownloadService.DownloadAttachmentAsync(attachment.DownloadLink, attachment.MediaType);
                    userMessage.Add(jiraAttachment);
                }
            }

            // Confluence Page(s) Section

            // Get confluence page links from Jira story
            ConfluencePageReferencesDto confluencePageReferences = await _confluencePageReferenceExtractor.GetConfluencePageReferencesAsync(cleanedJiraStory);

            if (confluencePageReferences.ConfluencePages?.Count == 0)
            {
                // Search for confluence page links if not provided in Jira story
                confluencePageReferences = await _confluencePageSearchOrchestrator.GetConfluencePageReferencesAsync(cleanedJiraStory);
            }

            if (confluencePageReferences.ConfluencePages?.Count > 0)
            {
                foreach (ConfluencePage confluencePage in confluencePageReferences.ConfluencePages)
                {
                    // Get Confluence page
                    userMessage.Add(new TextContent("Consider the following confluence page only if it is relevant to the above Jira story:"));
                    JsonElement rawConfluencePage = await _confluencePageService.GetConfluencePageAsync(confluencePage.baseUrl, confluencePage.pageId);
                    JsonElement rawConfluenceComments = await _confluenceCommentsService.GetConfluenceCommentsAsync(confluencePage.baseUrl, confluencePage.pageId);

                    // Summarize Confluence page
                    ConfluencePageDto cleanedConfluencePage = _confluencePageCleaner.CleanConfluencePage(rawConfluencePage, rawConfluenceComments);
                    string summarizedConfluencePageBody = await _confluencePageSummary.SummarizeConfluencePageAsync(cleanedConfluencePage, confluencePage.baseUrl);

                    // Add summarized Confluence page to user message
                    userMessage.Add(new TextContent(summarizedConfluencePageBody));

                }
                
            }

            return userMessage;

        }
    }
}
