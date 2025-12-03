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
using System.Runtime.CompilerServices;
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


        // Builds user prompt by adding jira issue, relevant confluence pages, and triage subtask (if issue is a bug).
        public async Task<ChatMessageContentItemCollection> BuildAsync(JiraIssueDto jiraIssue, bool isBugIssue, bool summarizeAttachment)
        {
            var userPrompt = new ChatMessageContentItemCollection();

            await AddJiraIssueAsync(userPrompt, jiraIssue, summarizeAttachment);

            ConfluencePageReferencesDto confluencePageReferences = await GetRelevantConfluencePagesAsync(jiraIssue, isBugIssue);

            await AddConfluencePagesAsync(userPrompt, confluencePageReferences, summarizeAttachment);

            if (isBugIssue)
            {
                await AddTriageSubtaskAsync(userPrompt, jiraIssue, summarizeAttachment);
            }

            return userPrompt;
        }



        // Adds jira issue along with any attachments to user prompt.
        private async Task AddJiraIssueAsync(ChatMessageContentItemCollection userPrompt, JiraIssueDto jiraIssue, bool summarizeAttachment)
        {
            userPrompt.Add(new TextContent("Jira issue:"));
            userPrompt.Add(new TextContent(JsonSerializer.Serialize(jiraIssue, new JsonSerializerOptions { WriteIndented = true })));

            if (jiraIssue.Attachments?.Any() == true)
            {
                await AddAttachmentsAsync(userPrompt, jiraIssue.Attachments, summarizeAttachment, "Jira issue attachments:");
            }
        }



        // Find subtask with type "Triage" and add it (with comments and attachments) to user prompt.
        private async Task AddTriageSubtaskAsync(ChatMessageContentItemCollection userPrompt, JiraIssueDto jiraIssue, bool summarizeAttachment)
        {
            try
            {
                JsonElement jsonTriageSubtask = await _triageSubtaskService.GetSubtaskAsync(jiraIssue);
                TriageSubtaskDto triageSubtask = _jiraMapperService.MapTriageSubtaskToDto(jsonTriageSubtask);

                userPrompt.Add(new TextContent("Triage subtask:"));
                userPrompt.Add(new TextContent(JsonSerializer.Serialize(triageSubtask, new JsonSerializerOptions { WriteIndented = true })));

                if (triageSubtask.Attachments?.Any() == true)
                {
                    await AddAttachmentsAsync(userPrompt, triageSubtask.Attachments, summarizeAttachment, "Triage subtask attachments:");
                }
            }
            catch{
                _logger.LogWarning("Triage subtask not found for Jira issue {JiraIssueKey}", jiraIssue.Key);
                return;
            }
        }


        // Downloads attachment, summarize if (summarizeAttachment is true), then adds it to user prompt.
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



        // Extracts any relevant confluence pages references from jira issue. If (none found & issue is not a bug), searches for relevant confluence pages references. 
        private async Task<ConfluencePageReferencesDto> GetRelevantConfluencePagesAsync(JiraIssueDto jiraIssue, bool isBugIssue)
        {
            ConfluencePageReferencesDto confluencePageReferences = await _confluencePageReferenceExtractor.GetReferencesAsync(jiraIssue);
                
            if (!confluencePageReferences.ConfluencePages.Any() && !isBugIssue)
            {
                try
                {
                    confluencePageReferences = await _confluencePageSearchOrchestrator.SearchConfluencePageReferencesAsync(jiraIssue);
                    return confluencePageReferences ?? new ConfluencePageReferencesDto();
                }
                catch
                {
                    return new ConfluencePageReferencesDto();
                }
            }
            return confluencePageReferences;
        }



        // Adds a summary of a each confluence page (including its attachments) to user prompt.
        private async Task AddConfluencePagesAsync(ChatMessageContentItemCollection userPrompt, ConfluencePageReferencesDto confluencePageReferences, bool summarizeAttachment)
        {
            var confluencePageTasks = confluencePageReferences.ConfluencePages.Select(async page =>
            {
                try
                {
                    JsonElement jsonConfluencePage = await _confluenceHttpClientService.GetPageAsync(page.baseUrl, page.pageId);
                    JsonElement jsonConfluenceComments = await _confluenceHttpClientService.GetCommentsAsync(page.baseUrl, page.pageId);

                    ConfluencePageDto confluencePage = _confluenceMapperService.MapPageToDto(jsonConfluencePage, jsonConfluenceComments);

                    string summarizedConfluencePage = await _confluencePageSummaryService.SummarizePageAsync(confluencePage, page.baseUrl, summarizeAttachment);
                    return $"Consider the following summarized confluence page only if it is relevant to the above Jira issue:\n{summarizedConfluencePage}";
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Skipping Confluence page {PageId}", page.pageId);
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
