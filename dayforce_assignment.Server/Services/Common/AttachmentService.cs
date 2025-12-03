using dayforce_assignment.Server.Exceptions;
using dayforce_assignment.Server.Interfaces.Common;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace dayforce_assignment.Server.Services.Common
{
    public class AttachmentService: IAttachmentService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IChatCompletionService _chatCompletionService;
        private readonly ILogger<AttachmentService> _logger;


        public AttachmentService(IHttpClientFactory httpClientFactory, IChatCompletionService chatCompletionService, ILogger<AttachmentService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _chatCompletionService = chatCompletionService;
            _logger = logger;
        }

        public async Task<KernelContent> DownloadAttachmentAsync(string downloadLink, string mediaType)
        {
            
            var httpClient = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");

            byte[] responseBytes;

            try
            {
                var response = await httpClient.GetAsync(downloadLink);

                // Redirect status codes
                if ((int)response.StatusCode == 302)
                {
                    var redirectDownloadLink = response.Headers.Location;
                    responseBytes = await httpClient.GetByteArrayAsync(redirectDownloadLink);
                }
                else
                {
                    responseBytes = await response.Content.ReadAsByteArrayAsync();

                }
            }
            catch (Exception)
            {
                throw new AttachmentDownloadException(downloadLink);
            }

            if (mediaType.StartsWith("image/"))
            {
                return new ImageContent(new ReadOnlyMemory<byte>(responseBytes), mediaType);
            }

            if (mediaType.StartsWith("text/"))
            {
                return new TextContent(Encoding.UTF8.GetString(responseBytes));
            }

            throw new UnsupportedAttachmentMediaTypeException(mediaType);

        }



        public async Task<List<KernelContent>> DownloadAttachmentListAsync(IEnumerable<DTOs.Common.Attachment> attachments)
        {
            if (attachments == null || !attachments.Any())
                return new List<KernelContent>();

            var tasks = attachments.Select(async att =>
            {
                try
                {
                    return await DownloadAttachmentAsync(att.DownloadLink, att.MediaType);
                }
                catch
                {
                    _logger.LogWarning($"Download attachment failed for {att.DownloadLink}");
                    return null;
                }
            });

            var results = await Task.WhenAll(tasks);

            return results.Where(r => r != null).ToList()!;
        }








        public async Task<List<string>> SummarizeAttachmentListAsync(IEnumerable<DTOs.Common.Attachment> attachments)
        {
            
            var result = new List<string>();

            if (attachments == null || !attachments.Any())
                return result;


            var tasks = attachments.Select(async att =>
            {
                try
                {
                    KernelContent downloadedAttachment = await DownloadAttachmentAsync(att.DownloadLink, att.MediaType);

                    try
                    {
                        switch (downloadedAttachment)
                        {
                            case TextContent text:
                                // Directly return the text data
                                return text.Text;

                            case ImageContent image:
                                // Summarize the image using the LLM
                                return await SummarizeImageAttachmentAsync(image);

                            default:
                                return null;
                        }
                    }
                    catch (Exception)
                    {
                        _logger.LogWarning($"Failed to summarize image attachment failed for {att.DownloadLink}. Skipping attachment");
                        return null;
                    }
                }
                catch
                {
                    _logger.LogWarning($"Download attachment failed for {att.DownloadLink}. Skipping attachment");
                    return null;
                }
            });

            var results = await Task.WhenAll(tasks);

            return results
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToList()!;
        }



        public async Task<string> SummarizeImageAttachmentAsync(ImageContent attachment)
        {
            var history = new ChatHistory();
            var userPrompt = new ChatMessageContentItemCollection();

            // Load system prompt
            string systemPromptPath = "SystemPrompts/ImageAttachmentSummary.txt";

            if (!File.Exists(systemPromptPath))
                throw new FileNotFoundException($"System prompt file not found: {systemPromptPath}");

            string systemPrompt = await File.ReadAllTextAsync(systemPromptPath);

            history.AddSystemMessage(systemPrompt);

            userPrompt.Add(attachment);

            history.AddUserMessage(userPrompt);
            try
            {
                var response = await _chatCompletionService.GetChatMessageContentAsync(history);
                return response.ToString().Trim();

            }
            catch
            {
                throw new AttachmentSummaryException();
            }
        }





    }
}
