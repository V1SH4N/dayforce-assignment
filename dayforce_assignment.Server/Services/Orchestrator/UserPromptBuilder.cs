using dayforce_assignment.Server.DTOs.Common;
using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.DTOs.Jira;
using dayforce_assignment.Server.Exceptions;
using dayforce_assignment.Server.Interfaces.Common;
using dayforce_assignment.Server.Interfaces.Confluence;
using dayforce_assignment.Server.Interfaces.EventSinks;
using dayforce_assignment.Server.Interfaces.Jira;
using dayforce_assignment.Server.Interfaces.Orchestrator;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;
using System.Text.Json.Serialization;


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

        public UserPromptBuilder(
            IJiraMapperService jiraMapperService,
            ITriageSubtaskService triageSubtaskService,
            IConfluenceHttpClientService confluenceHttpClientService,
            IConfluenceMapperService confluenceMapperService,
            IConfluencePageReferenceExtractor confluencePageReferenceExtractor,
            IConfluencePageSummaryService confluencePageSummaryService,
            IAttachmentService attachmentService,
            IConfluencePageSearchOrchestrator confluencePageSearchOrchestrator
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
        }


        // Build user prompt.
        public async Task<ChatMessageContentItemCollection> BuildAsync(JiraIssueDto jiraIssue, bool isBugIssue, bool summarizeAttachment, ISseEventSink events, CancellationToken cancellationToken)
        {
            var userPrompt = new ChatMessageContentItemCollection();

            // Add jira issue to user prompt.
            await AddJiraIssueAsync(userPrompt, jiraIssue, summarizeAttachment, cancellationToken);


            // Get relevant confluence pages references
            ConfluencePageReferencesDto confluencePageReferences = await GetRelevantConfluencePagesAsync(jiraIssue, cancellationToken);

            // Add relevant confluence pages' content to user prompt.
            await AddConfluencePagesAsync(userPrompt, confluencePageReferences, summarizeAttachment, events, cancellationToken);

            if (isBugIssue)
            {
                // Add triage subtask to user prompt.
                await AddTriageSubtaskAsync(userPrompt, jiraIssue, summarizeAttachment, events, cancellationToken);
            }

            return userPrompt;
        }




        // Adds jira issue ( with attachments ) to user prompt.
        private async Task AddJiraIssueAsync(ChatMessageContentItemCollection userPrompt, JiraIssueDto jiraIssue, bool summarizeAttachment, CancellationToken cancellationToken)
        {
            userPrompt.Add(new TextContent("Jira issue:"));

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new JsonStringEnumConverter() }
            };

            userPrompt.Add(new TextContent(JsonSerializer.Serialize(jiraIssue, options)));

            if (jiraIssue.Attachments.Any() == true)
            {
                await AddAttachmentsAsync(userPrompt, jiraIssue.Attachments, summarizeAttachment, "Jira issue attachments:", cancellationToken);
            }
        }




        // Finds subtask with type "Triage".
        // Adds triage subtask's content (with comments & attachments) to user prompt.
        private async Task AddTriageSubtaskAsync(ChatMessageContentItemCollection userPrompt, JiraIssueDto jiraIssue, bool summarizeAttachment, ISseEventSink events, CancellationToken cancellationToken)
        {
            var jsonTriageSubtask = new JsonElement();
            try
            {
                jsonTriageSubtask = await _triageSubtaskService.GetSubtaskAsync(jiraIssue, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                return;
            }

            if (jsonTriageSubtask.ValueKind == JsonValueKind.Undefined)
                return;

            TriageSubtaskDto triageSubtask = _jiraMapperService.MapTriageSubtaskToDto(jsonTriageSubtask);

            await events.SubtaskStartAsync(triageSubtask.Key, triageSubtask.Title, cancellationToken);

            try 
            { 
                userPrompt.Add(new TextContent("Triage subtask:"));
                userPrompt.Add(new TextContent(JsonSerializer.Serialize(triageSubtask, new JsonSerializerOptions { WriteIndented = true })));

                if (triageSubtask.Attachments?.Any() == true)
                {
                    try
                    {
                        await AddAttachmentsAsync(userPrompt, triageSubtask.Attachments, summarizeAttachment, "Triage subtask attachments:", cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception) { }
                }

                await events.SubtaskFinishedAsync(triageSubtask.Key, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (ex is DomainException)
                {
                    await events.SubtaskErrorAsync(triageSubtask.Key, ex.Message, cancellationToken);
                }
                else
                {
                    string errorMessage = "An unexpected error has occured";
                    await events.SubtaskErrorAsync(triageSubtask.Key, errorMessage, cancellationToken);
                }

            }
        }




        // Downloads attachment.
        // Summarizes attachment if (summarizeAttachment is true), then adds it to user prompt.
        private async Task AddAttachmentsAsync(ChatMessageContentItemCollection userPrompt, IEnumerable<Attachment> attachments, bool summarizeAttachment, string header, CancellationToken cancellationToken)
        {
            userPrompt.Add(new TextContent(header + "\n"));

            if (summarizeAttachment)
            {
                List<string> summarizedAttachments = await _attachmentService.SummarizeAttachmentListAsync(attachments, cancellationToken);
                foreach (string attachment in summarizedAttachments)
                {
                    userPrompt.Add(new TextContent(attachment));
                }
            }
            else
            {
                List<KernelContent> downloadedAttachments = await _attachmentService.DownloadAttachmentListAsync(attachments, cancellationToken);
                foreach (KernelContent attachment in downloadedAttachments)
                {
                    userPrompt.Add(attachment);
                }
            }
        }




        // Extracts any confluence pages references from Jira issue.
        // Searches for any additional confluence page references if information is lacking.
        private async Task<ConfluencePageReferencesDto> GetRelevantConfluencePagesAsync(JiraIssueDto jiraIssue, CancellationToken cancellationToken)
        {
            ConfluencePageReferencesDto confluencePageRefereces = _confluencePageReferenceExtractor.GetReferences(jiraIssue);

            if (jiraIssue.IssueType == IssueType.Story) 
            {
                await _confluencePageSearchOrchestrator.SearchConfluencePageReferencesAsync(jiraIssue, confluencePageRefereces, cancellationToken);
            }
            return confluencePageRefereces;
        }




        // Adds summary of each confluence page's content (including its attachments) to user prompt.
        private async Task AddConfluencePagesAsync(ChatMessageContentItemCollection userPrompt, ConfluencePageReferencesDto confluencePageReferences, bool summarizeAttachment, ISseEventSink events, CancellationToken cancellationToken)
        {
            var confluencePageTasks = confluencePageReferences.ConfluencePages.Select(async page =>
            {
                try
                {
                    var jsonConfluencePage = new JsonElement();
                    try
                    {
                        jsonConfluencePage = await _confluenceHttpClientService.GetPageAsync(page.Value.baseUrl, page.Value.pageId, cancellationToken);
                    }
                    catch (Exception)
                    {
                        await events.ConfluencePageStartAsync(page.Key, "Untitled page", cancellationToken);
                        throw;
                    }
                    var jsonConfluenceComments = new JsonElement();
                    try
                    {
                        jsonConfluenceComments = await _confluenceHttpClientService.GetCommentsAsync(page.Value.baseUrl, page.Value.pageId, cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        throw;
                    }
                    catch (Exception) { } 

                    ConfluencePageDto confluencePage = _confluenceMapperService.MapPageToDto(jsonConfluencePage, jsonConfluenceComments);
                    await events.ConfluencePageStartAsync(confluencePage.Id, confluencePage.Title, cancellationToken);

                    string summarizedConfluencePage = await _confluencePageSummaryService.SummarizePageAsync(confluencePage, page.Value.baseUrl, summarizeAttachment, cancellationToken, events);

                    await events.ConfluencePageFinishedAsync(confluencePage.Id, cancellationToken);

                    return $"Consider the following summarized confluence page only if it is directly relevant to the above Jira issue:\n{summarizedConfluencePage}";
                }
                catch (Exception ex)
                {
                    if (ex is DomainException)
                    {
                        await events.ConfluencePageErrorAsync(page.Key, ex.Message, cancellationToken);
                    }
                    else
                    {
                        string errorMessage = "An unexpected error has occured.";
                        await events.ConfluencePageErrorAsync(page.Key, errorMessage, cancellationToken);

                    }
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
