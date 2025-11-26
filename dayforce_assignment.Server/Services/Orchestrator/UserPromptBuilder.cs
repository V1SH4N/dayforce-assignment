//using dayforce_assignment.Server.DTOs.Jira;
//using dayforce_assignment.Server.Exceptions;
//using dayforce_assignment.Server.Interfaces.Common;
//using dayforce_assignment.Server.Interfaces.Confluence;
//using dayforce_assignment.Server.Interfaces.Jira;
//using dayforce_assignment.Server.Interfaces.Orchestrator;
//using dayforce_assignment.Server.Services.Jira;
//using Microsoft.SemanticKernel;
//using Microsoft.SemanticKernel.ChatCompletion;
//using System.Text.Json;

//namespace dayforce_assignment.Server.Services.Orchestrator
//{
//    public class UserPromptBuilder : IUserPromptBuilder
//    {
//        private readonly ITriageSubtaskService _triageSubtaskService;
//        private readonly ITriageSubtaskCleanerService _triageSubtaskCleanerService;
//        private readonly IConfluencePageReferenceExtractor _confluencePageReferenceExtractor;
//        private readonly IConfluencePageService _confluencePageService;
//        private readonly IConfluencePageCleaner _confluencePageCleaner;
//        private readonly IAttachmentDownloadService _attachmentDownloadService;
//        private readonly IConfluencePageSearchOrchestrator _confluencePageSearchOrchestrator;
//        private readonly IConfluenceCommentsService _confluenceCommentsService;
//        private readonly IConfluencePageSummaryService _confluencePageSummary;
//        private readonly ILogger<GlobalExceptionMiddleware> _logger;



//        public UserPromptBuilder(
//            ITriageSubtaskService triageSubtaskService,
//            ITriageSubtaskCleanerService triageSubtaskCleanerService,
//            IConfluencePageReferenceExtractor confluencePageReferenceExtractor,
//            IConfluencePageService confluencePageService,
//            IConfluencePageCleaner confluencePageCleaner,
//            IConfluenceAttachmentsService confluenceAttachmentsService,
//            IConfluenceAttachmentsCleaner confluenceAttachmentsCleaner,
//            IAttachmentDownloadService attachmentDownloadService,
//            IConfluencePageSearchOrchestrator confluencePageSearchOrchestrator,
//            IConfluenceCommentsService confluenceCommentsService,
//            IConfluencePageSummaryService confluencePageSummary,
//            ILogger<GlobalExceptionMiddleware> logger
//        )
//        {
//            _triageSubtaskService = triageSubtaskService;
//            _triageSubtaskCleanerService = triageSubtaskCleanerService;
//            _confluencePageReferenceExtractor = confluencePageReferenceExtractor;
//            _confluencePageService = confluencePageService;
//            _confluencePageCleaner = confluencePageCleaner;
//            _attachmentDownloadService = attachmentDownloadService;
//            _confluencePageSearchOrchestrator = confluencePageSearchOrchestrator;
//            _confluenceCommentsService = confluenceCommentsService;
//            _confluencePageSummary = confluencePageSummary;
//            _logger = logger;
//        }

//        public async Task<ChatMessageContentItemCollection> BuildAsync(JiraIssueDto jiraIssue, bool IsBugIssue)
//        {
//            var userMessage = new ChatMessageContentItemCollection();
//            var jiraKey = jiraIssue.Key;

//            try
//            {
//                userMessage.Add(new TextContent("Jira issue:"));
//                userMessage.Add(new TextContent(JsonSerializer.Serialize(jiraIssue)));

//                // Jira attachments
//                if (jiraIssue.Attachments?.Count > 0)
//                {
//                    var downloadTasks = jiraIssue.Attachments
//                        .Select(async att =>
//                        {
//                            try
//                            {
//                                return await _attachmentDownloadService.DownloadAttachmentAsync(att.DownloadLink, att.MediaType);
//                            }
//                            catch (Exception ex)
//                            {
//                                _logger.LogWarning(ex, "Failed to download attachment {Link}, skipping.", att.DownloadLink);
//                                return null;
//                            }
//                        })
//                        .ToList();

//                    var downloadedAttachments = await Task.WhenAll(downloadTasks);

//                    foreach (var attachment in downloadedAttachments)
//                        if (attachment != null)
//                            userMessage.Add(attachment);
//                }

//                // Confluence page references
//                var confluencePageReferences = await _confluencePageReferenceExtractor.GetConfluencePageReferencesAsync(jiraIssue);


//                if (confluencePageReferences.ConfluencePages?.Count == 0 && !IsBugIssue)
//                {
//                    // fallback search if no references in Jira
//                    confluencePageReferences = await _confluencePageSearchOrchestrator.GetConfluencePageReferencesAsync(jiraIssue);
//                }

