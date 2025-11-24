using dayforce_assignment.Server.DTOs.Confluence;
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
                var downloadTasks = cleanedConfluencePageAttachments.Attachments.Select(async attachment =>
                {
                    try
                    {
                        var confluenceAttachment = await _attachmentDownloadService.DownloadAttachmentAsync(
                            attachment.DownloadLink,
                            attachment.mediaType
                        );

                        return confluenceAttachment;
                    }
                    catch
                    {
                        // Ignore unsupported attachments
                        return null;
                    }
                });

                var results = await Task.WhenAll(downloadTasks);

                foreach (var result in results)
                {
                    if (result != null)
                    {
                        userMessage.Add(result); 
                    }
                }
            }

            var response = await _chatCompletionService.GetChatMessageContentAsync(history);

            return response.ToString().Trim();
        }
    }
}
