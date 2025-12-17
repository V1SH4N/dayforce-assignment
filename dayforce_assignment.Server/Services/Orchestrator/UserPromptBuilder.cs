using dayforce_assignment.Server.DTOs.Common;
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
        private readonly IJiraMapperService _jiraMapperService;
        private readonly ITriageSubtaskService _triageSubtaskService;
        private readonly IConfluenceHttpClientService _confluenceHttpClientService;
        private readonly IConfluenceMapperService _confluenceMapperService;
        private readonly IConfluencePageReferenceExtractor _confluencePageReferenceExtractor;
        private readonly IConfluencePageSummaryService _confluencePageSummaryService;
        private readonly IAttachmentService _attachmentService;
        private readonly IConfluencePageSearchOrchestrator _confluencePageSearchOrchestrator;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;



        public UserPromptBuilder(
            IJiraMapperService jiraMapperService,
            ITriageSubtaskService triageSubtaskService,
            IConfluenceHttpClientService confluenceHttpClientService,
            IConfluenceMapperService confluenceMapperService,
            IConfluencePageReferenceExtractor confluencePageReferenceExtractor,
            IConfluencePageSummaryService confluencePageSummaryService,
            IAttachmentService attachmentService,
            IConfluencePageSearchOrchestrator confluencePageSearchOrchestrator,
            ILogger<GlobalExceptionMiddleware> logger
        )
        {
            _jiraMapperService = jiraMapperService;
            _triageSubtaskService = triageSubtaskService;
            _confluenceHttpClientService = confluenceHttpClientService;
            _confluenceMapperService = confluenceMapperService;
            _confluencePageReferenceExtractor = confluencePageReferenceExtractor;
            _confluencePageSummaryService = confluencePageSummaryService;
            _attachmentService = attachmentService;
            _confluencePageSearchOrchestrator = confluencePageSearchOrchestrator;
            _logger = logger;
        }


        // Build user prompt.
        public async Task<ChatMessageContentItemCollection> BuildAsync(JiraIssueDto jiraIssue, bool isBugIssue, bool summarizeAttachment)
        {
            var userPrompt = new ChatMessageContentItemCollection();

            // Add jira issue to user prompt.
            await AddJiraIssueAsync(userPrompt, jiraIssue, summarizeAttachment);

            // Get relevant confluence pages references
            ConfluencePageReferencesDto confluencePageReferences = await GetRelevantConfluencePagesAsync(jiraIssue);

            // Add relevant confluence pages to user prompt.
            await AddConfluencePagesAsync(userPrompt, confluencePageReferences, summarizeAttachment);


            if (isBugIssue)
            {
                // Add triage subtask to user prompt.
                await AddTriageSubtaskAsync(userPrompt, jiraIssue, summarizeAttachment);
            }

            return userPrompt;
        }



        // Add jira issue ( with attachments ) to user prompt.
        private async Task AddJiraIssueAsync(ChatMessageContentItemCollection userPrompt, JiraIssueDto jiraIssue, bool summarizeAttachment)
        {
            userPrompt.Add(new TextContent("Jira issue:"));
            userPrompt.Add(new TextContent(JsonSerializer.Serialize(jiraIssue, new JsonSerializerOptions { WriteIndented = true })));

            if (jiraIssue.Attachments?.Any() == true)
            {
                await AddAttachmentsAsync(userPrompt, jiraIssue.Attachments, summarizeAttachment, "Jira issue attachments:");
            }
        }



        // Get subtask with type "Triage" and add it (with comments and attachments) to user prompt. Skips triage subtask if exception occurs.
        private async Task AddTriageSubtaskAsync(ChatMessageContentItemCollection userPrompt, JiraIssueDto jiraIssue, bool summarizeAttachment)
        {
            try
            {
                JsonElement jsonTriageSubtask = await _triageSubtaskService.GetSubtaskAsync(jiraIssue);
                if (jsonTriageSubtask.ValueKind == JsonValueKind.Undefined)
                    return;

                TriageSubtaskDto triageSubtask = _jiraMapperService.MapTriageSubtaskToDto(jsonTriageSubtask);

                userPrompt.Add(new TextContent("Triage subtask:"));
                userPrompt.Add(new TextContent(JsonSerializer.Serialize(triageSubtask, new JsonSerializerOptions { WriteIndented = true })));

                if (triageSubtask.Attachments?.Any() == true)
                {
                    await AddAttachmentsAsync(userPrompt, triageSubtask.Attachments, summarizeAttachment, "Triage subtask attachments:");
                }
            }
            catch (Exception)
            {
                _logger.LogWarning("Unexpected error occured. Skipping Triage subtask");
                return;
            }
        }


        // Download attachment, summarize if (summarizeAttachment is true), then add to user prompt.
        private async Task AddAttachmentsAsync(ChatMessageContentItemCollection userPrompt, IEnumerable<Attachment> attachments, bool summarizeAttachment, string header)
        {
            userPrompt.Add(new TextContent(header + "\n"));

            if (summarizeAttachment)
            {
                List<string> summarizedAttachments = await _attachmentService.SummarizeAttachmentListAsync(attachments);
                foreach (string attachment in summarizedAttachments)
                {
                    userPrompt.Add(new TextContent(attachment));
                }
            }
            else
            {
                List<KernelContent> downloadedAttachments = await _attachmentService.DownloadAttachmentListAsync(attachments);
                foreach (KernelContent attachment in downloadedAttachments)
                {
                    userPrompt.Add(attachment);
                }
            }
        }



        // Extract any relevant confluence pages from Jira issue. Search for any additional confluence pages if information is lacking. Returns new ConfluencePageReferenceDto if no references found.
        private async Task<ConfluencePageReferencesDto> GetRelevantConfluencePagesAsync(JiraIssueDto jiraIssue)
        {
            ConfluencePageReferencesDto confluencePageRefereces = _confluencePageReferenceExtractor.GetReferences(jiraIssue);
            await _confluencePageSearchOrchestrator.SearchConfluencePageReferencesAsync(jiraIssue, confluencePageRefereces);
            return confluencePageRefereces;
        }



        // Add summary of each confluence page (including its attachments) to user prompt. Skips confluence page summary if exception is thrown.
        private async Task AddConfluencePagesAsync(ChatMessageContentItemCollection userPrompt, ConfluencePageReferencesDto confluencePageReferences, bool summarizeAttachment)
        {
            var confluencePageTasks = confluencePageReferences.ConfluencePages.Select(async page =>
            {
                try
                {
                    JsonElement jsonConfluencePage = await _confluenceHttpClientService.GetPageAsync(page.Value.baseUrl, page.Value.pageId);
                    JsonElement jsonConfluenceComments = await _confluenceHttpClientService.GetCommentsAsync(page.Value.baseUrl, page.Value.pageId);

                    ConfluencePageDto confluencePage = _confluenceMapperService.MapPageToDto(jsonConfluencePage, jsonConfluenceComments);

                    string summarizedConfluencePage = await _confluencePageSummaryService.SummarizePageAsync(confluencePage, page.Value.baseUrl, summarizeAttachment);
                    return $"Consider the following summarized confluence page only if it is directly relevant to the above Jira issue:\n{summarizedConfluencePage}";
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping Confluence page {PageId}", page.Value.pageId);
                    return null;
                }
            });

            var taskResults = await Task.WhenAll(confluencePageTasks);

            foreach (string pageSummary in taskResults.Where(summary => !string.IsNullOrEmpty(summary))!)
            {
                userPrompt.Add(new TextContent(pageSummary));
            }
        }



    }
}