//                if (confluencePageReferences.ConfluencePages?.Count > 0)
//                {
//                    var confluenceTasks = confluencePageReferences.ConfluencePages.Select(async page =>
//                    {
//                        try
//                        {
//                            // Get Confluence page and comments concurrently
//                            var rawPageTask = _confluencePageService.GetConfluencePageAsync(page.baseUrl, page.pageId);
//                            var rawCommentsTask = _confluenceCommentsService.GetConfluenceCommentsAsync(page.baseUrl, page.pageId);
//                            await Task.WhenAll(rawPageTask, rawCommentsTask);

//                            var cleanedPage = _confluencePageCleaner.CleanConfluencePage(rawPageTask.Result, rawCommentsTask.Result);

//                            // Summarize page
//                            var summarizedBody = await _confluencePageSummary.SummarizeConfluencePageAsync(cleanedPage, page.baseUrl);

//                            return $"Consider the following summarized confluence page only if it is relevant to the above Jira story:\n{summarizedBody}";
//                        }
//                        catch (Exception ex)
//                        {
//                            _logger.LogWarning(ex, "Skipping Confluence page {PageId}", page.pageId);
//                            return null;
//                        }

//                    }).ToList();

//                    var summarizedPages = await Task.WhenAll(confluenceTasks);

//                    foreach (var pageContent in summarizedPages)
//                        userMessage.Add(new TextContent(pageContent));
//                }

//                if (IsBugIssue)
//                {
//                    // Triage subtask
//                    JsonElement rawTriageSubtask = await _triageSubtaskService.GetTriageSubTaskAsync(jiraIssue);
//                    TriageSubtaskDto triageSubtask = _triageSubtaskCleanerService.CleanTriageSubtask(rawTriageSubtask);

//                    userMessage.Add(new TextContent(JsonSerializer.Serialize(triageSubtask)));

//                    // Triage attachments
//                    if (triageSubtask.Attachments?.Count > 0)
//                    {
//                        var downloadTasks = triageSubtask.Attachments
//                            .Select(async att =>
//                            {
//                                try
//                                {
//                                    return await _attachmentDownloadService.DownloadAttachmentAsync(att.DownloadLink, att.MediaType);
//                                }
//                                catch (Exception ex)
//                                {
//                                    _logger.LogWarning(ex, "Failed to download attachment {Link}, skipping.", att.DownloadLink);
//                                    return null;
//                                }
//                            })
//                            .ToList();

//                        var downloadedAttachments = await Task.WhenAll(downloadTasks);

