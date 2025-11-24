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
        private readonly IJiraIssueService _jiraStoryService;
        private readonly IJiraRemoteLinksService _jiraRemoteLinksService;
        private readonly IJiraIssueCleaner _jiraStoryCleaner;
        private readonly IConfluencePageReferenceExtractor _confluencePageReferenceExtractor;
        private readonly IConfluencePageService _confluencePageService;
        private readonly IConfluencePageCleaner _confluencePageCleaner;
        private readonly IAttachmentDownloadService _attachmentDownloadService;
        private readonly IConfluencePageSearchOrchestrator _confluencePageSearchOrchestrator;
        private readonly IConfluenceCommentsService _confluenceCommentsService;
        private readonly IConfluencePageSummaryService _confluencePageSummary;

        public UserMessageBuilder(
            IJiraIssueService jiraStoryService,
            IJiraRemoteLinksService jiraRemoteLinksService,
            IJiraIssueCleaner jiraStoryCleaner,
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

            // Get Jira issue
            var rawJiraStoryTask = _jiraStoryService.GetJiraIssueAsync(jiraId);
            var rawJiraRemoteLinksTask = _jiraRemoteLinksService.GetJiraRemoteLinksAsync(jiraId);

            await Task.WhenAll(rawJiraStoryTask, rawJiraRemoteLinksTask);

            JiraIssueDto cleanedJiraStory = _jiraStoryCleaner.CleanJiraIssue(rawJiraStoryTask.Result, rawJiraRemoteLinksTask.Result);

            userMessage.Add(new TextContent("Jira story:"));
            userMessage.Add(new TextContent(JsonSerializer.Serialize(cleanedJiraStory)));

            // Jira attachment
            if (cleanedJiraStory.Attachments?.Count > 0)
            {
                var jiraAttachmentTasks = cleanedJiraStory.Attachments.Select(att =>
                    _attachmentDownloadService.DownloadAttachmentAsync(att.DownloadLink, att.MediaType)
                ).ToList();

                var downloadedAttachments = await Task.WhenAll(jiraAttachmentTasks);

                foreach (var attachment in downloadedAttachments)
                {
                    if (attachment != null)
                        userMessage.Add(attachment);
                }
            }

            // get Confluence page references
            ConfluencePageReferencesDto confluencePageReferences = await _confluencePageReferenceExtractor.GetConfluencePageReferencesAsync(cleanedJiraStory);

            if (confluencePageReferences.ConfluencePages?.Count == 0)
            {
                // Search if no confluence page references in Jira issue
                confluencePageReferences = await _confluencePageSearchOrchestrator.GetConfluencePageReferencesAsync(cleanedJiraStory);
            }

            if (confluencePageReferences.ConfluencePages?.Count > 0)
            {
                var confluenceTasks = confluencePageReferences.ConfluencePages.Select(async page =>
                {
                    // Get Confluence page
                    var rawPageTask = _confluencePageService.GetConfluencePageAsync(page.baseUrl, page.pageId);
                    var rawCommentsTask = _confluenceCommentsService.GetConfluenceCommentsAsync(page.baseUrl, page.pageId);

                    await Task.WhenAll(rawPageTask, rawCommentsTask);

                    var cleanedConfluencePage = _confluencePageCleaner.CleanConfluencePage(rawPageTask.Result, rawCommentsTask.Result);

                    // Summarize Confluence page
                    var summarizedBody = await _confluencePageSummary.SummarizeConfluencePageAsync(cleanedConfluencePage, page.baseUrl);
                    return $"Consider the following summarized confluence page only if it is relevant to the above Jira story:\n{summarizedBody}";
                    
                }).ToList();

                var summarizedPages = await Task.WhenAll(confluenceTasks);

                foreach (var pageContent in summarizedPages)
                {
                    userMessage.Add(new TextContent(pageContent));
                }
            }

            return userMessage;
        }
    }
}
