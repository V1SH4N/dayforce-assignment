using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.Exceptions;
using dayforce_assignment.Server.Interfaces.Common;
using dayforce_assignment.Server.Interfaces.Confluence;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Confluence
{
    public class ConfluencePageSummaryService : IConfluencePageSummaryService
    {
        private readonly IChatCompletionService _chatCompletionService;
        private readonly IConfluenceHttpClientService _confluenceHttpClientService;
        private readonly IConfluenceMapperService _confluenceMapperService;
        private readonly IAttachmentService _attachmentService;

        public ConfluencePageSummaryService(
            IChatCompletionService chatCompletionService,
            IConfluenceHttpClientService confluenceHttpClientService,
            IConfluenceMapperService confluenceMapperService,
            IAttachmentService attachmentService
            )
        {
            _chatCompletionService = chatCompletionService;
            _confluenceHttpClientService = confluenceHttpClientService;
            _confluenceMapperService = confluenceMapperService;
            _attachmentService = attachmentService;
        }


        // Creates confluence page summary.
        public async Task<string> SummarizePageAsync(ConfluencePageDto confluencePage, string baseUrl, bool summarizeAttachment)
        {
            string pageId = confluencePage.Id;

            ChatMessageContent response;

            // Load system prompt
            string systemPromptPath = "SystemPrompts/ConfluencePageSummary.txt";

            if (!File.Exists(systemPromptPath))
                throw new FileNotFoundException($"System prompt file not found: {systemPromptPath}");

            string systemPrompt = await File.ReadAllTextAsync(systemPromptPath);

            // Get Confluence attachments 
            JsonElement jsonAttachments = await _confluenceHttpClientService.GetAttachmentsAsync(baseUrl, pageId);
            ConfluencePageAttachmentsDto confluencePageAttachments = _confluenceMapperService.MapAttachmentsToDto(jsonAttachments);


            ChatHistory history = await BuildChatHistoryAsync(systemPrompt, confluencePage, confluencePageAttachments, summarizeAttachment);

            try
            {
                try
                {
                    response = await _chatCompletionService.GetChatMessageContentAsync(history);
                }
                catch (HttpOperationException ex) when (ex.StatusCode.HasValue && (int)ex.StatusCode.Value == 413) // Error status code when chatHistory excees token limit
                {
                    history = await BuildChatHistoryAsync(systemPrompt, confluencePage, confluencePageAttachments, summarizeAttachment: true);
                    response = await _chatCompletionService.GetChatMessageContentAsync(history);
                }
            }
            catch (Exception ex) when (ex is not DomainException)
            {
                throw new ConfluencePageSummaryException(pageId, "Unexpected error");
            }

            return response.ToString().Trim();
        }


        // Builds chatHistory to summarize confluence page
        private async Task<ChatHistory> BuildChatHistoryAsync(
        string systemPrompt,
        ConfluencePageDto confluencePage,
        ConfluencePageAttachmentsDto confluencePageAttachments,
        bool summarizeAttachment)
        {
            var history = new ChatHistory();
            var userPrompt = new ChatMessageContentItemCollection();


            history.AddSystemMessage(systemPrompt);

            userPrompt.Add(new TextContent(JsonSerializer.Serialize(confluencePage, new JsonSerializerOptions { WriteIndented = true })));


            if (confluencePageAttachments.Attachments?.Any() == true)
            {
                await AddConfluenceAttachment(userPrompt, confluencePageAttachments, summarizeAttachment);
            }

            history.AddUserMessage(userPrompt);
            return history;
        }


        // Adds confluence page attachment to user prompt. (Summarizes image attachments if summarizeAttachment is true.)
        private async Task AddConfluenceAttachment(ChatMessageContentItemCollection userPrompt, ConfluencePageAttachmentsDto confluencePageAttachments, bool summarizeAttachment)
        {
            userPrompt.Add(new TextContent("Confluence page attachments:\n"));

            if (confluencePageAttachments.Attachments?.Any() == true)
            {
                if (summarizeAttachment)
                {
                    var summarizedAttachments = await _attachmentService.SummarizeAttachmentListAsync(confluencePageAttachments.Attachments);
                    foreach (var attachment in summarizedAttachments)
                    {
                        userPrompt.Add(new TextContent(attachment));
                    }
                }
                else
                {
                    var downloadedAttachments = await _attachmentService.DownloadAttachmentListAsync(confluencePageAttachments.Attachments);
                    foreach (var attachment in downloadedAttachments)
                    {
                        userPrompt.Add((attachment));
                    }
                }
            }
        }



    }
}
        