//                        foreach (var attachment in downloadedAttachments)
//                            if (attachment != null)
//                                userMessage.Add(attachment);
//                    }
//                }
//                return userMessage;
//            }
//            catch (DomainException)
//            {
//                throw;
//            }
//            catch (Exception ex)
//            {
//                throw new UserMessageBuilderException(jiraKey, ex.Message);
//            }
//        }
using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Exceptions;
using dayforce_assignment.Server.Interfaces.Common;
using dayforce_assignment.Server.Interfaces.Confluence;
using dayforce_assignment.Server.Interfaces.Jira;
using dayforce_assignment.Server.Interfaces.Orchestrator;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Orchestrator
{
    public class UserPromptBuilder : IUserPromptBuilder
    {
        private readonly ITriageSubtaskService _triageSubtaskService;
        private readonly ITriageSubtaskMapper _triageSubtaskMapper;
        private readonly IConfluencePageReferenceExtractor _confluencePageReferenceExtractor;
        private readonly IConfluencePageService _confluencePageService;
        private readonly IConfluencePageCleaner _confluencePageCleaner;
        private readonly IAttachmentListDownloadService _attachmentListDownloadService;
        private readonly IConfluencePageSearchOrchestrator _confluencePageSearchOrchestrator;
        private readonly IConfluenceCommentsService _confluenceCommentsService;
        private readonly IConfluencePageSummaryService _confluencePageSummary;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;



        public UserPromptBuilder(
            ITriageSubtaskService triageSubtaskService,
            ITriageSubtaskMapper triageSubtaskMapper,
            IConfluencePageReferenceExtractor confluencePageReferenceExtractor,
            IConfluencePageService confluencePageService,
            IConfluencePageCleaner confluencePageCleaner,
            IConfluenceAttachmentsService confluenceAttachmentsService,
            IConfluenceAttachmentsCleaner confluenceAttachmentsCleaner,
            IAttachmentListDownloadService attachmentListDownloadService,
            IConfluencePageSearchOrchestrator confluencePageSearchOrchestrator,
            IConfluenceCommentsService confluenceCommentsService,
            IConfluencePageSummaryService confluencePageSummary,
            ILogger<GlobalExceptionMiddleware> logger
        )
        {
            _triageSubtaskService = triageSubtaskService;
            _triageSubtaskMapper = triageSubtaskMapper;
            _confluencePageReferenceExtractor = confluencePageReferenceExtractor;
            _confluencePageService = confluencePageService;
            _confluencePageCleaner = confluencePageCleaner;
            _attachmentListDownloadService = attachmentListDownloadService;
            _confluencePageSearchOrchestrator = confluencePageSearchOrchestrator;
            _confluenceCommentsService = confluenceCommentsService;
            _confluencePageSummary = confluencePageSummary;
            _logger = logger;
        }

        public async Task<ChatMessageContentItemCollection> BuildAsync(JiraIssueDto jiraIssue, bool isBugIssue)
        {
            var userPrompt = new ChatMessageContentItemCollection();
            var jiraKey = jiraIssue.Key;

            try
            {
                // Add Jira issue to user prompt
                await AddJiraIssueAsync(userPrompt, jiraIssue);

                // Get list of relevant confluence page references
                ConfluencePageReferencesDto confluencePageReferences = await GetRelevantConfluencePagesAsync(jiraIssue, isBugIssue);

                // Add summary of relevant confluence pages to user prompt
                await AddConfluencePagesAsync(userPrompt, confluencePageReferences);

                if (isBugIssue)
                {
                    // Add triage subtask to user prompt
                    await AddTriageSubtaskAsync(userPrompt, jiraIssue);
                }

                return userPrompt;

            }
            catch (Exception ex) when (!(ex is DomainException))
            {
                throw new UserMessageBuilderException(jiraKey, "An unexpected error has occured.");
            }
        }



        // Task which adds jira issue along with any attachment to user prompt
        private async Task AddJiraIssueAsync(ChatMessageContentItemCollection userPrompt, JiraIssueDto jiraIssue)
        {
            userPrompt.Add(new TextContent("Jira issue:"));
            userPrompt.Add(new TextContent(JsonSerializer.Serialize(jiraIssue, new JsonSerializerOptions { WriteIndented = true })));

            if (jiraIssue.Attachments?.Any() == true)
            {
                var downloadedAttachments = await _attachmentListDownloadService.DownloadAttachmentsAsync(jiraIssue.Attachments);
                foreach (var attachment in downloadedAttachments)
                {
                    userPrompt.Add(attachment);
                }
            }
        }
        
        // Task which extracts any relevant confluence pages from jira issue. If (none found & issue is not a bug), searches for relevant confluence pages. 
        private async Task<ConfluencePageReferencesDto> GetRelevantConfluencePagesAsync(JiraIssueDto jiraIssue, bool isBugIssue)
        {
            ConfluencePageReferencesDto confluencePageReferences = await _confluencePageReferenceExtractor.GetConfluencePageReferencesAsync(jiraIssue);

            if (!confluencePageReferences.ConfluencePages.Any() && !isBugIssue)
            {
                confluencePageReferences = await _confluencePageSearchOrchestrator.SearchConfluencePageReferencesAsync(jiraIssue);
            }

            return confluencePageReferences ?? new ConfluencePageReferencesDto();
        }

        // task which adds a summary of a each confluence page (including its attachments) to user prompt.
        private async Task AddConfluencePagesAsync(ChatMessageContentItemCollection userPrompt, ConfluencePageReferencesDto confluencePageReferences)
        {
            var confluencePageTasks = confluencePageReferences.ConfluencePages.Select(async page =>
            {
                try
                {
                    var jsonConfluencePageTask = _confluencePageService.GetConfluencePageAsync(page.baseUrl, page.pageId);
                    var jsonConfluenceCommentsTask = _confluenceCommentsService.GetConfluenceCommentsAsync(page.baseUrl, page.pageId);
                    await Task.WhenAll(jsonConfluencePageTask, jsonConfluenceCommentsTask);

                    ConfluencePageDto confluencePage = _confluencePageCleaner.CleanConfluencePage(jsonConfluencePageTask.Result, jsonConfluenceCommentsTask.Result);

                    String summarizedConfluencePage = await _confluencePageSummary.SummarizeConfluencePageAsync(confluencePage, page.baseUrl);

                    return $"Consider the following summarized confluence page only if it is relevant to the above Jira issue:\n{summarizedConfluencePage}";
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping Confluence page {PageId}", page.pageId);
                    return null;
                }
            });

            var results = await Task.WhenAll(confluencePageTasks);

            foreach (var pageContent in results.Where(c => !string.IsNullOrEmpty(c)))
            {
                userPrompt.Add(new TextContent(pageContent));
            }
        }

        // Task which find subtask with type "Triage" and add it (with comments and attachments) to user prompt.
        private async Task AddTriageSubtaskAsync(ChatMessageContentItemCollection userPrompt, JiraIssueDto jiraIssue)
        {
            JsonElement jsonTriageSubtask = await _triageSubtaskService.GetTriageSubTaskAsync(jiraIssue);
            TriageSubtaskDto triageSubtask = _triageSubtaskMapper.MapToDto(jsonTriageSubtask);

            userPrompt.Add(new TextContent("Triage subtask:"));
            userPrompt.Add(new TextContent(JsonSerializer.Serialize(triageSubtask, new JsonSerializerOptions { WriteIndented = true })));

            if (triageSubtask.Attachments?.Any() == true)
            {
                var downloadedAttachments = await _attachmentListDownloadService.DownloadAttachmentsAsync(triageSubtask.Attachments);
                foreach (var attachment in downloadedAttachments)
                    userPrompt.Add(attachment);
            }
        }
    }
}
