using dayforce_assignment.Server.DTOs.Common;
using dayforce_assignment.Server.Exceptions;
using dayforce_assignment.Server.Interfaces.Common;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Text;

namespace dayforce_assignment.Server.Services.Common
{
    public class AttachmentService: IAttachmentService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IChatCompletionService _chatCompletionService;

        public AttachmentService(IHttpClientFactory httpClientFactory, IChatCompletionService chatCompletionService)
        {
            _httpClientFactory = httpClientFactory;
            _chatCompletionService = chatCompletionService;        
        }


        // Download attachments.
        // Throws exception if attachment fails.
        // Trims attachment if it is csv file.
        public async Task<KernelContent> DownloadAttachmentAsync(string downloadLink, string mediaType, string fileName, CancellationToken cancellationToken)
        {
            var httpClient = _httpClientFactory.CreateClient("AtlassianAuthenticatedClient");

            byte[] responseBytes;
            try
            {
                var response = await httpClient.GetAsync(downloadLink, cancellationToken);

                // Redirect status codes
                if ((int)response.StatusCode == 302)
                {
                    var redirectDownloadLink = response.Headers.Location;
                    responseBytes = await httpClient.GetByteArrayAsync(redirectDownloadLink, cancellationToken);
                }
                else
                {
                    responseBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);

                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new AttachmentDownloadException(fileName);
            }

            if (mediaType.StartsWith("image/"))
            {
                return new ImageContent(new ReadOnlyMemory<byte>(responseBytes), mediaType);
            }
            else if (mediaType.StartsWith("text/"))
            {
                if (fileName.EndsWith(".csv"))
                {
                    return GetCsvFirstNLinesAsync(responseBytes, 15);
                }

                return new TextContent(Encoding.UTF8.GetString(responseBytes));
            }
            else
            {
                throw new UnsupportedAttachmentMediaTypeException(fileName);
            }

        }



        // Download attachment list.
        // Input List<Attachment>.
        // Returns List<kernelContent>.
        // Skips attachment if download throws exception.
        public async Task<List<KernelContent>> DownloadAttachmentListAsync(IEnumerable<Attachment> attachments, CancellationToken cancellationToken)
        {
            if (attachments == null || !attachments.Any())
                return new List<KernelContent>();

            var tasks = attachments.Select(async att =>
            {
                try
                {
                    return await DownloadAttachmentAsync(att.DownloadLink, att.MediaType, att.FileName, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception) 
                {
                    return null;
                }
            });

            var results = await Task.WhenAll(tasks);

            return results.Where(r => r != null).ToList()!;
        }



        // Download & summarize attachment list.
        // Input List<Attachment>.
        // Returns List<string>.
        // Skips attachment if download/summarization throws exception.
        public async Task<List<string>> SummarizeAttachmentListAsync(IEnumerable<Attachment> attachments, CancellationToken cancellationToken)
        {
            
            var result = new List<string>();

            if (attachments == null || !attachments.Any())
                return result;


            var tasks = attachments.Select(async att =>
            {
                try
                {
                    KernelContent downloadedAttachment = await DownloadAttachmentAsync(att.DownloadLink, att.MediaType, att.FileName, cancellationToken);

                    switch (downloadedAttachment)
                    {
                        case TextContent text:
                            return text.Text;

                        case ImageContent image:
                            return await SummarizeImageAttachmentAsync(att.FileName, image, cancellationToken);

                        default:
                            return null;
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception)
                {
                    return null;
                }
            });

            var results = await Task.WhenAll(tasks);

            return results
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToList()!;
        }



        // Summarizes image attachments.
        // Throws exception if summary fails.
        public async Task<string> SummarizeImageAttachmentAsync(string fileName, ImageContent attachment, CancellationToken cancellationToken)
        {
            var history = new ChatHistory();
            var userPrompt = new ChatMessageContentItemCollection();

            string systemPromptPath = "SystemPrompts/ImageAttachmentSummary.txt";

            if (!File.Exists(systemPromptPath))
                throw new FileNotFoundException($"System prompt file not found: {systemPromptPath}");

            string systemPrompt = await File.ReadAllTextAsync(systemPromptPath, cancellationToken);

            history.AddSystemMessage(systemPrompt);

            userPrompt.Add(attachment);

            history.AddUserMessage(userPrompt);
            try
            {
                var response = await _chatCompletionService.GetChatMessageContentAsync(chatHistory: history, cancellationToken: cancellationToken);
                return response.ToString().Trim();

            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception)
            {
                throw new AttachmentSummaryException(fileName);
            }
        }



        // Trims csv file
        private TextContent GetCsvFirstNLinesAsync(byte[] csvBytes, int maxLines)
        {
            using var memoryStream = new MemoryStream(csvBytes);
            using var reader = new StreamReader(memoryStream, Encoding.UTF8);

            var lines = new List<string>();
            string? line;
            int count = 0;

            while (count < maxLines && (line = reader.ReadLine()) != null)
            {
                lines.Add(line);
                count++;
            }

            var contentString = string.Join(Environment.NewLine, lines);
            return new TextContent(contentString);
        }



    }
}
