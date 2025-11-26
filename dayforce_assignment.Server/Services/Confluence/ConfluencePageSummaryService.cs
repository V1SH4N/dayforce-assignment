using dayforce_assignment.Server.DTOs.Confluence;
using dayforce_assignment.Server.Exceptions;
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
        private readonly IConfluenceAttachmentsMapper _confluenceAttachmentsMapper;
        private readonly IAttachmentDownloadService _attachmentDownloadService;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;


        public ConfluencePageSummaryService(
            IChatCompletionService chatCompletionService,
            IConfluenceAttachmentsMapper confluenceAttachmentsMapper,
            IConfluenceAttachmentsService confluenceAttachmentsService,
            IAttachmentDownloadService attachmentDownloadService,
            ILogger<GlobalExceptionMiddleware> logger
            )
        {
            _chatCompletionService = chatCompletionService;
            _confluenceAttachmentsMapper = confluenceAttachmentsMapper;
            _confluenceAttachmentsService = confluenceAttachmentsService;
            _attachmentDownloadService = attachmentDownloadService;
            _logger = logger;
        }

        public async Task<string> SummarizePageAsync(ConfluencePageDto confluencePage, string baseUrl)
        {
            var pageId = confluencePage?.Id ?? "unknown";

            try
            {
                var history = new ChatHistory();
                var userMessage = new ChatMessageContentItemCollection();

                string systemPrompt = File.ReadAllText("SystemPrompts/ConfluencePageSummary.txt");
                history.AddSystemMessage(systemPrompt);

                // Get Confluence attachments 
                var rawAttachments = await _confluenceAttachmentsService.GetAttachmentsAsync(baseUrl, pageId);
                var cleanedAttachments = _confluenceAttachmentsMapper.MapToDto(rawAttachments);

                history.AddUserMessage(JsonSerializer.Serialize(confluencePage));

                if (cleanedAttachments.Attachments?.Count > 0)
                {
                    var downloadTasks = cleanedAttachments.Attachments.Select(async attachment =>
                    {
                        try
                        {
                            return await _attachmentDownloadService.DownloadAttachmentAsync(
                                attachment.DownloadLink,
                                attachment.MediaType
                            );
                        }    
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Skipping attachment {Link} due to error.", attachment.DownloadLink);
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
            catch (DomainException)
            {
                throw; // propagate known domain exceptions
            }
            catch (Exception ex)
            {
                throw new ConfluencePageSummaryException(pageId, ex.Message);
            }
        }
    }
}
