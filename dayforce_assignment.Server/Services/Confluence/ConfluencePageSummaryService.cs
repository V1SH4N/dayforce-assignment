using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.Exceptions.ApiExceptions;
using dayforce_assignment.Server.Interfaces.Common;
using dayforce_assignment.Server.Interfaces.Confluence;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text.Json;

namespace dayforce_assignment.Server.Services.Confluence
{
    public class ConfluencePageSummaryService : IConfluencePageSummaryService
    {
        private readonly IChatCompletionService _chatCompletionService;
        private readonly IConfluenceAttachmentsService _confluenceAttachmentsService;
        private readonly IConfluenceAttachmentsCleaner _confluenceAttachmentsCleaner;
        private readonly IAttachmentDownloadService _attachmentDownloadService;

        public ConfluencePageSummaryService(
            IChatCompletionService chatCompletionService,
            IConfluenceAttachmentsCleaner confluenceAttachmentsCleaner,
            IConfluenceAttachmentsService confluenceAttachmentsService,
            IAttachmentDownloadService attachmentDownloadService)
        {
            _chatCompletionService = chatCompletionService;
            _confluenceAttachmentsCleaner = confluenceAttachmentsCleaner;
            _confluenceAttachmentsService = confluenceAttachmentsService;
            _attachmentDownloadService = attachmentDownloadService;
        }

        public async Task<string> SummarizeConfluencePageAsync(ConfluencePageDto confluencePage, string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new ApiException(
                    StatusCodes.Status400BadRequest,
                    "Confluence base URL cannot be empty.",
                    internalMessage: "Attempted to summarize Confluence page with empty base URL.");

            if (confluencePage == null || string.IsNullOrWhiteSpace(confluencePage.Id))
                throw new ApiException(
                    StatusCodes.Status400BadRequest,
                    "Invalid Confluence page.",
                    internalMessage: "ConfluencePageDto is null or missing Id.");

            try
            {
                var history = new ChatHistory();

                var userMessage = new ChatMessageContentItemCollection();

                string systemPrompt = File.ReadAllText("SystemPrompts/ConfluencePageSummary.txt");

                history.AddSystemMessage(systemPrompt);

                // Get Confluence attachments 
                var rawConfluencePageAttachments = await _confluenceAttachmentsService.GetConfluenceAttachmentsAsync(baseUrl, confluencePage.Id);

                var cleanedConfluencePageAttachments = _confluenceAttachmentsCleaner.CleanConfluenceAttachments(rawConfluencePageAttachments);

                history.AddUserMessage(JsonSerializer.Serialize(confluencePage));

                if (cleanedConfluencePageAttachments.Attachments?.Count > 0)
                {
                    foreach (var attachment in cleanedConfluencePageAttachments.Attachments)
                    {
                        try
                        {
                            var confluenceAttachment = await _attachmentDownloadService.DownloadAttachmentAsync(attachment.DownloadLink, attachment.mediaType);
                            userMessage.Add(confluenceAttachment);
                        }
                        catch
                        {
                            // Ignore unsupported attachments
                        }
                    }
                }

                var response = await _chatCompletionService.GetChatMessageContentAsync(history);

                return response.ToString().Trim();
            }
            catch (Exception ex)
            {
                throw new ApiException(
                    StatusCodes.Status502BadGateway,
                    "Failed to summarize Confluence page.",
                    internalMessage: ex.ToString());
            }
        }
    }
}
